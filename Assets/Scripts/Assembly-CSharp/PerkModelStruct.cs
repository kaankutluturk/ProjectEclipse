using System.Collections.Generic;
using System.Diagnostics;

public class PerkModelStruct
{
    // best guess for name
    internal List<InfoPerk> ActivePerkEffects => GetInfoPerks();
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Model ownerModel;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<InfoPerk> infoPerks;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkData> perkDataList;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> generalTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> comboTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> everyFrameTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> hitPreCritTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> hitPostCritTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> postHitTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> magicChargedTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> roundStageStartTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> animationStartTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> animationEndTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> styleTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> modExpiresTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> areaEnterTriggers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkTrigger> areaExitTriggers;

	private List<PerkTrigger> FMIGRATIONINTERVALEND;

	public Model OwnerModel
	{
		get
		{
			return get_Model();
		}
		set
		{
			set_Model(value);
		}
	}

	public List<InfoPerk> InfoPerks
	{
		get
		{
			return GetInfoPerks();
		}
		protected set
		{
			SetInfoPerks(value);
		}
	}

	public List<PerkData> PerkDataList
	{
		get
		{
			return GetPerkDataList();
		}
		protected set
		{
			SetPerkDataList(value);
		}
	}

	public List<PerkTrigger> GeneralTriggers
	{
		get
		{
			return GetGeneralTriggers();
		}
		protected set
		{
			SetGeneralTriggers(value);
		}
	}

	public List<PerkTrigger> ComboTriggers
	{
		get
		{
			return GetComboTriggers();
		}
		protected set
		{
			SetComboTriggers(value);
		}
	}

	public List<PerkTrigger> EveryFrameTriggers
	{
		get
		{
			return GetEveryFrameTriggers();
		}
		protected set
		{
			SetEveryFrameTriggers(value);
		}
	}

	public List<PerkTrigger> HitPreCritTriggers
	{
		get
		{
			return GetHitPreCritTriggers();
		}
		protected set
		{
			SetHitPreCritTriggers(value);
		}
	}

	public List<PerkTrigger> HitPostCritTriggers
	{
		get
		{
			return GetHitPostCritTriggers();
		}
		protected set
		{
			SetHitPostCritTriggers(value);
		}
	}

	public List<PerkTrigger> PostHitTriggers
	{
		get
		{
			return GetPostHitTriggers();
		}
		protected set
		{
			SetPostHitTriggers(value);
		}
	}

	public List<PerkTrigger> MagicChargedTriggers
	{
		get
		{
			return GetMagicChargedTriggers();
		}
		protected set
		{
			SetMagicChargedTriggers(value);
		}
	}

	public List<PerkTrigger> RoundStageStartTriggers
	{
		get
		{
			return GetRoundStageStartTriggers();
		}
		protected set
		{
			SetRoundStageStartTriggers(value);
		}
	}

	public List<PerkTrigger> AnimationStartTriggers
	{
		get
		{
			return GetAnimationStartTriggers();
		}
		protected set
		{
			SetAnimationStartTriggers(value);
		}
	}

	public List<PerkTrigger> AnimationEndTriggers
	{
		get
		{
			return GetAnimationEndTriggers();
		}
		protected set
		{
			SetAnimationEndTriggers(value);
		}
	}

	public List<PerkTrigger> StyleTriggers
	{
		get
		{
			return GetStyleTriggers();
		}
		protected set
		{
			SetStyleTriggers(value);
		}
	}

	public List<PerkTrigger> ModExpiresTriggers
	{
		get
		{
			return GetModExpiresTriggers();
		}
		protected set
		{
			SetModExpiresTriggers(value);
		}
	}

	public List<PerkTrigger> AreaEnterTriggers
	{
		get
		{
			return GetAreaEnterTriggers();
		}
		protected set
		{
			SetAreaEnterTriggers(value);
		}
	}

	public List<PerkTrigger> AreaExitTriggers
	{
		get
		{
			return GetAreaExitTriggers();
		}
		protected set
		{
			SetAreaExitTriggers(value);
		}
	}

