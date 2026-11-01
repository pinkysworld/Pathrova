using System.Diagnostics;

namespace Pathrova.Core;

public sealed record ScanProgress(long Files, long Folders, string Current);

public sealed class ScanResult
{
    public required string Root { get; init; }
    public required CheckOptions Options { get; init; }
    public List<Issue> Issues { get; } = [];
    public long Files { get; set; }
    public long Folders { get; set; }
    public long Bytes { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Cancelled { get; set; }
    public DateTime CreatedAt { get; } = DateTime.Now;

    public IEnumerable<IGrouping<IssueKind, Issue>> Groups => Issues.GroupBy(i => i.Kind).OrderBy(g => g.Key);
    public int Count(Severity s) => Issues.Count(i => i.Severity == s);
    /// <summary>Betroffene Elemente einschließlich der zusammengefassten Unterelemente bei zu langen Pfaden.</summary>
    public long Affected(IssueKind k) => Issues.Where(i => i.Kind == k).Sum(i => 1L + i.Inside);
}

/// <summary>Durchläuft den Ordnerbaum einmal (ohne Rekursion auf dem Stack) und prüft jedes Element.</summary>
public static class Scanner
{
    public const long RecommendedMaxItems = 300_000;

    private static readonly EnumerationOptions Enum = new()
    {
        RecurseSubdirectories = false, IgnoreInaccessible = false, AttributesToSkip = 0, ReturnSpecialDirectories = false,
    };

    public static Task<ScanResult> ScanAsync(string root, CheckOptions options, IProgress<ScanProgress>? progress = null, CancellationToken ct = default) =>
        Task.Run(() => Scan(root, options, progress, ct), CancellationToken.None);

    public static ScanResult Scan(string root, CheckOptions o, IProgress<ScanProgress>? progress = null, CancellationToken ct = default)
    {
        root = Path.GetFullPath(root);
        var r = new ScanResult { Root = root, Options = o };
        var sw = Stopwatch.StartNew();
        var blocked = o.BlockedSet();
        var stack = new Stack<(DirectoryInfo Dir, string Rel, Issue? CloudParent, Issue? LocalParent, Issue? OfficeParent)>();
        stack.Push((new DirectoryInfo(root), "", null, null, null));
        long lastReport = 0;
        try
        {
            while (stack.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                var (dir, rel, cloudP, localP, officeP) = stack.Pop();
                List<FileSystemInfo> list;
                try { list = dir.EnumerateFileSystemInfos("*", Enum).ToList(); }
                catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or System.Security.SecurityException)
                {
                    r.Issues.Add(new Issue(dir.FullName, rel, true, IssueKind.AccessDenied, detail: ex.Message));
                    continue;
                }
                foreach (var e in list)
                {
                    ct.ThrowIfCancellationRequested();
                    bool isDir = e is DirectoryInfo;
                    var childRel = rel.Length == 0 ? e.Name : rel + "/" + e.Name;
                    if (isDir) r.Folders++; else { r.Files++; try { r.Bytes += ((FileInfo)e).Length; } catch (IOException) { } }

                    foreach (var (kind, detail) in Rules.CheckName(e.Name, isDir, rel.Length == 0, o, blocked))
                        r.Issues.Add(new Issue(e.FullName, childRel, isDir, kind, e.Name.Length, detail));
                    if (!isDir && e is FileInfo fi && SafeLength(fi) > o.MaxFileSize)
                        r.Issues.Add(new Issue(e.FullName, childRel, false, IssueKind.FileTooLarge, detail: Format.Size(SafeLength(fi))));

                    var cloud = Length(r, cloudP, IssueKind.PathTooLong, Rules.CloudLength(Rules.CloudPath(o, childRel), o.CountEncoded), o.CloudLimit, e, childRel, isDir);
                    var local = Length(r, localP, IssueKind.LocalPathTooLong, Rules.LocalPath(o, childRel).Length, o.LocalLimit, e, childRel, isDir);
                    var office = Length(r, officeP, IssueKind.OfficePathLong, Rules.LocalPath(o, childRel).Length, o.OfficeLimit, e, childRel, isDir);

                    if (isDir)
                    {
                        if (e.Attributes.HasFlag(FileAttributes.ReparsePoint))
                            r.Issues.Add(new Issue(e.FullName, childRel, true, IssueKind.LinkNotSupported, detail: e.LinkTarget ?? ""));
                        else
                            stack.Push(((DirectoryInfo)e, childRel, cloud, local, office));
                    }
                    else if (e.Attributes.HasFlag(FileAttributes.ReparsePoint) && e.LinkTarget is { } target)
                        r.Issues.Add(new Issue(e.FullName, childRel, false, IssueKind.LinkNotSupported, detail: target));

                    long n = r.Files + r.Folders;
                    if (progress is not null && n - lastReport >= 1000) { lastReport = n; progress.Report(new ScanProgress(r.Files, r.Folders, e.FullName)); }
                }
            }
        }
        catch (OperationCanceledException) { r.Cancelled = true; }
        if (r.Files + r.Folders > RecommendedMaxItems)
            r.Issues.Add(new Issue(root, "", true, IssueKind.TooManyItems, detail: (r.Files + r.Folders).ToString("N0")));
        r.Duration = sw.Elapsed;
        progress?.Report(new ScanProgress(r.Files, r.Folders, ""));
        return r;
    }

    private static long SafeLength(FileInfo f) { try { return f.Length; } catch (IOException) { return 0; } }

    /// <summary>Meldet ein zu langes Element einmal; Unterelemente eines schon gemeldeten Ordners erhöhen nur dessen Zähler.</summary>
    private static Issue? Length(ScanResult r, Issue? parent, IssueKind kind, int length, int limit, FileSystemInfo e, string rel, bool isDir)
    {
        if (length <= limit) return null;
        if (parent is not null) { parent.Inside++; return parent; }
        var issue = new Issue(e.FullName, rel, isDir, kind, length, $"{length}/{limit}");
        r.Issues.Add(issue);
        return issue;
    }
}
