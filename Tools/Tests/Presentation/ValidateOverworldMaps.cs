// Native overworld checks shared by the focused and full packaged-art fixtures.
using System;
using System.IO;
using System.Linq;
using Eclipse.Content;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ValidateOverworldMaps
{
    private static int checks;
    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidDataException(message);
        checks++;
    }

    public static int Check()
    {
        string[] mapNames = { "Map1.1", "Map1.2", "Map1.3", "Map2.4", "Map2.5", "Map2.6", "Map3.7", "7" };
        foreach (string mapName in mapNames)
        {
            Sprite map = ResourcesAndBundles.Load<Sprite>("UI/zones/" + mapName);
            Require(map != null && map.name == mapName && map.texture.width == 2040 && map.texture.height == 972 &&
                map.rect == new Rect(0, 0, 2040, 972) && Mathf.Abs(map.bounds.size.x - 13.65f) < 0.001f,
                "Default overworld map failed through the core resolver: " + mapName);
            Require(map.vertices.Length == 4 && map.triangles.Length == 6 &&
                map.uv.Any(value => value == Vector2.zero) && map.uv.Any(value => value == Vector2.one),
                "Overworld map does not cover its complete source PNG: " + mapName);
            if (mapName == "7") continue;
            string atlasName = mapName.Substring(0, mapName.IndexOf('.'));
            Sprite low = PackagedArtCatalog.LoadWithSubAssets<Sprite>("UI/zones/" + atlasName + "_low")
                .FirstOrDefault(value => value.name == mapName);
            Require(low != null && low.texture.width == 2040 && low.texture.height == 972 &&
                Mathf.Abs(low.bounds.size.x - 6.83f) < 0.001f,
                "Legacy low-quality overworld map failed: " + mapName);
        }
        return checks;
    }

#if UNITY_EDITOR
    public static void Run()
    {
        try
        {
            PlayerSettings.companyName = "EclipseTests";
            PlayerSettings.productName = "OverworldContentSmoke";
            PackagedArtCatalog.ValidateProjectFiles(Application.dataPath);
            Debug.Log("[OverworldMaps] PASS: " + Check() + " native checks.");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogError("[OverworldMaps] " + exception);
            EditorApplication.Exit(1);
        }
    }
#endif
}
