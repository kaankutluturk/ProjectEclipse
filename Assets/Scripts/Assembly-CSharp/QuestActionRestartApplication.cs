public class QuestActionRestartApplication : QuestAction
{
	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ListSF.GetInstance().OnAuthenticate(true);
		FinishAction();
		GameUtils.ResetScenes();
	}
}
