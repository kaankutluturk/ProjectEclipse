using System;
using System.Collections.Generic;

public class TrickInfo
{
	public string Title;

	public string Description;

	public List<float> AttackDamages;

	public InfoAnimation Animation;

	public Action<object> OnClickCallback;

	public TrickInfo(string title, InfoAnimation animation, List<float> attackDamages, Action<object> _dlg = null, string _description = "")
	{
		Title = title;
		Description = _description;
		Animation = animation;
		AttackDamages = attackDamages;
		OnClickCallback = _dlg;
	}
}
