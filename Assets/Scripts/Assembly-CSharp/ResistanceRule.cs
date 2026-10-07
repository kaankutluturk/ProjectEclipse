using System.Xml;

public class ResistanceRule : InFightRule
{
	private string _resistanceName;

	private int _resistanceValue;

	public ResistanceRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleResistance, RuleAppliance.ApplianceAll, node)
	{
		Parse(node);
		SubscribeEvent(FightEvent.ResistanceCheckEvent);
	}

	public string GetResistanceName()
	{
		return _resistanceName;
	}

	public int GetResistanceValue()
	{
		return _resistanceValue;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		_resistanceName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_resistanceValue = node.Attributes["Value"].ParseInt();
		if (_resistanceValue < 0)
		{
			_resistanceValue = 0;
		}
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new ResistanceRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
