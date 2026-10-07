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

	public SimpleDialogInfo(string HHAAFADDOJB, string HCPNFPMHFCM, BaseDialog.FooterType HJNAHNICGMH, string ALOJJLCOGMP, string PAJIOGEINPI, LabelButton.ButtonColor HGAGMJENCNM, LabelButton.ButtonColor PHBOACBIMMF, bool LMAFOFCILBL, bool KPBBOCBCMBN, string DOEEIGAHKEN, Action<object> ODDEOFKLIAG)
	{
		Title = HHAAFADDOJB;
		Message = HCPNFPMHFCM;
		FooterType = HJNAHNICGMH;
		OkButtonText = ALOJJLCOGMP;
		CancelButtonText = PAJIOGEINPI;
		OkButtonStyle = HGAGMJENCNM;
		CancelButtonStyle = PHBOACBIMMF;
		ShowCheckBox = LMAFOFCILBL;
		CheckBoxChecked = KPBBOCBCMBN;
		CheckBoxText = DOEEIGAHKEN;
		Dlg = ODDEOFKLIAG;
	}
}
