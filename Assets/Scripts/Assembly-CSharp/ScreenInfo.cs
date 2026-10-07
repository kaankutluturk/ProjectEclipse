using System;

public class ScreenInfo
{
	public ScreenType PreviousScreenType = ScreenType.ModuleNone;

	public ScreenType ScreenType = ScreenType.ModuleNone;

	public object Data;

	public Action<object> Dlg;
}
