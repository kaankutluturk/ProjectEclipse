public class QuestActionResetPerks : QuestAction
{
	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		Roster roster = ListSF.GetRoster();
		if (roster != null)
		{
			roster.GetPerks().ResetPerks();
		}
		FinishAction();
	}
}
