using System.Collections.Generic;
using System.Xml;

public class ConditionAnimation
{
	public enum ConditionType
	{
		NONE = 0,
		ROUND = 1,
		DISTANCE = 2,
		ANIMATION = 3,
		KEYS = 4,
		WEAPONS = 5,
		PLAYER = 6,
		HEALTH = 7,
		LIST = 8,
		CURRENT_INTERVAL = 9,
		CURRENT_ANIMATION = 10,
		PHYSICS_FRAME = 11,
		ROUND_RESULT = 12,
		ITEM = 13,
		BULLETS = 14,
		PERK = 15,
		BIRTH = 16,
		NAME = 17,
		SCREEN = 18,
		MIRROR = 19,
		MOD_EXISTS = 20,
		EVENT = 21,
		DIRECTION = 22,
		BATTLE_TYPE = 23,
		BOSS_ABILITY_STATE = 24,
        ECLIPSE_CHARACTER = 25
	}

	protected ModelType.ModelTargetType _targetModelType;

	public ConditionType Type;

	public bool IsNot;

	// best guess for name
	public ModelType.ModelTargetType TargetModelType
	{
		get
		{
			return GetTargetModelType();
		}
		set
		{
			SetTargetModelType(value);
		}
	}

	public ConditionAnimation(ConditionType LFLGCDNKNJI)
	{
		Type = LFLGCDNKNJI;
		IsNot = false;
	}

	public ModelType.ModelTargetType GetTargetModelType()
	{
		return _targetModelType;
	}

	public void SetTargetModelType(ModelType.ModelTargetType value)
	{
		_targetModelType = value;
	}

	public virtual void Init()
	{
	}

	public virtual bool IsEqual(ModelConditions conditions)
	{
		GameLog.Error("ERROR: Unknown condition type checked: " + Type);
		return false;
	}

	public virtual bool IsEqual(Model ACENLMONNPA, InfoAnimation DBOLBEOCEME)
	{
		return IsEqual(ACENLMONNPA.GetConditions());
	}

	public virtual void Parse(XmlNode BGPKIKNPIKP)
	{
		IsNot = XmlUtils.ParseBool(BGPKIKNPIKP.Attributes["Not"]);
		_targetModelType = ModelType.ParseTargetType(XmlUtils.ParseString(BGPKIKNPIKP.Attributes["Player"], "Me"));
		Init();
	}

	private static int CollectConditionsOfType(List<ConditionAnimation> BBNKIBKPBLO, ConditionType KLFPAELMPJL, List<ConditionAnimation> GKHEPKGMEFI)
	{
		int count = GKHEPKGMEFI.Count;
		foreach (ConditionAnimation item in BBNKIBKPBLO)
		{
			if (KLFPAELMPJL == item.Type)
			{
				GKHEPKGMEFI.Add(item);
			}
			if (item.Type == ConditionType.LIST)
			{
				ConditionList eLFKOGJJNMN = item as ConditionList;
				if (eLFKOGJJNMN != null)
				{
					List<ConditionAnimation> bBNKIBKPBLO = eLFKOGJJNMN.GetConditions();
					CollectConditionsOfType(bBNKIBKPBLO, KLFPAELMPJL, GKHEPKGMEFI);
				}
			}
		}
		return GKHEPKGMEFI.Count - count;
	}

	public virtual Model ResolveTargetModel(Model BPBMKGHEEBI, ModelType.ModelTargetType LFLGCDNKNJI)
	{
		return BPBMKGHEEBI.GetModelByType(LFLGCDNKNJI);
	}

	public virtual void ApplyTargetModelType(ModelType.ModelTargetType LFLGCDNKNJI)
	{
		_targetModelType = LFLGCDNKNJI;
	}
}
