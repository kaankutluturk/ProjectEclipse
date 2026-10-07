using System.Collections.Generic;
using System.Xml;

public class PerkConditionRandom : PerkConditionFunctionExtension
{
	public PerkConditionRandom()
	{
		set_Type(PerkConditionType.CONDITION_RANDOM);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		string bLLCOEAOJGF = node.Attributes["Chance"].GetStringOrDefault(string.Empty);
		functionExtension.Parse(bLLCOEAOJGF);
		functionExtension.set_Target(this);
	}

	public override bool IsEqual(Model ACENLMONNPA, List<string> NIKHAICFGNM)
	{
		base.IsEqual(ACENLMONNPA, NIKHAICFGNM);
		FunctionResult dEIHAOLOPLC = functionExtension.Calculate();
		float num = dEIHAOLOPLC.ToFloat();
		return NekkiMath.randomChance(num * 100f);
	}
}
