using System.Collections.Generic;
using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;

public class Reward
{
	private RewardPrize basePrize = new RewardPrize();

	private List<LogMessage> levelPrizes = new List<LogMessage>();

	public Reward(XmlNode node, ushort CDCJKJNGPOE = 0, ushort MCDAHGPLLDO = 0)
	{
		basePrize.Parse(node, CDCJKJNGPOE, MCDAHGPLLDO);
		foreach (XmlNode item2 in node.SelectNodes("Level"))
		{
			LogMessage item = default(LogMessage);
			item.Prize = new RewardPrize();
			item.Prize.Parse(item2, CDCJKJNGPOE, MCDAHGPLLDO);
			item.MinLevel = item2.Attributes["Min"].ParseInt(int.MinValue);
			item.MaxLevel = item2.Attributes["Max"].ParseInt(int.MaxValue);
			item.Prize.IsCloned = true;
			levelPrizes.Add(item);
		}
	}

	public RewardPrize GetPrizeForLevel(int GNLOCMLBNHF)
	{
		RewardPrize cMHHEHILIIH = new RewardPrize();
		cMHHEHILIIH.IsCloned = true;
		cMHHEHILIIH.Merge(basePrize);
		foreach (LogMessage item in levelPrizes)
		{
			if (item.MinLevel <= GNLOCMLBNHF && GNLOCMLBNHF <= item.MaxLevel)
			{
				cMHHEHILIIH.Merge(item.Prize);
			}
		}
		return cMHHEHILIIH;
	}

	public void ApplyDenomination(int NPFOBKBJAOB)
	{
		basePrize.money = (ObscuredLong)(GameUtils.GetDenominatedValue((ObscuredLong)(basePrize.money), NPFOBKBJAOB));
		foreach (LogMessage item in levelPrizes)
		{
			item.Prize.money = (ObscuredLong)(GameUtils.GetDenominatedValue((ObscuredLong)(item.Prize.money), NPFOBKBJAOB));
		}
	}

	public void RandomizeObscuredVars()
	{
		basePrize.RandomizeObscuredVars();
		levelPrizes.ForEach((LogMessage DHDMNHCIPEH) =>
		{
			if (DHDMNHCIPEH.Prize != null)
			{
				DHDMNHCIPEH.Prize.RandomizeObscuredVars();
			}
		});
	}
}
