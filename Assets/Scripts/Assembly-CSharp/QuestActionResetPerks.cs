public class QuestActionResetPerks : QuestAction
{
	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP != null)
		{
			nKGLHEGIKKP.GetPerks().ResetPerks();
		}
		FinishAction();
	}
}
