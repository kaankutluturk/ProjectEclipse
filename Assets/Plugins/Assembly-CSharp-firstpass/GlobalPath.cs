using System;
using System.IO;
using UnityEngine;

public class GlobalPath
{
	private const string PathToResourcesFolder = "Resources";

	public static readonly string PathToLoaderFolder = "assets/gamedata/" + "Resources".ToLower();

	public static string LogsPath
	{
		get
		{
			return GetLogsPath();
		}
	}

	public static string ExternalResourcesPath
	{
		get
		{
			return GetExternalResourcesPath();
		}
	}

	public static string InternalResourcesPath
	{
		get
		{
			return GetInternalResourcesPath();
		}
	}

	public static string ExternalGameDataPath
	{
		get
		{
			return GetExternalGameDataPath();
		}
	}

	public static string InternalGameDataPath
	{
		get
		{
			return GetInternalGameDataPath();
		}
	}

	public static string GetLogsPath()
	{
		return GetExternalGameDataPath() + "/Logs";
	}

	public static string GetExternalResourcesPath()
	{
		return GetExternalGameDataPath() + "/Resources";
	}

	public static string GetInternalResourcesPath()
	{
		return GetInternalGameDataPath() + "/Resources";
	}

	public static string GetExternalGameDataPath()
	{
		if (SystemProperties.IsMobilePlatform())
		{
			return Application.persistentDataPath + "/gamedata";
		}
		string text = Application.dataPath.Replace("Assets", string.Empty).TrimEnd('/');
		return text + "/gamedata";
	}

	public static string GetInternalGameDataPath()
	{
		return Application.dataPath + "/gamedata";
	}

	public static int GetIndexExtension(string path, int offset = 0)
	{
		int num = path.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);
		return (num <= -1) ? (path.Length - offset) : (num - offset);
	}

	public static string CombineExternalGameDataPath(string path)
	{
		return Path.Combine(GetExternalGameDataPath(), path);
	}

	private static string GetExternalPathByKey(string key)
	{
		return InternalSettings.GetExternalPath(key);
	}

	public static string GetInternalPath(string key, string name = "")
	{
		return GetExternalPathByKey(key) + name;
	}

	public static string GetLoaderPath(string path)
	{
		return PathToLoaderFolder + "/" + path;
	}
}
