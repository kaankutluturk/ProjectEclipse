using System.Collections.Generic;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Fight;
using UnityEngine;

public class BackKeyManager : SFMonoBehaviour<object>
{
	private static BackKeyManager _instance;

	private List<BackKeyController> controllers = new List<BackKeyController>();

	public static BackKeyManager SharedInstance
	{
		get
		{
			return get_Instance();
		}
	}

	public static BackKeyManager get_Instance()
	{
		if (_instance == null)
		{
			GameObject gameObject = new GameObject("[BackKeyManager]");
			_instance = gameObject.AddComponent<BackKeyManager>();
			Object.DontDestroyOnLoad(gameObject);
		}
		return _instance;
	}

	private void OnDestroy()
	{
		RemoveAllEventListener();
		_instance = null;
	}

	public void AddBackKeyController(BackKeyController controller)
	{
		controllers.AddIfNotExist(controller);
	}

	public void RemoveBackKeyController(BackKeyController controller)
	{
		controllers.Remove(controller);
	}

	public void Clear()
	{
		controllers.Clear();
	}

	private void Update()
	{
		if (Eclipse.Input.EclipseInput.GetKeyDown(KeyCode.Escape))
		{
			OnBackKeyClicked();
		}
		Eclipse.Input.FightPauseKey.Tick(controllers.Count);
	}

	public void OnBackKeyClicked()
	{
		if (Eclipse.UI.Modding.ModUiGameBridge.TryHandleBack()) return;
		if (controllers.Count > 0)
		{
			controllers[controllers.Count - 1].OnBackKeyClicked(0);
			return;
		}
		switch (SceneManagerSF.GetCurrentScreen())
		{
		case ScreenType.ModuleFight:
		{
			FightScene current = Scene<FightScene>.get_Current();
			if (current != null && current.Fight != null)
			{
				current.Fight.TogglePauseMenu(true);
			}
			break;
		}
		case ScreenType.ModuleDojo:
			DialogsOpener.OpenExitDialog();
			break;
		case ScreenType.ModuleShop:
		case ScreenType.ModuleMap:
		case ScreenType.ModuleProfile:
			Module.OpenScreen(ScreenType.ModuleDojo);
			break;
		}
	}
}
