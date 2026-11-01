using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Pathrova.Core;

namespace Pathrova.App.Views;

public sealed record IssueRow(Issue Issue, string Kind, string Path, string Detail, string? Proposal);

public sealed class MainWindow : Window
{
    private readonly AppSettings _settings;
    private Texts T => Texts.For(_settings.English);
    private CheckOptions O => _settings.Check;

    private TextBox _folder = null!, _search = null!, _prefix = null!, _localRoot = null!, _blocked = null!;
    private CheckBox _encoded = null!, _hash = null!, _office = null!;
    private Button _scan = null!, _rename = null!, _undo = null!, _csv = null!, _pdf = null!;
    private TextBlock _status = null!;
    private ProgressBar _progress = null!;
    private ListBox _groups = null!, _list = null!;
    private Expander _options = null!;
    private StackPanel _kpis = null!;
    private Control _body = null!, _empty = null!;

    private ScanResult? _result;
    private List<RenameOp> _plan = [];
    private Dictionary<string, string> _proposals = [];
    private CancellationTokenSource? _cts;
    private IssueKind? _kindFilter;
    private Severity? _severityFilter;

    public ScanResult? Result => _result;
    public IReadOnlyList<RenameOp> Plan => _plan;
    public IReadOnlyList<IssueRow> VisibleRows => (_list.ItemsSource as IReadOnlyList<IssueRow>) ?? [];
    public ListBox Groups => _groups;
    public string StatusText => _status.Text ?? "";

    public MainWindow(AppSettings settings)
    {
        _settings = settings;
        Width = 1280; Height = 780; MinWidth = 820; MinHeight = 540;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        try { Icon = new WindowIcon(Avalonia.Platform.AssetLoader.Open(new Uri("avares://Pathrova/Assets/logo.png"))); } catch (Exception) { }
        Build();
        Closing += (_, _) => { _cts?.Cancel(); Capture(); _settings.Save(); };
    }

    public void SetFolder(string path) { _folder.Text = path; }

    /// <summary>Für Screenshots: Ordnerfeld mit Anzeigepfad statt Testpfad.</summary>
    public void ShowFolderText() => _folder.Text = Ui.DisplayPath(_folder.Text ?? "");

    public void ExpandOptions() => _options.IsExpanded = true;

    private void Build()
    {
        Title = T.AppTitle;
        var folderText = _folder?.Text ?? _settings.LastFolder;

        var logo = new Image { Width = 28, Height = 28 };
        try { logo.Source = new Avalonia.Media.Imaging.Bitmap(Avalonia.Platform.AssetLoader.Open(new Uri("avares://Pathrova/Assets/logo.png"))); } catch (Exception) { }
        var settingsBtn = new Button { Content = "⚙  " + T.Settings, Flyout = SettingsMenu() };
        settingsBtn.Classes.Add("ghost");
        AutomationProperties.SetName(settingsBtn, T.Settings);
        var header = new Grid { ColumnDefinitions = new("Auto,Auto,*,Auto"), ColumnSpacing = 12 };
        header.Children.Add(logo);
        var name = Ui.Text("Pathrova", "h2"); Grid.SetColumn(name, 1); header.Children.Add(name);
        Grid.SetColumn(settingsBtn, 3); header.Children.Add(settingsBtn);

        _folder = new TextBox { Text = folderText, PlaceholderText = T.FolderHint };
        AutomationProperties.SetName(_folder, T.Folder); AutomationProperties.SetAutomationId(_folder, "Folder");
        _folder.KeyDown += (_, e) => { if (e.Key == Key.Enter) { _ = RunScan(); e.Handled = true; } };
        var browse = Ui.Button(T.Browse, () => _ = Browse(), automationId: "Browse");
        _scan = Ui.Button(T.Scan, () => { if (_cts is null) _ = RunScan(); else _cts.Cancel(); }, "primary", "Scan");
        var folderRow = new Grid { ColumnDefinitions = new("*,Auto,Auto"), ColumnSpacing = 8 };
        folderRow.Children.Add(_folder);
        Grid.SetColumn(browse, 1); folderRow.Children.Add(browse);
        Grid.SetColumn(_scan, 2); folderRow.Children.Add(_scan);

        _prefix = Field(O.TargetPrefix, T.TargetPrefixHint, T.TargetPrefix, "Prefix");
        _localRoot = Field(O.LocalRoot, T.LocalRootHint, T.LocalRoot, "LocalRoot");
        _blocked = Field(O.BlockedExtensions, T.BlockedHint, T.Blocked, "Blocked");
        _encoded = Check(T.CountEncoded, O.CountEncoded, "Encoded");
        _hash = Check(T.WarnHash, O.WarnHashPercent, "WarnHash");
        _office = Check(T.WarnOffice, O.WarnOfficeCharacters, "WarnOffice");
        var optGrid = new Grid { ColumnDefinitions = new("Auto,*"), RowDefinitions = new("Auto,Auto,Auto,Auto"), ColumnSpacing = 12, RowSpacing = 8 };
        void Put(Control c, int row, int col) { Grid.SetRow(c, row); Grid.SetColumn(c, col); optGrid.Children.Add(c); }
        Put(Ui.Text(T.TargetPrefix), 0, 0); Put(_prefix, 0, 1);
        Put(Ui.Text(T.LocalRoot), 1, 0); Put(_localRoot, 1, 1);
        Put(Ui.Text(T.Blocked), 2, 0); Put(_blocked, 2, 1);
        var checks = new WrapPanel { ItemSpacing = 18, Children = { _encoded, _hash, _office } };
        Put(checks, 3, 1);
        _options = new Expander { Header = OptionsHeader(), Content = optGrid, IsExpanded = false, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 6, 0, 0) };
        _options.Classes.Add("options");
        AutomationProperties.SetAutomationId(_options, "Options");

