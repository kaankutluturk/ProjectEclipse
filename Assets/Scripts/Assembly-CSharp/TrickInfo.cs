using System;
using System.Collections.Generic;

public class TrickInfo
{
	public string Title;

	public string Description;

	public List<float> AttackDamages;

	public InfoAnimation Animation;

	public Action<object> OnClickCallback;

	public TrickInfo(string PGAFPNEHHLB, InfoAnimation HMMCEHGINBG, List<float> JOGLLIIGDMN, Action<object> _dlg = null, string _description = "")
	{
		Title = PGAFPNEHHLB;
		Description = _description;
		Animation = HMMCEHGINBG;
		AttackDamages = JOGLLIIGDMN;
		OnClickCallback = _dlg;
	}
}
