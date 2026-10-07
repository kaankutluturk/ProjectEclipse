using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Xml;

namespace Eclipse.LocationParamsEditor;

/// <summary>
/// Visual editor for location layouts. Left: layers and their pictures. Centre: the canvas.
/// Right: the selected element's attributes (every attribute, including ones this editor
/// does not know) and the pictures in the images folder.
/// </summary>
public sealed class MainForm : Form
{
    private ParamsDocument? _document;
    private readonly ImageLibrary _images = new();
    private readonly CanvasView _canvas = new() { Dock = DockStyle.Fill };
    private readonly TreeView _tree = new() { Dock = DockStyle.Fill, HideSelection = false, CheckBoxes = true, FullRowSelect = true };
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.CellSelect,
    };
    private readonly Label _gridTitle = new() { Dock = DockStyle.Top, Height = 22, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
    private readonly ListView _library = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false };
    private readonly TextBox _warnings = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9f) };
    private readonly ToolStripStatusLabel _status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripStatusLabel _pointer = new() { AutoSize = true };
    private readonly TrackBar _camera = new() { Minimum = -1000, Maximum = 1000, TickFrequency = 250, Width = 260, Value = 0 };
    private readonly NumericUpDown _artScale = new() { DecimalPlaces = 3, Increment = 0.05m, Minimum = 0.01m, Maximum = 10m, Value = 0.5m, Width = 70 };
    private readonly ToolStripButton _viewport = new("Game viewport") { CheckOnClick = true, ToolTipText = "Show the location through the game's fight camera at the chosen screen shape" };
    private readonly ToolStripComboBox _aspect = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
    private readonly NumericUpDown _distance = new() { Minimum = 0, Maximum = 5000, Increment = 25, Value = 0, Width = 70 };
    private static readonly (string name, float value)[] Aspects = { ("21:9", 21f / 9f), ("16:9", 16f / 9f), ("18:9", 2f), ("32:9", 32f / 9f), ("16:10", 1.6f), ("4:3", 4f / 3f) };
    private bool _updatingGrid, _updatingTree;
    private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EclipseParamsEditor", "settings.json");
    private Settings _settings = Settings.Load(SettingsPath);

    private sealed class Settings
    {
        public Dictionary<string, string> ImageFolders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public List<string> ExtraFolders { get; set; } = new();
        public string? LastFile { get; set; }
        public static Settings Load(string path)
        {
            try { return File.Exists(path) ? JsonSerializer.Deserialize<Settings>(File.ReadAllText(path)) ?? new Settings() : new Settings(); }
            catch (Exception) { return new Settings(); }
        }
        public void Save(string path)
        {
            try { Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true })); }
            catch (Exception) { }
        }
    }

    public MainForm(string? openPath)
    {
        Text = "Location Params Editor";
        Width = 1600; Height = 950;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;
        AllowDrop = true;
        _images.ExtraFolders.AddRange(_settings.ExtraFolders);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Attribute", FillWeight = 40 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Value", FillWeight = 60 });
        _grid.CellEndEdit += (_, e) => CommitGridRow(e.RowIndex);
        _library.Columns.Add("Picture", 190);
        _library.Columns.Add("Size", 80);
        _library.Columns.Add("Used", 50);
        _library.DoubleClick += (_, _) => AddPictureFromLibrary();

        _canvas.Changing += () => _document?.Checkpoint();
        _canvas.Changed += () => { RefreshGrid(); RefreshWarnings(); UpdateTitle(); };
        _canvas.SelectionChanged += element => SelectElement(element, fromCanvas: true);
        _canvas.PointerMoved += p => _pointer.Text = $"x {p.X.ToString("0.#", CultureInfo.InvariantCulture)}   y {p.Y.ToString("0.#", CultureInfo.InvariantCulture)}";
        _tree.AfterSelect += (_, e) => { if (!_updatingTree) SelectElement(e.Node?.Tag as XmlElement, fromCanvas: false); };
        _tree.AfterCheck += (_, e) =>
        {
            if (_updatingTree || e.Node?.Tag is not XmlElement layer || layer.Name != "Layer") return;
            if (e.Node.Checked) _canvas.Options.HiddenLayers.Remove(layer); else _canvas.Options.HiddenLayers.Add(layer);
            _canvas.Invalidate();
        };
        _camera.ValueChanged += (_, _) => { _canvas.Options.CameraX = _camera.Value; ResetFighters(); _canvas.Invalidate(); };
        _canvas.FightersMoved += () => ShowCameraStatus();

        Controls.Add(_canvas);
        Controls.Add(BuildRightPanel());
        Controls.Add(BuildLeftPanel());
        Controls.Add(BuildToolbar());
        Controls.Add(BuildMenu());
        var statusStrip = new StatusStrip();
        statusStrip.Items.Add(_status);
        statusStrip.Items.Add(_pointer);
        Controls.Add(statusStrip);

        DragEnter += (_, e) => { if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true) e.Effect = DragDropEffects.Copy; };
        DragDrop += (_, e) => { if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0) OpenFile(files[0]); };
        FormClosing += (_, e) => { if (!ConfirmDiscard()) e.Cancel = true; else _settings.Save(SettingsPath); };
        Shown += (_, _) =>
        {
            if (openPath != null) OpenFile(openPath);
            else SetStatus("Open a params.xml or params.txt (File > Open), then choose the folder with its pictures.");
        };
    }

    // ---- Layout ----

    private MenuStrip BuildMenu()
    {
        var menu = new MenuStrip();
        var file = new ToolStripMenuItem("&File");
        file.DropDownItems.Add("&New layout", null, (_, _) => NewFile());
        file.DropDownItems.Add("&Open...\tCtrl+O", null, (_, _) => OpenDialog());
        file.DropDownItems.Add("&Save\tCtrl+S", null, (_, _) => Save(false));
        file.DropDownItems.Add("Save &as...", null, (_, _) => Save(true));
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add("Choose &images folder...", null, (_, _) => ChooseImagesFolder());
        file.DropDownItems.Add("Add e&xtra search folder...", null, (_, _) => AddExtraFolder());
        file.DropDownItems.Add("Clear extra search folders", null, (_, _) => { _images.ExtraFolders.Clear(); _settings.ExtraFolders.Clear(); _images.Clear(); RefreshAll(); });
        file.DropDownItems.Add("&Export preview PNG...", null, (_, _) => ExportPreview());
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add("E&xit", null, (_, _) => Close());
        var edit = new ToolStripMenuItem("&Edit");
        edit.DropDownItems.Add("&Undo\tCtrl+Z", null, (_, _) => Undo(true));
        edit.DropDownItems.Add("&Redo\tCtrl+Y", null, (_, _) => Undo(false));
        edit.DropDownItems.Add(new ToolStripSeparator());
        edit.DropDownItems.Add("&Duplicate\tCtrl+D", null, (_, _) => Duplicate());
        edit.DropDownItems.Add("De&lete\tDel", null, (_, _) => DeleteSelected());
        edit.DropDownItems.Add("Move &forward (later in draw order)\tPgUp", null, (_, _) => MoveSelected(1));
        edit.DropDownItems.Add("Move &backward\tPgDn", null, (_, _) => MoveSelected(-1));
        edit.DropDownItems.Add(new ToolStripSeparator());
        edit.DropDownItems.Add("Add &layer", null, (_, _) => AddLayer());
        edit.DropDownItems.Add("Add &picture from list", null, (_, _) => AddPictureFromLibrary());
        edit.DropDownItems.Add(new ToolStripSeparator());
        edit.DropDownItems.Add("Size box to &picture (keep height)", null, (_, _) => FitBox(keepHeight: true));
        edit.DropDownItems.Add("Size box to picture × art &scale", null, (_, _) => FitBox(keepHeight: false));
        var view = new ToolStripMenuItem("&View");
        view.DropDownItems.Add("&Fit all\tCtrl+0", null, (_, _) => _canvas.FitAll());
        view.DropDownItems.Add("Frame &selection\tF", null, (_, _) => _canvas.FrameSelection());
        view.DropDownItems.Add(Toggle("Location bounds", () => _canvas.Options.Bounds, v => _canvas.Options.Bounds = v));
        view.DropDownItems.Add(Toggle("Floor and walls", () => _canvas.Options.FloorAndWalls, v => _canvas.Options.FloorAndWalls = v));
        view.DropDownItems.Add(Toggle("Fighter start points", () => _canvas.Options.Fighters, v => _canvas.Options.Fighters = v));
        view.DropDownItems.Add(Toggle("Missing-picture and effect placeholders", () => _canvas.Options.Placeholders, v => _canvas.Options.Placeholders = v));
        view.DropDownItems.Add(Toggle("Picture names", () => _canvas.Options.Labels, v => _canvas.Options.Labels = v));
        view.DropDownItems.Add(Toggle("Lock fill pixels (click through pixel_*)", () => _canvas.LockFills, v => _canvas.LockFills = v));
        view.DropDownItems.Add(Toggle("Solo the selected layer", () => _canvas.Options.SoloLayer != null, v =>
            _canvas.Options.SoloLayer = v && _canvas.Selected != null ? (_canvas.Selected.Name == "Layer" ? _canvas.Selected : _document?.Layer(_canvas.Selected)) : null));
        var help = new ToolStripMenuItem("&Help");
        help.DropDownItems.Add("How it works", null, (_, _) => MessageBox.Show(this, HelpText, "Location Params Editor", MessageBoxButtons.OK, MessageBoxIcon.Information));
        menu.Items.AddRange(new ToolStripItem[] { file, edit, view, help });
        MainMenuStrip = menu;
        return menu;
    }

    private ToolStripMenuItem Toggle(string text, Func<bool> get, Action<bool> set)
    {
        var item = new ToolStripMenuItem(text) { Checked = get(), CheckOnClick = true };
        item.CheckedChanged += (_, _) => { set(item.Checked); _canvas.Invalidate(); };
        return item;
    }

    private ToolStrip BuildToolbar()
    {
        var bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
        bar.Items.Add(new ToolStripButton("Open", null, (_, _) => OpenDialog()));
        bar.Items.Add(new ToolStripButton("Save", null, (_, _) => Save(false)));
        bar.Items.Add(new ToolStripButton("Images folder", null, (_, _) => ChooseImagesFolder()));
        bar.Items.Add(new ToolStripSeparator());
        bar.Items.Add(new ToolStripButton("Undo", null, (_, _) => Undo(true)));
        bar.Items.Add(new ToolStripButton("Redo", null, (_, _) => Undo(false)));
        bar.Items.Add(new ToolStripSeparator());
        bar.Items.Add(new ToolStripButton("Add layer", null, (_, _) => AddLayer()));
        bar.Items.Add(new ToolStripButton("Duplicate", null, (_, _) => Duplicate()));
        bar.Items.Add(new ToolStripButton("Delete", null, (_, _) => DeleteSelected()));
        bar.Items.Add(new ToolStripButton("Forward", null, (_, _) => MoveSelected(1)));
        bar.Items.Add(new ToolStripButton("Backward", null, (_, _) => MoveSelected(-1)));
        bar.Items.Add(new ToolStripButton("Fit box to picture", null, (_, _) => FitBox(keepHeight: true)) { ToolTipText = "Width from the picture's proportions, keeping Height" });
        bar.Items.Add(new ToolStripSeparator());
        bar.Items.Add(new ToolStripLabel("Art scale"));
        bar.Items.Add(new ToolStripControlHost(_artScale) { ToolTipText = "Layout units per picture pixel for new pictures (0.5 for 2x art)" });
        bar.Items.Add(new ToolStripSeparator());
        bar.Items.Add(_viewport);
        bar.Items.Add(_aspect);
        bar.Items.Add(new ToolStripLabel("Fighter distance"));
        bar.Items.Add(new ToolStripControlHost(_distance) { ToolTipText = "Distance between the fighters, which sets the game's auto zoom. 0: their start positions." });
        bar.Items.Add(new ToolStripLabel("Camera"));
        bar.Items.Add(new ToolStripControlHost(_camera) { ToolTipText = "Editor view: parallax preview. Game viewport: where the fight is, from the middle of the location." });
        bar.Items.Add(new ToolStripButton("Centre", null, (_, _) => _camera.Value = 0));
        bar.Items.Add(new ToolStripButton("Fit all", null, (_, _) => _canvas.FitAll()));
        foreach (var (name, _) in Aspects) _aspect.Items.Add(name);
        _aspect.SelectedIndex = 0;
        _viewport.CheckedChanged += (_, _) => ApplyViewport(fit: true);
        _aspect.SelectedIndexChanged += (_, _) => ApplyViewport(fit: _viewport.Checked);
        _distance.ValueChanged += (_, _) => ApplyViewport(fit: false);
        return bar;
    }

    private Control BuildLeftPanel()
    {
        var panel = new Panel { Dock = DockStyle.Left, Width = 300 };
        var header = new Label { Dock = DockStyle.Top, Height = 22, Text = "  Layers (tick = visible; top is drawn first)", TextAlign = ContentAlignment.MiddleLeft };
        panel.Controls.Add(_tree);
        panel.Controls.Add(header);
        return panel;
    }

    private Control BuildRightPanel()
    {
        var split = new SplitContainer { Dock = DockStyle.Right, Width = 380, Orientation = Orientation.Horizontal, SplitterDistance = 330 };
        var attributes = new Panel { Dock = DockStyle.Fill };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 30 };
        var add = new Button { Text = "Add attribute", AutoSize = true };
        add.Click += (_, _) => AddAttribute();
        var remove = new Button { Text = "Remove attribute", AutoSize = true };
        remove.Click += (_, _) => RemoveAttribute();
        buttons.Controls.Add(add); buttons.Controls.Add(remove);
        attributes.Controls.Add(_grid);
        attributes.Controls.Add(buttons);
        attributes.Controls.Add(_gridTitle);
        split.Panel1.Controls.Add(attributes);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var pictures = new TabPage("Pictures in folder");
        pictures.Controls.Add(_library);
        pictures.Controls.Add(new Label { Dock = DockStyle.Top, Height = 32, Text = "Double-click to place a picture in the selected layer at the view centre.", TextAlign = ContentAlignment.MiddleLeft });
        var warnings = new TabPage("Checks");
        warnings.Controls.Add(_warnings);
        tabs.TabPages.Add(pictures);
        tabs.TabPages.Add(warnings);
        split.Panel2.Controls.Add(tabs);
        return split;
    }

    private void ApplyViewport(bool fit)
    {
        var options = _canvas.Options;
        options.Viewport = _viewport.Checked;
        options.Aspect = Aspects[Math.Max(0, _aspect.SelectedIndex)].value;
        options.FighterDistance = _distance.Value > 0 ? (float)_distance.Value : null;
        ResetFighters();
        if (fit) _canvas.FitAll(); else _canvas.Invalidate();
        if (options.Viewport) _canvas.Focus();
        ShowCameraStatus();
    }

    /// <summary>Distance or camera changes put the fighters back where those controls say.</summary>
    private void ResetFighters() { _canvas.Options.Player1X = null; _canvas.Options.Player2X = null; }

    private void ShowCameraStatus()
    {
        var options = _canvas.Options;
        if (!options.Viewport || _document == null) return;
        SceneRenderer.UpdateCamera(_document, options);
        var camera = options.Camera;
        SetStatus($"Game viewport {SceneRenderer.AspectName(options.Aspect)}: zoom {camera.Zoom:0.###}, the screen shows {camera.VisibleWidth / camera.Zoom:0} × {camera.Height / camera.Zoom:0} game-layer units" +
            $", fighters {camera.Distance:0} apart" +
            (camera.MaxWidthLimited ? $" (zoom capped by MaxWidth {options.MaxWidth:0}: the camera keeps player 1 in view)" : "") +
            ".   Drive the fighters: A / D and Left / Right (Shift runs); click the canvas first.");
    }

    // ---- Files ----

    private void NewFile()
    {
        if (!ConfirmDiscard()) return;
        _document = ParamsDocument.New();
        AttachDocument();
        SetStatus("New layout. Choose an images folder, then add pictures from the list.");
    }

    private void OpenDialog()
    {
        using var dialog = new OpenFileDialog { Filter = "Location params (*.xml;*.txt)|*.xml;*.txt|All files|*.*", Title = "Open a location params file" };
        if (_settings.LastFile != null) dialog.InitialDirectory = Path.GetDirectoryName(_settings.LastFile);
        if (dialog.ShowDialog(this) == DialogResult.OK) OpenFile(dialog.FileName);
    }

    private void OpenFile(string path)
    {
        if (!ConfirmDiscard()) return;
        try { _document = ParamsDocument.Load(path); }
        catch (Exception ex) { MessageBox.Show(this, "Could not open " + path + ":\n" + ex.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        _settings.LastFile = path;
        _canvas.Options.HiddenLayers.Clear();
        _canvas.Options.SoloLayer = null;
        _images.SetFolder(_settings.ImageFolders.TryGetValue(path, out var folder) && Directory.Exists(folder) ? folder : GuessFolder(path));
        AttachDocument();
        if (_images.Folder == null) ChooseImagesFolder();
        SetStatus("Opened " + path + (_images.Folder != null ? "   ·   pictures from " + _images.Folder : ""));
    }

    /// <summary>A folder next to the file that holds most of the pictures it names.</summary>
    private string? GuessFolder(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (directory == null || _document == null) return null;
        var names = _document.Layers.SelectMany(ParamsDocument.Items).Select(e => e.GetAttribute("ClassName")).Where(n => n.Length > 0).ToHashSet(StringComparer.OrdinalIgnoreCase);
        int Score(string folder) => Directory.Exists(folder) ? Directory.EnumerateFiles(folder).Count(f => names.Contains(Path.GetFileNameWithoutExtension(f))) : 0;
        var candidates = new List<string> { directory };
        candidates.AddRange(Directory.Exists(directory) ? Directory.GetDirectories(directory) : Array.Empty<string>());
        var best = candidates.OrderByDescending(Score).FirstOrDefault();
        return best != null && Score(best) > 0 ? best : null;
    }

    private void ChooseImagesFolder()
    {
        using var dialog = new FolderBrowserDialog { Description = "Folder with the pictures this layout names (ClassName + .png)", UseDescriptionForTitle = true };
        if (_images.Folder != null) dialog.InitialDirectory = _images.Folder;
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _images.SetFolder(dialog.SelectedPath);
        if (_document?.Path != null) _settings.ImageFolders[_document.Path] = dialog.SelectedPath;
        _settings.Save(SettingsPath);
        RefreshAll();
        SetStatus("Pictures from " + dialog.SelectedPath);
    }

    private void AddExtraFolder()
    {
        using var dialog = new FolderBrowserDialog { Description = "Extra folder searched for pictures (e.g. the folder holding other locations for Path= layers)", UseDescriptionForTitle = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _images.ExtraFolders.Add(dialog.SelectedPath);
        _settings.ExtraFolders = _images.ExtraFolders.ToList();
        _settings.Save(SettingsPath);
        _images.Clear();
        RefreshAll();
    }

    private void Save(bool saveAs)
    {
        if (_document == null) return;
        string? path = _document.Path;
        if (saveAs || path == null)
        {
            using var dialog = new SaveFileDialog { Filter = "Location params (*.xml)|*.xml|Fallback params (params.txt)|*.txt|All files|*.*", FileName = path != null ? Path.GetFileName(path) : "params.xml" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            path = dialog.FileName;
        }
        try { _document.Save(path); }
        catch (Exception ex) { MessageBox.Show(this, "Not saved: " + ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (_images.Folder != null) _settings.ImageFolders[path] = _images.Folder;
        _settings.LastFile = path;
        _settings.Save(SettingsPath);
        UpdateTitle();
        SetStatus("Saved " + path);
    }

    private void ExportPreview()
    {
        if (_document == null) return;
        using var dialog = new SaveFileDialog { Filter = "PNG image|*.png", FileName = "preview.png" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        SceneRenderer.RenderToFile(_document, _images, _canvas.Options, dialog.FileName, 1f);
        SetStatus("Exported " + dialog.FileName);
    }

    private bool ConfirmDiscard()
    {
        if (_document == null || !_document.Dirty) return true;
        var answer = MessageBox.Show(this, "Save changes to " + (_document.Path ?? "the new layout") + "?", "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (answer == DialogResult.Cancel) return false;
        if (answer == DialogResult.Yes) Save(false);
        return answer == DialogResult.No || !_document.Dirty;
    }

    private void AttachDocument()
    {
        _canvas.Document = _document;
        _canvas.Images = _images;
        _canvas.Selected = null;
        float width = _document?.RootNumber("Width") ?? 1000;
        _camera.Minimum = -(int)Math.Max(100, width / 2);
        _camera.Maximum = (int)Math.Max(100, width / 2);
        _camera.Value = 0;
        RefreshAll();
        _canvas.FitAll();
    }

    // ---- Selection, tree, grid ----

    private void RefreshAll()
    {
        RefreshTree();
        RefreshGrid();
        RefreshLibrary();
        RefreshWarnings();
        UpdateTitle();
        _canvas.Invalidate();
    }

    private void UpdateTitle() =>
        Text = "Location Params Editor — " + (_document?.Path ?? "new layout") + (_document?.Dirty == true ? " *" : "");

    private void SetStatus(string text) => _status.Text = text;

    private void RefreshTree()
    {
        _updatingTree = true;
        _tree.BeginUpdate();
        _tree.Nodes.Clear();
        if (_document != null)
        {
            var root = new TreeNode("Location (Root)") { Tag = _document.Root, Checked = true };
            _tree.Nodes.Add(root);
            int index = 0;
            foreach (XmlElement layer in _document.Layers)
            {
                string type = layer.GetAttribute("Type") == "2" ? "fighters" : "layer";
                string extra = (layer.GetAttribute("Path").Length > 0 ? "  path " + layer.GetAttribute("Path") : "") + (layer.GetAttribute("Scaling") == "1" ? "  scaled" : "");
                var node = new TreeNode($"{index++}. {type}  factor {layer.GetAttribute("Factor")}{extra}") { Tag = layer, Checked = !_canvas.Options.HiddenLayers.Contains(layer) };
                foreach (XmlElement item in ParamsDocument.Items(layer))
                {
                    string label = item.Name == "ModelsViewer" ? "fighter start positions"
                        : item.Name == "SimpleEffect" && item.GetAttribute("Type") != "Picture" ? "animation " + item.GetAttribute("ClassName")
                        : item.Name.Contains("Particle") ? "particles"
                        : item.GetAttribute("ClassName");
                    bool missing = ParamsDocument.IsPicture(item) && _images.Find(layer, item.GetAttribute("ClassName")) == null;
                    node.Nodes.Add(new TreeNode(label + (missing ? "  (missing)" : "")) { Tag = item, ForeColor = missing ? Color.Firebrick : SystemColors.WindowText });
                }
                root.Nodes.Add(node);
            }
            root.Expand();
            foreach (TreeNode node in root.Nodes) node.Expand();
            var selected = Find(_tree.Nodes, _canvas.Selected);
            if (selected != null) _tree.SelectedNode = selected;
        }
        _tree.EndUpdate();
        _updatingTree = false;
    }

    private static TreeNode? Find(TreeNodeCollection nodes, XmlElement? element)
    {
        if (element == null) return null;
        foreach (TreeNode node in nodes)
        {
            if (node.Tag == element) return node;
            var inner = Find(node.Nodes, element);
            if (inner != null) return inner;
        }
        return null;
    }

    private void SelectElement(XmlElement? element, bool fromCanvas)
    {
        _canvas.Selected = element;
        if (fromCanvas)
        {
            _updatingTree = true;
            var node = Find(_tree.Nodes, element);
            if (node != null) { _tree.SelectedNode = node; node.EnsureVisible(); }
            _updatingTree = false;
        }
        RefreshGrid();
        _canvas.Invalidate();
    }

    private void RefreshGrid()
    {
        _updatingGrid = true;
        _grid.Rows.Clear();
        var element = _canvas.Selected;
        _gridTitle.Text = element == null ? "  Nothing selected" : "  <" + element.Name + ">";
        if (element != null)
        {
            foreach (XmlAttribute attribute in element.Attributes) _grid.Rows.Add(attribute.Name, attribute.Value);
            if (ParamsDocument.IsPicture(element) && _document?.Layer(element) is XmlElement layer && _images.Load(layer, element.GetAttribute("ClassName")) is Bitmap bitmap)
                _gridTitle.Text += $"   picture {bitmap.Width}×{bitmap.Height}";
        }
        _updatingGrid = false;
    }

    private void CommitGridRow(int row)
    {
        if (_updatingGrid || _canvas.Selected == null || _document == null || row < 0) return;
        string name = Convert.ToString(_grid.Rows[row].Cells[0].Value)?.Trim() ?? "";
        string value = Convert.ToString(_grid.Rows[row].Cells[1].Value) ?? "";
        if (name.Length == 0) return;
        try { XmlConvert.VerifyName(name); }
        catch (XmlException) { SetStatus("'" + name + "' is not a valid attribute name."); RefreshGrid(); return; }
        // Rows keep their original name until saved; a renamed row replaces its attribute.
        var attributes = _canvas.Selected.Attributes.Cast<XmlAttribute>().ToList();
        string? oldName = row < attributes.Count ? attributes[row].Name : null;
        if (oldName == name && _canvas.Selected.GetAttribute(name) == value) return;
        _document.Checkpoint();
        if (oldName != null && oldName != name) _canvas.Selected.RemoveAttribute(oldName);
        _canvas.Selected.SetAttribute(name, value);
        AfterEdit(structure: name is "ClassName" or "Type" or "Factor" or "Path" or "Scaling");
    }

    private void AddAttribute()
    {
        if (_canvas.Selected == null || _document == null) return;
        string name = Prompt("Attribute name (e.g. FlipX, Color, IsOpaque)", "");
        if (string.IsNullOrWhiteSpace(name)) return;
        try { XmlConvert.VerifyName(name.Trim()); } catch (XmlException) { SetStatus("Not a valid attribute name."); return; }
        _document.Checkpoint();
        _canvas.Selected.SetAttribute(name.Trim(), "");
        AfterEdit(structure: false);
    }

    private void RemoveAttribute()
    {
        if (_canvas.Selected == null || _document == null || _grid.CurrentCell == null) return;
        string name = Convert.ToString(_grid.Rows[_grid.CurrentCell.RowIndex].Cells[0].Value) ?? "";
        if (name.Length == 0 || !_canvas.Selected.HasAttribute(name)) return;
        _document.Checkpoint();
        _canvas.Selected.RemoveAttribute(name);
        AfterEdit(structure: false);
    }

    private void AfterEdit(bool structure)
    {
        if (structure) RefreshTree(); else UpdateTreeLabels();
        RefreshGrid();
        RefreshWarnings();
        RefreshLibrary();
        UpdateTitle();
        _canvas.Invalidate();
    }

    private void UpdateTreeLabels() => RefreshTree();

    // ---- Edits ----

    private void Undo(bool undo)
    {
        if (_document == null) return;
        var path = PathOf(_canvas.Selected);
        if (!(undo ? _document.Undo() : _document.Redo())) { SetStatus(undo ? "Nothing to undo." : "Nothing to redo."); return; }
        _canvas.Options.HiddenLayers.Clear();
        _canvas.Options.SoloLayer = null;
        _canvas.Selected = ElementAt(path);
        RefreshAll();
    }

    /// <summary>Index path of an element under Root, to find it again in a restored document.</summary>
    private List<int>? PathOf(XmlElement? element)
    {
        if (element == null || _document == null) return null;
        var path = new List<int>();
        for (XmlNode? node = element; node != null && node != _document.Root; node = node.ParentNode)
        {
            if (node.ParentNode == null) return null;
            path.Insert(0, node.ParentNode.ChildNodes.OfType<XmlElement>().ToList().IndexOf((XmlElement)node));
        }
        return path;
    }

    private XmlElement? ElementAt(List<int>? path)
    {
        if (path == null || _document == null) return null;
        XmlElement current = _document.Root;
        foreach (int index in path)
        {
            var children = current.ChildNodes.OfType<XmlElement>().ToList();
            if (index < 0 || index >= children.Count) return null;
            current = children[index];
        }
        return current;
    }

    private XmlElement? TargetLayer() =>
        _canvas.Selected == null ? _document?.Layers.LastOrDefault(l => l.GetAttribute("Type") != "2")
        : _canvas.Selected.Name == "Layer" ? _canvas.Selected : _document?.Layer(_canvas.Selected);

    private void AddLayer()
    {
        if (_document == null) return;
        _document.Checkpoint();
        var layer = _document.Xml.CreateElement("Layer");
        layer.SetAttribute("Type", "1");
        layer.SetAttribute("Factor", "1");
        var after = TargetLayer();
        if (after != null && after.ParentNode == _document.Root) _document.Root.InsertAfter(layer, after); else _document.Root.AppendChild(layer);
        _canvas.Selected = layer;
        AfterEdit(structure: true);
        SetStatus("Added a layer. Factor sets its parallax: 1 moves with the fighters, smaller values are further away.");
    }

    private void AddPictureFromLibrary()
    {
        if (_document == null || _library.SelectedItems.Count == 0) { SetStatus("Pick a picture in the list first."); return; }
        var layer = TargetLayer();
        if (layer == null || layer.GetAttribute("Type") == "2") { SetStatus("Select a picture layer (not the fighters layer) first."); return; }
        if (layer.GetAttribute("Path").Length > 0) SetStatus("Note: this layer draws from " + layer.GetAttribute("Path") + "; the picture must exist there in game.");
        string name = _library.SelectedItems[0].Text;
        var bitmap = _images.Load(null, name);
        float scale = (float)_artScale.Value;
        _document.Checkpoint();
        var image = _document.Xml.CreateElement("Image");
        var centre = _canvas.ViewCenter;
        var t = SceneRenderer.Transform(layer, _canvas.Options);
        ParamsDocument.SetNumber(image, "X", MathF.Round((centre.X - t.Dx) / t.Scale));
        ParamsDocument.SetNumber(image, "Y", MathF.Round((centre.Y - t.Dy) / t.Scale));
        image.SetAttribute("ClassName", name);
        ParamsDocument.SetNumber(image, "Width", (bitmap?.Width ?? 100) * scale);
        ParamsDocument.SetNumber(image, "Height", (bitmap?.Height ?? 100) * scale);
        if (_canvas.Selected != null && _canvas.Selected.ParentNode == layer) layer.InsertAfter(image, _canvas.Selected); else layer.AppendChild(image);
        _canvas.Selected = image;
        AfterEdit(structure: true);
    }

    private void Duplicate()
    {
        if (_document == null || _canvas.Selected == null || _canvas.Selected == _document.Root || _canvas.Selected.ParentNode == null) return;
        _document.Checkpoint();
        var copy = (XmlElement)_canvas.Selected.CloneNode(true);
        if (ParamsDocument.HasBox(copy)) ParamsDocument.SetNumber(copy, "X", ParamsDocument.Number(copy, "X") + 20);
        _canvas.Selected.ParentNode.InsertAfter(copy, _canvas.Selected);
        _canvas.Selected = copy;
        AfterEdit(structure: true);
    }

    private void DeleteSelected()
    {
        if (_document == null || _canvas.Selected == null || _canvas.Selected == _document.Root || _canvas.Selected.ParentNode == null) return;
        if (_canvas.Selected.Name == "Layer" && _canvas.Selected.HasChildNodes &&
            MessageBox.Show(this, "Delete this layer and everything in it?", "Delete layer", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;
        _document.Checkpoint();
        var parent = _canvas.Selected.ParentNode;
        var next = (_canvas.Selected.NextSibling ?? _canvas.Selected.PreviousSibling) as XmlElement;
        parent.RemoveChild(_canvas.Selected);
        _canvas.Options.HiddenLayers.RemoveWhere(l => l.ParentNode == null);
        _canvas.Selected = next ?? parent as XmlElement;
        AfterEdit(structure: true);
    }

    /// <summary>Moves the selection later (+1, drawn in front) or earlier (-1) among its siblings.</summary>
    private void MoveSelected(int direction)
    {
        if (_document == null || _canvas.Selected?.ParentNode is not XmlNode parent || _canvas.Selected == _document.Root) return;
        var siblings = parent.ChildNodes.OfType<XmlElement>().Where(e => e.Name == _canvas.Selected.Name || ParamsDocument.HasBox(e) && ParamsDocument.HasBox(_canvas.Selected)).ToList();
        int index = siblings.IndexOf(_canvas.Selected);
        int target = index + direction;
        if (target < 0 || target >= siblings.Count) return;
        _document.Checkpoint();
        var element = _canvas.Selected;
        parent.RemoveChild(element);
        if (direction > 0) parent.InsertAfter(element, siblings[target]); else parent.InsertBefore(element, siblings[target]);
        AfterEdit(structure: true);
    }

    private void FitBox(bool keepHeight)
    {
        if (_document == null || _canvas.Selected == null || !ParamsDocument.IsPicture(_canvas.Selected) || _document.Layer(_canvas.Selected) is not XmlElement layer) return;
        var bitmap = _images.Load(layer, _canvas.Selected.GetAttribute("ClassName"));
        if (bitmap == null) { SetStatus("The picture is missing; cannot read its size."); return; }
        _document.Checkpoint();
        if (keepHeight)
        {
            float height = Math.Abs(ParamsDocument.Number(_canvas.Selected, "Height", bitmap.Height * (float)_artScale.Value));
            ParamsDocument.SetNumber(_canvas.Selected, "Width", height * bitmap.Width / bitmap.Height);
            ParamsDocument.SetNumber(_canvas.Selected, "Height", height);
        }
        else
        {
            ParamsDocument.SetNumber(_canvas.Selected, "Width", bitmap.Width * (float)_artScale.Value);
            ParamsDocument.SetNumber(_canvas.Selected, "Height", bitmap.Height * (float)_artScale.Value);
        }
        AfterEdit(structure: false);
    }

    // ---- Library and checks ----

    private void RefreshLibrary()
    {
        _library.BeginUpdate();
        _library.Items.Clear();
        var used = _document == null ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : _document.Layers.Where(l => l.GetAttribute("Path").Length == 0).SelectMany(ParamsDocument.Items).Select(e => e.GetAttribute("ClassName")).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (string name in _images.Names())
        {
            var bitmap = _images.Load(null, name);
            var item = new ListViewItem(new[] { name, bitmap != null ? $"{bitmap.Width}×{bitmap.Height}" : "?", used.Contains(name) ? "yes" : "" });
            if (!used.Contains(name)) item.ForeColor = Color.DarkGoldenrod;
            _library.Items.Add(item);
        }
        _library.EndUpdate();
    }

    private void RefreshWarnings()
    {
        if (_document == null) { _warnings.Text = ""; return; }
        var lines = new List<string>();
        foreach (XmlElement layer in _document.Layers)
            foreach (XmlElement element in ParamsDocument.Items(layer))
            {
                if (!ParamsDocument.IsPicture(element)) continue;
                string name = element.GetAttribute("ClassName");
                var bitmap = _images.Load(layer, name);
                if (bitmap == null) { lines.Add("MISSING   " + name + (layer.GetAttribute("Path").Length > 0 ? "  (layer path " + layer.GetAttribute("Path") + ")" : "")); continue; }
                float w = Math.Abs(ParamsDocument.Number(element, "Width")), h = Math.Abs(ParamsDocument.Number(element, "Height"));
                if (w <= 0 || h <= 0 || name.StartsWith("pixel", StringComparison.OrdinalIgnoreCase)) continue;
                float ratio = bitmap.Width / (float)bitmap.Height / (w / h);
                if (Math.Abs(ratio - 1) > 0.05f)
                    lines.Add($"STRETCHED {name}: picture {bitmap.Width}×{bitmap.Height}, box {ParamsDocument.Format(w)}×{ParamsDocument.Format(h)} ({(ratio > 1 ? "squeezed" : "widened")} {Math.Abs(ratio - 1) * 100:0}%)");
            }
        if (_images.Folder == null) lines.Insert(0, "No images folder chosen (File > Choose images folder).");
        _warnings.Text = lines.Count == 0 ? "Every picture is found and keeps its proportions." : string.Join(Environment.NewLine, lines);
    }

    // ---- Keys and helpers ----

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        bool typing = ActiveControl is TextBoxBase || _grid.IsCurrentCellInEditMode;
        switch (keyData)
        {
            case Keys.Control | Keys.S: Save(false); return true;
            case Keys.Control | Keys.O: OpenDialog(); return true;
            case Keys.Control | Keys.D0: _canvas.FitAll(); return true;
        }
        if (!typing)
            switch (keyData)
            {
                case Keys.Control | Keys.Z: Undo(true); return true;
                case Keys.Control | Keys.Y: case Keys.Control | Keys.Shift | Keys.Z: Undo(false); return true;
                case Keys.Control | Keys.D: Duplicate(); return true;
                case Keys.Delete when _canvas.Focused || _tree.Focused: DeleteSelected(); return true;
                case Keys.PageUp when _canvas.Focused || _tree.Focused: MoveSelected(1); return true;
                case Keys.PageDown when _canvas.Focused || _tree.Focused: MoveSelected(-1); return true;
                case Keys.F when _canvas.Focused: _canvas.FrameSelection(); return true;
            }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private string Prompt(string question, string value)
    {
        using var form = new Form { Text = "Location Params Editor", Width = 420, Height = 150, FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent, MinimizeBox = false, MaximizeBox = false };
        var label = new Label { Text = question, Left = 12, Top = 12, Width = 380 };
        var box = new TextBox { Text = value, Left = 12, Top = 36, Width = 380 };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 232, Top = 70, Width = 75 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 317, Top = 70, Width = 75 };
        form.Controls.AddRange(new Control[] { label, box, ok, cancel });
        form.AcceptButton = ok; form.CancelButton = cancel;
        return form.ShowDialog(this) == DialogResult.OK ? box.Text : "";
    }

    private const string HelpText =
        "A location layout (params) lists layers drawn back to front. Each picture is centred at X, Y in its layer " +
        "(Y points down; 0, 0 is the middle of the location) and stretched to Width × Height. ClassName is the picture's " +
        "file name without .png. Layers with Path=\"Locations/<name>/\" draw another location's pictures: put that location's " +
        "folder (named <name> or <name>_new) next to the images folder, or add it as an extra search folder.\n\n" +
        "Factor is a layer's parallax: 1 moves with the fighters, 0.1 hardly moves. Move the Camera slider to preview it. " +
        "The green line is the floor (Floor), the orange lines the walls (Wall), and the boxes mark the fighter start points.\n\n" +
        "Mouse: wheel zooms, right/middle drag pans, left drag moves, the handles resize (Shift keeps proportions, Ctrl snaps to whole units). " +
        "Keys: arrows nudge (Shift ×10), Del deletes, Ctrl+D duplicates, PgUp/PgDn change draw order, F frames the selection, Ctrl+0 fits all.\n\n" +
        "Animated effects and particles are shown as blue placeholders; edit their attributes in the grid. " +
        "Saving keeps every attribute, including ones the editor does not know.";
}
