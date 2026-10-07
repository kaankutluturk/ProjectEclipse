using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

namespace Eclipse.LocationParamsEditor;

/// <summary>
/// The layout canvas. Wheel zooms around the pointer; right or middle drag pans. Left click
/// selects the front-most picture under the pointer (fill pixels are skipped while "lock
/// fills" is on); dragging moves it, the handles resize it (Shift keeps the proportions),
/// arrow keys nudge it by 1 (Shift: 10). Ctrl while moving snaps to whole units.
/// </summary>
public sealed class CanvasView : Control
{
    public ParamsDocument? Document;
    public ImageLibrary? Images;
    public SceneOptions Options = new();
    public XmlElement? Selected;
    public bool LockFills = true;
    /// <summary>Before a drag or nudge changes the document (one undo step per gesture).</summary>
    public event Action? Changing;
    /// <summary>After a drag, resize or nudge.</summary>
    public event Action? Changed;
    public event Action<XmlElement?>? SelectionChanged;
    public event Action<PointF>? PointerMoved;
    /// <summary>After the driven fighters move (game viewport).</summary>
    public event Action? FightersMoved;

    // Game viewport: A/D move player one, Left/Right player two, while held (Shift: faster).
    private const float WalkSpeed = 350f, RunSpeed = 1000f, FighterGap = 80f;
    private readonly HashSet<Keys> _held = new();
    private readonly System.Windows.Forms.Timer _driveTimer = new() { Interval = 15 };
    private readonly System.Diagnostics.Stopwatch _driveClock = new();

    private float _zoom = 0.5f;
    private PointF _center;
    private Point _panFrom;
    private bool _panning;
    private enum Drag { None, Move, Resize }
    private Drag _drag;
    private int _handle;
    private PointF _dragWorld;
    private RectangleF _dragBox;
    private bool _dragChanged;

