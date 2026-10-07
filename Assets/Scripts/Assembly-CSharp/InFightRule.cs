using System.Collections.Generic;
using System.Xml;

public class InFightRule : Rule
{
    internal virtual System.Action PrepareModelRebind(Model expected, Model replacement) { return null; }
	protected RuleAppliance appliance;

	protected HashSet<FightEvent> subscribedEvents;

	protected bool isDeathRule;

	protected bool applianceLosesOnTrigger;

	public InFightRule(RuleType LFLGCDNKNJI, RuleAppliance EJPOJJKKICO, XmlNode node, InFightRule CEFOMFMPHJM = null)
		: base(LFLGCDNKNJI, node)
	{
		appliance = EJPOJJKKICO;
		applianceLosesOnTrigger = true;
		isDeathRule = false;
		subscribedEvents = new HashSet<FightEvent>();
		isDeathRule = node.Attributes["Death"].ParseBool();
	}

	public virtual InFightRule Copy()
	{
		return null;
	}

	public void SubscribeEvent(FightEvent KOJNCHKPLLN)
	{
		subscribedEvents.Add(KOJNCHKPLLN);
	}

	public bool IsSubscribedTo(FightEvent KOJNCHKPLLN)
	{
		return subscribedEvents.Contains(KOJNCHKPLLN);
	}

	public virtual void Reset()
	{
	}

	public override bool Compare(object data)
	{
		PrepareCompare(data);
		PlayersFightData jNGGHELCPFM = (PlayersFightData)data;
		if (appliance == RuleAppliance.AppliancePlayer)
		{
			return CompareSingle(jNGGHELCPFM.PlayerData);
		}
		if (appliance == RuleAppliance.ApplianceOpponent)
		{
			return CompareSingle(jNGGHELCPFM.EnemyData);
		}
		return false;
	}

	protected virtual bool CompareSingle(object data)
	{
		return false;
	}

	public virtual void InitRule(object data)
	{
	}

	public virtual void Clear()
	{
	}

	public virtual void Stop()
	{
	}

	public bool DoesApplianceLoseOnTrigger()
	{
		return applianceLosesOnTrigger;
	}

	public bool IsDeathRule()
	{
		return isDeathRule;
	}

	public RuleAppliance GetAppliance()
	{
		return appliance;
	}

	public void SetAppliance(RuleAppliance IGFNCCEHFEK)
	{
		appliance = IGFNCCEHFEK;
	}

	public virtual RuleAppliance GetWinnerAppliance()
	{
		switch (appliance)
		{
		case RuleAppliance.AppliancePlayer:
			return (!applianceLosesOnTrigger) ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent;
		case RuleAppliance.ApplianceOpponent:
			return applianceLosesOnTrigger ? RuleAppliance.AppliancePlayer : RuleAppliance.ApplianceOpponent;
		default:
			GameLog.Error("InFightRule::getWinnerAppliance ERROR  - wrong playerAppliance " + appliance);
			return RuleAppliance.ApplianceAll;
		}
	}

	protected virtual void PrepareCompare(object data)
	{
	}
}
