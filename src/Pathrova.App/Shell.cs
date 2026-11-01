using System.Diagnostics;

namespace Pathrova.App;

/// <summary>Öffnen im Standardprogramm bzw. Explorer.</summary>
public static class Shell
{
    public static void Open(string path)
    {
        try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); } catch (Exception) { }
    }

    public static void Reveal(string path)
    {
        try
        {
            if (OperatingSystem.IsWindows()) Process.Start("explorer.exe", $"/select,\"{path}\"");
            else if (OperatingSystem.IsMacOS()) Process.Start("open", ["-R", path]);
            else Open(Path.GetDirectoryName(path)!);
        }
        catch (Exception) { }
    }
}
