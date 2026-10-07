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

	public StrangerDialogInfo(string JFJKJIJPJJM, string HFEGNMEEDCF, List<StoryDialogContent> IHMEPGICLGF, float _ratio, Action<object> _dlg = null, string FGJCMOLFFGH = "", string NMFJJEJEHMC = "", string BFNHNNFIBNM = "", LabelButton.ButtonColor FEAEKLBFDPA = LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor ODLCOLGNJFF = LabelButton.ButtonColor.BUTTON_DARK, LabelButton.ButtonColor DGMLFODMFKD = LabelButton.ButtonColor.BUTTON_WHITE, bool HFCFEKNIEEA = true, bool AOFKALBFNNI = false, bool NKPIIFBDEIB = false, bool CJJBDGPDOFF = false, string IAHHOEJJJHP = "")
	{
		PortraitName = JFJKJIJPJJM;
		Title = HFEGNMEEDCF;
		Contents = IHMEPGICLGF;
		Ratio = _ratio;
		StoreButtonText = FGJCMOLFFGH;
		RejectButtonText = NMFJJEJEHMC;
		AcceptButtonText = BFNHNNFIBNM;
		StoreButtonColor = FEAEKLBFDPA;
		RejectButtonColor = ODLCOLGNJFF;
		AcceptButtonColor = DGMLFODMFKD;
		ShowDifficulty = HFCFEKNIEEA;
		UseEdgeButtons = AOFKALBFNNI;
		ShowCheckBox = NKPIIFBDEIB;
		CheckBoxChecked = CJJBDGPDOFF;
		CheckBoxText = IAHHOEJJJHP;
		Dlg = _dlg;
	}
}