	public PerkModelStruct()
	{
		set_Model(null);
		SetInfoPerks(new List<InfoPerk>());
		SetPerkDataList(new List<PerkData>());
		SetGeneralTriggers(new List<PerkTrigger>());
		SetComboTriggers(new List<PerkTrigger>());
		SetEveryFrameTriggers(new List<PerkTrigger>());
		SetHitPreCritTriggers(new List<PerkTrigger>());
		SetHitPostCritTriggers(new List<PerkTrigger>());
		SetPostHitTriggers(new List<PerkTrigger>());
		SetMagicChargedTriggers(new List<PerkTrigger>());
		SetRoundStageStartTriggers(new List<PerkTrigger>());
		SetAnimationStartTriggers(new List<PerkTrigger>());
		SetAnimationEndTriggers(new List<PerkTrigger>());
		SetStyleTriggers(new List<PerkTrigger>());
		SetModExpiresTriggers(new List<PerkTrigger>());
		SetAreaEnterTriggers(new List<PerkTrigger>());
		SetAreaExitTriggers(new List<PerkTrigger>());
		FMIGRATIONINTERVALEND = new List<PerkTrigger>();
	}

	public Model get_Model()
	{
		return ownerModel;
	}

	public void set_Model(Model value)
	{
		ownerModel = value;
	}

	public List<InfoPerk> GetInfoPerks()
	{
		return infoPerks;
	}

	protected void SetInfoPerks(List<InfoPerk> value)
	{
		infoPerks = value;
	}

	public List<PerkData> GetPerkDataList()
	{
		return perkDataList;
	}

	protected void SetPerkDataList(List<PerkData> value)
	{
		perkDataList = value;
	}

	public List<PerkTrigger> GetGeneralTriggers()
	{
		return generalTriggers;
	}

	protected void SetGeneralTriggers(List<PerkTrigger> value)
	{
		generalTriggers = value;
	}

	public List<PerkTrigger> GetComboTriggers()
	{
		return comboTriggers;
	}

	protected void SetComboTriggers(List<PerkTrigger> value)
	{
		comboTriggers = value;
	}

	public List<PerkTrigger> GetEveryFrameTriggers()
	{
		return everyFrameTriggers;
	}

	protected void SetEveryFrameTriggers(List<PerkTrigger> value)
	{
		everyFrameTriggers = value;
	}

	public List<PerkTrigger> GetHitPreCritTriggers()
	{
		return hitPreCritTriggers;
	}

	protected void SetHitPreCritTriggers(List<PerkTrigger> value)
	{
		hitPreCritTriggers = value;
	}

	public List<PerkTrigger> GetHitPostCritTriggers()
	{
		return hitPostCritTriggers;
	}

	protected void SetHitPostCritTriggers(List<PerkTrigger> value)
	{
		hitPostCritTriggers = value;
	}

	public List<PerkTrigger> GetPostHitTriggers()
	{
		return postHitTriggers;
	}

	protected void SetPostHitTriggers(List<PerkTrigger> value)
	{
		postHitTriggers = value;
	}

	public List<PerkTrigger> GetMagicChargedTriggers()
	{
		return magicChargedTriggers;
	}

	protected void SetMagicChargedTriggers(List<PerkTrigger> value)
	{
		magicChargedTriggers = value;
	}

	public List<PerkTrigger> GetRoundStageStartTriggers()
	{
		return roundStageStartTriggers;
	}

	protected void SetRoundStageStartTriggers(List<PerkTrigger> value)
	{
		roundStageStartTriggers = value;
	}

	public List<PerkTrigger> GetAnimationStartTriggers()
	{
		return animationStartTriggers;
	}

	protected void SetAnimationStartTriggers(List<PerkTrigger> value)
	{
		animationStartTriggers = value;
	}

	public List<PerkTrigger> GetAnimationEndTriggers()
	{
		return animationEndTriggers;
	}

	protected void SetAnimationEndTriggers(List<PerkTrigger> value)
	{
		animationEndTriggers = value;
	}

