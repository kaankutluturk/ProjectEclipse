using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;
using Nekki.SF2.Core;
using Nekki.SF2.Core.Fights.Controller;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Fight;
using Nekki.SF2.GUI.Map;
using Nekki.SF2.GUI.Shop;
using Eclipse.Underworld.Diagnostics;
using UnityEngine;

public static class GameUtils
{
	public enum TutorialDialogStep
	{
		STEP_DIALOGS_NONE = 0,
		STEP_DIALOGS_WIN_BODYGARD_1 = 1,
		STEP_DIALOGS_LOSS_BODYGARD_1 = 2,
		STEP_DIALOGS_TOURNAMENT = 3,
		STEP_DIALOGS_WIN_TOURNAMENT_1 = 4,
		STEP_DIALOGS_WIN_BODYGARD_2 = 5,
		STEP_DIALOGS_LEVEL_LOSS = 6,
		STEP_DIALOGS_LOSS_BODYGARD_2 = 7,
		STEP_DIALOGS_LOSS_TOURNAMENT_ARMOR = 8,
		STEP_DIALOGS_LOSS_TOURNAMENT_HELMET = 9,
		STEP_DIALOGS_TRAINING_FIGHT = 10,
		STEP_DIALOGS_WELLCOM_TUTORIAL = 11,
		STEP_DIALOGS_FAMILIARTY_GIRL = 12,
		STEP_DIALOGS_FAMILIARTY_PLEASE = 13,
		STEP_DIALOGS_FAMILIARTY_GIRL_END = 14,
		STEP_DIALOGS_START_BODYGARD_1 = 15,
		STEP_DIALOGS_BUY_KNIVES = 16,
		STEP_DIALOGS_NOT_ENOUGH_RUBY = 17,
		STEP_DIALOGS_NOT_ENOUGH_GOLD = 18,
		STEP_DIALOGS_NOT_ENOUGH_LEVEL = 19,
		STEP_DIALOGS_CHALLENGE_REQUIREMENTS_FAIL = 20,
		STEP_DIALOGS_NOT_NETWORK = 21,
		STEP_DIALOGS_TUTORIAL_END = 22
	}

	public enum DayRelation
	{
		NEXT_DAY = 0,
		AFTER_NEXT_DAY = 1,
		THIS_DAY = 2,
		BEFORE_THIS_DAY = 3
	}

	public class ZoomEffect
	{
		public int EffectTime;

		public int ElapsedFrames;

		public float StartScale;

		public float CurrentScale;

		public float TargetScale;
	}

	public class HitEffect
	{
		public string Type;

		public int PauseTime;

		public int EffectTime;

		public float AmplitudeX;

		public float AmplitudeY;

		public float FrequencyX;

		public float FrequencyY;
	}

	public class HitEffects
	{
		public List<HitEffect> Effects = new List<HitEffect>();

		public void Parse(XmlNode node)
		{
			Effects.Clear();
			foreach (XmlNode childNode in node.ChildNodes)
			{
				HitEffect effect = new HitEffect();
				effect.Type = childNode.Attributes["Type"].GetStringOrDefault(string.Empty);
				effect.PauseTime = childNode.Attributes["PauseTime"].ParseInt();
				effect.EffectTime = childNode.Attributes["EffectTime"].ParseInt();
				effect.AmplitudeX = childNode.Attributes["AmplitudeX"].ParseFloat();
				effect.AmplitudeY = childNode.Attributes["AmplitudeY"].ParseFloat();
				effect.FrequencyX = childNode.Attributes["FrequencyX"].ParseFloat();
				effect.FrequencyY = childNode.Attributes["FrequencyY"].ParseFloat();
				Effects.Add(effect);
			}
		}

		public HitEffect GetEffectByType(string type)
		{
			foreach (HitEffect item in Effects)
			{
				if (item.Type == type)
				{
					return item;
				}
			}
			return null;
		}
	}

	public class AlignTargetAttribute
	{
		public string Name;

		public float Value;

		public static float GetValue(string name)
		{
			return GetValue(AlignTargetAttributes, name);
		}

		public static float GetValue(List<AlignTargetAttribute> attributes, string name)
		{
			for (int i = 0; i < attributes.Count; i++)
			{
				if (attributes[i].Name == name)
				{
					return attributes[i].Value;
				}
			}
			return 0f;
		}

		public static int Parse(XmlNode node, List<AlignTargetAttribute> attributes)
		{
			int count = attributes.Count;
			foreach (XmlNode childNode in node.ChildNodes)
			{
				AlignTargetAttribute attribute = new AlignTargetAttribute();
				attribute.Parse(childNode);
				attributes.Add(attribute);
			}
			return attributes.Count - count;
		}

		private void Parse(XmlNode node)
		{
			Name = XmlUtils.ParseString(node.Attributes["Name"]);
			Value = XmlUtils.ParseFloat(node.Attributes["Value"]);
		}
	}

	public class CameraSettigs
	{
		public string CameraNode;

		public string BindingNode;

		public float BindingLength;

		public float MaxWidthDelta;

		public float MaxWidth;

		public void Parse(XmlNode node)
		{
			CameraNode = node["CameraSettings"].Attributes["CameraNode"].GetStringOrDefault("COM");
			BindingNode = node["CameraSettings"].Attributes["BindingNode"].GetStringOrDefault("NPivot");
			BindingLength = node["CameraSettings"].Attributes["BindingLength"].ParseFloat();
			MaxWidth = node["CameraSettings"].Attributes["MaxWidth"].ParseFloat(-1f);
			MaxWidthDelta = node["CameraSettings"].Attributes["MaxWidthDelta"].ParseFloat();
		}
	}

	public class BaseSettigs
	{
		public string Attribute;

		public float Base;

		public void Parse(XmlNode node)
		{
			Attribute = node.Attributes["Attribute"].GetStringOrDefault("COM");
			Base = node.Attributes["Base"].ParseFloat();
		}
	}

	public class SupportLink
	{
		public string Name = string.Empty;

		public string Url = string.Empty;
	}

	public class SupportChoiceStruct
	{
		public List<SupportLink> Links = new List<SupportLink>();

		public void Parse(XmlNode node)
		{
			Links.Clear();
			foreach (XmlNode childNode in node.ChildNodes)
			{
				SupportLink link = new SupportLink();
				link.Name = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
				link.Url = childNode.Attributes["Url"].GetStringOrDefault(string.Empty);
				Links.Add(link);
			}
		}

		public SupportLink FindLink(string name)
		{
			foreach (SupportLink item in Links)
			{
				if (item.Name == name)
				{
					return item;
				}
			}
			return null;
		}

		public string GetUrl(string name, string fallbackName)
		{
			SupportLink link = FindLink(name);
			if (link == null)
			{
				link = FindLink(fallbackName);
			}
			return (link == null) ? string.Empty : link.Url;
		}
	}

	public class SlidersIndexStruct
	{
		public Dictionary<string, int> sliderIndices = new Dictionary<string, int>();

		public void SetIndex(string name, int index)
		{
			sliderIndices[name] = index;
		}

		public int GetIndex(string name)
		{
			int result = -1;
			foreach (KeyValuePair<string, int> item in sliderIndices)
			{
				if (item.Key == name)
				{
					result = item.Value;
					break;
				}
			}
			if (name == "SHOP_RUBY_SLIDER")
			{
				result = 4;
			}
			else if (name == "SHOP_FREE_SLIDER")
			{
				result = 1;
			}
			return result;
		}
	}

	public class Currencies
	{
		private List<GameCurrency> currencies = new List<GameCurrency>();

		public void Parse(XmlNode node)
		{
			currencies.Clear();
			foreach (XmlNode childNode in node.ChildNodes)
			{
				GameCurrency item = new GameCurrency(childNode);
				currencies.Add(item);
			}
		}

		public void AddOrUpdateCurrency(XmlNode node)
		{
			GameCurrency currency = new GameCurrency(node);
			bool flag = true;
			foreach (GameCurrency item in currencies)
			{
				if (item.Name == currency.Name)
				{
					item.CopyFrom(currency);
					flag = false;
				}
			}
			if (flag)
			{
				currencies.Add(currency);
			}
		}

		public void AddCurrency(GameCurrency currency)
		{
			currencies.Add(currency);
		}

		public List<GameCurrency> GetCurrencies()
		{
			return currencies;
		}

		public GameCurrency GetCurrencyByName(string name)
		{
			foreach (GameCurrency item in currencies)
			{
				if (item.Name == name)
				{
					return item;
				}
			}
			return null;
		}
	}

	public class ResistanceList
	{
		private List<GameResistance> resistances = new List<GameResistance>();

		public List<GameResistance> Resistances
		{
			get
			{
				return GetResistances();
			}
		}

		public List<GameResistance> GetResistances()
		{
			return resistances;
		}

		public void Parse(XmlNode node)
		{
			resistances.Clear();
			foreach (XmlNode childNode in node.ChildNodes)
			{
				GameResistance item = new GameResistance(childNode);
				resistances.Add(item);
			}
		}

		public GameResistance GetResistanceByName(string name)
		{
			foreach (GameResistance item in resistances)
			{
				if (item.Name == name)
				{
					return item;
				}
			}
			return null;
		}
	}

	public class AchievementCounters
	{
		public Dictionary<string, Counter> AllCounters = new Dictionary<string, Counter>();

		public Dictionary<string, Counter> NormalModeCounters = new Dictionary<string, Counter>();

		public Dictionary<string, Counter> EclipseModeCounters = new Dictionary<string, Counter>();

		public Dictionary<string, Counter> RaidModeCounters = new Dictionary<string, Counter>();

		public void Parse(XmlNode node)
		{
			AllCounters.Clear();
			NormalModeCounters.Clear();
			EclipseModeCounters.Clear();
			RaidModeCounters.Clear();
			foreach (XmlNode childNode in node.ChildNodes)
			{
				Counter counter = new Counter(childNode);
				AllCounters[counter.Name] = counter;
				switch (counter.Mode)
				{
				case Counter.CounterMode.ECLIPSE_MODE:
					EclipseModeCounters[counter.Name] = counter;
					break;
				case Counter.CounterMode.NORMAL_MODE:
					NormalModeCounters[counter.Name] = counter;
					break;
				case Counter.CounterMode.RAID_MODE:
					RaidModeCounters[counter.Name] = counter;
					break;
				case Counter.CounterMode.NONE_MODE:
					EclipseModeCounters[counter.Name] = counter;
					NormalModeCounters[counter.Name] = counter;
					break;
				}
			}
		}

