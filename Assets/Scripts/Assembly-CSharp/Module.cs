using System;
using Nekki.SF2.GUI;
using UnityEngine.SceneManagement;

public class Module : global::EventDispatcher<object>
{
	public enum ModuleEvent
	{
		OnPrepareOpenScene = 0,
		OnOpenScene = 1,
		OnEnterFrame = 2,
		OnCloseScene = 3,
		OnSceneCreated = 4,
		OnStartShake = 5,
		OnStopShake = 6
	}

	private static Module instance;

	public ScreenInfo ScreenInfo = new ScreenInfo();

	private ModuleHolder currentHolder;

	// best guess for name
	private bool _inputLocked;

	// best guess for name
	private bool _visibleInputLock;

	private bool gameInputLock;

	private bool questInputLock;
    private int _presentationLocks;
    public IDisposable AcquirePresentationLock()
    {
        bool visible = _inputLocked && _visibleInputLock;
        _presentationLocks++;
        RefreshInputLock(visible);
        return new PresentationLock(this);
    }
    private sealed class PresentationLock : IDisposable
    {
        private Module owner;
        public PresentationLock(Module owner) { this.owner = owner; }
        public void Dispose()
        {
            var current = owner; owner = null;
            if (current == null) return;
            current._presentationLocks--;
            current.RefreshInputLock(current._visibleInputLock);
        }
    }

	private bool unusedInputLock;

	public static Module CurrentModule
	{
		get
		{
			return GetInstance();
		}
	}

	public ModuleHolder CurrentHolder
	{
		get
		{
			return GetCurrentHolder();
		}
	}

	private Module()
	{
	}

	// best guess for name
	public static Module GetInstance()
	{
		if (instance == null)
		{
			instance = new Module();
		}
		return instance;
	}

	public ModuleHolder GetCurrentHolder()
	{
		return currentHolder;
	}

	public static void Reset()
	{
		instance = null;
	}

	public static bool OpenScreenByName(string screenName, object data = null, Action<object> callback = null, bool notifyListeners = true)
	{
		ScreenType screenType = ParseScreenType(screenName);
		return OpenScreen(screenType, data, callback, notifyListeners);
	}

	public static bool OpenScreen(ScreenType screenType, object data = null, Action<object> callback = null, bool notifyListeners = true)
	{
		Module moduleInstance = GetInstance();
		QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
		questParameters.sceneFrom = questParameters.currentSceneName;
		questParameters.sceneTo = GetScreenName(screenType);
		SliderType previousSliderType = GameUtils.GetSliderTypeByName(questParameters.currentTabName);
		SliderType targetSliderType = GetSliderTypeForScreen(screenType, data);
		if (notifyListeners && GameUtils.NotifyShopOpened(screenType))
		{
			return false;
		}
		if (notifyListeners && GameUtils.NotifyTabChanged(previousSliderType, targetSliderType))
		{
			return false;
		}
		string currentSceneName = questParameters.currentSceneName;
		string text = GetScreenName(ScreenType.ModuleShop);
		if (currentSceneName == text)
		{
			MenuController.SetNormalViewMode(false);
		}
		MenuController.RefreshRubySale();
		moduleInstance.ScreenInfo.PreviousScreenType = moduleInstance.ScreenInfo.ScreenType;
		moduleInstance.ScreenInfo.ScreenType = screenType;
		moduleInstance.ScreenInfo.Data = data;
		moduleInstance.ScreenInfo.Dlg = callback;
		Action load = () =>
		{
			moduleInstance.LoadCurrentScreen();
			moduleInstance.CallEvent(0, moduleInstance.ScreenInfo);
		};
		if (!Eclipse.UI.MenuSceneFade.Begin(moduleInstance.ScreenInfo.PreviousScreenType, screenType, load)) load();
		return true;
	}

    internal void OpenLocalVersus(Eclipse.Multiplayer.LocalVersusMatch match)
    {
        if (match == null || !Eclipse.Multiplayer.LocalVersusSession.IsActive)
            throw new InvalidOperationException("Local versus must own the scene transition.");
        ScreenInfo.PreviousScreenType = ScreenInfo.ScreenType;
        ScreenInfo.ScreenType = ScreenType.ModuleFight;
        ScreenInfo.Data = match;
        ScreenInfo.Dlg = null;
        CallEvent(3, ScreenInfo.PreviousScreenType);
        DialogsManager.CloseNonQuestDialogs();
        SceneManagerSF.Load(ScreenType.ModuleFight);
        CallEvent(4, ScreenType.ModuleFight);
        CallEvent(0, ScreenInfo);
    }

