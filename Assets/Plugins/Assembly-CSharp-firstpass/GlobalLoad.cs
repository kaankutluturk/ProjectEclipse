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

	public static GameObject GetLoadGameObjectInstanceInternal(string CKANLCOICIL, string CBKHNNNCPLO = "")
	{
		GameObject dONFADGOEDE = GetLoadGameObjectInternal(CKANLCOICIL, CBKHNNNCPLO);
		return GetGameObjectInstance(dONFADGOEDE);
	}

	public static GameObject GetLoadGameObjectInstance(string path)
	{
		GameObject dONFADGOEDE = GetLoadGameObject(path);
		return GetGameObjectInstance(dONFADGOEDE);
	}

	private static GameObject GetGameObjectInstance(GameObject DONFADGOEDE)
	{
		if (DONFADGOEDE != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(DONFADGOEDE);
			gameObject.name = gameObject.name.Replace("(Clone)", string.Empty);
			return gameObject;
		}
		return null;
	}

	public static GameObject GetLoadGameObjectInternal(string CKANLCOICIL, string CBKHNNNCPLO = "")
	{
		return GetLoadObjectInternal<GameObject>(CKANLCOICIL, CBKHNNNCPLO);
	}

	public static GameObject GetLoadGameObject(string path)
	{
		return GetLoadObject<GameObject>(path);
	}

	public static AudioClip GetLoadAudioClipInternal(string CKANLCOICIL, string CBKHNNNCPLO = "")
	{
		return GetLoadObjectInternal<AudioClip>(CKANLCOICIL, CBKHNNNCPLO);
	}

	public static AudioClip GetLoadAudioClip(string path)
	{
		return GetLoadObject<AudioClip>(path);
	}

	public static Texture2D GetLoadTexture2DInternal(string CKANLCOICIL, string CBKHNNNCPLO = "")
	{
		Texture2D aOMLCBHAJJH = GetLoadObjectInternal<Texture2D>(CKANLCOICIL, CBKHNNNCPLO);
		return ObjecOrDefault(aOMLCBHAJJH, GetNoImageTexture());
	}

	public static Texture2D GetLoadTexture2D(string path)
	{
		Texture2D aOMLCBHAJJH = GetLoadObject<Texture2D>(path);
		return ObjecOrDefault(aOMLCBHAJJH, GetNoImageTexture());
	}

	public static Sprite GetLoadSpriteInternal(string CKANLCOICIL, string CBKHNNNCPLO = "")
	{
		Sprite aOMLCBHAJJH = GetLoadObjectInternal<Sprite>(CKANLCOICIL, CBKHNNNCPLO);
		return ObjecOrDefault(aOMLCBHAJJH, GetNoImageSprite());
	}

	public static Sprite GetLoadSprite(string path)
	{
		Sprite aOMLCBHAJJH = GetLoadObject<Sprite>(path);
		return ObjecOrDefault(aOMLCBHAJJH, GetNoImageSprite());
	}

	public static Sprite GetLoadSpriteFromTextureInternal(string CKANLCOICIL, string CBKHNNNCPLO = "")
	{
		Texture2D dAELKEKILOB = GetLoadTexture2DInternal(CKANLCOICIL, CBKHNNNCPLO);
		return TexturesUtils.CreateSprite(dAELKEKILOB);
	}

	public static Sprite GetLoadSpriteFromTexture(string path)
	{
		Texture2D dAELKEKILOB = GetLoadTexture2D(path);
		return TexturesUtils.CreateSprite(dAELKEKILOB);
	}

	public static Sprite GetLoadSpriteFromAtlas(string NJKCBALJDMM, string KIKMPCLOBCK, string JGIGOMLGLPN)
	{
		return TexturesUtils.GetSpriteFromAtlas(NJKCBALJDMM, KIKMPCLOBCK, JGIGOMLGLPN);
	}

	public static Sprite GetLoadSpriteFromAtlas(string KIKMPCLOBCK, string JGIGOMLGLPN)
	{
		return TexturesUtils.GetSpriteFromAtlas(KIKMPCLOBCK, JGIGOMLGLPN);
	}

	public static byte[] GetLoadBytesInternal(string CKANLCOICIL, string CBKHNNNCPLO = "")
	{
		TextAsset textAsset = GetLoadObjectInternal<TextAsset>(CKANLCOICIL, CBKHNNNCPLO);
		return (!(textAsset == null)) ? textAsset.bytes : null;
	}

	public static byte[] GetLoadBytes(string path)
	{
		TextAsset textAsset = GetLoadObject<TextAsset>(path);
		return (!(textAsset == null)) ? textAsset.bytes : null;
	}

	public static string GetLoadTextInternal(string CKANLCOICIL, string CBKHNNNCPLO = "")
	{
		TextAsset textAsset = GetLoadObjectInternal<TextAsset>(CKANLCOICIL, CBKHNNNCPLO);
		return (!(textAsset == null)) ? textAsset.text : null;
	}

	public static string GetLoadText(string path)
	{
		TextAsset textAsset = GetLoadObject<TextAsset>(path);
		return (!(textAsset == null)) ? textAsset.text : null;
	}

	public static T GetLoadJsonInternal<T>(string CKANLCOICIL, string CBKHNNNCPLO = "") where T : class
	{
		string dMNBDBJNKME = GetLoadTextInternal(CKANLCOICIL, CBKHNNNCPLO);
		return DeserializeJson<T>(dMNBDBJNKME);
	}

	public static T GetLoadJson<T>(string path) where T : class
	{
		string dMNBDBJNKME = GetLoadText(path);
		return DeserializeJson<T>(dMNBDBJNKME);
	}

	private static T DeserializeJson<T>(string DMNBDBJNKME) where T : class
	{
		if (!DMNBDBJNKME.IsNullOrEmpty())
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(DMNBDBJNKME);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error GetLoadJson [" + ex.Message + "]");
			}
		}
		return (T)null;
	}

	private static T ObjecOrDefault<T>(T AOMLCBHAJJH, T LKJLDJGIAOJ = null) where T : UnityEngine.Object
	{
		return AOMLCBHAJJH ?? LKJLDJGIAOJ;
	}

	public static T[] GetLoadObjectsInternal<T>(string CKANLCOICIL, string CBKHNNNCPLO = "") where T : UnityEngine.Object
	{
		return LoadAll<T>(GlobalPath.GetInternalPath(CKANLCOICIL, CBKHNNNCPLO));
	}

	public static T[] GetLoadObjects<T>(string MFBENNFFKNC) where T : UnityEngine.Object
	{
		return LoadAll<T>(GlobalPath.GetLoaderPath(MFBENNFFKNC));
	}

	public static T GetLoadObjectInternal<T>(string CKANLCOICIL, string CBKHNNNCPLO = "") where T : UnityEngine.Object
	{
		return Load<T>(GlobalPath.GetInternalPath(CKANLCOICIL, CBKHNNNCPLO));
	}

	public static T GetLoadObject<T>(string MFBENNFFKNC) where T : UnityEngine.Object
	{
		return Load<T>(GlobalPath.GetLoaderPath(MFBENNFFKNC));
	}

	private static TResult FirstNonNull<TResult, Arg>(Arg EHCLMBADLKH, params Func<Arg, TResult>[] EFAICNOJJIP)
	{
		foreach (Func<Arg, TResult> func in EFAICNOJJIP)
		{
			TResult val = func(EHCLMBADLKH);
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

	public static void Unload(UnityEngine.Object AOMLCBHAJJH, bool OJCKACIMFEJ = true)
	{
		if (!(AOMLCBHAJJH == null))
		{
#if UNITY_6000_0_OR_NEWER
			// Same value Object.GetInstanceID() returns in Unity 6: the low 32 bits of the EntityId. The legacy sign test still holds.
			if (unchecked((int)EntityId.ToULong(AOMLCBHAJJH.GetEntityId())) <= 0)
#else
			if (AOMLCBHAJJH.GetInstanceID() <= 0)
#endif
			{
				DestroyObject(AOMLCBHAJJH, OJCKACIMFEJ);
			}
			else
			{
				ResourcesUtil.UnloadAsset(AOMLCBHAJJH, OJCKACIMFEJ);
			}
		}
	}

	public static void DestroyObject(UnityEngine.Object AOMLCBHAJJH, bool OJCKACIMFEJ = true)
	{
		try
		{
			if (!OJCKACIMFEJ)
			{
				UnityEngine.Object.Destroy(AOMLCBHAJJH);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(AOMLCBHAJJH);
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

	public static string GetFileOrResourcesText(string path, string IDGLPJGEFKB, string name = "")
	{
		string text = FileUtils.ReadAllText(path);
		if (!text.IsNullOrEmpty())
		{
			return text;
		}
		return GetLoadTextInternal(IDGLPJGEFKB, name);
	}
}
