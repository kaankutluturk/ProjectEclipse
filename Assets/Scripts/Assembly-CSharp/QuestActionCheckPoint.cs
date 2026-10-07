public class QuestActionCheckPoint : QuestAction
{
	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		SaveCheckPoint(parameters);
		ListSF.GetInstance().RequestSave();
		FinishAction();
	}

	public void SaveCheckPoint(QuestParameters parameters)
	{
		Roster roster = ListSF.GetRoster();
		RosterQuest rosterQuest = roster.FindQuest(QuestName);
		if (rosterQuest == null)
		{
			rosterQuest = ListSF.GetRoster().AddQuest(QuestName, QuestFileName);
			CallEvent(2, rosterQuest);
		}
		rosterQuest.SaveCheckpoint(parameters, StageIndex, Index);
		rosterQuest.FileName = QuestFileName;
		ListSF.GetInstance().RequestSave();
	}
}
