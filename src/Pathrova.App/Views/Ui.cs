using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using Pathrova.Core;

namespace Pathrova.App.Views;

/// <summary>Kleine Bausteine für die in C# gebauten Ansichten.</summary>
public static class Ui
{
    /// <summary>Anzeige von Ordnerpfaden (Screenshots ersetzen hier Testpfade).</summary>
    public static Func<string, string> DisplayPath { get; set; } = p => p;

    public static TextBlock Text(string text, string? cls = null, double? size = null)
    {
        var t = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        if (cls is not null) foreach (var c in cls.Split(' ')) t.Classes.Add(c);
        if (size is { } s) t.FontSize = s;
        return t;
    }

    public static Button Button(string text, Action onClick, string? cls = null, string? automationId = null)
    {
        var b = new Button { Content = text };
        if (cls is not null) foreach (var c in cls.Split(' ')) b.Classes.Add(c);
        b.Click += (_, _) => onClick();
        AutomationProperties.SetName(b, text);
        if (automationId is not null) AutomationProperties.SetAutomationId(b, automationId);
        return b;
    }

    public static StackPanel Row(double spacing, params Control[] children)
    {
        var p = new StackPanel { Orientation = Orientation.Horizontal, Spacing = spacing };
        p.Children.AddRange(children);
        return p;
    }

    public static StackPanel Column(double spacing, params Control[] children)
    {
        var p = new StackPanel { Spacing = spacing };
        p.Children.AddRange(children);
        return p;
    }

    public static Border Card(Control child, Thickness? padding = null) => new() { Classes = { "card" }, Child = child, Padding = padding ?? new Thickness(16) };

    public static T Res<T>(this Control c, string key) where T : class =>
        c.TryFindResource(key, c.ActualThemeVariant, out var v) && v is T t ? t : throw new KeyNotFoundException(key);

    /// <summary>Stufe als Form und Farbe (nicht nur Farbe): ✕ Fehler, ! Warnung, i Hinweis.</summary>
    public static Control SeverityBadge(Severity s)
    {
        var glyph = new TextBlock { Text = s switch { Severity.Error => "✕", Severity.Warning => "!", _ => "i" }, FontSize = 10, FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        glyph.BindBrush(TextBlock.ForegroundProperty, "OnAccent");
        var b = new Border { Width = 16, Height = 16, CornerRadius = new CornerRadius(s == Severity.Warning ? 3 : 8), Child = glyph, VerticalAlignment = VerticalAlignment.Center };
        b.BindBrush(Border.BackgroundProperty, SeverityBrushKey(s));
        return b;
    }

    public static string SeverityBrushKey(Severity s) => s switch { Severity.Error => "Error", Severity.Warning => "Warn", _ => "Info" };

    /// <summary>Dynamische Farbe, die beim Wechsel Hell/Dunkel mitgeht.</summary>
    public static void BindBrush(this AvaloniaObject target, AvaloniaProperty property, string key) =>
        target.Bind(property, new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension(key));
}
