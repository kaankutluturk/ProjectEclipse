using System;

public class ExitDialogData
{
	public bool IsInFight;

	public Action<object> Dlg;

	public ExitDialogData(bool isInFight = false, Action<object> _dlg = null)
	{
		IsInFight = isInFight;
		Dlg = _dlg;
	}
}
