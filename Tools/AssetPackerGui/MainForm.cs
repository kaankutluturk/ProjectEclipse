using System.Diagnostics;
using System.Media;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Packer = Eclipse.AssetPacker.Program;

namespace Eclipse.AssetPackerGui;

internal static class Entry
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

// Browser/editor for an AssetPacker workspace. Every change goes through the AssetPacker
// commands (move/delete/prune/check/repack-all), so the GUI and CLI behave identically.
internal sealed class MainForm : Form
{
    private const string CatalogRelative = "Assets/Resources/SF2Content/Art/catalog.json";
    private const string AllGroups = "(all groups)";

    private readonly TextBox _repoBox = new() { Dock = DockStyle.Fill };
    private readonly TextBox _workspaceBox = new() { Dock = DockStyle.Fill };
    private readonly ListBox _groups = new() { Dock = DockStyle.Fill, IntegralHeight = false };
    private readonly TextBox _filter = new() { Dock = DockStyle.Top, PlaceholderText = "Filter by address or name (supports * and **)" };
    private readonly ListView _assets = new()
    {
        Dock = DockStyle.Fill, View = View.Details, VirtualMode = true, FullRowSelect = true, HideSelection = false,
    };
    private readonly PictureBox _preview = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(48, 48, 48) };
    private readonly TextBox _details = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font(FontFamily.GenericMonospace, 9f) };
    private readonly TextBox _log = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font(FontFamily.GenericMonospace, 9f) };
    private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft };
    private readonly List<Control> _busyControls = [];

    private List<AssetRow> _all = [];
    private List<AssetRow> _visible = [];
    private List<string> _groupOrder = [];
    private int _sortColumn;
    private bool _sortDescending;
    private SoundPlayer? _player;

    public MainForm()
    {
        Text = "Eclipse Asset Packer";
        Width = 1400;
        Height = 900;
        StartPosition = FormStartPosition.CenterScreen;
        BuildLayout();
        Console.SetOut(new LogWriter(this));
        Console.SetError(new LogWriter(this));

        _repoBox.Text = FindRepository() ?? string.Empty;
        _workspaceBox.Text = Settings.Load() ?? DefaultWorkspace(_repoBox.Text);
        Shown += (_, _) => ReloadWorkspace();
        FormClosing += (_, _) => Settings.Save(_workspaceBox.Text);
    }

    // ------------------------------------------------------------------ layout

    private void BuildLayout()
    {
        var paths = new TableLayoutPanel { Dock = DockStyle.Top, Height = 64, ColumnCount = 3, Padding = new Padding(6, 6, 6, 0) };
        paths.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        paths.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        paths.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        paths.Controls.Add(new Label { Text = "Game project:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        paths.Controls.Add(_repoBox, 1, 0);
        paths.Controls.Add(MakeButton("Browse...", BrowseRepository), 2, 0);
        paths.Controls.Add(new Label { Text = "Workspace folder:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        paths.Controls.Add(_workspaceBox, 1, 1);
        paths.Controls.Add(MakeButton("Browse...", BrowseWorkspace), 2, 1);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(6, 4, 6, 0) };
        actions.Controls.Add(MakeButton("1. Unpack all bundles", UnpackAll, 160));
        actions.Controls.Add(MakeButton("Reload", ReloadWorkspace));
        actions.Controls.Add(MakeButton("Open folder", () => OpenInExplorer(_workspaceBox.Text, select: false)));
        actions.Controls.Add(MakeButton("Check for problems", () => RunCommand("check", Workspace), 140));
        actions.Controls.Add(MakeButton("Delete unused files", CleanUp, 140));
        actions.Controls.Add(MakeButton("Export CSV report...", ExportReport, 140));
        actions.Controls.Add(MakeButton("2. Repack into game", Repack, 160));

        _assets.Columns.Add("Address", 330);
        _assets.Columns.Add("Name", 160);
        _assets.Columns.Add("Type", 55);
        _assets.Columns.Add("Group", 120);
        _assets.Columns.Add("Size", 70, HorizontalAlignment.Right);
        _assets.RetrieveVirtualItem += (_, e) => e.Item = ToItem(_visible[e.ItemIndex]);
        _assets.SelectedIndexChanged += (_, _) => ShowSelection();
        _assets.ColumnClick += (_, e) => SortBy(e.Column);
        _assets.DoubleClick += (_, _) => { if (Selected().FirstOrDefault() is { } row) OpenInExplorer(row.PayloadPath, select: true); };
        _assets.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Delete) DeleteSelected();
            if (e.Control && e.KeyCode == Keys.A) SelectAllVisible();
        };
        _groups.SelectedIndexChanged += (_, _) => ApplyFilter();
        _filter.TextChanged += (_, _) => ApplyFilter();

        var assetButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 36, Padding = new Padding(0, 4, 0, 0) };
        assetButtons.Controls.Add(MakeButton("Move selected to group...", MoveSelected, 180));
        assetButtons.Controls.Add(MakeButton("Delete selected", DeleteSelected, 120));
        assetButtons.Controls.Add(MakeButton("Select all shown", SelectAllVisible, 120));
        assetButtons.Controls.Add(MakeButton("Show file", () => { if (Selected().FirstOrDefault() is { } row) OpenInExplorer(row.PayloadPath, select: true); }));

        var groupPanel = new Panel { Dock = DockStyle.Fill };
        groupPanel.Controls.Add(_groups);
        groupPanel.Controls.Add(new Label { Text = "Groups (first wins on duplicates)", Dock = DockStyle.Top, Height = 20 });

        var assetPanel = new Panel { Dock = DockStyle.Fill };
        assetPanel.Controls.Add(_assets);
        assetPanel.Controls.Add(_filter);
        assetPanel.Controls.Add(assetButtons);

        var playButton = MakeButton("Play sound", PlaySelected);
        playButton.Dock = DockStyle.Top;
        var previewSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
        previewSplit.Panel1.Controls.Add(_preview);
        previewSplit.Panel2.Controls.Add(_details);
        previewSplit.Panel2.Controls.Add(playButton);

        var right = new SplitContainer { Dock = DockStyle.Fill };
        right.Panel1.Controls.Add(assetPanel);
        right.Panel2.Controls.Add(previewSplit);

        var main = new SplitContainer { Dock = DockStyle.Fill };
        main.Panel1.Controls.Add(groupPanel);
        main.Panel2.Controls.Add(right);

        var outer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
        outer.Panel1.Controls.Add(main);
        outer.Panel2.Controls.Add(_log);

        Controls.Add(outer);
        Controls.Add(actions);
        Controls.Add(paths);
        Controls.Add(_status);

        Load += (_, _) =>
        {
            outer.SplitterDistance = Math.Max(200, ClientSize.Height - 280);
            main.SplitterDistance = 230;
            right.SplitterDistance = Math.Max(300, right.Width - 380);
            previewSplit.SplitterDistance = Math.Max(150, previewSplit.Height / 2);
        };
    }

    private Button MakeButton(string text, Action action, int width = 90)
    {
        var button = new Button { Text = text, Width = width, Height = 28 };
        button.Click += (_, _) => action();
        _busyControls.Add(button);
        return button;
    }

    // ------------------------------------------------------------------ toolbar actions

    private string Workspace => _workspaceBox.Text.Trim();
    private string Repository => _repoBox.Text.Trim();
    private bool HasWorkspace => File.Exists(Path.Combine(Workspace, "workspace.json"));

    private void BrowseRepository()
    {
        using var dialog = new FolderBrowserDialog { Description = "Select the Eclipse project folder (contains Assets/)", SelectedPath = Repository };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _repoBox.Text = dialog.SelectedPath;
        if (!File.Exists(Path.Combine(Repository, CatalogRelative)))
            MessageBox.Show(this, "That folder does not contain " + CatalogRelative, Text);
    }

    private void BrowseWorkspace()
    {
        using var dialog = new FolderBrowserDialog { Description = "Select or create the workspace folder", SelectedPath = Workspace, ShowNewFolderButton = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _workspaceBox.Text = dialog.SelectedPath;
        Settings.Save(Workspace);
        ReloadWorkspace();
    }

    private void UnpackAll()
    {
        if (!File.Exists(Path.Combine(Repository, CatalogRelative)))
        {
            MessageBox.Show(this, "Pick the game project folder first (the one containing Assets/).", Text);
            return;
        }
        if (HasWorkspace)
        {
            MessageBox.Show(this, "This workspace is already unpacked. Pick an empty folder, or delete this one to start over.", Text);
            return;
        }
        if (Directory.Exists(Workspace) && Directory.EnumerateFileSystemEntries(Workspace).Any())
        {
            MessageBox.Show(this, "The workspace folder must be empty.", Text);
            return;
        }
        if (Ask("Unpack every bundle into\n" + Workspace + "?\n\nThis needs about 3 GB of free disk space.") != DialogResult.Yes)
            return;
        RunCommand("unpack-all", Workspace, "--catalog", Path.Combine(Repository, CatalogRelative),
            "--bundles", Path.Combine(Repository, "Assets/StreamingAssets/SF2Content/ArtBundles"));
    }

    private void CleanUp()
    {
        if (!RequireWorkspace()) return;
        if (Ask("Delete payload files that no asset uses, and groups with no assets left?") == DialogResult.Yes)
            RunCommand("prune", Workspace);
    }

    private void ExportReport()
    {
        if (!RequireWorkspace()) return;
        using var dialog = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "art-report.csv" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var args = new List<string> { "report", Workspace, dialog.FileName };
        foreach (string refs in new[] { "Assets/vanillaXml", "Assets/Scripts" }.Select(r => Path.Combine(Repository, r)).Where(Directory.Exists))
        {
            args.Add("--refs");
            args.Add(refs);
        }
        RunCommand([.. args], reload: false);
    }

    private void Repack()
    {
        if (!RequireWorkspace()) return;
        if (Ask("Repack changed groups and overwrite the game's bundles and catalog.json?\n\n" +
                "Removed groups have their .tar.lz4 deleted. Git can restore everything if needed.") != DialogResult.Yes)
            return;
        RunCommand("repack-all", Workspace);
    }

    // ------------------------------------------------------------------ asset actions

    private void MoveSelected()
    {
        List<AssetRow> rows = Selected();
        if (rows.Count == 0) { MessageBox.Show(this, "Select some assets first.", Text); return; }
        string? target = GroupPrompt.Ask(this, _groupOrder, rows.Count);
        if (target == null) return;
        if (!Regex.IsMatch(target, "^[A-Za-z0-9_-]+$"))
        {
            MessageBox.Show(this, "Group names may only use letters, digits, '_' and '-'.", Text);
            return;
        }
        var args = new List<string> { "move", Workspace, "@" + WriteAddressList(rows), target };
        if (CurrentGroup() is { } group) { args.Add("--from"); args.Add(group); }
        RunCommand([.. args]);
    }

    private void DeleteSelected()
    {
        List<AssetRow> rows = Selected();
        if (rows.Count == 0) return;
        int addresses = rows.Select(r => r.Address).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        string scope = CurrentGroup() == null ? "from every group" : "from " + CurrentGroup();
        if (Ask($"Delete {addresses} asset address(es) {scope}?\n(Sprites sharing an address are deleted together.)") != DialogResult.Yes)
            return;
        var args = new List<string> { "delete", Workspace, "@" + WriteAddressList(rows) };
        if (CurrentGroup() is { } group) { args.Add("--from"); args.Add(group); }
        RunCommand([.. args]);
    }

    private void SelectAllVisible()
    {
        _assets.BeginUpdate();
        for (int i = 0; i < _visible.Count; i++) _assets.SelectedIndices.Add(i);
        _assets.EndUpdate();
    }

    private void PlaySelected()
    {
        if (Selected().FirstOrDefault() is not { Type: "audio" } row || !File.Exists(row.PayloadPath)) return;
        try
        {
            _player?.Stop();
            _player = new SoundPlayer(row.PayloadPath);
            _player.Play();
        }
        catch (Exception exception)
        {
            AppendLog("Cannot play: " + exception.Message + Environment.NewLine);
        }
    }

    private static string WriteAddressList(IEnumerable<AssetRow> rows)
    {
        string path = Path.Combine(Path.GetTempPath(), "eclipse-assetpacker-selection.txt");
        File.WriteAllLines(path, rows.Select(r => r.Address).Distinct(StringComparer.OrdinalIgnoreCase));
        return path;
    }

    // ------------------------------------------------------------------ command runner

    private void RunCommand(params string[] args) => RunCommand(args, reload: true);

    private async void RunCommand(string[] args, bool reload)
    {
        if (args.Length > 1 && args[0] != "unpack-all" && !RequireWorkspace()) return;
        SetBusy(true, args[0] + "...");
        AppendLog("> " + string.Join(" ", args.Select(a => a.Contains(' ') ? "\"" + a + "\"" : a)) + Environment.NewLine);
        string previous = Environment.CurrentDirectory;
        int code;
        try
        {
            if (Directory.Exists(Repository)) Environment.CurrentDirectory = Repository;
            code = await Task.Run(() => Packer.Main(args));
        }
        finally
        {
            Environment.CurrentDirectory = previous;
        }
        AppendLog((code == 0 ? "Done." : "FAILED (see messages above).") + Environment.NewLine + Environment.NewLine);
        SetBusy(false, code == 0 ? args[0] + " finished" : args[0] + " failed");
        if (reload) ReloadWorkspace();
    }

    private void SetBusy(bool busy, string status)
    {
        foreach (Control control in _busyControls) control.Enabled = !busy;
        _assets.Enabled = !busy;
        UseWaitCursor = busy;
        _status.Text = status;
    }

    private bool RequireWorkspace()
    {
        if (HasWorkspace) return true;
        MessageBox.Show(this, "No workspace here yet. Click \"1. Unpack all bundles\" first.", Text);
        return false;
    }

    private DialogResult Ask(string message) =>
        MessageBox.Show(this, message, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

    // ------------------------------------------------------------------ workspace browsing

    private async void ReloadWorkspace()
    {
        string? selectedGroup = CurrentGroup();
        _groups.Items.Clear();
        _all = [];
        _visible = [];
        _assets.VirtualListSize = 0;
        ClearPreview();
        if (!HasWorkspace)
        {
            _status.Text = "No workspace yet - pick a folder and click \"1. Unpack all bundles\".";
            return;
        }
        string root = Workspace;
        _status.Text = "Loading workspace...";
        (List<string> order, List<AssetRow> rows) = await Task.Run(() => LoadRows(root));
        _groupOrder = order;
        _all = rows;
        var counts = rows.GroupBy(r => r.Group, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        _groups.Items.Add(new GroupItem(AllGroups, rows.Count));
        foreach (string name in order)
            _groups.Items.Add(new GroupItem(name, counts.GetValueOrDefault(name)));
        int index = selectedGroup == null ? 0 : Math.Max(0, order.FindIndex(n => n.Equals(selectedGroup, StringComparison.OrdinalIgnoreCase)) + 1);
        _groups.SelectedIndex = index;
        _status.Text = $"{rows.Count} assets in {order.Count} groups";
    }

    private static (List<string>, List<AssetRow>) LoadRows(string root)
    {
        var order = new List<string>();
        var unpacked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (JsonNode.Parse(File.ReadAllText(Path.Combine(root, "workspace.json")))?["groups"] is JsonArray groups)
            foreach (JsonObject group in groups.OfType<JsonObject>())
            {
                string name = (string?)group["name"] ?? string.Empty;
                if ((bool?)group["unpacked"] == true && Directory.Exists(Path.Combine(root, name)))
                    order.Add(name);
                if ((bool?)group["unpacked"] == true) unpacked.Add(name);
            }
        foreach (string directory in Directory.GetDirectories(root).Select(Path.GetFileName).OfType<string>()
                     .Where(n => !n.StartsWith('.') && !unpacked.Contains(n)).OrderBy(n => n, StringComparer.Ordinal))
            order.Add(directory);

        var rows = new List<AssetRow>();
        foreach (string group in order)
        {
            string directory = Path.Combine(root, group);
            foreach (string meta in Directory.EnumerateFiles(directory, "*.meta", SearchOption.AllDirectories))
            {
                var values = ParseDescriptor(meta);
                string type = values.GetValueOrDefault("type", "?").ToLowerInvariant();
                if (type is "sound" or "music") type = "audio";
                string payload = values.GetValueOrDefault(type == "sprite" ? "texture" : "file", string.Empty);
                string payloadPath = Path.Combine(directory, payload.Replace('/', Path.DirectorySeparatorChar));
                long size = File.Exists(payloadPath) ? new FileInfo(payloadPath).Length : -1;
                rows.Add(new AssetRow(group, type, values.GetValueOrDefault("address", "?"),
                    values.GetValueOrDefault("name", string.Empty), meta, payloadPath, size,
                    values.GetValueOrDefault("rect", string.Empty)));
            }
        }
        return (order, rows);
    }

    private static Dictionary<string, string> ParseDescriptor(string path)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string raw in File.ReadLines(path))
        {
            string line = raw.Trim();
            int equals = line.IndexOf('=');
            if (line.Length == 0 || line.StartsWith('#') || equals <= 0) continue;
            values.TryAdd(line[..equals].Trim(), line[(equals + 1)..].Trim());
        }
        return values;
    }

    private string? CurrentGroup() =>
        _groups.SelectedItem is GroupItem item && item.Name != AllGroups ? item.Name : null;

    private void ApplyFilter()
    {
        string? group = CurrentGroup();
        string text = _filter.Text.Trim();
        Func<AssetRow, bool> match = _ => true;
        if (text.Contains('*') || text.Contains('?'))
        {
            string pattern = "^" + Regex.Escape(text.Replace('\\', '/')).Replace(@"\*\*", ".*").Replace(@"\*", "[^/]*").Replace(@"\?", "[^/]") + "$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            match = r => regex.IsMatch(r.Address);
        }
        else if (text.Length > 0)
        {
            match = r => r.Address.Contains(text, StringComparison.OrdinalIgnoreCase) || r.Name.Contains(text, StringComparison.OrdinalIgnoreCase);
        }
        _visible = _all.Where(r => (group == null || r.Group.Equals(group, StringComparison.OrdinalIgnoreCase)) && match(r)).ToList();
        SortRows();
        _assets.SelectedIndices.Clear();
        _assets.VirtualListSize = _visible.Count;
        _assets.Invalidate();
        _status.Text = $"{_visible.Count} shown of {_all.Count} assets";
    }

    private void SortBy(int column)
    {
        _sortDescending = column == _sortColumn && !_sortDescending;
        _sortColumn = column;
        SortRows();
        _assets.SelectedIndices.Clear();
        _assets.Invalidate();
    }

    private void SortRows()
    {
        Comparison<AssetRow> compare = _sortColumn switch
        {
            1 => (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase),
            2 => (a, b) => string.Compare(a.Type, b.Type, StringComparison.Ordinal),
            3 => (a, b) => _groupOrder.FindIndex(n => n == a.Group).CompareTo(_groupOrder.FindIndex(n => n == b.Group)),
            4 => (a, b) => a.Size.CompareTo(b.Size),
            _ => (a, b) => string.Compare(a.Address, b.Address, StringComparison.OrdinalIgnoreCase),
        };
        _visible.Sort((a, b) =>
        {
            int result = compare(a, b);
            if (result == 0) result = string.Compare(a.Address, b.Address, StringComparison.OrdinalIgnoreCase);
            return _sortDescending ? -result : result;
        });
    }

    private static ListViewItem ToItem(AssetRow row) => new([
        row.Address, row.Name, row.Type, row.Group, row.Size < 0 ? "missing" : FormatSize(row.Size),
    ]);

    private static string FormatSize(long bytes) =>
        bytes >= 1 << 20 ? $"{bytes / 1048576.0:0.0} MB" : $"{Math.Max(1, bytes / 1024)} KB";

    private List<AssetRow> Selected() =>
        _assets.SelectedIndices.Cast<int>().Where(i => i < _visible.Count).Select(i => _visible[i]).ToList();

    // ------------------------------------------------------------------ preview

    private void ShowSelection()
    {
        List<AssetRow> rows = Selected();
        if (rows.Count != 1)
        {
            ClearPreview();
            if (rows.Count > 1) _details.Text = rows.Count + " assets selected";
            return;
        }
        AssetRow row = rows[0];
        var details = new StringBuilder();
        details.AppendLine("Group:   " + row.Group);
        details.AppendLine("File:    " + row.PayloadPath);
        details.AppendLine();
        try { details.Append(File.ReadAllText(row.DescriptorPath)); }
        catch (IOException exception) { details.Append(exception.Message); }
        if (row.Type is "model" or "atlas" && File.Exists(row.PayloadPath))
        {
            string text = File.ReadAllText(row.PayloadPath);
            details.AppendLine().AppendLine("----").Append(text.Length > 20000 ? text[..20000] + "\n..." : text);
        }
        _details.Text = details.ToString().Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        SetPreview(row.Type == "sprite" ? LoadSpritePreview(row) : null);
    }

    // Whole texture with the sprite's rect outlined (atlases share one texture between many sprites).
    private static Image? LoadSpritePreview(AssetRow row)
    {
        if (!File.Exists(row.PayloadPath)) return null;
        try
        {
            using var stream = new MemoryStream(File.ReadAllBytes(row.PayloadPath));
            using var source = Image.FromStream(stream);
            var bitmap = new Bitmap(source);
            float[] rect = row.Rect.Split(',').Select(v => float.TryParse(v, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float f) ? f : 0).ToArray();
            if (rect.Length == 4 && (rect[2] < bitmap.Width - 1 || rect[3] < bitmap.Height - 1))
            {
                using Graphics graphics = Graphics.FromImage(bitmap);
                using var pen = new Pen(Color.Red, Math.Max(2, bitmap.Width / 300f));
                // Unity rects start at the bottom-left corner.
                graphics.DrawRectangle(pen, rect[0], bitmap.Height - rect[1] - rect[3], rect[2], rect[3]);
            }
            return bitmap;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void SetPreview(Image? image)
    {
        Image? old = _preview.Image;
        _preview.Image = image;
        old?.Dispose();
    }

    private void ClearPreview()
    {
        SetPreview(null);
        _details.Text = string.Empty;
    }

    // ------------------------------------------------------------------ misc

    private static void OpenInExplorer(string path, bool select)
    {
        if (select && File.Exists(path))
            Process.Start("explorer.exe", "/select,\"" + path + "\"");
        else if (Directory.Exists(path))
            Process.Start("explorer.exe", "\"" + path + "\"");
    }

    private static string? FindRepository()
    {
        foreach (string start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
            for (DirectoryInfo? directory = new(start); directory != null; directory = directory.Parent)
                if (File.Exists(Path.Combine(directory.FullName, CatalogRelative)))
                    return directory.FullName;
        return null;
    }

    private static string DefaultWorkspace(string repository) =>
        repository.Length == 0 ? string.Empty
            : Path.Combine(Path.GetDirectoryName(repository.TrimEnd('\\', '/')) ?? repository, "EclipseArtWorkspace");

    internal void AppendLog(string text)
    {
        if (InvokeRequired) { BeginInvoke(() => AppendLog(text)); return; }
        _log.AppendText(text.Replace("\r\n", "\n").Replace("\n", Environment.NewLine));
    }

    private sealed record AssetRow(string Group, string Type, string Address, string Name,
        string DescriptorPath, string PayloadPath, long Size, string Rect);

    private sealed record GroupItem(string Name, int Count)
    {
        public override string ToString() => $"{Name}  ({Count})";
    }

    private sealed class LogWriter(MainForm form) : TextWriter
    {
        private readonly StringBuilder _line = new();
        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            lock (_line)
            {
                _line.Append(value);
                if (value != '\n') return;
                form.AppendLog(_line.ToString());
                _line.Clear();
            }
        }

        public override void Write(string? value)
        {
            if (value == null) return;
            foreach (char c in value) Write(c);
        }
    }

    private static class Settings
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EclipseAssetPacker", "workspace.txt");

        public static string? Load()
        {
            try { return File.Exists(FilePath) ? File.ReadAllText(FilePath).Trim() : null; }
            catch (IOException) { return null; }
        }

        public static void Save(string workspace)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, workspace);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}

internal static class GroupPrompt
{
    public static string? Ask(IWin32Window owner, IEnumerable<string> groups, int count)
    {
        using var form = new Form
        {
            Text = "Move to group", Width = 420, Height = 170, FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent, MinimizeBox = false, MaximizeBox = false,
        };
        var label = new Label { Text = $"Move {count} selected asset(s) to which group?\nType a new name to create a group.", Left = 12, Top = 10, Width = 380, Height = 36 };
        var combo = new ComboBox { Left = 12, Top = 50, Width = 380, DropDownStyle = ComboBoxStyle.DropDown };
        combo.Items.AddRange(groups.Cast<object>().ToArray());
        var ok = new Button { Text = "Move", Left = 216, Top = 86, Width = 85, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancel", Left = 307, Top = 86, Width = 85, DialogResult = DialogResult.Cancel };
        form.Controls.AddRange([label, combo, ok, cancel]);
        form.AcceptButton = ok;
        form.CancelButton = cancel;
        return form.ShowDialog(owner) == DialogResult.OK && combo.Text.Trim().Length > 0 ? combo.Text.Trim().ToUpperInvariant() : null;
    }
}
