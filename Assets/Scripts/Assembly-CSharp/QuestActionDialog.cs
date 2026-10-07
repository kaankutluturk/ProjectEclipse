using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Nekki.SF2.GUI.Dialogs;
using UnityEngine;

public class QuestActionDialog : QuestAction
{
	public enum DialogButtonType
	{
		BUTTON_TYPE_NONE = 0,
		BUTTON_TYPE_LEFT = 1,
		BUTTON_TYPE_RIGHT = 2,
		BUTTON_TYPE_MIDDLE = 3
	}

	public class DialogButton
	{
		public DialogButtonType Type;

		public string Text = string.Empty;

		public QuestActionsSequence Actions = new QuestActionsSequence();

		public string Color = string.Empty;
	}

	public class LineActionBinding
	{
		public int Id;

		public QuestActionsSequence Actions = new QuestActionsSequence();
	}

	public class DialogCheckBox
	{
		public string InitialValue = string.Empty;

		public string Text = string.Empty;

		public QuestActionsSequence OnActions = new QuestActionsSequence();

		public QuestActionsSequence OffActions = new QuestActionsSequence();
	}

	private string title = string.Empty;

	private string image = string.Empty;

	private string dialogType = string.Empty;

	private string itemIcon = string.Empty;

	private string itemBefore = string.Empty;

	private string itemAfter = string.Empty;

	private bool isMirrored;

	private bool ignoreBack;

	private bool isButtonHandled;

	private float readTime;

	private List<StoryDialogContent> lines = new List<StoryDialogContent>();

	private DialogButton leftButton;

	private DialogButton rightButton;

	private DialogButton middleButton;

	private List<LineActionBinding> lineActions = new List<LineActionBinding>();

	private string timerName = string.Empty;

	private DialogCheckBox checkBox;

	private int timersID = 5;

	private string fightExpression = string.Empty;

