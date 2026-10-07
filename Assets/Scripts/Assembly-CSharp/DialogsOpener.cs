using System;
using System.Collections.Generic;
using Nekki.SF2.Core;
using Nekki.SF2.GUI.Dialogs;
using UnityEngine;

public class DialogsOpener
{
	// Local replacement for the removed MobileNativePopups plugin. Reuse the
	// game's touch/mouse dialog and its existing input-lock lifetime.
	public static void OpenLocalAlertDialog(string title, string message, string ok, Action onOk)
	{
		OpenLocalAlertDialog(title, message, ok, string.Empty, onOk, null);
	}

	public static void OpenLocalAlertDialog(string title, string message, string ok, string cancel, Action onOk, Action onCancel)
	{
		bool completed = false;
		OpenSimpleDialog(title, message, ok, cancel, result =>
		{
			if (completed) return;
			completed = true;
			Action callback = string.IsNullOrEmpty(cancel) || Convert.ToInt32(result) == 1 ? onOk : onCancel;
			if (callback != null) callback();
		}, literalText: true);
	}

	public const float HeaderHeight = 65f;

	public const float FooterHeight = 90f;

	public const float ContentWidth = 470f;

	private static bool appleIdWarningAcknowledged;

	public static bool IsAppleIdWarningAcknowledged
	{
		get
		{
			return GetAppleIdWarningAcknowledged();
		}
	}

	public static void OpenTradeDialog(TradeDialog.TradeAction tradeAction, GameValueType value, long price, Action<object> callback, long deliverySeconds = 0L)
	{
		TradeDialogInfo dialogInfo = new TradeDialogInfo(tradeAction, value, price, callback, deliverySeconds);
		DialogsManager.ShowDialog(DialogType.DialogBuy, dialogInfo);
	}

	public static void OpenImpossibleDialog(ImpossibleDialog.ImpossibleDialogType reason, Action<object> callback = null, object content = null)
	{
		ImpossibleDialogInfo dialogInfo = new ImpossibleDialogInfo(reason, callback, content);
		DialogsManager.ShowDialog(DialogType.DialogImpossible, dialogInfo);
	}

	public static BaseDialog OpenStrangerDialog(string portraitName, string title, List<StoryDialogContent> contents, float ratio, Action<object> callback = null, string storeButtonText = "", string rejectButtonText = "", string acceptButtonText = "", LabelButton.ButtonColor storeButtonColor = LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor rejectButtonColor = LabelButton.ButtonColor.BUTTON_DARK, LabelButton.ButtonColor acceptButtonColor = LabelButton.ButtonColor.BUTTON_WHITE, bool showDifficulty = true, bool useEdgeButtons = false, bool showCheckBox = false, bool checkBoxChecked = false, string checkBoxText = "")
	{
		bool flag = true;
		for (int i = 0; i < contents.Count; i++)
		{
			StoryDialogContent content = contents[i];
			if (!content.RefreshItemTimer())
			{
				flag = false;
			}
		}
		if ((contents.Count > 0 && contents[0].CheckTimer && contents[0].Timer <= 0) || (useEdgeButtons && !flag))
		{
			if (callback != null)
			{
				int firstContentId = contents[0].Id;
				callback(firstContentId);
			}
			return null;
		}
		StrangerDialogInfo dialogInfo = new StrangerDialogInfo(portraitName, title, contents, ratio, callback, storeButtonText, rejectButtonText, acceptButtonText, storeButtonColor, rejectButtonColor, acceptButtonColor, showDifficulty, useEdgeButtons, showCheckBox, checkBoxChecked, checkBoxText);
		return DialogsManager.ShowDialog(DialogType.DialogStranger, dialogInfo);
	}

	public static BaseDialog OpenSimpleDialog(string title, string message, string okText, string cancelText = "", Action<object> callback = null, LabelButton.ButtonColor okButtonColor = LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor cancelButtonColor = LabelButton.ButtonColor.BUTTON_DARK, bool showCheckBox = false, bool checkBoxChecked = false, string checkBoxText = "", bool literalText = false)
	{
		BaseDialog.FooterType footerType = BaseDialog.FooterType.FOOTER_BOTH;
		if (okText == string.Empty || cancelText == string.Empty)
		{
			footerType = BaseDialog.FooterType.FOOTER_NONE;
			footerType = ((okText != string.Empty) ? BaseDialog.FooterType.FOOTER_OK : ((cancelText != string.Empty) ? BaseDialog.FooterType.FOOTER_CANCEL : BaseDialog.FooterType.FOOTER_NONE));
		}
		SimpleDialogInfo dialogInfo = new SimpleDialogInfo(title, message, footerType, okText, cancelText, okButtonColor, cancelButtonColor, showCheckBox, checkBoxChecked, checkBoxText, callback);
		dialogInfo.UseLiteralText = literalText;
		BaseDialog baseDialog = DialogsManager.ShowDialog(DialogType.DialogSimple, dialogInfo);
		if (callback != null)
		{
			baseDialog.AddEventListener(0, callback);
		}
		return baseDialog;
	}

