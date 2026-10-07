using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class Battle
{
	public const string BasePrefix = "base_";

	public const string ActivePrefix = "active_";

	public const string PressedPrefix = "pressed_";

	public const string LockedPrefix = "locked_";

	public const string LockedActivePrefix = "locked_active_";

	protected RosterBattle _rosterBattle;

	protected string _name = string.Empty;

	protected BattleType _type;

	protected Zone _zone;

	protected Vector2 _pos;

	protected string _alias = string.Empty;

	protected string _title = string.Empty;

	protected string _iconName = string.Empty;

	// Optional atlas family used by newer map/raid battle icons. The original
	// runtime assumed every icon lived in BattleBtnBase/BattleBtnActive.
	protected string _iconAtlas = string.Empty;

	protected string _previewIcon = string.Empty;

	protected string _description;

	protected string _location = string.Empty;

	protected string _music = string.Empty;

	protected string _rewardImage = string.Empty;

	protected string _showResistance = string.Empty;

	protected List<string> _fightsNames = new List<string>();

	protected List<FightList> _fights = new List<FightList>();

	protected DeflatedString _sourceDefinition = new DeflatedString();

	// best guess for name
	public bool IsMapVisible;

	protected bool _fightsParsed;

	public ushort LoadedFightCount;

	protected ushort _rewardDigits;

	protected ushort _prizeBaseDigits;

	protected ushort _fightCount;

	public RosterBattle RosterBattle
	{
		get
		{
			return GetRosterBattle();
		}
		set
		{
			SetRosterBattle(value);
		}
	}

	public Zone ParentZone
	{
		get
		{
			return GetZone();
		}
		set
		{
			SetZone(value);
		}
	}

	public Vector2 Position
	{
		get
		{
			return GetPosition();
		}
	}

	public string Alias
	{
		get
		{
			return GetAlias();
		}
	}

	public string Title
	{
		get
		{
			return GetTitle();
		}
	}

	public string IconName
	{
		get
		{
			return GetIconName();
		}
	}

	public string PreviewIcon
	{
		get
		{
			return GetPreviewIcon();
		}
	}

	public string Description
	{
		get
		{
			return GetDescription();
		}
	}

	public string Location
	{
		get
		{
			return GetLocation();
		}
	}

	public string Music
	{
		get
		{
			return GetMusic();
		}
	}

	public string RewardImage
	{
		get
		{
			return GetRewardImage();
		}
	}

	public string ShowResistance
	{
		get
		{
			return GetShowResistance();
		}
	}

	public List<FightList> LoadedFights
	{
		get
		{
			return GetLoadedFights();
		}
	}

	public ushort RewardDigits
	{
		get
		{
			return GetRewardDigits();
		}
	}

	public ushort PrizeBaseDigits
	{
		get
		{
			return GetPrizeBaseDigits();
		}
	}

	public ushort FightCount
	{
		get
		{
			return GetFightCount();
		}
	}

	public virtual List<FightList> Fights
	{
		get
		{
			return GetFights();
		}
	}

	public Battle(string LFLGCDNKNJI, Vector2 MGMMDGFPBLP, string name, string ADONPNOBBDE, string LHCFHAIDNDP, string EMDJGBHIAIA, ushort CDCJKJNGPOE, ushort MCDAHGPLLDO, string LOKLDPLAPOL, string PEMOECLNECD, string LPJNEDFCBOI, string PINIIFIOECE, string OAPKHNPPGHP, string IHBMPGKIBAN)
	{
		_pos = MGMMDGFPBLP;
		_name = name;
		_alias = LOKLDPLAPOL;
		_title = PEMOECLNECD;
		_iconName = ADONPNOBBDE;
		_previewIcon = LHCFHAIDNDP;
		_description = EMDJGBHIAIA;
		_zone = null;
		IsMapVisible = false;
		_rosterBattle = null;
		_rewardDigits = CDCJKJNGPOE;
		_prizeBaseDigits = MCDAHGPLLDO;
		_fightsParsed = false;
		LoadedFightCount = 0;
		_location = LPJNEDFCBOI;
		_music = PINIIFIOECE;
		_fightCount = 0;
		_rewardImage = OAPKHNPPGHP;
		_showResistance = IHBMPGKIBAN;
		ParseTypeBattle(LFLGCDNKNJI);
	}

	public RosterBattle GetRosterBattle()
	{
		return _rosterBattle;
	}

	public void SetRosterBattle(RosterBattle value)
	{
		_rosterBattle = value;
	}

	public string get_Name()
	{
		return _name;
	}

	public BattleType get_Type()
	{
		return _type;
	}

	public Zone GetZone()
	{
		return _zone;
	}

	public void SetZone(Zone value)
	{
		_zone = value;
	}

	public Vector2 GetPosition()
	{
		return _pos;
	}

	public string GetAlias()
	{
		return _alias;
	}

	public string GetTitle()
	{
		return _title;
	}

	public string GetIconName()
	{
		return _iconName;
	}

	public string GetIconAtlas()
	{
		return _iconAtlas;
	}

	public string GetPreviewIcon()
	{
		return _previewIcon;
	}

	public string GetDescription()
	{
		return _description;
	}

	public string GetLocation()
	{
		return _location;
	}

	public string GetMusic()
	{
		return _music;
	}

	public string GetRewardImage()
	{
		return _rewardImage;
	}

	public string GetShowResistance()
	{
		return _showResistance;
	}

	public List<FightList> GetLoadedFights()
	{
		return _fights;
	}

	public ushort GetRewardDigits()
	{
		return _rewardDigits;
	}

	public ushort GetPrizeBaseDigits()
	{
		return _prizeBaseDigits;
	}

	public ushort GetFightCount()
	{
		return _fightCount;
	}

	public string GetZoneBattleKey()
	{
		return string.Format("{0}|{1}|", _zone.get_Name(), get_Name());
	}

	public FightList FindLoadedFightByName(string name)
	{
		foreach (FightList item in _fights)
		{
			if (item.Name == name)
			{
				return item;
			}
		}
		return null;
	}

	public virtual FightList GetFightByName(string name)
	{
		if (!_fightsNames.Contains(name))
		{
			return null;
		}
		FightList jDIPBIHBGPF = FindLoadedFightByName(name);
		if (jDIPBIHBGPF == null)
		{
			XmlNode xmlNode = _sourceDefinition.GetNode();
			int num = 0;
			foreach (XmlNode item in xmlNode.SelectNodes("Fight"))
			{
				string text = item.Attributes["Name"].GetStringOrDefault(string.Empty);
				if (text == name)
				{
					jDIPBIHBGPF = ParseFightNode(item, num);
					break;
				}
				num++;
			}
		}
		return jDIPBIHBGPF;
	}

	// best guess for name
	public virtual List<FightList> GetFights()
	{
		if (!_fightsParsed)
		{
			LoadAllFights();
		}
		return _fights;
	}

	public virtual FightList GetFirstOpenFight()
	{
		for (int i = 0; i < _fightCount; i++)
		{
			FightList jDIPBIHBGPF = GetFightByIndex(i);
			if (jDIPBIHBGPF.Status == ConditionStatus.StatusOpen)
			{
				return jDIPBIHBGPF;
			}
		}
		return null;
	}

	public virtual FightList GetFightByIndex(int index)
	{
		if (index > _fightCount - 1)
		{
			return null;
		}
		foreach (FightList item in _fights)
		{
			if (item.Index == index)
			{
				return item;
			}
		}
		return ParseFightByIndex(index);
	}

	public string GetBaseIconName()
	{
		return "base_" + _iconName;
	}

	public string GetActiveIconName()
	{
		return "active_" + _iconName;
	}

	public string GetPressedIconName()
	{
		return "pressed_" + _iconName;
	}

	public string GetLockedIconName()
	{
		return "locked_" + _iconName;
	}

	public string GetLockedActiveIconName()
	{
		return "locked_active_" + _iconName;
	}

	public string GetPressedIconNameDuplicate()
	{
		return "pressed_" + _iconName;
	}

	public virtual ConditionStatus GetStatus()
	{
        if (Eclipse.Modding.ModModeRuntime.TryCurrent(this, out var modeFight)) return modeFight != null ? ConditionStatus.StatusOpen : ConditionStatus.StatusComplete;
		uint aIKIOPMGCEG = _fightCount;
		if (!_fightsParsed)
		{
			LoadAllFights();
		}
		if (CountFightsWithStatus(ConditionStatus.StatusComplete) == aIKIOPMGCEG)
		{
			return ConditionStatus.StatusComplete;
		}
		if (CountFightsWithStatus(ConditionStatus.StatusIncomplete) == aIKIOPMGCEG)
		{
			return ConditionStatus.StatusIncomplete;
		}
		return ConditionStatus.StatusOpen;
	}

	public virtual void SetAllFightsStatus(ConditionStatus status)
	{
		foreach (FightList item in _fights)
		{
			item.Status = status;
		}
	}

	public virtual void SetTime(long time)
	{
		foreach (FightList item in _fights)
		{
			item.SetTime(time);
		}
	}

	public virtual void UpdateByTime(long time)
	{
		GameLog.Error("Battle::update ERROR - calling ancestor method. Must call BattleDaily::update or BattlePeriodic::update instead");
	}

	public virtual void UpdateRosterFight(FightList KGKDKENMAOA, bool FFIBGBMOMPD)
	{
		KGKDKENMAOA.SetRosterFight(ListSF.LoadRosterFight(KGKDKENMAOA, FFIBGBMOMPD));
	}

	public virtual void ResetLastFightProgress()
	{
		FightList jDIPBIHBGPF = null;
		if (!_fightsParsed)
		{
			LoadAllFights();
		}
		foreach (FightList item in _fights)
		{
			if (item.Status == ConditionStatus.StatusOpen)
			{
				if (item.Index > 0)
				{
					jDIPBIHBGPF = _fights[item.Index - 1];
				}
				break;
			}
		}
		if (jDIPBIHBGPF == null)
		{
			jDIPBIHBGPF = _fights[_fights.Count - 1];
		}
		RosterFight pIGKOIFBOME = jDIPBIHBGPF.GetRosterFight();
		if (pIGKOIFBOME != null)
		{
			pIGKOIFBOME.SetWinCount(0);
			pIGKOIFBOME.SetEclipseWinCount(0);
		}
	}

	public bool HasCompletedFight()
	{
		foreach (FightList item in _fights)
		{
			if (item.Status == ConditionStatus.StatusComplete)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsLocked()
	{
		if (_rosterBattle == null)
		{
			return false;
		}
		return _rosterBattle.IsLocked();
	}

	public void ClearRosterBattle()
	{
		_rosterBattle = null;
	}

	public bool IsHidden()
	{
		if (_rosterBattle != null)
		{
			return _rosterBattle.IsHidden();
		}
		return false;
	}

	public DeflatedString GetSourceDefinition()
	{
		return _sourceDefinition;
	}

	public void SetSourceDefinition(XmlNode node)
	{
		_iconAtlas = node.Attributes["IconAtlas"].GetStringOrDefault(string.Empty);
		ReadFightNames(node);
		_sourceDefinition.Set(node);
	}

	// Narrow Eclipse modding seam. Stage patches operate on the stored source definition
	// before fights are lazily materialized. This deliberately does not expose the live
	// FightList objects or mutate the base stages.xml document.
	public XmlNode CloneSourceDefinitionForModding()
	{
		XmlNode node = _sourceDefinition.GetNode();
		return node == null ? null : node.CloneNode(true);
	}

	public bool ReplaceSourceDefinitionForModding(XmlNode node, out string error)
	{
		error = string.Empty;
		if (node == null || node.Name != "Battle")
		{
			error = "Battle replacement source must be a Battle XML node.";
			return false;
		}
		if (_fights.Count != 0 || _fightsParsed || LoadedFightCount != 0)
		{
			error = "Battle fights were already materialized; stage patches must be applied before fight parsing.";
			return false;
		}
		string replacementName = node.Attributes?["Name"]?.Value ?? string.Empty;
		if (!string.Equals(replacementName, _name, System.StringComparison.Ordinal))
		{
			error = "Battle replacement cannot change recovered identity '" + _name + "'.";
			return false;
		}
		SetSourceDefinition(node);
		ReadMapPositionForModding(node);
		return true;
	}

	// sf2.battles.patch changes only the map placement; ListSF reads it the same way.
	private void ReadMapPositionForModding(XmlNode node)
	{
		_pos = new Vector2(node.Attributes["X"].ParseInt(), node.Attributes["Y"].ParseInt());
	}

	public bool RestoreSourceDefinitionForModding(XmlNode node, out string error)
	{
		error = string.Empty;
		if (node == null || node.Name != "Battle")
		{
			error = "Battle restore source must be a Battle XML node.";
			return false;
		}
		string replacementName = node.Attributes?["Name"]?.Value ?? string.Empty;
		if (!string.Equals(replacementName, _name, System.StringComparison.Ordinal))
		{
			error = "Battle restore cannot change recovered identity '" + _name + "'.";
			return false;
		}
		// Live mod unloading is intentionally unsupported. Restoring the stored source is
		// sufficient for the next lazy parse/reinitialization without touching a running fight.
		SetSourceDefinition(node);
		ReadMapPositionForModding(node);
		return true;
	}

	public void UnloadFights()
	{
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		QuestParameters hAOHNNFLOGK = ListSF.GetInstance().LotteryQuestParameters;
		int num = 0;
		while (num != _fights.Count)
		{
			FightList jDIPBIHBGPF = _fights[num];
			if (hHKLFIIBIFF.GetFightList() != jDIPBIHBGPF && hAOHNNFLOGK.GetFightList() != jDIPBIHBGPF)
			{
				_fights.RemoveAt(num);
				ListSF.GetInstance().RemoveFight(jDIPBIHBGPF);
			}
			else
			{
				num++;
			}
		}
		_fights.Clear();
		_fightsParsed = false;
		LoadedFightCount = 0;
	}

	public virtual void LoadAllFights()
	{
		for (int i = 0; i < _fightCount; i++)
		{
			bool flag = false;
			foreach (FightList item in _fights)
			{
				if (item.Index == i)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				flag = UseAllredyParsedFight(i);
			}
			if (!flag)
			{
				ParseFightByIndex(i);
			}
		}
	}

	public virtual void AddFight(FightList KGKDKENMAOA, int index)
	{
		KGKDKENMAOA.Battle = this;
		KGKDKENMAOA.Index = index;
		_fights.Add(KGKDKENMAOA);
		ListSF.GetInstance().AddFight(KGKDKENMAOA);
		LoadedFightCount++;
		if (LoadedFightCount >= _fightCount)
		{
			_fightsParsed = true;
		}
		_fights = _fights.OrderBy((FightList fight) => fight.Index).ToList();
	}

	public virtual void OnBattleCreated()
	{
	}

	protected void ParseTypeBattle(string LFLGCDNKNJI)
	{
		_type = ListSF.GetInstance().GetBattleTypeByName(LFLGCDNKNJI);
	}

	protected bool UseAllredyParsedFight(int index)
	{
		List<FightList> list = ListSF.GetFightsForBattle(this);
		foreach (FightList item in list)
		{
			if (item.Index == index)
			{
				AddFight(item, index);
				return true;
			}
		}
		return false;
	}

	protected virtual FightList ParseFightByIndex(int index)
	{
		XmlNode xmlNode = _sourceDefinition.GetNode();
		int num = 0;
		XmlNode hKPPBKPJOEO = null;
		foreach (XmlNode item in xmlNode.SelectNodes("Fight"))
		{
			if (index == num)
			{
				hKPPBKPJOEO = item;
				break;
			}
			num++;
		}
		return ParseFightNode(hKPPBKPJOEO, index);
	}

	protected virtual FightList ParseFightNode(XmlNode node, int index)
	{
		ListSF.GetInstance().IsContentLoaded = false;
		FightList jDIPBIHBGPF = new FightList();
		ListSF.GetInstance().ParseFight(jDIPBIHBGPF, node, _type, _location, _music, this);
		AddFight(jDIPBIHBGPF, index);
		ListSF.GetInstance().IsContentLoaded = true;
		return jDIPBIHBGPF;
	}

	protected virtual uint CountFightsWithStatus(ConditionStatus status)
	{
		uint num = 0u;
		foreach (FightList item in _fights)
		{
			if (item.Status == status)
			{
				num++;
			}
		}
		return num;
	}

	protected void ReadFightNames(XmlNode IHKJDPGFDOE)
	{
		_fightCount = 0;
		_fightsNames.Clear();
		if (IHKJDPGFDOE == null)
		{
			return;
		}
		XmlNodeList xmlNodeList = IHKJDPGFDOE.SelectNodes("Fight");
		_fightCount = (ushort)xmlNodeList.Count;
		foreach (XmlNode item2 in xmlNodeList)
		{
			string item = item2.Attributes["Name"].GetStringOrDefault(string.Empty);
			_fightsNames.Add(item);
		}
	}
}
