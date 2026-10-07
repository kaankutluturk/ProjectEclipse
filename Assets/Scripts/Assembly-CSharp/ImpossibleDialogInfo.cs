using System;
using Nekki.SF2.GUI.Dialogs;

public class ImpossibleDialogInfo
{
	public Action<object> Dlg;

	public ImpossibleDialog.ImpossibleDialogType Reason;

	public object Content;

	public ImpossibleDialogInfo(ImpossibleDialog.ImpossibleDialogType CBFFIFKAHHN, Action<object> _dlg = null, object GCGGIJDKKKO = null)
	{
		Dlg = _dlg;
		Reason = CBFFIFKAHHN;
		Content = GCGGIJDKKKO;
	}
}
