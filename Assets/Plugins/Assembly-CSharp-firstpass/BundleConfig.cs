using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class BundleConfig
{
	public const string ConfigName = "bundlesConfig.json";

	public Dictionary<string, BundleData> Bundles = new Dictionary<string, BundleData>();

	public Dictionary<string, AssetsData> Assets = new Dictionary<string, AssetsData>();

	public string BundlesPath;

	public static BundleConfig Load(string path)
	{
		return FileUtils.ReadFileJson<BundleConfig>(path);
	}

	public List<string> GetAllDependencies()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, BundleData> bundle in Bundles)
		{
			string[] dependencies = bundle.Value.Dependencies;
			foreach (string item in dependencies)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public void AddBundleData(string name, BundleData data)
	{
		if (!Bundles.ContainsKey(name))
		{
			Bundles.Add(name, data);
		}
	}

	public void AddAssetsData(string path, AssetsData data)
	{
		string key = NormalizeAssetPath(path);
		if (!Assets.ContainsKey(key))
		{
			Assets.Add(key, data);
		}
	}

	public void SetAvailable(string name, bool value)
	{
		if (Bundles.ContainsKey(name))
		{
			Bundles[name].Available = value;
		}
	}

	public void Save(string path)
	{
		string json = JsonConvert.SerializeObject(this, Formatting.Indented);
		FileUtils.WriteText(path, json);
	}

	public bool Equal(Hash128 hash)
	{
		return Equal(hash.ToString());
	}

	public bool Equal(string hash)
	{
		foreach (KeyValuePair<string, BundleData> bundle in Bundles)
		{
			if (bundle.Value.Hash.Equals(hash))
			{
				return true;
			}
		}
		return false;
	}

	public AssetsData GetAssetsData(string path)
	{
		path = NormalizeAssetPath(path);
		if (Assets.ContainsKey(path))
		{
			return Assets[path];
		}
		return null;
	}

	private string NormalizeAssetPath(string path)
	{
		int length = GlobalPath.GetIndexExtension(path);
		return path.Substring(0, length).ToLower();
	}

	public string GetBundlePath(string name)
	{
		if (Bundles.ContainsKey(name) && Bundles[name].Available)
		{
			return BundlesPath + name;
		}
		return null;
	}

	public override string ToString()
	{
		string text = string.Empty;
		foreach (KeyValuePair<string, BundleData> bundle in Bundles)
		{
			text = string.Concat(text, bundle, "\n");
		}
		return text;
	}
}
