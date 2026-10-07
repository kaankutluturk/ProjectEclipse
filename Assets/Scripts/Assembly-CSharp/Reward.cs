using System.Collections.Generic;
using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;

public class Reward
{
	private RewardPrize basePrize = new RewardPrize();

	private List<LogMessage> levelPrizes = new List<LogMessage>();

	public Reward(XmlNode node, ushort moneyExponent = 0, ushort prizeBaseExponent = 0)
	{
		basePrize.Parse(node, moneyExponent, prizeBaseExponent);
		foreach (XmlNode item2 in node.SelectNodes("Level"))
		{
			LogMessage item = default(LogMessage);
			item.Prize = new RewardPrize();
			item.Prize.Parse(item2, moneyExponent, prizeBaseExponent);
			item.MinLevel = item2.Attributes["Min"].ParseInt(int.MinValue);
			item.MaxLevel = item2.Attributes["Max"].ParseInt(int.MaxValue);
			item.Prize.IsCloned = true;
			levelPrizes.Add(item);
		}
	}

	public RewardPrize GetPrizeForLevel(int level)
	{
		RewardPrize prize = new RewardPrize();
		prize.IsCloned = true;
		prize.Merge(basePrize);
		foreach (LogMessage item in levelPrizes)
		{
			if (item.MinLevel <= level && level <= item.MaxLevel)
			{
				prize.Merge(item.Prize);
			}
		}
		return prize;
	}

	public void ApplyDenomination(int digits)
	{
		basePrize.money = (ObscuredLong)(GameUtils.GetDenominatedValue((ObscuredLong)(basePrize.money), digits));
		foreach (LogMessage item in levelPrizes)
		{
			item.Prize.money = (ObscuredLong)(GameUtils.GetDenominatedValue((ObscuredLong)(item.Prize.money), digits));
		}
	}

	public void RandomizeObscuredVars()
	{
		basePrize.RandomizeObscuredVars();
		levelPrizes.ForEach((LogMessage logMessage) =>
		{
			if (logMessage.Prize != null)
			{
				logMessage.Prize.RandomizeObscuredVars();
			}
		});
	}
}
