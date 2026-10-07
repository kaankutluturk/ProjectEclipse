public class QuestActionResetDuelTimer : QuestAction
{
	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		BattlePeriodic.Reset(false);
		FinishAction();
	}
}
