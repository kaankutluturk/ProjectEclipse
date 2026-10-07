using System.Xml;

public class NoPerksRule : InFightRule
{
	protected string _name = string.Empty;

	public NoPerksRule(XmlNode node, RuleAppliance appliance)
		: base(RuleType.RuleNoPerks, appliance, node)
	{
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public string GetPerkName()
	{
		return _name;
	}

	public override InFightRule Copy()
	{
		InFightRule copy = null;
		RuleAppliance appliance = GetAppliance();
		XmlNode ruleNode = GetXmlSource().GetNode();
		copy = new NoPerksRule(ruleNode, appliance);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
