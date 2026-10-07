using System.Collections.Generic;
using System.Xml;

public class AnimationListRule : InFightRule
{
	protected List<InfoAnimation> animations = new List<InfoAnimation>();

	public AnimationListRule(RuleType LFLGCDNKNJI, RuleAppliance EJPOJJKKICO, XmlNode node)
		: base(LFLGCDNKNJI, EJPOJJKKICO, node)
	{
		FillAnimations(node);
	}

	public bool CheckAnimation(InfoAnimation DBOLBEOCEME)
	{
		if (DBOLBEOCEME == null)
		{
			return false;
		}
		return CheckAnimation(DBOLBEOCEME.Name);
	}

	public bool CheckAnimation(string name)
	{
		foreach (InfoAnimation item in animations)
		{
			if (item.Name == name || item.HasTemplateName(name))
			{
				return true;
			}
		}
		return false;
	}

	protected void FillAnimations(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "Animation")
			{
				string gOHIIMFFFJI = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
				AnimationData.AddTemplateAnimations(gOHIIMFFFJI, animations);
			}
		}
	}

	public override InFightRule Copy()
	{
		AnimationListRule kCGODLBLCDJ = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		kCGODLBLCDJ = new AnimationListRule(_type, eJPOJJKKICO, hKPPBKPJOEO);
		kCGODLBLCDJ.IsRandom = IsRandom;
		return kCGODLBLCDJ;
	}
}
