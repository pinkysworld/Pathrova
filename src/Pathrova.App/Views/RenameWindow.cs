using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Pathrova.Core;

namespace Pathrova.App.Views;

/// <summary>Vorschau aller Umbenennungen mit Auswahl. Gibt true zurück, wenn der Nutzer ausdrücklich bestätigt hat.</summary>
public sealed class RenameWindow : Window
{
    private readonly List<RenameOp> _ops;
    private readonly Texts _t;
    private readonly Button _go;
    private readonly ListBox _list;

    public RenameWindow(List<RenameOp> ops, Texts t, string root)
    {
        _ops = ops; _t = t;
        Title = t.RenameTitle; Width = 980; Height = 640; MinWidth = 640; MinHeight = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var intro = new TextBlock { Text = t.RenameIntro(ops.Count), TextWrapping = TextWrapping.Wrap };
        intro.Classes.Add("muted");
        _list = new ListBox { ItemsSource = ops, Background = Brushes.Transparent, ItemTemplate = new FuncDataTemplate<RenameOp>((o, _) => o is null ? new Panel() : Row(o)) };
        AutomationProperties.SetAutomationId(_list, "RenameList");
        var all = Ui.Button(t.SelectAll, () => SetAll(true), "ghost", "SelectAll");
        var none = Ui.Button(t.SelectNone, () => SetAll(false), "ghost", "SelectNone");
        _go = Ui.Button(t.RenameNow(ops.Count(o => o.Selected)), () => _ = Go(), "primary", "RenameNow");
        var close = Ui.Button(t.Close, () => Close(false), automationId: "RenameClose");
        var bottom = new Grid { ColumnDefinitions = new("Auto,*,Auto"), Margin = new Thickness(0, 12, 0, 0) };
        bottom.Children.Add(Ui.Row(4, all, none));
        var right = Ui.Row(8, close, _go); Grid.SetColumn(right, 2); bottom.Children.Add(right);
        var dock = new DockPanel { Margin = new Thickness(20) };
        var top = Ui.Column(6, Ui.Text(t.RenameTitle, "h2"), intro, Ui.Text(Ui.DisplayPath(root), "muted", 12));
        top.Margin = new Thickness(0, 0, 0, 12);
        DockPanel.SetDock(top, Dock.Top); dock.Children.Add(top);
        DockPanel.SetDock(bottom, Dock.Bottom); dock.Children.Add(bottom);
        dock.Children.Add(Ui.Card(_list, new Thickness(4)));
        Content = dock;
    }

    public IReadOnlyList<RenameOp> Ops => _ops;

    private void SetAll(bool on)
    {
        foreach (var o in _ops) o.Selected = on;
        _list.ItemsSource = null; _list.ItemsSource = _ops;
        Update();
    }

    private void Update()
    {
        int n = _ops.Count(o => o.Selected);
        _go.Content = _t.RenameNow(n);
        _go.IsEnabled = n > 0;
    }

    private Control Row(RenameOp o)
    {
        var box = new CheckBox { IsChecked = o.Selected, VerticalAlignment = VerticalAlignment.Center };
        AutomationProperties.SetName(box, o.OldName);
        box.IsCheckedChanged += (_, _) => { o.Selected = box.IsChecked == true; Update(); };
        var from = Ui.Text(o.OldName);
        var to = Ui.Text(o.NewName); to.FontWeight = FontWeight.SemiBold; to.BindBrush(TextBlock.ForegroundProperty, "Accent");
        var rel = Ui.Text(o.RelativePath, "muted", 12);
        var names = new Grid { ColumnDefinitions = new("*,Auto,*"), ColumnSpacing = 10 };
        names.Children.Add(from);
        var arrow = Ui.Text("→", "muted"); Grid.SetColumn(arrow, 1); names.Children.Add(arrow);
        Grid.SetColumn(to, 2); names.Children.Add(to);
        var grid = new Grid { ColumnDefinitions = new("Auto,*"), ColumnSpacing = 10, Margin = new Thickness(8, 5) };
        grid.Children.Add(box);
        var col = Ui.Column(1, names, rel); Grid.SetColumn(col, 1); grid.Children.Add(col);
        return grid;
    }

    private async Task Go()
    {
        int n = _ops.Count(o => o.Selected);
        if (n == 0) return;
        if (await Confirm.Ask(this, _t.RenameTitle, _t.ConfirmRename(n), _t.Yes, _t.No)) Close(true);
    }
}
