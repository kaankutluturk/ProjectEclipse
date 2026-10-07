using System.Xml;

public class LifeStealRule : InFightRule
{
	private float damagePart;

	private float lastLifeStolen;

	public LifeStealRule(XmlNode node, RuleAppliance ruleAppliance)
		: base(RuleType.RuleLifeSteal, ruleAppliance, node)
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
		FightData fightData = (FightData)data;
		FightEvent fightEvent = fightData.FightEventType;
		if (fightEvent == FightEvent.StrikeEvent)
		{
			lastLifeStolen = fightData.DamageDealt * damagePart;
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
		InFightRule copy = null;
		RuleAppliance ruleAppliance = GetAppliance();
		XmlNode sourceNode = GetXmlSource().GetNode();
		copy = new LifeStealRule(sourceNode, ruleAppliance);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
