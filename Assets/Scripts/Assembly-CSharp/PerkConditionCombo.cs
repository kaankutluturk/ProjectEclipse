using System.Collections.Generic;
using System.Xml;

public class PerkConditionCombo : PerkConditionMatchMinMax
{
	public PerkConditionCombo()
	{
		set_Type(PerkConditionType.CONDITION_COMBO);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		minMax.Parse(node, this, GetPerk());
	}

	public override bool IsEqual(Model model, List<string> args)
	{
		Model targetModel = ResolveTargetModel(model);
		if (model == null)
		{
			return false;
		}
		minMax.EvaluateFunctions();
		int num = targetModel.GetComboCount();
		if (!minMax.GetMinUnbounded() && (int)minMax.GetMinValue() > num)
		{
			return false;
		}
		if (!minMax.GetMaxUnbounded() && (int)minMax.GetMaxValue() < num)
		{
			return false;
		}
		return true;
	}
}