		public void InitConditions()
		{
			foreach (KeyValuePair<string, Counter> item in AllCounters)
			{
				item.Value.Initialize();
			}
		}

		public Dictionary<string, Counter> GetCountersByMode(Counter.CounterMode mode)
		{
			switch (mode)
			{
			case Counter.CounterMode.ECLIPSE_MODE:
				return EclipseModeCounters;
			case Counter.CounterMode.RAID_MODE:
				return RaidModeCounters;
			default:
				return NormalModeCounters;
			}
		}

		public Dictionary<string, Counter> GetCountersForFight(FightList fight)
		{
			if (fight != null && fight.get_Type() == BattleType.FightRaid)
			{
				return RaidModeCounters;
			}
			Counter.CounterMode mode = ((!ListSF.GetRoster().IsEclipseMode()) ? Counter.CounterMode.NORMAL_MODE : Counter.CounterMode.ECLIPSE_MODE);
			return GetCountersByMode(mode);
		}

		public Dictionary<string, Counter> GetCountersByType(string type, Counter.CounterMode mode)
		{
			Dictionary<string, Counter> dictionary = new Dictionary<string, Counter>();
			Dictionary<string, Counter> dictionary2 = GetCountersByMode(mode);
			foreach (KeyValuePair<string, Counter> item in dictionary2)
			{
				Counter value = item.Value;
				if (value.Type == type)
				{
					dictionary[item.Key] = value;
				}
			}
			return dictionary;
		}

		public Counter FindCounterByAchievement(string achievementName)
		{
			for (int i = 0; i < 3; i++)
			{
				Counter.CounterMode mode = (Counter.CounterMode)i;
				Dictionary<string, Counter> dictionary = GetCountersByMode(mode);
				foreach (KeyValuePair<string, Counter> item in dictionary)
				{
					Counter value = item.Value;
					AchievCounter achievementCounter = AchievementDefinitions.GetCounterByName(value.Name);
					if (achievementCounter == null)
					{
						continue;
					}
					List<Achievement> achievements = achievementCounter.Achievements;
					bool flag = false;
					for (int j = 0; j < achievements.Count; j++)
					{
						Achievement achievement = achievements[j];
						if (achievement.Name == achievementName)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
					return value;
				}
			}
			return null;
		}
	}

	public class AchievCounters
	{
		public List<AchievCounter> Counters = new List<AchievCounter>();

		public void Parse(XmlNode node)
		{
			Counters.Clear();
			foreach (XmlNode childNode in node.ChildNodes)
			{
				AchievCounter item = new AchievCounter(childNode);
				Counters.Add(item);
			}
		}

		public AchievCounter GetCounterByName(string name)
		{
			foreach (AchievCounter item in Counters)
			{
				if (item.Name == name)
				{
					return item;
				}
			}
			return null;
		}

		public Achievement GetAchievementByName(string name)
		{
			foreach (AchievCounter item in Counters)
			{
				foreach (Achievement item2 in item.Achievements)
				{
					if (item2.Name == name)
					{
						return item2;
					}
				}
			}
			return null;
		}

		public List<global::Pair<Achievement, int>> GetUnlockableAchievements(List<string> counterNames)
		{
			List<global::Pair<Achievement, int>> list = new List<global::Pair<Achievement, int>>();
			foreach (string item in counterNames)
			{
				AchievCounter achievementCounter = AchievementDefinitions.GetCounterByName(item);
				RosterAchievCounter rosterCounter = ListSF.GetRoster().GetAchievements().FindCounter(item);
				if (achievementCounter == null || rosterCounter == null)
				{
					continue;
				}
				int num = rosterCounter.GetCounter();
				foreach (Achievement item2 in achievementCounter.Achievements)
				{
					if (!IsAchievementUnlocked(item2.Name))
					{
						list.Add(new global::Pair<Achievement, int>(item2, num));
					}
					if (num < item2.CounterValue)
					{
						break;
					}
				}
			}
			return list;
		}

		public List<Achievement> GetEarnedAchievements()
		{
			List<Achievement> list = new List<Achievement>();
			List<RosterAchievCounter> list2 = ListSF.GetRoster().GetAchievements().GetCounters();
			for (int i = 0; i < list2.Count; i++)
			{
				RosterAchievCounter rosterCounter = list2[i];
				string counterName = rosterCounter.get_Name();
				AchievCounter achievementCounter = AchievementDefinitions.GetCounterByName(counterName);
				if (achievementCounter == null)
				{
					continue;
				}
				List<Achievement> achievements = achievementCounter.Achievements;
				for (int j = 0; j < achievements.Count; j++)
				{
					Achievement achievement = achievements[j];
					if (rosterCounter.GetCounter() >= achievement.CounterValue)
					{
						list.Add(achievement);
					}
				}
			}
			return list;
		}
	}

	public class TutorSettingsStruct
	{
		public string TutorialWeapon = string.Empty;

		public string TutorialBoss = string.Empty;

		public string TutorialTournament = string.Empty;

		public float DefaultTutorialStepTimeout;

		public List<string> StepsNames = new List<string>();

		public void Parse(XmlNode node)
		{
			TutorialWeapon = node["TutorialWeapon"].Attributes["Name"].GetStringOrDefault("WEAPON_KNIVES");
			TutorialBoss = node["TutorialBoss"].Attributes["Name"].GetStringOrDefault("ZONE_1|BOSS_LYNX|1");
			TutorialTournament = node["TutorialTournament"].Attributes["Name"].GetStringOrDefault("ZONE_1|Tournament|1");
			DefaultTutorialStepTimeout = node["TutorialStepTimeout"].Attributes["Value"].ParseFloat();
			foreach (XmlNode childNode in node["StepsNames"].ChildNodes)
			{
				if ("Step" == childNode.Name)
				{
					StepsNames.Add(childNode.Attributes["Name"].GetStringOrDefault());
				}
			}
		}

		public bool IsStepName(string stepName)
		{
			return StepsNames.Contains(stepName);
		}
	}

	public struct DebugLogSettings
	{
		public bool Enabled;

		public bool LogQuests;

		public bool LogQuestActions;

		public bool LogAnimations;

		public bool LogHits;

		public bool LogHitDamage;

		public bool LogHitStyle;

		public bool LogTactics;

		public bool LogPerks;

		private void Parse(XmlNode node)
		{
			Enabled = false;
			LogQuests = false;
			LogQuestActions = false;
			LogAnimations = false;
			LogHits = false;
			LogHitDamage = false;
			LogHitStyle = false;
			LogTactics = false;
			LogPerks = false;
			Enabled = node.FirstAttribute().ParseBool();
			if (Enabled)
			{
				LogQuests = node["Quests"].FirstAttribute().ParseBool();
				if (LogQuests)
				{
					LogQuestActions = node["Quests"]["Actions"].FirstAttribute().ParseBool();
				}
				LogAnimations = node["Animations"].FirstAttribute().ParseBool();
				LogTactics = node["Tactics"].FirstAttribute().ParseBool();
				LogPerks = node["Perks"].FirstAttribute().ParseBool();
				LogHits = node["Hits"].FirstAttribute().ParseBool();
				if (LogHits)
				{
					LogHitDamage = node["Hits"]["Damage"].FirstAttribute().ParseBool();
					LogHitStyle = node["Hits"]["Style"].FirstAttribute().ParseBool();
				}
			}
		}
	}

	public static WarriorAttributes WarriorAttributeList = new WarriorAttributes();

	public static LevelAttributeGain StartingAttributes = new LevelAttributeGain();

	public static LevelAttributeGain LevelAttributeGains = new LevelAttributeGain();

	public static List<AlignTargetAttribute> AlignTargetAttributes = new List<AlignTargetAttribute>();

	public static ModifiedAlignFormula ModifiedAlign = new ModifiedAlignFormula();

	public static MagicSettings MagicConfig = new MagicSettings();

	public static RandomTactic RandomTactics = new RandomTactic();

	public static Shock ShockSettings = new Shock();

	public static CritialDefault CriticalHitDefaults = new CritialDefault();

	public static PerkItems PerkItemList = new PerkItems();

	public static OutdateLevels OutdateLevelTable = new OutdateLevels();

	public static StyleLevels StyleLevelTable = new StyleLevels();

	public static RewardsPrize RewardsPrizeSettings = new RewardsPrize();

	private static AspectConstants aspectConstants = new AspectConstants();

	private static List<Aspect> aspects = new List<Aspect>();

	private static AspectDoublingRange aspectDoublingRange = new AspectDoublingRange();

	public static float DamageDoublingRange = 0f;

	public static bool AlwaysMagicMode = false;

	public static LevelThresholds LevelThresholdTable = new LevelThresholds();

	public static float MenuBackgroundHeight = 0f;

	public static float MenuBackgroundPadding = 0f;

	private static float leftWall = 0f;

	private static float rightWall = 0f;

	private static int slowMode = 1;

	private static int maxPower = 5;

	private static float damageFactorBase = 0f;

	private static string damageFactorAttribute = string.Empty;

	private static float damageFactorMaxValue = 20000f;

	private static int _ComboTime = 0;

	private static int comboMinHits = 0;

	private static int announcementTime = 0;

	private static int hotGroundTime = 0;

	public static Currencies GameCurrencies = new Currencies();

	public static ResistanceList GameResistances = new ResistanceList();

	public static int FrameRate = 60;

	public static int FpsReductionDivisor = 2;

	public static AchievCounters AchievementDefinitions = new AchievCounters();

	public static SlidersIndexStruct SliderIndices = new SlidersIndexStruct();

	public static CurrencyBaseValues CurrencyBaseValueTable = new CurrencyBaseValues();

	public static MoneyBaseValues MoneyBaseValueTable = new MoneyBaseValues();

	public static BarScales BarScaleTable = new BarScales();

	public static AchievementCounters ModeCounters = new AchievementCounters();

	public static ShopOverrideConteyner ShopOverrides = new ShopOverrideConteyner();

	public static int CounterPunches = 0;

	public static int PowerMaxTime = 0;

	private static float lifeBarValue;

	private static bool extraFlag = false;

	public static bool ShowNews = true;

	public static bool HackDetected = false;

	public static SupportChoiceStruct SupportChoices = new SupportChoiceStruct();

	private static HitEffects hitEffects = new HitEffects();

	private static CameraSettigs cameraSettings = new CameraSettigs();

	private static BaseSettigs blockDamageFactor = new BaseSettigs();

	private static BaseSettigs criticalHitDamage = new BaseSettigs();

	private static BaseSettigs regeneration = new BaseSettigs();

