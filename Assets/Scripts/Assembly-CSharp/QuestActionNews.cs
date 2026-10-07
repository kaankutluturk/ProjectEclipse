public class QuestActionNews : QuestAction
{
	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		DialogsOpener.OpenNewsDialog();
		FinishAction();
	}
}
