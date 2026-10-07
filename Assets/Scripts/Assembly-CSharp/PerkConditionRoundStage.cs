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

	public override bool IsEqual(Model ACENLMONNPA, List<string> NIKHAICFGNM)
	{
		Model fGCODGKLHED = ResolveTargetModel(ACENLMONNPA);
		if (ACENLMONNPA == null)
		{
			return false;
		}
		int jMHJDHLBHLK = fGCODGKLHED.RoundStage;
		if (_roundStage != 0 && _roundStage != jMHJDHLBHLK)
		{
			return false;
		}
		return true;
	}
}
