public class QuestActionRecount : QuestAction
{
	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		GameUtils.RecountAchievements();
		FinishAction();
	}
}
