using System;

public class ExitDialogData
{
	public bool IsInFight;

	public Action<object> Dlg;

	public ExitDialogData(bool FPLFLJDPMMC = false, Action<object> _dlg = null)
	{
		IsInFight = FPLFLJDPMMC;
		Dlg = _dlg;
	}
}
