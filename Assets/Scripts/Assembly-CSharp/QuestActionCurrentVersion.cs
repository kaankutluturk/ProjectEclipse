using System.Xml;

public class QuestActionCurrentVersion : QuestAction
{
	private string productionExpression = string.Empty;

	private string majorExpression = string.Empty;

	private string minorExpression = string.Empty;

	private string dataVersionExpression = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		productionExpression = EPKLCPOEELO.Attributes["Production"].GetStringOrDefault(string.Empty);
		majorExpression = EPKLCPOEELO.Attributes["Major"].GetStringOrDefault(string.Empty);
		minorExpression = EPKLCPOEELO.Attributes["Minor"].GetStringOrDefault(string.Empty);
		dataVersionExpression = EPKLCPOEELO.Attributes["DataVersion"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		ConditionExtension.CompareResult lNIDLHOIHIM2 = new ConditionExtension.CompareResult();
		ConditionExtension.CompareResult lNIDLHOIHIM3 = new ConditionExtension.CompareResult();
		ConditionExtension.CompareResult lNIDLHOIHIM4 = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		kKDGLNECFHA.SetValue(productionExpression, lNIDLHOIHIM);
		kKDGLNECFHA.SetValue(majorExpression, lNIDLHOIHIM2);
		kKDGLNECFHA.SetValue(minorExpression, lNIDLHOIHIM3);
		kKDGLNECFHA.SetValue(dataVersionExpression, lNIDLHOIHIM4);
		string empty = string.Empty;
		empty += lNIDLHOIHIM.ToString();
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
