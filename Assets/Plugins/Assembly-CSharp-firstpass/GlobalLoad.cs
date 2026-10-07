using System;
using Newtonsoft.Json;
using UnityEngine;

public class GlobalLoad : GlobalPath
{
	private static Sprite noImageSprite;

	public static Sprite NoImageSprite
	{
		get
		{
			return GetNoImageSprite();
		}
	}

	public static Texture2D NoImageTexture
	{
		get
		{
			return GetNoImageTexture();
		}
	}

	public static Sprite GetNoImageSprite()
	{
		if (noImageSprite == null)
		{
			noImageSprite = GetLoadObject<Sprite>(InternalSettings.NoImageTexture);
		}
		return noImageSprite;
	}

	public static Texture2D GetNoImageTexture()
	{
		return GetNoImageSprite().texture;
	}

	public static GameObject GetLoadGameObjectInstanceInternal(string key, string name = "")
	{
		GameObject prefab = GetLoadGameObjectInternal(key, name);
		return GetGameObjectInstance(prefab);
	}

	public static GameObject GetLoadGameObjectInstance(string path)
	{
		GameObject prefab = GetLoadGameObject(path);
		return GetGameObjectInstance(prefab);
	}

	private static GameObject GetGameObjectInstance(GameObject prefab)
	{
		if (prefab != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
			gameObject.name = gameObject.name.Replace("(Clone)", string.Empty);
			return gameObject;
		}
		return null;
	}

	public static GameObject GetLoadGameObjectInternal(string key, string name = "")
	{
		return GetLoadObjectInternal<GameObject>(key, name);
	}

	public static GameObject GetLoadGameObject(string path)
	{
		return GetLoadObject<GameObject>(path);
	}

	public static AudioClip GetLoadAudioClipInternal(string key, string name = "")
	{
		return GetLoadObjectInternal<AudioClip>(key, name);
	}

	public static AudioClip GetLoadAudioClip(string path)
	{
		return GetLoadObject<AudioClip>(path);
	}

	public static Texture2D GetLoadTexture2DInternal(string key, string name = "")
	{
		Texture2D texture = GetLoadObjectInternal<Texture2D>(key, name);
		return ObjecOrDefault(texture, GetNoImageTexture());
	}

	public static Texture2D GetLoadTexture2D(string path)
	{
		Texture2D texture = GetLoadObject<Texture2D>(path);
		return ObjecOrDefault(texture, GetNoImageTexture());
	}

	public static Sprite GetLoadSpriteInternal(string key, string name = "")
	{
		Sprite loadedSprite = GetLoadObjectInternal<Sprite>(key, name);
		return ObjecOrDefault(loadedSprite, GetNoImageSprite());
	}

	public static Sprite GetLoadSprite(string path)
	{
		Sprite loadedSprite = GetLoadObject<Sprite>(path);
		return ObjecOrDefault(loadedSprite, GetNoImageSprite());
	}

	public static Sprite GetLoadSpriteFromTextureInternal(string key, string name = "")
	{
		Texture2D texture = GetLoadTexture2DInternal(key, name);
		return TexturesUtils.CreateSprite(texture);
	}

	public static Sprite GetLoadSpriteFromTexture(string path)
	{
		Texture2D texture = GetLoadTexture2D(path);
		return TexturesUtils.CreateSprite(texture);
	}

	public static Sprite GetLoadSpriteFromAtlas(string atlasPath, string atlasName, string spriteName)
	{
		return TexturesUtils.GetSpriteFromAtlas(atlasPath, atlasName, spriteName);
	}

	public static Sprite GetLoadSpriteFromAtlas(string atlasPath, string spriteName)
	{
		return TexturesUtils.GetSpriteFromAtlas(atlasPath, spriteName);
	}

	public static byte[] GetLoadBytesInternal(string key, string name = "")
	{
		TextAsset textAsset = GetLoadObjectInternal<TextAsset>(key, name);
		return (!(textAsset == null)) ? textAsset.bytes : null;
	}

	public static byte[] GetLoadBytes(string path)
	{
		TextAsset textAsset = GetLoadObject<TextAsset>(path);
		return (!(textAsset == null)) ? textAsset.bytes : null;
	}

	public static string GetLoadTextInternal(string key, string name = "")
	{
		TextAsset textAsset = GetLoadObjectInternal<TextAsset>(key, name);
		return (!(textAsset == null)) ? textAsset.text : null;
	}

