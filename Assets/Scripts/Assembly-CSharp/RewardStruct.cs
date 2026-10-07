using System.Xml;

public class RewardStruct
{
	public Reward CommonReward;

	public Reward NormalModeReward;

	public Reward EclipseModeReward;

	public RewardStruct(XmlNode node, ushort CDCJKJNGPOE, ushort MCDAHGPLLDO)
	{
		CommonReward = new Reward(node, CDCJKJNGPOE, MCDAHGPLLDO);
		XmlNode xmlNode = node["NormalModeReward"];
		if (xmlNode != null)
		{
			NormalModeReward = new Reward(xmlNode, CDCJKJNGPOE, MCDAHGPLLDO);
		}
		else
		{
			NormalModeReward = null;
		}
		XmlNode xmlNode2 = node["EclipseModeReward"];
		if (xmlNode2 != null)
		{
			EclipseModeReward = new Reward(xmlNode2, CDCJKJNGPOE, MCDAHGPLLDO);
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

	public RewardPrize GetPrizeForLevel(int GNLOCMLBNHF)
	{
		RewardPrize cMHHEHILIIH = new RewardPrize();
		if (CommonReward != null)
		{
			cMHHEHILIIH = CommonReward.GetPrizeForLevel(GNLOCMLBNHF);
			cMHHEHILIIH.IsCloned = true;
		}
		Reward lOELDGJGPIF = ((!ListSF.GetRoster().IsEclipseMode()) ? NormalModeReward : EclipseModeReward);
		if (lOELDGJGPIF != null)
		{
			RewardPrize cMHHEHILIIH2 = lOELDGJGPIF.GetPrizeForLevel(GNLOCMLBNHF);
			cMHHEHILIIH2.IsCloned = true;
			cMHHEHILIIH.Merge(cMHHEHILIIH2);
		}
		return cMHHEHILIIH;
	}
}
