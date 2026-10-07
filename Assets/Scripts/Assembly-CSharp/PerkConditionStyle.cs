using System.Collections.Generic;
using System.Xml;

public class PerkConditionStyle : PerkConditionMatchMinMax
{
	public PerkConditionStyle()
	{
		set_Type(PerkConditionType.CONDITION_STYLE);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		int num = ParseStyleIndex(node.Attributes["Min"].GetStringOrDefault(string.Empty));
		int num2 = ParseStyleIndex(node.Attributes["Max"].GetStringOrDefault(string.Empty));
		minMax.EvaluateFunctions();
		if (num != -1)
		{
			minMax.SetMinValue(num);
			minMax.SetMinUnbounded(false);
		}
		if (num2 != -1)
		{
			minMax.SetMaxValue(num2);
			minMax.SetMaxUnbounded(false);
		}
	}

	private int ParseStyleIndex(string name)
	{
		switch (name)
		{
		case "Turtle":
			return 0;
		case "Hard":
			return 1;
		case "Brutal":
			return 2;
		case "Aggressive":
			return 3;
		case "Crazy":
			return 4;
		case "Fantastic":
			return 5;
		default:
			return -1;
		}
	}

	public override bool IsEqual(Model GIAMLEDNFJD, List<string> NIKHAICFGNM)
	{
		Model fGCODGKLHED = ResolveTargetModel(GIAMLEDNFJD);
		if (fGCODGKLHED == null)
		{
			return false;
		}
		int pACHBHGEIGN = fGCODGKLHED.StyleRank;
		if (!minMax.GetMinUnbounded() && (int)minMax.GetMinValue() > pACHBHGEIGN)
		{
			return false;
		}
		if (!minMax.GetMaxUnbounded() && (int)minMax.GetMaxValue() < pACHBHGEIGN)
		{
			return false;
		}
		return true;
	}
}
