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

	public override bool IsEqual(Model ACENLMONNPA, List<string> NIKHAICFGNM)
	{
		Model fGCODGKLHED = ResolveTargetModel(ACENLMONNPA);
		if (ACENLMONNPA == null)
		{
			return false;
		}
		minMax.EvaluateFunctions();
		float num = fGCODGKLHED.GetMagicChargeFraction();
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
