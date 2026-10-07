using System.Xml;

public class NoPerksRule : InFightRule
{
	protected string _name = string.Empty;

	public NoPerksRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleNoPerks, EJPOJJKKICO, node)
	{
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public string GetPerkName()
	{
		return _name;
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new NoPerksRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
