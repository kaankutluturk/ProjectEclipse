using System;
using System.Collections.Generic;

public class StrangerDialogInfo
{
	public string PortraitName;

	public string Title;

	public List<StoryDialogContent> Contents;

	public float Ratio;

	public string StoreButtonText;

	public string RejectButtonText;

	public string AcceptButtonText;

	public LabelButton.ButtonColor StoreButtonColor;

	public LabelButton.ButtonColor RejectButtonColor;

	public LabelButton.ButtonColor AcceptButtonColor;

	public bool ShowDifficulty;

	public bool UseEdgeButtons;

	public bool ShowCheckBox;

	public bool CheckBoxChecked;

	public string CheckBoxText;

	public Action<object> Dlg;

	public StrangerDialogInfo(string portraitName, string title, List<StoryDialogContent> contents, float _ratio, Action<object> _dlg = null, string storeButtonText = "", string rejectButtonText = "", string acceptButtonText = "", LabelButton.ButtonColor storeButtonColor = LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor rejectButtonColor = LabelButton.ButtonColor.BUTTON_DARK, LabelButton.ButtonColor acceptButtonColor = LabelButton.ButtonColor.BUTTON_WHITE, bool showDifficulty = true, bool useEdgeButtons = false, bool showCheckBox = false, bool checkBoxChecked = false, string checkBoxText = "")
	{
		PortraitName = portraitName;
		Title = title;
		Contents = contents;
		Ratio = _ratio;
		StoreButtonText = storeButtonText;
		RejectButtonText = rejectButtonText;
		AcceptButtonText = acceptButtonText;
		StoreButtonColor = storeButtonColor;
		RejectButtonColor = rejectButtonColor;
		AcceptButtonColor = acceptButtonColor;
		ShowDifficulty = showDifficulty;
		UseEdgeButtons = useEdgeButtons;
		ShowCheckBox = showCheckBox;
		CheckBoxChecked = checkBoxChecked;
		CheckBoxText = checkBoxText;
		Dlg = _dlg;
	}
}
