public class QuestActionShowCredits : QuestAction
{
	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		CreditsScreen.Create(base.FinishAction);
	}
}
