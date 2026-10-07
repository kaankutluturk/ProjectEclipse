using System.Xml;

public class WinShockRule : InFightRule
{
	public WinShockRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleWinShock, EJPOJJKKICO, node)
	{
		applianceLosesOnTrigger = false;
		SubscribeEvent(FightEvent.StrikeEvent);
	}

	protected override bool CompareSingle(object data)
	{
		FightData hCPJJKMNMCE = (FightData)data;
		return hCPJJKMNMCE.IsOpponentShocked;
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new WinShockRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
