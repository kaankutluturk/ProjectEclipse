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
				HitEffect pIHIIMOOICM = new HitEffect();
				pIHIIMOOICM.Type = childNode.Attributes["Type"].GetStringOrDefault(string.Empty);
				pIHIIMOOICM.PauseTime = childNode.Attributes["PauseTime"].ParseInt();
				pIHIIMOOICM.EffectTime = childNode.Attributes["EffectTime"].ParseInt();
				pIHIIMOOICM.AmplitudeX = childNode.Attributes["AmplitudeX"].ParseFloat();
				pIHIIMOOICM.AmplitudeY = childNode.Attributes["AmplitudeY"].ParseFloat();
				pIHIIMOOICM.FrequencyX = childNode.Attributes["FrequencyX"].ParseFloat();
				pIHIIMOOICM.FrequencyY = childNode.Attributes["FrequencyY"].ParseFloat();
				Effects.Add(pIHIIMOOICM);
			}
		}

		public HitEffect GetEffectByType(string LFLGCDNKNJI)
		{
			foreach (HitEffect item in Effects)
			{
				if (item.Type == LFLGCDNKNJI)
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

		public static float GetValue(List<AlignTargetAttribute> BBNKIBKPBLO, string name)
		{
			for (int i = 0; i < BBNKIBKPBLO.Count; i++)
			{
				if (BBNKIBKPBLO[i].Name == name)
				{
					return BBNKIBKPBLO[i].Value;
				}
			}
			return 0f;
		}

		public static int Parse(XmlNode AFHNINCKJEE, List<AlignTargetAttribute> OEMALIFPGPO)
		{
			int count = OEMALIFPGPO.Count;
			foreach (XmlNode childNode in AFHNINCKJEE.ChildNodes)
			{
				AlignTargetAttribute kDDAFBJDOMN = new AlignTargetAttribute();
				kDDAFBJDOMN.Parse(childNode);
				OEMALIFPGPO.Add(kDDAFBJDOMN);
			}
			return OEMALIFPGPO.Count - count;
		}

		private void Parse(XmlNode AFHNINCKJEE)
		{
			Name = XmlUtils.ParseString(AFHNINCKJEE.Attributes["Name"]);
			Value = XmlUtils.ParseFloat(AFHNINCKJEE.Attributes["Value"]);
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

		public void Parse(XmlNode IKGBGEEMPCD)
		{
			Links.Clear();
			foreach (XmlNode childNode in IKGBGEEMPCD.ChildNodes)
			{
				SupportLink kDFMEPMFMEE = new SupportLink();
				kDFMEPMFMEE.Name = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
				kDFMEPMFMEE.Url = childNode.Attributes["Url"].GetStringOrDefault(string.Empty);
				Links.Add(kDFMEPMFMEE);
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

		public string GetUrl(string name, string JAGNJBDLEMF)
		{
			SupportLink kDFMEPMFMEE = FindLink(name);
			if (kDFMEPMFMEE == null)
			{
				kDFMEPMFMEE = FindLink(JAGNJBDLEMF);
			}
			return (kDFMEPMFMEE == null) ? string.Empty : kDFMEPMFMEE.Url;
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
			GameCurrency cJJOFMHLFFM = new GameCurrency(node);
			bool flag = true;
			foreach (GameCurrency item in currencies)
			{
				if (item.Name == cJJOFMHLFFM.Name)
				{
					item.CopyFrom(cJJOFMHLFFM);
					flag = false;
				}
			}
			if (flag)
			{
				currencies.Add(cJJOFMHLFFM);
			}
		}

		public void AddCurrency(GameCurrency KHBNGFMPEBG)
		{
			currencies.Add(KHBNGFMPEBG);
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

		public void Parse(XmlNode EBLIGDMALEA)
		{
			resistances.Clear();
			foreach (XmlNode childNode in EBLIGDMALEA.ChildNodes)
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

		public void Parse(XmlNode EBLIGDMALEA)
		{
			AllCounters.Clear();
			NormalModeCounters.Clear();
			EclipseModeCounters.Clear();
			RaidModeCounters.Clear();
			foreach (XmlNode childNode in EBLIGDMALEA.ChildNodes)
			{
				Counter jLOIMNNHFKH = new Counter(childNode);
				AllCounters[jLOIMNNHFKH.Name] = jLOIMNNHFKH;
				switch (jLOIMNNHFKH.Mode)
				{
				case Counter.CounterMode.ECLIPSE_MODE:
					EclipseModeCounters[jLOIMNNHFKH.Name] = jLOIMNNHFKH;
					break;
				case Counter.CounterMode.NORMAL_MODE:
					NormalModeCounters[jLOIMNNHFKH.Name] = jLOIMNNHFKH;
					break;
				case Counter.CounterMode.RAID_MODE:
					RaidModeCounters[jLOIMNNHFKH.Name] = jLOIMNNHFKH;
					break;
				case Counter.CounterMode.NONE_MODE:
					EclipseModeCounters[jLOIMNNHFKH.Name] = jLOIMNNHFKH;
					NormalModeCounters[jLOIMNNHFKH.Name] = jLOIMNNHFKH;
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

		public Dictionary<string, Counter> GetCountersByMode(Counter.CounterMode NMMPBADCFHK)
		{
			switch (NMMPBADCFHK)
			{
			case Counter.CounterMode.ECLIPSE_MODE:
				return EclipseModeCounters;
			case Counter.CounterMode.RAID_MODE:
				return RaidModeCounters;
			default:
				return NormalModeCounters;
			}
		}

		public Dictionary<string, Counter> GetCountersForFight(FightList KGKDKENMAOA)
		{
			if (KGKDKENMAOA != null && KGKDKENMAOA.get_Type() == BattleType.FightRaid)
			{
				return RaidModeCounters;
			}
			Counter.CounterMode nMMPBADCFHK = ((!ListSF.GetRoster().IsEclipseMode()) ? Counter.CounterMode.NORMAL_MODE : Counter.CounterMode.ECLIPSE_MODE);
			return GetCountersByMode(nMMPBADCFHK);
		}

		public Dictionary<string, Counter> GetCountersByType(string LFLGCDNKNJI, Counter.CounterMode NMMPBADCFHK)
		{
			Dictionary<string, Counter> dictionary = new Dictionary<string, Counter>();
			Dictionary<string, Counter> dictionary2 = GetCountersByMode(NMMPBADCFHK);
			foreach (KeyValuePair<string, Counter> item in dictionary2)
			{
				Counter value = item.Value;
				if (value.Type == LFLGCDNKNJI)
				{
					dictionary[item.Key] = value;
				}
			}
			return dictionary;
		}

		public Counter FindCounterByAchievement(string LMBBKNMNKOB)
		{
			for (int i = 0; i < 3; i++)
			{
				Counter.CounterMode nMMPBADCFHK = (Counter.CounterMode)i;
				Dictionary<string, Counter> dictionary = GetCountersByMode(nMMPBADCFHK);
				foreach (KeyValuePair<string, Counter> item in dictionary)
				{
					Counter value = item.Value;
					AchievCounter iFDAFNGCIBP = AchievementDefinitions.GetCounterByName(value.Name);
					if (iFDAFNGCIBP == null)
					{
						continue;
					}
					List<Achievement> fOICCCGPCMJ = iFDAFNGCIBP.Achievements;
					bool flag = false;
					for (int j = 0; j < fOICCCGPCMJ.Count; j++)
					{
						Achievement jNPIOKEKMII = fOICCCGPCMJ[j];
						if (jNPIOKEKMII.Name == LMBBKNMNKOB)
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

		public void Parse(XmlNode EBLIGDMALEA)
		{
			Counters.Clear();
			foreach (XmlNode childNode in EBLIGDMALEA.ChildNodes)
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

		public List<global::Pair<Achievement, int>> GetUnlockableAchievements(List<string> EAHPNCOEHJG)
		{
			List<global::Pair<Achievement, int>> list = new List<global::Pair<Achievement, int>>();
			foreach (string item in EAHPNCOEHJG)
			{
				AchievCounter iFDAFNGCIBP = AchievementDefinitions.GetCounterByName(item);
				RosterAchievCounter cKJBHGKBPPM = ListSF.GetRoster().GetAchievements().FindCounter(item);
				if (iFDAFNGCIBP == null || cKJBHGKBPPM == null)
				{
					continue;
				}
				int num = cKJBHGKBPPM.GetCounter();
				foreach (Achievement item2 in iFDAFNGCIBP.Achievements)
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
				RosterAchievCounter cKJBHGKBPPM = list2[i];
				string gOHIIMFFFJI = cKJBHGKBPPM.get_Name();
				AchievCounter iFDAFNGCIBP = AchievementDefinitions.GetCounterByName(gOHIIMFFFJI);
				if (iFDAFNGCIBP == null)
				{
					continue;
				}
				List<Achievement> fOICCCGPCMJ = iFDAFNGCIBP.Achievements;
				for (int j = 0; j < fOICCCGPCMJ.Count; j++)
				{
					Achievement jNPIOKEKMII = fOICCCGPCMJ[j];
					if (cKJBHGKBPPM.GetCounter() >= jNPIOKEKMII.CounterValue)
					{
						list.Add(jNPIOKEKMII);
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

		public bool IsStepName(string NOFMEBBEDKK)
		{
			return StepsNames.Contains(NOFMEBBEDKK);
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

	public static Achievement FindReachedAchievement(Counter EPJGLECOIBG, int value)
	{
		Achievement jNPIOKEKMII = null;
		AchievCounter iFDAFNGCIBP = AchievementDefinitions.GetCounterByName(EPJGLECOIBG.Name);
		if (iFDAFNGCIBP == null)
		{
			return null;
		}
		RosterAchievCounter cKJBHGKBPPM = ListSF.GetRoster().GetAchievements().FindCounter(EPJGLECOIBG.Name);
		int num = ((cKJBHGKBPPM != null) ? cKJBHGKBPPM.GetCounter() : 0);
		List<Achievement> fOICCCGPCMJ = iFDAFNGCIBP.Achievements;
		int num2 = num + value;
		for (int i = 0; i < fOICCCGPCMJ.Count; i++)
		{
			Achievement jNPIOKEKMII2 = fOICCCGPCMJ[i];
			if (jNPIOKEKMII2.CounterValue >= num && jNPIOKEKMII2.CounterValue <= num2 && !jNPIOKEKMII2.IsUnlocked)
			{
				jNPIOKEKMII = jNPIOKEKMII2;
				if (!EPJGLECOIBG.IsFightEnd)
				{
					jNPIOKEKMII.IsUnlocked = true;
				}
				else
				{
					pendingAchievements.Add(jNPIOKEKMII);
				}
			}
			else if (jNPIOKEKMII2.CounterValue >= num && jNPIOKEKMII2.CounterValue >= num2)
			{
				break;
			}
		}
		return jNPIOKEKMII;
	}

	public static Achievement GiveAchievement(string name)
	{
		Achievement jNPIOKEKMII = AchievementDefinitions.GetAchievementByName(name);
		if (jNPIOKEKMII == null)
		{
			return null;
		}
		if (!jNPIOKEKMII.IsUnlocked)
		{
			jNPIOKEKMII.IsUnlocked = true;
		}
		RosterAchievement pMGCOHHMIIC = ListSF.GetRoster().GetAchievements().FindAchievement(name);
		if (pMGCOHHMIIC == null)
		{
			Counter jLOIMNNHFKH = ModeCounters.FindCounterByAchievement(name);
			if (jLOIMNNHFKH != null && jLOIMNNHFKH.CompleteValue <= 0)
			{
				jLOIMNNHFKH.CompleteValue = 1;
			}
		}
		ListSF.GetRoster().GetAchievements().ApplyPendingCounters();
		return jNPIOKEKMII;
	}

	public static void UnlockAchievements(List<global::Pair<Achievement, int>> CIMGCGDDKCE)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		List<SocialAchievement> list = new List<SocialAchievement>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < CIMGCGDDKCE.Count; i++)
		{
			Achievement lLHEDBIEHAA = CIMGCGDDKCE[i].First;
			int nFNBFHCDEGG = CIMGCGDDKCE[i].Second;
			SocialAchievement iFLOCBJBEGL = new SocialAchievement(lLHEDBIEHAA.PlatformId, nFNBFHCDEGG, lLHEDBIEHAA.CounterValue);
			if (nFNBFHCDEGG >= lLHEDBIEHAA.CounterValue)
			{
				nKGLHEGIKKP.GetAchievements().UnlockAchievement(lLHEDBIEHAA, false);
				iFLOCBJBEGL.value = lLHEDBIEHAA.CounterValue;
			}
			list.Add(iFLOCBJBEGL);
			list2.Add(lLHEDBIEHAA.Name);
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
			Achievement jNPIOKEKMII = AchievementDefinitions.GetAchievementByName(list[i].get_Name());
			// Local mod achievements have no platform achievement identity.
			if (jNPIOKEKMII != null && !string.IsNullOrEmpty(jNPIOKEKMII.PlatformId))
			{
				list2.Add(new SocialAchievement(jNPIOKEKMII.PlatformId, jNPIOKEKMII.CounterValue, jNPIOKEKMII.CounterValue));
				list3.Add(jNPIOKEKMII.Name);
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
		List<AchievCounter> mDNKEAFGAOB = AchievementDefinitions.Counters;
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < mDNKEAFGAOB.Count; j++)
			{
				RosterAchievCounter cKJBHGKBPPM = ListSF.GetRoster().GetAchievements().FindCounter(mDNKEAFGAOB[j].Name);
				if (cKJBHGKBPPM == null)
				{
					continue;
				}
				List<Achievement> fOICCCGPCMJ = mDNKEAFGAOB[j].Achievements;
				for (int k = 0; k < fOICCCGPCMJ.Count; k++)
				{
					if (fOICCCGPCMJ[k].Name == list[i].get_Name() && !string.IsNullOrEmpty(fOICCCGPCMJ[k].PlatformId))
					{
						if (fOICCCGPCMJ[k].CounterValue == 0)
						{
							GameLog.Write("Error - counter == 0");
						}
						SocialAchievement item = new SocialAchievement(fOICCCGPCMJ[k].PlatformId, cKJBHGKBPPM.GetCounter(), fOICCCGPCMJ[k].CounterValue);
						list2.Add(item);
					}
				}
			}
		}
		GameCenterController.SyncAchievements(list2);
		ListSF.GetRoster().GetAchievements().RemoveRepostAchievements(list);
	}

	public static bool AvatarExists(string FHLFEBDNIFF)
	{
		string path = string.Format("{0}{1}", SF2Paths.GetUsersUiPath(), FHLFEBDNIFF);
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

	public static bool IsAlignTargetAttribute(string MJMEBBCLHII)
	{
		AlignTargetAttribute kDDAFBJDOMN = AlignTargetAttributes.Find((AlignTargetAttribute DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(MJMEBBCLHII));
		return kDDAFBJDOMN != null;
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

	public static float GetFightDifficulty(FightList KGKDKENMAOA)
	{
		return 0f;
	}

	public static float CalculateAttributesAlignment(bool GHFKMONMAJG, ModelParameters DPLNMPKPOJD, ModelParameters JEHPCONLCMI, List<global::Pair<string, float>> CHLLJCFCGHO, string KLIIDDMHNOL, ref float AJEDGBBFGDF, ref float DGILKKFKPAL, ref float ILGBEJNHNMH)
	{
		float num = AlignTargetAttribute.GetValue(KLIIDDMHNOL);
		float num2 = 0f - num;
		float num3 = 0f;
		float num4 = 0f;
		int OEMALIFPGPO = 0;
		JEHPCONLCMI.FinalAttributes.Get(KLIIDDMHNOL, ref OEMALIFPGPO);
		float num5 = OEMALIFPGPO;
		float num6 = float.MinValue;
		ModelParameters kIKOGDEPGHB = ((!GHFKMONMAJG) ? DPLNMPKPOJD : JEHPCONLCMI);
		List<AttributesAlign> list = new List<AttributesAlign>();
		int num7 = AttributesAlign.AppendHighestPriority(kIKOGDEPGHB.AttributeAlignments, list);
		for (int i = 0; i < CHLLJCFCGHO.Count; i++)
		{
			string lLHEDBIEHAA = CHLLJCFCGHO[i].First;
			OEMALIFPGPO = 0;
			DPLNMPKPOJD.FinalAttributes.Get(lLHEDBIEHAA, ref OEMALIFPGPO);
			float num8 = OEMALIFPGPO;
			float num9 = num8 + CHLLJCFCGHO[i].Second;
			float num10 = AlignTargetAttribute.GetValue(lLHEDBIEHAA);
			float num11 = 0f;
			if (GHFKMONMAJG)
			{
				num11 = float.MaxValue;
				foreach (AttributesAlign item in list)
				{
					bool flag = ListSF.GetRoster().IsEclipseMode();
					bool flag2 = QuestUtils.GetDifficultyOptions().GetOverrideAllDifficulties();
					if ((flag && item.DifficultyFilter == ModelParameters.DifficultyFilter.DFHard) || (!flag && item.DifficultyFilter == ModelParameters.DifficultyFilter.DFNormal) || item.DifficultyFilter == ModelParameters.DifficultyFilter.DFBoth || flag2)
					{
						float oNMMKLDMHJD = item.Factor;
						float fJAHKFNFNCK = item.Shift;
						float num12 = (num9 - num5) * (1f - oNMMKLDMHJD) + (num10 - num) * oNMMKLDMHJD - fJAHKFNFNCK;
						if (num12 < num11)
						{
							num11 = num12;
							num3 = oNMMKLDMHJD;
							num4 = fJAHKFNFNCK;
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
						float oNMMKLDMHJD2 = item2.Factor;
						float fJAHKFNFNCK2 = item2.Shift;
						float num13 = (num9 - num5) * (1f - oNMMKLDMHJD2) + (num10 - num) * oNMMKLDMHJD2 + fJAHKFNFNCK2;
						if (num11 < num13)
						{
							num11 = num13;
							num3 = oNMMKLDMHJD2;
							num4 = fJAHKFNFNCK2;
						}
					}
				}
			}
			ModifiedAlignFormula.DamageAttribute jNLEHBPFPBN = ModifiedAlign.FindDamageAttribute(lLHEDBIEHAA);
			if (jNLEHBPFPBN != null)
			{
				float gFHOHECBODM = jNLEHBPFPBN.MinAttributeDifference;
				string kLAIAPBONFM = jNLEHBPFPBN.ApplyTo;
				bool flag5 = kLAIAPBONFM == "Player";
				if (num11 >= gFHOHECBODM && GHFKMONMAJG == flag5)
				{
					float bGJPLNFFEOB = DamageDoublingRange;
					float bPPJAMCGICA = jNLEHBPFPBN.NetDamage;
					float oPHGJJGKIHE = jNLEHBPFPBN.DamageMultiplier;
					num11 = bGJPLNFFEOB * Mathf.Log((1f - (1f - bPPJAMCGICA) * Mathf.Pow(oPHGJJGKIHE, (0f - num11) / bGJPLNFFEOB)) / bPPJAMCGICA) / Mathf.Log(2f);
				}
			}
			if (num6 < num11)
			{
				num6 = num11;
				num2 = num10 - num;
			}
		}
		AJEDGBBFGDF = num2;
		DGILKKFKPAL = num3;
		ILGBEJNHNMH = num4;
		return num6;
	}

	public static float GetAttributesHitMultiplier(bool GHFKMONMAJG, ModelParameters DPLNMPKPOJD, ModelParameters JEHPCONLCMI, List<global::Pair<string, float>> CHLLJCFCGHO, string KLIIDDMHNOL)
	{
		float AJEDGBBFGDF = 0f;
		float DGILKKFKPAL = 0f;
		float ILGBEJNHNMH = 0f;
		float eEPAABANDBL = CalculateAttributesAlignment(GHFKMONMAJG, DPLNMPKPOJD, JEHPCONLCMI, CHLLJCFCGHO, KLIIDDMHNOL, ref AJEDGBBFGDF, ref DGILKKFKPAL, ref ILGBEJNHNMH);
		return GetAttributesHitMultiplier(eEPAABANDBL);
	}

	public static float GetAttributesHitMultiplier(float EEPAABANDBL)
	{
		float bGJPLNFFEOB = DamageDoublingRange;
		return Mathf.Pow(2f, EEPAABANDBL / bGJPLNFFEOB);
	}

	public static List<ModelParameters> CreateOpponentParameters(List<ModelParameters> JCICKLIMBEF)
	{
		List<ModelParameters> list = new List<ModelParameters>();
		foreach (ModelParameters item2 in JCICKLIMBEF)
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
		ModelParameters kIKOGDEPGHB = ListSF.GetPlayerParameters();
		List<ItemInfo> list = kIKOGDEPGHB.GetEquippedItemsByType();
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			ItemInfo dJKEECEOCJB = list[i];
			if (dJKEECEOCJB != null)
			{
				UserItem dKCHDHMLKHN = nKGLHEGIKKP.GetInventory().FindItem(dJKEECEOCJB.Name);
				if (dKCHDHMLKHN != null && dKCHDHMLKHN.GetHasUpgradedItem())
				{
					kIKOGDEPGHB.SetItemByType(list[i].Type, dKCHDHMLKHN.GetCurrentUpgradeItem());
				}
			}
		}
		InitializeCombatParameters(kIKOGDEPGHB);
		kIKOGDEPGHB.UserControlled = true;
		kIKOGDEPGHB.AiControlled = false;
		kIKOGDEPGHB.IsPlayer = true;
		return kIKOGDEPGHB;
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
		private static ModelParameters InitializeCombatParameters(ModelParameters JCICKLIMBEF)
	{
		if (JCICKLIMBEF.Skeleton == null)
		{
			JCICKLIMBEF.Skeleton = ListSF.GetItems().GetItemByName(GetDefaultSkeleton());
		}
		JCICKLIMBEF.RoundsWon = 0;
		JCICKLIMBEF.IsPlayer = false;
		int num = (ObscuredInt)(JCICKLIMBEF.GetBaseHealth());
		if (num > 0)
		{
			JCICKLIMBEF.MaxLife = num;
		}
		else
		{
			JCICKLIMBEF.MaxLife = 1f;
		}
		JCICKLIMBEF.MovesInitialized = false;
		JCICKLIMBEF.IsUntouched = true;
		JCICKLIMBEF.RestoreFullLife();
		JCICKLIMBEF.MoveIds = LoadMoves(AnimationData.GetAnimationCount());
		JCICKLIMBEF.BuildModelDocuments();
		JCICKLIMBEF.CalculateAttributes();
		return JCICKLIMBEF;
	}

	public static void LockInput(bool LLOLBKJMKNC = true)
	{
		Module.GetInstance().SetGameInputLock(true, LLOLBKJMKNC);
	}

	public static void UnlockInput()
	{
		Module.GetInstance().SetGameInputLock(false);
	}

	public static void ResetScenes()
	{
		SceneManagerSF.Reset();
	}

	private static List<int> LoadMoves(int KAEPJHHLLPK = 50)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < KAEPJHHLLPK; i++)
		{
			list.Add(i);
		}
		return list;
	}

	public static FightList GetOpenFight(Battle DPOOIONCEOA)
	{
		if (Eclipse.Modding.ModModeRuntime.TryCurrent(DPOOIONCEOA, out var modeFight)) return modeFight;
		if (DPOOIONCEOA != null)
		{
			List<FightList> list = DPOOIONCEOA.GetFights();
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

	public static FightList GetLastFight(Battle DPOOIONCEOA)
	{
		if (DPOOIONCEOA != null)
		{
			List<FightList> list = DPOOIONCEOA.GetFights();
			if (list.Count > 0)
			{
				return list[list.Count - 1];
			}
		}
		return null;
	}

		public static Fight CreateFight(object data, PreFight preFight = null, GameController LPGANKOAPJL = null)
		{
			if (data is Eclipse.Multiplayer.LocalVersusMatch localMatch)
				return localMatch.CreateFight(preFight, LPGANKOAPJL);
		FightList jDIPBIHBGPF = (FightList)data;
		ModelParameters kIKOGDEPGHB = null;
		kIKOGDEPGHB = Eclipse.Modding.ModRuntime.BuildFightPlayerParameters(jDIPBIHBGPF) ?? GetPlayerModelParameters().Clone();
		List<ModelParameters> list = new List<ModelParameters>();
		BattleType pJMEMGHKKBM = jDIPBIHBGPF.get_Type();
		if (pJMEMGHKKBM == BattleType.FightPeriodic)
		{
			if (!jDIPBIHBGPF.GetRosterFight().HasRandomSeeds)
			{
				jDIPBIHBGPF.GetRosterFight().RandomizeSeeds();
			}
			NekkiMath.SetSeed(jDIPBIHBGPF.GetRosterFight().GetRandomGroupSeed());
			list = CreateOpponentParameters(jDIPBIHBGPF.GetOpponents());
			NekkiMath.SetSeed();
		}
		else
		{
			list = CreateOpponentParameters(jDIPBIHBGPF.GetOpponents());
		}
		if (list.Count == 0)
		{
			// Keep a malformed/newer fight playable and make the migration defect
			// explicit. AdaptStages normally materializes battle-level warrior pools;
			// this is the final safety net for future unsupported stage shapes.
			ModelParameters fallbackEnemy = InitializeCombatParameters(kIKOGDEPGHB.Clone());
			fallbackEnemy.IsPlayer = false;
			fallbackEnemy.Avatar = "man_fist";
			list.Add(fallbackEnemy);
			UnityEngine.Debug.LogError("[Fight] '" + jDIPBIHBGPF.Name +
				"' contained no usable enemies; using a player-equipment compatibility opponent.");
		}
		UnderworldRaidDiagnostics.LogEnemies(jDIPBIHBGPF, kIKOGDEPGHB, list);
		Eclipse.Underworld.UnderworldZonePolicy.NoteFightStarted(jDIPBIHBGPF.Battle);
		return new Fight(jDIPBIHBGPF, kIKOGDEPGHB, list, preFight, LPGANKOAPJL);
	}

	public static void OnFightPrepared(Battle DPOOIONCEOA)
	{
	}

	public static FightList GetFinalFight(FightList KGKDKENMAOA)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP.GetDiscipleMode() > 0)
		{
			Battle cNAOMDMIGLJ = KGKDKENMAOA.Battle;
			List<FightList> list = cNAOMDMIGLJ.GetFights();
			return list[list.Count - 1];
		}
		return KGKDKENMAOA;
	}

	public static bool ApplyItemAction(ItemInfo item, ItemAction LFLGCDNKNJI, int count = 1, Action<object> callback = null)
	{
		UserItem nDMCFNGEPOA = ListSF.GetUserItem(item.Name);
		return ApplyItemAction(item, nDMCFNGEPOA, LFLGCDNKNJI, count, callback);
	}

	private static bool ApplyItemAction(ItemInfo item, UserItem NDMCFNGEPOA, ItemAction LFLGCDNKNJI, int count = 1, Action<object> callback = null)
	{
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		hHKLFIIBIFF.purchaseFailureReason = string.Empty;
		ListSF.CheckItems bJEBPDNMNAE = new ListSF.CheckItems();
		if (LFLGCDNKNJI == ItemAction.Item_Buy_Gold || LFLGCDNKNJI == ItemAction.Item_Buy_Ruby || LFLGCDNKNJI == ItemAction.Item_Buy_Real || LFLGCDNKNJI == ItemAction.Item_Upgrade_Gold || LFLGCDNKNJI == ItemAction.Item_Upgrade_Ruby || LFLGCDNKNJI == ItemAction.Item_Delivery_Ruby || LFLGCDNKNJI == ItemAction.Item_Recipe || LFLGCDNKNJI == ItemAction.Item_Recipe_Delivery_Ruby || LFLGCDNKNJI == ItemAction.Item_Consumable)
		{
			bJEBPDNMNAE = ListSF.CheckItemPurchase(item, LFLGCDNKNJI, count);
			if (bJEBPDNMNAE.Value == -1)
			{
				if (LFLGCDNKNJI == ItemAction.Item_Recipe)
				{
					NotifyEnchantmentUnsuccessful(item, bJEBPDNMNAE.Type);
				}
				else
				{
					NotifyPurchaseUnsuccessful(item, bJEBPDNMNAE.Type);
				}
				return false;
			}
		}
		if ((item.DeliveryTime == 0 && (LFLGCDNKNJI == ItemAction.Item_Buy_Gold || LFLGCDNKNJI == ItemAction.Item_Upgrade_Gold)) || (NDMCFNGEPOA != null && NDMCFNGEPOA.GetIsOwned()) || LFLGCDNKNJI == ItemAction.Item_Buy_Ruby || LFLGCDNKNJI == ItemAction.Item_Equip || LFLGCDNKNJI == ItemAction.Item_Upgrade_Ruby || LFLGCDNKNJI == ItemAction.Item_Delivery_Ruby)
		{
			ListSF.UnequipOtherItemsOfType(item);
		}
		if (NDMCFNGEPOA != null && NDMCFNGEPOA.GetIsEquipped() && LFLGCDNKNJI == ItemAction.Item_Unequip)
		{
			ListSF.GetInstance().EquipDefaultItem(item);
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool result = false;
		switch (LFLGCDNKNJI)
		{
		case ItemAction.Item_Buy_Gold:
		case ItemAction.Item_Buy_Ruby:
		case ItemAction.Item_Buy_Real:
		case ItemAction.Item_Upgrade_Gold:
		case ItemAction.Item_Upgrade_Ruby:
		case ItemAction.Item_Delivery_Ruby:
		case ItemAction.Item_Free:
		case ItemAction.Item_Consumable:
			result = ListSF.BuyItem(item, LFLGCDNKNJI, bJEBPDNMNAE.Value, count, callback);
			break;
		case ItemAction.Item_Recipe_Delivery_Ruby:
			flag2 = true;
			flag3 = true;
			result = ListSF.BuyItem(item, LFLGCDNKNJI, bJEBPDNMNAE.Value, count, callback);
			break;
		case ItemAction.Item_Equip:
			result = ListSF.UseItem(NDMCFNGEPOA, true);
			break;
		case ItemAction.Item_Unequip:
			result = ListSF.UseItem(NDMCFNGEPOA, false);
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
			RecipeItemInfo mBIJKDIEFIF2 = (RecipeItemInfo)item;
			ScheduleEnchantmentNotification("enchantment_notification", mBIJKDIEFIF2);
			break;
		}
		case ItemAction.Item_Recipe_Delivery_End:
		{
			flag2 = true;
			flag3 = true;
			RecipeItemInfo mBIJKDIEFIF = (RecipeItemInfo)item;
			CancelEnchantmentNotification("enchantment_notification", mBIJKDIEFIF);
			break;
		}
		}
		UserItem dKCHDHMLKHN = ListSF.GetUserItem(item.Name);
		if (flag3)
		{
		}
		if (dKCHDHMLKHN != null && dKCHDHMLKHN.GetDeliveryTimestamp() > 0)
		{
			ScheduleItemNotification("item_notification", dKCHDHMLKHN);
		}
		else if (dKCHDHMLKHN != null && dKCHDHMLKHN.GetDeliveryTimestamp() == 0)
		{
			CancelItemNotification("item_notification", dKCHDHMLKHN);
		}
		return result;
	}

	public static void NotifyPurchaseUnsuccessful(ItemInfo item, ListSF.CheckItemType DDEDNPLHOJH)
	{
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		hHKLFIIBIFF.purchasedItem = item;
		switch (DDEDNPLHOJH)
		{
		case ListSF.CheckItemType.CHECK_ITEM_MONEY:
			hHKLFIIBIFF.purchaseFailureReason = "Coins";
			break;
		case ListSF.CheckItemType.CHECK_ITEM_BONUS:
			hHKLFIIBIFF.purchaseFailureReason = "Ruby";
			break;
		case ListSF.CheckItemType.CHECK_ITEM_NO_NETWORK:
			hHKLFIIBIFF.purchaseFailureReason = "Connection";
			break;
		}
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE_UNSUCCESSFUL))
		{
			ListSF.GetInstance().RunQuestActions();
		}
		TrackEvent("Insufficient Currency");
	}

	private static void NotifyEnchantmentUnsuccessful(ItemInfo item, ListSF.CheckItemType DDEDNPLHOJH)
	{
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		RecipeItemInfo bNJOCBKNPMG = (RecipeItemInfo)item;
		hHKLFIIBIFF.enchantment.itemName = bNJOCBKNPMG.GetUserItem().get_Name();
		hHKLFIIBIFF.enchantment.recipeName = bNJOCBKNPMG.GetRecipe().Name;
		long num = ListSF.GetServerTime();
		long bMNFPNBAMAF = num + bNJOCBKNPMG.GetPrice().DeliveryTimeSeconds;
		hHKLFIIBIFF.enchantment.endTimestamp = bMNFPNBAMAF;
		hHKLFIIBIFF.enchantment.costType = "Materials";
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_ENCHANTMENT_UNSUCCESSFUL))
		{
			ListSF.GetInstance().RunQuestActions();
		}
	}

	public static bool NotifyTabChanged(SliderType OFEMKBGPNBH, SliderType CFDMHKKBGIN)
	{
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		hHKLFIIBIFF.tabFrom = SliderNames[OFEMKBGPNBH];
		hHKLFIIBIFF.tabTo = SliderNames[CFDMHKKBGIN];
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_CHANGE_TAB))
		{
			ListSF.GetInstance().RunQuestActions();
			return true;
		}
		return false;
	}

	public static bool NotifyShopOpened(ScreenType JPDNPODKKJP)
	{
		if (JPDNPODKKJP == ScreenType.ModuleShop &&
			ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_SHOP_BUTTON_PRESS))
		{
			ListSF.GetInstance().RunQuestActions();
			// The migrated quest graph observes this event but does not always issue
			// its own scene transition.  Continue the user's requested navigation.
			return false;
		}
		if (JPDNPODKKJP == ScreenType.ModuleShop && ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_SHOP_ENTER))
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

	public static uint GetFightExpReward(FightList KGKDKENMAOA, bool FFIBGBMOMPD)
	{
		FightResult nHIDAJFLHJN = new FightResult();
		nHIDAJFLHJN.FightType = KGKDKENMAOA.get_Type();
		nHIDAJFLHJN.FightId = KGKDKENMAOA.FightId;
		int num = KGKDKENMAOA.RewardIndex;
		if (FFIBGBMOMPD)
		{
			num++;
		}
		if (KGKDKENMAOA.HasMultipleOpponentsAndRounds())
		{
			num = (FFIBGBMOMPD ? 1 : 0);
		}
		RewardStruct lGDIIADDFLH = KGKDKENMAOA.GetRewardAt(num);
		nHIDAJFLHJN.CalculateRewards(lGDIIADDFLH, null, null, KGKDKENMAOA);
		return nHIDAJFLHJN.Prize.exp;
	}

	public static void HandleSurrender(FightResult HEIADONEACH)
	{
		IsFightTransitioning = true;
		IsFightTransitioning = false;
		Module.OpenScreen(ScreenType.ModuleMap);
	}

	public static void EndFight(ComboStatistic AIOMDIAFHGB, FightList KGKDKENMAOA, ModelParameters ABKBEJBICOA, ModelParameters LEBLJJCFKOP, GameOverTypes MHNEKAEGNBO, ComboStatistic MOJHPBGGNAH = null, float OEIFGEHDHLE = 0f, int JEGCHOOLDLB = 0, float PIFMOMMPFFM = 0f, int JEDPGMGCJGA = 0)
	{
		bool flag = LEBLJJCFKOP == null && MOJHPBGGNAH == null;
		bool flag2 = KGKDKENMAOA == null || MHNEKAEGNBO == GameOverTypes.GAME_OVER_SURRENDER;
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		KGKDKENMAOA = ((KGKDKENMAOA == null) ? ListSF.GetFightById(nKGLHEGIKKP.GetFightIds()) : KGKDKENMAOA);
		if (KGKDKENMAOA == null)
		{
			GameLog.Error("0 == fightList");
		}
		if (!Eclipse.Modding.ModModeRuntime.CanResolve(KGKDKENMAOA)) return;
		var eclipseStoryEncounter = KGKDKENMAOA.EclipseStoryEncounter;
		if (!Eclipse.Modding.ModRuntime.StoryEvents.TryBeginEncounterResult(eclipseStoryEncounter)) eclipseStoryEncounter = null;
		var eclipseStoryResult = eclipseStoryEncounter == null ? null : Eclipse.Modding.ModRuntime.CaptureBattleResult(nKGLHEGIKKP, KGKDKENMAOA, MHNEKAEGNBO, ABKBEJBICOA, LEBLJJCFKOP);
		FightResult nHIDAJFLHJN = new FightResult();
		nHIDAJFLHJN.FightDefinition = KGKDKENMAOA;
		nHIDAJFLHJN.FightType = KGKDKENMAOA.get_Type();
		nHIDAJFLHJN.FightId = new FightIDS(KGKDKENMAOA.FightId);
		nHIDAJFLHJN.PlayerParameters = ABKBEJBICOA;
		nHIDAJFLHJN.OpponentParameters = LEBLJJCFKOP;
		nHIDAJFLHJN.GameOverType = MHNEKAEGNBO;
		int num = KGKDKENMAOA.RewardIndex;
		if (nHIDAJFLHJN.IsWinner())
		{
			num++;
		}
		if (KGKDKENMAOA.HasMultipleOpponentsAndRounds())
		{
			num = (nHIDAJFLHJN.IsWinner() ? 1 : 0);
		}
		RewardStruct lGDIIADDFLH = KGKDKENMAOA.GetRewardAt(num);
		nHIDAJFLHJN.CalculateRewards(lGDIIADDFLH, AIOMDIAFHGB, MOJHPBGGNAH, KGKDKENMAOA);
		if (!flag)
		{
			ArgsDict kEMMIFBFDPK = new ArgsDict();
			kEMMIFBFDPK.Add("fightResult", nHIDAJFLHJN);
			kEMMIFBFDPK.Add("isSurrender", flag2);
			kEMMIFBFDPK.Add("avgFps", OEIFGEHDHLE);
			kEMMIFBFDPK.Add("fightList", KGKDKENMAOA);
			kEMMIFBFDPK.Add("fightTimeElapsed", PIFMOMMPFFM);
			StatisticsCollector.LogEvent(StatisticsEvent.EventType.Fight_End, kEMMIFBFDPK);
		}
		bool flag3 = false;
		if (!flag2)
		{
			KGKDKENMAOA.Battle.UpdateRosterFight(KGKDKENMAOA, nHIDAJFLHJN.IsWinner());
			if (KGKDKENMAOA.get_Type() == BattleType.FightPeriodic)
			{
				if (nHIDAJFLHJN.IsWinner())
				{
					KGKDKENMAOA.Battle.UpdateByTime(ListSF.GetCurrentTime());
					KGKDKENMAOA.GetRosterFight().RandomizeSeeds();
				}
				SchedulePeriodicFightNotification(KGKDKENMAOA, nHIDAJFLHJN.IsWinner());
			}
			BattleType pJMEMGHKKBM = KGKDKENMAOA.get_Type();
			if (pJMEMGHKKBM == BattleType.FightReplayable || pJMEMGHKKBM == BattleType.FightBossesReplayable || pJMEMGHKKBM == BattleType.FightFinalReplayable)
			{
				BattleReplayable bKKPCBGAEHC = (BattleReplayable)KGKDKENMAOA.Battle;
				if (bKKPCBGAEHC.GetStatus() == ConditionStatus.StatusComplete)
				{
					ListSF.RegisterReplayableBattle(bKKPCBGAEHC);
				}
			}
			flag3 = ListSF.GetInstance().ApplyFightRewards(nHIDAJFLHJN);
		}
		Eclipse.Modding.ModModeRuntime.Complete(KGKDKENMAOA, !flag2 && nHIDAJFLHJN.IsWinner());
        if (Eclipse.Modding.ModModeRuntime.IsRaid(KGKDKENMAOA)) Eclipse.Modding.ModModeRuntime.SetRaidResult(nHIDAJFLHJN);
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		if (KGKDKENMAOA.get_Type() == BattleType.FightAscension)
		{
			Battle cGJCGEBPCAF = ListSF.GetBattleById(KGKDKENMAOA.FightId);
			BattleAscension bGFLODNGLPK = (BattleAscension)cGJCGEBPCAF;
			if (flag2 || !nHIDAJFLHJN.IsWinner())
			{
				bGFLODNGLPK.SetAscensionLevel(1);
				ListSF.RegisterReplayableBattle(bGFLODNGLPK);
				hHKLFIIBIFF.fightIds = KGKDKENMAOA.FightId;
				if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_RESET_ASCENSION))
				{
					ListSF.GetInstance().RunQuestActions();
				}
			}
			else
			{
				bGFLODNGLPK.AdvanceAscensionAfterFight(KGKDKENMAOA);
				if (bGFLODNGLPK.GetRosterBattle() != null && bGFLODNGLPK.GetRosterBattle().GetAscensionLevel() > bGFLODNGLPK.GetFightCount())
				{
					bGFLODNGLPK.SetAscensionLevel(1);
					ListSF.RegisterReplayableBattle(bGFLODNGLPK);
				}
			}
			bGFLODNGLPK.RefreshAllFightStatuses();
		}
		hHKLFIIBIFF.fightIds = KGKDKENMAOA.FightId;
		hHKLFIIBIFF.fightResult = (nHIDAJFLHJN.IsWinner() ? "Win" : ((!flag2) ? "Loss" : "Surrender"));
		hHKLFIIBIFF.levelUp = (flag3 ? 1 : 0);
		hHKLFIIBIFF.fightAvgFps = OEIFGEHDHLE;
		if (KGKDKENMAOA.get_Type() == BattleType.FightRaid)
		{
			hHKLFIIBIFF.raidId = KGKDKENMAOA.FightId.GetBattle();
		}
        Eclipse.Modding.ModModeRuntime.NotifyResult(KGKDKENMAOA);
		// Use this result's already composed reward. Scanning all reward scopes
		// can defer a normal win because another win count contains a lottery,
		// and re-evaluates reward expressions after progression/level-up changes.
		bool flag4 = !flag2 && nHIDAJFLHJN.Prize.Lottery != null;
		if (flag4 && nHIDAJFLHJN.IsWinner())
		{
			ListSF.GetInstance().LotteryQuestParameters = hHKLFIIBIFF;
			Eclipse.Modding.ModRuntime.PrepareBattleLottery(nHIDAJFLHJN.Prize.Lottery, hHKLFIIBIFF,
				KGKDKENMAOA.get_Type() == BattleType.FightRaid, KGKDKENMAOA.EclipseStoryEncounter?.Id);
		}
		if ((!flag4 || !nHIDAJFLHJN.IsWinner()) && ((KGKDKENMAOA.get_Type() != BattleType.FightRaid && ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_FIGHT_END)) || (KGKDKENMAOA.get_Type() == BattleType.FightRaid && ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_FIGHT_END))) && flag)
		{
			ListSF.GetInstance().RunQuestActions();
		}
		ListSF.GetInstance().RequestSave();
		MenuController.RefreshMoney();
		if (CanShowRatePrompt(KGKDKENMAOA.FightId.ToString()) && nHIDAJFLHJN.IsWinner() && (KGKDKENMAOA.get_Type() == BattleType.FightBosses || KGKDKENMAOA.get_Type() == BattleType.FightFinalTitan || KGKDKENMAOA.get_Type() == BattleType.FightBossesReplayable || KGKDKENMAOA.get_Type() == BattleType.FightTournament || KGKDKENMAOA.get_Type() == BattleType.FightAscension))
		{
			ShowRatePrompt = true;
		}
		MenuController.RefreshMenu();
		if (!flag)
		{
			if (KGKDKENMAOA.get_Type() == BattleType.FightRaid && !Eclipse.Modding.ModModeRuntime.IsRaid(KGKDKENMAOA))
			{
				int num2 = 0;
				ModelParameters kIKOGDEPGHB = ((!ABKBEJBICOA.IsPlayer) ? ABKBEJBICOA : LEBLJJCFKOP);
				float num3 = (ObscuredFloat)(kIKOGDEPGHB.GetCurrentLife());
				float num4 = (float)num2 - num3;
				float num5 = 0f;
				if (num5 > num4)
				{
					num5 = num4;
				}
				float num6 = num4 - num5;
				if (nHIDAJFLHJN.IsWinner() || num6 > (float)num2 - num5)
				{
					num6 = (float)num2 - num5;
				}
				RaidModelParameters kAOPLEPILDH = ((!ABKBEJBICOA.IsPlayer) ? LEBLJJCFKOP : ABKBEJBICOA) as RaidModelParameters;
				if (JEGCHOOLDLB > 0 && kAOPLEPILDH == null)
				{
				}
			}
			else if (flag2)
			{
				HandleSurrender(nHIDAJFLHJN);
			}
			else if (Fight.GetCurrentFight() != null)
			{
				if (Eclipse.Modding.ModModeRuntime.IsRaid(KGKDKENMAOA)) Eclipse.Modding.ModModeRuntime.ShowRaidResult();
                else Fight.GetCurrentFight().ShowEndFightScreen(nHIDAJFLHJN);
			}
		}
		if (nHIDAJFLHJN.IsWinner())
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

	public static bool StartFight(FightList KGKDKENMAOA = null, bool FLLKCPMJOEL = false, Battle DPOOIONCEOA = null, bool CDFICPGIBEE = true, bool IINNCMDDLGE = true)
	{
		if (KGKDKENMAOA == null)
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
        var requestedFight = KGKDKENMAOA;
        if (!Eclipse.Modding.ModModeRuntime.PrepareEntry(requestedFight, () => StartFight(requestedFight,FLLKCPMJOEL,DPOOIONCEOA,CDFICPGIBEE,IINNCMDDLGE))) return false;
		if (!Eclipse.Modding.ModModeRuntime.ResolveEntry(ref KGKDKENMAOA)) return false;
        var storyFight = KGKDKENMAOA;
        var storyEntry = Eclipse.Modding.ModRuntime.TryStoryFightEntry(storyFight,
            () => StartFight(storyFight, FLLKCPMJOEL, DPOOIONCEOA, CDFICPGIBEE, IINNCMDDLGE));
        if (storyEntry.HasValue) return storyEntry.Value;
		ScreenType iPKNDMINFMJ = Module.GetInstance().GetCurrentScreenType();
		FightList jDIPBIHBGPF = null;
		if (iPKNDMINFMJ == ScreenType.ModuleDojo || iPKNDMINFMJ == ScreenType.ModuleFight)
		{
			jDIPBIHBGPF = FightHolder.fightList;
		}
		if (iPKNDMINFMJ == ScreenType.ModuleFight && KGKDKENMAOA == jDIPBIHBGPF)
		{
			return false;
		}
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		RosterFight pIGKOIFBOME = nKGLHEGIKKP.FindSavedFightRecord(KGKDKENMAOA.FightId);
		if (pIGKOIFBOME == null)
		{
			pIGKOIFBOME = nKGLHEGIKKP.CreateFight(KGKDKENMAOA.FightId);
		}
		KGKDKENMAOA.SetRosterFight(pIGKOIFBOME);
		Battle cNAOMDMIGLJ = KGKDKENMAOA.Battle;
		if (cNAOMDMIGLJ.get_Type() != BattleType.FightUnregister)
		{
			ListSF.GetRoster().SetFightIds(KGKDKENMAOA.FightId);
		}
		if (IINNCMDDLGE)
		{
			QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
			hHKLFIIBIFF.fightIds = KGKDKENMAOA.FightId;
			hHKLFIIBIFF.fightResult = string.Empty;
			hHKLFIIBIFF.raidResult = string.Empty;
			if (KGKDKENMAOA.get_Type() == BattleType.FightRaid)
			{
				hHKLFIIBIFF.raidId = KGKDKENMAOA.FightId.GetBattle();
			}
			if ((KGKDKENMAOA.get_Type() != BattleType.FightRaid && ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_FIGHT_ENTER)) || (KGKDKENMAOA.get_Type() == BattleType.FightRaid && ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_FIGHT_ENTER)))
			{
				ListSF.GetInstance().RunQuestActions();
				return true;
			}
		}
		if (!Eclipse.Modding.ModModeRuntime.Begin(KGKDKENMAOA)) return false;
		var eclipseStoryEncounter = Eclipse.Modding.ModRuntime.StoryEvents.BeginEncounter();
		KGKDKENMAOA.EclipseStoryEncounter = eclipseStoryEncounter;
		if (KGKDKENMAOA.get_Type() != BattleType.FightPeriodic)
		{
			KGKDKENMAOA.GetRosterFight().SetCompletionTimestamp(ListSF.GetServerTime());
		}
		if (KGKDKENMAOA.get_Type() == BattleType.FightAscension)
		{
		}
		ListSF.GetInstance().RequestSave();
		if (CDFICPGIBEE)
		{
			Sound.PlaySound("snd_gong");
			IsFightTransitioning = false;
		}
		MapScene current = Scene<MapScene>.get_Current();
		if (!AutoWinPending || (!(AutoWinFightKey == KGKDKENMAOA.Location + KGKDKENMAOA.Battle.get_Name() + KGKDKENMAOA.Name) && !(KGKDKENMAOA.FightId.GetBattle() == "QuestBattle")))
		{
			if (current != null)
			{
				current.EnableMapButtons(false);
			}
			if (!Module.OpenScreen(ScreenType.ModuleFight, KGKDKENMAOA))
			{
                if (current != null) current.EnableMapButtons(true);
                Eclipse.Modding.ModModeRuntime.CancelLaunch(KGKDKENMAOA);
				Eclipse.Modding.ModRuntime.StoryEvents.CancelEncounter(eclipseStoryEncounter);
                return false;
			}
		}
		else
		{
			AutoWinPending = false;
			RosterFight pIGKOIFBOME2 = nKGLHEGIKKP.FindSavedFightRecord(KGKDKENMAOA.FightId);
			if (pIGKOIFBOME2 == null)
			{
				pIGKOIFBOME2 = nKGLHEGIKKP.CreateFight(KGKDKENMAOA.FightId);
			}
			KGKDKENMAOA.SetRosterFight(pIGKOIFBOME2);
			int num = KGKDKENMAOA.GetRewards().Count - 2;
			if (num < 0)
			{
				num = 0;
			}
			KGKDKENMAOA.RewardIndex = num;
			ComboStatistic aIOMDIAFHGB = new ComboStatistic();
			EndFight(aIOMDIAFHGB, KGKDKENMAOA, ListSF.GetRoster().get_Parameters(), null, GameOverTypes.GAME_OVER_WIN);
			if (current != null)
			{
				current.GetInfoBattle().UpdateBattleInfo(current.GetInfoBattle().GetCurrentBattle());
			}
		}
		FightHolder.fightList = KGKDKENMAOA;
        Eclipse.Modding.ModModeRuntime.NotifyEntry(KGKDKENMAOA);
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

	public static void ParseDefaultItems(XmlNode AFHNINCKJEE)
	{
		defaultItems.Clear();
		foreach (XmlNode childNode in AFHNINCKJEE.ChildNodes)
		{
			if ("Item" == childNode.Name)
			{
				string gBCLEDJAOBM = childNode.Attributes["Type"].GetStringOrDefault(string.Empty);
				string pOFHDGJAFMP = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
				defaultItems.Add(new global::Pair<string, string>(gBCLEDJAOBM, pOFHDGJAFMP));
			}
		}
	}

	public static string GetDefaultItem(string CNKBLODAFDO)
	{
		foreach (global::Pair<string, string> item in defaultItems)
		{
			if (item.First == CNKBLODAFDO)
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

	public static void ShowEnterScreen(string HCPNFPMHFCM, Action FLHKCGAMDJJ)
	{
		EnterScreen enterScreen = EnterScreen.Create();
		enterScreen.Init(HCPNFPMHFCM, FLHKCGAMDJJ);
	}

	public static void ShowEnterScreen(List<KeyValuePair<string, int>> IGLEKOAILHD, Action FLHKCGAMDJJ)
	{
		EnterScreen enterScreen = EnterScreen.Create();
		enterScreen.Init(IGLEKOAILHD, FLHKCGAMDJJ);
	}

	public static Trick GetTrickByName(string name)
	{
		return GetPlayerTricks().Find((Trick GNAONAPDDLD) => GNAONAPDDLD.Name == name);
	}

	public static List<Trick> GetPlayerTricks(SceneTypes MHOCFOODLLL = SceneTypes.SceneFight)
	{
		List<Trick> list = new List<Trick>();
		List<ItemInfo> hELFDCAIJNE = GetPlayerModelParameters().GetEquippedItems();
		List<PerkInfoItem> jOGBKOJCINM = GetPlayerModelParameters().GetAllPerks();
		bool aBGINCCBACK = false;
		AnimationData.CollectAvailableTricks(list, hELFDCAIJNE, aBGINCCBACK, null, jOGBKOJCINM, MHOCFOODLLL);
		return list;
	}

	private static bool CanShowRatePrompt(string DIAIIPCBMFL)
	{
		int count = _notShowRateFights.Count;
		for (int i = 0; i < count; i++)
		{
			if (_notShowRateFights[i] == DIAIIPCBMFL)
			{
				return false;
			}
		}
		return true;
	}

	public static void TrackItemAction(ItemInfo item, ItemAction LFLGCDNKNJI, int count, long BMNFPNBAMAF)
	{
	}

	public static void TrackDelivery(string CPMFDAEAHAM)
	{
	}

	private static void ScheduleEnchantmentNotification(string LOKLDPLAPOL, RecipeItemInfo item)
	{
		if (item != null && item.GetUserItem() != null)
		{
			string text = item.GetUserItem().get_Name();
			string lIOGIBJBHAH = LocalizationManager.GetString(LOKLDPLAPOL, "1", text);
			long num = item.GetDeliveryEndTime() - GetCurrentTime();
			if (num > 0)
			{
				LocalNotificationManager.GetInstance().ScheduleRecipe(lIOGIBJBHAH, num);
			}
		}
	}

	private static void CancelEnchantmentNotification(string LOKLDPLAPOL, RecipeItemInfo item)
	{
		if (item != null && item.GetUserItem() != null)
		{
			LocalNotificationManager.GetInstance().CancelRecipe();
		}
	}

	private static void ScheduleItemNotification(string LOKLDPLAPOL, UserItem item)
	{
		if (item != null)
		{
			string text = LocalizationManager.GetString(item.get_Name());
			string lIOGIBJBHAH = LocalizationManager.GetString(LOKLDPLAPOL, "1", text);
			long num = item.GetDeliveryTimestamp() - GetCurrentTime();
			if (num > 0)
			{
				LocalNotificationManager.GetInstance().ScheduleItem(lIOGIBJBHAH, num);
			}
		}
	}

	private static void CancelItemNotification(string LOKLDPLAPOL, UserItem item)
	{
		if (item != null)
		{
			LocalNotificationManager.GetInstance().CancelItem();
		}
	}

	private static void SchedulePeriodicFightNotification(FightList KGKDKENMAOA, bool EEEJJGMOKDF)
	{
		if (KGKDKENMAOA.get_Type() == BattleType.FightPeriodic)
		{
			RosterFight pIGKOIFBOME = KGKDKENMAOA.GetRosterFight();
			long iHDMLLNEGIK = ((!EEEJJGMOKDF) ? (pIGKOIFBOME.GetRandomizeTimestamp() + KGKDKENMAOA.RepeatTime - ListSF.GetCurrentTime()) : (pIGKOIFBOME.GetCompletionTimestamp() + KGKDKENMAOA.RepeatTime - ListSF.GetCurrentTime()));
			LocalNotificationManager.GetInstance().CancelPeriodic();
			LocalNotificationManager.GetInstance().SchedulePeriodic(iHDMLLNEGIK);
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

	public static SliderType GetSliderTypeByName(string LIKHBAFOEND)
	{
		foreach (KeyValuePair<SliderType, string> item in SliderNames)
		{
			if (item.Value == LIKHBAFOEND)
			{
				return item.Key;
			}
		}
		return SliderType.SliderNone;
	}

	public static float CalculateFightDifficulty(FightList fight)
	{
		List<ModelParameters> list = CreateOpponentParameters(fight.GetOpponents());
		ModelParameters kIKOGDEPGHB = Eclipse.Modding.ModRuntime.BuildFightPlayerParameters(fight) ?? GetPlayerModelParameters().Clone();
		kIKOGDEPGHB.SetItemsFromRules(fight.GetItemRules(), true);
		kIKOGDEPGHB.CalculateAttributes();
		float result = fight.CalculateDifficultyVsLastOpponent(kIKOGDEPGHB, list);
		list.Clear();
		return result;
	}

	public static int ParseAspects(XmlNode AFHNINCKJEE)
	{
		aspects.Clear();
		int num = 0;
		foreach (XmlNode childNode in AFHNINCKJEE.ChildNodes)
		{
			if (childNode.Name.Equals("Aspect"))
			{
				Aspect hOHAPDGFMHL = new Aspect();
				hOHAPDGFMHL.Parse(childNode);
				aspects.Add(hOHAPDGFMHL);
				num++;
			}
		}
		return num;
	}

	public static void ParseAspectConstants(XmlNode AFHNINCKJEE)
	{
		GetAspectConstants().Antilimit = AFHNINCKJEE.Attributes["Antilimit"].ParseFloat();
		GetAspectConstants().DoublingRange = AFHNINCKJEE.Attributes["DoublingRange"].ParseFloat();
		GetAspectConstants().Limit = AFHNINCKJEE.Attributes["Limit"].ParseFloat();
	}

	public static void RegisterAspectAttributes()
	{
		foreach (Aspect item in GetAspects())
		{
			WarriorAttribute bCNOAOPGAEI = new WarriorAttribute();
			bCNOAOPGAEI.set_Name(item.get_Name());
			bCNOAOPGAEI.IsHidden = true;
			bCNOAOPGAEI.IsShopHidden = true;
			bCNOAOPGAEI.IsProfileHidden = true;
			bCNOAOPGAEI.UnusedFlag = true;
			WarriorAttributeList.AttributeList.Add(bCNOAOPGAEI);
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

	public static RaidTutorialStepCode GetRaidTutorialStepByName(string NOFMEBBEDKK)
	{
		foreach (KeyValuePair<RaidTutorialStepCode, string> item in RaidTutorialStepNames)
		{
			if (item.Value == NOFMEBBEDKK)
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

	public static long GetDenominatedValue(long value, int NPFOBKBJAOB = 0)
	{
		int num = ListSF.GetRoster().GetDenominationDigits();
		float f = (float)value / Mathf.Pow(10f, num - NPFOBKBJAOB);
		return (long)Mathf.Ceil(f);
	}

	public static bool HasEnoughCurrencyForFight(FightList KGKDKENMAOA)
	{
		List<CurrencyCostRule> list = KGKDKENMAOA.GetCurrencyCostRules();
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

	public static void TrackEvent(string DOPHKKGNAEF)
	{
	}

	public static bool IsPerkCompatibleWithEquipment(PerkInfoItem AEFFHJGMNFI)
	{
		List<UserItem> list = ListSF.GetRoster().GetInventory().GetEquippedItems();
		for (int i = 0; i < list.Count; i++)
		{
			if (IsEquipmentItem(list[i].GetInfo()) && !list[i].HasEnchantment(AEFFHJGMNFI))
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
