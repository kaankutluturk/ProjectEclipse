using System.Collections.Generic;
using UnityEngine;

public class BundlesUtil
{
	private static readonly Dictionary<string, AssetBundle> bundlesChach = new Dictionary<string, AssetBundle>();

	private static readonly List<AssetBundle> bundlesDependencies = new List<AssetBundle>();

	private static BundleConfig currentConfig;

	public static void InitConfig(BundleConfig config)
	{
		currentConfig = config;
		UnloadDependencies();
		LoadDependencies();
	}

	private static void UnloadDependencies()
	{
		foreach (AssetBundle item in bundlesDependencies)
		{
			item.Unload(true);
		}
		bundlesDependencies.Clear();
	}

	private static void LoadDependencies()
	{
		List<string> list = currentConfig.GetAllDependencies();
		foreach (string item in list)
		{
			AssetBundle assetBundle = GetAssetBundle(item);
			if ((bool)assetBundle)
			{
				bundlesDependencies.Add(assetBundle);
			}
		}
	}

	public static T[] GetObjects<T>(string path) where T : Object
	{
		AssetBundle assetBundle = GetAssetBundleSafe(ref path);
		if (assetBundle == null)
		{
			return null;
		}
		return assetBundle.LoadAssetWithSubAssets<T>(path);
	}

	public static T GetObject<T>(string path) where T : Object
	{
		AssetBundle assetBundle = GetAssetBundleSafe(ref path);
		if (assetBundle == null)
		{
			return (T)null;
		}
		return assetBundle.LoadAsset<T>(path);
	}

	private static AssetBundle GetAssetBundleSafe(ref string path)
	{
		if (currentConfig != null)
		{
			AssetsData assetsData = currentConfig.GetAssetsData(path);
			if (!path.IsNullOrEmpty() && assetsData != null)
			{
				AssetBundle result = GetAssetBundle(assetsData.BundleName);
				path = assetsData.Path;
				return result;
			}
		}
		return null;
	}

	public static void UnloadAsset(Object asset)
	{
		ResourcesUtil.UnloadAsset(asset);
	}

	public static void UnloadUnusedAssets()
	{
	}

	public static AssetBundle GetAssetBundle(string name)
	{
		if (bundlesChach.ContainsKey(name))
		{
			return bundlesChach[name];
		}
		if (currentConfig != null)
		{
			string text = currentConfig.GetBundlePath(name);
			if (!text.IsNullOrEmpty())
			{
				AssetBundle assetBundle = AssetBundle.LoadFromFile(text);
				if ((bool)assetBundle)
				{
					bundlesChach.Add(name, assetBundle);
				}
				return assetBundle;
			}
		}
		return null;
	}
}
