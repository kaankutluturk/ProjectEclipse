using System.Xml;

public class QuestActionHideMapButton : QuestAction
{
	private string _name = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		_name = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		string name = string.Empty;
		GetValues(ref name);
		MapButtonController.GetInstance().RemoveButton(name);
		FinishAction();
	}

	private void GetValues(ref string name)
	{
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(Parameters);
		kKDGLNECFHA.SetValue(_name, lNIDLHOIHIM);
		name = lNIDLHOIHIM.ToString();
	}
}
