public class QuestActionForge : QuestAction
{
	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		Roster roster = ListSF.GetRoster();
		if (roster != null)
		{
			roster.SetShowForge(true);
			ListSF.GetInstance().RequestSave();
		}
		FinishAction();
	}
}
