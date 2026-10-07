using System.Collections.Generic;
using System.Xml;

public class PerkConditionCurrentAnimation : PerkConditionMatchMinMax
{
	private string Name;

	public PerkConditionCurrentAnimation()
	{
		set_Type(PerkConditionType.CONDITION_CURRENT_ANIMATION);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		minMax.Parse(node, this, GetPerk());
	}

	public override bool IsEqual(Model model, List<string> args)
	{
		Model targetModel = ResolveTargetModel(model);
		if (model == null)
		{
			return false;
		}
		InfoAnimation currentAnimation = targetModel.GetCurrentAnimation();
		if (currentAnimation == null || !currentAnimation.HasName(Name))
		{
			return false;
		}
		minMax.EvaluateFunctions();
		int num = targetModel.GetCurrentFrame();
		if (!minMax.GetMinUnbounded() && minMax.GetMinValue() > (float)num)
		{
			return false;
		}
		if (!minMax.GetMaxUnbounded() && minMax.GetMaxValue() < (float)num)
		{
			return false;
		}
		return true;
	}
}
