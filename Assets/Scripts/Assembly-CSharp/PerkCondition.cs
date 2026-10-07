using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public abstract class PerkCondition : PerkObject
{
	public enum PerkConditionType
	{
		CONDITION_NONE = 0,
		CONDITION_RANDOM = 1,
		CONDITION_STYLE = 2,
		CONDITION_COMBO = 3,
		CONDITION_ROUND_STAGE = 4,
		CONDITION_CURRENT_ANIMATION = 5,
		CONDITION_CURRENT_INTERVAL = 6,
		CONDITION_HEALTH = 7,
		CONDITION_ITEM = 8,
		CONDITION_ROUND = 9,
		CONDITION_BULLETS = 10,
		CONDITION_MAGIC_CHARGE = 11,
		CONDITION_MOD_EXISTS = 12,
		CONDITION_PAIN = 13,
		CONDITION_OPERATOR = 14,
		CONDITION_IN_THE_AREA = 15,
		CONDITION_PERK_START = 16,
		CONDITION_PERK_COMPARISON = 17
	}

	protected object Info;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private PerkConditionType _type;

	public PerkCondition()
	{
		set_Type(PerkConditionType.CONDITION_NONE);
	}

	public PerkConditionType get_Type()
	{
		return _type;
	}

	protected void set_Type(PerkConditionType value)
	{
		_type = value;
	}

	public static List<PerkCondition> Create(XmlNode node, PerkInfoItem perk)
	{
		List<PerkCondition> list = new List<PerkCondition>();
		if (node == null)
		{
			return list;
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			PerkCondition condition = null;
			string name = childNode.Name;
			if (FunctionExtension.ParseCompareType(name) != FunctionExtension.CompareType.COMPARE_NONE)
			{
				condition = new PerkConditionComparison();
			}
			else
			{
				switch (name)
				{
				case "Random":
					condition = new PerkConditionRandom();
					break;
				case "Style":
					condition = new PerkConditionStyle();
					break;
				case "Combo":
					condition = new PerkConditionCombo();
					break;
				case "RoundStage":
				case "RoundStageStart":
					condition = new PerkConditionRoundStage();
					break;
				case "CurrentAnimation":
					condition = new PerkConditionCurrentAnimation();
					break;
				case "CurrentInterval":
					condition = new PerkConditionCurrentInterval();
					break;
				case "Health":
					condition = new PerkConditionHealth();
					break;
				case "Item":
					condition = new PerkConditionItem();
					break;
				case "Round":
					condition = new PerkConditionRound();
					break;
				case "Bullets":
					condition = new PerkConditionBullets();
					break;
				case "MagicCharge":
					condition = new PerkConditionMagicCharge();
					break;
				case "ModExists":
					condition = new PerkConditionModExists();
					break;
				case "Pain":
					condition = new PerkConditionPain();
					break;
				case "Operator":
					condition = new PerkConditionOperator();
					break;
				case "InTheArea":
					condition = new PerkConditionInTheArea();
					break;
				case "PerkStart":
					condition = new PerkConditionPerkStart(perk.Name, false);
					break;
				}
			}
			if (condition != null)
			{
				condition.SetPerk(perk);
				condition.Parse(childNode);
				list.Add(condition);
			}
		}
		return list;
	}

	public abstract bool IsEqual(Model model, List<string> args);

	protected Model ResolveTargetModel(Model model)
	{
		if (TargetPlayer == PlayerType.PLAYER_ME)
		{
			return model;
		}
		if (TargetPlayer == PlayerType.PLAYER_ENEMY)
		{
			return model.GetCombatTarget();
		}
		return null;
	}
}
