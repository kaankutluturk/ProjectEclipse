using System.Collections.Generic;
using System.Xml;

public class ComplexRule : Rule
{
	private List<Rule> _rules = new List<Rule>();

	public ComplexRule(XmlNode node)
		: base(RuleType.RuleComplex, node)
	{
		Parse(node);
	}

	public override void SetActive(bool value)
	{
		base.SetActive(value);
		foreach (Rule item in _rules)
		{
			item.SetActive(value);
		}
	}

	public List<Rule> GetRules()
	{
		return _rules;
	}

	protected override void Parse(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			Rule childRule = RuleParser.ParseRule(childNode);
			if (childRule != null)
			{
				_rules.Add(childRule);
			}
		}
	}
}
