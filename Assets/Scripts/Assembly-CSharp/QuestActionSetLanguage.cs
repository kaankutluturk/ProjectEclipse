using System.Xml;

public class QuestActionSetLanguage : QuestAction
{
	private string languageExpression;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		languageExpression = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(languageExpression, result);
		string languageName = result.ToString();
		LocalizationManager.Language language = LocalizationManager.FindLanguageByName(languageName);
		if (language != null)
		{
			LocalizationManager.ChangeLanguage(language);
		}
		FinishAction();
	}
}