	public void LoadCurrentScreen()
	{
		CallEvent(3, ScreenInfo.PreviousScreenType);
		DialogsManager.CloseNonQuestDialogs();
		SceneManagerSF.Load(ScreenInfo.ScreenType);
		CallEvent(4, ScreenInfo.ScreenType);
		QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
		questParameters.currentSceneName = GetScreenName(ScreenInfo.ScreenType);
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_SCENE_LOADED))
		{
			ListSF.GetInstance().RunQuestActions();
		}
	}

	// best guess for name
	public ScreenType GetCurrentScreenType()
	{
		return ScreenInfo.ScreenType;
	}

	public Scene GetCurrentScene()
	{
		return SceneManagerSF.GetActiveScene();
	}

	public static ScreenType ParseScreenType(string screenName)
	{
		switch (screenName)
		{
		case "Dojo":
			return ScreenType.ModuleDojo;
		case "Map":
			return ScreenType.ModuleMap;
		case "Shop":
			return ScreenType.ModuleShop;
		case "Profile":
			return ScreenType.ModuleProfile;
		case "Loader":
			return ScreenType.ModulePreloader;
		default:
			GameLog.Error("Module::getScreenTypeFromString - screen: %s", screenName);
			return ScreenType.ModuleFight;
		}
	}

	public static string GetScreenName(ScreenType screenType)
	{
		string result = string.Empty;
		switch (screenType)
		{
		case ScreenType.ModulePreloader:
			result = "Loader";
			break;
		case ScreenType.ModuleFight:
			result = "Fight";
			break;
		case ScreenType.ModuleShop:
			result = "Shop";
			break;
		case ScreenType.ModuleMap:
			result = "Map";
			break;
		case ScreenType.ModuleProfile:
			result = "Profile";
			break;
		case ScreenType.ModuleCreditsScreen:
			result = "Credits";
			break;
		case ScreenType.ModuleDojo:
			result = "Dojo";
			break;
		default:
			GameLog.Error("Module::getScreenNameFromType - screen: " + screenType);
			break;
		}
		return result;
	}

	public static SliderType GetSliderTypeForScreen(ScreenType screenType, object data)
	{
		SliderType sliderType = SliderType.SliderNone;
		switch (screenType)
		{
		case ScreenType.ModuleShop:
		{
			DelayedStrike delayedStrike = ((data == null) ? null : ((DelayedStrike)data));
			if (delayedStrike != null)
			{
				return delayedStrike.SliderType;
			}
			return SliderType.SliderWeapon;
		}
		case ScreenType.ModuleProfile:
			return SliderType.SliderPerks;
		case ScreenType.ModuleMap:
			return SliderType.SliderStoryMap;
		default:
			return SliderType.SliderNone;
		}
	}

	public void OnSceneReady()
	{
		if (Eclipse.Multiplayer.LocalVersusSession.IsActive)
		{
			CallEvent(1, ScreenInfo.ScreenType);
			CallEvent(2, 0);
			return;
		}
		if (ScreenInfo.ScreenType != ScreenType.ModulePreloader)
		{
			if (ScreenInfo.Dlg != null)
			{
				ScreenInfo.Dlg(null);
				ScreenInfo.Dlg = null;
			}
			else
			{
				ListSF.GetInstance().RunQuestActions();
			}
		}
		CallEvent(1, ScreenInfo.ScreenType);
		Eclipse.Modding.ModRuntime.ShowPendingBattleLottery();
		CallEvent(2, 0);
	}

	public void RegisterHolder(ModuleHolder holder)
	{
		currentHolder = holder;
		OnSceneReady();
	}

	public void UnregisterHolder(ModuleHolder holder)
	{
		currentHolder = null;
		BackKeyManager.get_Instance().Clear();
	}

	public bool IsUserTutorialComplete()
	{
		Roster roster = ListSF.GetRoster();
		if (roster == null)
		{
			return true;
		}
		return roster.GetTutorials().GetIsStoryTutorialActive();
	}

	public void SetGameInputLock(bool locked, bool visible = true)
	{
		gameInputLock = locked;
		RefreshInputLock(visible);
	}

	public void SetQuestInputLock(bool locked, bool visible = true)
	{
		questInputLock = locked;
		RefreshInputLock(visible);
	}

    // best guess for name
	public void RefreshInputLock(bool visible)
	{
		bool flag = gameInputLock || questInputLock || unusedInputLock || _presentationLocks > 0;
		if (_inputLocked != flag)
		{
			_inputLocked = flag;
			_visibleInputLock = visible;
			if (_inputLocked)
			{
				ShowInputLock(_visibleInputLock);
			}
			else
			{
				ReleaseInputLock();
			}
		}
		else if (_inputLocked && _inputLocked == flag && _visibleInputLock != visible)
		{
			_visibleInputLock = visible;
			ReleaseInputLock();
			ShowInputLock(_visibleInputLock);
		}
	}

	private void ApplyLockScreen(bool value, bool visible = true)
	{
		LockScreen.Lock(value, visible);
	}

	private void ShowInputLock(bool visible)
	{
		ApplyLockScreen(_inputLocked, visible);
	}

	private void ReleaseInputLock()
	{
		ApplyLockScreen(_inputLocked);
	}

	public void ReloadCurrentScreen()
	{
		ScreenType screenType = GetCurrentScreenType();
		OpenScreen(screenType);
	}
}
