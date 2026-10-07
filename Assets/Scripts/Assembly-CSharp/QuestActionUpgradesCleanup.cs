public class QuestActionUpgradesCleanup : QuestAction
{
	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ListSF.GetRoster().GetInventory().CorrectUpgradeLevels();
		FinishAction();
	}
}
