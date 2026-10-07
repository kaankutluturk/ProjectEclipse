using System.Xml;

public class TacticRule : InFightRule
{
	private string _tacticName;

	public TacticRule(XmlNode node, RuleAppliance EJPOJJKKICO = RuleAppliance.ApplianceOpponent)
		: base(RuleType.RuleTactic, EJPOJJKKICO, node)
	{
		Parse(node);
	}

	public string GetTacticName()
	{
		return _tacticName;
	}

	protected override void Parse(XmlNode node)
	{
		_tacticName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new TacticRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
