using System.Xml;

public class QuestActionCurrentVersion : QuestAction
{
	private string productionExpression = string.Empty;

	private string majorExpression = string.Empty;

	private string minorExpression = string.Empty;

	private string dataVersionExpression = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		productionExpression = node.Attributes["Production"].GetStringOrDefault(string.Empty);
		majorExpression = node.Attributes["Major"].GetStringOrDefault(string.Empty);
		minorExpression = node.Attributes["Minor"].GetStringOrDefault(string.Empty);
		dataVersionExpression = node.Attributes["DataVersion"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		ConditionExtension.CompareResult majorResult = new ConditionExtension.CompareResult();
		ConditionExtension.CompareResult minorResult = new ConditionExtension.CompareResult();
		ConditionExtension.CompareResult dataVersionResult = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(productionExpression, result);
		condition.SetValue(majorExpression, majorResult);
		condition.SetValue(minorExpression, minorResult);
		condition.SetValue(dataVersionExpression, dataVersionResult);
		string empty = string.Empty;
		empty += result.ToString();
		empty += ".";
		empty += majorResult.ToString();
		empty += ".";
		empty += minorResult.ToString();
		empty += ".";
		empty += dataVersionResult.ToString();
		SystemProperties.GetDataVersion().SetVersion(empty);
		ListSF.GetInstance().SetDataVersion(empty);
		FinishAction();
	}
}
