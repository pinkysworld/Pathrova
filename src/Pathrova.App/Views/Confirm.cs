using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Pathrova.App.Views;

/// <summary>Kleine Rückfrage mit zwei Knöpfen.</summary>
public static class Confirm
{
    public static async Task<bool> Ask(Window owner, string title, string message, string yes, string no)
    {
        var w = new Window { Title = title, Width = 460, SizeToContent = SizeToContent.Height, CanResize = false, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        bool result = false;
        var ok = Ui.Button(yes, () => { result = true; w.Close(); }, "primary", "ConfirmYes");
        var cancel = Ui.Button(no, w.Close, automationId: "ConfirmNo");
        var buttons = Ui.Row(8, cancel, ok); buttons.HorizontalAlignment = HorizontalAlignment.Right;
        w.Content = Ui.Column(16, Ui.Text(title, "h2"), new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap }, buttons);
        ((Control)w.Content).Margin = new Thickness(20);
        await w.ShowDialog(owner);
        return result;
    }
}
