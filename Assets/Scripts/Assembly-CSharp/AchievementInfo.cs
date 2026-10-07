using System;

public class AchievementInfo
{
	public string Title;

	public string Description;

	public int BonusPrize;

	public int MoneyPrize;

	public Action<object> OnTakeReward;

	public bool CanTakeReward;

	public bool IsCompleted;

	public AchievementInfo(string PGAFPNEHHLB, string EFBADJCNDMG, int MJBFFBPLAGC, int BDONIKLHFLJ, Action<object> _dlg = null, bool BODCOGFGHAD = false, bool DPJOPMHPGKG = false)
	{
		Title = PGAFPNEHHLB;
		Description = EFBADJCNDMG;
		MoneyPrize = MJBFFBPLAGC;
		BonusPrize = BDONIKLHFLJ;
		OnTakeReward = _dlg;
		CanTakeReward = BODCOGFGHAD;
		IsCompleted = DPJOPMHPGKG;
	}
}
