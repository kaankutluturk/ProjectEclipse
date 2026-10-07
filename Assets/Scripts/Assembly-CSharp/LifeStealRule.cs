using System.Xml;

public class LifeStealRule : InFightRule
{
	private float damagePart;

	private float lastLifeStolen;

	public LifeStealRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleLifeSteal, EJPOJJKKICO, node)
	{
		lastLifeStolen = 0f;
		SubscribeEvent(FightEvent.StrikeEvent);
		Parse(node);
		Reset();
	}

	public float GetLastLifeStolen()
	{
		return lastLifeStolen;
	}

	protected override bool CompareSingle(object data)
	{
		FightData hCPJJKMNMCE = (FightData)data;
		FightEvent kOJNCHKPLLN = hCPJJKMNMCE.FightEventType;
		if (kOJNCHKPLLN == FightEvent.StrikeEvent)
		{
			lastLifeStolen = hCPJJKMNMCE.DamageDealt * damagePart;
			return lastLifeStolen != 0f;
		}
		return false;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		damagePart = node.Attributes["DamagePart"].ParseFloat();
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new LifeStealRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
