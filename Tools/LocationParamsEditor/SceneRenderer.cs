using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Xml;

namespace Eclipse.LocationParamsEditor;

/// <summary>Display options shared by the editor canvas and the --render command.</summary>
public sealed class SceneOptions
{
    public bool Bounds = true, FloorAndWalls = true, Fighters = true, Placeholders = true, Labels = false;
    /// <summary>
    /// Editor view: camera position along the game layer (each layer moves by -camera × Factor).
    /// Viewport view: the fight centre's offset from the middle of the location.
    /// </summary>
    public float CameraX;
    public HashSet<XmlElement> HiddenLayers = new();
    public XmlElement? SoloLayer;

    /// <summary>Show the location through the game's fight camera at <see cref="Aspect"/>.</summary>
    public bool Viewport;
    public float Aspect = 21f / 9f;
    /// <summary>Distance between the fighters (drives the auto zoom); null uses the start positions.</summary>
    public float? FighterDistance;
    /// <summary>
    /// Fighter positions on the game layer (0 = middle of the location) once they are driven
    /// with the keys; null derives them from the start positions, distance and camera.
    /// </summary>
    public float? Player1X, Player2X;
    public float BindingLength = 100f;
    /// <summary>internalSettings.xml CameraSettings.</summary>
    public float MaxWidth = 1100f, MaxWidthDelta = 50f;
    /// <summary>Filled by <see cref="SceneRenderer.UpdateCamera"/>.</summary>
    public ViewportCamera Camera = ViewportCamera.Editor;
}

/// <summary>Where one layer lands: points scale by <see cref="Scale"/> about the location centre, then move.</summary>
public readonly record struct LayerTransform(float Scale, float Dx, float Dy)
{
    public PointF Apply(PointF p) => new(p.X * Scale + Dx, p.Y * Scale + Dy);
}

/// <summary>The fight camera for one frame, as Render.UpdatePosition computes it.</summary>
public sealed record ViewportCamera(bool Active, float Zoom, float OffsetX, float VisibleWidth, float Height, float VerticalOffset, float Distance, bool MaxWidthLimited,
    float Player1X = 0, float Player2X = 0)
{
    public static readonly ViewportCamera Editor = new(false, 1, 0, 0, 0, 0, 0, false);
    /// <summary>The screen in location space (origin at the screen centre, Y down).</summary>
    public RectangleF Screen => new(-VisibleWidth / 2, -Height / 2, VisibleWidth, Height);
}

/// <summary>
/// Draws a layout the way Location.cs builds it: each picture is centred at (X, Y) in its layer
/// (Y pointing down) and stretched to Width × Height; FlipX/FlipY mirror it and Color tints it.
/// Layers and pictures are drawn in document order. The world origin is the middle of the
/// location. In viewport mode each layer gets the fight camera's transform (Render.cs):
/// zoom for the game layer and Scaling="1" layers, a vertical shift for the others, and the
/// horizontal pan times Factor.
/// </summary>
public static class SceneRenderer
{
    /// <summary>
    /// Render.RefreshViewportMetrics + UpdatePosition: the location's Height fills the screen
    /// height at zoom 1, so the screen shows Height × aspect units. Zoom follows the fighter
    /// distance (min(visible / (distance + 300), 1)), never shows more than CameraSettings
    /// MaxWidth units across nor more than the location's Width, and the pan is clamped so the
    /// location edges (less MaxWidthDelta) stay off screen.
    /// </summary>
    public static void UpdateCamera(ParamsDocument document, SceneOptions options)
    {
        if (!options.Viewport) { options.Camera = ViewportCamera.Editor; return; }
        float width = Math.Max(1, document.RootNumber("Width")), height = Math.Max(1, document.RootNumber("Height"));
        float floor = document.RootNumber("Floor");
        float visible = height * options.Aspect;
        float minZoom = visible / width;
        var (p1, p2) = Fighters(document, options);
        float distance = Math.Abs(p2 - p1);
        float zoom = Math.Min(visible / (distance + 300f), 1f);
        // The camera centres between the fighters (the fight's centre position).
        float offset = -(p1 + p2) / 2;
        bool limited = false;
        if (options.MaxWidth > 0)
        {
            float widest = minZoom / (options.MaxWidth / width);
            if (zoom < widest)
            {
                zoom = widest;
                limited = true;
                // Zoom is capped, so the fighters may not both fit: the camera moves just enough to
                // keep player one (the camera's target) BindingLength inside the screen edge.
                float half = visible / zoom / 2, fromCentre = p1 + offset;
                if (Math.Abs(fromCentre) + options.BindingLength > half)
                    offset += -Math.Sign(fromCentre) * (Math.Abs(fromCentre) - half + options.BindingLength);
            }
        }
        zoom = Math.Max(zoom, minZoom);
        offset *= zoom;
        float limit = (width - options.MaxWidthDelta) * zoom / 2 - visible / 2;
        if (Math.Abs(offset) > limit) offset = offset < 0 ? -limit : limit;
        options.Camera = new ViewportCamera(true, zoom, offset, visible, height, (height / 2 - floor) / 2, distance, limited, p1, p2);
    }

