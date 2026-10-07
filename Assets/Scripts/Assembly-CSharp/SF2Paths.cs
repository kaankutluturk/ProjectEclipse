using System;
using System.IO;
using UnityEngine;

public static class SF2Paths
{
	private const string GameDataFolder = "/gamedata";

	private const string UserDataFolder = "/userdata";

	private const string AnimationsFolder = "/animations";

	private const string BinaryAnimationsFolder = "/animations/binary";

	private const string ModelsFolder = "/models";

	private const string LocalizationsFolder = "/localizations";

	private const string LocationsFolder = "/locations";

	private const string TexturesFolder = "/textures";

	private const string FullscreenTexturesFolder = "textures/fullscreen/";

	private const string _Statistics = "/statistics";

	private const string NewsFolder = "/news";

	private const string BundlesFolder = "/bundles";

	private const string VideoFolder = "/video";

	private const string PacksFileName = "/packs.xml";

	private const string GuiResourcesFolder = "Assets/src/GUI/Resources";

	private const string ItemsUiFolder = "UI/Items/";

	private const string UsersUiFolder = "UI/Users/";

	private const string SkillsUiFolder = "UI/Skills/";

	public const string AchievementsUiFolder = "UI/Achievements/";

	public const string FullscreenUiFolder = "UI/Fullscreen/";

	public const string LogosTexturesFolder = "textures/Logos/";

	public static string GameDataRoot = string.Empty;

	public static string UserDataRoot = string.Empty;

	public static string WritableDataRoot = string.Empty;

	public static bool UseBundledResources = true;

	private static bool initialized;

	public static string GameDataPath
	{
		get
		{
			return GetGameDataPath();
		}
	}

	public static string WritableGameDataPath
	{
		get
		{
			return GetWritableGameDataPath();
		}
	}

	public static string AnimationsPath
	{
		get
		{
			return GetAnimationsPath();
		}
	}

	public static string BinaryAnimationsPath
	{
		get
		{
			return GetBinaryAnimationsPath();
		}
	}

	public static string ModelsPath
	{
		get
		{
			return GetModelsPath();
		}
	}

	public static string VideoPath
	{
		get
		{
			return GetVideoPath();
		}
	}

	public static string ItemsUiPath
	{
		get
		{
			return GetItemsUiPath();
		}
	}

	public static string Textures
	{
		get
		{
			return GetTexturesPath();
		}
	}

	public static string UsersUiPath
	{
		get
		{
			return GetUsersUiPath();
		}
	}

	public static string SkillsUiPath
	{
		get
		{
			return GetSkillsUiPath();
		}
	}

	public static string FullscreenTexturesPath
	{
		get
		{
			return GetFullscreenTexturesPath();
		}
	}

	public static string LocalizationsPath
	{
		get
		{
			return GetLocalizationsPath();
		}
	}

	public static string LocationsPath
	{
		get
		{
			return GetLocationsPath();
		}
	}

	public static string UserDataPath
	{
		get
		{
			return GetUserDataDirectory();
		}
	}

	public static string StatisticsPath
	{
		get
		{
			return GetStatisticsPath();
		}
	}

	public static string NewsPath
	{
		get
		{
			return GetNewsPath();
		}
	}

	public static string BundlesPath
	{
		get
		{
			return GetBundlesPath();
		}
	}

	public static string PacksFilePath
	{
		get
		{
			return GetPacksFilePath();
		}
	}

	public static string GuiResourcesRoot
	{
		get
		{
			return GetGuiResourcesRoot();
		}
	}

	public static bool IsInitialized
	{
		get
		{
			return GetIsInitialized();
		}
	}

	public static string GetGameDataPath()
	{
		return GameDataRoot + "/gamedata";
	}

	public static string GetWritableGameDataPath()
	{
		return WritableDataRoot + "/gamedata";
	}

	public static string GetAnimationsPath()
	{
		return GetGameDataPath() + "/animations";
	}

	public static string GetBinaryAnimationsPath()
	{
		return GetGameDataPath() + "/animations/binary";
	}

	public static string GetModelsPath()
	{
		return GetGameDataPath() + "/models";
	}

	public static string GetVideoPath()
	{
		return GetGameDataPath() + "/video";
	}

