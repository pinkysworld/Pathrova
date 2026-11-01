using Pathrova.Core;

namespace Pathrova.Tests;

public class CoreTests
{
    private static readonly CheckOptions O = new() { TargetPrefix = "Documents", LocalRoot = @"C:\Users\Max\OneDrive - Firma" };

    private static List<IssueKind> Kinds(string name, bool dir = false, bool root = false) => Rules.CheckName(name, dir, root, O, new CheckOptions { BlockedExtensions = "exe; .BAT" }.BlockedSet()).Select(x => x.Kind).ToList();

    [Theory]
    [InlineData("a:b.txt")] [InlineData("a*b")] [InlineData("a?b")] [InlineData("a\"b")] [InlineData("a<b>")] [InlineData("a|b")] [InlineData("a\\b")] [InlineData("a\tb")]
    public void InvalidCharacters(string name) => Assert.Contains(IssueKind.InvalidCharacter, Kinds(name));

    [Theory]
    [InlineData("CON")] [InlineData("con.txt")] [InlineData("PRN.tar.gz")] [InlineData("COM0")] [InlineData("LPT9.doc")] [InlineData("NUL")] [InlineData(".lock")]
    [InlineData("~$Bericht.docx")] [InlineData("my_vti_file.txt")]
    public void ReservedNames(string name) => Assert.Contains(IssueKind.ReservedName, Kinds(name));

    [Theory]
    [InlineData("CONSOLE.txt")] [InlineData("COM10")] [InlineData("Bericht.docx")] [InlineData("forms")] [InlineData("lock")]
    public void AllowedNames(string name) => Assert.DoesNotContain(IssueKind.ReservedName, Kinds(name));

    [Fact]
    public void FormsOnlyAtRoot()
    {
        Assert.Contains(IssueKind.ReservedName, Kinds("Forms", true, root: true));
        Assert.DoesNotContain(IssueKind.ReservedName, Kinds("Forms", true, root: false));
    }

    [Fact]
    public void SpacesDotsAndSpecialCharacters()
    {
        Assert.Contains(IssueKind.LeadingTrailingSpace, Kinds(" a.txt"));
        Assert.Contains(IssueKind.LeadingTrailingSpace, Kinds("a.txt "));
        Assert.Contains(IssueKind.TrailingDot, Kinds("Ordner.", true));
        Assert.Contains(IssueKind.RestrictedCharacter, Kinds("Rechnung #12.pdf"));
        Assert.Contains(IssueKind.RestrictedCharacter, Kinds("50%.pdf"));
        Assert.Contains(IssueKind.OfficeCharacter, Kinds("A & B.docx"));
        Assert.Empty(Kinds("Ganz normal – Ä Ö Ü ß.pdf"));
    }

    [Fact]
    public void FileTypesAndSystemFiles()
    {
        Assert.Contains(IssueKind.BlockedType, Kinds("setup.EXE"));
        Assert.Contains(IssueKind.BlockedType, Kinds("x.bat"));
        Assert.DoesNotContain(IssueKind.BlockedType, Kinds("exe", true));
        Assert.Contains(IssueKind.NotSynced, Kinds("desktop.ini"));
        Assert.Contains(IssueKind.NotSynced, Kinds("Scan.TMP"));
        Assert.Contains(IssueKind.NotSynced, Kinds(".DS_Store"));
        Assert.Contains(IssueKind.NameTooLong, Kinds(new string('a', 256)));
        Assert.DoesNotContain(IssueKind.NameTooLong, Kinds(new string('a', 255)));
    }

    [Fact]
    public void EncodedLengthCountsLikeUrls()
    {
        Assert.Equal(5, Rules.CloudLength("a b/c", false));
        Assert.Equal(7, Rules.CloudLength("a b/c", true)); // Leerzeichen = %20
        Assert.Equal(6, Rules.CloudLength("ä", true));     // UTF-8 zwei Bytes = %C3%A4
        Assert.Equal(12, Rules.CloudLength("😀", true));    // vier Bytes
        Assert.Equal("Documents/x/y.txt", Rules.CloudPath(O, "x/y.txt"));
        Assert.Equal(@"C:\Users\Max\OneDrive - Firma\x\y.txt", Rules.LocalPath(O, "x/y.txt"));
    }

