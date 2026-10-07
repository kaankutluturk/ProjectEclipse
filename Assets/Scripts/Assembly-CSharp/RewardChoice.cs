using System.Collections.Generic;
using System.Xml;

public class RewardChoice
{
	public struct WeightedReward
	{
		public Rewardable reward;

		public float weight;

		public WeightedReward(XmlNode node)
		{
			weight = node.Attributes["Weight"].ParseFloat(1f);
			switch (node.Name)
			{
			case "Item":
				reward = new RewardItem(node);
				break;
			case "Money":
				reward = new RewardMoney(node);
				break;
			case "Currency":
				reward = new RewardCurrency(node);
				break;
			case "Resistance":
				reward = new RewardResistance(node);
				break;
			case "Lottery":
				reward = new RewardLottery(node, 0, 0);
				break;
			default:
				reward = null;
				break;
			}
		}
	}

	private List<WeightedReward> choices = new List<WeightedReward>();

	public RewardChoice(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			WeightedReward item = new WeightedReward(childNode);
			choices.Add(item);
		}
	}

	public Rewardable ChooseRandomReward()
	{
		float num = 0f;
		List<float> list = new List<float>();
		list.Add(0f);
		foreach (WeightedReward item in choices)
		{
			num += item.weight;
			list.Add(num);
		}
		int index = 0;
		float num2 = NekkiMath.randomFloat(0f, num);
		int i = 0;
		for (int num3 = list.Count - 1; i < num3; i++)
		{
			if (list[i] <= num2 && list[i + 1] >= num2)
			{
				index = i;
				break;
			}
		}
		return choices[index].reward;
	}
}