	public static string GetItemsUiPath()
	{
		return "UI/Items/";
	}

	public static string GetTexturesPath()
	{
		return "/textures";
	}

	public static string GetUsersUiPath()
	{
		return "UI/Users/";
	}

	public static string GetSkillsUiPath()
	{
		return "UI/Skills/";
	}

	public static string GetFullscreenTexturesPath()
	{
		return "textures/fullscreen/";
	}

	public static string GetLocalizationsPath()
	{
		return GetGameDataPath() + "/localizations";
	}

	public static string GetLocationsPath()
	{
		return GetGameDataPath() + "/locations";
	}

	// best guess for name
	public static string GetUserDataDirectory()
	{
		return (Eclipse.Saves.CampaignSaveSession.UserDataDirectory ?? GetLegacyUserDataDirectory()).Replace('\\', '/');
	}

	public static string GetLegacyUserDataDirectory()
	{
		return UserDataRoot + "/userdata";
	}

	public static string GetStatisticsPath()
	{
		return GetWritableGameDataPath() + "/statistics";
	}

	public static string GetNewsPath()
	{
		return GetWritableGameDataPath() + "/news";
	}

	public static string GetBundlesPath()
	{
		return GetWritableGameDataPath() + "/bundles";
	}

	public static string GetPacksFilePath()
	{
		return GetWritableGameDataPath() + "/packs.xml";
	}

	public static string GetGuiResourcesRoot()
	{
		return "Assets/src/GUI/Resources";
	}

	public static bool GetIsInitialized()
	{
		return initialized;
	}

	public static void Init()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		GameDataRoot = string.Empty;
		UserDataRoot = Eclipse.Runtime.EditorPlayModeContext.PersistentDataPath;
		WritableDataRoot = Eclipse.Runtime.EditorPlayModeContext.PersistentDataPath;
		UseBundledResources = true;
		string text = GetAndroidFilesDir();
		if (string.IsNullOrEmpty(UserDataRoot))
		{
			UserDataRoot = (WritableDataRoot = text);
		}
		else
		{
			try
			{
				if (!Directory.Exists(UserDataRoot + "/userdata") && Directory.Exists(text + "/userdata"))
				{
					UserDataRoot = (WritableDataRoot = text);
				}
			}
			catch
			{
			}
		}
		EnsureDirectories();
	}

	public static void EnsureDirectories()
	{
		if (!Directory.Exists(GetUserDataDirectory()))
		{
			Directory.CreateDirectory(GetUserDataDirectory());
		}
		if (!Directory.Exists(GetWritableGameDataPath()))
		{
			Directory.CreateDirectory(GetWritableGameDataPath());
		}
	}

	public static string ResolveWritablePath(string ONEIGMLOGDC)
	{
		if (ONEIGMLOGDC.Contains(WritableDataRoot))
		{
			return ONEIGMLOGDC;
		}
		return string.Format("{0}/{1}", WritableDataRoot, ONEIGMLOGDC);
	}

	public static void ResetBundlesDirectory()
	{
		if (Directory.Exists(GetBundlesPath()))
		{
			Directory.Delete(GetBundlesPath(), true);
		}
		Directory.CreateDirectory(GetBundlesPath());
	}

	public static string GetAndroidFilesDir()
	{
		string text = string.Empty;
		if (Application.platform != RuntimePlatform.Android || Application.isEditor)
		{
			return text;
		}
		try
		{
			IntPtr javaClass = AndroidJNI.FindClass("android/content/ContextWrapper");
			IntPtr methodID = AndroidJNIHelper.GetMethodID(javaClass, "getFilesDir", "()Ljava/io/File;");
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
				{
					IntPtr obj = AndroidJNI.CallObjectMethod(androidJavaObject.GetRawObject(), methodID, new jvalue[0]);
					IntPtr javaClass2 = AndroidJNI.FindClass("java/io/File");
					IntPtr methodID2 = AndroidJNIHelper.GetMethodID(javaClass2, "getAbsolutePath", "()Ljava/lang/String;");
					text = AndroidJNI.CallStringMethod(obj, methodID2, new jvalue[0]);
					if (text == null)
					{
						Debug.Log("Using fallback path");
						text = "/data/data/com.nekki.shadowfight/files";
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		return text;
	}
}
