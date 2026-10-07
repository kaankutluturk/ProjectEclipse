public class ProfilePerk : global::EventDispatcher<object>
{
	public enum ProfilePerkEvent
	{
		PerkStateUpdate = 0,
		PerkDesroyed = 1,
		PerkChanged = 2,
		PerkInfoChanged = 3
	}

	public enum ProfilePerkState
	{
		PERK_AVAILABLE = 0,
		PERK_UNAVAILABLE = 1,
		PERK_SELECTED = 2,
		PERK_LOCK = 3
	}

	public enum ProfilePerkType
	{
		TYPE_NONE = 0,
		TYPE_PERK = 1,
		TYPE_UPGRADE = 2,
		TYPE_PERK_SELETED = 3
	}

	protected ProfilePerkState perkState;

	protected int _level;

	protected PerkInfoItem perkInfo;

	public bool IsNew;

	protected ProfilePerkType _type;

	protected string _description;

	public ProfilePerkState PerkState
	{
		get
		{
			return GetState();
		}
		set
		{
			set_State(value);
		}
	}

	public int Level
	{
		get
		{
			return GetLevel();
		}
	}

	public PerkInfoItem PerkInfo
	{
		get
		{
			return GetPerkInfo();
		}
		set
		{
			SetPerkInfo(value);
		}
	}

	public string DescriptionKey
	{
		get
		{
			return GetDescription();
		}
		set
		{
			set_Description(value);
		}
	}

	public ProfilePerk(PerkInfoItem info, int level, ProfilePerkState state = ProfilePerkState.PERK_AVAILABLE, ProfilePerkType perkType = ProfilePerkType.TYPE_NONE)
	{
		perkState = state;
		_level = level;
		perkInfo = info;
		IsNew = false;
		_type = perkType;
		_description = ((perkInfo == null) ? string.Empty : perkInfo.DescriptionKey);
	}

	public ProfilePerkState GetState()
	{
		return perkState;
	}

	public void set_State(ProfilePerkState value)
	{
		perkState = value;
		CallEvent(0, perkState);
	}

	public int GetLevel()
	{
		return _level;
	}

	public PerkInfoItem GetPerkInfo()
	{
		return perkInfo;
	}

	public void SetPerkInfo(PerkInfoItem value)
	{
		perkInfo = value;
		CallEvent(2, 0);
	}

	public ProfilePerkType get_Type()
	{
		return _type;
	}

	public string GetDescription()
	{
		return _description;
	}

	public void set_Description(string value)
	{
		_description = value;
		CallEvent(3, 0);
	}

	private void NotifyDestroyed()
	{
		CallEvent(1, null);
	}

	public bool Islevel(int level)
	{
		return level < _level;
	}

	public int GetUpgradeLevel()
	{
		return (perkInfo != null) ? perkInfo.UpgradeLevel : 0;
	}

	public string GetMoveName()
	{
		return (perkInfo == null) ? string.Empty : perkInfo.MoveName;
	}

	public string GetPerkName()
	{
		return (perkInfo == null) ? string.Empty : perkInfo.Name;
	}

	public string GetImageName()
	{
		return (perkInfo == null) ? string.Empty : perkInfo.ImageName;
	}
}
