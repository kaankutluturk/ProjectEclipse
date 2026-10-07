using System.Xml;

public class QuestActionActivateTimer : QuestAction
{
	private string timerName;

	private string durationExpression;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		timerName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		durationExpression = node.Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(durationExpression, result);
		long num = (long)result.resultNumber;
		long num2 = ListSF.GetCurrentTime();
		Roster roster = ListSF.GetRoster();
		RosterTimerContainer timerContainer = roster.GetTimerContainer();
		timerContainer.AddTimer(timerName, num2 + num);
		FinishAction();
	}
}
