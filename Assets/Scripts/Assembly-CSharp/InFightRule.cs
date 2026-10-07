using System.Collections.Generic;
using System.Xml;

public class InFightRule : Rule
{
    internal virtual System.Action PrepareModelRebind(Model expected, Model replacement) { return null; }
	protected RuleAppliance appliance;

	protected HashSet<FightEvent> subscribedEvents;

	protected bool isDeathRule;

	protected bool applianceLosesOnTrigger;

	public InFightRule(RuleType ruleType, RuleAppliance ruleAppliance, XmlNode node, InFightRule sourceRule = null)
		: base(ruleType, node)
	{
		appliance = ruleAppliance;
		applianceLosesOnTrigger = true;
		isDeathRule = false;
		subscribedEvents = new HashSet<FightEvent>();
		isDeathRule = node.Attributes["Death"].ParseBool();
	}

	public virtual InFightRule Copy()
	{
		return null;
	}

	public void SubscribeEvent(FightEvent fightEvent)
	{
		subscribedEvents.Add(fightEvent);
	}

	public bool IsSubscribedTo(FightEvent fightEvent)
	{
		return subscribedEvents.Contains(fightEvent);
	}

	public virtual void Reset()
	{
	}

	public override bool Compare(object data)
	{
		PrepareCompare(data);
		PlayersFightData playersData = (PlayersFightData)data;
		if (appliance == RuleAppliance.AppliancePlayer)
		{
			return CompareSingle(playersData.PlayerData);
		}
		if (appliance == RuleAppliance.ApplianceOpponent)
		{
			return CompareSingle(playersData.EnemyData);
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

	public void SetAppliance(RuleAppliance ruleAppliance)
	{
		appliance = ruleAppliance;
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
