using System.Collections.Generic;
using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;

public class PerkConditionHealth : PerkConditionMatchMinMax
{
	public PerkConditionHealth()
	{
		set_Type(PerkConditionType.CONDITION_HEALTH);
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
			GameLog.Error("PerkConditionHealth::isEqual - model is null");
			return false;
		}
		minMax.EvaluateFunctions();
		float num = (ObscuredFloat)(targetModel.Parameters.GetCurrentLife());
		if (!minMax.GetMinUnbounded() && minMax.GetMinValue() > num)
		{
			return false;
		}
		if (!minMax.GetMaxUnbounded() && minMax.GetMaxValue() < num)
		{
			return false;
		}
		return true;
	}
}
