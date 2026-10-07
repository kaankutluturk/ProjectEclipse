public class QuestActionUpgradesCleanup : QuestAction
{
	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ListSF.GetRoster().GetInventory().CorrectUpgradeLevels();
		FinishAction();
	}
}