    [Theory]
    [InlineData("a:b?.txt", "a_b_.txt")]
    [InlineData("  Angebot .docx ", "Angebot .docx")]
    [InlineData("Ordner...", "Ordner")]
    [InlineData("CON.txt", "CON_.txt")]
    [InlineData("_vti_cnf", "_vti-cnf")]
    [InlineData(".lock", "_lock")]
    public void Suggestions(string name, string expected) => Assert.Equal(expected, Rules.Suggest(name, name == "_vti_cnf", false, O));

    [Fact]
    public void SuggestionsLeaveSystemFilesAndShortenWithExtension()
    {
        Assert.Null(Rules.Suggest("desktop.ini", false, false, O));
        Assert.Null(Rules.Suggest("~$Bericht.docx", false, false, O));
        Assert.Null(Rules.Suggest("ok.txt", false, false, O));
        Assert.Equal("forms_", Rules.Suggest("forms", true, true, O));
        var s = Rules.Suggest(new string('x', 300) + ".pdf", false, false, O)!;
        Assert.Equal(255, s.Length); Assert.EndsWith(".pdf", s);
        Assert.Equal("abcdefghij.pdf", Rules.Suggest("abcdefghijklmnop.pdf", false, false, O, 14));
        Assert.Null(Rules.Suggest("abcdefghijklmnop.pdf", false, false, O, 6)); // weniger als 8 Zeichen Stamm: nichts Automatisches
    }

    [Fact]
    public void ScannerFindsAllIssuesInOnePassAndGroupsLongPaths()
    {
        using var d = new TempDirs();
        var top = DemoData.Create(d.Root, false);
        var r = Scanner.Scan(top, new CheckOptions { TargetPrefix = "Freigegebene Dokumente", LocalRoot = @"C:\Users\Max\OneDrive - Firma", BlockedExtensions = "exe" });
        var kinds = r.Issues.Select(i => i.Kind).ToHashSet();
        foreach (var k in new[] { IssueKind.PathTooLong, IssueKind.OfficePathLong, IssueKind.InvalidCharacter, IssueKind.ReservedName, IssueKind.LeadingTrailingSpace,
                     IssueKind.TrailingDot, IssueKind.RestrictedCharacter, IssueKind.OfficeCharacter, IssueKind.BlockedType, IssueKind.NotSynced })
            Assert.True(kinds.Contains(k), k.ToString());
        // Unter einem zu langen Ordner wird nicht jedes Kind einzeln gelistet.
        var longOnes = r.Issues.Where(i => i.Kind == IssueKind.PathTooLong).ToList();
        Assert.Equal(2, longOnes.Count);
        Assert.All(longOnes, i => Assert.False(i.IsDirectory));
        // Office-Grenze (256 lokal): schon der Ordner ist zu lang, die zwei Dateien darin zählen nur mit.
        var office = r.Issues.Where(i => i.Kind == IssueKind.OfficePathLong).ToList();
        Assert.Contains(office, i => i.IsDirectory && i.Inside >= 2);
        Assert.DoesNotContain(office, i => !i.IsDirectory && i.Name.StartsWith("Grundriss"));
        Assert.Equal(office.Count + office.Sum(i => i.Inside), r.Affected(IssueKind.OfficePathLong));
        Assert.Contains(r.Issues, i => i.Kind == IssueKind.ReservedName && i.Name == "forms");
        Assert.Equal(41, r.Files);
        Assert.False(r.Cancelled);
    }

    [Fact]
    public void ScannerCanBeCancelled()
    {
        using var d = new TempDirs();
        DemoData.Create(d.Root, true);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var r = Scanner.Scan(d.Root, O, ct: cts.Token);
        Assert.True(r.Cancelled);
    }

