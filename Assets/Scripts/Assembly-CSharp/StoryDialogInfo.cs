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

	public StoryDialogInfo(string portraitName, string title, List<StoryDialogContent> contents, Action<object> _dlg = null, string okButtonText = "", string cancelButtonText = "CANCEL", bool showCancelButton = false, LabelButton.ButtonColor okButtonColor = LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor cancelButtonColor = LabelButton.ButtonColor.BUTTON_DARK, bool showPortrait = true, bool useEdgeButtons = false, float scrollWidth = 840f)
	{
		PortraitName = portraitName;
		Title = title;
		Contents = contents;
		Dlg = _dlg;
		OkButtonText = okButtonText;
		CancelButtonText = cancelButtonText;
		OkButtonColor = okButtonColor;
		CancelButtonColor = cancelButtonColor;
		ScrollWidth = scrollWidth;
		ShowCancelButton = showCancelButton;
		ShowPortrait = showPortrait;
		UseEdgeButtons = useEdgeButtons;
	}
}
