using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class Rule : global::EventDispatcher<object>
{
	public enum RuleType
	{
		RuleItem = 0,
		RuleEquipItem = 1,
		RuleRandomAquiredItem = 2,
		RuleNoButton = 3,
		RuleNoAnimation = 4,
		RuleRingout = 5,
		RuleDarkness = 6,
		RuleHotGround = 7,
		RuleLoseFall = 8,
		RuleRegeneration = 9,
		RuleAttributes = 10,
		RuleDamageFactor = 11,
		RuleRemoveInterval = 12,
		RuleCrazy = 13,
		RuleLifeSteal = 14,
		RuleNoHealthBar = 15,
		RuleCombo = 16,
		RuleTimeoutWin = 17,
		RulePoints = 18,
		RuleRechargeMagicEachRound = 19,
		RuleNoBulletsReplenishment = 20,
		RuleRandom = 21,
		RuleComplex = 22,
		RuleDescription = 23,
		RulePerk = 24,
		RuleNoPerks = 25,
		RuleWinStyle = 26,
		RuleWinCombo = 27,
		RuleWinShock = 28,
		RuleChangeFight = 29,
		RuleTactic = 30,
		RuleInvertJoystick = 31,
		RuleRandomArea = 32,
		RuleRatingEvaluation = 33,
		RuleInvulnerability = 34,
		RuleCurrencyCost = 35,
		RuleResistance = 36,
		RuleRaidCurrencyCost = 37,
		RuleAvatar = 38,
		RuleName = 39,
		RuleLightInTheDarkness = 42
	}

	public enum RuleModeFilter
	{
		MODE_ECLIPSE = 0,
		MODE_NORMAL = 1,
		MODE_ALL = 2
	}

	public Rule ParentRule;

	protected DeflatedString xmlSource = new DeflatedString();

	protected RuleType _type;

	public RuleModeFilter ModeFilter;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool active;

	protected bool appliesToAllRounds;

	public bool IsRandom;

	protected List<int> unusedIntList = new List<int>();

	protected List<int> _rounds = new List<int>();

	public int MinLevel;

	public int MaxLevel;

	public DeflatedString XmlSource
	{
		get
		{
			return GetXmlSource();
		}
	}

	public bool IsActive
	{
		get
		{
			return GetActive();
		}
		private set
		{
			set_Active(value);
		}
	}

	public Rule(RuleType LFLGCDNKNJI, XmlNode node)
	{
		_type = LFLGCDNKNJI;
		set_Active(true);
		appliesToAllRounds = true;
		ParentRule = null;
		IsRandom = false;
		ModeFilter = RuleModeFilter.MODE_ALL;
		MinLevel = 0;
		MaxLevel = int.MaxValue;
		xmlSource.Set(node);
		ParseRounds(node);
		ParseEclipseMode(node);
	}

	public Rule(Rule HNBFMAKFJAM)
	{
		_type = HNBFMAKFJAM._type;
		set_Active(HNBFMAKFJAM.GetActive());
		appliesToAllRounds = HNBFMAKFJAM.appliesToAllRounds;
		ParentRule = HNBFMAKFJAM.ParentRule;
		IsRandom = HNBFMAKFJAM.IsRandom;
		ModeFilter = HNBFMAKFJAM.ModeFilter;
		MinLevel = HNBFMAKFJAM.MinLevel;
		MaxLevel = HNBFMAKFJAM.MaxLevel;
		_rounds = HNBFMAKFJAM._rounds;
		xmlSource = HNBFMAKFJAM.xmlSource;
	}

	public DeflatedString GetXmlSource()
	{
		return xmlSource;
	}

	public RuleType get_Type()
	{
		return _type;
	}

	public bool GetActive()
	{
		return active;
	}

	private void set_Active(bool value)
	{
		active = value;
	}

	public virtual void SetActive(bool value)
	{
		set_Active(value);
	}

	public virtual bool Compare(object data)
	{
		return true;
	}

	public bool AppliesToAllRounds()
	{
		return appliesToAllRounds;
	}

	public bool AppliesToRound(int round)
	{
		if (!appliesToAllRounds)
		{
			foreach (int item in _rounds)
			{
				if (item == round)
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public bool IsPlayerLevelInRange()
	{
		return IsLevelInRange();
	}

	protected bool IsLevelInRange(int MHNCENBCECJ)
	{
		return MHNCENBCECJ >= MinLevel && MHNCENBCECJ <= MaxLevel;
	}

	protected bool IsLevelInRange()
	{
		int mHNCENBCECJ = ListSF.GetRoster().GetLevel();
		return IsLevelInRange(mHNCENBCECJ);
	}

	protected virtual void Parse(XmlNode node)
	{
		ParseRounds(node);
		ParseEclipseMode(node);
	}

	protected void ParseRounds(XmlNode node)
	{
		XmlAttribute cJBEMNNNHDM = node.Attributes["Round"];
		if (!cJBEMNNNHDM.Empty())
		{
			appliesToAllRounds = false;
			string text = cJBEMNNNHDM.GetStringOrDefault(string.Empty);
			string[] array = text.Split('|');
			string[] array2 = array;
			foreach (string value in array2)
			{
				int item = Convert.ToInt32(value);
				_rounds.Add(item);
			}
		}
	}

	protected void ParseEclipseMode(XmlNode node)
	{
		bool flag = node.Attributes["Eclipse"].Empty();
		bool flag2 = node.Attributes["Eclipse"].ParseBool();
		if (flag)
		{
			ModeFilter = RuleModeFilter.MODE_ALL;
		}
		else if (flag2)
		{
			ModeFilter = RuleModeFilter.MODE_ECLIPSE;
		}
		else
		{
			ModeFilter = RuleModeFilter.MODE_NORMAL;
		}
	}
}