	private static BaseSettigs lifesteal = new BaseSettigs();

	private static float resistanceDoublingRange;

	private static float greatMaxHealth = 0f;

	public static bool IsLoginComplete = false;

	public static int SlowModeSpeed = 10;

	public static bool UnusedFlag;

	private static string slowMotionDefense = string.Empty;

	private static string blockDefenseAttribute = string.Empty;

	private static string startingMagicAttribute = string.Empty;

	public static bool IsFightTransitioning = false;

	public static TutorSettingsStruct TutorialSettings = new TutorSettingsStruct();

	public static string PivotNodeName = string.Empty;

	public static float SellPriceFactor = 1f;

	private static string defaultAvatar = string.Empty;

	private static string defaultSkeleton = string.Empty;

	public const int UnusedConstant = 1;

	public const int DefaultPushRetentionTime = 172800;

	public static int PushRetentionTime = 172800;

	public static uint MaximumExperience = 30000000u;

	public static bool DailyDebugMode = false;

	public static long DailyDebugTime = 0L;

	public static bool ReduceFps = false;

	public static bool ParticlesDisabled = false;

	public static bool SequencesDisabled = false;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool fightFlag;

	public static Dictionary<SliderType, string> SliderNames = new Dictionary<SliderType, string>();

	public static bool AutoWinPending = false;

	public static string AutoWinFightKey = string.Empty;

	public static DebugLogSettings LogSettings;

	public static Dictionary<RaidTutorialStepCode, string> RaidTutorialStepNames = new Dictionary<RaidTutorialStepCode, string>();

	private static int EightHoursSeconds = 28800;

	private static bool ShowRatePrompt = false;

	private static List<string> _notShowRateFights = new List<string>();

	public static string DefaultLocation = string.Empty;

	private static List<global::Pair<string, string>> defaultItems = new List<global::Pair<string, string>>();

	private static List<Achievement> pendingAchievements = new List<Achievement>();

	public static AspectConstants AspectConstantSettings
	{
		get
		{
			return GetAspectConstants();
		}
	}

	public static List<Aspect> AspectList
	{
		get
		{
			return GetAspects();
		}
	}

	public static AspectDoublingRange AspectDoublingRangeSettings
	{
		get
		{
			return GetAspectDoublingRange();
		}
	}

	public static float LeftWall
	{
		get
		{
			return GetLeftWall();
		}
		set
		{
			SetLeftWall(value);
		}
	}

	public static float RightWall
	{
		get
		{
			return GetRightWall();
		}
		set
		{
			SetRightWall(value);
		}
	}

	public static int SlowMode
	{
		get
		{
			return GetSlowMode();
		}
		set
		{
			SetSlowMode(value);
		}
	}

	public static int MaxPower
	{
		get
		{
			return GetMaxPower();
		}
		set
		{
			SetMaxPower(value);
		}
	}

	public static float DamageFactorBase
	{
		get
		{
			return GetDamageFactorBase();
		}
		set
		{
			SetDamageFactorBase(value);
		}
	}

	public static string DamageFactorAttribute
	{
		get
		{
			return GetDamageFactorAttribute();
		}
		set
		{
			SetDamageFactorAttribute(value);
		}
	}

	public static float DamageFactorMaxValue
	{
		get
		{
			return GetDamageFactorMaxValue();
		}
		set
		{
			SetDamageFactorMaxValue(value);
		}
	}

	public static int ComboTime
	{
		get
		{
			return GetComboTime();
		}
		set
		{
			SetComboTime(value);
		}
	}

	public static int ComboMinHits
	{
		get
		{
			return GetComboMinHits();
		}
		set
		{
			SetComboMinHits(value);
		}
	}

	public static int AnnouncementTime
	{
		get
		{
			return GetAnnouncementTime();
		}
		set
		{
			SetAnnouncementTime(value);
		}
	}

	public static int HotGroundTime
	{
		get
		{
			return GetHotGroundTime();
		}
		set
		{
			SetHotGroundTime(value);
		}
	}

	public static float LifeBarValue
	{
		get
		{
			return GetLifeBarValue();
		}
		set
		{
			SetLifeBarValue(value);
		}
	}

	public static bool ExtraFlag
	{
		get
		{
			return GetExtraFlag();
		}
		set
		{
			SetExtraFlag(value);
		}
	}

	public static HitEffects HitEffectSettings
	{
		get
		{
			return GetHitEffects();
		}
	}

	public static CameraSettigs CameraConfig
	{
		get
		{
			return GetCameraSettings();
		}
	}

	public static BaseSettigs BlockDamageFactor
	{
		get
		{
			return GetBlockDamageFactor();
		}
	}

	public static BaseSettigs CriticalHitDamage
	{
		get
		{
			return GetCriticalHitDamage();
		}
	}

	public static BaseSettigs RegenerationSettings
	{
		get
		{
			return GetRegeneration();
		}
	}

	public static BaseSettigs LifestealSettings
	{
		get
		{
			return GetLifesteal();
		}
	}

	public static float ResistanceDoublingRange
	{
		get
		{
			return GetResistanceDoublingRange();
		}
		set
		{
			SetResistanceDoublingRange(value);
		}
	}

	public static float GreatMaxHealth
	{
		get
		{
			return GetGreatMaxHealth();
		}
		set
		{
			SetGreatMaxHealth(value);
		}
	}

	public static string SlowMotionDefense
	{
		get
		{
			return GetSlowMotionDefense();
		}
		set
		{
			SetSlowMotionDefense(value);
		}
	}

	public static string BlockDefenseAttribute
	{
		get
		{
			return GetBlockDefenseAttribute();
		}
		set
		{
			SetBlockDefenseAttribute(value);
		}
	}

	public static string StartingMagicAttribute
	{
		get
		{
			return GetStartingMagicAttribute();
		}
		set
		{
			SetStartingMagicAttribute(value);
		}
	}

	public static ModelParameters DefaultModelParameters
	{
		get
		{
			return GetPlayerModelParameters();
		}
	}

	public static string DefaultAvatar
	{
		get
		{
			return GetDefaultAvatar();
		}
		set
		{
			SetDefaultAvatar(value);
		}
	}

	public static string DefaultSkeletonName
	{
		get
		{
			return GetDefaultSkeleton();
		}
		set
		{
			SetDefaultSkeleton(value);
		}
	}

	public static bool FightFlag
	{
		get
		{
			return GetFightFlag();
		}
		set
		{
			SetFightFlag(value);
		}
	}

	public static AspectConstants GetAspectConstants()
	{
		return aspectConstants;
	}

	public static List<Aspect> GetAspects()
	{
		return aspects;
	}

	public static AspectDoublingRange GetAspectDoublingRange()
	{
		return aspectDoublingRange;
	}

	public static float GetLeftWall()
	{
		return leftWall;
	}

	public static void SetLeftWall(float value)
	{
		leftWall = value;
	}

	public static float GetRightWall()
	{
		return rightWall;
	}

	public static void SetRightWall(float value)
	{
		rightWall = value;
	}

	public static int GetSlowMode()
	{
		return slowMode;
	}

	public static void SetSlowMode(int value)
	{
		slowMode = value;
	}

	public static int GetMaxPower()
	{
		return maxPower;
	}

	public static void SetMaxPower(int value)
	{
		maxPower = value;
	}

	public static float GetDamageFactorBase()
	{
		return damageFactorBase;
	}

	public static void SetDamageFactorBase(float value)
	{
		damageFactorBase = value;
	}

	public static string GetDamageFactorAttribute()
	{
		return damageFactorAttribute;
	}

	public static void SetDamageFactorAttribute(string value)
	{
		damageFactorAttribute = value;
	}

	public static float GetDamageFactorMaxValue()
	{
		return damageFactorMaxValue;
	}

	public static void SetDamageFactorMaxValue(float value)
	{
		damageFactorMaxValue = value;
	}

	public static int GetComboTime()
	{
		return _ComboTime;
	}

	public static void SetComboTime(int value)
	{
		_ComboTime = value;
	}

	public static int GetComboMinHits()
	{
		return comboMinHits;
	}

	public static void SetComboMinHits(int value)
	{
		comboMinHits = value;
	}

	public static int GetAnnouncementTime()
	{
		return announcementTime;
	}

	public static void SetAnnouncementTime(int value)
	{
		announcementTime = value;
	}

	public static int GetHotGroundTime()
	{
		return hotGroundTime;
	}

	public static void SetHotGroundTime(int value)
	{
		hotGroundTime = value;
	}

	public static Achievement FindReachedAchievement(Counter counter, int value)
	{
		Achievement reachedAchievement = null;
		AchievCounter achievementCounter = AchievementDefinitions.GetCounterByName(counter.Name);
		if (achievementCounter == null)
		{
			return null;
		}
		RosterAchievCounter rosterCounter = ListSF.GetRoster().GetAchievements().FindCounter(counter.Name);
		int num = ((rosterCounter != null) ? rosterCounter.GetCounter() : 0);
		List<Achievement> achievements = achievementCounter.Achievements;
		int num2 = num + value;
		for (int i = 0; i < achievements.Count; i++)
		{
			Achievement achievement = achievements[i];
			if (achievement.CounterValue >= num && achievement.CounterValue <= num2 && !achievement.IsUnlocked)
			{
				reachedAchievement = achievement;
				if (!counter.IsFightEnd)
				{
					reachedAchievement.IsUnlocked = true;
				}
				else
				{
					pendingAchievements.Add(reachedAchievement);
				}
			}
			else if (achievement.CounterValue >= num && achievement.CounterValue >= num2)
			{
				break;
			}
		}
		return reachedAchievement;
	}

	public static Achievement GiveAchievement(string name)
	{
		Achievement achievement = AchievementDefinitions.GetAchievementByName(name);
		if (achievement == null)
		{
			return null;
		}
		if (!achievement.IsUnlocked)
		{
			achievement.IsUnlocked = true;
		}
		RosterAchievement rosterAchievement = ListSF.GetRoster().GetAchievements().FindAchievement(name);
		if (rosterAchievement == null)
		{
			Counter counter = ModeCounters.FindCounterByAchievement(name);
			if (counter != null && counter.CompleteValue <= 0)
			{
				counter.CompleteValue = 1;
			}
		}
		ListSF.GetRoster().GetAchievements().ApplyPendingCounters();
		return achievement;
	}

