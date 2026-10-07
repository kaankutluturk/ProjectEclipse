using System.Xml;

public struct LotteryPrizeEntry
{
	public enum PrizeCountType
	{
		prizeCountNone = 0,
		prizeCountMoney = 1,
		prizeCountCurrency = 2
	}

	private PrizeCountType prizeCountType;

	private Reward reward;

	public int MinLevel;

	public int MaxLevel;

	private string image;

	private string cancellingItem;

	private string viewType;

	internal float Weight { get; private set; }
	internal string Image => image;
	internal string CancellingItem => cancellingItem;
	internal string ViewType => viewType;
	internal bool IsAvailableAtLevel(int level) => reward != null && IsLevelInRange(level);

	internal bool TryEvaluateAtLevel(int level, out RewardPrize prize)
	{
		prize = null;
		if (!IsAvailableAtLevel(level)) return false;
		prize = reward.GetPrizeForLevel(level);
		return true;
	}

	public LotteryPrizeEntry(XmlNode node, ushort CDCJKJNGPOE, ushort MCDAHGPLLDO)
	{
		reward = null;
		MinLevel = -1;
		MaxLevel = -1;
		reward = new Reward(node, CDCJKJNGPOE, MCDAHGPLLDO);
		image = node.Attributes["Image"].GetStringOrDefault(string.Empty);
		cancellingItem = node.Attributes["CancellingItem"].GetStringOrDefault(string.Empty);
		viewType = node.Attributes["ViewType"].GetStringOrDefault(string.Empty);
		Weight = node.Attributes["Weight"].ParseFloat(1f);
		if (node["Money"] != null)
		{
			prizeCountType = PrizeCountType.prizeCountMoney;
		}
		else if (node["Currency"] != null)
		{
			prizeCountType = PrizeCountType.prizeCountCurrency;
		}
		else
		{
			prizeCountType = PrizeCountType.prizeCountNone;
		}
	}

	private bool IsLevelInRange(int GNLOCMLBNHF)
	{
		if (MinLevel < 0 && MaxLevel < 0)
		{
			return true;
		}
		if (MinLevel <= GNLOCMLBNHF && MaxLevel >= GNLOCMLBNHF)
		{
			return true;
		}
		if (MinLevel < 0 && MaxLevel >= GNLOCMLBNHF)
		{
			return true;
		}
		if (MinLevel <= GNLOCMLBNHF && MaxLevel < 0)
		{
			return true;
		}
		return false;
	}
}
