using System;
using Nekki.SF2.GUI.Dialogs;

public class ImpossibleDialogInfo
{
	public Action<object> Dlg;

	public ImpossibleDialog.ImpossibleDialogType Reason;

	public object Content;

	public ImpossibleDialogInfo(ImpossibleDialog.ImpossibleDialogType reason, Action<object> _dlg = null, object content = null)
	{
		Dlg = _dlg;
		Reason = reason;
		Content = content;
	}
}
