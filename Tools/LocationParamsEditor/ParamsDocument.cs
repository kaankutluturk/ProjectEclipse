using System.Globalization;
using System.Text;
using System.Xml;

namespace Eclipse.LocationParamsEditor;

/// <summary>
/// A location layout (params.xml / params.txt) kept as the original XML, so attributes and
/// elements the editor does not know survive a save. Element order is draw order: later
/// layers, and later pictures within a layer, are drawn in front.
/// </summary>
public sealed class ParamsDocument
{
    private readonly List<string> _undo = new();
    private readonly List<string> _redo = new();
    private const int MaxUndo = 200;

    public XmlDocument Xml { get; private set; } = new();
    public string? Path { get; private set; }
    public bool Dirty { get; private set; }
    private string _newLine = "\n";

    public XmlElement Root => Xml.DocumentElement ?? throw new InvalidOperationException("The file has no <Root> element.");

    public static ParamsDocument Load(string path)
    {
        string text = File.ReadAllText(path);
        var document = new ParamsDocument { Path = path, _newLine = text.Contains("\r\n") ? "\r\n" : "\n" };
        document.Xml = Parse(text);
        if (document.Root.Name != "Root") throw new InvalidDataException("A location layout starts with <Root>, not <" + document.Root.Name + ">.");
        return document;
    }

    public static ParamsDocument New()
    {
        var document = new ParamsDocument();
        document.Xml = Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Root Wall=\"75\" Floor=\"43\" Width=\"1536\" Height=\"512\">\n" +
            "  <Layer Type=\"1\" Factor=\"1\" Scaling=\"1\" />\n" +
            "  <Layer Type=\"2\" Factor=\"1\">\n    <ModelsViewer PlayerPositionX=\"500\" PlayerPositionY=\"-93\" EnemyPositionX=\"763\" EnemyPositionY=\"-110\" />\n  </Layer>\n</Root>\n");
        return document;
    }

    private static XmlDocument Parse(string text)
    {
        var xml = new XmlDocument { XmlResolver = null, PreserveWhitespace = false };
        using var reader = XmlReader.Create(new StringReader(text), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        xml.Load(reader);
        return xml;
    }

    public void Save(string path)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true, IndentChars = "  ", NewLineChars = _newLine, NewLineHandling = NewLineHandling.Replace,
            Encoding = new UTF8Encoding(false), OmitXmlDeclaration = false,
        };
        var buffer = new StringBuilder();
        using (var writer = XmlWriter.Create(new StringWriterUtf8(buffer), settings)) Xml.Save(writer);
        string temp = path + ".editor-write";
        File.WriteAllText(temp, buffer.ToString() + _newLine, new UTF8Encoding(false));
        if (File.Exists(path)) File.Replace(temp, path, null); else File.Move(temp, path);
        Path = path;
        Dirty = false;
    }

    private sealed class StringWriterUtf8 : StringWriter
    {
        public StringWriterUtf8(StringBuilder builder) : base(builder, CultureInfo.InvariantCulture) { }
        public override Encoding Encoding => new UTF8Encoding(false);
    }

    // ---- Undo ----

    /// <summary>Records the state before a change; call once per user action.</summary>
    public void Checkpoint()
    {
        _undo.Add(Xml.OuterXml);
        if (_undo.Count > MaxUndo) _undo.RemoveAt(0);
        _redo.Clear();
        Dirty = true;
    }

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public bool Undo() => Restore(_undo, _redo);
    public bool Redo() => Restore(_redo, _undo);

    private bool Restore(List<string> from, List<string> to)
    {
        if (from.Count == 0) return false;
        to.Add(Xml.OuterXml);
        string state = from[^1];
        from.RemoveAt(from.Count - 1);
        Xml = Parse(state);
        Dirty = true;
        return true;
    }

    // ---- Structure ----

    public IEnumerable<XmlElement> Layers => Root.ChildNodes.OfType<XmlElement>().Where(e => e.Name == "Layer");

    /// <summary>Elements of a layer the canvas draws: pictures, picture effects, animations, particles, fighters.</summary>
    public static IEnumerable<XmlElement> Items(XmlElement layer) =>
        layer.ChildNodes.OfType<XmlElement>().Where(e => e.Name is "Image" or "SpriteMask" or "SimpleEffect" or "ParticleEffect" or "NewParticleEffect" or "ModelsViewer");

    /// <summary>A picture the canvas can move and size: an image, sprite mask or picture effect.</summary>
    public static bool IsPicture(XmlElement element) =>
        element.Name is "Image" or "SpriteMask" || element.Name == "SimpleEffect" && element.GetAttribute("Type") == "Picture";

    public static bool HasBox(XmlElement element) =>
        IsPicture(element) || element.Name == "SimpleEffect" || element.Name is "ParticleEffect" or "NewParticleEffect";

    public static float Number(XmlElement element, string name, float fallback = 0f) =>
        float.TryParse(element.GetAttribute(name), NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? value : fallback;

    public static void SetNumber(XmlElement element, string name, float value) =>
        element.SetAttribute(name, Format(value));

    public static string Format(float value)
    {
        double rounded = Math.Round(value, 3, MidpointRounding.AwayFromZero);
        if (Math.Abs(rounded) < 0.0005) rounded = 0;
        return rounded.ToString("0.###", CultureInfo.InvariantCulture);
    }

    public float RootNumber(string name, float fallback = 0f) => Number(Root, name, fallback);

    public XmlElement? Layer(XmlElement element)
    {
        for (XmlNode? node = element; node != null; node = node.ParentNode)
            if (node is XmlElement e && e.Name == "Layer") return e;
        return null;
    }

    public XmlElement? FighterLayer => Layers.FirstOrDefault(l => l.GetAttribute("Type") == "2");
}
