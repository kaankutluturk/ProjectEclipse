using System;
using System.Collections.Generic;

public class StoryDialogInfo
{
	public string PortraitName;

	public string Title;

	public List<StoryDialogContent> Contents;

	public Action<object> Dlg;

	public string OkButtonText;

	public string CancelButtonText;

	public LabelButton.ButtonColor OkButtonColor;

	public LabelButton.ButtonColor CancelButtonColor;

	public float ScrollWidth;

	public bool ShowCancelButton;

	public bool ShowPortrait;

	public bool UseEdgeButtons;

	public StoryDialogInfo(string JFJKJIJPJJM, string HFEGNMEEDCF, List<StoryDialogContent> IHMEPGICLGF, Action<object> _dlg = null, string FLDCNEGDMCK = "", string HBBAFHJCHIC = "CANCEL", bool OJMOOJCOGCE = false, LabelButton.ButtonColor JCAOLHHIFEC = LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor OKJBFFAIJPL = LabelButton.ButtonColor.BUTTON_DARK, bool ANIJAKJOHED = true, bool AOFKALBFNNI = false, float OCMLLEDKLFL = 840f)
	{
		PortraitName = JFJKJIJPJJM;
		Title = HFEGNMEEDCF;
		Contents = IHMEPGICLGF;
		Dlg = _dlg;
		OkButtonText = FLDCNEGDMCK;
		CancelButtonText = HBBAFHJCHIC;
		OkButtonColor = JCAOLHHIFEC;
		CancelButtonColor = OKJBFFAIJPL;
		ScrollWidth = OCMLLEDKLFL;
		ShowCancelButton = OJMOOJCOGCE;
		ShowPortrait = ANIJAKJOHED;
		UseEdgeButtons = AOFKALBFNNI;
	}
}
