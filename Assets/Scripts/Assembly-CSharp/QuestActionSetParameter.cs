using System.Xml;

public class QuestActionSetParameter : QuestAction
{
	private class SetParameterOperands
	{
		public ConditionExtension.CompareResult itemNameResult;

		public ConditionExtension.CompareResult parameterNameResult;

		public ConditionExtension.CompareResult valueResult;
	}

	private string name;

	private string parameterExpression;

	private string value;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		name = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
		parameterExpression = EPKLCPOEELO.Attributes["Parameter"].GetStringOrDefault(string.Empty);
		value = EPKLCPOEELO.Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		SetParameterOperands kJHEFADMIOA = new SetParameterOperands();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		kKDGLNECFHA.SetValue(name, kJHEFADMIOA.itemNameResult);
		kKDGLNECFHA.SetValue(parameterExpression, kJHEFADMIOA.parameterNameResult);
		kKDGLNECFHA.SetValue(value, kJHEFADMIOA.valueResult);
		ApplyParameter(kJHEFADMIOA);
		FinishAction();
	}

	private void ApplyParameter(SetParameterOperands DCJLKCFKCOM)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		UserItem dKCHDHMLKHN = nKGLHEGIKKP.GetInventory().FindItem(DCJLKCFKCOM.itemNameResult.resultSTR);
		if (dKCHDHMLKHN != null)
		{
			string iBBAMMHHBFE = DCJLKCFKCOM.parameterNameResult.resultSTR;
			if (iBBAMMHHBFE.Equals("UpgradeLevel"))
			{
				dKCHDHMLKHN.SetUpgradeLevel((int)DCJLKCFKCOM.valueResult.resultNumber);
				dKCHDHMLKHN.RefreshUpgradeState(nKGLHEGIKKP.GetLevel());
			}
		}
	}
}
