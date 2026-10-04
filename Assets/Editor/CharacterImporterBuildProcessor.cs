using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Ships the Blender character importer beside desktop player data, where
// Eclipse.Modding.CharacterImporter.ScriptsDirectory looks for it.
public sealed class CharacterImporterBuildProcessor : IPostprocessBuildWithReport
{
    private static readonly string[] Scripts =
        { "ImportCharacter.py", "CharacterPipeline.py", "PackageCharacter.py", "HumanoidMotion.py", "RigMapping.py", "SkinPreview.py" };

    public int callbackOrder => 110;

    public void OnPostprocessBuild(BuildReport report)
    {
        string output = report.summary.outputPath;
        string data;
        switch (report.summary.platform)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneLinux64:
                data = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + "_Data");
                break;
            case BuildTarget.StandaloneOSX:
                data = Path.Combine(output, "Contents");
                break;
            default:
                return;
        }
        string source = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Tools", "Animation"));
        string target = Path.Combine(data, "CharacterImport");
        Directory.CreateDirectory(target);
        foreach (string script in Scripts)
            File.Copy(Path.Combine(source, script), Path.Combine(target, script), true);
        Debug.Log("[CharacterImport] Copied importer scripts to " + target);
    }
}
