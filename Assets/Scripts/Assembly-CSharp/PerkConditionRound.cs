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
		string expression = node.Attributes["Number"].GetStringOrDefault(string.Empty);
		functionExtension.Parse(expression);
		functionExtension.set_Target(this);
	}

	public override bool IsEqual(Model model, List<string> args)
	{
		Model targetModel = ResolveTargetModel(model);
		if (model == null)
		{
			return false;
		}
		FunctionResult functionResult = functionExtension.Calculate();
		int num = functionResult.Value.ToInt();
		int num2 = targetModel.GetRound();
		if (num2 != num)
		{
			return false;
		}
		return true;
	}
}
