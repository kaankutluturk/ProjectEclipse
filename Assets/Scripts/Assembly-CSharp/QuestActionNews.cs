public class QuestActionNews : QuestAction
{
	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		DialogsOpener.OpenNewsDialog();
		FinishAction();
	}
}
