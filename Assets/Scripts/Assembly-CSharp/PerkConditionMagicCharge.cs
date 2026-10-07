using System.Collections.Generic;
using System.Xml;

public class PerkConditionMagicCharge : PerkConditionMatchMinMax
{
	private string _chargeType;

	public PerkConditionMagicCharge()
	{
		set_Type(PerkConditionType.CONDITION_MAGIC_CHARGE);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		minMax.Parse(node, this, GetPerk());
		_chargeType = node.Attributes["Type"].GetStringOrDefault(string.Empty);
	}

	public override bool IsEqual(Model model, List<string> args)
	{
		Model targetModel = ResolveTargetModel(model);
		if (model == null)
		{
			return false;
		}
		minMax.EvaluateFunctions();
		float num = targetModel.GetMagicChargeFraction();
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
