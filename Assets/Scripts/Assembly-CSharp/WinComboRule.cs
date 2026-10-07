using System.Xml;

public class WinComboRule : InFightRule
{
	private int requiredComboLevel;

	public WinComboRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleWinCombo, EJPOJJKKICO, node)
	{
		Parse(node);
		applianceLosesOnTrigger = false;
		SubscribeEvent(FightEvent.ComboEvent);
	}

	protected override bool CompareSingle(object data)
	{
		FightData hCPJJKMNMCE = (FightData)data;
		return hCPJJKMNMCE.currentComboLevel >= requiredComboLevel;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		requiredComboLevel = node.Attributes["Value"].ParseInt();
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new WinComboRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
