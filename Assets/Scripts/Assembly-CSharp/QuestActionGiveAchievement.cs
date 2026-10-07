using System.Xml;

public class QuestActionGiveAchievement : QuestAction
{
	private string achievementName = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		achievementName = node.Attributes["Name"].GetStringOrDefault();
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		Achievement achievement = GameUtils.GiveAchievement(achievementName);
		FinishAction();
	}
}
