public class QuestActionRecount : QuestAction
{
	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		GameUtils.RecountAchievements();
		FinishAction();
	}
}
