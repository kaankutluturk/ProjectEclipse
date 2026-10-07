using System.IO;
using UnityEngine;

public class BundleRef
{
	private string _BundleId;

	private AssetBundle _UnityBundle;

	public string FilePath
	{
		get
		{
			return GetFilePath();
		}
	}

	public string BundleId
	{
		get
		{
			return GetBundleId();
		}
	}

	public bool IsLoaded
	{
		get
		{
			return GetIsLoaded();
		}
	}

	public BundleRef(string BKKKEENLEDP)
	{
		_BundleId = BKKKEENLEDP;
	}

	public string GetFilePath()
	{
		return SF2Paths.GetBundlesPath() + "/" + _BundleId;
	}

	public string GetBundleId()
	{
		return _BundleId;
	}

	public bool GetIsLoaded()
	{
		return _UnityBundle != null;
	}

	public string[] GetAllAssetNames()
	{
		if (_UnityBundle != null)
		{
			return _UnityBundle.GetAllAssetNames();
		}
		return new string[0];
	}

	public void Load()
	{
		if (!(_UnityBundle != null) && File.Exists(GetFilePath()))
		{
			_UnityBundle = AssetBundle.LoadFromFile(GetFilePath());
		}
	}

	public void Unload()
	{
		if (!(_UnityBundle == null))
		{
			_UnityBundle.Unload(false);
			_UnityBundle = null;
		}
	}

	public T LoadAsset<T>(string JHEMALDDIFN) where T : Object
	{
		if (_UnityBundle == null)
		{
			return (T)null;
		}
		return _UnityBundle.LoadAsset<T>(JHEMALDDIFN);
	}

	public T[] LoadAssetWithSubAssets<T>(string JHEMALDDIFN) where T : Object
	{
		if (_UnityBundle == null)
		{
			return null;
		}
		return _UnityBundle.LoadAssetWithSubAssets<T>(JHEMALDDIFN);
	}
}
