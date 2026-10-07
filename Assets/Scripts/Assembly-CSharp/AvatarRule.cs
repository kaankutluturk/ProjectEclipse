using System.Diagnostics;
using System.Xml;

public class AvatarRule : Rule
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string name;

	public AvatarRule(XmlNode node)
		: base(RuleType.RuleAvatar, node)
	{
		set_Name(node.Attributes["Name"].GetStringOrDefault(string.Empty));
	}

	public string get_Name()
	{
		return name;
	}

	protected void set_Name(string value)
	{
		name = value;
	}
}
