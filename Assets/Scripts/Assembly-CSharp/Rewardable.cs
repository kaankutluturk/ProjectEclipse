using System.Xml;

public class Rewardable
{
	public enum RewardKind
	{
		REWARD_NOTHING = 0,
		REWARD_ITEM = 1,
		REWARD_MONEY = 2,
		REWARD_CURRENCY = 3,
		REWARD_RESISTANCE = 4,
		REWARD_LOTTERY = 5
	}

	public RewardKind Kind;

	public bool IsDrop;

	public bool ShowReward;

	public virtual void Parse(XmlNode node)
	{
		IsDrop = node.Attributes["Drop"].ParseBool();
		ShowReward = node.Attributes["ShowReward"].ParseBool();
	}
}