	public static void UnlockAchievements(List<global::Pair<Achievement, int>> achievementProgress)
	{
		Roster roster = ListSF.GetRoster();
		List<SocialAchievement> list = new List<SocialAchievement>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < achievementProgress.Count; i++)
		{
			Achievement achievement = achievementProgress[i].First;
			int progress = achievementProgress[i].Second;
			SocialAchievement socialAchievement = new SocialAchievement(achievement.PlatformId, progress, achievement.CounterValue);
			if (progress >= achievement.CounterValue)
			{
				roster.GetAchievements().UnlockAchievement(achievement, false);
				socialAchievement.value = achievement.CounterValue;
			}
			list.Add(socialAchievement);
			list2.Add(achievement.Name);
		}
		if (InternetController.IsPostAchievementsEnabled())
		{
			if (SystemProperties.CheckConnection() && GameCenterController.GetIsAuthenticated())
			{
				GameCenterController.SyncAchievements(list);
			}
			else
			{
				ListSF.GetRoster().GetAchievements().AddRepostAchievements(list2);
			}
		}
	}

	public static void RecountAchievements()
	{
		List<RosterAchievement> list = ListSF.GetRoster().GetAchievements().GetAchievements();
		List<SocialAchievement> list2 = new List<SocialAchievement>();
		List<string> list3 = new List<string>();
		for (int i = 0; i < list.Count; i++)
		{
			Achievement achievement = AchievementDefinitions.GetAchievementByName(list[i].get_Name());
			// Local mod achievements have no platform achievement identity.
			if (achievement != null && !string.IsNullOrEmpty(achievement.PlatformId))
			{
				list2.Add(new SocialAchievement(achievement.PlatformId, achievement.CounterValue, achievement.CounterValue));
				list3.Add(achievement.Name);
			}
		}
		if (InternetController.IsPostAchievementsEnabled())
		{
			if (SystemProperties.CheckConnection() && GameCenterController.GetIsAuthenticated())
			{
				GameCenterController.SyncAchievements(list2);
			}
			else
			{
				ListSF.GetRoster().GetAchievements().AddRepostAchievements(list3);
			}
		}
	}