	public static string GetLoadText(string path)
	{
		TextAsset textAsset = GetLoadObject<TextAsset>(path);
		return (!(textAsset == null)) ? textAsset.text : null;
	}

	public static T GetLoadJsonInternal<T>(string key, string name = "") where T : class
	{
		string json = GetLoadTextInternal(key, name);
		return DeserializeJson<T>(json);
	}

	public static T GetLoadJson<T>(string path) where T : class
	{
		string json = GetLoadText(path);
		return DeserializeJson<T>(json);
	}

	private static T DeserializeJson<T>(string json) where T : class
	{
		if (!json.IsNullOrEmpty())
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(json);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error GetLoadJson [" + ex.Message + "]");
			}
		}
		return (T)null;
	}

	private static T ObjecOrDefault<T>(T loaded, T fallback = null) where T : UnityEngine.Object
	{
		return loaded ?? fallback;
	}

	public static T[] GetLoadObjectsInternal<T>(string key, string name = "") where T : UnityEngine.Object
	{
		return LoadAll<T>(GlobalPath.GetInternalPath(key, name));
	}

	public static T[] GetLoadObjects<T>(string path) where T : UnityEngine.Object
	{
		return LoadAll<T>(GlobalPath.GetLoaderPath(path));
	}

	public static T GetLoadObjectInternal<T>(string key, string name = "") where T : UnityEngine.Object
	{
		return Load<T>(GlobalPath.GetInternalPath(key, name));
	}

	public static T GetLoadObject<T>(string path) where T : UnityEngine.Object
	{
		return Load<T>(GlobalPath.GetLoaderPath(path));
	}

	private static TResult FirstNonNull<TResult, Arg>(Arg argument, params Func<Arg, TResult>[] loaders)
	{
		foreach (Func<Arg, TResult> func in loaders)
		{
			TResult val = func(argument);
			if (val != null)
			{
				return val;
			}
		}
		return default(TResult);
	}

	private static T[] LoadAll<T>(string path) where T : UnityEngine.Object
	{
		T[] array = FirstNonNull<T[], string>(path, GetBundleObjects<T>, GetResources<T>);
		if (array == null)
		{
			Debug.LogError("LoadObject Not Found - " + path);
		}
		return array;
	}

	private static T Load<T>(string path) where T : UnityEngine.Object
	{
		T val = FirstNonNull<T, string>(path, GetBundleObject<T>, GetResource<T>);
		if (val == null)
		{
			Debug.LogError("LoadObject Not Found - " + path);
		}
		return val;
	}

	private static T[] GetResources<T>(string path) where T : UnityEngine.Object
	{
		return ResourcesUtil.GetResources<T>(path);
	}

	private static T GetResource<T>(string path) where T : UnityEngine.Object
	{
		return ResourcesUtil.GetResource<T>(path);
	}

	private static T[] GetBundleObjects<T>(string path) where T : UnityEngine.Object
	{
		return BundlesUtil.GetObjects<T>(path);
	}

	private static T GetBundleObject<T>(string path) where T : UnityEngine.Object
	{
		return BundlesUtil.GetObject<T>(path);
	}

	public static void Unload(UnityEngine.Object asset, bool immediate = true)
	{
		if (!(asset == null))
		{
#if UNITY_6000_0_OR_NEWER
			// Same value Object.GetInstanceID() returns in Unity 6: the low 32 bits of the EntityId. The legacy sign test still holds.
			if (unchecked((int)EntityId.ToULong(asset.GetEntityId())) <= 0)
#else
			if (asset.GetInstanceID() <= 0)
#endif
			{
				DestroyObject(asset, immediate);
			}
			else
			{
				ResourcesUtil.UnloadAsset(asset, immediate);
			}
		}
	}

	public static void DestroyObject(UnityEngine.Object obj, bool immediate = true)
	{
		try
		{
			if (!immediate)
			{
				UnityEngine.Object.Destroy(obj);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(obj);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void UnloadUnusedAssets()
	{
		BundlesUtil.UnloadUnusedAssets();
		ResourcesUtil.UnloadUnusedAssets();
	}

	public static void CollectGarbage()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	public static string GetFileOrResourcesText(string path, string key, string name = "")
	{
		string text = FileUtils.ReadAllText(path);
		if (!text.IsNullOrEmpty())
		{
			return text;
		}
		return GetLoadTextInternal(key, name);
	}
}
