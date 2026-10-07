using System.Collections.Generic;
using System.Xml;

public class PerkConditionPain : PerkConditionMatchMinMax
{
	public PerkConditionPain()
	{
		set_Type(PerkConditionType.CONDITION_PAIN);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		minMax.Parse(node, this, GetPerk());
	}

	public override bool IsEqual(Model ACENLMONNPA, List<string> NIKHAICFGNM)
	{
		Model fGCODGKLHED = ResolveTargetModel(ACENLMONNPA);
		if (ACENLMONNPA == null)
		{
			return false;
		}
		minMax.EvaluateFunctions();
		float num = fGCODGKLHED.GetPain();
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