	public List<PerkTrigger> GetStyleTriggers()
	{
		return styleTriggers;
	}

	protected void SetStyleTriggers(List<PerkTrigger> value)
	{
		styleTriggers = value;
	}

	public List<PerkTrigger> GetModExpiresTriggers()
	{
		return modExpiresTriggers;
	}

	protected void SetModExpiresTriggers(List<PerkTrigger> value)
	{
		modExpiresTriggers = value;
	}

	public List<PerkTrigger> GetAreaEnterTriggers()
	{
		return areaEnterTriggers;
	}

	protected void SetAreaEnterTriggers(List<PerkTrigger> value)
	{
		areaEnterTriggers = value;
	}

	public List<PerkTrigger> GetAreaExitTriggers()
	{
		return areaExitTriggers;
	}

	public List<PerkTrigger> GetIntervalEndTriggers()
	{
		return FMIGRATIONINTERVALEND;
	}

	protected void SetAreaExitTriggers(List<PerkTrigger> value)
	{
		areaExitTriggers = value;
	}

	public void RemovePerk(PerkInfoItem perk)
	{
		foreach (PerkData item in GetPerkDataList())
		{
			if (item.PerkInfo == perk)
			{
				GetPerkDataList().Remove(item);
				break;
			}
		}
		foreach (PerkTrigger item2 in perk.GetTriggers())
		{
			RemoveTriggerFromEvents(item2);
		}
	}

	public PerkData FindPerkData(PerkInfoItem perk)
	{
		foreach (PerkData item in GetPerkDataList())
		{
			if (item.PerkInfo == perk)
			{
				return item;
			}
		}
		return null;
	}

	public List<PerkTrigger> GetTriggersForEvent(PerkEvent.PerkEventType eventType)
	{
		switch (eventType)
		{
		case PerkEvent.PerkEventType.EVENT_COMBO:
			return GetComboTriggers();
		case PerkEvent.PerkEventType.EVENT_EVERY_FRAME:
			return GetEveryFrameTriggers();
		case PerkEvent.PerkEventType.EVENT_HIT_PRECRIT:
			return GetHitPreCritTriggers();
		case PerkEvent.PerkEventType.EVENT_HIT_POSTCRIT:
			return GetHitPostCritTriggers();
		case PerkEvent.PerkEventType.EVENT_POST_HIT:
			return GetPostHitTriggers();
		case PerkEvent.PerkEventType.EVENT_MAGIC_CHARGED:
			return GetMagicChargedTriggers();
		case PerkEvent.PerkEventType.EVENT_ROUND_STAGE_START:
			return GetRoundStageStartTriggers();
		case PerkEvent.PerkEventType.EVENT_STYLE:
			return GetStyleTriggers();
		case PerkEvent.PerkEventType.EVENT_ANIMATION_START:
			return GetAnimationStartTriggers();
		case PerkEvent.PerkEventType.EVENT_ANIMATION_END:
			return GetAnimationEndTriggers();
		case PerkEvent.PerkEventType.EVENT_MOD_EXPIRES:
			return GetModExpiresTriggers();
		case PerkEvent.PerkEventType.EVENT_AREA_ENTER:
			return GetAreaEnterTriggers();
		case PerkEvent.PerkEventType.EVENT_AREA_EXIT:
			return GetAreaExitTriggers();
		case PerkEvent.PerkEventType.EVENT_INTERVAL_END:
			return GetIntervalEndTriggers();
		default:
			return null;
		}
	}

	public void RemoveTriggerFromEvents(PerkTrigger trigger)
	{
		foreach (PerkEvent item in trigger.GetEvents())
		{
			List<PerkTrigger> list = GetTriggersForEvent(item.get_Type());
			list.Remove(trigger);
		}
	}

	public void SetPerkEnabled(PerkInfoItem perk, bool value)
	{
		foreach (PerkData item in GetPerkDataList())
		{
			if (item.PerkInfo == perk)
			{
				item.Enabled = value;
				break;
			}
		}
	}
}
