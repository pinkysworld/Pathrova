namespace Pathrova.Tests;

public sealed class TempDirs : IDisposable
{
    public string Root { get; } = Directory.CreateTempSubdirectory("pathrova-test-").FullName;
    public string Left => Directory.CreateDirectory(Path.Combine(Root, "left")).FullName;
    public string Right => Directory.CreateDirectory(Path.Combine(Root, "right")).FullName;

    public static string Write(string root, string rel, string content, DateTime? utc = null)
    {
        var p = Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(p)!);
        File.WriteAllText(p, content);
        File.SetLastWriteTimeUtc(p, utc ?? new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc));
        return p;
    }

    public void Dispose() { try { Directory.Delete(Root, true); } catch (IOException) { } }
}
