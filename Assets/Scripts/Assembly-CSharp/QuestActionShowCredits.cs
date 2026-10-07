public class QuestActionShowCredits : QuestAction
{
	public override void Execute(QuestParameters parameters)
	{
		CreditsScreen.Create(base.FinishAction);
	}
}