    [Fact]
    public void ScannerHandlesManyFilesQuickly()
    {
        using var d = new TempDirs();
        for (int a = 0; a < 100; a++)
        {
            var dir = Directory.CreateDirectory(Path.Combine(d.Root, $"Ordner {a:000}")).FullName;
            for (int b = 0; b < 200; b++) File.WriteAllBytes(Path.Combine(dir, $"Datei {b:000}.txt{(b == 7 ? " " : "")}"), []);
        }
        var reports = 0;
        var r = Scanner.Scan(d.Root, O, new SyncProgress<ScanProgress>(_ => reports++));
        Assert.Equal(20_000, r.Files);
        Assert.Equal(100, r.Issues.Count(i => i.Kind == IssueKind.LeadingTrailingSpace));
        Assert.True(reports >= 20);
        Assert.True(r.Duration < TimeSpan.FromSeconds(20), r.Duration.ToString());
    }

    [Fact]
    public void SymlinksAreReportedNotFollowed()
    {
        using var d = new TempDirs();
        var target = Directory.CreateDirectory(Path.Combine(d.Root, "real")).FullName;
        File.WriteAllText(Path.Combine(target, "a:b.txt"), "x");
        var scan = Directory.CreateDirectory(Path.Combine(d.Root, "scan")).FullName;
        Directory.CreateSymbolicLink(Path.Combine(scan, "link"), target);
        var r = Scanner.Scan(scan, O);
        Assert.Contains(r.Issues, i => i.Kind == IssueKind.LinkNotSupported);
        Assert.DoesNotContain(r.Issues, i => i.Kind == IssueKind.InvalidCharacter);
    }

    [Fact]
    public void RenamePlanRunsAndUndoes()
    {
        using var d = new TempDirs();
        var top = DemoData.Create(d.Root, false);
        File.WriteAllText(Path.Combine(top, "Personal", "Urlaubsplan _Entwurf_.xlsx"), "existiert schon");
        var o = new CheckOptions { TargetPrefix = "Freigegebene Dokumente", LocalRoot = @"C:\Users\Max\OneDrive - Firma" };
        var r = Scanner.Scan(top, o);
        var plan = Renamer.Plan(r);
        Assert.Contains(plan, p => p.OldName == "Angebot: final?.docx" && p.NewName == "Angebot_ final_.docx");
        Assert.Contains(plan, p => p.OldName == " Steuerberater" && p.NewName == "Steuerberater" && p.IsDirectory);
        Assert.Contains(plan, p => p.OldName == "Urlaubsplan <Entwurf>.xlsx" && p.NewName == "Urlaubsplan _Entwurf_ (2).xlsx"); // Konflikt aufgelöst
        Assert.DoesNotContain(plan, p => p.OldName == "desktop.ini" || p.OldName.StartsWith("~$"));
        // Datei im zu langen Pfad wird so gekürzt, dass sie passt (sofern die Ordner allein nicht schon zu lang sind) – hier sind sie es: keine Ordnerkürzung.
        Assert.DoesNotContain(plan, p => p.IsDirectory && p.RelativePath.StartsWith("Projekte/2024/Kunde"));
        Assert.True(plan.Zip(plan.Skip(1)).All(x => x.First.Depth >= x.Second.Depth));

        plan.First(p => p.OldName == "CON.txt").Selected = false;
        var outcome = Renamer.Run(top, plan);
        Assert.Empty(outcome.Failures);
        Assert.True(File.Exists(Path.Combine(top, "Buchhaltung", "Steuerberater", "Jahresabschluss 2023.xlsx")));
        Assert.True(File.Exists(Path.Combine(top, "Personal", "CON.txt")));
        var after = Scanner.Scan(top, o);
        Assert.DoesNotContain(after.Issues, i => i.Kind is IssueKind.InvalidCharacter or IssueKind.LeadingTrailingSpace or IssueKind.TrailingDot);

        var logFile = Renamer.SaveLog(outcome.Log, Path.Combine(d.Root, "undo"));
        Assert.Equal(logFile, Renamer.LatestLog(Path.Combine(d.Root, "undo")));
        var undo = Renamer.Undo(Renamer.LoadLog(logFile)!);
        Assert.Empty(undo.Failures);
        Assert.True(File.Exists(Path.Combine(top, "Buchhaltung", " Steuerberater", "Jahresabschluss 2023.xlsx")));
        Assert.True(File.Exists(Path.Combine(top, "Projekte", "2024", "Kunde Müller & Söhne – Sanierung Bürogebäude Hauptstraße", "Planungsunterlagen und Genehmigungen vom Bauamt", "Angebot: final?.docx")));
        Assert.Equal("existiert schon", File.ReadAllText(Path.Combine(top, "Personal", "Urlaubsplan _Entwurf_.xlsx")));
    }