	private QuestParameters runParameters;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		title = EPKLCPOEELO.Attributes["Title"].GetStringOrDefault(string.Empty);
		image += EPKLCPOEELO.Attributes["Image"].GetStringOrDefault(string.Empty);
		itemIcon = EPKLCPOEELO.Attributes["ItemIcon"].GetStringOrDefault(string.Empty);
		itemBefore = EPKLCPOEELO.Attributes["ItemBefore"].GetStringOrDefault(string.Empty);
		itemAfter = EPKLCPOEELO.Attributes["ItemAfter"].GetStringOrDefault(string.Empty);
		dialogType = EPKLCPOEELO.Attributes["Type"].GetStringOrDefault("Regular");
		ignoreBack = EPKLCPOEELO.Attributes["IgnoreBack"].ParseBool();
		isMirrored = EPKLCPOEELO.Attributes["Mirrored"].ParseBool();
		readTime = EPKLCPOEELO.Attributes["ReadTime"].ParseFloat(BasicGUI.GetNotificationDefaultReadTime());
		foreach (XmlNode childNode in EPKLCPOEELO.ChildNodes)
		{
			switch (childNode.Name)
			{
			case "Line":
			case "DeliveryDelay":
			case "PriceLine":
				ParseLine(childNode);
				break;
			case "Button":
				ParseButton(childNode);
				break;
			case "DifficultyOf":
				ParseDifficultyOf(childNode);
				break;
			case "CheckBox":
				ParseCheckBox(childNode);
				break;
			case "Timer":
				timerName = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
				break;
			}
		}
	}

	public override void Execute(QuestParameters JCICKLIMBEF)
	{
		ResetSequences();
		base.Execute(JCICKLIMBEF);
		runParameters = JCICKLIMBEF;
		isButtonHandled = false;
		float lDHEHCLPMOK = GetFightDifficulty();
		Action<object> action = OnButtonPressed;
		string pMDPPGNJAFE = ((leftButton == null) ? "dlgStoryNegative" : leftButton.Text);
		string pMDPPGNJAFE2 = ((rightButton == null) ? "dlgStoryPositive" : rightButton.Text);
		string pMDPPGNJAFE3 = ((middleButton == null) ? "dlgButtonFight" : middleButton.Text);
		pMDPPGNJAFE = QuestTextResolver.ResolveText(pMDPPGNJAFE, JCICKLIMBEF);
		pMDPPGNJAFE2 = QuestTextResolver.ResolveText(pMDPPGNJAFE2, JCICKLIMBEF);
		pMDPPGNJAFE3 = QuestTextResolver.ResolveText(pMDPPGNJAFE3, JCICKLIMBEF);
		LabelButton.ButtonColor fBMGEHJPPIK = ((leftButton == null || !(leftButton.Color != string.Empty)) ? LabelButton.GetBtnColor("Red") : LabelButton.GetBtnColor(leftButton.Color));
		LabelButton.ButtonColor fBMGEHJPPIK2 = ((rightButton == null || !(rightButton.Color != string.Empty)) ? LabelButton.GetBtnColor("Beige") : LabelButton.GetBtnColor(rightButton.Color));
		LabelButton.ButtonColor nFDONPAIONH = ((middleButton == null || !(middleButton.Color != string.Empty)) ? LabelButton.GetBtnColor("Beige") : LabelButton.GetBtnColor(middleButton.Color));
		string text = ((rightButton == null) ? string.Empty : pMDPPGNJAFE2);
		string text2 = ((leftButton == null) ? string.Empty : pMDPPGNJAFE);
		string bFNHNNFIBNM = ((middleButton == null) ? string.Empty : pMDPPGNJAFE3);
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(JCICKLIMBEF);
		kKDGLNECFHA.SetValue(title, lNIDLHOIHIM);
		string text3 = QuestTextResolver.ResolveText(lNIDLHOIHIM.resultSTR, JCICKLIMBEF);
		kKDGLNECFHA.SetValue(image, lNIDLHOIHIM);
		string iBBAMMHHBFE = lNIDLHOIHIM.resultSTR;
		string empty = string.Empty;
		if (!iBBAMMHHBFE.Contains("/"))
		{
		}
		empty += iBBAMMHHBFE;
		if (!iBBAMMHHBFE.Contains("."))
		{
			empty += ".png";
		}
		if (isMirrored)
		{
			empty += "|Flip";
		}
		BaseDialog baseDialog = null;
		List<StoryDialogContent> list = new List<StoryDialogContent>();
		for (int i = 0; i < lines.Count; i++)
		{
			StoryDialogContent nJEPNCJLPPF = new StoryDialogContent(lines[i]);
			if (nJEPNCJLPPF.ItemName != string.Empty)
			{
				kKDGLNECFHA.SetValue(nJEPNCJLPPF.ItemName, lNIDLHOIHIM);
				nJEPNCJLPPF.ItemName = lNIDLHOIHIM.resultSTR;
			}
			if (nJEPNCJLPPF.EnchantmentName != string.Empty)
			{
				kKDGLNECFHA.SetValue(nJEPNCJLPPF.EnchantmentName, lNIDLHOIHIM);
				nJEPNCJLPPF.EnchantmentName = lNIDLHOIHIM.resultSTR;
			}
			kKDGLNECFHA.SetValue(nJEPNCJLPPF.Text, lNIDLHOIHIM);
			nJEPNCJLPPF.Text = lNIDLHOIHIM.ToString();
			nJEPNCJLPPF.Text = QuestTextResolver.ResolveText(nJEPNCJLPPF.Text, JCICKLIMBEF);
			list.Add(nJEPNCJLPPF);
		}
		if (dialogType == "Regular")
		{
			baseDialog = DialogsOpener.OpenStoryDialog(empty, text3, list, action, pMDPPGNJAFE, leftButton != null && rightButton != null, pMDPPGNJAFE2, fBMGEHJPPIK2, fBMGEHJPPIK);
		}
		else if (dialogType == "Stranger")
		{
			baseDialog = DialogsOpener.OpenStrangerDialog(empty, text3, list, lDHEHCLPMOK, action, text, text2, bFNHNNFIBNM, fBMGEHJPPIK2, fBMGEHJPPIK, nFDONPAIONH, true, false, false, false, string.Empty);
		}
		else if (dialogType == "NoAvatar")
		{
			string dOEEIGAHKEN = ((checkBox == null) ? string.Empty : checkBox.Text);
			bool ePHHGNKDPEG = false;
			if (checkBox != null)
			{
				ConditionExtension.CompareResult lNIDLHOIHIM2 = new ConditionExtension.CompareResult();
				QuestCondition kKDGLNECFHA2 = new QuestCondition();
				kKDGLNECFHA2.SetParameters(JCICKLIMBEF);
				kKDGLNECFHA2.SetValue(checkBox.InitialValue, lNIDLHOIHIM2);
				ePHHGNKDPEG = lNIDLHOIHIM2.resultNumber == 1.0;
			}
			bool lMAFOFCILBL = checkBox != null;
			StoryDialogContent nJEPNCJLPPF2 = list[0];
			string hCPNFPMHFCM = ((list.Count <= 0) ? string.Empty : QuestTextResolver.ResolveText(nJEPNCJLPPF2.Text, JCICKLIMBEF));
			baseDialog = DialogsOpener.OpenSimpleDialog(text3, hCPNFPMHFCM, text, text2, action, fBMGEHJPPIK2, fBMGEHJPPIK, lMAFOFCILBL, ePHHGNKDPEG, dOEEIGAHKEN);
		}
		else if (dialogType == "ThreeButtons")
		{
			baseDialog = DialogsOpener.OpenStrangerDialog(empty, text3, list, 0f, action, text, text2, bFNHNNFIBNM, fBMGEHJPPIK2, fBMGEHJPPIK, nFDONPAIONH, false, false, false, false, string.Empty);
		}
		else if (dialogType == "Multiline")
		{
			string iAHHOEJJJHP = ((checkBox == null) ? string.Empty : checkBox.Text);
			bool cJJBDGPDOFF = false;
			if (checkBox != null)
			{
				ConditionExtension.CompareResult lNIDLHOIHIM3 = new ConditionExtension.CompareResult();
				QuestCondition kKDGLNECFHA3 = new QuestCondition();
				kKDGLNECFHA3.SetParameters(JCICKLIMBEF);
				kKDGLNECFHA3.SetValue(checkBox.InitialValue, lNIDLHOIHIM3);
				cJJBDGPDOFF = lNIDLHOIHIM3.resultNumber == 1.0;
			}
			bool nKPIIFBDEIB = checkBox != null;
			baseDialog = DialogsOpener.OpenStrangerDialog(empty, text3, list, 0f, action, text, text2, bFNHNNFIBNM, fBMGEHJPPIK2, fBMGEHJPPIK, nFDONPAIONH, false, true, nKPIIFBDEIB, cJJBDGPDOFF, iAHHOEJJJHP);
		}
		else if (dialogType == "Notification")
		{
			NotificationsGame.get_Instance().OpenNotification(empty, list, action, text, fBMGEHJPPIK2, readTime);
		}
		if (baseDialog != null)
		{
			baseDialog.IsIgnoreBack = ignoreBack;
			baseDialog.IsQuestDialog = true;
		}
		else
		{
			FinishAction();
		}
	}

	public override void ResetSequences()
	{
		if (leftButton != null)
		{
			leftButton.Actions.Reset();
		}
		if (rightButton != null)
		{
			rightButton.Actions.Reset();
		}
		if (middleButton != null)
		{
			middleButton.Actions.Reset();
		}
		for (int i = 0; i < lineActions.Count; i++)
		{
			lineActions[i].Actions.Reset();
		}
		ResetCheckBoxSequences();
	}

	private void ParseLine(XmlNode EPKLCPOEELO)
	{
		StoryDialogContent nJEPNCJLPPF = new StoryDialogContent(string.Empty, string.Empty, string.Empty, string.Empty);
		nJEPNCJLPPF.Text = EPKLCPOEELO.Attributes["Text"].GetStringOrDefault(string.Empty);
		nJEPNCJLPPF.ButtonText = EPKLCPOEELO.Attributes["ButtonText"].GetStringOrDefault(string.Empty);
		nJEPNCJLPPF.FontName = EPKLCPOEELO.Attributes["FontName"].GetStringOrDefault(string.Empty);
		nJEPNCJLPPF.ItemName = EPKLCPOEELO.Attributes["Item"].GetStringOrDefault(string.Empty);
		nJEPNCJLPPF.EnchantmentName = EPKLCPOEELO.Attributes["Enchantment"].GetStringOrDefault(string.Empty);
		string name = EPKLCPOEELO.Name;
		if (name == "PriceLine")
		{
			nJEPNCJLPPF.Type = StoryDialogContent.ContentType.CONTENT_TYPE_PRICELINE;
		}
		else
		{
			nJEPNCJLPPF.Type = StoryDialogContent.ContentType.CONTENT_TYPE_REGULAR;
		}
		string text = EPKLCPOEELO.Attributes["TextColor"].GetStringOrDefault(string.Empty);
		text = text.Replace("0x", string.Empty);
		text = text.Replace("#", string.Empty);
		if (text.Length == 8)
		{
			byte r = byte.Parse(text.Substring(0, 2), NumberStyles.HexNumber);
			byte g = byte.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
			byte b = byte.Parse(text.Substring(4, 2), NumberStyles.HexNumber);
			byte a = byte.Parse(text.Substring(6, 2), NumberStyles.HexNumber);
			nJEPNCJLPPF.FontColor = new Color32(r, g, b, a);
		}
		foreach (XmlNode childNode in EPKLCPOEELO.ChildNodes)
		{
			if (childNode.Name == "Timer")
			{
				nJEPNCJLPPF.ItemTimer = CreateTextTimer(childNode, nJEPNCJLPPF.FontName);
			}
		}
		if (nJEPNCJLPPF.ItemName != null || nJEPNCJLPPF.EnchantmentName != null)
		{
			LineActionBinding cLNIMHCJIAL = new LineActionBinding();
			cLNIMHCJIAL.Actions.AddEventListener(1, OnActionComplete);
			cLNIMHCJIAL.Id = timersID++;
			nJEPNCJLPPF.Id = cLNIMHCJIAL.Id;
			foreach (XmlNode childNode2 in EPKLCPOEELO.ChildNodes)
			{
				QuestAction mBAAKHELFKL = QuestAction.GetClassActionByName(childNode2.Name);
				mBAAKHELFKL.QuestName = QuestName;
				mBAAKHELFKL.Parse(childNode2);
				cLNIMHCJIAL.Actions.AddAction(mBAAKHELFKL);
			}
			lineActions.Add(cLNIMHCJIAL);
		}
		lines.Add(nJEPNCJLPPF);
	}

	private void ParseButton(XmlNode EPKLCPOEELO)
	{
		string text = EPKLCPOEELO.Attributes["Type"].GetStringOrDefault(string.Empty);
		string gGDJIPKMKFC = EPKLCPOEELO.Attributes["Text"].GetStringOrDefault(string.Empty);
		string mDADHHOFCNG = EPKLCPOEELO.Attributes["Color"].GetStringOrDefault(string.Empty);
		DialogButton fFIBFAFPEGF = null;
		switch (text)
		{
		case "Left":
			leftButton = new DialogButton();
			leftButton.Type = DialogButtonType.BUTTON_TYPE_LEFT;
			fFIBFAFPEGF = leftButton;
			break;
		case "Right":
			rightButton = new DialogButton();
			rightButton.Type = DialogButtonType.BUTTON_TYPE_RIGHT;
			fFIBFAFPEGF = rightButton;
			break;
		case "Middle":
			middleButton = new DialogButton();
			middleButton.Type = DialogButtonType.BUTTON_TYPE_MIDDLE;
			fFIBFAFPEGF = middleButton;
			break;
		default:
			GameLog.Error("Strange typeName %s", text);
			break;
		}
		if (fFIBFAFPEGF != null)
		{
			fFIBFAFPEGF.Text = gGDJIPKMKFC;
			fFIBFAFPEGF.Color = mDADHHOFCNG;
			fFIBFAFPEGF.Actions.AddEventListener(1, OnActionComplete);
			ParseActionSequence(EPKLCPOEELO, fFIBFAFPEGF.Actions);
		}
		else
		{
			GameLog.Error("button is null");
		}
	}

	private void ParseDifficultyOf(XmlNode EPKLCPOEELO)
	{
		fightExpression = EPKLCPOEELO.Attributes["Fight"].GetStringOrDefault(string.Empty);
	}

	private void ParseCheckBox(XmlNode EPKLCPOEELO)
	{
		checkBox = new DialogCheckBox();
		checkBox.InitialValue = EPKLCPOEELO.Attributes["InitialValue"].GetStringOrDefault(string.Empty);
		checkBox.Text = EPKLCPOEELO.Attributes["Text"].GetStringOrDefault(string.Empty);
		foreach (XmlNode childNode in EPKLCPOEELO.ChildNodes)
		{
			string name = childNode.Name;
			if (name == "On")
			{
				ParseActionSequence(childNode, checkBox.OnActions);
			}
			else if (name == "Off")
			{
				ParseActionSequence(childNode, checkBox.OffActions);
			}
		}
	}

	private TextTimer CreateTextTimer(XmlNode node, string IFHPLGGBDPM)
	{
		string text = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		TextTimer dKPAACCMAPO = new TextTimer();
		dKPAACCMAPO.Color = Constants.DialogHeaderColor;
		switch (text)
		{
		case "EnergyRefillTimer":
			dKPAACCMAPO.Delegate = ListSF.GetInstance().UpdateEnergyRefillTimer;
			break;
		case "DuelAccessibilityTimer":
			dKPAACCMAPO.Delegate = ListSF.GetInstance().UpdateDuelAccessibilityTimer;
			break;
		case "DeliveryTimer":
		{
			dKPAACCMAPO.Delegate = ListSF.GetInstance().UpdateDeliveryTimer;
			string gOHIIMFFFJI = node.Attributes["Item"].GetStringOrDefault(string.Empty);
			UserItem bAINMLLIKOL = ListSF.GetUserItem(gOHIIMFFFJI);
			dKPAACCMAPO.set_Data(bAINMLLIKOL);
			break;
		}
		case "StarterPackTimer":
			dKPAACCMAPO.Delegate = ListSF.GetInstance().UpdateStartPackTimer;
			break;
		default:
			dKPAACCMAPO.Delegate = ListSF.GetInstance().UpdateCustomRosterTimer;
			dKPAACCMAPO.set_Data(text);
			break;
		}
		dKPAACCMAPO.set_Label(null);
		return dKPAACCMAPO;
	}

	private void ResetCheckBoxSequences()
	{
		if (checkBox != null)
		{
			checkBox.OnActions.Reset();
			checkBox.OffActions.Reset();
		}
	}

	private float GetFightDifficulty()
	{
		float result = -1f;
		if (fightExpression != string.Empty)
		{
			ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
			QuestCondition kKDGLNECFHA = new QuestCondition();
			kKDGLNECFHA.SetParameters(runParameters);
			kKDGLNECFHA.SetValue(fightExpression, lNIDLHOIHIM);
			FightIDS mOCEDDJOAEB = new FightIDS();
			mOCEDDJOAEB.SetFightIDSByString(lNIDLHOIHIM.resultSTR);
			FightList jDIPBIHBGPF = ListSF.GetFightById(mOCEDDJOAEB);
			if (jDIPBIHBGPF != null)
			{
				List<ModelParameters> list = GameUtils.CreateOpponentParameters(jDIPBIHBGPF.GetOpponents());
				ModelParameters aCENLMONNPA = GameUtils.GetPlayerModelParameters();
				result = jDIPBIHBGPF.CalculateDifficultyVsLastOpponent(aCENLMONNPA, list);
				list.Clear();
			}
		}
		return result;
	}

	private void OnButtonPressed(object data)
	{
		if (isButtonHandled)
		{
			return;
		}
		int num = ((data != null) ? ((int)data) : 0);
		if (num == 0 && leftButton != null)
		{
			isButtonHandled = true;
			leftButton.Actions.Run(runParameters);
		}
		else if (num == 0 && leftButton == null)
		{
			isButtonHandled = true;
			FinishAction();
		}
		else if (num == 1 && rightButton != null)
		{
			isButtonHandled = true;
			rightButton.Actions.Run(runParameters);
		}
		else if (num == 2 && middleButton != null)
		{
			isButtonHandled = true;
			middleButton.Actions.Run(runParameters);
		}
		else if (num == 3 && checkBox != null)
		{
			checkBox.OnActions.Run(runParameters);
			ResetCheckBoxSequences();
		}
		else if (num == 4 && checkBox != null)
		{
			checkBox.OffActions.Run(runParameters);
			ResetCheckBoxSequences();
		}
		else
		{
			if (num >= timersID)
			{
				return;
			}
			for (int i = 0; i < lineActions.Count; i++)
			{
				LineActionBinding cLNIMHCJIAL = lineActions[i];
				if (cLNIMHCJIAL.Id == num)
				{
					cLNIMHCJIAL.Actions.Run(runParameters);
					break;
				}
			}
		}
	}

	private void OnActionComplete(object data)
	{
		FinishAction();
	}

	protected virtual void ParseActionSequence(XmlNode EPKLCPOEELO, QuestActionsSequence AFENHJFICNN)
	{
		foreach (XmlNode childNode in EPKLCPOEELO.ChildNodes)
		{
			string name = childNode.Name;
			QuestAction mBAAKHELFKL = QuestAction.GetClassActionByName(name);
			mBAAKHELFKL.QuestName = QuestName;
			mBAAKHELFKL.Parse(childNode);
			AFENHJFICNN.AddAction(mBAAKHELFKL);
		}
	}
}
