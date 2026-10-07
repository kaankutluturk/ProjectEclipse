using System.Xml;

public class NoAnimationRule : Rule
{
	private string _animationName;

	public NoAnimationRule(XmlNode node)
		: base(RuleType.RuleNoAnimation, node)
	{
		_animationName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public string GetAnimationName()
	{
		return _animationName;
	}
}
