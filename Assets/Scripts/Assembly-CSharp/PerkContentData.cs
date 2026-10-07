using System;

public class PerkContentData
{
	public string description;

	public string name;

	public InfoAnimation Animation;

	public ProfilePerk.ProfilePerkState state;

	public Action<object> Callback;

	public float LabelWidth;

	public PerkContentData(string _name = "", string _description = "", ProfilePerk.ProfilePerkState MAFFNGPOMJD = ProfilePerk.ProfilePerkState.PERK_LOCK, Action<object> _dlg = null, InfoAnimation BJONHDGCNFE = null, float GEFOLNHPJMI = -1f)
	{
		name = _name;
		description = _description;
		Callback = _dlg;
		state = MAFFNGPOMJD;
		Animation = BJONHDGCNFE;
		LabelWidth = GEFOLNHPJMI;
	}
}
