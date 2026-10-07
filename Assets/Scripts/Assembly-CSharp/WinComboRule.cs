using System.Xml;

public class WinComboRule : InFightRule
{
	private int requiredComboLevel;

	public WinComboRule(XmlNode node, RuleAppliance ruleAppliance)
		: base(RuleType.RuleWinCombo, ruleAppliance, node)
	{
		Parse(node);
		applianceLosesOnTrigger = false;
		SubscribeEvent(FightEvent.ComboEvent);
	}

	protected override bool CompareSingle(object data)
	{
		FightData fightData = (FightData)data;
		return fightData.currentComboLevel >= requiredComboLevel;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		requiredComboLevel = node.Attributes["Value"].ParseInt();
	}

	public override InFightRule Copy()
	{
		InFightRule ruleCopy = null;
		RuleAppliance ruleAppliance = GetAppliance();
		XmlNode sourceNode = GetXmlSource().GetNode();
		ruleCopy = new WinComboRule(sourceNode, ruleAppliance);
		ruleCopy.IsRandom = IsRandom;
		return ruleCopy;
	}
}