	private static bool IsAchievementUnlocked(string name)
	{
		List<RosterAchievement> list = ListSF.GetRoster().GetAchievements().GetAchievements();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].get_Name() == name)
			{
				return true;
			}
		}
		return false;
	}

	public static void SendRepostedAchievements()
	{
		if (!InternetController.IsPostAchievementsEnabled() || !SystemProperties.CheckConnection() || !GameCenterController.GetIsAuthenticated())
		{
			return;
		}
		List<RepostAchievement> list = ListSF.GetRoster().GetAchievements().GetRepostAchievements();
		List<SocialAchievement> list2 = new List<SocialAchievement>();
		List<AchievCounter> achievementCounters = AchievementDefinitions.Counters;
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < achievementCounters.Count; j++)
			{
				RosterAchievCounter rosterCounter = ListSF.GetRoster().GetAchievements().FindCounter(achievementCounters[j].Name);
				if (rosterCounter == null)
				{
					continue;
				}
				List<Achievement> achievements = achievementCounters[j].Achievements;
				for (int k = 0; k < achievements.Count; k++)
				{
					if (achievements[k].Name == list[i].get_Name() && !string.IsNullOrEmpty(achievements[k].PlatformId))
					{
						if (achievements[k].CounterValue == 0)
						{
							GameLog.Write("Error - counter == 0");
						}
						SocialAchievement item = new SocialAchievement(achievements[k].PlatformId, rosterCounter.GetCounter(), achievements[k].CounterValue);
						list2.Add(item);
					}
				}
			}
		}
		GameCenterController.SyncAchievements(list2);
		ListSF.GetRoster().GetAchievements().RemoveRepostAchievements(list);
	}

	public static bool AvatarExists(string avatarName)
	{
		string path = string.Format("{0}{1}", SF2Paths.GetUsersUiPath(), avatarName);
		UnityEngine.Object obj = Resources.Load(path);
		return obj != null;
	}

	public static float GetLifeBarValue()
	{
		return lifeBarValue;
	}

	public static void SetLifeBarValue(float value)
	{
		lifeBarValue = value;
	}

	public static bool GetExtraFlag()
	{
		return extraFlag;
	}

	public static void SetExtraFlag(bool value)
	{
		extraFlag = value;
	}

	public static HitEffects GetHitEffects()
	{
		return hitEffects;
	}

	public static CameraSettigs GetCameraSettings()
	{
		return cameraSettings;
	}

	public static BaseSettigs GetBlockDamageFactor()
	{
		return blockDamageFactor;
	}

	public static BaseSettigs GetCriticalHitDamage()
	{
		return criticalHitDamage;
	}

	public static BaseSettigs GetRegeneration()
	{
		return regeneration;
	}

	public static BaseSettigs GetLifesteal()
	{
		return lifesteal;
	}

	public static float GetResistanceDoublingRange()
	{
		return resistanceDoublingRange;
	}

	public static void SetResistanceDoublingRange(float value)
	{
		resistanceDoublingRange = value;
	}

	public static bool IsAlignTargetAttribute(string attributeName)
	{
		AlignTargetAttribute alignAttribute = AlignTargetAttributes.Find((AlignTargetAttribute attribute) => attribute.Name.Equals(attributeName));
		return alignAttribute != null;
	}

	public static float GetGreatMaxHealth()
	{
		return greatMaxHealth;
	}

	public static void SetGreatMaxHealth(float value)
	{
		greatMaxHealth = value;
	}

	public static string GetSlowMotionDefense()
	{
		return slowMotionDefense;
	}

	public static void SetSlowMotionDefense(string value)
	{
		slowMotionDefense = value;
	}

	public static string GetBlockDefenseAttribute()
	{
		return blockDefenseAttribute;
	}

	public static void SetBlockDefenseAttribute(string value)
	{
		blockDefenseAttribute = value;
	}

	public static string GetStartingMagicAttribute()
	{
		return startingMagicAttribute;
	}

	public static void SetStartingMagicAttribute(string value)
	{
		startingMagicAttribute = value;
	}

	public static bool IsProbality(float value)
	{
		return NekkiMath.randomChance(value * 100f);
	}

	public static float GetFightDifficulty(FightList fight)
	{
		return 0f;
	}

	public static float CalculateAttributesAlignment(bool isPlayer, ModelParameters sourceModel, ModelParameters targetModel, List<global::Pair<string, float>> attributeBonuses, string targetAttributeName, ref float bestAttributeDifference, ref float bestFactor, ref float bestShift)
	{
		float num = AlignTargetAttribute.GetValue(targetAttributeName);
		float num2 = 0f - num;
		float num3 = 0f;
		float num4 = 0f;
		int attributeValue = 0;
		targetModel.FinalAttributes.Get(targetAttributeName, ref attributeValue);
		float num5 = attributeValue;
		float num6 = float.MinValue;
		ModelParameters alignmentModel = ((!isPlayer) ? sourceModel : targetModel);
		List<AttributesAlign> list = new List<AttributesAlign>();
		int num7 = AttributesAlign.AppendHighestPriority(alignmentModel.AttributeAlignments, list);
		for (int i = 0; i < attributeBonuses.Count; i++)
		{
			string attributeName = attributeBonuses[i].First;
			attributeValue = 0;
			sourceModel.FinalAttributes.Get(attributeName, ref attributeValue);
			float num8 = attributeValue;
			float num9 = num8 + attributeBonuses[i].Second;
			float num10 = AlignTargetAttribute.GetValue(attributeName);
			float num11 = 0f;
			if (isPlayer)
			{
				num11 = float.MaxValue;
				foreach (AttributesAlign item in list)
				{
					bool flag = ListSF.GetRoster().IsEclipseMode();
					bool flag2 = QuestUtils.GetDifficultyOptions().GetOverrideAllDifficulties();
					if ((flag && item.DifficultyFilter == ModelParameters.DifficultyFilter.DFHard) || (!flag && item.DifficultyFilter == ModelParameters.DifficultyFilter.DFNormal) || item.DifficultyFilter == ModelParameters.DifficultyFilter.DFBoth || flag2)
					{
						float factor = item.Factor;
						float shift = item.Shift;
						float num12 = (num9 - num5) * (1f - factor) + (num10 - num) * factor - shift;
						if (num12 < num11)
						{
							num11 = num12;
							num3 = factor;
							num4 = shift;
						}
					}
				}
			}
			else
			{
				num11 = float.MinValue;
				foreach (AttributesAlign item2 in list)
				{
					bool flag3 = ListSF.GetRoster().IsEclipseMode();
					bool flag4 = QuestUtils.GetDifficultyOptions().GetOverrideAllDifficulties();
					if ((flag3 && item2.DifficultyFilter == ModelParameters.DifficultyFilter.DFHard) || (!flag3 && item2.DifficultyFilter == ModelParameters.DifficultyFilter.DFNormal) || item2.DifficultyFilter == ModelParameters.DifficultyFilter.DFBoth || flag4)
					{
						float blendFactor = item2.Factor;
						float alignmentOffset = item2.Shift;
						float num13 = (num9 - num5) * (1f - blendFactor) + (num10 - num) * blendFactor + alignmentOffset;
						if (num11 < num13)
						{
							num11 = num13;
							num3 = blendFactor;
							num4 = alignmentOffset;
						}
					}
				}
			}
			ModifiedAlignFormula.DamageAttribute damageAttribute = ModifiedAlign.FindDamageAttribute(attributeName);
			if (damageAttribute != null)
			{
				float minAttributeDifference = damageAttribute.MinAttributeDifference;
				string applyTo = damageAttribute.ApplyTo;
				bool flag5 = applyTo == "Player";
				if (num11 >= minAttributeDifference && isPlayer == flag5)
				{
					float doublingRange = DamageDoublingRange;
					float netDamage = damageAttribute.NetDamage;
					float damageMultiplier = damageAttribute.DamageMultiplier;
					num11 = doublingRange * Mathf.Log((1f - (1f - netDamage) * Mathf.Pow(damageMultiplier, (0f - num11) / doublingRange)) / netDamage) / Mathf.Log(2f);
				}
			}
			if (num6 < num11)
			{
				num6 = num11;
				num2 = num10 - num;
			}
		}
		bestAttributeDifference = num2;
		bestFactor = num3;
		bestShift = num4;
		return num6;
	}

	public static float GetAttributesHitMultiplier(bool isPlayer, ModelParameters sourceModel, ModelParameters targetModel, List<global::Pair<string, float>> attributeBonuses, string targetAttributeName)
	{
		float attributeDifference = 0f;
		float factor = 0f;
		float shift = 0f;
		float alignment = CalculateAttributesAlignment(isPlayer, sourceModel, targetModel, attributeBonuses, targetAttributeName, ref attributeDifference, ref factor, ref shift);
		return GetAttributesHitMultiplier(alignment);
	}

	public static float GetAttributesHitMultiplier(float alignment)
	{
		float doublingRange = DamageDoublingRange;
		return Mathf.Pow(2f, alignment / doublingRange);
	}

	public static List<ModelParameters> CreateOpponentParameters(List<ModelParameters> opponentTemplates)
	{
		List<ModelParameters> list = new List<ModelParameters>();
		foreach (ModelParameters item2 in opponentTemplates)
		{
			List<ModelParameters> list2 = ListSF.GetInstance().CreateOpponentParameters(item2, item2.OpponentCount);
			foreach (ModelParameters item3 in list2)
			{
				ModelParameters item = InitializeCombatParameters(item3);
				list.Add(item);
			}
		}
		return list;
	}

	public static ModelParameters GetPlayerModelParameters()
	{
		ModelParameters playerParameters = ListSF.GetPlayerParameters();
		List<ItemInfo> list = playerParameters.GetEquippedItemsByType();
		Roster roster = ListSF.GetRoster();
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			ItemInfo equippedItem = list[i];
			if (equippedItem != null)
			{
				UserItem userItem = roster.GetInventory().FindItem(equippedItem.Name);
				if (userItem != null && userItem.GetHasUpgradedItem())
				{
					playerParameters.SetItemByType(list[i].Type, userItem.GetCurrentUpgradeItem());
				}
			}
		}
		InitializeCombatParameters(playerParameters);
		playerParameters.UserControlled = true;
		playerParameters.AiControlled = false;
		playerParameters.IsPlayer = true;
		return playerParameters;
	}

    internal static ModelParameters InitializeFormParameters(ModelParameters parameters, ModelParameters current)
    {
        if (parameters == null || current == null || ReferenceEquals(parameters, current))
            throw new ArgumentException("Form initialization requires distinct prepared and current parameters.");
        // Raw warrior projection has no fight health pool or move selection. Use
        // the same preparation as ordinary fighters before applying round rules.
        InitializeCombatParameters(parameters);
        // Participant ownership and input eligibility belong to the live slot;
        // a warrior template may otherwise turn a player form into an AI fighter.
        parameters.IsPlayer = current.IsPlayer;
        parameters.UserControlled = current.UserControlled;
        parameters.AiControlled = current.AiControlled;
        return parameters;
    }
    internal static ModelParameters InitializeActorParameters(ModelParameters parameters, bool playerTeam, bool ai, float maxHealth)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        InitializeCombatParameters(parameters);
        parameters.IsPlayer = playerTeam;
        parameters.UserControlled = false;
        parameters.AiControlled = ai;
        parameters.MaxLife = maxHealth;
        parameters.SetCurrentLife(maxHealth);
        return parameters;
    }
    internal static void InitializePlayerCharacterParameters(ModelParameters parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        InitializeCombatParameters(parameters);
        parameters.IsPlayer = true;
        parameters.UserControlled = true;
        parameters.AiControlled = false;
    }
    internal static void InitializeLocalVersusParameters(ModelParameters parameters, bool playerOne)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        InitializeCombatParameters(parameters);
        parameters.IsPlayer = playerOne;
        parameters.UserControlled = true;
        parameters.AiControlled = false;
    }

		// best guess for name
		private static ModelParameters InitializeCombatParameters(ModelParameters parameters)
	{
		if (parameters.Skeleton == null)
		{
			parameters.Skeleton = ListSF.GetItems().GetItemByName(GetDefaultSkeleton());
		}
		parameters.RoundsWon = 0;
		parameters.IsPlayer = false;
		int num = (ObscuredInt)(parameters.GetBaseHealth());
		if (num > 0)
		{
			parameters.MaxLife = num;
		}
		else
		{
			parameters.MaxLife = 1f;
		}
		parameters.MovesInitialized = false;
		parameters.IsUntouched = true;
		parameters.RestoreFullLife();
		parameters.MoveIds = LoadMoves(AnimationData.GetAnimationCount());
		parameters.BuildModelDocuments();
		parameters.CalculateAttributes();
		return parameters;
	}

	public static void LockInput(bool visible = true)
	{
		Module.GetInstance().SetGameInputLock(true, visible);
	}

	public static void UnlockInput()
	{
		Module.GetInstance().SetGameInputLock(false);
	}

	public static void ResetScenes()
	{
		SceneManagerSF.Reset();
	}

	private static List<int> LoadMoves(int count = 50)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < count; i++)
		{
			list.Add(i);
		}
		return list;
	}

	public static FightList GetOpenFight(Battle battle)
	{
		if (Eclipse.Modding.ModModeRuntime.TryCurrent(battle, out var modeFight)) return modeFight;
		if (battle != null)
		{
			List<FightList> list = battle.GetFights();
			foreach (FightList item in list)
			{
				if (item.Status == ConditionStatus.StatusOpen)
				{
					return item;
				}
			}
		}
		return null;
	}

	public static FightList GetLastFight(Battle battle)
	{
		if (battle != null)
		{
			List<FightList> list = battle.GetFights();
			if (list.Count > 0)
			{
				return list[list.Count - 1];
			}
		}
		return null;
	}

		public static Fight CreateFight(object data, PreFight preFight = null, GameController gameController = null)
		{
			if (data is Eclipse.Multiplayer.LocalVersusMatch localMatch)
				return localMatch.CreateFight(preFight, gameController);
		FightList fightList = (FightList)data;
		ModelParameters playerParameters = null;
		playerParameters = Eclipse.Modding.ModRuntime.BuildFightPlayerParameters(fightList) ?? GetPlayerModelParameters().Clone();
		List<ModelParameters> list = new List<ModelParameters>();
		BattleType battleType = fightList.get_Type();
		if (battleType == BattleType.FightPeriodic)
		{
			if (!fightList.GetRosterFight().HasRandomSeeds)
			{
				fightList.GetRosterFight().RandomizeSeeds();
			}
			NekkiMath.SetSeed(fightList.GetRosterFight().GetRandomGroupSeed());
			list = CreateOpponentParameters(fightList.GetOpponents());
			NekkiMath.SetSeed();
		}
		else
		{
			list = CreateOpponentParameters(fightList.GetOpponents());
		}
		if (list.Count == 0)
		{
			// Keep a malformed/newer fight playable and make the migration defect
			// explicit. AdaptStages normally materializes battle-level warrior pools;
			// this is the final safety net for future unsupported stage shapes.
			ModelParameters fallbackEnemy = InitializeCombatParameters(playerParameters.Clone());
			fallbackEnemy.IsPlayer = false;
			fallbackEnemy.Avatar = "man_fist";
			list.Add(fallbackEnemy);
			UnityEngine.Debug.LogError("[Fight] '" + fightList.Name +
				"' contained no usable enemies; using a player-equipment compatibility opponent.");
		}
		UnderworldRaidDiagnostics.LogEnemies(fightList, playerParameters, list);
		Eclipse.Underworld.UnderworldZonePolicy.NoteFightStarted(fightList.Battle);
		return new Fight(fightList, playerParameters, list, preFight, gameController);
	}

	public static void OnFightPrepared(Battle battle)
	{
	}

	public static FightList GetFinalFight(FightList fightList)
	{
		Roster roster = ListSF.GetRoster();
		if (roster.GetDiscipleMode() > 0)
		{
			Battle battle = fightList.Battle;
			List<FightList> list = battle.GetFights();
			return list[list.Count - 1];
		}
		return fightList;
	}

	public static bool ApplyItemAction(ItemInfo item, ItemAction itemAction, int count = 1, Action<object> callback = null)
	{
		UserItem userItem = ListSF.GetUserItem(item.Name);
		return ApplyItemAction(item, userItem, itemAction, count, callback);
	}

	private static bool ApplyItemAction(ItemInfo item, UserItem userItem, ItemAction itemAction, int count = 1, Action<object> callback = null)
	{
		QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
		questParameters.purchaseFailureReason = string.Empty;
		ListSF.CheckItems purchaseCheck = new ListSF.CheckItems();
		if (itemAction == ItemAction.Item_Buy_Gold || itemAction == ItemAction.Item_Buy_Ruby || itemAction == ItemAction.Item_Buy_Real || itemAction == ItemAction.Item_Upgrade_Gold || itemAction == ItemAction.Item_Upgrade_Ruby || itemAction == ItemAction.Item_Delivery_Ruby || itemAction == ItemAction.Item_Recipe || itemAction == ItemAction.Item_Recipe_Delivery_Ruby || itemAction == ItemAction.Item_Consumable)
		{
			purchaseCheck = ListSF.CheckItemPurchase(item, itemAction, count);
			if (purchaseCheck.Value == -1)
			{
				if (itemAction == ItemAction.Item_Recipe)
				{
					NotifyEnchantmentUnsuccessful(item, purchaseCheck.Type);
				}
				else
				{
					NotifyPurchaseUnsuccessful(item, purchaseCheck.Type);
				}
				return false;
			}
		}
		if ((item.DeliveryTime == 0 && (itemAction == ItemAction.Item_Buy_Gold || itemAction == ItemAction.Item_Upgrade_Gold)) || (userItem != null && userItem.GetIsOwned()) || itemAction == ItemAction.Item_Buy_Ruby || itemAction == ItemAction.Item_Equip || itemAction == ItemAction.Item_Upgrade_Ruby || itemAction == ItemAction.Item_Delivery_Ruby)
		{
			ListSF.UnequipOtherItemsOfType(item);
		}
		if (userItem != null && userItem.GetIsEquipped() && itemAction == ItemAction.Item_Unequip)
		{
			ListSF.GetInstance().EquipDefaultItem(item);
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool result = false;
		switch (itemAction)
		{
		case ItemAction.Item_Buy_Gold:
		case ItemAction.Item_Buy_Ruby:
		case ItemAction.Item_Buy_Real:
		case ItemAction.Item_Upgrade_Gold:
		case ItemAction.Item_Upgrade_Ruby:
		case ItemAction.Item_Delivery_Ruby:
		case ItemAction.Item_Free:
		case ItemAction.Item_Consumable:
			result = ListSF.BuyItem(item, itemAction, purchaseCheck.Value, count, callback);
			break;
		case ItemAction.Item_Recipe_Delivery_Ruby:
			flag2 = true;
			flag3 = true;
			result = ListSF.BuyItem(item, itemAction, purchaseCheck.Value, count, callback);
			break;
		case ItemAction.Item_Equip:
			result = ListSF.UseItem(userItem, true);
			break;
		case ItemAction.Item_Unequip:
			result = ListSF.UseItem(userItem, false);
			break;
		case ItemAction.Item_Delivery_End:
			result = ListSF.GiveItemUnequipped(item);
			break;
		case ItemAction.Item_Update:
			result = true;
			break;
		case ItemAction.Item_Recipe:
		{
			flag = true;
			RecipeItemInfo recipeItem = (RecipeItemInfo)item;
			ScheduleEnchantmentNotification("enchantment_notification", recipeItem);
			break;
		}
		case ItemAction.Item_Recipe_Delivery_End:
		{
			flag2 = true;
			flag3 = true;
			RecipeItemInfo recipeItem = (RecipeItemInfo)item;
			CancelEnchantmentNotification("enchantment_notification", recipeItem);
			break;
		}
		}
		UserItem refreshedUserItem = ListSF.GetUserItem(item.Name);
		if (flag3)
		{
		}
		if (refreshedUserItem != null && refreshedUserItem.GetDeliveryTimestamp() > 0)
		{
			ScheduleItemNotification("item_notification", refreshedUserItem);
		}
		else if (refreshedUserItem != null && refreshedUserItem.GetDeliveryTimestamp() == 0)
		{
			CancelItemNotification("item_notification", refreshedUserItem);
		}
		return result;
	}

	public static void NotifyPurchaseUnsuccessful(ItemInfo item, ListSF.CheckItemType checkType)
	{
		QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
		questParameters.purchasedItem = item;
		switch (checkType)
		{
		case ListSF.CheckItemType.CHECK_ITEM_MONEY:
			questParameters.purchaseFailureReason = "Coins";
			break;
		case ListSF.CheckItemType.CHECK_ITEM_BONUS:
			questParameters.purchaseFailureReason = "Ruby";
			break;
		case ListSF.CheckItemType.CHECK_ITEM_NO_NETWORK:
			questParameters.purchaseFailureReason = "Connection";
			break;
		}
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE_UNSUCCESSFUL))
		{
			ListSF.GetInstance().RunQuestActions();
		}
		TrackEvent("Insufficient Currency");
	}

	private static void NotifyEnchantmentUnsuccessful(ItemInfo item, ListSF.CheckItemType checkType)
	{
		QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
		RecipeItemInfo recipeItem = (RecipeItemInfo)item;
		questParameters.enchantment.itemName = recipeItem.GetUserItem().get_Name();
		questParameters.enchantment.recipeName = recipeItem.GetRecipe().Name;
		long num = ListSF.GetServerTime();
		long endTimestamp = num + recipeItem.GetPrice().DeliveryTimeSeconds;
		questParameters.enchantment.endTimestamp = endTimestamp;
		questParameters.enchantment.costType = "Materials";
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_ENCHANTMENT_UNSUCCESSFUL))
		{
			ListSF.GetInstance().RunQuestActions();
		}
	}

	public static bool NotifyTabChanged(SliderType fromTab, SliderType toTab)
	{
		QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
		questParameters.tabFrom = SliderNames[fromTab];
		questParameters.tabTo = SliderNames[toTab];
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_CHANGE_TAB))
		{
			ListSF.GetInstance().RunQuestActions();
			return true;
		}
		return false;
	}

	public static bool NotifyShopOpened(ScreenType screenType)
	{
		if (screenType == ScreenType.ModuleShop &&
			ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_SHOP_BUTTON_PRESS))
		{
			ListSF.GetInstance().RunQuestActions();
			// The migrated quest graph observes this event but does not always issue
			// its own scene transition.  Continue the user's requested navigation.
			return false;
		}
		if (screenType == ScreenType.ModuleShop && ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_SHOP_ENTER))
		{
			ListSF.GetInstance().RunQuestActions();
			return false;
		}
		return false;
	}

	public static void ExitApplication()
	{
		GameLog.Write("exitApplication() called, quiting");
		TrackEvent("App Close");
		ApplicationController.Quit();
	}

	public static uint GetFightExpReward(FightList fightList, bool isWinner)
	{
		FightResult fightResult = new FightResult();
		fightResult.FightType = fightList.get_Type();
		fightResult.FightId = fightList.FightId;
		int num = fightList.RewardIndex;
		if (isWinner)
		{
			num++;
		}
		if (fightList.HasMultipleOpponentsAndRounds())
		{
			num = (isWinner ? 1 : 0);
		}
		RewardStruct reward = fightList.GetRewardAt(num);
		fightResult.CalculateRewards(reward, null, null, fightList);
		return fightResult.Prize.exp;
	}

	public static void HandleSurrender(FightResult fightResult)
	{
		IsFightTransitioning = true;
		IsFightTransitioning = false;
		Module.OpenScreen(ScreenType.ModuleMap);
	}

	public static void EndFight(ComboStatistic playerCombo, FightList fightList, ModelParameters playerParameters, ModelParameters opponentParameters, GameOverTypes gameOverType, ComboStatistic opponentCombo = null, float averageFps = 0f, int raidDamage = 0, float fightTimeElapsed = 0f, int unusedCount = 0)
	{
		bool flag = opponentParameters == null && opponentCombo == null;
		bool flag2 = fightList == null || gameOverType == GameOverTypes.GAME_OVER_SURRENDER;
		Roster roster = ListSF.GetRoster();
		fightList = ((fightList == null) ? ListSF.GetFightById(roster.GetFightIds()) : fightList);
		if (fightList == null)
		{
			GameLog.Error("0 == fightList");
		}
		if (!Eclipse.Modding.ModModeRuntime.CanResolve(fightList)) return;
		var eclipseStoryEncounter = fightList.EclipseStoryEncounter;
		if (!Eclipse.Modding.ModRuntime.StoryEvents.TryBeginEncounterResult(eclipseStoryEncounter)) eclipseStoryEncounter = null;
		var eclipseStoryResult = eclipseStoryEncounter == null ? null : Eclipse.Modding.ModRuntime.CaptureBattleResult(roster, fightList, gameOverType, playerParameters, opponentParameters);
		FightResult fightResult = new FightResult();
		fightResult.FightDefinition = fightList;
		fightResult.FightType = fightList.get_Type();
		fightResult.FightId = new FightIDS(fightList.FightId);
		fightResult.PlayerParameters = playerParameters;
		fightResult.OpponentParameters = opponentParameters;
		fightResult.GameOverType = gameOverType;
		int num = fightList.RewardIndex;
		if (fightResult.IsWinner())
		{
			num++;
		}
		if (fightList.HasMultipleOpponentsAndRounds())
		{
			num = (fightResult.IsWinner() ? 1 : 0);
		}
		RewardStruct reward = fightList.GetRewardAt(num);
		fightResult.CalculateRewards(reward, playerCombo, opponentCombo, fightList);
		if (!flag)
		{
			ArgsDict eventArgs = new ArgsDict();
			eventArgs.Add("fightResult", fightResult);
			eventArgs.Add("isSurrender", flag2);
			eventArgs.Add("avgFps", averageFps);
			eventArgs.Add("fightList", fightList);
			eventArgs.Add("fightTimeElapsed", fightTimeElapsed);
			StatisticsCollector.LogEvent(StatisticsEvent.EventType.Fight_End, eventArgs);
		}
		bool flag3 = false;
		if (!flag2)
		{
			fightList.Battle.UpdateRosterFight(fightList, fightResult.IsWinner());
			if (fightList.get_Type() == BattleType.FightPeriodic)
			{
				if (fightResult.IsWinner())
				{
					fightList.Battle.UpdateByTime(ListSF.GetCurrentTime());
					fightList.GetRosterFight().RandomizeSeeds();
				}
				SchedulePeriodicFightNotification(fightList, fightResult.IsWinner());
			}
			BattleType battleType = fightList.get_Type();
			if (battleType == BattleType.FightReplayable || battleType == BattleType.FightBossesReplayable || battleType == BattleType.FightFinalReplayable)
			{
				BattleReplayable replayableBattle = (BattleReplayable)fightList.Battle;
				if (replayableBattle.GetStatus() == ConditionStatus.StatusComplete)
				{
					ListSF.RegisterReplayableBattle(replayableBattle);
				}
			}
			flag3 = ListSF.GetInstance().ApplyFightRewards(fightResult);
		}
		Eclipse.Modding.ModModeRuntime.Complete(fightList, !flag2 && fightResult.IsWinner());
        if (Eclipse.Modding.ModModeRuntime.IsRaid(fightList)) Eclipse.Modding.ModModeRuntime.SetRaidResult(fightResult);
		QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
		if (fightList.get_Type() == BattleType.FightAscension)
		{
			Battle battle = ListSF.GetBattleById(fightList.FightId);
			BattleAscension ascensionBattle = (BattleAscension)battle;
			if (flag2 || !fightResult.IsWinner())
			{
				ascensionBattle.SetAscensionLevel(1);
				ListSF.RegisterReplayableBattle(ascensionBattle);
				questParameters.fightIds = fightList.FightId;
				if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_RESET_ASCENSION))
				{
					ListSF.GetInstance().RunQuestActions();
				}
			}
			else
			{
				ascensionBattle.AdvanceAscensionAfterFight(fightList);
				if (ascensionBattle.GetRosterBattle() != null && ascensionBattle.GetRosterBattle().GetAscensionLevel() > ascensionBattle.GetFightCount())
				{
					ascensionBattle.SetAscensionLevel(1);
					ListSF.RegisterReplayableBattle(ascensionBattle);
				}
			}
			ascensionBattle.RefreshAllFightStatuses();
		}
		questParameters.fightIds = fightList.FightId;
		questParameters.fightResult = (fightResult.IsWinner() ? "Win" : ((!flag2) ? "Loss" : "Surrender"));
		questParameters.levelUp = (flag3 ? 1 : 0);
		questParameters.fightAvgFps = averageFps;
		if (fightList.get_Type() == BattleType.FightRaid)
		{
			questParameters.raidId = fightList.FightId.GetBattle();
		}
        Eclipse.Modding.ModModeRuntime.NotifyResult(fightList);
		// Use this result's already composed reward. Scanning all reward scopes
		// can defer a normal win because another win count contains a lottery,
		// and re-evaluates reward expressions after progression/level-up changes.
		bool flag4 = !flag2 && fightResult.Prize.Lottery != null;
		if (flag4 && fightResult.IsWinner())
		{
			ListSF.GetInstance().LotteryQuestParameters = questParameters;
			Eclipse.Modding.ModRuntime.PrepareBattleLottery(fightResult.Prize.Lottery, questParameters,
				fightList.get_Type() == BattleType.FightRaid, fightList.EclipseStoryEncounter?.Id);
		}
		if ((!flag4 || !fightResult.IsWinner()) && ((fightList.get_Type() != BattleType.FightRaid && ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_FIGHT_END)) || (fightList.get_Type() == BattleType.FightRaid && ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_FIGHT_END))) && flag)
		{
			ListSF.GetInstance().RunQuestActions();
		}
		ListSF.GetInstance().RequestSave();
		MenuController.RefreshMoney();
		if (CanShowRatePrompt(fightList.FightId.ToString()) && fightResult.IsWinner() && (fightList.get_Type() == BattleType.FightBosses || fightList.get_Type() == BattleType.FightFinalTitan || fightList.get_Type() == BattleType.FightBossesReplayable || fightList.get_Type() == BattleType.FightTournament || fightList.get_Type() == BattleType.FightAscension))
		{
			ShowRatePrompt = true;
		}
		MenuController.RefreshMenu();
		if (!flag)
		{
			if (fightList.get_Type() == BattleType.FightRaid && !Eclipse.Modding.ModModeRuntime.IsRaid(fightList))
			{
				int num2 = 0;
				ModelParameters raidBossParameters = ((!playerParameters.IsPlayer) ? playerParameters : opponentParameters);
				float num3 = (ObscuredFloat)(raidBossParameters.GetCurrentLife());
				float num4 = (float)num2 - num3;
				float num5 = 0f;
				if (num5 > num4)
				{
					num5 = num4;
				}
				float num6 = num4 - num5;
				if (fightResult.IsWinner() || num6 > (float)num2 - num5)
				{
					num6 = (float)num2 - num5;
				}
				RaidModelParameters raidParameters = ((!playerParameters.IsPlayer) ? opponentParameters : playerParameters) as RaidModelParameters;
				if (raidDamage > 0 && raidParameters == null)
				{
				}
			}
			else if (flag2)
			{
				HandleSurrender(fightResult);
			}
			else if (Fight.GetCurrentFight() != null)
			{
				if (Eclipse.Modding.ModModeRuntime.IsRaid(fightList)) Eclipse.Modding.ModModeRuntime.ShowRaidResult();
                else Fight.GetCurrentFight().ShowEndFightScreen(fightResult);
			}
		}
		if (fightResult.IsWinner())
		{
			TrackEvent("Stage Complete");
		}
		else
		{
			TrackEvent("Stage Failed");
		}
		if (Eclipse.Modding.ModRuntime.StoryEvents.TryCompleteEncounterResult(eclipseStoryEncounter) && eclipseStoryResult != null)
			Eclipse.Modding.ModRuntime.StoryEvents.Publish(eclipseStoryResult);
		Eclipse.Modding.ModRuntime.ShowPendingBattleLottery();
	}

	public static void NotifyApplicationStart()
	{
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_START_APPLICATION))
		{
			ListSF.GetInstance().RunQuestActions();
		}
	}

	public static bool StartFight(FightList fightList = null, bool forceStart = false, Battle sourceBattle = null, bool playGong = true, bool raiseQuestEvents = true)
	{
		if (fightList == null)
		{
			GameLog.Error("GameUtils::StartFight(..) Error! FightList is empty!");
			return false;
		}
        if (Eclipse.Modding.ModRuntime.HasPendingLottery)
        {
            Eclipse.Modding.ModRuntime.ShowPendingBattleLottery();
            return false;
        }
        if (Eclipse.Modding.ModRuntime.StoryEvents.FightEntries.HasPending) return false;
        var requestedFight = fightList;
        if (!Eclipse.Modding.ModModeRuntime.PrepareEntry(requestedFight, () => StartFight(requestedFight,forceStart,sourceBattle,playGong,raiseQuestEvents))) return false;
		if (!Eclipse.Modding.ModModeRuntime.ResolveEntry(ref fightList)) return false;
        var storyFight = fightList;
        var storyEntry = Eclipse.Modding.ModRuntime.TryStoryFightEntry(storyFight,
            () => StartFight(storyFight, forceStart, sourceBattle, playGong, raiseQuestEvents));
        if (storyEntry.HasValue) return storyEntry.Value;
		ScreenType currentScreenType = Module.GetInstance().GetCurrentScreenType();
		FightList holderFight = null;
		if (currentScreenType == ScreenType.ModuleDojo || currentScreenType == ScreenType.ModuleFight)
		{
			holderFight = FightHolder.fightList;
		}
		if (currentScreenType == ScreenType.ModuleFight && fightList == holderFight)
		{
			return false;
		}
		Roster roster = ListSF.GetRoster();
		RosterFight rosterFight = roster.FindSavedFightRecord(fightList.FightId);
		if (rosterFight == null)
		{
			rosterFight = roster.CreateFight(fightList.FightId);
		}
		fightList.SetRosterFight(rosterFight);
		Battle battle = fightList.Battle;
		if (battle.get_Type() != BattleType.FightUnregister)
		{
			ListSF.GetRoster().SetFightIds(fightList.FightId);
		}
		if (raiseQuestEvents)
		{
			QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
			questParameters.fightIds = fightList.FightId;
			questParameters.fightResult = string.Empty;
			questParameters.raidResult = string.Empty;
			if (fightList.get_Type() == BattleType.FightRaid)
			{
				questParameters.raidId = fightList.FightId.GetBattle();
			}
			if ((fightList.get_Type() != BattleType.FightRaid && ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_FIGHT_ENTER)) || (fightList.get_Type() == BattleType.FightRaid && ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_FIGHT_ENTER)))
			{
				ListSF.GetInstance().RunQuestActions();
				return true;
			}
		}
		if (!Eclipse.Modding.ModModeRuntime.Begin(fightList)) return false;
		var eclipseStoryEncounter = Eclipse.Modding.ModRuntime.StoryEvents.BeginEncounter();
		fightList.EclipseStoryEncounter = eclipseStoryEncounter;
		if (fightList.get_Type() != BattleType.FightPeriodic)
		{
			fightList.GetRosterFight().SetCompletionTimestamp(ListSF.GetServerTime());
		}
		if (fightList.get_Type() == BattleType.FightAscension)
		{
		}
		ListSF.GetInstance().RequestSave();
		if (playGong)
		{
			Sound.PlaySound("snd_gong");
			IsFightTransitioning = false;
		}
		MapScene current = Scene<MapScene>.get_Current();
		if (!AutoWinPending || (!(AutoWinFightKey == fightList.Location + fightList.Battle.get_Name() + fightList.Name) && !(fightList.FightId.GetBattle() == "QuestBattle")))
		{
			if (current != null)
			{
				current.EnableMapButtons(false);
			}
			if (!Module.OpenScreen(ScreenType.ModuleFight, fightList))
			{
                if (current != null) current.EnableMapButtons(true);
                Eclipse.Modding.ModModeRuntime.CancelLaunch(fightList);
				Eclipse.Modding.ModRuntime.StoryEvents.CancelEncounter(eclipseStoryEncounter);
                return false;
			}
		}
		else
		{
			AutoWinPending = false;
			RosterFight savedRosterFight = roster.FindSavedFightRecord(fightList.FightId);
			if (savedRosterFight == null)
			{
				savedRosterFight = roster.CreateFight(fightList.FightId);
			}
			fightList.SetRosterFight(savedRosterFight);
			int num = fightList.GetRewards().Count - 2;
			if (num < 0)
			{
				num = 0;
			}
			fightList.RewardIndex = num;
			ComboStatistic comboStatistic = new ComboStatistic();
			EndFight(comboStatistic, fightList, ListSF.GetRoster().get_Parameters(), null, GameOverTypes.GAME_OVER_WIN);
			if (current != null)
			{
				current.GetInfoBattle().UpdateBattleInfo(current.GetInfoBattle().GetCurrentBattle());
			}
		}
		FightHolder.fightList = fightList;
        Eclipse.Modding.ModModeRuntime.NotifyEntry(fightList);
		return true;
	}

	public static void GrantEnergyAndRefreshNotifications()
	{
		ListSF.ChangeEnergy(10);
		ListSF.GetInstance().RequestSave();
		LocalNotificationManager.GetInstance().CancelEnergy();
		LocalNotificationManager.GetInstance().CancelEnergyFull();
	}

	public static void OpenProfileScreen()
	{
		Module.OpenScreen(ScreenType.ModuleProfile, 0);
	}

	public static string GetDefaultAvatar()
	{
		return defaultAvatar;
	}

	public static void SetDefaultAvatar(string value)
	{
		defaultAvatar = value;
	}

	// best guess for name
	public static string GetDefaultSkeleton()
	{
		return defaultSkeleton;
	}

	public static void SetDefaultSkeleton(string value)
	{
		defaultSkeleton = value;
	}

	public static void ParseDefaultItems(XmlNode node)
	{
		defaultItems.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if ("Item" == childNode.Name)
			{
				string itemType = childNode.Attributes["Type"].GetStringOrDefault(string.Empty);
				string itemName = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
				defaultItems.Add(new global::Pair<string, string>(itemType, itemName));
			}
		}
	}

	public static string GetDefaultItem(string itemType)
	{
		foreach (global::Pair<string, string> item in defaultItems)
		{
			if (item.First == itemType)
			{
				return item.Second;
			}
		}
		return string.Empty;
	}

	public static long GetCurrentTime()
	{
		return ListSF.GetCurrentTime();
	}

	public static long GetLeftTime(long time)
	{
		long num = GetCurrentTime();
		return (time <= num) ? 0 : (time - num);
	}

	public static void ScheduleStartupNotifications()
	{
		LocalNotificationManager.GetInstance().CancelRetention();
		LocalNotificationManager.GetInstance().ScheduleRetention(PushRetentionTime);
	}

	public static void ShowEnterScreen(string screenName, Action onComplete)
	{
		EnterScreen enterScreen = EnterScreen.Create();
		enterScreen.Init(screenName, onComplete);
	}

	public static void ShowEnterScreen(List<KeyValuePair<string, int>> entries, Action onComplete)
	{
		EnterScreen enterScreen = EnterScreen.Create();
		enterScreen.Init(entries, onComplete);
	}

	public static Trick GetTrickByName(string name)
	{
		return GetPlayerTricks().Find((Trick trick) => trick.Name == name);
	}

	public static List<Trick> GetPlayerTricks(SceneTypes sceneType = SceneTypes.SceneFight)
	{
		List<Trick> list = new List<Trick>();
		List<ItemInfo> equippedItems = GetPlayerModelParameters().GetEquippedItems();
		List<PerkInfoItem> perks = GetPlayerModelParameters().GetAllPerks();
		bool includeLocked = false;
		AnimationData.CollectAvailableTricks(list, equippedItems, includeLocked, null, perks, sceneType);
		return list;
	}

	private static bool CanShowRatePrompt(string fightId)
	{
		int count = _notShowRateFights.Count;
		for (int i = 0; i < count; i++)
		{
			if (_notShowRateFights[i] == fightId)
			{
				return false;
			}
		}
		return true;
	}

	public static void TrackItemAction(ItemInfo item, ItemAction itemAction, int count, long timestamp)
	{
	}

	public static void TrackDelivery(string itemName)
	{
	}

	private static void ScheduleEnchantmentNotification(string localizationKey, RecipeItemInfo item)
	{
		if (item != null && item.GetUserItem() != null)
		{
			string text = item.GetUserItem().get_Name();
			string message = LocalizationManager.GetString(localizationKey, "1", text);
			long num = item.GetDeliveryEndTime() - GetCurrentTime();
			if (num > 0)
			{
				LocalNotificationManager.GetInstance().ScheduleRecipe(message, num);
			}
		}
	}

	private static void CancelEnchantmentNotification(string localizationKey, RecipeItemInfo item)
	{
		if (item != null && item.GetUserItem() != null)
		{
			LocalNotificationManager.GetInstance().CancelRecipe();
		}
	}

	private static void ScheduleItemNotification(string localizationKey, UserItem item)
	{
		if (item != null)
		{
			string text = LocalizationManager.GetString(item.get_Name());
			string message = LocalizationManager.GetString(localizationKey, "1", text);
			long num = item.GetDeliveryTimestamp() - GetCurrentTime();
			if (num > 0)
			{
				LocalNotificationManager.GetInstance().ScheduleItem(message, num);
			}
		}
	}

	private static void CancelItemNotification(string localizationKey, UserItem item)
	{
		if (item != null)
		{
			LocalNotificationManager.GetInstance().CancelItem();
		}
	}

	private static void SchedulePeriodicFightNotification(FightList fightList, bool isWinner)
	{
		if (fightList.get_Type() == BattleType.FightPeriodic)
		{
			RosterFight rosterFight = fightList.GetRosterFight();
			long delaySeconds = ((!isWinner) ? (rosterFight.GetRandomizeTimestamp() + fightList.RepeatTime - ListSF.GetCurrentTime()) : (rosterFight.GetCompletionTimestamp() + fightList.RepeatTime - ListSF.GetCurrentTime()));
			LocalNotificationManager.GetInstance().CancelPeriodic();
			LocalNotificationManager.GetInstance().SchedulePeriodic(delaySeconds);
		}
	}

	public static void UpdateShopNewItemsCounters()
	{
		ShopScene current = Scene<ShopScene>.get_Current();
		if (current != null)
		{
			current.UpdateNewItemsCounters();
		}
	}

	public static void SetFightFlag(bool value)
	{
		fightFlag = value;
	}

	public static bool GetFightFlag()
	{
		return fightFlag;
	}

	public static float GetDialogContentWidth()
	{
		return 650f;
	}

	public static void InitVariables()
	{
		ReduceFps = false;
		ParticlesDisabled = false;
		SequencesDisabled = false;
	}

	public static SliderType GetSliderTypeByName(string sliderName)
	{
		foreach (KeyValuePair<SliderType, string> item in SliderNames)
		{
			if (item.Value == sliderName)
			{
				return item.Key;
			}
		}
		return SliderType.SliderNone;
	}

	public static float CalculateFightDifficulty(FightList fight)
	{
		List<ModelParameters> list = CreateOpponentParameters(fight.GetOpponents());
		ModelParameters playerParameters = Eclipse.Modding.ModRuntime.BuildFightPlayerParameters(fight) ?? GetPlayerModelParameters().Clone();
		playerParameters.SetItemsFromRules(fight.GetItemRules(), true);
		playerParameters.CalculateAttributes();
		float result = fight.CalculateDifficultyVsLastOpponent(playerParameters, list);
		list.Clear();
		return result;
	}

	public static int ParseAspects(XmlNode node)
	{
		aspects.Clear();
		int num = 0;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name.Equals("Aspect"))
			{
				Aspect aspect = new Aspect();
				aspect.Parse(childNode);
				aspects.Add(aspect);
				num++;
			}
		}
		return num;
	}

	public static void ParseAspectConstants(XmlNode node)
	{
		GetAspectConstants().Antilimit = node.Attributes["Antilimit"].ParseFloat();
		GetAspectConstants().DoublingRange = node.Attributes["DoublingRange"].ParseFloat();
		GetAspectConstants().Limit = node.Attributes["Limit"].ParseFloat();
	}

	public static void RegisterAspectAttributes()
	{
		foreach (Aspect item in GetAspects())
		{
			WarriorAttribute aspectAttribute = new WarriorAttribute();
			aspectAttribute.set_Name(item.get_Name());
			aspectAttribute.IsHidden = true;
			aspectAttribute.IsShopHidden = true;
			aspectAttribute.IsProfileHidden = true;
			aspectAttribute.UnusedFlag = true;
			WarriorAttributeList.AttributeList.Add(aspectAttribute);
		}
	}

	public static Aspect GetAspectByName(string name)
	{
		List<Aspect> list = GetAspects();
		foreach (Aspect item in list)
		{
			if (item.get_Name().Equals(name))
			{
				return item;
			}
		}
		return null;
	}

	public static void InitSliderNames()
	{
		SliderNames[SliderType.SliderNone] = "Default";
		SliderNames[SliderType.SliderWeapon] = "Weapon";
		SliderNames[SliderType.SliderArmor] = "Armor";
		SliderNames[SliderType.SliderHelmet] = "Helm";
		SliderNames[SliderType.SliderMissile] = "Ranged";
		SliderNames[SliderType.SliderMagic] = "Magic";
		SliderNames[SliderType.SliderRuby] = "Ruby";
		SliderNames[SliderType.SliderFree] = "Free";
		SliderNames[SliderType.SliderCheat] = "Cheat";
		SliderNames[SliderType.SliderPerks] = "Perks";
		SliderNames[SliderType.SliderTricks] = "Moves";
		SliderNames[SliderType.SliderAchievements] = "Achievements";
		SliderNames[SliderType.SliderSeals] = "QuestItems";
		SliderNames[SliderType.SliderCount] = "Count";
		SliderNames[SliderType.SliderRaidItemPack] = "RaidConsumable";
		SliderNames[SliderType.SliderRaidMap] = "RaidMapStage";
		SliderNames[SliderType.SliderStoryMap] = "StoryMapStage";
	}

	public static void InitRaidTutorialStepNames()
	{
		RaidTutorialStepNames[RaidTutorialStepCode.RaidTutNotStarted] = "NotStarted";
		RaidTutorialStepNames[RaidTutorialStepCode.RaidTutToggleButton] = "RaidButtonTutorial";
		RaidTutorialStepNames[RaidTutorialStepCode.RaidTutGiveKey] = "RaidGiveKeyTutorial";
		RaidTutorialStepNames[RaidTutorialStepCode.RaidTutFirstRaidEnter] = "FirstRaidEnterTutorial";
		RaidTutorialStepNames[RaidTutorialStepCode.RaidTutFinalFight] = "RaidFinalFightTutorial";
		RaidTutorialStepNames[RaidTutorialStepCode.RaidTutLeagueWindow] = "LeagueWindowTutorial";
		RaidTutorialStepNames[RaidTutorialStepCode.RaidTutShopNotification] = "RaidShopNotification";
		RaidTutorialStepNames[RaidTutorialStepCode.RaidTutComplete] = "TutorialComplete";
	}

	public static RaidTutorialStepCode GetRaidTutorialStepByName(string stepName)
	{
		foreach (KeyValuePair<RaidTutorialStepCode, string> item in RaidTutorialStepNames)
		{
			if (item.Value == stepName)
			{
				return item.Key;
			}
		}
		return RaidTutorialStepCode.RaidTutNotStarted;
	}

	public static void CommitPendingAchievements()
	{
		foreach (Achievement item in pendingAchievements)
		{
			item.IsUnlocked = true;
		}
		pendingAchievements.Clear();
	}

	public static long GetDenominatedValue(long value, int digitOffset = 0)
	{
		int num = ListSF.GetRoster().GetDenominationDigits();
		float f = (float)value / Mathf.Pow(10f, num - digitOffset);
		return (long)Mathf.Ceil(f);
	}

	public static bool HasEnoughCurrencyForFight(FightList fightList)
	{
		List<CurrencyCostRule> list = fightList.GetCurrencyCostRules();
		if (list.Count == 0)
		{
			return true;
		}
		foreach (CurrencyCostRule item in list)
		{
			string text = item.GetCurrencyName();
			if (!(text != string.Empty))
			{
				continue;
			}
			int num = ListSF.GetRoster().GetCurrencyCount(text);
			int num2 = 0;
			foreach (CurrencyCostRule item2 in list)
			{
				if (item2.GetCurrencyName() == text)
				{
					int num3 = item2.GetCurrencyValue();
					num2 += num3;
				}
			}
			if (num < num2)
			{
				return false;
			}
		}
		return true;
	}

	public static void TrackEvent(string eventName)
	{
	}

	public static bool IsPerkCompatibleWithEquipment(PerkInfoItem perk)
	{
		List<UserItem> list = ListSF.GetRoster().GetInventory().GetEquippedItems();
		for (int i = 0; i < list.Count; i++)
		{
			if (IsEquipmentItem(list[i].GetInfo()) && !list[i].HasEnchantment(perk))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsEquipmentItem(ItemInfo item)
	{
		// Newer vanilla lists can legitimately omit historical/seasonal item
		// definitions still present in an existing local save. Such UserItems
		// remain useful as save data, but they cannot participate in equipment
		// perk compatibility until an ItemInfo exists in the active list.
		if (item == null)
		{
			return false;
		}
		switch (item.Type)
		{
		case "Weapon":
		case "Armor":
		case "Helm":
		case "Ranged":
		case "Magic":
			return true;
		default:
			return false;
		}
	}

	public static void PrintTextureCache()
	{
		if (AssemblyController.GetCacheTexturesLog())
		{
			GameLog.Info(string.Empty);
			GameLog.Info("------------------------------Print texture cache ------------------------------");
			GameLog.Info("------------------------------Screen: " + Module.GetScreenName(Module.GetInstance().GetCurrentScreenType()) + " ------------------------------");
		}
	}
}
