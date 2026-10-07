using System.IO;
using UnityEngine;

public static class AssetBundleExtension
{
	public static string GetNormalizedPngPath(string path)
	{
		if (!path.Contains(SF2Paths.GetGuiResourcesRoot()))
		{
			path = string.Format("{0}/{1}.png", SF2Paths.GetGuiResourcesRoot(), path);
		}
		return path.ToLower();
	}

	public static string GetSimplifiedAssetName(string assetPath)
	{
		return Path.GetFileNameWithoutExtension(assetPath.ToLower());
	}

	public static string[] GetAllSimplifiedAssetNames(this AssetBundle bundle)
	{
		string[] allAssetNames = bundle.GetAllAssetNames();
		string[] array = new string[allAssetNames.Length];
		int i = 0;
		for (int num = allAssetNames.Length; i < num; i++)
		{
			array[i] = GetSimplifiedAssetName(allAssetNames[i]);
		}
		return array;
	}
}
