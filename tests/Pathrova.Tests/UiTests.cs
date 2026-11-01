using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Pathrova.App;
using Pathrova.App.Views;
using Pathrova.Core;

namespace Pathrova.Tests;

public class UiTests
{
    private static string RepoRoot([System.Runtime.CompilerServices.CallerFilePath] string here = "") => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", ".."));

    private static AppSettings Settings(bool english) => new()
    {
        Language = english ? "en" : "de",
        Check = new CheckOptions { TargetPrefix = english ? "Shared Documents" : "Freigegebene Dokumente", LocalRoot = english ? @"C:\Users\Max\OneDrive - Contoso" : @"C:\Users\Max\OneDrive - Firma", BlockedExtensions = "exe" },
    };

    private static async Task<MainWindow> Scanned(TempDirs d, bool english)
    {
        var top = DemoData.Create(d.Root, english);
        var w = new MainWindow(Settings(english)) { Width = 1366, Height = 820 };
        w.Show();
        w.SetFolder(top);
        await w.RunScan();
        Dispatcher.UIThread.RunJobs();
        return w;
    }

    [AvaloniaFact]
    public async Task ScanShowsGroupsAndFilters()
    {
        using var d = new TempDirs();
        var w = await Scanned(d, false);
        Assert.NotNull(w.Result);
        Assert.Equal(w.Result!.Issues.Count, w.VisibleRows.Count);
        Assert.Contains("41 Dateien", w.StatusText);
        var group = w.Groups.ItemsSource!.Cast<object>().First(g => g.ToString()!.Contains("Ungültige Zeichen"));
        w.Groups.SelectedItem = group;
        Dispatcher.UIThread.RunJobs();
        Assert.All(w.VisibleRows, r => Assert.Equal(IssueKind.InvalidCharacter, r.Issue.Kind));
        Assert.Contains(w.VisibleRows, r => r.Proposal == "Angebot_ final_.docx");
        w.Close();
    }

    [AvaloniaFact]
    public async Task RenameWindowNeedsConfirmationAndUndoRestores()
    {
        using var d = new TempDirs();
        if (Directory.Exists(AppSettings.UndoDirectory)) Directory.Delete(AppSettings.UndoDirectory, true);
        var w = await Scanned(d, true);
        var plan = w.Plan.ToList();
        Assert.NotEmpty(plan);
        // Die Vorschau ändert nichts, solange nicht bestätigt wird.
        var rw = new RenameWindow(plan, Texts.For(true), w.Result!.Root);
        rw.Show(); Dispatcher.UIThread.RunJobs(); rw.Close();
        Assert.True(File.Exists(Path.Combine(w.Result.Root, "Staff", "Holiday plan <draft>.xlsx")));
        int before = w.Result.Issues.Count;
        await w.RunRename();
        Dispatcher.UIThread.RunJobs();
        Assert.True(w.Result!.Issues.Count < before);
        Assert.True(File.Exists(Path.Combine(w.Result.Root, "Staff", "Holiday plan _draft_.xlsx")));
        Assert.NotNull(Renamer.LatestLog(AppSettings.UndoDirectory));
        await w.UndoLatest();
        Dispatcher.UIThread.RunJobs();
        Assert.True(File.Exists(Path.Combine(w.Result!.Root, "Staff", "Holiday plan <draft>.xlsx")));
        Assert.Null(Renamer.LatestLog(AppSettings.UndoDirectory));
        Assert.Equal(before, w.Result.Issues.Count);
        w.Close();
    }

    [AvaloniaFact]
    public async Task ExportAndLanguageSwitch()
    {
        using var d = new TempDirs();
        var w = await Scanned(d, false);
        var csv = Path.Combine(d.Root, "b.csv"); var pdf = Path.Combine(d.Root, "b.pdf");
        w.ExportTo(csv); w.ExportTo(pdf);
        Assert.StartsWith("Stufe;Problem", File.ReadAllText(csv).TrimStart('\uFEFF'));
        Assert.True(new FileInfo(pdf).Length > 2000);
        w.SetLanguage("en");
        Dispatcher.UIThread.RunJobs();
        Assert.StartsWith("Pathrova – Path Checker", w.Title);
        Assert.Contains("41 files", w.StatusText);
        w.Close();
    }

    [AvaloniaFact]
    public async Task StoreScreenshots()
    {
        var outDir = Path.Combine(RepoRoot(), "store", "screenshots");
        foreach (var english in new[] { false, true })
        foreach (var dark in new[] { false, true })
        {
            using var d = new TempDirs();
            Avalonia.Application.Current!.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
            var dir = Directory.CreateDirectory(Path.Combine(outDir, english ? "en" : "de", dark ? "dark" : "light")).FullName;
            var root = d.Root;
            Ui.DisplayPath = p => p.StartsWith(root) ? @"\\fileserver\daten" + p[root.Length..].Replace('/', '\\') : p;
            var w = await Scanned(d, english);
            w.ShowFolderText();
            Save(w, Path.Combine(dir, "1-overview.png"));
            var g = w.Groups.ItemsSource!.Cast<object>().First(x => x.ToString()!.Contains(english ? "Path too long (cloud" : "Pfad zu lang (Cloud"));
            w.Groups.SelectedItem = g; Dispatcher.UIThread.RunJobs();
            Save(w, Path.Combine(dir, "2-long-paths.png"));
            w.ExpandOptions(); w.Groups.SelectedIndex = 0; Dispatcher.UIThread.RunJobs();
            Save(w, Path.Combine(dir, "3-rules.png"));
            var rw = new RenameWindow(w.Plan.ToList(), Texts.For(english), w.Result!.Root) { Width = 1100, Height = 700 };
            rw.Show(); Dispatcher.UIThread.RunJobs();
            Save(rw, Path.Combine(dir, "4-rename-preview.png"));
            rw.Close(); w.Close();
            Ui.DisplayPath = p => p;
        }
        Avalonia.Application.Current!.RequestedThemeVariant = ThemeVariant.Default;
        Assert.True(File.Exists(Path.Combine(outDir, "de", "light", "4-rename-preview.png")));
    }

    private static void Save(Window w, string path)
    {
        Dispatcher.UIThread.RunJobs();
        using var frame = w.CaptureRenderedFrame() ?? throw new InvalidOperationException("no frame");
#pragma warning disable CS0618
        frame.Save(path);
#pragma warning restore CS0618
    }
}