    public CanvasView()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
        BackColor = Color.FromArgb(40, 40, 46);
        TabStop = true;
        _driveTimer.Tick += (_, _) => Drive();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _driveTimer.Dispose();
        base.Dispose(disposing);
    }

    private static bool IsDriveKey(Keys key) => key is Keys.A or Keys.D or Keys.Left or Keys.Right;

    private void Drive()
    {
        if (Document == null || !Options.Viewport || _held.Count == 0) { _driveTimer.Stop(); _driveClock.Reset(); return; }
        float dt = Math.Min(0.1f, (float)_driveClock.Elapsed.TotalSeconds);
        _driveClock.Restart();
        if (!Options.Player1X.HasValue || !Options.Player2X.HasValue)
        {
            var (start1, start2) = SceneRenderer.Fighters(Document, Options);
            Options.Player1X = start1; Options.Player2X = start2;
        }
        float speed = ((ModifierKeys & Keys.Shift) != 0 ? RunSpeed : WalkSpeed) * dt;
        float move1 = (_held.Contains(Keys.D) ? 1 : 0) - (_held.Contains(Keys.A) ? 1 : 0);
        float move2 = (_held.Contains(Keys.Right) ? 1 : 0) - (_held.Contains(Keys.Left) ? 1 : 0);
        float width = Document.RootNumber("Width"), wall = Document.RootNumber("Wall");
        float low = -width / 2 + wall, high = width / 2 - wall;
        float p1 = Options.Player1X.Value + move1 * speed, p2 = Options.Player2X.Value + move2 * speed;
        if (high > low) { p1 = Math.Clamp(p1, low, high); p2 = Math.Clamp(p2, low, high); }
        // The fighters cannot walk through each other: whoever moved stops at the other.
        if (p1 > p2 - FighterGap)
        {
            if (move1 != 0 && move2 == 0) p1 = p2 - FighterGap;
            else if (move2 != 0 && move1 == 0) p2 = p1 + FighterGap;
            else { float mid = (p1 + p2) / 2; p1 = mid - FighterGap / 2; p2 = mid + FighterGap / 2; }
        }
        Options.Player1X = p1; Options.Player2X = p2;
        // Keep the screen in view as the camera moves.
        SceneRenderer.UpdateCamera(Document, Options);
        Invalidate();
        FightersMoved?.Invoke();
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (_held.Remove(e.KeyCode)) e.Handled = true;
        else base.OnKeyUp(e);
    }

    protected override void OnLostFocus(EventArgs e)
    {
        _held.Clear();
        base.OnLostFocus(e);
    }

    public float Zoom => _zoom;

    public PointF ToWorld(Point screen) =>
        new((screen.X - Width / 2f) / _zoom + _center.X, (screen.Y - Height / 2f) / _zoom + _center.Y);

    public PointF ViewCenter => _center;

    public void FitAll()
    {
        if (Document == null) return;
        var frame = SceneRenderer.Frame(Document, Options);
        _center = new PointF(frame.X + frame.Width / 2, frame.Y + frame.Height / 2);
        _zoom = Math.Max(0.03f, Math.Min((Width - 20) / frame.Width, (Height - 20) / frame.Height));
        Invalidate();
    }

    public void FrameSelection()
    {
        if (Document == null || Selected == null || Document.Layer(Selected) is not XmlElement layer || !ParamsDocument.HasBox(Selected)) return;
        var box = SceneRenderer.Box(Selected, layer, Options);
        _center = new PointF(box.X + box.Width / 2, box.Y + box.Height / 2);
        _zoom = Math.Max(0.05f, Math.Min(4f, Math.Min((Width - 80) / Math.Max(box.Width, 1), (Height - 80) / Math.Max(box.Height, 1))));
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        if (Document == null || Images == null)
        {
            TextRenderer.DrawText(g, "Open a params file (File > Open, or drop one here).", Font, ClientRectangle, Color.Gray,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }
        var state = g.Save();
        g.TranslateTransform(Width / 2f, Height / 2f);
        g.ScaleTransform(_zoom, _zoom);
        g.TranslateTransform(-_center.X, -_center.Y);
        try { SceneRenderer.Draw(g, Document, Images, Options, _zoom); }
        catch (Exception ex) { g.Restore(state); TextRenderer.DrawText(g, "Draw failed: " + ex.Message, Font, new Point(8, 8), Color.OrangeRed); return; }
        if (Selected != null && Document.Layer(Selected) is XmlElement layer && ParamsDocument.HasBox(Selected))
        {
            var box = SceneRenderer.Box(Selected, layer, Options);
            using var pen = new Pen(Color.Gold, 2f / _zoom);
            g.DrawRectangle(pen, box.X, box.Y, box.Width, box.Height);
            if (ParamsDocument.IsPicture(Selected))
                foreach (var handle in Handles(box))
                {
                    float s = 7f / _zoom;
                    g.FillRectangle(Brushes.Gold, handle.X - s / 2, handle.Y - s / 2, s, s);
                }
        }
        g.Restore(state);
        TextRenderer.DrawText(g, $"zoom {_zoom * 100:0}%", Font, new Point(6, Height - 20), Color.Gray);
    }

    private static PointF[] Handles(RectangleF box) => new[]
    {
        new PointF(box.Left, box.Top), new PointF(box.Right, box.Top), new PointF(box.Left, box.Bottom), new PointF(box.Right, box.Bottom),
        new PointF(box.Left + box.Width / 2, box.Top), new PointF(box.Left + box.Width / 2, box.Bottom),
        new PointF(box.Left, box.Top + box.Height / 2), new PointF(box.Right, box.Top + box.Height / 2),
    };

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        var before = ToWorld(e.Location);
        _zoom = Math.Clamp(_zoom * (e.Delta > 0 ? 1.15f : 1 / 1.15f), 0.03f, 16f);
        var after = ToWorld(e.Location);
        _center = new PointF(_center.X + before.X - after.X, _center.Y + before.Y - after.Y);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        Focus();
        if (e.Button is MouseButtons.Right or MouseButtons.Middle) { _panning = true; _panFrom = e.Location; return; }
        if (e.Button != MouseButtons.Left || Document == null) return;
        var world = ToWorld(e.Location);
        if (Selected != null && ParamsDocument.IsPicture(Selected) && Document.Layer(Selected) is XmlElement selectedLayer)
        {
            var box = SceneRenderer.Box(Selected, selectedLayer, Options);
            var handles = Handles(box);
            for (int i = 0; i < handles.Length; i++)
                if (Math.Abs(handles[i].X - world.X) * _zoom <= 6 && Math.Abs(handles[i].Y - world.Y) * _zoom <= 6)
                {
                    Begin(Drag.Resize, world, box); _handle = i; return;
                }
        }
        var hit = HitTest(world);
        if (hit != Selected) { Selected = hit; SelectionChanged?.Invoke(hit); }
        if (hit != null && ParamsDocument.HasBox(hit) && Document.Layer(hit) is XmlElement layer)
            Begin(Drag.Move, world, SceneRenderer.Box(hit, layer, Options));
        Invalidate();
    }

    private void Begin(Drag drag, PointF world, RectangleF box)
    {
        _drag = drag; _dragWorld = world; _dragBox = box; _dragChanged = false;
        Capture = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var world = ToWorld(e.Location);
        PointerMoved?.Invoke(world);
        if (_panning)
        {
            _center = new PointF(_center.X - (e.X - _panFrom.X) / _zoom, _center.Y - (e.Y - _panFrom.Y) / _zoom);
            _panFrom = e.Location;
            Invalidate();
            return;
        }
        if (_drag == Drag.None || Selected == null || Document?.Layer(Selected) is not XmlElement layer) return;
        float dx = world.X - _dragWorld.X, dy = world.Y - _dragWorld.Y;
        if (!_dragChanged && Math.Abs(dx) * _zoom < 2 && Math.Abs(dy) * _zoom < 2) return;
        if (!_dragChanged) { Changing?.Invoke(); _dragChanged = true; }
        // Screen-space changes map back through the layer's transform (viewport zoom and pan).
        var t = SceneRenderer.Transform(layer, Options);
        bool snap = (ModifierKeys & Keys.Control) != 0;
        if (_drag == Drag.Move)
        {
            float x = (_dragBox.X + _dragBox.Width / 2 + dx - t.Dx) / t.Scale, y = (_dragBox.Y + _dragBox.Height / 2 + dy - t.Dy) / t.Scale;
            if (snap) { x = MathF.Round(x); y = MathF.Round(y); }
            ParamsDocument.SetNumber(Selected, "X", x);
            ParamsDocument.SetNumber(Selected, "Y", y);
        }
        else
        {
            float left = _dragBox.Left, right = _dragBox.Right, top = _dragBox.Top, bottom = _dragBox.Bottom;
            if (_handle is 0 or 2 or 6) left += dx;
            if (_handle is 1 or 3 or 7) right += dx;
            if (_handle is 0 or 1 or 4) top += dy;
            if (_handle is 2 or 3 or 5) bottom += dy;
            if ((ModifierKeys & Keys.Shift) != 0 && _handle < 4 && _dragBox.Height > 0)
            {
                // Keep the proportions: follow the larger change and anchor the opposite corner.
                float aspect = _dragBox.Width / _dragBox.Height;
                float w = Math.Abs(right - left), h = Math.Abs(bottom - top);
                if (w / aspect > h) h = w / aspect; else w = h * aspect;
                if (_handle is 0 or 2) left = right - w; else right = left + w;
                if (_handle is 0 or 1) top = bottom - h; else bottom = top + h;
            }
            float width = Math.Max(1, Math.Abs(right - left)) / t.Scale, height = Math.Max(1, Math.Abs(bottom - top)) / t.Scale;
            if (snap) { width = MathF.Round(width); height = MathF.Round(height); }
            ParamsDocument.SetNumber(Selected, "X", (Math.Min(left, right) + width * t.Scale / 2 - t.Dx) / t.Scale);
            ParamsDocument.SetNumber(Selected, "Y", (Math.Min(top, bottom) + height * t.Scale / 2 - t.Dy) / t.Scale);
            ParamsDocument.SetNumber(Selected, "Width", width);
            ParamsDocument.SetNumber(Selected, "Height", height);
        }
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _panning = false;
        if (_drag != Drag.None)
        {
            _drag = Drag.None;
            Capture = false;
            if (_dragChanged) Changed?.Invoke();
        }
    }

    protected override bool IsInputKey(Keys keyData) =>
        (keyData & Keys.KeyCode) is Keys.Left or Keys.Right or Keys.Up or Keys.Down || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (Options.Viewport && IsDriveKey(e.KeyCode) && !e.Control && !e.Alt)
        {
            if (_held.Add(e.KeyCode) && !_driveTimer.Enabled) { _driveClock.Restart(); _driveTimer.Start(); }
            e.Handled = true; e.SuppressKeyPress = true;
            return;
        }
        if (Selected == null || !ParamsDocument.HasBox(Selected)) { base.OnKeyDown(e); return; }
        float step = e.Shift ? 10 : 1, dx = 0, dy = 0;
        switch (e.KeyCode)
        {
            case Keys.Left: dx = -step; break;
            case Keys.Right: dx = step; break;
            case Keys.Up: dy = -step; break;
            case Keys.Down: dy = step; break;
            default: base.OnKeyDown(e); return;
        }
        Changing?.Invoke();
        ParamsDocument.SetNumber(Selected, "X", ParamsDocument.Number(Selected, "X") + dx);
        ParamsDocument.SetNumber(Selected, "Y", ParamsDocument.Number(Selected, "Y") + dy);
        Changed?.Invoke();
        Invalidate();
        e.Handled = true;
    }

    /// <summary>The front-most visible boxed element under a world point.</summary>
    public XmlElement? HitTest(PointF world)
    {
        if (Document == null) return null;
        XmlElement? hit = null;
        foreach (XmlElement layer in Document.Layers)
        {
            if (Options.HiddenLayers.Contains(layer)) continue;
            if (Options.SoloLayer != null && Options.SoloLayer != layer) continue;
            foreach (XmlElement element in ParamsDocument.Items(layer))
            {
                if (!ParamsDocument.HasBox(element)) continue;
                if (LockFills && element.GetAttribute("ClassName").StartsWith("pixel", StringComparison.OrdinalIgnoreCase)) continue;
                if (SceneRenderer.Box(element, layer, Options).Contains(world)) hit = element;
            }
        }
        return hit;
    }
}
