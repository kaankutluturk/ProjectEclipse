using System.Xml;

public class QuestActionSetLanguage : QuestAction
{
	private string languageExpression;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		languageExpression = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		kKDGLNECFHA.SetValue(languageExpression, lNIDLHOIHIM);
		string kEEACJILEEK = lNIDLHOIHIM.ToString();
		LocalizationManager.Language pPNFBAFOOAH = LocalizationManager.FindLanguageByName(kEEACJILEEK);
		if (pPNFBAFOOAH != null)
		{
			LocalizationManager.ChangeLanguage(pPNFBAFOOAH);
		}
		FinishAction();
	}
}