    /// <summary>
    /// Fighter positions on the game layer: the driven ones, else the start positions (or the
    /// chosen distance around the chosen camera), kept between the walls.
    /// </summary>
    public static (float p1, float p2) Fighters(ParamsDocument document, SceneOptions options)
    {
        float width = Math.Max(1, document.RootNumber("Width")), wall = document.RootNumber("Wall");
        float p1, p2;
        if (options.Player1X.HasValue && options.Player2X.HasValue) { p1 = options.Player1X.Value; p2 = options.Player2X.Value; }
        else
        {
            var viewer = document.FighterLayer?.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.Name == "ModelsViewer");
            if (viewer != null && options.FighterDistance == null && options.CameraX == 0)
            {
                p1 = ParamsDocument.Number(viewer, "PlayerPositionX") - width / 2;
                p2 = ParamsDocument.Number(viewer, "EnemyPositionX") - width / 2;
            }
            else
            {
                float half = (options.FighterDistance ?? StartDistance(document)) / 2;
                p1 = options.CameraX - half; p2 = options.CameraX + half;
            }
        }
        float low = -width / 2 + wall, high = width / 2 - wall;
        if (high > low) { p1 = Math.Clamp(p1, low, high); p2 = Math.Clamp(p2, low, high); }
        return (p1, p2);
    }

    public static float StartDistance(ParamsDocument document)
    {
        var viewer = document.FighterLayer?.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.Name == "ModelsViewer");
        return viewer == null ? 300 : Math.Abs(ParamsDocument.Number(viewer, "EnemyPositionX") - ParamsDocument.Number(viewer, "PlayerPositionX"));
    }

    public static LayerTransform Transform(XmlElement layer, SceneOptions options)
    {
        float factor = ParamsDocument.Number(layer, "Factor", 1f);
        var camera = options.Camera;
        if (!camera.Active) return new LayerTransform(1, -options.CameraX * factor, 0);
        bool scaled = layer.GetAttribute("Type") == "2" || ParamsDocument.Number(layer, "Scaling") > 0;
        return scaled ? new LayerTransform(camera.Zoom, camera.OffsetX * factor, 0)
            : new LayerTransform(1, camera.OffsetX * factor, camera.VerticalOffset * (1 - camera.Zoom));
    }

    /// <summary>The game layer's transform (floor, walls and fighters live there).</summary>
    private static LayerTransform GameTransform(ParamsDocument document, SceneOptions options) =>
        document.FighterLayer is XmlElement layer ? Transform(layer, options)
            : options.Camera.Active ? new LayerTransform(options.Camera.Zoom, options.Camera.OffsetX, 0) : new LayerTransform(1, -options.CameraX, 0);

    /// <summary>The drawn rectangle (Y down) of a boxed element.</summary>
    public static RectangleF Box(XmlElement element, XmlElement layer, SceneOptions options)
    {
        float w = Math.Abs(ParamsDocument.Number(element, "Width"));
        float h = Math.Abs(ParamsDocument.Number(element, "Height"));
        if (w <= 0 || h <= 0) { w = Math.Max(w, 40); h = Math.Max(h, 40); }
        var t = Transform(layer, options);
        var centre = t.Apply(new PointF(ParamsDocument.Number(element, "X"), ParamsDocument.Number(element, "Y")));
        return new RectangleF(centre.X - w * t.Scale / 2, centre.Y - h * t.Scale / 2, w * t.Scale, h * t.Scale);
    }

    public static void Draw(Graphics g, ParamsDocument document, ImageLibrary images, SceneOptions options, float zoom)
    {
        UpdateCamera(document, options);
        g.InterpolationMode = zoom < 1f ? InterpolationMode.HighQualityBilinear : InterpolationMode.Bilinear;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        using var labelFont = new Font("Segoe UI", 9f / Math.Max(zoom, 0.05f), GraphicsUnit.Pixel);
        foreach (XmlElement layer in document.Layers)
        {
            if (options.HiddenLayers.Contains(layer)) continue;
            bool dim = options.SoloLayer != null && options.SoloLayer != layer;
            foreach (XmlElement element in ParamsDocument.Items(layer))
            {
                if (element.Name == "ModelsViewer") continue;
                var box = Box(element, layer, options);
                if (ParamsDocument.IsPicture(element))
                {
                    var bitmap = images.Load(layer, element.GetAttribute("ClassName"));
                    if (bitmap != null) DrawPicture(g, bitmap, element, box, dim);
                    else if (options.Placeholders) DrawPlaceholder(g, box, element.GetAttribute("ClassName") + "  (missing)", Color.FromArgb(200, 220, 60, 160), labelFont, zoom);
                }
                else if (options.Placeholders)
                {
                    string kind = element.Name == "SimpleEffect" ? "animation " + element.GetAttribute("ClassName") : "particles";
                    DrawPlaceholder(g, box, kind, Color.FromArgb(160, 80, 170, 230), labelFont, zoom);
                }
                if (options.Labels && ParamsDocument.IsPicture(element))
                    g.DrawString(element.GetAttribute("ClassName"), labelFont, Brushes.White, box.Left + 2 / zoom, box.Top + 2 / zoom);
            }
        }
        DrawOverlays(g, document, options, zoom, labelFont);
        if (options.Camera.Active) DrawScreen(g, options, zoom, labelFont);
    }

    private static void DrawPicture(Graphics g, Bitmap bitmap, XmlElement element, RectangleF box, bool dim)
    {
        bool flipX = element.GetAttribute("FlipX") is "1" or "true";
        bool flipY = element.GetAttribute("FlipY") is "1" or "true";
        if (ParamsDocument.Number(element, "Width") < 0) flipX = !flipX;
        if (ParamsDocument.Number(element, "Height") < 0) flipY = !flipY;
        var tint = ParseColor(element.GetAttribute("Color"));
        using var attributes = new ImageAttributes();
        float a = dim ? 0.25f : 1f;
        attributes.SetColorMatrix(new ColorMatrix(new[]
        {
            new[] { tint.R / 255f, 0, 0, 0, 0 },
            new[] { 0, tint.G / 255f, 0, 0, 0 },
            new[] { 0, 0, tint.B / 255f, 0, 0 },
            new[] { 0, 0, 0, tint.A / 255f * a, 0 },
            new[] { 0f, 0, 0, 0, 1 },
        }));
        attributes.SetWrapMode(WrapMode.TileFlipXY);
        var state = g.Save();
        g.TranslateTransform(box.X + box.Width / 2, box.Y + box.Height / 2);
        g.ScaleTransform(flipX ? -1 : 1, flipY ? -1 : 1);
        var destination = new[] { new PointF(-box.Width / 2, -box.Height / 2), new PointF(box.Width / 2, -box.Height / 2), new PointF(-box.Width / 2, box.Height / 2) };
        g.DrawImage(bitmap, destination, new RectangleF(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel, attributes);
        g.Restore(state);
    }

    private static void DrawPlaceholder(Graphics g, RectangleF box, string text, Color color, Font font, float zoom)
    {
        using var brush = new HatchBrush(HatchStyle.WideUpwardDiagonal, color, Color.FromArgb(40, color));
        using var pen = new Pen(color, 1.5f / zoom);
        g.FillRectangle(brush, box);
        g.DrawRectangle(pen, box.X, box.Y, box.Width, box.Height);
        g.DrawString(text, font, Brushes.White, box.X + 3 / zoom, box.Y + 3 / zoom);
    }

    private static void DrawOverlays(Graphics g, ParamsDocument document, SceneOptions options, float zoom, Font font)
    {
        float width = document.RootNumber("Width"), height = document.RootNumber("Height");
        float floor = document.RootNumber("Floor"), wall = document.RootNumber("Wall");
        var t = GameTransform(document, options);
        PointF P(float x, float y) => t.Apply(new PointF(x, y));
        if (options.Bounds && width > 0 && height > 0 && !options.Camera.Active)
        {
            using var pen = new Pen(Color.FromArgb(200, 255, 255, 255), 1.5f / zoom) { DashStyle = DashStyle.Dash };
            var a = P(-width / 2, -height / 2);
            g.DrawRectangle(pen, a.X, a.Y, width * t.Scale, height * t.Scale);
            g.DrawString($"location {Fmt(width)} × {Fmt(height)}", font, Brushes.White, a.X + 4 / zoom, a.Y - 14 / zoom);
            using var cross = new Pen(Color.FromArgb(160, 255, 255, 255), 1f / zoom);
            var o = P(0, 0);
            g.DrawLine(cross, o.X - 12 / zoom, o.Y, o.X + 12 / zoom, o.Y);
            g.DrawLine(cross, o.X, o.Y - 12 / zoom, o.X, o.Y + 12 / zoom);
        }
        if (options.FloorAndWalls && width > 0 && height > 0)
        {
            float floorY = height / 2 - floor;
            using var floorPen = new Pen(Color.FromArgb(230, 90, 220, 120), 2f / zoom);
            var left = P(-width / 2, floorY); var right = P(width / 2, floorY);
            g.DrawLine(floorPen, left, right);
            g.DrawString($"floor ({Fmt(floor)})", font, Brushes.LightGreen, left.X + 4 / zoom, left.Y + 2 / zoom);
            if (wall > 0)
            {
                using var wallPen = new Pen(Color.FromArgb(230, 240, 170, 60), 2f / zoom) { DashStyle = DashStyle.DashDot };
                foreach (float x in new[] { -width / 2 + wall, width / 2 - wall })
                    g.DrawLine(wallPen, P(x, -height / 2), P(x, floorY));
                var label = P(-width / 2 + wall, -height / 2);
                g.DrawString($"wall ({Fmt(wall)})", font, Brushes.Orange, label.X + 4 / zoom, label.Y + 4 / zoom);
            }
        }
        if (options.Fighters && document.FighterLayer?.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.Name == "ModelsViewer") is XmlElement viewer && width > 0)
        {
            float floorY = height / 2 - floor;
            foreach (var (name, color, label) in new[] { ("Player", Color.FromArgb(220, 80, 160, 255), "player 1  (A / D)"), ("Enemy", Color.FromArgb(220, 255, 90, 90), "player 2  (Left / Right)") })
            {
                float x = options.Camera.Active ? (name == "Player" ? options.Camera.Player1X : options.Camera.Player2X)
                    : ParamsDocument.Number(viewer, name + "PositionX") - width / 2;
                var foot = P(x, floorY);
                float w = 60 * t.Scale, h = 170 * t.Scale;
                using var brush = new SolidBrush(Color.FromArgb(70, color));
                using var pen = new Pen(color, 1.5f / zoom);
                var body = new RectangleF(foot.X - w / 2, foot.Y - h, w, h);
                g.FillRectangle(brush, body);
                g.DrawRectangle(pen, body.X, body.Y, body.Width, body.Height);
                g.DrawString(options.Camera.Active ? label : name.ToLowerInvariant() + " start", font, Brushes.White, body.X, body.Y - 13 / zoom);
            }
        }
    }

    /// <summary>Darkens everything outside the screen and labels the camera.</summary>
    private static void DrawScreen(Graphics g, SceneOptions options, float zoom, Font font)
    {
        var camera = options.Camera;
        var screen = camera.Screen;
        using (var outside = new Region(new RectangleF(-100000, -100000, 200000, 200000)))
        {
            outside.Exclude(screen);
            using var shade = new SolidBrush(Color.FromArgb(200, 12, 12, 16));
            g.FillRegion(shade, outside);
        }
        using var pen = new Pen(Color.FromArgb(255, 120, 200, 255), 2f / zoom);
        g.DrawRectangle(pen, screen.X, screen.Y, screen.Width, screen.Height);
        string aspect = AspectName(options.Aspect);
        string text = $"{aspect} screen   ·   zoom {camera.Zoom:0.###}   ·   shows {Fmt(camera.VisibleWidth / camera.Zoom)} × {Fmt(camera.Height / camera.Zoom)} game-layer units" +
            $"   ·   fighters {Fmt(camera.Distance)} apart" + (camera.MaxWidthLimited ? "   ·   limited by MaxWidth " + Fmt(options.MaxWidth) : "");
        g.DrawString(text, font, Brushes.LightSkyBlue, screen.X, screen.Bottom + 6 / zoom);
    }

    public static string AspectName(float aspect)
    {
        foreach (var (w, h) in new[] { (21, 9), (16, 9), (18, 9), (32, 9), (4, 3), (16, 10), (3, 2) })
            if (Math.Abs(aspect - (float)w / h) < 0.01f) return $"{w}:{h}";
        return aspect.ToString("0.###", CultureInfo.InvariantCulture) + ":1";
    }

    public static Color ParseColor(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Color.White;
        string hex = text.Trim();
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) hex = hex[2..];
        if (hex.StartsWith('#')) hex = hex[1..];
        if (!uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value)) return Color.White;
        return hex.Length >= 8 ? Color.FromArgb((int)(value & 0xFF), (int)(value >> 24 & 0xFF), (int)(value >> 16 & 0xFF), (int)(value >> 8 & 0xFF))
            : Color.FromArgb(255, (int)(value >> 16 & 0xFF), (int)(value >> 8 & 0xFF), (int)(value & 0xFF));
    }

    private static string Fmt(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>The world area a render or "fit all" frames.</summary>
    public static RectangleF Frame(ParamsDocument document, SceneOptions options)
    {
        UpdateCamera(document, options);
        if (options.Camera.Active)
        {
            var screen = options.Camera.Screen;
            screen.Inflate(screen.Width * 0.06f, screen.Height * 0.12f);
            return screen;
        }
        float width = Math.Max(document.RootNumber("Width"), 400), height = Math.Max(document.RootNumber("Height"), 300);
        return new RectangleF(-width / 2 - 200 - options.CameraX, -height / 2 - 200, width + 400, height + 400);
    }

    /// <summary>Renders the layout (or the viewport) to a PNG, for checks and previews.</summary>
    public static void RenderToFile(ParamsDocument document, ImageLibrary images, SceneOptions options, string output, float scale)
    {
        var world = Frame(document, options);
        int pw = Math.Max(1, (int)(world.Width * scale)), ph = Math.Max(1, (int)(world.Height * scale));
        using var bitmap = new Bitmap(pw, ph, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.FromArgb(40, 40, 46));
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-world.X, -world.Y);
            Draw(g, document, images, options, scale);
        }
        bitmap.Save(output, ImageFormat.Png);
    }
}