        _kpis = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 28, IsVisible = false };
        _status = new TextBlock { Text = "", TextWrapping = TextWrapping.Wrap };
        _status.Classes.Add("muted");
        _progress = new ProgressBar { IsIndeterminate = true, IsVisible = false, Height = 4, MinHeight = 4 };

        _groups = new ListBox { Background = Brushes.Transparent, ItemTemplate = new FuncDataTemplate<GroupItem>((g, _) => g is null ? new Panel() : GroupRow(g)) };
        AutomationProperties.SetAutomationId(_groups, "Groups");
        _groups.SelectionChanged += (_, _) =>
        {
            if (_groups.SelectedItem is GroupItem g) { _kindFilter = g.Kind; _severityFilter = g.Severity; Refresh(); }
        };
        _search = new TextBox { PlaceholderText = T.Search, Width = 280, HorizontalAlignment = HorizontalAlignment.Left };
        AutomationProperties.SetName(_search, T.Search);
        _search.TextChanged += (_, _) => Refresh();
        _list = new ListBox { Background = Brushes.Transparent, ItemTemplate = new FuncDataTemplate<IssueRow>((r, _) => r is null ? new Panel() : IssueControl(r)) };
        AutomationProperties.SetAutomationId(_list, "Issues");
        _list.DoubleTapped += (_, _) => { if (_list.SelectedItem is IssueRow r) Shell.Reveal(r.Issue.FullPath); };

        var groupsCard = Ui.Card(_groups, new Thickness(6));
        var listPanel = new DockPanel();
        var searchRow = Ui.Row(8, _search); searchRow.Margin = new Thickness(0, 0, 0, 8);
        DockPanel.SetDock(searchRow, Dock.Top); listPanel.Children.Add(searchRow);
        listPanel.Children.Add(Ui.Card(_list, new Thickness(4)));
        var body = new Grid { ColumnDefinitions = new("330,*"), ColumnSpacing = 14 };
        body.Children.Add(groupsCard);
        Grid.SetColumn(listPanel, 1); body.Children.Add(listPanel);
        _body = body;
        _empty = EmptyState();

        _rename = Ui.Button(T.Rename, () => _ = OpenRename(), "primary", "Rename");
        _undo = Ui.Button(T.Undo, () => _ = UndoLast(), automationId: "Undo");
        _csv = Ui.Button("⇩  " + T.ExportCsv, () => _ = Export("csv"), automationId: "ExportCsv");
        _pdf = Ui.Button("⇩  " + T.ExportPdf, () => _ = Export("pdf"), automationId: "ExportPdf");
        var actions = new Grid { ColumnDefinitions = new("*,Auto"), ColumnSpacing = 16 };
        var buttons = Ui.Row(8, _undo, _csv, _pdf, _rename); Grid.SetColumn(buttons, 1); actions.Children.Add(buttons);

        var root = new DockPanel { Margin = new Thickness(20, 14, 20, 16) };
        var top = Ui.Column(12, header, Ui.Card(Ui.Column(4, folderRow, _options), new Thickness(14, 12)), Ui.Column(6, _kpis, _status, _progress));
        top.Margin = new Thickness(0, 0, 0, 12);
        DockPanel.SetDock(top, Dock.Top); root.Children.Add(top);
        actions.Margin = new Thickness(0, 12, 0, 0);
        DockPanel.SetDock(actions, Dock.Bottom); root.Children.Add(actions);
        root.Children.Add(new Panel { Children = { body, _empty } });
        Content = root;

        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, (_, e) => { e.DragEffects = DragDropEffects.Link; e.Handled = true; });
        AddHandler(DragDrop.DropEvent, (_, e) =>
        {
            var path = e.DataTransfer.TryGetFiles()?.Select(f => f.TryGetLocalPath()).OfType<string>().FirstOrDefault();
            if (path is null) return;
            _folder.Text = Directory.Exists(path) ? path : System.IO.Path.GetDirectoryName(path);
            e.Handled = true;
        });

        if (_result is not null) ShowResult(); else UpdateButtons();
        _body.IsVisible = _result is not null; _empty.IsVisible = _result is null && _cts is null;
    }

    private Control EmptyState()
    {
        var icon = new Image { Width = 72, Height = 72 };
        try { icon.Source = new Avalonia.Media.Imaging.Bitmap(Avalonia.Platform.AssetLoader.Open(new Uri("avares://Pathrova/Assets/logo.png"))); } catch (Exception) { }
        var head = new TextBlock { Text = T.Tagline, TextWrapping = TextWrapping.Wrap, FontSize = 20, FontWeight = FontWeight.SemiBold, TextAlignment = TextAlignment.Center, MaxWidth = 640 };
        var hint = new TextBlock { Text = T.Empty, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, MaxWidth = 560 };
        hint.Classes.Add("muted");
        var note = new TextBlock { Text = T.Disclaimer, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, MaxWidth = 560, FontSize = 12 };
        note.Classes.Add("muted");
        var col = Ui.Column(14, icon, head, hint, note);
        col.HorizontalAlignment = HorizontalAlignment.Center; col.VerticalAlignment = VerticalAlignment.Center;
        icon.HorizontalAlignment = HorizontalAlignment.Center;
        return Ui.Card(col, new Thickness(32));
    }

    private string OptionsHeader() => $"{T.Options}   ·   {T.Target}: {Rules.CloudPath(O, "")}/…   ·   {(O.CountEncoded ? T.Counting(true) : T.Counting(false))}";

    private Control Kpi(string value, string label, string brush)
    {
        var v = new TextBlock { Text = value, FontSize = 28, FontWeight = FontWeight.SemiBold };
        v.BindBrush(TextBlock.ForegroundProperty, brush);
        var l = Ui.Text(label, "muted", 13);
        var c = Ui.Column(0, v, l);
        AutomationProperties.SetName(c, $"{value} {label}");
        return c;
    }

    /// <summary>Kürzt lange Pfade in der Mitte: Anfang (Laufwerk/Server) und Dateiname bleiben sichtbar.</summary>
    public static string MiddleEllipsis(string path, int max = 150)
    {
        if (path.Length <= max) return path;
        int tail = Math.Min(path.Length - path.LastIndexOfAny(['\\', '/']) + 30, max * 2 / 3);
        int head = max - tail - 1;
        return path[..head] + "…" + path[^tail..];
    }

    private TextBox Field(string value, string hint, string label, string id)
    {
        var box = new TextBox { Text = value, PlaceholderText = hint };
        ToolTip.SetTip(box, hint);
        AutomationProperties.SetName(box, label); AutomationProperties.SetAutomationId(box, id);
        return box;
    }

    private static CheckBox Check(string label, bool value, string id)
    {
        var c = new CheckBox { Content = label, IsChecked = value };
        AutomationProperties.SetName(c, label); AutomationProperties.SetAutomationId(c, id);
        return c;
    }

    private MenuFlyout SettingsMenu()
    {
        var m = new MenuFlyout();
        var theme = new MenuItem { Header = T.Theme };
        foreach (var (key, label) in new[] { ("system", T.System), ("light", T.Light), ("dark", T.Dark) })
        {
            var item = new MenuItem { Header = (_settings.Theme == key ? "✓ " : "   ") + label };
            item.Click += (_, _) => { _settings.Theme = key; App.ApplyTheme(key); _settings.Save(); };
            theme.Items.Add(item);
        }
        var lang = new MenuItem { Header = T.Language };
        foreach (var (key, label) in new[] { ("system", T.System), ("de", "Deutsch"), ("en", "English") })
        {
            var item = new MenuItem { Header = (_settings.Language == key ? "✓ " : "   ") + label };
            item.Click += (_, _) => SetLanguage(key);
            lang.Items.Add(item);
        }
        var about = new MenuItem { Header = T.About };
        about.Click += (_, _) => _ = Confirm.Ask(this, "Pathrova 1.0", T.Disclaimer + "\n\n" + T.AboutPrivacy, "OK", T.Close);
        m.Items.Add(theme); m.Items.Add(lang); m.Items.Add(new Separator()); m.Items.Add(about);
        return m;
    }

    public void SetLanguage(string key) { Capture(); _settings.Language = key; _settings.Save(); Build(); }

    /// <summary>Übernimmt die Eingaben in die Einstellungen.</summary>
    private void Capture()
    {
        _settings.LastFolder = _folder.Text?.Trim() ?? "";
        O.TargetPrefix = _prefix.Text?.Trim() ?? "";
        O.LocalRoot = _localRoot.Text?.Trim() ?? "";
        O.BlockedExtensions = _blocked.Text?.Trim() ?? "";
        O.CountEncoded = _encoded.IsChecked == true;
        O.WarnHashPercent = _hash.IsChecked == true;
        O.WarnOfficeCharacters = _office.IsChecked == true;
    }

    private async Task Browse()
    {
        var start = Directory.Exists(_folder.Text) ? await StorageProvider.TryGetFolderFromPathAsync(_folder.Text!) : null;
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false, SuggestedStartLocation = start });
        if (folders.FirstOrDefault()?.TryGetLocalPath() is { } path) _folder.Text = path;
    }

    public async Task RunScan()
    {
        if (_cts is not null) return;
        Capture();
        _options.Header = OptionsHeader();
        var folder = _settings.LastFolder;
        if (!Directory.Exists(folder)) { _status.Text = T.NotFound; return; }
        _settings.Save();
        _cts = new CancellationTokenSource();
        _scan.Content = T.Cancel;
        _progress.IsVisible = true;
        _rename.IsEnabled = _csv.IsEnabled = _pdf.IsEnabled = false;
        _status.Text = T.Scanning(0, 0);
        _empty.IsVisible = false;
        var progress = new Progress<ScanProgress>(p => _status.Text = T.Scanning(p.Files, p.Folders));
        try
        {
            // Kopie der Optionen: Änderungen während des Scans gelten erst beim nächsten Durchgang.
            var opts = System.Text.Json.JsonSerializer.Deserialize<CheckOptions>(System.Text.Json.JsonSerializer.Serialize(O))!;
            _result = await Scanner.ScanAsync(folder, opts, progress, _cts.Token);
            _plan = await Task.Run(() => Renamer.Plan(_result));
            _proposals = _plan.ToDictionary(p => p.OldPath, p => p.NewName);
        }
        finally
        {
            _cts.Dispose(); _cts = null;
            _scan.Content = T.Scan;
            _progress.IsVisible = false;
        }
        _kindFilter = null; _severityFilter = null;
        ShowResult();
    }

    private void ShowResult()
    {
        if (_result is not { } r) return;
        var groups = new List<GroupItem> { new(T.All, null, null, r.Issues.Sum(i => 1L + i.Inside)) };
        foreach (var s in Enum.GetValues<Severity>())
            if (r.Issues.Any(i => i.Severity == s)) groups.Add(new(s switch { Severity.Error => T.Errors, Severity.Warning => T.Warnings, _ => T.Infos }, null, s, r.Issues.Where(i => i.Severity == s).Sum(i => 1L + i.Inside)));
        foreach (var g in r.Groups) groups.Add(new(T.Kind(g.Key), g.Key, Rules.SeverityOf(g.Key), r.Affected(g.Key)));
        _groups.ItemsSource = groups;
        _groups.SelectedItem = groups.FirstOrDefault(g => g.Kind == _kindFilter && g.Severity == (_kindFilter is null ? _severityFilter : g.Severity)) ?? groups[0];
        _status.Text = T.Done(r) + " " + (r.Issues.Count == 0 ? T.NoIssues : "");
        long Sum(Severity s) => r.Issues.Where(i => i.Severity == s).Sum(i => 1L + i.Inside);
        _kpis.Children.Clear();
        _kpis.Children.Add(Kpi(Format.Number(Sum(Severity.Error), _settings.English), T.Errors, "Error"));
        _kpis.Children.Add(Kpi(Format.Number(Sum(Severity.Warning), _settings.English), T.Warnings, "Warn"));
        _kpis.Children.Add(Kpi(Format.Number(Sum(Severity.Info), _settings.English), T.Infos, "Info"));
        _kpis.Children.Add(Kpi(Format.Number(_plan.Count, _settings.English), T.Fixable, "Accent"));
        _kpis.IsVisible = true;
        _body.IsVisible = true; _empty.IsVisible = false;
        Refresh();
    }

    private IEnumerable<Issue> Filtered()
    {
        if (_result is null) return [];
        var q = _search.Text?.Trim() ?? "";
        return _result.Issues.Where(i => (_kindFilter is null || i.Kind == _kindFilter) && (_severityFilter is null || i.Severity == _severityFilter)
                                         && (q.Length == 0 || i.FullPath.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(i => i.Kind).ThenBy(i => i.RelativePath, StringComparer.OrdinalIgnoreCase);
    }

    private void Refresh()
    {
        if (_result is null) { UpdateButtons(); return; }
        _list.ItemsSource = Filtered().Select(i => new IssueRow(i, T.Kind(i.Kind), Ui.DisplayPath(i.FullPath),
            string.Join(" · ", new[] { i.Detail, i.Inside > 0 ? T.AndInside(i.Inside) : "" }.Where(s => s.Length > 0)),
            _proposals.TryGetValue(i.FullPath, out var p) ? p : null)).ToList();
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool has = _result is not null && _cts is null;
        _rename.IsEnabled = has && _plan.Count > 0;
        _csv.IsEnabled = _pdf.IsEnabled = has;
        _undo.IsEnabled = Renamer.LatestLog(AppSettings.UndoDirectory) is not null;
    }

    private sealed record GroupItem(string Label, IssueKind? Kind, Severity? Severity, long Count);

    private Control GroupRow(GroupItem g)
    {
        Control dot = g.Severity is { } s ? Ui.SeverityBadge(s) : new Panel { Width = 16 };
        var label = Ui.Text(g.Label, g.Kind is null ? "h2" : null, g.Kind is null ? 14 : 13.5);
        var count = new Border { Classes = { "count" }, CornerRadius = new CornerRadius(9), Padding = new Thickness(8, 1), Child = Ui.Text(Format.Number(g.Count, _settings.English), size: 12) };
        var grid = new Grid { ColumnDefinitions = new("Auto,*,Auto"), ColumnSpacing = 8, Margin = new Thickness(g.Kind is null && g.Severity is not null ? 0 : g.Kind is null ? 0 : 10, 7, 6, 7) };
        grid.Children.Add(dot);
        Grid.SetColumn(label, 1); grid.Children.Add(label);
        Grid.SetColumn(count, 2); grid.Children.Add(count);
        AutomationProperties.SetName(grid, $"{g.Label}: {g.Count}");
        if (g.Kind is { } k) ToolTip.SetTip(grid, T.Explain(k));
        return grid;
    }

    private Control IssueControl(IssueRow r)
    {
        var kind = Ui.Text(r.Kind, size: 12);
        kind.FontWeight = FontWeight.SemiBold;
        kind.BindBrush(TextBlock.ForegroundProperty, Ui.SeverityBrushKey(r.Issue.Severity));
        var path = Ui.Text(MiddleEllipsis(r.Path));
        ToolTip.SetTip(path, r.Issue.FullPath);
        var line2 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 14 };
        line2.Children.Add(Ui.Row(6, Ui.SeverityBadge(r.Issue.Severity), kind));
        if (r.Detail.Length > 0) line2.Children.Add(Ui.Text(r.Detail, "muted", 12));
        if (r.Proposal is { } p) { var prop = Ui.Text("→ " + p, size: 12); prop.BindBrush(TextBlock.ForegroundProperty, "Accent"); line2.Children.Add(prop); }
        var col = Ui.Column(3, path, line2);
        col.Margin = new Thickness(12, 9);
        var row = new Border { Child = col, BorderThickness = new Thickness(0, 0, 0, 1) };
        row.BindBrush(Border.BorderBrushProperty, "Line");
        AutomationProperties.SetName(row, $"{r.Kind}: {r.Issue.FullPath}");
        return row;
    }

    private async Task OpenRename()
    {
        if (_result is null || _plan.Count == 0) { _status.Text = T.NothingToRename; return; }
        var w = new RenameWindow(_plan, T, _result.Root);
        var go = await w.ShowDialog<bool>(this);
        if (!go) return;
        await RunRename();
    }

    /// <summary>Führt die ausgewählten Vorschläge aus (nach Bestätigung im Vorschaufenster) und prüft neu.</summary>
    public async Task RunRename()
    {
        if (_result is null) return;
        var outcome = await Renamer.RunAsync(_result.Root, _plan);
        if (outcome.Log.Done.Count > 0) Renamer.SaveLog(outcome.Log, AppSettings.UndoDirectory);
        var msg = T.Renamed(outcome.Log.Done.Count, outcome.Failures.Count);
        await RunScan();
        _status.Text = msg.TrimEnd('.', ' ', '…') + " · " + StatusText;
    }

    private async Task UndoLast()
    {
        if (Renamer.LatestLog(AppSettings.UndoDirectory) is not { } file || Renamer.LoadLog(file) is not { } log) { UpdateButtons(); return; }
        if (!await Confirm.Ask(this, T.Undo, T.ConfirmUndo(log.Done.Count), T.UndoDo, T.No)) return;
        await UndoLatest();
    }

    public async Task UndoLatest()
    {
        if (Renamer.LatestLog(AppSettings.UndoDirectory) is not { } file || Renamer.LoadLog(file) is not { } log) return;
        var outcome = await Task.Run(() => Renamer.Undo(log));
        // Erledigtes Protokoll umbenennen statt löschen (Nachweis bleibt).
        try { File.Move(file, System.IO.Path.ChangeExtension(file, ".undone")); } catch (IOException) { }
        var msg = T.Undone(outcome.Log.Done.Count, outcome.Failures.Count);
        if (_result is not null && Directory.Exists(_result.Root)) await RunScan();
        _status.Text = msg + " " + StatusText;
        UpdateButtons();
    }

    private async Task Export(string ext)
    {
        if (_result is null) return;
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = $"Pathrova-{(T.English ? "report" : "Bericht")}-{DateTime.Now:yyyy-MM-dd}.{ext}",
            DefaultExtension = ext, ShowOverwritePrompt = true,
            FileTypeChoices = [new FilePickerFileType(ext.ToUpperInvariant()) { Patterns = ["*." + ext] }],
        });
        if (file?.TryGetLocalPath() is not { } path) return;
        ExportTo(path);
    }

    public void ExportTo(string path)
    {
        if (_result is null) return;
        try
        {
            var items = Filtered().ToList();
            if (path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) Reports.WritePdf(path, _result, items, _settings.English);
            else Reports.WriteCsv(path, _result, items, _settings.English, _proposals);
            _status.Text = T.Saved(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { _status.Text = ex.Message; }
    }
}
