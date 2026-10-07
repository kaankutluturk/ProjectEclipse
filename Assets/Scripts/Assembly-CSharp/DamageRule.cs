using System.Xml;

public class DamageRule : InFightRule
{
	protected bool isNoDamage;

	public DamageRule(XmlNode node, RuleAppliance EJPOJJKKICO, RuleType LFLGCDNKNJI)
		: base(LFLGCDNKNJI, EJPOJJKKICO, node)
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
		RuleInitData oIFPCFEGFOB = (RuleInitData)data;
		Compare(oIFPCFEGFOB.FightData);
	}

	protected override bool CompareSingle(object data)
	{
		return false;
	}

	protected virtual bool CheckIsNoDamageChange(bool EGDPHJKMGAB)
	{
		bool result = isNoDamage != EGDPHJKMGAB;
		isNoDamage = EGDPHJKMGAB;
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
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new DamageRule(hKPPBKPJOEO, eJPOJJKKICO, _type);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
