using System.Diagnostics;
using Nekki.SF2.GUI.Scenes;
using UnityEngine.SceneManagement;

public static class SceneManagerSF
{
	private static ScreenType currentScreen;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool initialized;

	public static ScreenType CurrentScreen
	{
		get
		{
			return GetCurrentScreen();
		}
		set
		{
			SetCurrentScreen(value);
		}
	}

	public static bool Initialized
	{
		get
		{
			return GetIsInitialized();
		}
		private set
		{
			set_IsInitialized(value);
		}
	}

	public static ScreenType GetCurrentScreen()
	{
		return currentScreen;
	}

	public static void SetCurrentScreen(ScreenType value)
	{
		currentScreen = value;
	}

	public static bool GetIsInitialized()
	{
		return initialized;
	}

	private static void set_IsInitialized(bool value)
	{
		initialized = value;
	}

	public static bool Init(ScreenType DAINBPONGAB)
	{
		if (!GetIsInitialized())
		{
			set_IsInitialized(true);
			if (DAINBPONGAB != ScreenType.ModulePreloader)
			{
				Reset();
				return false;
			}
		}
		return true;
	}

	public static void Reset()
	{
		GameLoaderScene.Stop();
		Load(ScreenType.ModulePreloader);
	}

	public static void Load(ScreenType MHOCFOODLLL)
	{
		if (MHOCFOODLLL != ScreenType.Loader)
		{
			LoaderScene.set_PrevScene(GetCurrentScreen());
			LoaderScene.set_NextScene(MHOCFOODLLL);
		}
		SceneManager.LoadSceneAsync(1);
	}

	public static Scene GetActiveScene()
	{
		return SceneManager.GetActiveScene();
	}
}
