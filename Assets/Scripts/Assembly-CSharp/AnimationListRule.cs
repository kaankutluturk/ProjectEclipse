using System.Collections.Generic;
using System.Xml;

public class AnimationListRule : InFightRule
{
	protected List<InfoAnimation> animations = new List<InfoAnimation>();

	public AnimationListRule(RuleType ruleType, RuleAppliance ruleAppliance, XmlNode node)
		: base(ruleType, ruleAppliance, node)
	{
		FillAnimations(node);
	}

	public bool CheckAnimation(InfoAnimation animation)
	{
		if (animation == null)
		{
			return false;
		}
		return CheckAnimation(animation.Name);
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
				string templateName = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
				AnimationData.AddTemplateAnimations(templateName, animations);
			}
		}
	}

	public override InFightRule Copy()
	{
		AnimationListRule copy = null;
		RuleAppliance ruleAppliance = GetAppliance();
		XmlNode sourceNode = GetXmlSource().GetNode();
		copy = new AnimationListRule(_type, ruleAppliance, sourceNode);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
