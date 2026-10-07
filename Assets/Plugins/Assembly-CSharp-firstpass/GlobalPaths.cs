using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Nekki.Yaml;
using UnityEngine;

public static class GlobalPaths
{
	public struct PathInfo
	{
		public string ResourcePath;

		public string ExternalPath;

		public string BundlePath;

		public string RawPath;

		public bool HasBundlePath;

		public bool HasResourcePath;

		public bool HasExternalPath;

		public static PathInfo Empty = new PathInfo(string.Empty, string.Empty, string.Empty, string.Empty);

		public bool IsEmpty
		{
			get
			{
				return GetIsEmpty();
			}
		}

		public PathInfo(string resourcePath, string externalPath, string bundle, string rawPath)
		{
			if (string.IsNullOrEmpty(resourcePath))
			{
				HasResourcePath = false;
				ResourcePath = string.Empty;
			}
			else
			{
				HasResourcePath = true;
				ResourcePath = resourcePath.Trim('/').Trim('\\');
			}
			if (string.IsNullOrEmpty(externalPath))
			{
				HasExternalPath = false;
				ExternalPath = string.Empty;
			}
			else
			{
				HasExternalPath = true;
				ExternalPath = externalPath.Trim('/').Trim('\\');
			}
			if (string.IsNullOrEmpty(bundle))
			{
				HasBundlePath = false;
				BundlePath = string.Empty;
			}
			else
			{
				HasBundlePath = true;
				BundlePath = bundle.Trim('/').Trim('\\');
			}
			RawPath = rawPath.Trim('/').Trim('\\');
		}

		public bool GetIsEmpty()
		{
			return !HasResourcePath && !HasExternalPath && !HasBundlePath;
		}

		public PathInfo Append(string suffix)
		{
			if (HasResourcePath)
			{
				ResourcePath = ResourcePath + "/" + suffix;
			}
			if (HasExternalPath)
			{
				ExternalPath = ExternalPath + "/" + suffix;
			}
			if (HasBundlePath)
			{
				BundlePath = BundlePath + "/" + suffix;
			}
			return this;
		}

		[SpecialName]
		public static string op_Explicit(PathInfo pathInfo)
		{
			return pathInfo.ExternalPath;
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static YamlDocumentNekki configYaml;

	private static readonly Dictionary<string, string> Pathes;

	private const string PathToResourcesFolder = "gamedata/Resources";

	private const string PathToBundlesFolder = "gamedata/Bundles";

	public static YamlDocumentNekki ConfigYamlDocument
	{
		get
		{
			return GetConfigYaml();
		}
		private set
		{
			set_ConfigYaml(value);
		}
	}

	public static string InternalResourcesPath
	{
		get
		{
			return GetInternalResourcesPath();
		}
	}

	public static string ExternalResourcesPath
	{
		get
		{
			return GetExternalResourcesPath();
		}
	}

	public static string ExternalBundlesPath
	{
		get
		{
			return GetExternalBundlesPath();
		}
	}

	public static string CurrentResolution
	{
		get
		{
			return GetCurrentResolution();
		}
	}

	public static string AssetServerPath
	{
		get
		{
			return GetAssetServerPath();
		}
	}

	static GlobalPaths()
	{
		Pathes = new Dictionary<string, string>();
		Pathes = new Dictionary<string, string>();
	}

	public static YamlDocumentNekki GetConfigYaml()
	{
		return configYaml;
	}

	private static void set_ConfigYaml(YamlDocumentNekki value)
	{
		configYaml = value;
	}

	public static string GetInternalResourcesPath()
	{
		return string.Format("{0}/{1}", Application.dataPath, "gamedata/Resources");
	}

	public static string GetExternalResourcesPath()
	{
		if (SystemProperties.IsMobilePlatform())
		{
			return string.Format("{0}/{1}", Application.persistentDataPath, "gamedata/Resources");
		}
		return string.Format("{0}/{1}", Application.dataPath.Replace("Assets", string.Empty).TrimEnd('/'), "gamedata/Resources");
	}

	public static string GetExternalBundlesPath()
	{
		if (SystemProperties.IsMobilePlatform())
		{
			return string.Format("{0}/{1}", Application.persistentDataPath, "gamedata/Bundles");
		}
		return string.Format("{0}/{1}", Application.dataPath.Replace("Assets", string.Empty).TrimEnd('/'), "gamedata/Bundles");
	}

	public static string GetCurrentResolution()
	{
		return (SystemProperties.GetDeviceInfo().GuiResolution != SystemProperties.PathType.PATH_BIG) ? "768" : "1536";
	}

	public static string GetGameDataPath()
	{
		if (Application.isMobilePlatform)
		{
			return Application.persistentDataPath;
		}
		if (Application.isEditor)
		{
			return Environment.CurrentDirectory + Path.DirectorySeparatorChar + "gamedata";
		}
		return Application.dataPath + Path.DirectorySeparatorChar + "gamedata";
	}

	public static string GetAssetServerPath()
	{
		return GetPath("AssetServer");
	}

	public static string GetAbsolutePath(string name)
	{
		if (Pathes.ContainsKey(name))
		{
			return Pathes[name].Replace("EXTERNAL_PATH", GetExternalResourcesPath()).Replace("CURRENT_RESOLUTION", GetCurrentResolution()).TrimEnd('/')
				.Trim('\\');
		}
		AdvLog.LogError(string.Format("path not found: {0}", name));
		return string.Empty;
	}

	public static string GetPath(string name)
	{
		if (Pathes.ContainsKey(name))
		{
			return Pathes[name].Replace("EXTERNAL_PATH", GetExternalResourcesPath()).Replace("CURRENT_RESOLUTION", GetCurrentResolution()).Trim('/')
				.Trim('\\');
		}
		AdvLog.LogError(string.Format("path not found: {0}", name));
		return string.Empty;
	}

	public static string GetRawPath(string name)
	{
		if (Pathes.ContainsKey(name))
		{
			return Pathes[name].Trim('/').Trim('\\');
		}
		AdvLog.LogError(string.Format("path not found: {0}", name));
		return string.Empty;
	}

	public static PathInfo GetPathInfo(string name)
	{
		if (Pathes.ContainsKey(name))
		{
			string text = Pathes[name];
			return new PathInfo(text.Replace("EXTERNAL_PATH/", string.Empty).Replace("CURRENT_RESOLUTION/", string.Empty), text.Replace("EXTERNAL_PATH", GetExternalResourcesPath()).Replace("CURRENT_RESOLUTION", GetCurrentResolution()), text.Replace("EXTERNAL_PATH/", string.Empty).Replace("CURRENT_RESOLUTION/", GetCurrentResolution()), text);
		}
		AdvLog.LogError(string.Format("path not found: {0}", name));
		return PathInfo.Empty;
	}

	public static PathInfo CreateExternalPathInfo(string path)
	{
		path = "EXTERNAL_PATH/" + path;
		return new PathInfo(path.Replace("EXTERNAL_PATH/", string.Empty).Replace("CURRENT_RESOLUTION/", string.Empty), path.Replace("EXTERNAL_PATH", GetExternalResourcesPath()).Replace("CURRENT_RESOLUTION", GetCurrentResolution()), path.Replace("EXTERNAL_PATH/", string.Empty).Replace("CURRENT_RESOLUTION/", GetCurrentResolution()), path);
	}
}
