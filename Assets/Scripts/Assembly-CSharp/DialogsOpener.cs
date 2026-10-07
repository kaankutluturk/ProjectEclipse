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

	public static void OpenTradeDialog(TradeDialog.TradeAction IBODMPMJELJ, GameValueType value, long GLGKKGBLFPH, Action<object> ODDEOFKLIAG, long CNIOCCCBDBJ = 0L)
	{
		TradeDialogInfo jGMLAFOPBBC = new TradeDialogInfo(IBODMPMJELJ, value, GLGKKGBLFPH, ODDEOFKLIAG, CNIOCCCBDBJ);
		DialogsManager.ShowDialog(DialogType.DialogBuy, jGMLAFOPBBC);
	}

	public static void OpenImpossibleDialog(ImpossibleDialog.ImpossibleDialogType IBODMPMJELJ, Action<object> ODDEOFKLIAG = null, object DMNBDBJNKME = null)
	{
		ImpossibleDialogInfo jGMLAFOPBBC = new ImpossibleDialogInfo(IBODMPMJELJ, ODDEOFKLIAG, DMNBDBJNKME);
		DialogsManager.ShowDialog(DialogType.DialogImpossible, jGMLAFOPBBC);
	}

	public static BaseDialog OpenStrangerDialog(string JIAKJEOEIMF, string GIBEOPMGOPG, List<StoryDialogContent> PBCJDMAPOOB, float ratio, Action<object> ODDEOFKLIAG = null, string FGJCMOLFFGH = "", string NMFJJEJEHMC = "", string BFNHNNFIBNM = "", LabelButton.ButtonColor IDHJGMKHNOP = LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor CPKKNLPKBIO = LabelButton.ButtonColor.BUTTON_DARK, LabelButton.ButtonColor NFDONPAIONH = LabelButton.ButtonColor.BUTTON_WHITE, bool HNNKHNCELDA = true, bool MHLJPGALMFO = false, bool NKPIIFBDEIB = false, bool CJJBDGPDOFF = false, string IAHHOEJJJHP = "")
	{
		bool flag = true;
		for (int i = 0; i < PBCJDMAPOOB.Count; i++)
		{
			StoryDialogContent nJEPNCJLPPF = PBCJDMAPOOB[i];
			if (!nJEPNCJLPPF.RefreshItemTimer())
			{
				flag = false;
			}
		}
		if ((PBCJDMAPOOB.Count > 0 && PBCJDMAPOOB[0].CheckTimer && PBCJDMAPOOB[0].Timer <= 0) || (MHLJPGALMFO && !flag))
		{
			if (ODDEOFKLIAG != null)
			{
				int oNNLBFAOMMB = PBCJDMAPOOB[0].Id;
				ODDEOFKLIAG(oNNLBFAOMMB);
			}
			return null;
		}
		StrangerDialogInfo jGMLAFOPBBC = new StrangerDialogInfo(JIAKJEOEIMF, GIBEOPMGOPG, PBCJDMAPOOB, ratio, ODDEOFKLIAG, FGJCMOLFFGH, NMFJJEJEHMC, BFNHNNFIBNM, IDHJGMKHNOP, CPKKNLPKBIO, NFDONPAIONH, HNNKHNCELDA, MHLJPGALMFO, NKPIIFBDEIB, CJJBDGPDOFF, IAHHOEJJJHP);
		return DialogsManager.ShowDialog(DialogType.DialogStranger, jGMLAFOPBBC);
	}

	public static BaseDialog OpenSimpleDialog(string HHAAFADDOJB, string HCPNFPMHFCM, string ALOJJLCOGMP, string PAJIOGEINPI = "", Action<object> ODDEOFKLIAG = null, LabelButton.ButtonColor HGAGMJENCNM = LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor PHBOACBIMMF = LabelButton.ButtonColor.BUTTON_DARK, bool LMAFOFCILBL = false, bool EPHHGNKDPEG = false, string DOEEIGAHKEN = "", bool literalText = false)
	{
		BaseDialog.FooterType hJNAHNICGMH = BaseDialog.FooterType.FOOTER_BOTH;
		if (ALOJJLCOGMP == string.Empty || PAJIOGEINPI == string.Empty)
		{
			hJNAHNICGMH = BaseDialog.FooterType.FOOTER_NONE;
			hJNAHNICGMH = ((ALOJJLCOGMP != string.Empty) ? BaseDialog.FooterType.FOOTER_OK : ((PAJIOGEINPI != string.Empty) ? BaseDialog.FooterType.FOOTER_CANCEL : BaseDialog.FooterType.FOOTER_NONE));
		}
		SimpleDialogInfo jGMLAFOPBBC = new SimpleDialogInfo(HHAAFADDOJB, HCPNFPMHFCM, hJNAHNICGMH, ALOJJLCOGMP, PAJIOGEINPI, HGAGMJENCNM, PHBOACBIMMF, LMAFOFCILBL, EPHHGNKDPEG, DOEEIGAHKEN, ODDEOFKLIAG);
		jGMLAFOPBBC.UseLiteralText = literalText;
		BaseDialog baseDialog = DialogsManager.ShowDialog(DialogType.DialogSimple, jGMLAFOPBBC);
		if (ODDEOFKLIAG != null)
		{
			baseDialog.AddEventListener(0, ODDEOFKLIAG);
		}
		return baseDialog;
	}

	public static bool GetAppleIdWarningAcknowledged()
	{
		return appleIdWarningAcknowledged;
	}

	public static BaseDialog OpenAppleIdRequiredDialog(Action JPCNFOHPAOB)
	{
		string hHAAFADDOJB = "dlgWarning";
		string hCPNFPMHFCM = "dlg_appleID_required";
		string aLOJJLCOGMP = "OK";
		string empty = string.Empty;
		return OpenSimpleDialog(hHAAFADDOJB, hCPNFPMHFCM, aLOJJLCOGMP, empty, (object KFBMKMCEMGG) =>
		{
			appleIdWarningAcknowledged = true;
			JPCNFOHPAOB();
		}, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
	}

	public static BaseDialog OpenNoInternetValidationDialog(Action JPCNFOHPAOB)
	{
		string hHAAFADDOJB = "Error";
		string hCPNFPMHFCM = "Error_validation_nointernet";
		string aLOJJLCOGMP = "OK";
		string empty = string.Empty;
		return OpenSimpleDialog(hHAAFADDOJB, hCPNFPMHFCM, aLOJJLCOGMP, empty, (object KFBMKMCEMGG) =>
		{
			JPCNFOHPAOB();
		}, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
	}

	public static BaseDialog OpenValidationFailedDialog()
	{
		string hHAAFADDOJB = "Error";
		string hCPNFPMHFCM = "Error_validation_failed";
		string aLOJJLCOGMP = "OK";
		string empty = string.Empty;
		return OpenSimpleDialog(hHAAFADDOJB, hCPNFPMHFCM, aLOJJLCOGMP, empty, (object KFBMKMCEMGG) =>
		{
			ApplicationController.Quit();
		}, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
	}

	public static BaseDialog OpenNoNetworkDialog()
	{
		string hHAAFADDOJB = "dlgNotNetworkTitle";
		string hCPNFPMHFCM = "dlgNotNetworkMessage";
		string aLOJJLCOGMP = "OK";
		string empty = string.Empty;
		return OpenSimpleDialog(hHAAFADDOJB, hCPNFPMHFCM, aLOJJLCOGMP, empty, null, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
	}

	public static BaseDialog OpenDuelLockedDialog()
	{
		string hHAAFADDOJB = "dlgDuelLockedTitle";
		string hCPNFPMHFCM = "dlgDuelLockedMessage";
		string aLOJJLCOGMP = "OK";
		string empty = string.Empty;
		return OpenSimpleDialog(hHAAFADDOJB, hCPNFPMHFCM, aLOJJLCOGMP, empty, null, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
	}

	public static BaseDialog OpenNotAvailableDialog()
	{
		string hHAAFADDOJB = "dlgNotAvaliableTitle";
		string hCPNFPMHFCM = "dlgNotAvaliableMessage";
		string aLOJJLCOGMP = "OK";
		string empty = string.Empty;
		return OpenSimpleDialog(hHAAFADDOJB, hCPNFPMHFCM, aLOJJLCOGMP, empty, null, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
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

	public static BaseDialog OpenNewsDialog(NewsDialogInfo EMBBNNBFODN)
	{
		return DialogsManager.ShowDialog(DialogType.DialogNews, EMBBNNBFODN);
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
			NewsDialogInfo eMBBNNBFODN = new NewsDialogInfo(list);
			OpenNewsDialog(eMBBNNBFODN);
		}
	}

	public static BaseDialog OpenStoryDialog(string GDLKNAOPKIL, string PEMOECLNECD, List<StoryDialogContent> PBCJDMAPOOB, Action<object> ODDEOFKLIAG = null, string AGEBBHHPFME = "CANCEL", bool IJCBBJHLGFI = false, string NGJFMFPMAFL = "", LabelButton.ButtonColor FHNFKIHDCPC = LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor ICLJIMNHGMN = LabelButton.ButtonColor.BUTTON_DARK, bool JLBJMEGPNPF = true, bool MHLJPGALMFO = false)
	{
		for (int i = 0; i < PBCJDMAPOOB.Count; i++)
		{
			StoryDialogContent nJEPNCJLPPF = PBCJDMAPOOB[i];
			nJEPNCJLPPF.RefreshItemTimer();
		}
		if (PBCJDMAPOOB.Count > 0 && PBCJDMAPOOB[0].CheckTimer && PBCJDMAPOOB[0].Timer <= 0)
		{
			if (ODDEOFKLIAG != null)
			{
				int oNNLBFAOMMB = PBCJDMAPOOB[0].Id;
				ODDEOFKLIAG(oNNLBFAOMMB);
			}
			return null;
		}
		StoryDialogInfo jGMLAFOPBBC = new StoryDialogInfo(GDLKNAOPKIL, PEMOECLNECD, PBCJDMAPOOB, ODDEOFKLIAG, NGJFMFPMAFL, AGEBBHHPFME, IJCBBJHLGFI, FHNFKIHDCPC, ICLJIMNHGMN, JLBJMEGPNPF, MHLJPGALMFO);
		return DialogsManager.ShowDialog(DialogType.DialogStory, jGMLAFOPBBC);
	}

	public static void OpenExternalLink()
	{
		OfflineServices.OpenExternalUrl(InternetController.GetRateUrl());
	}
}
