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
		ConditionExtension.CompareResult lNIDLHOIHIM2 = new ConditionExtension.CompareResult();
		ConditionExtension.CompareResult lNIDLHOIHIM3 = new ConditionExtension.CompareResult();
		ConditionExtension.CompareResult lNIDLHOIHIM4 = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(productionExpression, result);
		condition.SetValue(majorExpression, lNIDLHOIHIM2);
		condition.SetValue(minorExpression, lNIDLHOIHIM3);
		condition.SetValue(dataVersionExpression, lNIDLHOIHIM4);
		string empty = string.Empty;
		empty += result.ToString();
		empty += ".";
		empty += lNIDLHOIHIM2.ToString();
		empty += ".";
		empty += lNIDLHOIHIM3.ToString();
		empty += ".";
		empty += lNIDLHOIHIM4.ToString();
		SystemProperties.GetDataVersion().SetVersion(empty);
		ListSF.GetInstance().SetDataVersion(empty);
		FinishAction();
	}
}
