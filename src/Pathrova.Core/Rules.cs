using System.Text;

namespace Pathrova.Core;

public enum Severity { Error, Warning, Info }

/// <summary>Problemarten. Die Reihenfolge bestimmt die Gruppierung in Liste und Bericht.</summary>
public enum IssueKind
{
    PathTooLong, LocalPathTooLong, NameTooLong, InvalidCharacter, ReservedName, LeadingTrailingSpace, BlockedType, FileTooLarge,
    OfficePathLong, TrailingDot, RestrictedCharacter, OfficeCharacter, LinkNotSupported, NotSynced, AccessDenied, TooManyItems,
}

/// <summary>Prüfeinstellungen. Grenzen nach den Microsoft-Angaben vom 01.11.2026; Mandanten können abweichen.</summary>
public sealed class CheckOptions
{
    /// <summary>Pfad in der Zielbibliothek vor dem gescannten Ordner, z. B. „Freigegebene Dokumente/Projekte“.</summary>
    public string TargetPrefix { get; set; } = "Documents";
    /// <summary>Lokaler Sync-Ordner auf dem Ziel-PC, z. B. „C:\Users\Max\OneDrive - Firma“.</summary>
    public string LocalRoot { get; set; } = @"C:\Users\" + SafeUser() + @"\OneDrive - Company";
    /// <summary>Streng: Zeichen werden URL-kodiert gezählt (Leerzeichen = %20).</summary>
    public bool CountEncoded { get; set; } = true;
    public int CloudLimit { get; set; } = 400;
    public int LocalLimit { get; set; } = 520;
    public int OfficeLimit { get; set; } = 256;
    public bool WarnHashPercent { get; set; } = true;
    public bool WarnOfficeCharacters { get; set; } = true;
    public string BlockedExtensions { get; set; } = "";
    public long MaxFileSize { get; set; } = 250L * 1024 * 1024 * 1024;

    private static string SafeUser() { try { return Environment.UserName; } catch (Exception) { return "User"; } }

    public HashSet<string> BlockedSet() => BlockedExtensions.Split([';', ',', ' '], StringSplitOptions.RemoveEmptyEntries)
        .Select(e => "." + e.Trim().TrimStart('*').TrimStart('.').ToLowerInvariant()).Where(e => e.Length > 1).ToHashSet();
}

public sealed class Issue(string fullPath, string relativePath, bool isDirectory, IssueKind kind, int length = 0, string detail = "")
{
    public string FullPath { get; } = fullPath;
    public string RelativePath { get; } = relativePath;
    public bool IsDirectory { get; } = isDirectory;
    public IssueKind Kind { get; } = kind;
    public int Length { get; } = length;
    public string Detail { get; } = detail;
    public Severity Severity => Rules.SeverityOf(Kind);
    /// <summary>Bei zu langen Pfaden: so viele Elemente darunter sind ebenfalls betroffen (nicht einzeln gelistet).</summary>
    public int Inside { get; set; }
    public string Name => Path.GetFileName(FullPath.TrimEnd('\\', '/'));
}

