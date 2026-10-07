using System.Collections.Generic;

public class PerkConditionFunctionExtension : PerkCondition
{
	protected FunctionExtension functionExtension = new FunctionExtension();

	public override bool IsEqual(Model ACENLMONNPA, List<string> NIKHAICFGNM)
	{
		bool result = ResolveTargetModel(ACENLMONNPA) != null;
		functionExtension.SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
		functionExtension.SetVariableCallback(GetPerk().OnFunctionPreCallback);
		return result;
	}
}
