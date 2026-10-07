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

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		title = node.Attributes["Title"].GetStringOrDefault(string.Empty);
		image += node.Attributes["Image"].GetStringOrDefault(string.Empty);
		itemIcon = node.Attributes["ItemIcon"].GetStringOrDefault(string.Empty);
		itemBefore = node.Attributes["ItemBefore"].GetStringOrDefault(string.Empty);
		itemAfter = node.Attributes["ItemAfter"].GetStringOrDefault(string.Empty);
		dialogType = node.Attributes["Type"].GetStringOrDefault("Regular");
		ignoreBack = node.Attributes["IgnoreBack"].ParseBool();
		isMirrored = node.Attributes["Mirrored"].ParseBool();
		readTime = node.Attributes["ReadTime"].ParseFloat(BasicGUI.GetNotificationDefaultReadTime());
		foreach (XmlNode childNode in node.ChildNodes)
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

	public override void Execute(QuestParameters parameters)
	{
		ResetSequences();
		base.Execute(parameters);
		runParameters = parameters;
		isButtonHandled = false;
		float difficulty = GetFightDifficulty();
		Action<object> action = OnButtonPressed;
		string buttonText = ((leftButton == null) ? "dlgStoryNegative" : leftButton.Text);
		string pMDPPGNJAFE2 = ((rightButton == null) ? "dlgStoryPositive" : rightButton.Text);
		string pMDPPGNJAFE3 = ((middleButton == null) ? "dlgButtonFight" : middleButton.Text);
		buttonText = QuestTextResolver.ResolveText(buttonText, parameters);
		pMDPPGNJAFE2 = QuestTextResolver.ResolveText(pMDPPGNJAFE2, parameters);
		pMDPPGNJAFE3 = QuestTextResolver.ResolveText(pMDPPGNJAFE3, parameters);
		LabelButton.ButtonColor buttonColor = ((leftButton == null || !(leftButton.Color != string.Empty)) ? LabelButton.GetBtnColor("Red") : LabelButton.GetBtnColor(leftButton.Color));
		LabelButton.ButtonColor fBMGEHJPPIK2 = ((rightButton == null || !(rightButton.Color != string.Empty)) ? LabelButton.GetBtnColor("Beige") : LabelButton.GetBtnColor(rightButton.Color));
		LabelButton.ButtonColor middleButtonColor = ((middleButton == null || !(middleButton.Color != string.Empty)) ? LabelButton.GetBtnColor("Beige") : LabelButton.GetBtnColor(middleButton.Color));
		string text = ((rightButton == null) ? string.Empty : pMDPPGNJAFE2);
		string text2 = ((leftButton == null) ? string.Empty : buttonText);
		string middleButtonText = ((middleButton == null) ? string.Empty : pMDPPGNJAFE3);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(title, result);
		string text3 = QuestTextResolver.ResolveText(result.resultSTR, parameters);
		condition.SetValue(image, result);
		string imageName = result.resultSTR;
		string empty = string.Empty;
		if (!imageName.Contains("/"))
		{
		}
		empty += imageName;
		if (!imageName.Contains("."))
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
			StoryDialogContent lineContent = new StoryDialogContent(lines[i]);
			if (lineContent.ItemName != string.Empty)
			{
				condition.SetValue(lineContent.ItemName, result);
				lineContent.ItemName = result.resultSTR;
			}
			if (lineContent.EnchantmentName != string.Empty)
			{
				condition.SetValue(lineContent.EnchantmentName, result);
				lineContent.EnchantmentName = result.resultSTR;
			}
			condition.SetValue(lineContent.Text, result);
			lineContent.Text = result.ToString();
			lineContent.Text = QuestTextResolver.ResolveText(lineContent.Text, parameters);
			list.Add(lineContent);
		}
		if (dialogType == "Regular")
		{
			baseDialog = DialogsOpener.OpenStoryDialog(empty, text3, list, action, buttonText, leftButton != null && rightButton != null, pMDPPGNJAFE2, fBMGEHJPPIK2, buttonColor);
		}
		else if (dialogType == "Stranger")
		{
			baseDialog = DialogsOpener.OpenStrangerDialog(empty, text3, list, difficulty, action, text, text2, middleButtonText, fBMGEHJPPIK2, buttonColor, middleButtonColor, true, false, false, false, string.Empty);
		}
		else if (dialogType == "NoAvatar")
		{
			string checkBoxText = ((checkBox == null) ? string.Empty : checkBox.Text);
			bool isCheckBoxChecked = false;
			if (checkBox != null)
			{
				ConditionExtension.CompareResult lNIDLHOIHIM2 = new ConditionExtension.CompareResult();
				QuestCondition kKDGLNECFHA2 = new QuestCondition();
				kKDGLNECFHA2.SetParameters(parameters);
				kKDGLNECFHA2.SetValue(checkBox.InitialValue, lNIDLHOIHIM2);
				isCheckBoxChecked = lNIDLHOIHIM2.resultNumber == 1.0;
			}
			bool hasCheckBox = checkBox != null;
			StoryDialogContent nJEPNCJLPPF2 = list[0];
			string firstLineText = ((list.Count <= 0) ? string.Empty : QuestTextResolver.ResolveText(nJEPNCJLPPF2.Text, parameters));
			baseDialog = DialogsOpener.OpenSimpleDialog(text3, firstLineText, text, text2, action, fBMGEHJPPIK2, buttonColor, hasCheckBox, isCheckBoxChecked, checkBoxText);
		}
		else if (dialogType == "ThreeButtons")
		{
			baseDialog = DialogsOpener.OpenStrangerDialog(empty, text3, list, 0f, action, text, text2, middleButtonText, fBMGEHJPPIK2, buttonColor, middleButtonColor, false, false, false, false, string.Empty);
		}
		else if (dialogType == "Multiline")
		{
			string multilineCheckBoxText = ((checkBox == null) ? string.Empty : checkBox.Text);
			bool isMultilineChecked = false;
			if (checkBox != null)
			{
				ConditionExtension.CompareResult lNIDLHOIHIM3 = new ConditionExtension.CompareResult();
				QuestCondition kKDGLNECFHA3 = new QuestCondition();
				kKDGLNECFHA3.SetParameters(parameters);
				kKDGLNECFHA3.SetValue(checkBox.InitialValue, lNIDLHOIHIM3);
				isMultilineChecked = lNIDLHOIHIM3.resultNumber == 1.0;
			}
			bool hasMultilineCheckBox = checkBox != null;
			baseDialog = DialogsOpener.OpenStrangerDialog(empty, text3, list, 0f, action, text, text2, middleButtonText, fBMGEHJPPIK2, buttonColor, middleButtonColor, false, true, hasMultilineCheckBox, isMultilineChecked, multilineCheckBoxText);
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

	private void ParseLine(XmlNode node)
	{
		StoryDialogContent lineContent = new StoryDialogContent(string.Empty, string.Empty, string.Empty, string.Empty);
		lineContent.Text = node.Attributes["Text"].GetStringOrDefault(string.Empty);
		lineContent.ButtonText = node.Attributes["ButtonText"].GetStringOrDefault(string.Empty);
		lineContent.FontName = node.Attributes["FontName"].GetStringOrDefault(string.Empty);
		lineContent.ItemName = node.Attributes["Item"].GetStringOrDefault(string.Empty);
		lineContent.EnchantmentName = node.Attributes["Enchantment"].GetStringOrDefault(string.Empty);
		string name = node.Name;
		if (name == "PriceLine")
		{
			lineContent.Type = StoryDialogContent.ContentType.CONTENT_TYPE_PRICELINE;
		}
		else
		{
			lineContent.Type = StoryDialogContent.ContentType.CONTENT_TYPE_REGULAR;
		}
		string text = node.Attributes["TextColor"].GetStringOrDefault(string.Empty);
		text = text.Replace("0x", string.Empty);
		text = text.Replace("#", string.Empty);
		if (text.Length == 8)
		{
			byte r = byte.Parse(text.Substring(0, 2), NumberStyles.HexNumber);
			byte g = byte.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
			byte b = byte.Parse(text.Substring(4, 2), NumberStyles.HexNumber);
			byte a = byte.Parse(text.Substring(6, 2), NumberStyles.HexNumber);
			lineContent.FontColor = new Color32(r, g, b, a);
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "Timer")
			{
				lineContent.ItemTimer = CreateTextTimer(childNode, lineContent.FontName);
			}
		}
		if (lineContent.ItemName != null || lineContent.EnchantmentName != null)
		{
			LineActionBinding lineBinding = new LineActionBinding();
			lineBinding.Actions.AddEventListener(1, OnActionComplete);
			lineBinding.Id = timersID++;
			lineContent.Id = lineBinding.Id;
			foreach (XmlNode childNode2 in node.ChildNodes)
			{
				QuestAction childAction = QuestAction.GetClassActionByName(childNode2.Name);
				childAction.QuestName = QuestName;
				childAction.Parse(childNode2);
				lineBinding.Actions.AddAction(childAction);
			}
			lineActions.Add(lineBinding);
		}
		lines.Add(lineContent);
	}

	private void ParseButton(XmlNode node)
	{
		string text = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		string buttonText = node.Attributes["Text"].GetStringOrDefault(string.Empty);
		string buttonColor = node.Attributes["Color"].GetStringOrDefault(string.Empty);
		DialogButton button = null;
		switch (text)
		{
		case "Left":
			leftButton = new DialogButton();
			leftButton.Type = DialogButtonType.BUTTON_TYPE_LEFT;
			button = leftButton;
			break;
		case "Right":
			rightButton = new DialogButton();
			rightButton.Type = DialogButtonType.BUTTON_TYPE_RIGHT;
			button = rightButton;
			break;
		case "Middle":
			middleButton = new DialogButton();
			middleButton.Type = DialogButtonType.BUTTON_TYPE_MIDDLE;
			button = middleButton;
			break;
		default:
			GameLog.Error("Strange typeName %s", text);
			break;
		}
		if (button != null)
		{
			button.Text = buttonText;
			button.Color = buttonColor;
			button.Actions.AddEventListener(1, OnActionComplete);
			ParseActionSequence(node, button.Actions);
		}
		else
		{
			GameLog.Error("button is null");
		}
	}

	private void ParseDifficultyOf(XmlNode node)
	{
		fightExpression = node.Attributes["Fight"].GetStringOrDefault(string.Empty);
	}

	private void ParseCheckBox(XmlNode node)
	{
		checkBox = new DialogCheckBox();
		checkBox.InitialValue = node.Attributes["InitialValue"].GetStringOrDefault(string.Empty);
		checkBox.Text = node.Attributes["Text"].GetStringOrDefault(string.Empty);
		foreach (XmlNode childNode in node.ChildNodes)
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

	private TextTimer CreateTextTimer(XmlNode node, string fontName)
	{
		string text = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		TextTimer textTimer = new TextTimer();
		textTimer.Color = Constants.DialogHeaderColor;
		switch (text)
		{
		case "EnergyRefillTimer":
			textTimer.Delegate = ListSF.GetInstance().UpdateEnergyRefillTimer;
			break;
		case "DuelAccessibilityTimer":
			textTimer.Delegate = ListSF.GetInstance().UpdateDuelAccessibilityTimer;
			break;
		case "DeliveryTimer":
		{
			textTimer.Delegate = ListSF.GetInstance().UpdateDeliveryTimer;
			string itemName = node.Attributes["Item"].GetStringOrDefault(string.Empty);
			UserItem userItem = ListSF.GetUserItem(itemName);
			textTimer.set_Data(userItem);
			break;
		}
		case "StarterPackTimer":
			textTimer.Delegate = ListSF.GetInstance().UpdateStartPackTimer;
			break;
		default:
			textTimer.Delegate = ListSF.GetInstance().UpdateCustomRosterTimer;
			textTimer.set_Data(text);
			break;
		}
		textTimer.set_Label(null);
		return textTimer;
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
			ConditionExtension.CompareResult valueResult = new ConditionExtension.CompareResult();
			QuestCondition condition = new QuestCondition();
			condition.SetParameters(runParameters);
			condition.SetValue(fightExpression, valueResult);
			FightIDS fightIds = new FightIDS();
			fightIds.SetFightIDSByString(valueResult.resultSTR);
			FightList fight = ListSF.GetFightById(fightIds);
			if (fight != null)
			{
				List<ModelParameters> list = GameUtils.CreateOpponentParameters(fight.GetOpponents());
				ModelParameters playerParameters = GameUtils.GetPlayerModelParameters();
				result = fight.CalculateDifficultyVsLastOpponent(playerParameters, list);
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
				LineActionBinding lineBinding = lineActions[i];
				if (lineBinding.Id == num)
				{
					lineBinding.Actions.Run(runParameters);
					break;
				}
			}
		}
	}

	private void OnActionComplete(object data)
	{
		FinishAction();
	}

	protected virtual void ParseActionSequence(XmlNode node, QuestActionsSequence sequence)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string name = childNode.Name;
			QuestAction childAction = QuestAction.GetClassActionByName(name);
			childAction.QuestName = QuestName;
			childAction.Parse(childNode);
			sequence.AddAction(childAction);
		}
	}
}
