public class QuestActionForge : QuestAction
{
	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		Roster roster = ListSF.GetRoster();
		if (roster != null)
		{
			roster.SetShowForge(true);
			ListSF.GetInstance().RequestSave();
		}
		FinishAction();
	}
}
