using System.Xml;

public class RewardResistance : Rewardable
{
	public string Name = string.Empty;

	public int Value;

	public RewardResistance(XmlNode node)
	{
		Parse(node);
		Kind = RewardKind.REWARD_RESISTANCE;
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		Value = node.Attributes["Value"].ParseInt();
	}
}
