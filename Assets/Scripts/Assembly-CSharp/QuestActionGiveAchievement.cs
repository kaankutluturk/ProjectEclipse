using System.Xml;

public class QuestActionGiveAchievement : QuestAction
{
	private string achievementName = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		achievementName = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault();
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		Achievement jNPIOKEKMII = GameUtils.GiveAchievement(achievementName);
		FinishAction();
	}
}
