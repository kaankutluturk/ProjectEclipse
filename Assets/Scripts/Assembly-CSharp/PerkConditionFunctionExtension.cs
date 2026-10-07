using System.Collections.Generic;

public class PerkConditionFunctionExtension : PerkCondition
{
	protected FunctionExtension functionExtension = new FunctionExtension();

	public override bool IsEqual(Model model, List<string> args)
	{
		bool result = ResolveTargetModel(model) != null;
		functionExtension.SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
		functionExtension.SetVariableCallback(GetPerk().OnFunctionPreCallback);
		return result;
	}
}
