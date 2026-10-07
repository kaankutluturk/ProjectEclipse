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

	public override bool IsEqual(Model ACENLMONNPA, List<string> NIKHAICFGNM)
	{
		Model fGCODGKLHED = ResolveTargetModel(ACENLMONNPA);
		if (ACENLMONNPA == null)
		{
			return false;
		}
		InfoAnimation pJAHIOELGGD = fGCODGKLHED.GetCurrentAnimation();
		if (pJAHIOELGGD == null || !pJAHIOELGGD.HasName(Name))
		{
			return false;
		}
		minMax.EvaluateFunctions();
		int num = fGCODGKLHED.GetCurrentFrame();
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
