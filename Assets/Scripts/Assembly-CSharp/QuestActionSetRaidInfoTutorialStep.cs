using System.Xml;

public class QuestActionSetRaidInfoTutorialStep : QuestAction
{
	private string valueExpression = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		valueExpression = node.Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ListSF.GetRoster().GetTutorials().SetRaidStep(GameUtils.GetRaidTutorialStepByName(valueExpression));
		FinishAction();
	}
}
