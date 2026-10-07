using System.Xml;

public class RemoveIntervalRule : InFightRule
{
	private IntervalAnimation.IntervalType intervalType;

	public RemoveIntervalRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleRemoveInterval, EJPOJJKKICO, node)
	{
		intervalType = IntervalAnimation.IntervalType.INTERVAL_NONE;
		Parse(node);
	}

	protected override bool CompareSingle(object data)
	{
		return false;
	}

	public override void InitRule(object data)
	{
		RuleInitData oIFPCFEGFOB = (RuleInitData)data;
		switch (appliance)
		{
		case RuleAppliance.AppliancePlayer:
			oIFPCFEGFOB.PlayerModel.SuppressInterval(intervalType);
			break;
		case RuleAppliance.ApplianceOpponent:
			oIFPCFEGFOB.OpponentModel.SuppressInterval(intervalType);
			break;
		default:
			GameLog.Error("RemoveIntervalRule::initRule - wrong player appliance - %i", appliance);
			break;
		}
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		switch (node.Attributes["Type"].GetStringOrDefault(string.Empty))
		{
		case "Attack":
			intervalType = IntervalAnimation.IntervalType.INTERVAL_ATTACK;
			break;
		case "Block":
			intervalType = IntervalAnimation.IntervalType.INTERVAL_BLOCK;
			break;
		case "Invulnerable":
			intervalType = IntervalAnimation.IntervalType.INTERVAL_INVULNERABLE;
			break;
		case "None":
			intervalType = IntervalAnimation.IntervalType.INTERVAL_NONE;
			break;
		case "SelfUninterrupt":
			intervalType = IntervalAnimation.IntervalType.INTERVAL_SELF_UNINTERRUPT;
			break;
		case "Uninterrupt":
			intervalType = IntervalAnimation.IntervalType.INTERVAL_UNINTERRUPT;
			break;
		case "Unstable":
			intervalType = IntervalAnimation.IntervalType.INTERVAL_UNSTABLE;
			break;
		}
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new RemoveIntervalRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
