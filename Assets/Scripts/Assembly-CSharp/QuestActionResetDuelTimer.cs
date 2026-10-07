public class QuestActionResetDuelTimer : QuestAction
{
	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		BattlePeriodic.Reset(false);
		FinishAction();
	}
}