public static class Rules
{
    public static readonly char[] Invalid = ['"', '*', ':', '<', '>', '?', '/', '\\', '|'];
    private static readonly HashSet<string> Devices = new(StringComparer.OrdinalIgnoreCase)
        { "CON", "PRN", "AUX", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
    private static readonly HashSet<string> NotSyncedNames = new(StringComparer.OrdinalIgnoreCase) { "desktop.ini", ".ds_store", "thumbs.db" };

    public static Severity SeverityOf(IssueKind k) => k switch
    {
        IssueKind.OfficePathLong or IssueKind.TrailingDot or IssueKind.RestrictedCharacter or IssueKind.OfficeCharacter or IssueKind.LinkNotSupported or IssueKind.AccessDenied => Severity.Warning,
        IssueKind.NotSynced or IssueKind.TooManyItems => Severity.Info,
        _ => Severity.Error,
    };

    /// <summary>Länge so, wie SharePoint sie zählt: streng URL-kodiert (RFC 3986, Schrägstrich bleibt) oder als Zeichen.</summary>
    public static int CloudLength(string cloudPath, bool encoded)
    {
        if (!encoded) return cloudPath.Length;
        int n = 0;
        foreach (var rune in cloudPath.EnumerateRunes())
        {
            int c = rune.Value;
            bool plain = c < 128 && (char.IsAsciiLetterOrDigit((char)c) || "-._~/".Contains((char)c));
            n += plain ? 1 : 3 * rune.Utf8SequenceLength;
        }
        return n;
    }

    public static string CloudPath(CheckOptions o, string relative)
    {
        var rel = relative.Replace('\\', '/').Trim('/');
        var prefix = o.TargetPrefix.Replace('\\', '/').Trim('/');
        return prefix.Length == 0 ? rel : rel.Length == 0 ? prefix : prefix + "/" + rel;
    }

    public static string LocalPath(CheckOptions o, string relative) => o.LocalRoot.TrimEnd('\\', '/') + "\\" + relative.Replace('/', '\\').Trim('\\');

    /// <summary>Namensregeln für ein Element (ohne Pfadlänge).</summary>
    public static IEnumerable<(IssueKind Kind, string Detail)> CheckName(string name, bool isDirectory, bool atRoot, CheckOptions o, HashSet<string>? blocked = null)
    {
        var bad = name.Where(c => Invalid.Contains(c) || c < 32).Distinct().ToArray();
        if (bad.Length > 0) yield return (IssueKind.InvalidCharacter, string.Join(" ", bad.Select(Show)));
        if (name.Length > 0 && (name[0] == ' ' || name[^1] == ' ')) yield return (IssueKind.LeadingTrailingSpace, "");
        else if (name.EndsWith('.')) yield return (IssueKind.TrailingDot, "");
        if (ReservedReason(name, isDirectory, atRoot) is { } why) yield return (IssueKind.ReservedName, why);
        if (name.Length > 255) yield return (IssueKind.NameTooLong, name.Length.ToString());
        if (o.WarnHashPercent && name.IndexOfAny(['#', '%']) >= 0) yield return (IssueKind.RestrictedCharacter, string.Join(" ", name.Where(c => c is '#' or '%').Distinct()));
        if (o.WarnOfficeCharacters && name.IndexOfAny([';', '&']) >= 0) yield return (IssueKind.OfficeCharacter, string.Join(" ", name.Where(c => c is ';' or '&').Distinct()));
        if (!isDirectory)
        {
            var ext = Path.GetExtension(name).ToLowerInvariant();
            if (NotSyncedNames.Contains(name) || ext == ".tmp") yield return (IssueKind.NotSynced, name);
            if (blocked is { Count: > 0 } && ext.Length > 0 && blocked.Contains(ext)) yield return (IssueKind.BlockedType, ext);
        }
    }

    private static string Show(char c) => c < 32 ? $"U+{(int)c:X4}" : c.ToString();

    public static string? ReservedReason(string name, bool isDirectory, bool atRoot)
    {
        var stem = name.Split('.')[0].TrimEnd(' ');
        if (Devices.Contains(stem)) return stem.ToUpperInvariant();
        if (name.Equals(".lock", StringComparison.OrdinalIgnoreCase)) return ".lock";
        if (name.StartsWith("~$", StringComparison.Ordinal)) return "~$";
        if (name.Contains("_vti_", StringComparison.OrdinalIgnoreCase)) return "_vti_";
        if (atRoot && name.Equals("forms", StringComparison.OrdinalIgnoreCase)) return "forms";
        if (isDirectory && (name.StartsWith('゛') || name.StartsWith('ဧ'))) return name[..1];
        return null;
    }

    /// <summary>Sicherer Ersatzname für die gefundenen Namensprobleme, oder null, wenn nichts zu ändern ist bzw. nichts Sicheres möglich ist.</summary>
    /// <param name="maxLength">Höchste Länge des neuen Namens, null = nur Regeln.</param>
    public static string? Suggest(string name, bool isDirectory, bool atRoot, CheckOptions o, int? maxLength = null) =>
        Suggest(name, isDirectory, atRoot, o, maxLength is { } m ? n => n.Length <= m : null);

    /// <param name="fits">Prüft, ob ein Kandidat kurz genug ist (z. B. Pfadlänge in der Cloud), null = nur Regeln.</param>
    public static string? Suggest(string name, bool isDirectory, bool atRoot, CheckOptions o, Func<string, bool>? fits)
    {
        // Systemdateien und temporäre Office-Dateien nicht umbenennen.
        if (NotSyncedNames.Contains(name) || name.StartsWith("~$", StringComparison.Ordinal)) return null;
        var sb = new StringBuilder(name.Length);
        foreach (var c in name) sb.Append(Invalid.Contains(c) || c < 32 ? '_' : c);
        var s = sb.ToString().Trim(' ');
        while (s.EndsWith('.') || s.EndsWith(' ')) s = s[..^1];
        s = s.Replace("_vti_", "_vti-", StringComparison.OrdinalIgnoreCase);
        if (s.Equals(".lock", StringComparison.OrdinalIgnoreCase)) s = "_lock";
        if (isDirectory && s.Length > 0 && (s[0] == '゛' || s[0] == 'ဧ')) s = "_" + s;
        var (stem, ext) = Split(s, isDirectory);
        if (Devices.Contains(stem.TrimEnd(' ')) || (atRoot && s.Equals("forms", StringComparison.OrdinalIgnoreCase))) stem += "_";
        bool Ok(string st) => st.Length + ext.Length <= 255 && (fits is null || fits(st + ext));
        if (!Ok(stem))
        {
            stem = Shorten(stem, Ok);
            if (stem.Length == 0) return null;
        }
        s = stem + ext;
        if (s.Length == 0) s = "_";
        return s == name ? null : s;
    }

    /// <summary>Kürzt behutsam: erst Gedankenstriche mit Leerzeichen zu „-“ (spart kodiert viel), dann Wörter aus der Mitte entfernen;
    /// das erste Wort und ein letztes Wort mit Ziffern (Version, Datum) bleiben. Leer = nicht sicher möglich.</summary>
    public static string Shorten(string stem, Func<string, bool> ok)
    {
        var s = System.Text.RegularExpressions.Regex.Replace(stem, @"\s+[–—-]\s+", "-");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"\s{2,}", " ");
        if (ok(s)) return s;
        // Wörter samt Trennzeichen davor (Leerzeichen oder „-“), damit „7-freigegeben“ zwei Wörter sind.
        var words = System.Text.RegularExpressions.Regex.Matches(s, @"[ -]?[^ -]+").Select(m => m.Value).ToList();
        string? keepTail = words.Count > 1 && words[^1].Any(char.IsDigit) ? words[^1] : null;
        if (keepTail is not null) words.RemoveAt(words.Count - 1);
        while (words.Count > 1)
        {
            words.RemoveAt(words.Count - 1);
            var cand = string.Concat(words).TrimEnd('-', '_', '.', ',') + (keepTail is null ? "" : (keepTail[0] is ' ' or '-' ? keepTail : " " + keepTail));
            if (cand.Length >= 8 && !EndsWithFiller(words) && ok(cand)) return cand;
        }
        // Ein einziges langes Wort: Zeichen von hinten abschneiden.
        var one = words.Count == 1 ? words[0].TrimStart(' ', '-') : s;
        for (int n = one.Length - 1; n >= 8; n--)
        {
            var cand = one[..n];
            if (char.IsHighSurrogate(cand[^1])) continue;
            cand += keepTail ?? "";
            if (ok(cand)) return cand;
        }
        return "";
    }

    private static readonly HashSet<string> Fillers = new(StringComparer.OrdinalIgnoreCase)
        { "and", "or", "by", "of", "the", "a", "to", "for", "with", "from", "und", "oder", "von", "vom", "der", "die", "das", "den", "mit", "für", "zu", "zum", "zur", "im", "in", "an", "am", "auf", "et", "de", "du", "le", "la" };

    /// <summary>Kürzungen sollen nicht auf „und“, „by“ usw. enden.</summary>
    private static bool EndsWithFiller(List<string> words) => words.Count > 1 && Fillers.Contains(words[^1].TrimStart(' ', '-'));

    public static (string Stem, string Ext) Split(string name, bool isDirectory)
    {
        if (isDirectory) return (name, "");
        var ext = Path.GetExtension(name);
        return ext.Length > 0 && ext.Length <= 16 && ext.Length < name.Length ? (name[..^ext.Length], ext) : (name, "");
    }

    public static bool CanFix(IssueKind k) => k is IssueKind.InvalidCharacter or IssueKind.LeadingTrailingSpace or IssueKind.TrailingDot or IssueKind.ReservedName
        or IssueKind.NameTooLong or IssueKind.PathTooLong or IssueKind.LocalPathTooLong or IssueKind.OfficePathLong;
}
