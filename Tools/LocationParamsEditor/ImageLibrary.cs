using System.Drawing;
using System.Xml;

namespace Eclipse.LocationParamsEditor;

/// <summary>
/// Finds the picture files a layout names. Pictures of a layer without Path come from the
/// images folder; a layer with Path="Locations/&lt;name&gt;/" draws another location's art, looked
/// up in a folder named &lt;name&gt; (or &lt;name&gt;_new) beside the images folder or in an extra
/// search folder. Loaded bitmaps are cached until the folders change.
/// </summary>
public sealed class ImageLibrary : IDisposable
{
    private static readonly string[] Extensions = { ".png", ".jpg", ".jpeg" };
    private readonly Dictionary<string, Bitmap?> _bitmaps = new(StringComparer.OrdinalIgnoreCase);

    public string? Folder { get; private set; }
    public List<string> ExtraFolders { get; } = new();

    public void SetFolder(string? folder)
    {
        Folder = folder;
        Clear();
    }

    public void Clear()
    {
        foreach (var bitmap in _bitmaps.Values) bitmap?.Dispose();
        _bitmaps.Clear();
    }

    public void Dispose() => Clear();

    /// <summary>Folders a layer's pictures may come from, most specific first.</summary>
    public IEnumerable<string> FoldersFor(XmlElement? layer)
    {
        string path = layer?.GetAttribute("Path") ?? string.Empty;
        string? other = null;
        var parts = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && parts[0].Equals("Locations", StringComparison.OrdinalIgnoreCase)) other = parts[1];
        if (other != null)
        {
            var bases = new List<string>();
            if (Folder != null) { bases.Add(Folder); string? parent = System.IO.Path.GetDirectoryName(Folder); if (parent != null) bases.Add(parent); }
            bases.AddRange(ExtraFolders);
            foreach (string root in bases)
                foreach (string name in new[] { other, other + "_new" })
                {
                    string candidate = System.IO.Path.Combine(root, name);
                    if (Directory.Exists(candidate)) yield return candidate;
                }
            foreach (string extra in ExtraFolders) yield return extra;
            yield break;
        }
        if (Folder != null) yield return Folder;
        foreach (string extra in ExtraFolders) yield return extra;
    }

    /// <summary>The file for a picture name in a layer, or null.</summary>
    public string? Find(XmlElement? layer, string className)
    {
        if (string.IsNullOrEmpty(className) || className.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0) return null;
        foreach (string folder in FoldersFor(layer))
            foreach (string extension in Extensions)
            {
                string file = System.IO.Path.Combine(folder, className + extension);
                if (File.Exists(file)) return file;
            }
        return null;
    }

    public Bitmap? Load(XmlElement? layer, string className)
    {
        string? file = Find(layer, className);
        if (file == null) return null;
        if (_bitmaps.TryGetValue(file, out var cached)) return cached;
        Bitmap? bitmap = null;
        try
        {
            using var stream = File.OpenRead(file);
            using var image = Image.FromStream(stream);
            bitmap = new Bitmap(image);
        }
        catch (Exception) { bitmap = null; }
        _bitmaps[file] = bitmap;
        return bitmap;
    }

    /// <summary>Picture names (file names without extension) in the images folder.</summary>
    public List<string> Names()
    {
        if (Folder == null || !Directory.Exists(Folder)) return new List<string>();
        return Directory.EnumerateFiles(Folder)
            .Where(f => Extensions.Contains(System.IO.Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .Select(f => System.IO.Path.GetFileNameWithoutExtension(f))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
