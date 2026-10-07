using System.Xml;

public class RemoveIntervalRule : InFightRule
{
	private IntervalAnimation.IntervalType intervalType;

	public RemoveIntervalRule(XmlNode node, RuleAppliance appliance)
		: base(RuleType.RuleRemoveInterval, appliance, node)
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
		RuleInitData initData = (RuleInitData)data;
		switch (appliance)
		{
		case RuleAppliance.AppliancePlayer:
			initData.PlayerModel.SuppressInterval(intervalType);
			break;
		case RuleAppliance.ApplianceOpponent:
			initData.OpponentModel.SuppressInterval(intervalType);
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
		InFightRule copy = null;
		RuleAppliance appliance = GetAppliance();
		XmlNode node = GetXmlSource().GetNode();
		copy = new RemoveIntervalRule(node, appliance);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
