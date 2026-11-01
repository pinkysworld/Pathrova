using System.Text;
using SkiaSharp;

namespace Pathrova.Core;

public static class Reports
{
    public const int PdfMaxRows = 5000;

    public static string Csv(ScanResult r, IEnumerable<Issue> issues, bool english, IReadOnlyDictionary<string, string>? proposals = null)
    {
        var t = Texts.For(english);
        char sep = english ? ',' : ';';
        var sb = new StringBuilder();
        void Row(params string[] cells) => sb.AppendLine(string.Join(sep, cells.Select(c => Quote(c, sep))));
        Row(t.SeverityCol, t.KindCol, t.PathCol, t.LengthCol, t.DetailCol, t.ProposalCol);
        foreach (var i in issues)
            Row(t.Severity(i.Severity), t.Kind(i.Kind), i.FullPath, i.Length > 0 ? i.Length.ToString() : "",
                i.Inside > 0 ? (i.Detail + " " + t.AndInside(i.Inside)).Trim() : i.Detail,
                proposals is not null && proposals.TryGetValue(i.FullPath, out var p) ? p : "");
        return sb.ToString();
    }

    public static void WriteCsv(string path, ScanResult r, IEnumerable<Issue> issues, bool english, IReadOnlyDictionary<string, string>? proposals = null) =>
        File.WriteAllText(path, Csv(r, issues, english, proposals), new UTF8Encoding(true));

    private static string Quote(string s, char sep)
    {
        // Formel-Injektion in Tabellenprogrammen verhindern.
        if (s.Length > 0 && "=+-@\t\r".Contains(s[0])) s = "'" + s;
        return s.IndexOfAny([sep, '"', '\n', '\r']) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
    }

    private abstract record Line(float Height);
    private sealed record TextLine(string Text, float Size, SKColor Color, bool Bold, float Indent = 0, float Height = 0) : Line(Height);
    private sealed record Gap(float H) : Line(H);

    /// <summary>A4-Bericht: Kopf, Zusammenfassung je Problemart, dann die Einträge gruppiert (höchstens PdfMaxRows).</summary>
    public static void WritePdf(string path, ScanResult r, IEnumerable<Issue> issues, bool english)
    {
        var t = Texts.For(english);
        const float W = 595, H = 842, M = 48, Bottom = H - 50;
        using var regular = SKTypeface.FromFamilyName("Segoe UI") ?? SKTypeface.Default;
        using var bold = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold) ?? SKTypeface.Default;
        var ink = new SKColor(0x1E, 0x22, 0x30); var muted = new SKColor(0x5E, 0x63, 0x72);
        var red = new SKColor(0xB3, 0x26, 0x1E); var amber = new SKColor(0x9A, 0x5B, 0x00); var blue = new SKColor(0x2B, 0x5C, 0xB8);
        SKColor Col(Severity s) => s switch { Severity.Error => red, Severity.Warning => amber, _ => blue };

        var lines = new List<Line>();
        void Add(string text, float size, SKColor c, bool b = false, float indent = 0)
        {
            using var font = new SKFont(b ? bold : regular, size);
            foreach (var part in Wrap(text, font, W - 2 * M - indent)) lines.Add(new TextLine(part, size, c, b, indent, size * 1.45f));
        }
        Add(t.ReportTitle, 20, ink, true);
        Add($"{t.Folder}: {r.Root}", 10, muted);
        Add($"{t.Target}: {Rules.CloudPath(r.Options, "")} · {t.LocalRoot}: {r.Options.LocalRoot} · {t.Counting(r.Options.CountEncoded)}", 10, muted);
        Add($"{t.Created}: {r.CreatedAt:yyyy-MM-dd HH:mm} · {t.Done(r)}", 10, muted);
        lines.Add(new Gap(10));
        Add(t.Summary, 14, ink, true);
        var all = issues.ToList();
        if (all.Count == 0) Add(t.NoIssues, 11, ink);
        foreach (var g in all.GroupBy(i => i.Kind).OrderBy(g => g.Key))
            Add($"{t.Severity(Rules.SeverityOf(g.Key))} · {t.Kind(g.Key)}: {Format.Number(g.Sum(i => 1L + i.Inside), english)}", 11, Col(Rules.SeverityOf(g.Key)), false, 8);
        lines.Add(new Gap(12));
        int shown = 0;
        foreach (var g in all.GroupBy(i => i.Kind).OrderBy(g => g.Key))
        {
            if (shown >= PdfMaxRows) break;
            lines.Add(new Gap(6));
            Add(t.Kind(g.Key), 13, Col(Rules.SeverityOf(g.Key)), true);
            Add(t.Explain(g.Key), 9.5f, muted);
            foreach (var i in g)
            {
                if (shown++ >= PdfMaxRows) break;
                var extra = string.Join(" · ", new[] { i.Detail, i.Inside > 0 ? t.AndInside(i.Inside) : "" }.Where(s => s.Length > 0));
                Add(i.FullPath + (extra.Length > 0 ? "   [" + extra + "]" : ""), 9, ink, false, 8);
            }
        }
        if (all.Count > PdfMaxRows) { lines.Add(new Gap(8)); Add(t.Truncated(PdfMaxRows, all.Count), 10, muted); }
        lines.Add(new Gap(14));
        Add(t.Disclaimer, 9, muted);

        // Seiten aufteilen, dann zeichnen (für „Seite x von y“).
        var pages = new List<List<Line>> { new() };
        float y = M;
        foreach (var l in lines)
        {
            if (y + l.Height > Bottom && pages[^1].Count > 0) { pages.Add([]); y = M; if (l is Gap) continue; }
            pages[^1].Add(l); y += l.Height;
        }
        using var stream = File.Create(path);
        using var doc = SKDocument.CreatePdf(stream, new SKDocumentPdfMetadata { Title = t.ReportTitle, Creator = "Pathrova", Producer = "Pathrova" });
        for (int p = 0; p < pages.Count; p++)
        {
            var canvas = doc.BeginPage(W, H);
            y = M;
            foreach (var l in pages[p])
            {
                if (l is TextLine tl)
                {
                    using var font = new SKFont(tl.Bold ? bold : regular, tl.Size);
                    using var paint = new SKPaint { Color = tl.Color, IsAntialias = true };
                    canvas.DrawText(tl.Text, M + tl.Indent, y + tl.Size, font, paint);
                }
                y += l.Height;
            }
            using (var f = new SKFont(regular, 8.5f)) using (var pp = new SKPaint { Color = muted, IsAntialias = true })
            {
                canvas.DrawText("Pathrova", M, H - 28, f, pp);
                var label = t.Page(p + 1, pages.Count);
                canvas.DrawText(label, W - M - f.MeasureText(label), H - 28, f, pp);
            }
            doc.EndPage();
        }
        doc.Close();
    }

    /// <summary>Bricht an Leerzeichen oder Pfadtrennern um, notfalls mitten im Wort.</summary>
    private static IEnumerable<string> Wrap(string text, SKFont font, float width)
    {
        var rest = text;
        while (rest.Length > 0)
        {
            if (font.MeasureText(rest) <= width) { yield return rest; yield break; }
            int lo = 1, hi = rest.Length;
            while (lo < hi) { int mid = (lo + hi + 1) / 2; if (font.MeasureText(rest[..mid]) <= width) lo = mid; else hi = mid - 1; }
            int cut = lo;
            int brk = rest.LastIndexOfAny([' ', '\\', '/'], Math.Max(0, lo - 1));
            if (brk > lo / 2) cut = brk + 1;
            yield return rest[..cut];
            rest = rest[cut..];
        }
    }
}
