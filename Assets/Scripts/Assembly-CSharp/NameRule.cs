using System.Diagnostics;
using System.Xml;

public class NameRule : Rule
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string ruleName;

	public NameRule(XmlNode node)
		: base(RuleType.RuleName, node)
	{
		set_Name(node.Attributes["Name"].GetStringOrDefault(string.Empty));
	}

	public string get_Name()
	{
		return ruleName;
	}

	protected void set_Name(string value)
	{
		ruleName = value;
	}
}
