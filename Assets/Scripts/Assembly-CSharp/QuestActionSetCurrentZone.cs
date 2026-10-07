using System.Xml;

public class QuestActionSetCurrentZone : QuestAction
{
	private string zoneNameExpression = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		zoneNameExpression = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		kKDGLNECFHA.SetValue(zoneNameExpression, lNIDLHOIHIM);
		ListSF.GetRoster().SetCurrentZone(lNIDLHOIHIM.ToString());
		FinishAction();
	}
}