    [Fact]
    public void LongFileNameIsShortenedToFit()
    {
        using var d = new TempDirs();
        var dir = Directory.CreateDirectory(Path.Combine(d.Root, "Ordner")).FullName;
        var name = new string('b', 120) + ".docx";
        File.WriteAllText(Path.Combine(dir, name), "x");
        var o = new CheckOptions { TargetPrefix = "Documents", CloudLimit = 100, LocalRoot = "C:", OfficeLimit = 1000 };
        var r = Scanner.Scan(d.Root, o);
        var op = Assert.Single(Renamer.Plan(r));
        Assert.EndsWith(".docx", op.NewName);
        Assert.True(Rules.CloudLength(Rules.CloudPath(o, "Ordner/" + op.NewName), true) <= 100);
    }

    [Fact]
    public void CsvAndPdfReports()
    {
        using var d = new TempDirs();
        var top = DemoData.Create(d.Root, true);
        var r = Scanner.Scan(top, O);
        var csv = Reports.Csv(r, r.Issues, true, Renamer.Plan(r).ToDictionary(p => p.OldPath, p => p.NewName));
        Assert.StartsWith("Level,Issue,Path,Length,Details,Suggestion", csv);
        Assert.Contains("Quote_ final_.docx", csv);
        Assert.Contains("Invalid characters", csv);
        var de = Reports.Csv(r, r.Issues, false);
        Assert.StartsWith("Stufe;Problem;Pfad", de);
        var pdf = Path.Combine(d.Root, "r.pdf");
        Reports.WritePdf(pdf, r, r.Issues, false);
        var bytes = File.ReadAllBytes(pdf);
        Assert.True(bytes.Length > 2000);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public void CsvBlocksFormulas()
    {
        var r = new ScanResult { Root = "/x", Options = O };
        r.Issues.Add(new Issue("=cmd|x", "=cmd|x", false, IssueKind.InvalidCharacter, 0, "|"));
        Assert.Contains("'=cmd|x", Reports.Csv(r, r.Issues, true));
    }

    private sealed class SyncProgress<T>(Action<T> a) : IProgress<T> { public void Report(T v) => a(v); }
}

public class ShortenTests
{
    [Fact]
    public void KeepsVersionNumbersAndDropsDashesFirst()
    {
        Assert.Equal("Schnitte A-A und B-B-Revision 4", Rules.Shorten("Schnitte A-A und B-B – Revision 4", n => n.Length <= 32));
        Assert.Equal("Grundriss Erdgeschoss-Revision 7", Rules.Shorten("Grundriss Erdgeschoss – Revision 7 – freigegeben von Bauherr", n => n.Length <= 34));
        Assert.Equal("Bericht Quartal 2024", Rules.Shorten("Bericht Quartal sehr lang 2024", n => n.Length <= 20));
        Assert.Equal("", Rules.Shorten("abc defgh", n => n.Length <= 3));
        Assert.Equal("Ground floor plan approved by client", Rules.Shorten("Ground floor plan approved by client and authority", n => n.Length <= 40));
    }
}

public class DiffTests
{
    [Fact]
    public void MarksOnlyChangedCharacters()
    {
        var old = Pathrova.App.Views.RenameWindow.Segments("Grundriss – Revision 7 – frei.pdf", "Grundriss-Revision 7-frei.pdf");
        Assert.Equal("Grundriss – Revision 7 – frei.pdf", string.Concat(old.Select(s => s.Text)));
        Assert.Equal(" – ", string.Concat(old.Where(s => s.Changed).Select(s => s.Text))[..3]);
        Assert.Equal(6, old.Where(s => s.Changed).Sum(s => s.Text.Length));
        var neu = Pathrova.App.Views.RenameWindow.Segments("Grundriss-Revision 7-frei.pdf", "Grundriss – Revision 7 – frei.pdf");
        Assert.Equal("--", string.Concat(neu.Where(s => s.Changed).Select(s => s.Text)));
    }
}
