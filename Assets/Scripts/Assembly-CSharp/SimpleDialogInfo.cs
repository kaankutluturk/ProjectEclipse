using System;
using Nekki.SF2.GUI.Dialogs;

public class SimpleDialogInfo
{
	public bool UseLiteralText;

	public string Title = string.Empty;

	public string Message = string.Empty;

	public BaseDialog.FooterType FooterType;

	public string OkButtonText = string.Empty;

	public string CancelButtonText = string.Empty;

	public LabelButton.ButtonColor OkButtonStyle = LabelButton.ButtonColor.BUTTON_WHITE;

	public LabelButton.ButtonColor CancelButtonStyle;

	public bool ShowCheckBox;

	public bool CheckBoxChecked;

	public string CheckBoxText = string.Empty;

	public Action<object> Dlg;

	public SimpleDialogInfo(string title, string message, BaseDialog.FooterType footerType, string okButtonText, string cancelButtonText, LabelButton.ButtonColor okButtonStyle, LabelButton.ButtonColor cancelButtonStyle, bool showCheckBox, bool checkBoxChecked, string checkBoxText, Action<object> dialogCallback)
	{
		Title = title;
		Message = message;
		FooterType = footerType;
		OkButtonText = okButtonText;
		CancelButtonText = cancelButtonText;
		OkButtonStyle = okButtonStyle;
		CancelButtonStyle = cancelButtonStyle;
		ShowCheckBox = showCheckBox;
		CheckBoxChecked = checkBoxChecked;
		CheckBoxText = checkBoxText;
		Dlg = dialogCallback;
	}
}
