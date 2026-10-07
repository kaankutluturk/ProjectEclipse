using System.Globalization;

namespace Eclipse.LocationParamsEditor;

internal static class Program
{
    /// <summary>
    /// LocationParamsEditor [params-file]
    /// LocationParamsEditor --render params-file images-folder out.png [scale] [extra-folder...]
    ///     [--viewport 21:9] [--distance units] [--camera units]
    /// </summary>
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length >= 4 && args[0] == "--render")
        {
            try
            {
                var document = ParamsDocument.Load(args[1]);
                using var images = new ImageLibrary();
                images.SetFolder(args[2]);
                var options = new SceneOptions();
                float scale = 0.5f;
                for (int i = 4; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (arg == "--viewport" && i + 1 < args.Length) { options.Viewport = true; options.Aspect = ParseAspect(args[++i]); }
                    else if (arg == "--distance" && i + 1 < args.Length) options.FighterDistance = float.Parse(args[++i], CultureInfo.InvariantCulture);
                    else if (arg == "--camera" && i + 1 < args.Length) options.CameraX = float.Parse(args[++i], CultureInfo.InvariantCulture);
                    else if (i == 4 && float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out float s)) scale = s;
                    else images.ExtraFolders.Add(arg);
                }
                SceneRenderer.RenderToFile(document, images, options, args[3], scale);
                return 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(args[3] + ".error.txt", ex.ToString());
                return 1;
            }
        }
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(args.Length > 0 && File.Exists(args[0]) ? args[0] : null));
        return 0;
    }

    /// <summary>"21:9" or a plain ratio such as "2.333".</summary>
    private static float ParseAspect(string text)
    {
        var parts = text.Split(':');
        return parts.Length == 2 ? float.Parse(parts[0], CultureInfo.InvariantCulture) / float.Parse(parts[1], CultureInfo.InvariantCulture)
            : float.Parse(text, CultureInfo.InvariantCulture);
    }
}
