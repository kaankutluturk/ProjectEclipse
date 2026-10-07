using System.Collections.Generic;
using System.Xml;

public class PerkConditionRoundStage : PerkCondition
{
	private int _roundStage;

	public PerkConditionRoundStage()
	{
		set_Type(PerkConditionType.CONDITION_ROUND_STAGE);
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_roundStage = GetRoundStage(node.Attributes["Name"].GetStringOrDefault(string.Empty));
	}

	public override bool IsEqual(Model model, List<string> args)
	{
		Model targetModel = ResolveTargetModel(model);
		if (model == null)
		{
			return false;
		}
		int currentRoundStage = targetModel.RoundStage;
		if (_roundStage != 0 && _roundStage != currentRoundStage)
		{
			return false;
		}
		return true;
	}
}
