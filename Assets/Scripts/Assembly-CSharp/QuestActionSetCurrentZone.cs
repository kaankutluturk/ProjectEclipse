using System.Xml;

public class QuestActionSetCurrentZone : QuestAction
{
	private string zoneNameExpression = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		zoneNameExpression = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		condition.SetValue(zoneNameExpression, result);
		ListSF.GetRoster().SetCurrentZone(result.ToString());
		FinishAction();
	}
}
