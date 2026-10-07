using System.Xml;

public class QuestActionVariable : QuestAction
{
	private string _name = string.Empty;

	private string _value = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		_name = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
		_value = EPKLCPOEELO.Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ApplyVariable(GFIHPBCEEOB);
		FinishAction();
	}

	public void ApplyVariable(QuestParameters JCICKLIMBEF)
	{
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(JCICKLIMBEF);
		string bAINMLLIKOL = string.Empty;
		if (!string.IsNullOrEmpty(_value))
		{
			kKDGLNECFHA.SetValue(_value, lNIDLHOIHIM);
			bAINMLLIKOL = lNIDLHOIHIM.ToString();
		}
		lNIDLHOIHIM.Clear();
		kKDGLNECFHA.SetValue(_name, lNIDLHOIHIM);
		string gOHIIMFFFJI = lNIDLHOIHIM.ToString();
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		nKGLHEGIKKP.SetQuestVariable(gOHIIMFFFJI, bAINMLLIKOL);
		ListSF.GetInstance().RequestSave();
	}
}
