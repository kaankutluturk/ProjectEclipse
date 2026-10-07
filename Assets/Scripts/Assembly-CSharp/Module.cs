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

	public static bool OpenScreenByName(string HBGBPDEGKFE, object data = null, Action<object> ODDEOFKLIAG = null, bool EOIDGPINLAH = true)
	{
		ScreenType hBGBPDEGKFE = ParseScreenType(HBGBPDEGKFE);
		return OpenScreen(hBGBPDEGKFE, data, ODDEOFKLIAG, EOIDGPINLAH);
	}

	public static bool OpenScreen(ScreenType HBGBPDEGKFE, object data = null, Action<object> ODDEOFKLIAG = null, bool EOIDGPINLAH = true)
	{
		Module jLINNJGCFOG = GetInstance();
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		hHKLFIIBIFF.sceneFrom = hHKLFIIBIFF.currentSceneName;
		hHKLFIIBIFF.sceneTo = GetScreenName(HBGBPDEGKFE);
		SliderType oFEMKBGPNBH = GameUtils.GetSliderTypeByName(hHKLFIIBIFF.currentTabName);
		SliderType cFDMHKKBGIN = GetSliderTypeForScreen(HBGBPDEGKFE, data);
		if (EOIDGPINLAH && GameUtils.NotifyShopOpened(HBGBPDEGKFE))
		{
			return false;
		}
		if (EOIDGPINLAH && GameUtils.NotifyTabChanged(oFEMKBGPNBH, cFDMHKKBGIN))
		{
			return false;
		}
		string bPPAPLLPBIJ = hHKLFIIBIFF.currentSceneName;
		string text = GetScreenName(ScreenType.ModuleShop);
		if (bPPAPLLPBIJ == text)
		{
			MenuController.SetNormalViewMode(false);
		}
		MenuController.RefreshRubySale();
		jLINNJGCFOG.ScreenInfo.PreviousScreenType = jLINNJGCFOG.ScreenInfo.ScreenType;
		jLINNJGCFOG.ScreenInfo.ScreenType = HBGBPDEGKFE;
		jLINNJGCFOG.ScreenInfo.Data = data;
		jLINNJGCFOG.ScreenInfo.Dlg = ODDEOFKLIAG;
		Action load = () =>
		{
			jLINNJGCFOG.LoadCurrentScreen();
			jLINNJGCFOG.CallEvent(0, jLINNJGCFOG.ScreenInfo);
		};
		if (!Eclipse.UI.MenuSceneFade.Begin(jLINNJGCFOG.ScreenInfo.PreviousScreenType, HBGBPDEGKFE, load)) load();
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
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		hHKLFIIBIFF.currentSceneName = GetScreenName(ScreenInfo.ScreenType);
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

	public static ScreenType ParseScreenType(string PHJPOKBOJOG)
	{
		switch (PHJPOKBOJOG)
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
			GameLog.Error("Module::getScreenTypeFromString - screen: %s", PHJPOKBOJOG);
			return ScreenType.ModuleFight;
		}
	}

	public static string GetScreenName(ScreenType HBGBPDEGKFE)
	{
		string result = string.Empty;
		switch (HBGBPDEGKFE)
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
			GameLog.Error("Module::getScreenNameFromType - screen: " + HBGBPDEGKFE);
			break;
		}
		return result;
	}

	public static SliderType GetSliderTypeForScreen(ScreenType HBGBPDEGKFE, object data)
	{
		SliderType lBFKFBALMGA = SliderType.SliderNone;
		switch (HBGBPDEGKFE)
		{
		case ScreenType.ModuleShop:
		{
			DelayedStrike dDFFCNPELBC = ((data == null) ? null : ((DelayedStrike)data));
			if (dDFFCNPELBC != null)
			{
				return dDFFCNPELBC.SliderType;
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

	public void RegisterHolder(ModuleHolder MHOCFOODLLL)
	{
		currentHolder = MHOCFOODLLL;
		OnSceneReady();
	}

	public void UnregisterHolder(ModuleHolder MHOCFOODLLL)
	{
		currentHolder = null;
		BackKeyManager.get_Instance().Clear();
	}

	public bool IsUserTutorialComplete()
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP == null)
		{
			return true;
		}
		return nKGLHEGIKKP.GetTutorials().GetIsStoryTutorialActive();
	}

	public void SetGameInputLock(bool NKCGCFGFNDL, bool LLOLBKJMKNC = true)
	{
		gameInputLock = NKCGCFGFNDL;
		RefreshInputLock(LLOLBKJMKNC);
	}

	public void SetQuestInputLock(bool FHFEIGOAJHO, bool LLOLBKJMKNC = true)
	{
		questInputLock = FHFEIGOAJHO;
		RefreshInputLock(LLOLBKJMKNC);
	}

    // best guess for name
	public void RefreshInputLock(bool LLOLBKJMKNC)
	{
		bool flag = gameInputLock || questInputLock || unusedInputLock || _presentationLocks > 0;
		if (_inputLocked != flag)
		{
			_inputLocked = flag;
			_visibleInputLock = LLOLBKJMKNC;
			if (_inputLocked)
			{
				ShowInputLock(_visibleInputLock);
			}
			else
			{
				ReleaseInputLock();
			}
		}
		else if (_inputLocked && _inputLocked == flag && _visibleInputLock != LLOLBKJMKNC)
		{
			_visibleInputLock = LLOLBKJMKNC;
			ReleaseInputLock();
			ShowInputLock(_visibleInputLock);
		}
	}

	private void ApplyLockScreen(bool value, bool LLOLBKJMKNC = true)
	{
		LockScreen.Lock(value, LLOLBKJMKNC);
	}

	private void ShowInputLock(bool LLOLBKJMKNC)
	{
		ApplyLockScreen(_inputLocked, LLOLBKJMKNC);
	}

	private void ReleaseInputLock()
	{
		ApplyLockScreen(_inputLocked);
	}

	public void ReloadCurrentScreen()
	{
		ScreenType hBGBPDEGKFE = GetCurrentScreenType();
		OpenScreen(hBGBPDEGKFE);
	}
}
