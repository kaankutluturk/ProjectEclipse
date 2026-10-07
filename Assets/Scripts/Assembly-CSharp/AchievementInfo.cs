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

	public AchievementInfo(string title, string description, int moneyPrize, int bonusPrize, Action<object> _dlg = null, bool canTakeReward = false, bool isCompleted = false)
	{
		Title = title;
		Description = description;
		MoneyPrize = moneyPrize;
		BonusPrize = bonusPrize;
		OnTakeReward = _dlg;
		CanTakeReward = canTakeReward;
		IsCompleted = isCompleted;
	}
}
