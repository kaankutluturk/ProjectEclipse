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
		InFightRule aAJIFBJLJOA = null;
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new InvertJoystickRule(hKPPBKPJOEO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
