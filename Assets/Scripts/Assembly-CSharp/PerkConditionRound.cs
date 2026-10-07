using System.Collections.Generic;
using System.Xml;

public class PerkConditionRound : PerkConditionFunctionExtension
{
	public PerkConditionRound()
	{
		set_Type(PerkConditionType.CONDITION_ROUND);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		string bLLCOEAOJGF = node.Attributes["Number"].GetStringOrDefault(string.Empty);
		functionExtension.Parse(bLLCOEAOJGF);
		functionExtension.set_Target(this);
	}

	public override bool IsEqual(Model ACENLMONNPA, List<string> NIKHAICFGNM)
	{
		Model fGCODGKLHED = ResolveTargetModel(ACENLMONNPA);
		if (ACENLMONNPA == null)
		{
			return false;
		}
		FunctionResult dEIHAOLOPLC = functionExtension.Calculate();
		int num = dEIHAOLOPLC.Value.ToInt();
		int num2 = fGCODGKLHED.GetRound();
		if (num2 != num)
		{
			return false;
		}
		return true;
	}
}
