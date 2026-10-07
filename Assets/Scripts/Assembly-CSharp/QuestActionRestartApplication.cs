public class QuestActionRestartApplication : QuestAction
{
	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ListSF.GetInstance().OnAuthenticate(true);
		FinishAction();
		GameUtils.ResetScenes();
	}
}
