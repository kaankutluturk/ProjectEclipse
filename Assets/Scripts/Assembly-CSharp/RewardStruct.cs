using System.Xml;

public class RewardStruct
{
	public Reward CommonReward;

	public Reward NormalModeReward;

	public Reward EclipseModeReward;

	public RewardStruct(XmlNode node, ushort moneyExponent, ushort prizeBaseExponent)
	{
		CommonReward = new Reward(node, moneyExponent, prizeBaseExponent);
		XmlNode xmlNode = node["NormalModeReward"];
		if (xmlNode != null)
		{
			NormalModeReward = new Reward(xmlNode, moneyExponent, prizeBaseExponent);
		}
		else
		{
			NormalModeReward = null;
		}
		XmlNode xmlNode2 = node["EclipseModeReward"];
		if (xmlNode2 != null)
		{
			EclipseModeReward = new Reward(xmlNode2, moneyExponent, prizeBaseExponent);
		}
		else
		{
			EclipseModeReward = null;
		}
	}

	public void RandomizeObscuredVars()
	{
		if (CommonReward != null)
		{
			CommonReward.RandomizeObscuredVars();
		}
		if (NormalModeReward != null)
		{
			NormalModeReward.RandomizeObscuredVars();
		}
		if (EclipseModeReward != null)
		{
			EclipseModeReward.RandomizeObscuredVars();
		}
	}

	public RewardPrize GetPrizeForLevel(int level)
	{
		RewardPrize prize = new RewardPrize();
		if (CommonReward != null)
		{
			prize = CommonReward.GetPrizeForLevel(level);
			prize.IsCloned = true;
		}
		Reward modeReward = ((!ListSF.GetRoster().IsEclipseMode()) ? NormalModeReward : EclipseModeReward);
		if (modeReward != null)
		{
			RewardPrize levelPrize = modeReward.GetPrizeForLevel(level);
			levelPrize.IsCloned = true;
			prize.Merge(levelPrize);
		}
		return prize;
	}
}
