using System.Text.Json;

namespace Pathrova.Core;

public sealed class RenameOp
{
    public required string Directory { get; init; }
    public required string OldName { get; init; }
    public required string NewName { get; set; }
    public required bool IsDirectory { get; init; }
    public required string RelativePath { get; init; }
    public bool Selected { get; set; } = true;
    public string OldPath => Path.Combine(Directory, OldName);
    public string NewPath => Path.Combine(Directory, NewName);
    public int Depth => RelativePath.Count(c => c == '/');
}

public sealed record RenameEntry(string From, string To, bool IsDirectory);
public sealed record RenameLog(DateTime At, string Root, List<RenameEntry> Done);
public sealed record RenameFailure(string Path, string Message);
public sealed record RenameOutcome(RenameLog Log, List<RenameFailure> Failures);

/// <summary>Erstellt sichere Umbenennungsvorschläge und führt sie erst nach Bestätigung aus; jede Änderung landet im Rückgängig-Protokoll.</summary>
public static class Renamer
{
    public static List<RenameOp> Plan(ScanResult r)
    {
        var byPath = new Dictionary<string, RenameOp>(StringComparer.Ordinal);
        foreach (var i in r.Issues)
        {
            if (!Rules.CanFix(i.Kind) || i.RelativePath.Length == 0 || byPath.ContainsKey(i.FullPath)) continue;
            int? max = null;
            if (i.Kind is IssueKind.PathTooLong or IssueKind.LocalPathTooLong or IssueKind.OfficePathLong)
            {
                // Pfadlänge: nur Dateien kürzen (Ordnernamen tragen oft Bedeutung), um so viel wie nötig.
                if (i.IsDirectory) continue;
                int limit = i.Kind switch { IssueKind.PathTooLong => r.Options.CloudLimit, IssueKind.LocalPathTooLong => r.Options.LocalLimit, _ => r.Options.OfficeLimit };
                int over = i.Length - limit;
                // Bei kodierter Zählung kann ein Zeichen bis zu 9 Stellen kosten – grob über den Namen umrechnen.
                var name = i.Name;
                double perChar = i.Kind == IssueKind.PathTooLong ? Math.Max(1.0, (double)Rules.CloudLength(name, r.Options.CountEncoded) / Math.Max(1, name.Length)) : 1.0;
                max = name.Length - (int)Math.Ceiling(over / perChar);
            }
            var newName = Rules.Suggest(i.Name, i.IsDirectory, !i.RelativePath.Contains('/'), r.Options, max);
            if (newName is null) continue;
            byPath[i.FullPath] = new RenameOp { Directory = Path.GetDirectoryName(i.FullPath)!, OldName = i.Name, NewName = newName, IsDirectory = i.IsDirectory, RelativePath = i.RelativePath };
        }
        var ops = byPath.Values.OrderByDescending(o => o.Depth).ThenBy(o => o.RelativePath, StringComparer.Ordinal).ToList();
        ResolveConflicts(ops);
        return ops;
    }

    /// <summary>Hängt „ (2)“ usw. an, wenn das Ziel schon existiert oder zwei Vorschläge im selben Ordner kollidieren.</summary>
    public static void ResolveConflicts(List<RenameOp> ops)
    {
        foreach (var dir in ops.GroupBy(o => o.Directory))
        {
            var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var renamed = dir.Select(o => o.OldName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var e in new DirectoryInfo(dir.Key).EnumerateFileSystemInfos())
                    if (!renamed.Contains(e.Name)) taken.Add(e.Name);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            foreach (var op in dir.OrderBy(o => o.OldName, StringComparer.Ordinal))
            {
                var name = op.NewName;
                var (stem, ext) = Rules.Split(name, op.IsDirectory);
                for (int n = 2; taken.Contains(name) && !name.Equals(op.OldName, StringComparison.OrdinalIgnoreCase); n++) name = $"{stem} ({n}){ext}";
                op.NewName = name;
                taken.Add(name);
            }
        }
    }

    public static Task<RenameOutcome> RunAsync(string root, IEnumerable<RenameOp> ops, IProgress<int>? progress = null, CancellationToken ct = default) =>
        Task.Run(() => Run(root, ops, progress, ct), CancellationToken.None);

    /// <summary>Tiefste Elemente zuerst, damit die Pfade der übrigen gültig bleiben. Fehler brechen nicht ab.</summary>
    public static RenameOutcome Run(string root, IEnumerable<RenameOp> ops, IProgress<int>? progress = null, CancellationToken ct = default)
    {
        var log = new RenameLog(DateTime.Now, root, []);
        var failures = new List<RenameFailure>();
        int n = 0;
        foreach (var op in ops.Where(o => o.Selected).OrderByDescending(o => o.Depth))
        {
            if (ct.IsCancellationRequested) break;
            try
            {
                Move(op.OldPath, op.NewPath, op.IsDirectory);
                log.Done.Add(new RenameEntry(op.OldPath, op.NewPath, op.IsDirectory));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                failures.Add(new RenameFailure(op.OldPath, ex.Message));
            }
            progress?.Report(++n);
        }
        return new RenameOutcome(log, failures);
    }

    public static RenameOutcome Undo(RenameLog log)
    {
        var back = new RenameLog(DateTime.Now, log.Root, []);
        var failures = new List<RenameFailure>();
        foreach (var e in Enumerable.Reverse(log.Done))
        {
            try { Move(e.To, e.From, e.IsDirectory); back.Done.Add(new RenameEntry(e.To, e.From, e.IsDirectory)); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException) { failures.Add(new RenameFailure(e.To, ex.Message)); }
        }
        return new RenameOutcome(back, failures);
    }

    private static void Move(string from, string to, bool isDir)
    {
        bool exists = isDir ? System.IO.Directory.Exists(to) : File.Exists(to);
        bool caseOnly = string.Equals(from, to, StringComparison.OrdinalIgnoreCase);
        if (exists && !caseOnly) throw new IOException("Target exists: " + to);
        if (caseOnly)
        {
            var tmp = from + ".pathrova-" + Guid.NewGuid().ToString("N")[..6];
            Move(from, tmp, isDir); Move(tmp, to, isDir);
            return;
        }
        if (isDir) System.IO.Directory.Move(from, to); else File.Move(from, to);
    }

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static string SaveLog(RenameLog log, string directory)
    {
        System.IO.Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"rename-{log.At:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..4]}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(log, Json));
        return path;
    }

    public static RenameLog? LoadLog(string path)
    {
        try { return JsonSerializer.Deserialize<RenameLog>(File.ReadAllText(path)); }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { return null; }
    }

    public static string? LatestLog(string directory) =>
        System.IO.Directory.Exists(directory) ? System.IO.Directory.GetFiles(directory, "rename-*.json").OrderDescending().FirstOrDefault() : null;
}
