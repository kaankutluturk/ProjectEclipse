public class QuestActionCheckPoint : QuestAction
{
	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		SaveCheckPoint(GFIHPBCEEOB);
		ListSF.GetInstance().RequestSave();
		FinishAction();
	}

	public void SaveCheckPoint(QuestParameters GFIHPBCEEOB)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		RosterQuest dKBDLDGOFDN = nKGLHEGIKKP.FindQuest(QuestName);
		if (dKBDLDGOFDN == null)
		{
			dKBDLDGOFDN = ListSF.GetRoster().AddQuest(QuestName, QuestFileName);
			CallEvent(2, dKBDLDGOFDN);
		}
		dKBDLDGOFDN.SaveCheckpoint(GFIHPBCEEOB, StageIndex, Index);
		dKBDLDGOFDN.FileName = QuestFileName;
		ListSF.GetInstance().RequestSave();
	}
}