	public static bool GetAppleIdWarningAcknowledged()
	{
		return appleIdWarningAcknowledged;
	}

	public static BaseDialog OpenAppleIdRequiredDialog(Action callback)
	{
		string title = "dlgWarning";
		string message = "dlg_appleID_required";
		string okText = "OK";
		string empty = string.Empty;
		return OpenSimpleDialog(title, message, okText, empty, (object result) =>
		{
			appleIdWarningAcknowledged = true;
			callback();
		}, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
	}

	public static BaseDialog OpenNoInternetValidationDialog(Action callback)
	{
		string title = "Error";
		string message = "Error_validation_nointernet";
		string okText = "OK";
		string empty = string.Empty;
		return OpenSimpleDialog(title, message, okText, empty, (object result) =>
		{
			callback();
		}, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
	}

	public static BaseDialog OpenValidationFailedDialog()
	{
		string title = "Error";
		string message = "Error_validation_failed";
		string okText = "OK";
		string empty = string.Empty;
		return OpenSimpleDialog(title, message, okText, empty, (object result) =>
		{
			ApplicationController.Quit();
		}, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
	}

	public static BaseDialog OpenNoNetworkDialog()
	{
		string title = "dlgNotNetworkTitle";
		string message = "dlgNotNetworkMessage";
		string okText = "OK";
		string empty = string.Empty;
		return OpenSimpleDialog(title, message, okText, empty, null, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
	}

	public static BaseDialog OpenDuelLockedDialog()
	{
		string title = "dlgDuelLockedTitle";
		string message = "dlgDuelLockedMessage";
		string okText = "OK";
		string empty = string.Empty;
		return OpenSimpleDialog(title, message, okText, empty, null, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
	}

	public static BaseDialog OpenNotAvailableDialog()
	{
		string title = "dlgNotAvaliableTitle";
		string message = "dlgNotAvaliableMessage";
		string okText = "OK";
		string empty = string.Empty;
		return OpenSimpleDialog(title, message, okText, empty, null, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
	}

	public static BaseDialog OpenExitDialog()
	{
		return DialogsManager.ShowDialog(DialogType.DialogExit, null);
	}

	public static BaseDialog OpenSurrenderDialog(Action<object> _dlg)
	{
		return DialogsManager.ShowDialog(DialogType.DialogExit, new ExitDialogData(true, _dlg));
	}

	public static BaseDialog OpenSettingsDialog()
	{
		return DialogsManager.ShowDialog(DialogType.DialogSettings, null);
	}

	public static BaseDialog OpenAdvancedSettingsDialog()
	{
		return DialogsManager.ShowDialog(DialogType.DialogSettingsAdvenced, null);
	}

	public static BaseDialog OpenNewsDialog(NewsDialogInfo newsInfo)
	{
		return DialogsManager.ShowDialog(DialogType.DialogNews, newsInfo);
	}

	public static void OpenNewsDialog()
	{
		if (!GameUtils.ShowNews || GeneralConfig.CurrentNews.GetItems().Count == 0)
		{
			return;
		}
		List<NewsItem> list = new List<NewsItem>();
		foreach (NewsItem item in GeneralConfig.CurrentNews.GetItems())
		{
			bool flag = item.EndDate < 0 || item.EndDate > GameUtils.GetCurrentTime();
			bool flag2 = !item.WasShown;
			bool flag3 = ListSF.GetInstance().IsAdvertGroupAllowed(item.SpenderTypeId);
			if (item.IsActive && item.IsImageReady && flag && flag2 && flag3)
			{
				item.WasShown = true;
				list.Add(item);
			}
		}
		if (list.Count != 0)
		{
			NewsDialogInfo newsInfo = new NewsDialogInfo(list);
			OpenNewsDialog(newsInfo);
		}
	}

	public static BaseDialog OpenStoryDialog(string portraitName, string title, List<StoryDialogContent> contents, Action<object> callback = null, string cancelText = "CANCEL", bool showCancelButton = false, string okText = "", LabelButton.ButtonColor okButtonColor = LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor cancelButtonColor = LabelButton.ButtonColor.BUTTON_DARK, bool showPortrait = true, bool useEdgeButtons = false)
	{
		for (int i = 0; i < contents.Count; i++)
		{
			StoryDialogContent content = contents[i];
			content.RefreshItemTimer();
		}
		if (contents.Count > 0 && contents[0].CheckTimer && contents[0].Timer <= 0)
		{
			if (callback != null)
			{
				int firstContentId = contents[0].Id;
				callback(firstContentId);
			}
			return null;
		}
		StoryDialogInfo dialogInfo = new StoryDialogInfo(portraitName, title, contents, callback, okText, cancelText, showCancelButton, okButtonColor, cancelButtonColor, showPortrait, useEdgeButtons);
		return DialogsManager.ShowDialog(DialogType.DialogStory, dialogInfo);
	}

	public static void OpenExternalLink()
	{
		OfflineServices.OpenExternalUrl(InternetController.GetRateUrl());
	}
}
