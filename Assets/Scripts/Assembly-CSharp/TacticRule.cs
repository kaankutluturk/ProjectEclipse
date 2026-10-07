using System.Xml;

public class TacticRule : InFightRule
{
	private string _tacticName;

	public TacticRule(XmlNode node, RuleAppliance ruleAppliance = RuleAppliance.ApplianceOpponent)
		: base(RuleType.RuleTactic, ruleAppliance, node)
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
		InFightRule copiedRule = null;
		RuleAppliance ruleAppliance = GetAppliance();
		XmlNode ruleNode = GetXmlSource().GetNode();
		copiedRule = new TacticRule(ruleNode, ruleAppliance);
		copiedRule.IsRandom = IsRandom;
		return copiedRule;
	}
}
