using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Pathrova.App.Views;

namespace Pathrova.App;

public partial class App : Application
{
    public static AppSettings Settings { get; private set; } = new();

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // Das Fluent-Template blendet Platzhalter auf 50 % ab (zu wenig Kontrast). Ein lokaler Wert schlägt das Template.
        Avalonia.Controls.Primitives.TemplatedControl.TemplateAppliedEvent.AddClassHandler<Avalonia.Controls.TextBox>((tb, e) =>
        {
            if (e.NameScope.Find("PART_Placeholder") is Avalonia.Controls.TextBlock p) p.Opacity = 1;
        });
        Settings = AppSettings.Load();
        ApplyTheme(Settings.Theme);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var main = new MainWindow(Settings) { WindowState = Avalonia.Controls.WindowState.Maximized };
            desktop.MainWindow = main;
            // „pathrova <Ordner>“ prüft sofort.
            if (desktop.Args is [var folder] && System.IO.Directory.Exists(folder))
            {
                main.SetFolder(folder);
                main.Opened += (_, _) => _ = main.RunScan();
            }
        }
        base.OnFrameworkInitializationCompleted();
    }

    public static void ApplyTheme(string theme)
    {
        if (Current is null) return;
        Current.RequestedThemeVariant = theme switch { "light" => ThemeVariant.Light, "dark" => ThemeVariant.Dark, _ => ThemeVariant.Default };
    }
}
