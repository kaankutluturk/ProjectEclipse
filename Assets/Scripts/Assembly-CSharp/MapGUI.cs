using System.Collections.Generic;
using System.Xml;

public class MapGUI
{
	public struct ChallengeFadeSettings
	{
		public int MinOpacity;

		public int FadeSpeed;

		public int DelayBeforeFade;

		public int DifficultyIsFirstFrame;
	}

	public struct RaidInfoFadeSettings
	{
		public int MinOpacity;

		public int FadeSpeed;

		public int DelayBeforeFade;

		public int PrizeIsFirstFrame;
	}

	public struct ZoneSwitchFadeSettings
	{
		public int MinOpacity;

		public int FadeSpeed;

		public int DelayBeforeFade;

		public List<string> BattleTypeNames;
	}

	public struct RewardLineOscillationSettings
	{
		public float OscillationPeriod;

		public float OscillationFactor;
	}

	public static ChallengeFadeSettings ChallengeFade = default(ChallengeFadeSettings);

	public static ZoneSwitchFadeSettings ZoneSwitchFade = default(ZoneSwitchFadeSettings);

	public static RewardLineOscillationSettings RewardLineOscillation = default(RewardLineOscillationSettings);

	public static RaidInfoFadeSettings RaidInfoFade = default(RaidInfoFadeSettings);

	public static void Parse(XmlNode node)
	{
		if (node == null)
		{
			return;
		}
		XmlNode xmlNode = node["RewardLine"];
		RewardLineOscillation.OscillationPeriod = xmlNode["OscillationPeriod"].Attributes["Value"].ParseFloat();
		RewardLineOscillation.OscillationFactor = xmlNode["OscillationFactor"].Attributes["Value"].ParseFloat();
		XmlNode xmlNode2 = node["Challenge"];
		ChallengeFade.MinOpacity = xmlNode2["MinOpacity"].FirstAttribute().ParseInt();
		ChallengeFade.FadeSpeed = xmlNode2["FadeSpeed"].FirstAttribute().ParseInt();
		ChallengeFade.DelayBeforeFade = xmlNode2["DelayBeforeFade"].Attributes["Value"].ParseInt();
		ChallengeFade.DifficultyIsFirstFrame = xmlNode2["DifficultyIsFirstFrame"].Attributes["Value"].ParseInt();
		XmlNode xmlNode3 = node["RaidInfo"];
		if (xmlNode3 != null)
		{
			RaidInfoFade.MinOpacity = xmlNode3["MinOpacity"].FirstAttribute().ParseInt();
			RaidInfoFade.FadeSpeed = xmlNode3["FadeSpeed"].FirstAttribute().ParseInt();
			RaidInfoFade.DelayBeforeFade = xmlNode3["DelayBeforeFade"].Attributes["Value"].ParseInt();
			RaidInfoFade.PrizeIsFirstFrame = xmlNode3["PrizeIsFirstFrame"].Attributes["Value"].ParseInt();
		}
		XmlNode xmlNode4 = node["ZoneSwitch"];
		ZoneSwitchFade.MinOpacity = xmlNode4["MinOpacity"].Attributes["Value"].ParseInt();
		ZoneSwitchFade.FadeSpeed = xmlNode4["FadeSpeed"].Attributes["Value"].ParseInt();
		ZoneSwitchFade.DelayBeforeFade = xmlNode4["DelayBeforeFade"].Attributes["Value"].ParseInt();
		ZoneSwitchFade.BattleTypeNames = new List<string>();
		XmlNode xmlNode5 = xmlNode4["BattleTypes"];
		foreach (XmlNode childNode in xmlNode5.ChildNodes)
		{
			string item = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			ZoneSwitchFade.BattleTypeNames.Add(item);
		}
	}
}
