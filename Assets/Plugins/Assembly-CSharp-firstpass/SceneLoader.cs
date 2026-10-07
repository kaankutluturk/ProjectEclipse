using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class SceneLoader : ExtentionBehaviour
{
	public const int OnSceneLoadDone = 0;

	public const int OnConfigLoadDone = 1;

	public const string Server = "http://127.0.0.1";

	private static SceneLoader instance;

	private static readonly Dictionary<string, AssetBundle> CachedScenes = new Dictionary<string, AssetBundle>();

	private static readonly Dictionary<string, SceneConfig> CachedConfigs = new Dictionary<string, SceneConfig>();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static string datapath;

	public const string DataFolderName = "Export";

	private static bool _inited;

	public static SceneLoader CurrentInstance
	{
		get
		{
			return get_Instance();
		}
	}

	public static string DataDirectory
	{
		get
		{
			return get_Datapath();
		}
		private set
		{
			set_Datapath(value);
		}
	}

	public static SceneLoader get_Instance()
	{
		if (!instance)
		{
			SceneLoader sceneLoader = Object.FindObjectOfType<SceneLoader>();
			if (!sceneLoader)
			{
				instance = new GameObject("_sceneLoader").AddComponent<SceneLoader>();
				StaticObjectsManager.AddObject(instance.get_gameObject(), false);
			}
			else
			{
				instance = sceneLoader;
			}
		}
		return instance;
	}

	public static string get_Datapath()
	{
		return datapath;
	}

	private static void set_Datapath(string value)
	{
		datapath = value;
	}

	private static void Init()
	{
		string arg = ((!SystemProperties.IsMobilePlatform()) ? Application.dataPath : Application.persistentDataPath);
		set_Datapath(string.Format("{0}/{1}", arg, "Export"));
		if (!Directory.Exists(get_Datapath()))
		{
			Directory.CreateDirectory(get_Datapath());
		}
	}

	internal void Start()
	{
		if (_inited)
		{
			Object.Destroy(get_gameObject());
			return;
		}
		SF2DisplayFrameRate.Apply();
		_inited = true;
		Init();
		instance = this;
		StaticObjectsManager.AddObject(instance.get_gameObject(), false);
	}

	public static bool HasInstance()
	{
		return instance != null;
	}

	private static string GetPlatformFolder()
	{
		switch (Application.platform)
		{
		case RuntimePlatform.OSXEditor:
			return "standaloneMacOSX";
		case RuntimePlatform.OSXPlayer:
			return "standaloneMacOSX";
		case RuntimePlatform.WindowsPlayer:
			return "standaloneWindows";
		case RuntimePlatform.WindowsEditor:
			return "standaloneWindows";
		case RuntimePlatform.IPhonePlayer:
			return "ios";
		case RuntimePlatform.Android:
			return "android";
		default:
			return "standaloneWindows";
		}
	}

	public static void GetScene(string sceneName)
	{
		if (CachedScenes.ContainsKey(sceneName))
		{
			get_Instance().callEvent(0, Object.Instantiate(CachedScenes[sceneName].mainAsset));
		}
		else if (File.Exists(string.Format("{0}/{1}/SceneRoot_{2}.ab", get_Datapath(), GetPlatformFolder(), sceneName)))
		{
			get_Instance().StartCoroutine(LoadSceneBundle(sceneName));
		}
		else
		{
			get_Instance().StartCoroutine(LoadLocalScene(sceneName));
		}
		if (CachedConfigs.ContainsKey(sceneName))
		{
			get_Instance().callEvent(1, CachedConfigs[sceneName]);
		}
		else if (File.Exists(string.Format("{0}/{1}/SceneRoot_{2}_config.ab", get_Datapath(), GetPlatformFolder(), sceneName)))
		{
			get_Instance().StartCoroutine(LoadConfigBundle(sceneName));
		}
		else
		{
			get_Instance().StartCoroutine(LoadLocalConfig(sceneName));
		}
	}

	private static IEnumerator LoadConfigBundle(string sceneName)
	{
		string text = string.Format("file:///{0}/{1}/SceneRoot_{2}_config.ab", get_Datapath().Replace("\\", "/"), GetPlatformFolder(), sceneName);
		UnityEngine.Debug.Log(text);
		WWW wWW = new WWW(text);
		yield return wWW;
		if (string.IsNullOrEmpty(wWW.error))
		{
			if (!CachedConfigs.ContainsKey(sceneName))
			{
				CachedConfigs.Add(sceneName, ((GameObject)wWW.assetBundle.mainAsset).GetComponent<SceneConfig>());
			}
			else
			{
				CachedConfigs[sceneName] = ((GameObject)wWW.assetBundle.mainAsset).GetComponent<SceneConfig>();
			}
			get_Instance().callEvent(1, CachedConfigs[sceneName]);
		}
		else
		{
			get_Instance().LogError(string.Format("cant load scene config {0}: {1}", sceneName, wWW.error));
		}
	}

	private static IEnumerator LoadSceneBundle(string sceneName)
	{
		string text = string.Format("file:///{0}/{1}/SceneRoot_{2}.ab", get_Datapath().Replace("\\", "/"), GetPlatformFolder(), sceneName);
		UnityEngine.Debug.Log(text);
		WWW wWW = new WWW(text);
		yield return wWW;
		if (string.IsNullOrEmpty(wWW.error))
		{
			if (!CachedScenes.ContainsKey(sceneName))
			{
				CachedScenes.Add(sceneName, wWW.assetBundle);
			}
			else
			{
				CachedScenes[sceneName] = wWW.assetBundle;
			}
			get_Instance().callEvent(0, Object.Instantiate(CachedScenes[sceneName].mainAsset));
		}
		else
		{
			get_Instance().LogError(string.Format("cant load scene {0}: {1}", sceneName, wWW.error));
		}
	}

	private static IEnumerator LoadLocalScene(string sceneName)
	{
		GameObject gameObject = GlobalLoad.GetLoadGameObject(string.Format("Export/SceneRoot_{0}", sceneName));
		if ((bool)gameObject)
		{
			get_Instance().callEvent(0, Object.Instantiate(gameObject));
		}
		else
		{
			get_Instance().LogError(string.Format("cant load local scene {0}", sceneName));
		}
		yield break;
	}

	private static IEnumerator LoadLocalConfig(string sceneName)
	{
		GameObject gameObject = GlobalLoad.GetLoadGameObject(string.Format("Export/SceneRoot_{0}_config", sceneName));
		if ((bool)gameObject)
		{
			get_Instance().callEvent(0, Object.Instantiate(gameObject));
		}
		else
		{
			get_Instance().LogWarning(string.Format("cant load local scene config {0}", sceneName));
		}
		yield break;
	}
}
