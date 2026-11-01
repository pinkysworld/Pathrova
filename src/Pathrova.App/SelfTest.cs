using Pathrova.Core;

namespace Pathrova.App;

/// <summary>Funktionstest ohne Fenster für die Windows-Prüfung: Scan mit echten Windows-Pfaden über 260 Zeichen, Umbenennen, Rückgängig, Berichte.</summary>
public static class SelfTest
{
    public static int Run(string[] args)
    {
        var root = Path.Combine(Path.GetTempPath(), "pathrova-selftest-" + Guid.NewGuid().ToString("N")[..8]);
        var log = args.Length > 0 ? args[0] : Path.Combine(root, "selftest.log");
        try
        {
            Directory.CreateDirectory(root);
            var deep = root;
            for (int i = 0; i < 8; i++) deep = Directory.CreateDirectory(Path.Combine(deep, new string((char)('a' + i), 50))).FullName;
            File.WriteAllText(Path.Combine(deep, "tief.txt"), "x");
            File.WriteAllText(Path.Combine(root, "Angebot .docx"), "x");
            File.WriteAllText(Path.Combine(root, "CON.txt.bak"), "x");
            File.WriteAllText(Path.Combine(root, "Notiz#1.txt"), "x");
            var o = new CheckOptions { TargetPrefix = "Documents", LocalRoot = @"C:\Users\Test\OneDrive - Firma" };
            var r = Scanner.Scan(root, o);
            bool found = r.Issues.Any(i => i.Kind == IssueKind.PathTooLong) && r.Issues.Any(i => i.Kind == IssueKind.LeadingTrailingSpace)
                         && r.Issues.Any(i => i.Kind == IssueKind.RestrictedCharacter);
            var plan = Renamer.Plan(r);
            var done = Renamer.Run(root, plan);
            var after = Scanner.Scan(root, o);
            bool fixedSpace = !after.Issues.Any(i => i.Kind == IssueKind.LeadingTrailingSpace) && File.Exists(Path.Combine(root, "Angebot.docx"));
            var undo = Renamer.Undo(done.Log);
            bool restored = File.Exists(Path.Combine(root, "Angebot .docx")) && undo.Failures.Count == 0;
            Reports.WritePdf(Path.Combine(root, "r.pdf"), r, r.Issues, false);
            Reports.WriteCsv(Path.Combine(root, "r.csv"), r, r.Issues, true);
            bool ok = found && done.Failures.Count == 0 && fixedSpace && restored && new FileInfo(Path.Combine(root, "r.pdf")).Length > 500;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(log))!);
            File.WriteAllText(log, $"{(ok ? "PASS" : "FAIL")} issues={r.Issues.Count} plan={plan.Count} renamed={done.Log.Done.Count} failures={string.Join(" | ", done.Failures.Select(f => f.Message))} found={found} fixed={fixedSpace} restored={restored}");
            try { Directory.Delete(root, true); } catch (Exception) { }
            return ok ? 0 : 1;
        }
        catch (Exception ex)
        {
            try { File.WriteAllText(log, "FAIL " + ex); } catch (Exception) { }
            return 2;
        }
    }
}
