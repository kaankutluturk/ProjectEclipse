using System.Collections.Generic;
using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class RewardPrize
{
	public int number;

	public ObscuredLong money = (ObscuredLong)(0L);

	public ObscuredLong bonus = (ObscuredLong)(0L);

	public ObscuredUInt exp = (ObscuredUInt)(0u);

	public ObscuredFloat prizeBase = (ObscuredFloat)(-1f);

	public List<RewardItem> items = new List<RewardItem>();

	public List<RewardMoney> moneyRewards = new List<RewardMoney>();

	public List<RewardCurrency> currencyRewards = new List<RewardCurrency>();

	public List<RewardResistance> resistanceRewards = new List<RewardResistance>();

	public List<RewardChoice> choices = new List<RewardChoice>();

	public RewardLottery lottery;

	public bool IsCloned;

	public void Parse(XmlNode node, ushort moneyExponent = 0, ushort prizeBaseExponent = 0)
	{
		number = node.Attributes["Number"].ParseInt();
		money = (ObscuredLong)(node.Attributes["Money"].ParseLong(0L) * (long)Mathf.Pow(10f, (int)moneyExponent));
		bonus = (ObscuredLong)(node.Attributes["Bonus"].ParseLong(0L));
		exp = (ObscuredUInt)(node.Attributes["Exp"].ParseUint());
		prizeBase = (ObscuredFloat)(node.Attributes["PrizeBase"].ParseFloat(-1f));
		if ((ObscuredFloat)(prizeBase) != -1f)
		{
			prizeBase = (ObscuredFloat)((ObscuredFloat)(prizeBase) * Mathf.Pow(10f, (int)prizeBaseExponent));
		}
		foreach (XmlNode item6 in node.SelectNodes("Money"))
		{
			RewardMoney item = new RewardMoney(item6);
			moneyRewards.Add(item);
		}
		foreach (XmlNode item7 in node.SelectNodes("Currency"))
		{
			RewardCurrency item2 = new RewardCurrency(item7);
			currencyRewards.Add(item2);
		}
		foreach (XmlNode item8 in node.SelectNodes("Resistance"))
		{
			RewardResistance item3 = new RewardResistance(item8);
			resistanceRewards.Add(item3);
		}
		foreach (XmlNode item9 in node.SelectNodes("Lottery"))
		{
			RewardLottery rewardLottery = new RewardLottery(item9, moneyExponent, prizeBaseExponent);
			if (lottery == null)
			{
				lottery = rewardLottery;
			}
		}
		foreach (XmlNode item10 in node.SelectNodes("Item"))
		{
			RewardItem item4 = new RewardItem(item10);
			items.Add(item4);
		}
		foreach (XmlNode item11 in node.SelectNodes("Choice"))
		{
			RewardChoice item5 = new RewardChoice(item11);
			choices.Add(item5);
		}
	}

	public void Merge(RewardPrize otherPrize)
	{
		bonus = (ObscuredLong)((ObscuredLong)(bonus) + (ObscuredLong)(otherPrize.bonus));
		money = (ObscuredLong)((ObscuredLong)(money) + (ObscuredLong)(otherPrize.money));
		exp = (ObscuredUInt)((ObscuredUInt)(exp) + (ObscuredUInt)(otherPrize.exp));
		if ((ObscuredFloat)(prizeBase) < 0f)
		{
			prizeBase = otherPrize.prizeBase;
		}
		else if ((ObscuredFloat)(otherPrize.prizeBase) > 0f)
		{
			prizeBase = (ObscuredFloat)((ObscuredFloat)(prizeBase) + (ObscuredFloat)(otherPrize.prizeBase));
		}
		items.AddRange(otherPrize.items);
		moneyRewards.AddRange(otherPrize.moneyRewards);
		currencyRewards.AddRange(otherPrize.currencyRewards);
		resistanceRewards.AddRange(otherPrize.resistanceRewards);
		choices.AddRange(otherPrize.choices);
		if (otherPrize.lottery != null)
		{
			if (lottery != null)
			{
				lottery.slots.AddRange(otherPrize.lottery.slots);
			}
			else
			{
				lottery = otherPrize.lottery.CloneForRewardComposition();
			}
		}
	}

	public void RandomizeObscuredVars()
	{
		money.RandomizeCryptoKey();
		bonus.RandomizeCryptoKey();
		exp.RandomizeCryptoKey();
		prizeBase.RandomizeCryptoKey();
	}
}
