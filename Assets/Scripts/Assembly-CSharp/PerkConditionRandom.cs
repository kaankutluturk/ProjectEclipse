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
		string expression = node.Attributes["Chance"].GetStringOrDefault(string.Empty);
		functionExtension.Parse(expression);
		functionExtension.set_Target(this);
	}

	public override bool IsEqual(Model model, List<string> args)
	{
		base.IsEqual(model, args);
		FunctionResult functionResult = functionExtension.Calculate();
		float num = functionResult.ToFloat();
		return NekkiMath.randomChance(num * 100f);
	}
}
