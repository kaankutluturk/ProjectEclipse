using System.Xml;

public class NoHealthBarRule : InFightRule
{
	public NoHealthBarRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleNoHealthBar, EJPOJJKKICO, node)
	{
		Parse(node);
	}

	protected override bool CompareSingle(object data)
	{
		return false;
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new NoHealthBarRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
