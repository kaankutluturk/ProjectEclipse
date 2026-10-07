using System.Xml;

public class QuestActionSetStoryTutorialStep : QuestAction
{
	private string valueExpression = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		valueExpression = EPKLCPOEELO.Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ListSF.GetRoster().GetTutorials().set_StoryTutorialStep(valueExpression);
		FinishAction();
	}
}
