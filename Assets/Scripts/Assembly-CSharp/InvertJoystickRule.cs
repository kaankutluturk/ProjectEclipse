using System.Xml;

public class InvertJoystickRule : InFightRule
{
	public InvertJoystickRule(XmlNode node)
		: base(RuleType.RuleInvertJoystick, RuleAppliance.AppliancePlayer, node)
	{
		Parse(node);
	}

	public override InFightRule Copy()
	{
		InFightRule copy = null;
		XmlNode sourceNode = GetXmlSource().GetNode();
		copy = new InvertJoystickRule(sourceNode);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
