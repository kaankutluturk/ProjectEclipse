using System.Xml;

public class DamageRule : InFightRule
{
	protected bool isNoDamage;

	public DamageRule(XmlNode node, RuleAppliance ruleAppliance, RuleType ruleType)
		: base(ruleType, ruleAppliance, node)
	{
		isNoDamage = false;
		SubscribeEvent(FightEvent.DamageCheckEvent);
	}

	public bool IsNoDamage()
	{
		return isNoDamage;
	}

	public override void InitRule(object data)
	{
		RuleInitData initData = (RuleInitData)data;
		Compare(initData.FightData);
	}

	protected override bool CompareSingle(object data)
	{
		return false;
	}

	protected virtual bool CheckIsNoDamageChange(bool noDamage)
	{
		bool result = isNoDamage != noDamage;
		isNoDamage = noDamage;
		return result;
	}

	protected virtual void SwapAppliance()
	{
		if (appliance == RuleAppliance.AppliancePlayer)
		{
			appliance = RuleAppliance.ApplianceOpponent;
		}
		else if (appliance == RuleAppliance.ApplianceOpponent)
		{
			appliance = RuleAppliance.AppliancePlayer;
		}
	}

	public override InFightRule Copy()
	{
		InFightRule copy = null;
		RuleAppliance ruleAppliance = GetAppliance();
		XmlNode sourceNode = GetXmlSource().GetNode();
		copy = new DamageRule(sourceNode, ruleAppliance, _type);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
