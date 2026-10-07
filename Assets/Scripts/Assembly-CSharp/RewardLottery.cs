using System.Collections.Generic;
using System.Xml;

public class RewardLottery : Rewardable
{
	private string lotteryType;
	internal string LotteryType => lotteryType;

	public List<LotteryPrizeEntry> slots = new List<LotteryPrizeEntry>();

	internal RewardLottery CloneForRewardComposition()
	{
		var clone = (RewardLottery)MemberwiseClone();
		clone.slots = new List<LotteryPrizeEntry>(slots);
		return clone;
	}

	public RewardLottery(XmlNode node, ushort CDCJKJNGPOE, ushort MCDAHGPLLDO)
	{
		Kind = RewardKind.REWARD_LOTTERY;
		lotteryType = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "Slot")
			{
				LotteryPrizeEntry item = new LotteryPrizeEntry(childNode, CDCJKJNGPOE, MCDAHGPLLDO);
				slots.Add(item);
			}
			else
			{
				if (!(childNode.Name == "Level"))
				{
					continue;
				}
				int bDJKDCMHEBI = childNode.Attributes["Min"].ParseInt(-1);
				int cIKLDJLOFDJ = childNode.Attributes["Max"].ParseInt(-1);
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					LotteryPrizeEntry item2 = new LotteryPrizeEntry(childNode2, CDCJKJNGPOE, MCDAHGPLLDO)
					{
						MinLevel = bDJKDCMHEBI,
						MaxLevel = cIKLDJLOFDJ
					};
					slots.Add(item2);
				}
			}
		}
	}
}
