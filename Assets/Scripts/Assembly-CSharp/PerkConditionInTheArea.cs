using System.Collections.Generic;

public class PerkConditionInTheArea : PerkCondition
{
	public PerkConditionInTheArea()
	{
		set_Type(PerkConditionType.CONDITION_IN_THE_AREA);
	}

	public override bool IsEqual(Model model, List<string> args)
	{
		Model targetModel = ResolveTargetModel(model);
		if (model == null)
		{
			return false;
		}
		return targetModel.IsInsideArea();
	}
}
