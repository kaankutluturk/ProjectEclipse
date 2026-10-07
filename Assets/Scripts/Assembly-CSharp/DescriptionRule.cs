using System.Xml;

public class DescriptionRule : Rule
{
	private string _alias = string.Empty;

	public string DescriptionAlias
	{
		get
		{
			return GetDescriptionAlias();
		}
	}

	public DescriptionRule(XmlNode node)
		: base(RuleType.RuleDescription, node)
	{
		Parse(node);
	}

	public string GetDescriptionAlias()
	{
		return _alias;
	}

	protected override void Parse(XmlNode node)
	{
		_alias = node.Attributes["Alias"].GetStringOrDefault(string.Empty);
	}
}
