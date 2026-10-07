using System.Collections.Generic;
using Nekki.SF2.GUI.Dialogs;
using UnityEngine;

public class DialogsManager : global::EventDispatcher<object>
{
	public enum DialogEvent
	{
		OnStopDialog = 0
	}

	private static DialogsManager _instance;

	private static List<BaseDialog> openDialogs = new List<BaseDialog>();

	private static DialogType currentDialogType;

	private static BaseDialog currentDialog = null;

	[SerializeField]
	private static GameObject _SettingsDialogPrefab;

	public static DialogsManager Instance
	{
		get
		{
			return GetInstance();
		}
	}

	public static DialogsManager GetInstance()
	{
		if (_instance == null)
		{
			_instance = new DialogsManager();
		}
		return _instance;
	}

	public static BaseDialog ShowDialog(DialogType dialogType, object data)
	{
		currentDialogType = dialogType;
		OpenDialog(data);
		return currentDialog;
	}

	public static void OpenDialog(object data)
	{
		currentDialog = CreateDialog(currentDialogType);
		if (!(currentDialog == null))
		{
			currentDialog.Init(data);
			if (NotificationsGame.get_IsOpen())
			{
				NotificationsGame.CloseNotifications();
			}
			DialogCanvasController.get_Instance().BlockNotDialogTouches();
			if (currentDialog.TopMenuIsActive)
			{
			}
			openDialogs.Add(currentDialog);
			if (!currentDialog.IsPausing)
			{
			}
		}
	}

	public void StopDialog(BaseDialog dialog)
	{
		// Dialog prefabs live on a DontDestroyOnLoad canvas.  Scene changes and
		// reconstructed quest flows can therefore leave destroyed or inactive
		// entries in this static stack.  Treating those entries as open keeps all
		// scene GraphicRaycasters disabled after a buy/upgrade until Escape is
		// pressed.  Remove the closing dialog, duplicates, and stale entries in one
		// backwards pass before deciding whether input should remain blocked.
		int staleCount = 0;
		for (int i = openDialogs.Count - 1; i >= 0; i--)
		{
			BaseDialog baseDialog = openDialogs[i];
			if (baseDialog == null || baseDialog == dialog || !baseDialog.gameObject.activeInHierarchy)
			{
				openDialogs.RemoveAt(i);
				if (baseDialog != dialog)
				{
					staleCount++;
				}
			}
		}
		if (staleCount > 0)
		{
			Debug.LogWarning("[Dialogs] Removed " + staleCount + " stale dialog blocker(s).");
		}
		if (openDialogs.Count > 0)
		{
			currentDialog = openDialogs[openDialogs.Count - 1];
			DialogCanvasController.get_Instance().BlockNotDialogTouches();
			for (int j = 0; j < openDialogs.Count; j++)
			{
			}
		}
		else
		{
			currentDialog = null;
			DialogCanvasController.get_Instance().UnBlockTouches();
		}
		if (dialog.IsPausing)
		{
		}
		CallEvent(0, dialog);
	}

	public static void CloseNonQuestDialogs()
	{
		BaseDialog baseDialog = ((!(currentDialog != null)) ? null : currentDialog);
		bool flag = baseDialog != null && !baseDialog.IsQuestDialog;
		while (flag)
		{
			currentDialog.Close(0);
			baseDialog = ((!(currentDialog != null)) ? null : currentDialog);
			flag = baseDialog != null && !baseDialog.IsQuestDialog;
		}
	}

	public static BaseDialog CreateDialog(DialogType dialogType)
	{
		switch (dialogType)
		{
		case DialogType.DialogSimple:
			return DialogCanvasController.get_Instance().CreateDialog<SimpleDialog>();
		case DialogType.DialogExit:
			return DialogCanvasController.get_Instance().CreateDialog<ExitDialog>();
		case DialogType.DialogBuy:
			return DialogCanvasController.get_Instance().CreateDialog<TradeDialog>();
		case DialogType.DialogImpossible:
			return DialogCanvasController.get_Instance().CreateDialog<ImpossibleDialog>();
		case DialogType.DialogStory:
			return DialogCanvasController.get_Instance().CreateDialog<StoryDialog>();
		case DialogType.DialogSettings:
			return DialogCanvasController.get_Instance().CreateDialog<SettingsDialog>();
		case DialogType.DialogSettingsAdvenced:
			return DialogCanvasController.get_Instance().CreateDialog<SettingsAdvancedDialog>();
		case DialogType.DialogStranger:
			return DialogCanvasController.get_Instance().CreateDialog<StrangerDialog>();
		case DialogType.DialogNews:
			return DialogCanvasController.get_Instance().CreateDialog<NewsDialog>();
		default:
			GameLog.Write("ERROR: getDialog - unknown dialog type: " + dialogType);
			return null;
		}
	}
}
