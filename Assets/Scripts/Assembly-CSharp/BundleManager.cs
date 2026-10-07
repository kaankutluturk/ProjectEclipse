using System.Collections.Generic;
using UnityEngine;

public static class BundleManager
{
	private static Dictionary<string, BundleRef> _bundlesByAssetName = new Dictionary<string, BundleRef>();

	private static BundleRef _currentBundle = null;

	public static void AddBundle(string name)
	{
		BundleRef bundle = new BundleRef(name);
		bundle.Load();
		if (bundle.GetIsLoaded())
		{
			string[] array = bundle.GetAllAssetNames();
			string[] array2 = array;
			foreach (string assetName in array2)
			{
				string key = AssetBundleExtension.GetSimplifiedAssetName(assetName);
				_bundlesByAssetName[key] = bundle;
			}
		}
		bundle.Unload();
	}

	public static T LoadAsset<T>(string name) where T : Object
	{
		name = AssetBundleExtension.GetSimplifiedAssetName(name);
		BundleRef value = null;
		if (!_bundlesByAssetName.TryGetValue(name, out value))
		{
			return (T)null;
		}
		ActivateBundle(value);
		return (!value.GetIsLoaded()) ? ((T)null) : value.LoadAsset<T>(name);
	}

	public static T[] LoadAssetWithSubAssets<T>(string name) where T : Object
	{
		name = AssetBundleExtension.GetSimplifiedAssetName(name);
		BundleRef value = null;
		if (!_bundlesByAssetName.TryGetValue(name, out value))
		{
			return null;
		}
		ActivateBundle(value);
		return (!value.GetIsLoaded()) ? null : value.LoadAssetWithSubAssets<T>(name);
	}

	private static void ActivateBundle(BundleRef bundle)
	{
		if (bundle != null && _currentBundle != bundle)
		{
			if (_currentBundle != null)
			{
				_currentBundle.Unload();
			}
			bundle.Load();
			if (bundle.GetIsLoaded())
			{
				_currentBundle = bundle;
			}
		}
	}

	public static void Reset()
	{
		if (_currentBundle != null)
		{
			_currentBundle.Unload();
			_currentBundle = null;
		}
		_bundlesByAssetName.Clear();
	}
}
