using System.Globalization;
using System.Text.Json;
using Pathrova.Core;

namespace Pathrova.App;

public sealed class AppSettings
{
    public string Theme { get; set; } = "system";
    public string Language { get; set; } = "system";
    public string LastFolder { get; set; } = "";
    public CheckOptions Check { get; set; } = new();

    [System.Text.Json.Serialization.JsonIgnore]
    public bool English => Language switch { "en" => true, "de" => false, _ => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "de" };

    /// <summary>Unter MSIX wird LocalAppData automatisch in den Paketordner umgeleitet.</summary>
    public static string Directory => Environment.GetEnvironmentVariable("PATHROVA_TEST_HOME") is { Length: > 0 } t ? t
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pathrova");

    /// <summary>Rückgängig-Protokolle der Umbenennungen.</summary>
    public static string UndoDirectory => Path.Combine(Directory, "undo");

    private static string FilePath => Path.Combine(Directory, "settings.json");
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static AppSettings Load()
    {
        try { return File.Exists(FilePath) ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), Json) ?? new() : new(); }
        catch (Exception) { return new(); }
    }

    public void Save()
    {
        try
        {
            System.IO.Directory.CreateDirectory(Directory);
            var temp = FilePath + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(this, Json));
            File.Move(temp, FilePath, true);
        }
        catch (Exception) { }
    }
}
