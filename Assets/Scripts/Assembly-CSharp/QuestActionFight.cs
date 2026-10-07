using System.Xml;

public class QuestActionFight : QuestAction
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
		ListSF.GetInstance().ClearQuestsStack();
		if (string.IsNullOrEmpty(_name))
		{
			// The newer quest graph deliberately uses <Fight /> in the error branch of
			// ScriptsResumeOnStart.  It means "there was no interrupted fight to
			// resume, return to the normal game screen", rather than a malformed fight.
			Module.OpenScreen(ScreenType.ModuleDojo);
			FinishAction();
			return;
		}
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		kKDGLNECFHA.SetValue(_name, lNIDLHOIHIM);
		string bAINMLLIKOL = lNIDLHOIHIM.ToString();
		FightIDS mOCEDDJOAEB = new FightIDS();
		mOCEDDJOAEB.SetFightIDSByString(bAINMLLIKOL);
		FightList jDIPBIHBGPF = ListSF.GetFightById(mOCEDDJOAEB);
		if (jDIPBIHBGPF != null)
		{
			GameUtils.StartFight(jDIPBIHBGPF);
		}
		else
		{
			Module.OpenScreen(ScreenType.ModuleDojo);
		}
		FinishAction();
	}
}
