using System;
using System.Collections.Generic;
using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class Roster : SavedXmlProfile
{
	public enum BalanceType
	{
		BALANCE_NONE = 0,
		BALANCE_BONUS = 1,
		BALANCE_BONUS_PAID = 2,
		BALANCE_MONEY = 3,
		BALANCE_MONEY_PAID = 4
	}

	public enum BalanceChangeType
	{
		CHANGE_INIT = 0,
		CHANGE_PAYMENT = 1,
		CHANGE_ACHIEVEMENT = 2,
		CHANGE_FIGHT_REWARD = 3,
		CHANGE_VIDEO_VIEW = 4,
		CHANGE_FACEBOOK = 5,
		CHANGE_QUEST = 6,
		CHANGE_CHEAT = 7,
		CHANGE_SERVER_GIVE = 8,
		CHANGE_OFFER = 9,
		CHANGE_BUY_ITEM = 10,
		CHANGE_BUY_DELIVERY = 11,
		CHANGE_RAIDS_REWARD = 12,
		CHANGE_LEDGER = 13
	}

	public enum DeliveryKind
	{
		DELIVERY_ITEM = 0,
		DELIVERY_RECIPE = 1,
		ADD_CURRENCY = 2,
		ADD_RESISTANCE = 3,
		ECLIPSE_MODE_TOGGLE = 4
	}

	private string musicPath = SF2Paths.GetGameDataPath() + "/music/";

	private string soundsPath = SF2Paths.GetGameDataPath() + "/sounds/";

	public const int DefaultLevel = 1;

	public const int DumpIntervalSeconds = 900;

	private XmlNode itemsNode;

	private UserItems userItems = new UserItems();

	private UserAchievements userAchievements = new UserAchievements();

	private UserTutorials userTutorials = new UserTutorials();

	private UserPerks userPerks;

	private XmlNode fightsNode;

	private XmlNode shopNode;

	private XmlNode battlesNode;

	private XmlNode questsNode;

	private XmlNode variablesNode;

	private XmlNode openTricksNode;

	private XmlNode timersNode;

	private XmlNode currenciesNode;

	private XmlNode paymentsNode;

	private List<string> loadedQuestFiles = new List<string>();

	private List<RosterBattle> battles = new List<RosterBattle>();

	private List<RosterFight> fights = new List<RosterFight>();

	private List<RosterQuest> quests = new List<RosterQuest>();

	private List<string> openTricks = new List<string>();

	private List<string> shopLocks = new List<string>();

	private List<CurrencyStruct> currencies = new List<CurrencyStruct>();

	private List<ResistanceStruct> resistances = new List<ResistanceStruct>();

	private static PaymentOrdersManager paymentOrders;

	private ObscuredLong money;

	private ObscuredLong bonus;

	private ObscuredLong paidMoney = (ObscuredLong)(0L);

	private ObscuredLong paidBonus = (ObscuredLong)(0L);

	private uint _indexSlider;

	private ObscuredUInt _experience;

	private ObscuredInt _power;

	public int PowerMax;

	public int PowerRegenSeconds;

	private long powerSyncTime;

	private long energyRefillTimer = -1L;

	// Energy is disabled for every profile, including existing depleted saves.
	// Keep the legacy member writable for recovered item/quest callers.
	public bool HasUnlimitedEnergy
	{
		get { return true; }
		set { }
	}

	private long lastDumpTime;

	private long periodicPlayTime;

	private long lastDailyTimeOffset;

	private long lastEnergyTimeOffset;

	private bool showUpgrades;

	private bool showForge;

	private int discipleMode;

	private int denominationDigits;

	private FightIDS fightIds;

	private Color _MapMaskColor = Color.white;

	public bool UseLevelOverride;

	public int LevelOverride;

	protected ModelParameters modelParameters;

	protected string language = string.Empty;

	protected string coinIcon = string.Empty;

	protected XmlAttribute mapFocusAttribute;

	protected FightIDS mapFocus = new FightIDS();

	protected XmlAttribute raidMapFocusAttribute;

	protected FightIDS raidMapFocus = new FightIDS();

	private bool facebookLiked;

	private long starterPackTimerEndTime;

	private RosterTimerContainer timerContainer;

	private bool eclipseMode;

	public Dictionary<string, RosterQuest.QuestVariable> QuestVariables = new Dictionary<string, RosterQuest.QuestVariable>();

	private bool trySocialLogin;

	private bool askedForDumps;

	private bool gplusAutoLogin;

	private List<ObscuredUInt> _levelThresholds = new List<ObscuredUInt>();

	private int gplusFailedLogins;

	private string currentZone = string.Empty;

	private bool raidRemindRandomRule;

	public XmlNode ItemsNode
	{
		get
		{
			return GetItemsNode();
		}
	}

	public UserItems Inventory
	{
		get
		{
			return GetInventory();
		}
	}

	public UserAchievements Achievements
	{
		get
		{
			return GetAchievements();
		}
	}

	public UserTutorials Tutorials
	{
		get
		{
			return GetTutorials();
		}
	}

	public UserPerks Perks
	{
		get
		{
			return GetPerks();
		}
	}

	public List<RosterBattle> SavedBattles
	{
		get
		{
			return GetSavedBattles();
		}
	}

	public List<RosterFight> SavedFights
	{
		get
		{
			return GetSavedFights();
		}
	}

	public List<RosterQuest> Quests
	{
		get
		{
			return GetQuests();
		}
	}

	public List<string> OpenTricks
	{
		get
		{
			return GetOpenTricks();
		}
	}

	public static PaymentOrdersManager PaymentOrders
	{
		get
		{
			return GetPaymentOrders();
		}
	}

	public long Money
	{
		get
		{
			return GetMoney();
		}
	}

	public long Bonus
	{
		get
		{
			return GetBonus();
		}
	}

	public ObscuredLong PaidMoney
	{
		get
		{
			return GetPaidMoney();
		}
		set
		{
			SetPaidMoney(value);
		}
	}

	public ObscuredLong PaidBonus
	{
		get
		{
			return GetPaidBonus();
		}
		set
		{
			SetPaidBonus(value);
		}
	}

	public uint SliderIndex
	{
		get
		{
			return GetSliderIndex();
		}
		set
		{
			set_IndexSlider(value);
		}
	}

	public uint ExperienceToNextLevel
	{
		get
		{
			return GetExperienceToNextLevel();
		}
	}

	public uint Experience
	{
		get
		{
			return GetExperience();
		}
	}

	public int MaxPower
	{
		get
		{
			return GetMaxPower();
		}
	}

	public long PowerSyncTime
	{
		get
		{
			return GetPowerSyncTime();
		}
		set
		{
			SetPowerSyncTime(value);
		}
	}

	public long EnergyRefillTimer
	{
		get
		{
			return GetEnergyRefillTimer();
		}
	}

	public long LastDumpTime
	{
		get
		{
			return GetLastDumpTime();
		}
		set
		{
			SetLastDumpTime(value);
		}
	}

	public long PeriodicPlayTime
	{
		get
		{
			return GetPeriodicPlayTime();
		}
		set
		{
			SetPeriodicPlayTime(value);
		}
	}

	public long LastDailyTimeOffset
	{
		get
		{
			return GetLastDailyTimeOffset();
		}
		set
		{
			SetLastDailyTimeOffset(value);
		}
	}

	public long LastEnergyTimeOffset
	{
		get
		{
			return GetLastEnergyTimeOffset();
		}
		set
		{
			SetLastEnergyTimeOffset(value);
		}
	}

	public bool ShowUpgrades
	{
		get
		{
			return GetShowUpgrades();
		}
		set
		{
			SetShowUpgrades(value);
		}
	}

	public bool ShowForge
	{
		get
		{
			return GetShowForge();
		}
		set
		{
			SetShowForge(value);
		}
	}

	public int DiscipleMode
	{
		get
		{
			return GetDiscipleMode();
		}
	}

	public int DenominationDigits
	{
		get
		{
			return GetDenominationDigits();
		}
		set
		{
			SetDenominationDigits(value);
		}
	}

	public FightIDS FightIds
	{
		get
		{
			return GetFightIds();
		}
		set
		{
			SetFightIds(value);
		}
	}

	public Color MapMaskTint
	{
		get
		{
			return GetMapMaskColor();
		}
		set
		{
			set_MapMaskColor(value);
		}
	}

	public ModelParameters PlayerParameters
	{
		get
		{
			return get_Parameters();
		}
	}

	public string Language
	{
		get
		{
			return GetLanguage();
		}
		set
		{
			SetLanguage(value);
		}
	}

	public string CoinIcon
	{
		get
		{
			return GetCoinIcon();
		}
		set
		{
			SetCoinIcon(value);
		}
	}

	public FightIDS MapFocus
	{
		get
		{
			return GetMapFocus();
		}
		set
		{
			SetMapFocusIds(value);
		}
	}

	public FightIDS RaidMapFocus
	{
		get
		{
			return GetRaidMapFocus();
		}
		set
		{
			SetRaidMapFocusIds(value);
		}
	}

	public bool FacebookLiked
	{
		get
		{
			return GetFacebookLiked();
		}
		set
		{
			SetFacebookLiked(value);
		}
	}

	public long StarterPackTimerEndTime
	{
		get
		{
			return GetStarterPackTimerEndTime();
		}
		set
		{
			SetStarterPackTimerEndTime(value);
		}
	}

	public RosterTimerContainer Timers
	{
		get
		{
			return GetTimerContainer();
		}
	}

	public bool EclipseMode
	{
		get
		{
			return IsEclipseMode();
		}
	}

	public int Level
	{
		get
		{
			return GetLevel();
		}
		set
		{
			SetLevel(value);
		}
	}

	public int NewTricksCount
	{
		get
		{
			return CountNewTricks();
		}
	}

	public int SealCount
	{
		get
		{
			return CountOwnedSeals();
		}
	}

	public bool TrySocialLogin
	{
		get
		{
			return GetTrySocialLogin();
		}
		set
		{
			SetTrySocialLogin(value);
		}
	}

	public bool AskedForDumps
	{
		get
		{
			return GetAskedForDumps();
		}
		set
		{
			SetAskedForDumps(value);
		}
	}

	public bool GPlusAutoLogin
	{
		get
		{
			return GetGPlusAutoLogin();
		}
		set
		{
			SetGPlusAutoLogin(value);
		}
	}

	public string MarketCode
	{
		get
		{
			return GetMarketCode();
		}
	}

	public string CurrentZone
	{
		get
		{
			return GetCurrentZone();
		}
		set
		{
			SetCurrentZone(value);
		}
	}

	public bool RaidRemindRandomRule
	{
		get
		{
			return GetRaidRemindRandomRule();
		}
		set
		{
			SetRaidRemindRandomRule(value);
		}
	}

	public Roster(XmlNode node, ModelParameters JCICKLIMBEF)
		: base(node)
	{
		int restoredBossEntries = Eclipse.Content.QuestCompatibility.RestoreUnsupportedHardmodeBosses(node);
		if (restoredBossEntries != 0)
		{
			UnityEngine.Debug.Log("[DevXml] restored completed boss map entries from unsupported hardmode save data");
		}
		if (Eclipse.Content.QuestCompatibility.EnsureEligibleEclipseButton(node))
		{
			UnityEngine.Debug.Log("[DevXml] restored the Eclipse Mode map button from completed progression");
		}
		LevelOverride = 0;
		UseLevelOverride = false;
		SetServerUserId(node.Attributes["ServerUserID"].GetStringOrDefault(string.Empty));
		SetAskedForDumps(node.Attributes["AskedForDumps"].ParseBool());
		modelParameters = JCICKLIMBEF;
		if (JCICKLIMBEF != null) JCICKLIMBEF.EclipseRosterPlayer = true;
		LoadLevelThresholds();
		_indexSlider = node.Attributes["IndexSlider"].ParseUint();
		coinIcon = node.Attributes["CoinIcon"].GetStringOrDefault("MiscSprites.gold");
		if (coinIcon.Contains(".png"))
		{
			SetCoinIcon(coinIcon.Replace(".png", string.Empty));
		}
		if (!coinIcon.Contains("MiscSprites"))
		{
			SetCoinIcon(string.Format("{0}.{1}", "MiscSprites", coinIcon));
		}
		SetMoney(node.Attributes["Money"].ParseLong(0L));
		SetBonus(node.Attributes["Bonus"].ParseLong(0L), BalanceChangeType.CHANGE_INIT);
		paidBonus = (ObscuredLong)(node.Attributes["PaidBonus"].ParseLong(0L));
		paidMoney = (ObscuredLong)(node.Attributes["PaidMoney"].ParseLong(0L));
		denominationDigits = node.Attributes["DenominationDigits"].ParseInt();
		RescaleCurrencyDenomination();
		SetLevel(node.Attributes["Level"].ParseInt());
		_experience = (ObscuredUInt)(node.Attributes["Experience"].ParseUint());
		PowerMax = GameUtils.GetMaxPower();
		PowerRegenSeconds = GameUtils.PowerMaxTime;
		_power = (ObscuredInt)PowerMax;
		powerSyncTime = -1L;
		lastDailyTimeOffset = node.Attributes["LastDailyTimeOffset"].ParseInt();
		lastEnergyTimeOffset = node.Attributes["LastEnergyTimeOffset"].ParseInt();
		lastDumpTime = node.Attributes["LastDumpTime"].ParseInt();
		SetFightIdsFromString(node.Attributes["FightIDS"].GetStringOrDefault(string.Empty));
		mapFocusAttribute = node.Attributes["MapFocus"];
		SetMapFocus(node.Attributes["MapFocus"].GetStringOrDefault(string.Empty));
		raidMapFocusAttribute = node.Attributes["RaidMapFocus"];
		SetRaidMapFocus(node.Attributes["RaidMapFocus"].GetStringOrDefault(string.Empty));
		SetLanguage(node.Attributes["Language"].GetStringOrDefault(string.Empty));
		showUpgrades = node.Attributes["ShowUpgrades"].ParseBool();
		showForge = node.Attributes["ShowForge"].ParseBool();
		trySocialLogin = node.Attributes["TrySocialLogin"].ParseBool(true);
		periodicPlayTime = node.Attributes["PeriodicPlayTime"].ParseLong(0L);
		currentZone = node.Attributes["CurrentZone"].GetStringOrDefault(string.Empty);
		userTutorials.Parse(node);
		eclipseMode = node.Attributes["EclipseMode"].GetStringOrDefault("Off") == "On";
		// The mask is otherwise only set by the eclipse.xml toggle quests, so a
		// profile restored in Eclipse mode showed the daytime map until toggled.
		// Match the colors those quests apply for each mode.
		_MapMaskColor = eclipseMode ? ColorUtils.ParseHexColor("#83624BFF") : Color.white;
		itemsNode = node["Items"];
		userItems.Parse(itemsNode);
		battlesNode = node["Battles"];
		foreach (XmlNode childNode in battlesNode.ChildNodes)
		{
			AddBattle(new RosterBattle(childNode));
		}
		fightsNode = node["Fights"];
		if (fightsNode != null)
		{
			foreach (XmlNode childNode2 in fightsNode.ChildNodes)
			{
				AddFight(new RosterFight(childNode2));
			}
		}
		shopNode = node["Shop"];
		if (shopNode != null)
		{
			foreach (XmlNode childNode3 in shopNode.ChildNodes)
			{
				AddShopLock(childNode3.Attributes["Name"].GetStringOrDefault(string.Empty));
			}
		}
		userPerks = new UserPerks(modelParameters);
		userPerks.Parse(node);
		if (node["Quests"] != null)
		{
			questsNode = node["Quests"]["Quests"];
			if (questsNode != null)
			{
				foreach (XmlNode childNode4 in questsNode.ChildNodes)
				{
					AddQuestFromNode(childNode4);
				}
			}
			QuestVariables = new Dictionary<string, RosterQuest.QuestVariable>();
			variablesNode = node["Quests"]["Variables"];
			if (variablesNode != null)
			{
				foreach (XmlNode childNode5 in variablesNode.ChildNodes)
				{
					ParseQuestVariable(childNode5);
				}
			}
		}
		userAchievements.Parse(node);
		openTricksNode = node["OpenTricks"];
		if (openTricksNode != null)
		{
			foreach (XmlNode childNode6 in openTricksNode.ChildNodes)
			{
				AddOpenTrick(childNode6.Attributes["Name"].GetStringOrDefault(string.Empty), false);
			}
		}
		timersNode = node["Timers"];
		if (timersNode == null)
		{
			timersNode = node.AppendElement("Timers");
		}
		timerContainer = new RosterTimerContainer(timersNode);
		LoadSoundSettings(node);
		// The decompiled roster never restored the dojo opponent toggle from
		// SessionSettings.  A click wrote Disciple correctly, but reloading the
		// dojo immediately reset the backing field to zero and made the selector
		// appear non-functional.
		int disciple;
		if (!int.TryParse(GetSettingsXML("Disciple"), out disciple))
		{
			disciple = 0;
		}
		discipleMode = (disciple != 0) ? 1 : 0;
		LoadCounterItems();
		MapButtonController.GetInstance().Parse(node);
		currenciesNode = node["Currencies"];
		if (currenciesNode == null)
		{
			currenciesNode = node.AppendElement("Currencies");
		}
		ParseCurrencies(currenciesNode);
		paymentsNode = node["Payments"];
		if (paymentsNode == null)
		{
			paymentsNode = node.AppendElement("Payments");
		}
		if (paymentOrders == null)
		{
			paymentOrders = new PaymentOrdersManager();
		}
		if (!paymentOrders.LoadLegacyPaymentOrders(node.Attributes["PaymentOrders"]))
		{
			paymentOrders.LoadFromXml(paymentsNode);
		}
	}

	public XmlNode GetItemsNode()
	{
		return itemsNode;
	}

	public UserItems GetInventory()
	{
		return userItems;
	}

	public UserAchievements GetAchievements()
	{
		return userAchievements;
	}

	public UserTutorials GetTutorials()
	{
		return userTutorials;
	}

	public UserPerks GetPerks()
	{
		return userPerks;
	}

	// best guess for name
	public List<RosterBattle> GetSavedBattles()
	{
		return battles;
	}

	public List<RosterFight> GetSavedFights()
	{
		return fights;
	}

	public List<RosterQuest> GetQuests()
	{
		return quests;
	}

	public List<string> GetOpenTricks()
	{
		return openTricks;
	}

	public static PaymentOrdersManager GetPaymentOrders()
	{
		return paymentOrders;
	}

	public long GetMoney()
	{
		return (ObscuredLong)(money);
	}

	public void SetMoney(long value)
	{
		long num = (long)Math.Pow(10.0, denominationDigits);
		if (num <= 0)
		{
			num = 1L;
		}
		long num2 = (ObscuredLong)(money);
		money = (ObscuredLong)(value);
		SetNodeAttribute("Money", (ObscuredLong)(money) * num);
	}

	public long GetBonus()
	{
		return (ObscuredLong)(bonus);
	}

	public void SetBonus(long value, BalanceChangeType LFLGCDNKNJI, bool JEEOLJIFIOF = false)
	{
		long num = (ObscuredLong)(bonus);
		bonus = (ObscuredLong)(value);
		SetNodeAttribute("Bonus", (ObscuredLong)(bonus));
		long num2 = Math.Abs(num - value);
		if (LFLGCDNKNJI == BalanceChangeType.CHANGE_INIT)
		{
			return;
		}
		if (value > num)
		{
			if (JEEOLJIFIOF)
			{
				SetPaidBonus((ObscuredLong)((ObscuredLong)(paidBonus) + num2));
			}
			ArgsDict kEMMIFBFDPK = new ArgsDict();
			kEMMIFBFDPK["changed"] = num2;
			kEMMIFBFDPK["type"] = GetBalanceChangeName(LFLGCDNKNJI);
			kEMMIFBFDPK["isPaid"] = JEEOLJIFIOF;
			StatisticsCollector.LogEvent(StatisticsEvent.EventType.Gems_Changed, kEMMIFBFDPK);
		}
		else if (value < num)
		{
			long num3 = num - (ObscuredLong)(paidBonus);
			long num4 = Math.Max(0L, num2 - num3);
			SetPaidBonus((ObscuredLong)((ObscuredLong)(paidBonus) - num4));
		}
	}

	public ObscuredLong GetPaidMoney()
	{
		return paidMoney;
	}

	public void SetPaidMoney(ObscuredLong value)
	{
		long num = (long)Math.Pow(10.0, denominationDigits);
		if (num <= 0)
		{
			num = 1L;
		}
		long num2 = (ObscuredLong)(paidMoney);
		paidMoney = value;
		SetNodeAttribute("PaidMoney", (ObscuredLong)(value) * num);
	}

	public ObscuredLong GetPaidBonus()
	{
		return paidBonus;
	}

	public void SetPaidBonus(ObscuredLong value)
	{
		paidBonus = value;
		SetNodeAttribute("PaidBonus", (ObscuredLong)(value));
	}

	public uint GetSliderIndex()
	{
		return _indexSlider;
	}

	public void set_IndexSlider(uint value)
	{
		_indexSlider = value;
		SetNodeAttribute("IndexSlider", value);
	}

	public uint GetExperienceToNextLevel()
	{
		if (modelParameters != null)
		{
			int num = (ObscuredInt)(modelParameters.GetLevel()) - 1;
			if (num < _levelThresholds.Count)
			{
				return (ObscuredUInt)(_levelThresholds[num]);
			}
		}
		return 2147483647u;
	}

	public uint GetExperience()
	{
		return (ObscuredUInt)(_experience);
	}

	public bool SetExperience(uint value)
	{
		if (modelParameters == null)
		{
			return false;
		}
		int storyProfile = Eclipse.Modding.ModRuntime.StoryEvents.ProfileGeneration;
		int previousLevel = GetLevel();
		bool flag = false;
		uint num = GetExperienceToNextLevel();
		_experience = (ObscuredUInt)(value);
		uint num2 = (ObscuredUInt)(_experience);
		if (num <= num2)
		{
			flag = true;
			int num3 = (ObscuredInt)(modelParameters.GetLevel());
			while (num <= num2)
			{
				num2 -= num;
				num3++;
				modelParameters.SetLevel((ObscuredInt)(num3));
				num = GetExperienceToNextLevel();
				if (num2 > GameUtils.MaximumExperience)
				{
					num2 = GameUtils.MaximumExperience;
				}
			}
			if (num3 > _levelThresholds.Count)
			{
				num3 = _levelThresholds.Count;
				num2 = GetExperienceToNextLevel();
				flag = false;
			}
			_experience = (ObscuredUInt)(num2);
			modelParameters.SetLevel((ObscuredInt)(num3));
			SetNodeAttribute("Level", (ObscuredInt)(modelParameters.GetLevel()));
		}
		if ((ObscuredInt)(modelParameters.GetLevel()) == _levelThresholds.Count && (ObscuredInt)(modelParameters.GetLevel()) > 1)
		{
			num2 = (ObscuredUInt)(_levelThresholds[(ObscuredInt)(modelParameters.GetLevel()) - 2]);
			_experience = (ObscuredUInt)(num2);
		}
		if (flag)
		{
			GetInventory().RefreshUpgradeStates();
			GetInventory().UpdateLockItems(GetLevel());
			GameUtils.UpdateShopNewItemsCounters();
			ListSF.GetInstance().UpdateFightsLevel(GetLevel());
			StatisticsCollector.LogEvent(StatisticsEvent.EventType.Level_Up);
			GameUtils.TrackEvent("Level Up");
		}
		SetNodeAttribute("Experience", (ObscuredUInt)(_experience));
		Eclipse.Modding.ModRuntime.PublishLevelUp(this, previousLevel, storyProfile);
		return flag;
	}

	public bool AddExperience(uint value)
	{
		return SetExperience((ObscuredUInt)(_experience) + value);
	}

	public void RefreshLevelFromExperience()
	{
		bool flag = SetExperience((ObscuredUInt)(_experience));
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		hHKLFIIBIFF.levelUp = (flag ? 1 : 0);
	}

	public int GetMaxPower()
	{
		return PowerMax;
	}

	public long GetPowerSyncTime()
	{
		return powerSyncTime;
	}

	public void SetPowerSyncTime(long value)
	{
		powerSyncTime = value;
		SetNodeAttribute("PowerSyncTime", value);
	}

	public long GetEnergyRefillTimer()
	{
		return -1L;
	}

	public long GetLastDumpTime()
	{
		return lastDumpTime;
	}

	public void SetLastDumpTime(long value)
	{
		lastDumpTime = value;
		SetNodeAttribute("LastDumpTime", lastDumpTime);
	}

	public long GetPeriodicPlayTime()
	{
		return periodicPlayTime;
	}

	public void SetPeriodicPlayTime(long value)
	{
		periodicPlayTime = value;
		SetNodeAttribute("PeriodicPlayTime", periodicPlayTime);
	}

	public long GetLastDailyTimeOffset()
	{
		return lastDailyTimeOffset;
	}

	public void SetLastDailyTimeOffset(long value)
	{
		lastDailyTimeOffset = value;
		SetNodeAttribute("LastDailyTimeOffset", lastDailyTimeOffset);
	}

	public long GetLastEnergyTimeOffset()
	{
		return lastEnergyTimeOffset;
	}

	public void SetLastEnergyTimeOffset(long value)
	{
		lastEnergyTimeOffset = value;
		SetNodeAttribute("LastEnergyTimeOffset", lastEnergyTimeOffset);
	}

	public bool GetShowUpgrades()
	{
		return showUpgrades;
	}

	public void SetShowUpgrades(bool value)
	{
		showUpgrades = value;
		SetNodeAttribute("ShowUpgrades", showUpgrades);
	}

	public bool GetShowForge()
	{
		return showForge;
	}

	public void SetShowForge(bool value)
	{
		showForge = value;
		SetNodeAttribute("ShowForge", showForge);
	}

	public int GetDiscipleMode()
	{
		return discipleMode;
	}

	public int GetDenominationDigits()
	{
		return denominationDigits;
	}

	public void SetDenominationDigits(int value)
	{
		denominationDigits = value;
		SetNodeAttribute("DenominationDigits", denominationDigits);
	}

	public FightIDS GetFightIds()
	{
		return fightIds;
	}

	public void SetFightIds(FightIDS value)
	{
		fightIds = value;
		SetNodeAttribute("FightIDS", fightIds.ToString());
	}

	public void SetFightIdsFromString(string JFIIJBAOOIK)
	{
		if (fightIds != null)
		{
			fightIds.SetFightIDSByString(JFIIJBAOOIK);
		}
		else
		{
			fightIds = new FightIDS(JFIIJBAOOIK);
		}
	}

	public Color GetMapMaskColor()
	{
		return _MapMaskColor;
	}

	public void set_MapMaskColor(Color value)
	{
		_MapMaskColor = value;
	}

	public ModelParameters get_Parameters()
	{
		return modelParameters;
	}

	public string GetLanguage()
	{
		return language;
	}

	public void SetLanguage(string value)
	{
		language = value;
		SetNodeAttribute("Language", language);
	}

	public string GetCoinIcon()
	{
		return coinIcon;
	}

	public void SetCoinIcon(string value)
	{
		coinIcon = value;
		SetNodeAttribute("CoinIcon", coinIcon);
	}

	public FightIDS GetMapFocus()
	{
		return mapFocus;
	}

	public void SetMapFocusIds(FightIDS value)
	{
		SetMapFocus(value.ToString());
	}

	// best guess for name
	public void SetMapFocus(string JFIIJBAOOIK)
	{
		if (!mapFocus.EqualsZoneBattle(JFIIJBAOOIK))
		{
			mapFocus.SetFightIDSByString(JFIIJBAOOIK);
			if (mapFocusAttribute != null)
			{
				mapFocusAttribute.Value = mapFocus.GetZoneBattle();
			}
			else
			{
				SetNodeAttribute("MapFocus", mapFocus.GetZoneBattle());
			}
		}
	}

	public FightIDS GetRaidMapFocus()
	{
		if (raidMapFocus == null)
		{
			raidMapFocus = new FightIDS("ZONE_RAID|BOSS_1");
		}
		else if (raidMapFocus.IsEmpty())
		{
			raidMapFocus.SetFightIDSByString("ZONE_RAID|BOSS_1");
		}
		return raidMapFocus;
	}

	public void SetRaidMapFocusIds(FightIDS value)
	{
		SetRaidMapFocus(value.ToString());
	}

	// best guess for name
	public void SetRaidMapFocus(string JFIIJBAOOIK)
	{
		if (!raidMapFocus.EqualsZoneBattle(JFIIJBAOOIK))
		{
			raidMapFocus.SetFightIDSByString(JFIIJBAOOIK);
			if (raidMapFocus.IsEmpty())
			{
				raidMapFocus.SetFightIDSByString("ZONE_RAID|BOSS_1");
			}
			if (raidMapFocusAttribute != null)
			{
				raidMapFocusAttribute.Value = raidMapFocus.GetZoneBattle();
			}
			else
			{
				SetNodeAttribute("RaidMapFocus", raidMapFocus.GetZoneBattle());
			}
		}
	}

	public bool GetFacebookLiked()
	{
		return facebookLiked;
	}

	public void SetFacebookLiked(bool value)
	{
		facebookLiked = value;
		SetNodeAttribute("FacebookLiked", facebookLiked);
	}

	public long GetStarterPackTimerEndTime()
	{
		return starterPackTimerEndTime;
	}

	public void SetStarterPackTimerEndTime(long value)
	{
		starterPackTimerEndTime = value;
		SetNodeAttribute("StarterPackTimerEndTime", starterPackTimerEndTime);
	}

	public RosterTimerContainer GetTimerContainer()
	{
		return timerContainer;
	}

	// best guess for name
	public bool IsEclipseMode()
	{
		return eclipseMode;
	}

	public void SetEclipseMode(bool value)
	{
		eclipseMode = value;
		SetNodeAttribute("EclipseMode", value ? "On" : "Off");
	}

	public int GetLevel()
	{
		if (!UseLevelOverride)
		{
			return (modelParameters != null) ? (int)(modelParameters.GetLevel()) : 0;
		}
		return LevelOverride;
	}

	public void SetLevel(int value)
	{
		if (modelParameters != null)
		{
			modelParameters.SetLevel((ObscuredInt)(value));
			SetNodeAttribute("Level", (ObscuredInt)(modelParameters.GetLevel()));
		}
	}

	public int GetCurrencyCount(GameCurrency MDDNHLBDJBN)
	{
		CurrencyStruct lAPFHLGNAAF = FindCurrency(MDDNHLBDJBN);
		if (lAPFHLGNAAF != null)
		{
			return (ObscuredInt)(lAPFHLGNAAF.Count);
		}
		return 0;
	}

	public void ToggleDiscipleMode()
	{
		int num = ((discipleMode == 0) ? 1 : 0);
		SessionSettings("Disciple", num);
		discipleMode = num;
	}

	public int CountNewTricks()
	{
		int num = 0;
		List<Trick> list = GameUtils.GetPlayerTricks();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsNew)
			{
				num++;
			}
		}
		return num;
	}

	public int CountOwnedSeals()
	{
		int num = 0;
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP == null)
		{
			return 0;
		}
		List<UserItem> list = nKGLHEGIKKP.GetInventory().FindItemsByType("Seal", string.Empty);
		foreach (UserItem item in list)
		{
			if (item.GetCount() > 0 && item.GetInfo().GetIsNew())
			{
				num++;
			}
		}
		return num;
	}

	public bool HasShopLock(string name)
	{
		return FindShopLockIndex(name) >= 0;
	}

	public bool HasBattle(FightIDS DIAIIPCBMFL)
	{
		foreach (RosterBattle item in battles)
		{
			if (item.GetBattleId().Equals(DIAIIPCBMFL.ToString()))
			{
				return true;
			}
		}
		return false;
	}

	public string GetSettingsXML(string name)
	{
		if (_node == null || _node["SessionSettings"] == null || _node["SessionSettings"][name] == null)
		{
			return string.Empty;
		}
		return _node["SessionSettings"][name].Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public RosterQuest FindQuest(string name)
	{
		foreach (RosterQuest item in quests)
		{
			if (name == item.Name)
			{
				return item;
			}
		}
		return null;
	}

	public bool HasSessionSetting(string name)
	{
		XmlNode xmlNode = _node["SessionSettings"];
		if (xmlNode == null)
		{
			return false;
		}
		XmlNode xmlNode2 = xmlNode[name];
		if (xmlNode2 == null)
		{
			return false;
		}
		return true;
	}

	public bool DoesAbGroupExist(string name)
	{
		return false;
	}

	public void SaveMusicSettings()
	{
		XmlNode sounds = ((_node["Sounds"] != null) ? _node["Sounds"] : _node.AppendElement("Sounds"));
		XmlNode music = ((sounds["Music"] != null) ? sounds["Music"] : sounds.AppendElement("Music"));
		music.AppendAttribute("Value").Value = Sound.GetMusicVolume().ToString(System.Globalization.CultureInfo.InvariantCulture);
		music.AppendAttribute("Mute").Value = ((!Sound.GetMusicMuted()) ? "0" : "1");
		RequestSave();
	}

	public void SaveSoundSettings()
	{
		XmlNode sounds = ((_node["Sounds"] != null) ? _node["Sounds"] : _node.AppendElement("Sounds"));
		XmlNode sound = ((sounds["Sound"] != null) ? sounds["Sound"] : sounds.AppendElement("Sound"));
		sound.AppendAttribute("Value").Value = Sound.GetSoundVolume().ToString(System.Globalization.CultureInfo.InvariantCulture);
		sound.AppendAttribute("Mute").Value = ((!Sound.GetSoundMuted()) ? "0" : "1");
		RequestSave();
	}

	public static string GetBalanceChangeName(BalanceChangeType LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
		case BalanceChangeType.CHANGE_INIT:
			return "init";
		case BalanceChangeType.CHANGE_PAYMENT:
			return "payment";
		case BalanceChangeType.CHANGE_ACHIEVEMENT:
			return "achievement";
		case BalanceChangeType.CHANGE_FIGHT_REWARD:
			return "fight_reward";
		case BalanceChangeType.CHANGE_VIDEO_VIEW:
			return "video_view";
		case BalanceChangeType.CHANGE_FACEBOOK:
			return "facebook";
		case BalanceChangeType.CHANGE_QUEST:
			return "quest";
		case BalanceChangeType.CHANGE_CHEAT:
			return "cheat";
		case BalanceChangeType.CHANGE_SERVER_GIVE:
			return "server";
		case BalanceChangeType.CHANGE_OFFER:
			return "offer";
		case BalanceChangeType.CHANGE_BUY_ITEM:
			return "buy_item";
		case BalanceChangeType.CHANGE_BUY_DELIVERY:
			return "buy_delivery";
		case BalanceChangeType.CHANGE_RAIDS_REWARD:
			return "raids_reward";
		case BalanceChangeType.CHANGE_LEDGER:
			return "ledger";
		default:
			GameLog.Error("unknown BalanceChangeType");
			return "unknown";
		}
	}

	public List<RosterFight> FindFightsByLevel(int GNLOCMLBNHF)
	{
		List<RosterFight> list = new List<RosterFight>();
		foreach (RosterFight item in fights)
		{
			if (item.GetLevel() == GNLOCMLBNHF)
			{
				list.Add(item);
			}
		}
		return list;
	}

	// best guess for name
	public RosterFight FindSavedFightRecord(FightIDS JFIIJBAOOIK)
	{
		foreach (RosterFight item in fights)
		{
			if (JFIIJBAOOIK.Equals(item.GetFightIdString()))
			{
				return item;
			}
		}
		return null;
	}

	public RosterFight RecordFightWin(FightIDS DIAIIPCBMFL)
	{
		RosterFight pIGKOIFBOME = FindSavedFightRecord(DIAIIPCBMFL);
		pIGKOIFBOME.RecordWin();
		if (IsEclipseMode())
		{
			pIGKOIFBOME.IncrementEclipseWinCount();
		}
		return pIGKOIFBOME;
	}

	public RosterFight RecordFightLoss(FightIDS DIAIIPCBMFL)
	{
		RosterFight pIGKOIFBOME = FindSavedFightRecord(DIAIIPCBMFL);
		if (pIGKOIFBOME != null)
		{
			pIGKOIFBOME.RecordLoss();
			if (IsEclipseMode())
			{
				pIGKOIFBOME.IncrementEclipseLossCount();
			}
			return pIGKOIFBOME;
		}
		return null;
	}

	public RosterFight CreateFight(FightIDS DIAIIPCBMFL)
	{
		string text = "Fights";
		string jLEKBBJBLOE = "Fight";
		XmlNode mEEAKLDGLDF = ((_node[text] == null) ? _node.AppendElement(text) : _node[text]);
		XmlNode hKPPBKPJOEO = mEEAKLDGLDF.AppendElement(jLEKBBJBLOE);
		RosterFight pIGKOIFBOME = new RosterFight(hKPPBKPJOEO);
		pIGKOIFBOME.set_FightIDS(DIAIIPCBMFL.ToString());
		fights.Add(pIGKOIFBOME);
		return pIGKOIFBOME;
	}

	public RosterBattle CreateBattle(FightIDS DIAIIPCBMFL)
	{
		string text = "Battles";
		string jLEKBBJBLOE = "Battle";
		XmlNode mEEAKLDGLDF = (_node[text].IsEmpty ? _node.AppendElement(text) : _node[text]);
		string fOOKNBHPOOA = DIAIIPCBMFL.GetZone() + "|" + DIAIIPCBMFL.GetBattle() + "|";
		XmlNode xmlNode = mEEAKLDGLDF.FindChildWithAttribute("Battle", "Name", fOOKNBHPOOA);
		if (xmlNode == null)
		{
			xmlNode = mEEAKLDGLDF.AppendElement(jLEKBBJBLOE);
		}
		RosterBattle dDNLCGOPAGC = new RosterBattle(xmlNode);
		dDNLCGOPAGC.SetBattleId(DIAIIPCBMFL);
		battles.Add(dDNLCGOPAGC);
		return dDNLCGOPAGC;
	}

	public void EnsureBattleNode(FightIDS DIAIIPCBMFL)
	{
		string text = "Battles";
		string jLEKBBJBLOE = "Battle";
		XmlNode mEEAKLDGLDF = (_node[text].IsEmpty ? _node.AppendElement(text) : _node[text]);
		string text2 = DIAIIPCBMFL.GetZone() + "|" + DIAIIPCBMFL.GetBattle() + "|";
		XmlNode xmlNode = mEEAKLDGLDF.FindChildWithAttribute("Battle", "Name", text2);
		if (xmlNode == null)
		{
			XmlNode mEEAKLDGLDF2 = mEEAKLDGLDF.AppendElement(jLEKBBJBLOE);
			mEEAKLDGLDF2.AppendAttribute("Name").Value = text2;
		}
	}

	public RosterFight AddFight(RosterFight value, bool EFCPLDABOIF = false)
	{
		if (EFCPLDABOIF)
		{
			bool flag = false;
			foreach (RosterFight item in fights)
			{
				if (item.GetFightIdString() == value.GetFightIdString())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				fights.Add(value);
			}
		}
		else
		{
			fights.Add(value);
		}
		return value;
	}

	// best guess for name
	public void AddBattle(FightIDS DIAIIPCBMFL, bool EFCPLDABOIF = false, bool PEJELKNFEKJ = true, bool NIBIMBDBPMI = false, bool MDEHPLPLNNF = false, int OAHPBDFKJOK = 0)
	{
		if (DIAIIPCBMFL == null)
		{
			GameLog.Error("Roster::addBattle ERROR - ids is NULL");
			return;
		}
		if (!PEJELKNFEKJ)
		{
			RemoveBattle(DIAIIPCBMFL);
			return;
		}
		EnsureBattleNode(DIAIIPCBMFL);
		ListSF.GetInstance().OnAuthenticate();
		if (EFCPLDABOIF)
		{
			foreach (RosterBattle item in battles)
			{
				if (item.GetBattleId().Equals(DIAIIPCBMFL))
				{
					item.SetBattleId(DIAIIPCBMFL);
					item.SetLocked(NIBIMBDBPMI);
					item.SetHidden(MDEHPLPLNNF);
					item.SetReplayCount(OAHPBDFKJOK);
					return;
				}
			}
		}
		RosterBattle dDNLCGOPAGC = CreateBattle(DIAIIPCBMFL);
		dDNLCGOPAGC.SetLocked(NIBIMBDBPMI);
		dDNLCGOPAGC.SetHidden(MDEHPLPLNNF);
		dDNLCGOPAGC.SetReplayCount(OAHPBDFKJOK);
		Battle cGJCGEBPCAF = ListSF.GetBattleById(DIAIIPCBMFL);
		if (cGJCGEBPCAF != null)
		{
			dDNLCGOPAGC.LinkedBattle = cGJCGEBPCAF;
			cGJCGEBPCAF.SetRosterBattle(dDNLCGOPAGC);
			cGJCGEBPCAF.OnBattleCreated();
		}
	}

	// best guess for name
	public void AddBattle(RosterBattle ELBLEPOEKIL)
	{
		foreach (RosterBattle item in battles)
		{
			if (item.GetBattleId().Equals(ELBLEPOEKIL.GetBattleId()))
			{
				GameLog.Error("Battle already exists: " + item.GetBattleId().GetBattle());
				return;
			}
		}
		battles.Add(ELBLEPOEKIL);
	}

	// best guess for name
	public void AddBattle(Battle DPOOIONCEOA, bool EFCPLDABOIF = false, bool PEJELKNFEKJ = true, bool NIBIMBDBPMI = false, bool MDEHPLPLNNF = false, int OAHPBDFKJOK = 0)
	{
		FightIDS dIAIIPCBMFL = new FightIDS(DPOOIONCEOA.GetZone().get_Name(), DPOOIONCEOA.get_Name(), string.Empty);
		AddBattle(dIAIIPCBMFL, EFCPLDABOIF, PEJELKNFEKJ, NIBIMBDBPMI, MDEHPLPLNNF, OAHPBDFKJOK);
	}

	public void RemoveBattle(RosterBattle ELBLEPOEKIL)
	{
		string text = ELBLEPOEKIL.GetBattleId().ToString();
		XmlNode xmlNode = _node["Battles"];
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			string text2 = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			if (text2 == text)
			{
				xmlNode.RemoveChild(childNode);
				break;
			}
		}
		ListSF.GetInstance().RequestSave();
		foreach (RosterBattle item in battles)
		{
			if (ELBLEPOEKIL == item)
			{
				battles.Remove(item);
				break;
			}
		}
		Battle eDHMHFONDAI = ELBLEPOEKIL.LinkedBattle;
		eDHMHFONDAI.ClearRosterBattle();
	}

	public void RemoveBattle(FightIDS DIAIIPCBMFL)
	{
		foreach (RosterBattle item in battles)
		{
			if (item.GetBattleId().Equals(DIAIIPCBMFL))
			{
				RemoveBattle(item);
				break;
			}
		}
	}

	public List<RosterBattle> GetBattlesInZone(string PPBIPCKMFKB)
	{
		List<RosterBattle> list = new List<RosterBattle>();
		foreach (RosterBattle item in battles)
		{
			if (item.GetBattleId().GetZone() == PPBIPCKMFKB)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public RosterQuest AddQuest(string GACKIHNGHLE, string PMFEIPCHENB)
	{
		XmlNode xmlNode = ((_node["Quests"] == null) ? _node.AppendElement("Quests") : _node["Quests"]);
		XmlNode mEEAKLDGLDF = ((xmlNode["Quests"] == null) ? xmlNode.AppendElement("Quests") : xmlNode["Quests"]);
		XmlNode xmlNode2 = mEEAKLDGLDF.AppendElement("Quest");
		xmlNode2.AppendAttribute("Name").Value = GACKIHNGHLE;
		string value = DirectoryController.StripProtocol(PMFEIPCHENB);
		xmlNode2.AppendAttribute("FileName").Value = value;
		return AddQuestFromNode(xmlNode2);
	}

	public RosterQuest AddQuestFromNode(XmlNode FNMHECBANNJ)
	{
		RosterQuest dKBDLDGOFDN = new RosterQuest(FNMHECBANNJ);
		quests.Add(dKBDLDGOFDN);
		return dKBDLDGOFDN;
	}

	public void ParseQuestVariable(XmlNode node)
	{
		string text = "_" + node.Attributes["Name"].GetStringOrDefault(string.Empty);
		RosterQuest.QuestVariable nOKCOAHJIPB = FindQuestVariable(text);
		if (nOKCOAHJIPB != null)
		{
			nOKCOAHJIPB.SetValue(node.Attributes["Value"].GetStringOrDefault(string.Empty));
			return;
		}
		nOKCOAHJIPB = new RosterQuest.QuestVariable(node);
		QuestVariables[text] = nOKCOAHJIPB;
	}

	public RosterQuest.QuestVariable FindQuestVariable(string name)
	{
		if (QuestVariables.ContainsKey(name))
		{
			return QuestVariables[name];
		}
		return null;
	}

	public void SetQuestVariable(string name, string value)
	{
		string gOHIIMFFFJI = "_" + name;
		RosterQuest.QuestVariable nOKCOAHJIPB = FindQuestVariable(gOHIIMFFFJI);
		if (nOKCOAHJIPB != null)
		{
			nOKCOAHJIPB.SetValue(value);
			return;
		}
		XmlNode xmlNode = ((_node["Quests"] == null) ? _node.AppendElement("Quests") : _node["Quests"]);
		XmlNode mEEAKLDGLDF = ((xmlNode["Variables"] == null) ? xmlNode.AppendElement("Variables") : xmlNode["Variables"]);
		XmlNode xmlNode2 = mEEAKLDGLDF.AppendElement("Variable");
		xmlNode2.AppendAttribute("Name").Value = name;
		xmlNode2.AppendAttribute("Value").Value = value;
		nOKCOAHJIPB = new RosterQuest.QuestVariable(xmlNode2);
		gOHIIMFFFJI = "_" + nOKCOAHJIPB.Name;
		QuestVariables[gOHIIMFFFJI] = nOKCOAHJIPB;
	}

	public void SessionSettings(string name, string value)
	{
		XmlNode xmlNode = _node["SessionSettings"];
		if (xmlNode == null)
		{
			xmlNode = _node.AppendElement("SessionSettings");
		}
		XmlNode xmlNode2 = xmlNode[name];
		if (xmlNode2 == null)
		{
			xmlNode2 = xmlNode.AppendElement(name);
		}
		XmlAttribute xmlAttribute = xmlNode2.Attributes["Value"];
		if (xmlAttribute == null)
		{
			xmlAttribute = xmlNode2.AppendAttribute("Value");
		}
		xmlAttribute.Value = value;
	}

	public void SessionSettings(string name, int value)
	{
		SessionSettings(name, value.ToString());
	}

	public bool IsItemEquipped(ItemInfo item)
	{
		UserItem dKCHDHMLKHN = GetInventory().FindItem(item);
		return dKCHDHMLKHN != null && dKCHDHMLKHN.GetIsEquipped();
	}

	public long GetUnpaidBonus()
	{
		return GetBonus() - (ObscuredLong)(GetPaidBonus());
	}

	public bool GetTrySocialLogin()
	{
		return trySocialLogin;
	}

	public void SetTrySocialLogin(bool value)
	{
		trySocialLogin = value;
		SetNodeAttribute("TrySocialLogin", trySocialLogin);
	}

	public bool ChangePower(int value)
	{
		// Spending/refilling energy is a successful no-op, never a fight gate.
		return true;
	}

	public void SetPower(int value)
	{
		// Legacy SetEnergy quest actions must not reintroduce depletion.
		_power = (ObscuredInt)PowerMax;
	}

	public void UpdatePowerRegeneration(long LBIGLJLMIDG)
	{
		// No regeneration clock or save writes are needed without energy.
	}

	public bool EquipItem(UserItem item, bool JBCMFEPAKLK = true)
	{
		if (!JBCMFEPAKLK)
		{
			RequestSave();
			return true;
		}
		if (modelParameters != null)
		{
			ItemInfo dJKEECEOCJB = item.GetInfo();
			if (dJKEECEOCJB.GetUpgradeItemByUpgradeLevel(item.GetUpgradeLevel()) != null)
			{
				dJKEECEOCJB = dJKEECEOCJB.GetUpgradeItemByUpgradeLevel(item.GetUpgradeLevel());
			}
			modelParameters.SetItemByType(dJKEECEOCJB.Type, dJKEECEOCJB);
			SetNodeAttribute(dJKEECEOCJB.Type, (!JBCMFEPAKLK) ? string.Empty : dJKEECEOCJB.Name);
			modelParameters.BuildModelDocuments();
			return true;
		}
		return false;
	}

	public void UnequipItem(ItemInfo PJDAGCBPLJE)
	{
		if (modelParameters != null)
		{
			modelParameters.SetItemByType(PJDAGCBPLJE.Type, null);
			SetNodeAttribute(PJDAGCBPLJE.Type, string.Empty);
			modelParameters.BuildModelDocuments();
		}
	}

	public void RefillPower()
	{
		SetPower(PowerMax);
	}

	public bool StartPendingQuests()
	{
		RosterQuest dKBDLDGOFDN = null;
		int num = 0;
		List<QuestStage> list = new List<QuestStage>();
		foreach (RosterQuest item in quests)
		{
			if (ListSF.GetInstance().IsEclipseQuestSuppressed(item.Name, item.FileName)) continue;
			if (item.get_Parameters() == null)
			{
				continue;
			}
			if (!HasLoadedQuestFile(item.FileName))
			{
				ListSF.GetInstance().LoadQuests(item.FileName);
			}
			QuestStage mLLKDGBEGJI = ListSF.GetInstance().FindEclipseSavedQuest(item.Name, item.FileName);
			if (ListSF.GetInstance().IsEclipseQuestSuppressed(item.Name, item.FileName)) continue;
			if (mLLKDGBEGJI != null)
			{
				if (mLLKDGBEGJI.IsUnresumable())
				{
					mLLKDGBEGJI.MarkComplete();
					continue;
				}
				if (num == 0)
				{
					dKBDLDGOFDN = item;
				}
				foreach (QuestStage item2 in list)
				{
					if (item2 == mLLKDGBEGJI)
					{
						GameLog.Error("same name of quest %s", item.Name);
					}
				}
				list.Add(mLLKDGBEGJI);
				num++;
			}
			else
			{
				item.ClearParameters();
				item.IsParametersCleared = true;
			}
		}
		if (list.Count > 0)
		{
			ListSF.GetInstance().AddActionQuests(list);
			if (dKBDLDGOFDN != null)
			{
				ScreenType iPKNDMINFMJ = (ScreenType)dKBDLDGOFDN.GetCheckpointScreenType();
				ScreenType iPKNDMINFMJ2 = Module.GetInstance().GetCurrentScreenType();
				if (iPKNDMINFMJ == ScreenType.ModuleFight && iPKNDMINFMJ != iPKNDMINFMJ2)
				{
					Module.OpenScreen(ScreenType.ModuleDojo);
				}
				else if (iPKNDMINFMJ != (ScreenType)(-1) && iPKNDMINFMJ != iPKNDMINFMJ2)
				{
					Module.OpenScreen(iPKNDMINFMJ, 0);
				}
				return true;
			}
		}
		return false;
	}

	public bool GetAskedForDumps()
	{
		return askedForDumps;
	}

	public void SetAskedForDumps(bool value)
	{
		askedForDumps = value;
		SetNodeAttribute("AskedForDumps", askedForDumps);
	}

	public void UpdateLastDumpTime()
	{
		long num = ListSF.GetCurrentTime();
		if (num - GetLastDumpTime() >= 900)
		{
			SetLastDumpTime(num);
		}
	}

	public void SaveBillingItems()
	{
		if (_node == null)
		{
			return;
		}
		List<ItemInfo> list = ListSF.GetItems().GetItemsByType("RealMoneyItem");
		XmlNode xmlNode = _node["Billing"];
		if (xmlNode == null)
		{
			xmlNode = _node.AppendNewNode("Billing");
		}
		foreach (ItemInfo item in list)
		{
			XmlNode xmlNode2 = xmlNode.FindChildWithAttribute("Item", "Name", item.Name);
			if (xmlNode2 == null)
			{
				xmlNode2 = xmlNode.AppendNewNode("Item");
			}
			XmlAttribute xmlAttribute = xmlNode2.AppendAttribute("Name");
			xmlAttribute.Value = item.Name;
			xmlAttribute = xmlNode2.AppendAttribute("RealPrice");
			xmlAttribute.Value = item.LocalizedPriceString;
			xmlAttribute = xmlNode2.AppendAttribute("RealPriceConst");
			xmlAttribute.Value = item.PriceAmountText;
			xmlAttribute = xmlNode2.AppendAttribute("RealPriceCurrency");
			xmlAttribute.Value = item.CurrencyCode;
		}
		ListSF.GetInstance().RequestSave();
	}

	public bool AddShopLock(string name, bool FLOAHAOBNAP = false)
	{
		if (!HasShopLock(name))
		{
			if (FLOAHAOBNAP)
			{
				string text = "Shop";
				string iMGCANJHPND = "Lock";
				XmlNode xmlNode = _node[text];
				if (xmlNode == null)
				{
					xmlNode = _node.AppendNewNode(text);
				}
				XmlNode mEEAKLDGLDF = xmlNode.AppendNewNode(iMGCANJHPND);
				mEEAKLDGLDF.AppendAttribute("Name").Value = name;
			}
			shopLocks.Add(name);
			return true;
		}
		return false;
	}

	public bool RemoveShopLock(string name)
	{
		int num = FindShopLockIndex(name);
		if (num >= 0)
		{
			XmlNode xmlNode = _node["Shop"];
			XmlNode xmlNode2 = xmlNode.FindChildWithAttribute("Lock", "Name", name);
			if (xmlNode2 != null)
			{
				xmlNode.RemoveChild(xmlNode2);
			}
			shopLocks.RemoveAt(num);
			return true;
		}
		return false;
	}

	public int FindShopLockIndex(string name)
	{
		if (name == null)
		{
			return -1;
		}
		int num = 0;
		foreach (string item in shopLocks)
		{
			if (item.Equals(name))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public void AddOpenTrick(string name, bool NLCCJEHMAOF = true)
	{
		if (openTricks.Contains(name))
		{
			return;
		}
		openTricks.Add(name);
		List<Trick> list = AnimationData.GetTricks();
		for (int i = 0; i < list.Count; i++)
		{
			Trick iHNIKIHKFHC = list[i];
			if (iHNIKIHKFHC.Name == name)
			{
				iHNIKIHKFHC.IsNew = true;
				break;
			}
		}
		if (NLCCJEHMAOF)
		{
			string text = "OpenTricks";
			string jLEKBBJBLOE = "Trick";
			XmlNode mEEAKLDGLDF = ((_node[text] == null) ? _node.AppendElement(text) : _node[text]);
			XmlNode mEEAKLDGLDF2 = mEEAKLDGLDF.AppendElement(jLEKBBJBLOE);
			mEEAKLDGLDF2.AppendAttribute("Name").Value = name;
			RequestSave();
		}
	}

	public void RemoveOpenTrick(string name, bool NLCCJEHMAOF = true)
	{
		string nameKey = "OpenTricks";
		if (_node[nameKey] == null)
		{
			return;
		}
		XmlNode xmlNode = _node[nameKey];
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			if (childNode.Attributes["Name"].GetStringOrDefault(string.Empty) == nameKey)
			{
				xmlNode.RemoveChild(childNode);
				break;
			}
		}
		if (openTricks.Contains(name))
		{
			openTricks.Remove(name);
		}
		if (NLCCJEHMAOF)
		{
			RequestSave();
		}
	}

	private void LoadCounterItems()
	{
		XmlNode xmlNode = _node["CounterItems"];
		if (xmlNode == null)
		{
			return;
		}
		XmlNode xmlNode2 = xmlNode["Items"];
		if (xmlNode2 == null)
		{
			return;
		}
		foreach (XmlNode childNode in xmlNode2.ChildNodes)
		{
			ItemInfo dJKEECEOCJB = ListSF.GetItems().GetItemByName(childNode.Attributes["Name"].GetStringOrDefault());
			if (dJKEECEOCJB != null && dJKEECEOCJB.SilentReceive == 0)
			{
				dJKEECEOCJB.SetIsNew(true);
			}
		}
	}

	public void SaveCounterItems()
	{
		List<string> PIDDFMECFAO = new List<string>();
		List<ItemInfo> list = ListSF.GetItems().GetAllItems();
		list.ForEach((ItemInfo DHDMNHCIPEH) =>
		{
			if (DHDMNHCIPEH.GetIsNew())
			{
				PIDDFMECFAO.Add(DHDMNHCIPEH.Name);
			}
		});
		string text = "CounterItems";
		XmlNode xmlNode = _node[text];
		if (xmlNode != null)
		{
			_node.RemoveChild(xmlNode);
			xmlNode = null;
		}
		if (PIDDFMECFAO.Count <= 0)
		{
			return;
		}
		xmlNode = _node.AppendElement(text);
		XmlNode mEEAKLDGLDF = xmlNode.AppendElement("Items");
		foreach (string item in PIDDFMECFAO)
		{
			mEEAKLDGLDF.AppendElement("Item").AppendAttribute("Name").Value = item;
		}
	}

	public void SavePaymentOrders()
	{
		paymentOrders.SaveToXml(paymentsNode);
	}

	public void AddLoadedQuestFile(string EIDDAFDJJCJ)
	{
		if (!HasLoadedQuestFile(EIDDAFDJJCJ))
		{
			loadedQuestFiles.Add(EIDDAFDJJCJ);
		}
	}

	public bool HasLoadedQuestFile(string EIDDAFDJJCJ)
	{
		foreach (string item in loadedQuestFiles)
		{
			if (item == EIDDAFDJJCJ)
			{
				return true;
			}
		}
		return false;
	}

	public bool GetGPlusAutoLogin()
	{
		return gplusAutoLogin && gplusFailedLogins < 1;
	}

	public void SetGPlusAutoLogin(bool value)
	{
		gplusAutoLogin = value;
		SetNodeAttribute("GPlusAutoLogin", gplusAutoLogin);
	}

	public void IncrementGPlusFailedLogins()
	{
		gplusFailedLogins++;
		SetGPlusFailedLogins(gplusFailedLogins);
	}

	public void ResetGPlusFailedLogins()
	{
		gplusFailedLogins = 0;
		SetGPlusFailedLogins(gplusFailedLogins);
	}

	public void SetAbGroupToggle(string name, bool FLOAHAOBNAP)
	{
	}

	public void RescaleCurrencyDenomination(int NPFOBKBJAOB = 0)
	{
		double num = Math.Pow(10.0, denominationDigits - NPFOBKBJAOB);
		long num2 = 0L;
		double num3 = (double)(ObscuredLong)(money) / num;
		if (num > 1.0)
		{
			num2 = (ObscuredLong)(money) % (long)num;
			if (0 < num2)
			{
				num3++;
			}
		}
		money = (ObscuredLong)((long)num3);
		num2 = 0L;
		double num4 = (double)(ObscuredLong)(paidMoney) / num;
		if (num > 1.0)
		{
			num2 = (ObscuredLong)(paidMoney) % (long)num;
		}
		if (0 < num2)
		{
			num4++;
		}
		paidMoney = (ObscuredLong)((long)num4);
	}

	public void SetCurrencyCount(string name, int value)
	{
		GameCurrency cJJOFMHLFFM = GameUtils.GameCurrencies.GetCurrencyByName(name);
		if (cJJOFMHLFFM == null)
		{
			cJJOFMHLFFM = new GameCurrency(name, name);
			GameUtils.GameCurrencies.AddCurrency(cJJOFMHLFFM);
		}
		SetCurrencyCount(cJJOFMHLFFM, value);
	}

	public void SetCurrencyCount(GameCurrency MDDNHLBDJBN, int value)
	{
		if (value < 0)
		{
			value = 0;
		}
		CurrencyStruct lAPFHLGNAAF = FindCurrency(MDDNHLBDJBN);
		if (lAPFHLGNAAF == null)
		{
			CurrencyStruct item = new CurrencyStruct(MDDNHLBDJBN, value);
			currencies.Add(item);
			lAPFHLGNAAF = currencies[currencies.Count - 1];
		}
		lAPFHLGNAAF.Count = (ObscuredInt)(value);
		string mENAJEAJJBE = lAPFHLGNAAF.Currency.Name;
		if (currenciesNode == null)
		{
			currenciesNode = _node.AppendElement("Currencies");
		}
		if (currenciesNode.Attributes[mENAJEAJJBE] == null)
		{
			currenciesNode.AppendAttribute(mENAJEAJJBE);
		}
		currenciesNode.Attributes[mENAJEAJJBE].Value = value.ToString();
		RequestSave();
		CallEvent(2, lAPFHLGNAAF);
	}

	public CurrencyStruct FindCurrency(string name)
	{
		foreach (CurrencyStruct item in currencies)
		{
			if (item.Currency.Name == name)
			{
				return item;
			}
		}
		return null;
	}

	public CurrencyStruct FindCurrency(GameCurrency MDDNHLBDJBN)
	{
		foreach (CurrencyStruct item in currencies)
		{
			if (item.Currency == MDDNHLBDJBN)
			{
				return item;
			}
		}
		return null;
	}

	public void AddCurrencyCount(string name, int value)
	{
		GameCurrency cJJOFMHLFFM = GameUtils.GameCurrencies.GetCurrencyByName(name);
		if (cJJOFMHLFFM == null)
		{
			cJJOFMHLFFM = new GameCurrency(name, name);
			GameUtils.GameCurrencies.AddCurrency(cJJOFMHLFFM);
		}
		if (cJJOFMHLFFM != null)
		{
			AddCurrencyCount(cJJOFMHLFFM, value);
		}
	}

	public void AddCurrencyCount(GameCurrency MDDNHLBDJBN, int value)
	{
		CurrencyStruct lAPFHLGNAAF = FindCurrency(MDDNHLBDJBN);
		int num = 0;
		if (lAPFHLGNAAF != null)
		{
			num = (ObscuredInt)(lAPFHLGNAAF.Count);
		}
		SetCurrencyCount(MDDNHLBDJBN, num + value);
	}

	public bool GetIsCurrencyExist(string name)
	{
		return FindCurrency(name) != null;
	}

	public int GetCurrencyCount(string name)
	{
		CurrencyStruct lAPFHLGNAAF = FindCurrency(name);
		if (lAPFHLGNAAF != null)
		{
			return (ObscuredInt)(lAPFHLGNAAF.Count);
		}
		return 0;
	}

	public int GetResistanceCount(string name)
	{
		ResistanceStruct lONBLHKCFDH = FindResistance(name);
		if (lONBLHKCFDH != null)
		{
			return (ObscuredInt)(lONBLHKCFDH.Count);
		}
		return 0;
	}

	public string GetMarketCode()
	{
		MarketSettings cAGGDFBMJKG = AssemblyController.GetMarket();
		if (cAGGDFBMJKG.GetIsAmazonMarket() || cAGGDFBMJKG.GetIsAmazonMobileMarket())
		{
			return "ama";
		}
		if (cAGGDFBMJKG.GetIsChinaMarket())
		{
			return "chn";
		}
		if (cAGGDFBMJKG.GetIsKoreaMarket())
		{
			return "kak";
		}
		return SystemProperties.GetPlatformName();
	}

	public void SetAvatar(string FHLFEBDNIFF)
	{
		if (get_Parameters() != null)
		{
			get_Parameters().Avatar = FHLFEBDNIFF;
		}
		SetNodeAttribute("Avatar", FHLFEBDNIFF);
	}

	public void ApplyLanguage()
	{
		LocalizationManager.Language pPNFBAFOOAH = null;
		if (language == string.Empty)
		{
			string eOMNCDDELLB = SystemProperties.GetDeviceInfo().GetLanguage();
			pPNFBAFOOAH = LocalizationManager.FindLanguageByLocale(eOMNCDDELLB);
			if (pPNFBAFOOAH == null)
			{
				string pOIPGLLCCKC = LocalizationManager.DefaultLanguageName;
				pPNFBAFOOAH = LocalizationManager.FindLanguageByName(pOIPGLLCCKC);
			}
		}
		else
		{
			pPNFBAFOOAH = LocalizationManager.FindLanguageByName(language);
		}
		if (pPNFBAFOOAH == null)
		{
			GameLog.Error("Roster parse - null language");
			return;
		}
		if (!LocalizationManager.HasAllFonts(pPNFBAFOOAH))
		{
			string pOIPGLLCCKC2 = LocalizationManager.DefaultLanguageName;
			pPNFBAFOOAH = LocalizationManager.FindLanguageByName(pOIPGLLCCKC2);
		}
		SetLanguage(pPNFBAFOOAH.name);
		LocalizationManager.ChangeLanguage(pPNFBAFOOAH);
	}

	private void LoadLevelThresholds()
	{
		List<global::Pair<int, uint>> pEDIMBMABIG = GameUtils.LevelThresholdTable.Thresholds;
		foreach (global::Pair<int, uint> item2 in pEDIMBMABIG)
		{
			ObscuredUInt item = (ObscuredUInt)(item2.Second);
			_levelThresholds.Add(item);
		}
	}

	public string GetCurrentZone()
	{
		if (currentZone == string.Empty)
		{
			SetCurrentZone(ComputeCurrentZone());
		}
		return currentZone;
	}

	public void SetCurrentZone(string value)
	{
		currentZone = value;
		SetNodeAttribute("CurrentZone", currentZone);
	}

	public void RandomizeObscuredVars()
	{
		money.RandomizeCryptoKey();
		paidMoney.RandomizeCryptoKey();
		bonus.RandomizeCryptoKey();
		paidBonus.RandomizeCryptoKey();
		_experience.RandomizeCryptoKey();
		_power.RandomizeCryptoKey();
		_levelThresholds.ForEach((ObscuredUInt DHDMNHCIPEH) =>
		{
			DHDMNHCIPEH.RandomizeCryptoKey();
		});
	}

	public string ComputeCurrentZone()
	{
		int num = 0;
		foreach (RosterBattle item in battles)
		{
			if (item.IsLocked())
			{
				continue;
			}
			bool flag = false;
			foreach (RosterFight item2 in fights)
			{
				if (item2.GetBattleName() == item.GetBattleId().GetBattle())
				{
					flag = true;
					int num2 = item.LinkedBattle.GetZone().GetIndex();
					if (num2 > num)
					{
						num = num2;
					}
				}
				if (flag)
				{
					continue;
				}
				int num3 = item.LinkedBattle.GetFightCount();
				if (num3 > 0)
				{
					int num4 = item.LinkedBattle.GetZone().GetIndex();
					if (num4 > num)
					{
						num = num4;
					}
				}
			}
		}
		if (num < ListSF.GetZones().Count)
		{
			Zone pKCPOJKLMOK = ListSF.GetZones()[num];
			return pKCPOJKLMOK.get_Name();
		}
		return "ZONE_1";
	}

	private void SetGPlusFailedLogins(int count)
	{
		gplusFailedLogins = count;
		SetNodeAttribute("GPlusFiledLogins", gplusFailedLogins);
	}

	private void LoadSoundSettings(XmlNode node)
	{
		Sound.SetPaths(soundsPath, musicPath);
		XmlNode xmlNode = node["Sounds"];
		if (xmlNode != null)
		{
			XmlNode xmlNode2 = xmlNode["Sound"];
			if (xmlNode2 != null)
			{
				Sound.SetSoundVolume(xmlNode2.Attributes["Value"].ParseFloat(1f));
				Sound.SetSoundMuted(xmlNode2.Attributes["Mute"].ParseBool());
			}
			else
			{
				Debug.LogWarning("[UserData] Warrior Sound settings missing; using default sound volume.");
				Sound.SetSoundVolume(1f);
				Sound.SetSoundMuted(false);
			}
			xmlNode2 = xmlNode["Music"];
			if (xmlNode2 != null)
			{
				Sound.SetMusicVolume(xmlNode2.Attributes["Value"].ParseFloat(1f));
				Sound.SetMusicMuted(xmlNode2.Attributes["Mute"].ParseBool());
			}
			else
			{
				Debug.LogWarning("[UserData] Warrior Music settings missing; using default music volume.");
				Sound.SetMusicVolume(1f);
				Sound.SetMusicMuted(false);
			}
			ListSF.IsSoundEnabled = Sound.GetMusicMuted();
		}
		else
		{
			// Older/default desktop saves may not contain the per-warrior copy of
			// these settings.  The values below are the runtime defaults, so this is
			// recoverable and must not abort a Windows Editor playtest.
			Debug.LogWarning("[UserData] Warrior Sounds section missing; using default volumes.");
			Sound.SetSoundVolume(1f);
			Sound.SetSoundMuted(false);
			Sound.SetMusicVolume(1f);
			Sound.SetMusicMuted(false);
		}
		SoundController.ApplySavedVolumes();
	}

	private void ParseCurrencies(XmlNode HBKKCEFLPPE)
	{
		List<GameCurrency> list = GameUtils.GameCurrencies.GetCurrencies();
		foreach (GameCurrency item2 in list)
		{
			XmlAttribute cJBEMNNNHDM = HBKKCEFLPPE.Attributes[item2.Name];
			if (cJBEMNNNHDM.Empty())
			{
				HBKKCEFLPPE.AppendAttribute(item2.Name).Value = "0";
			}
			CurrencyStruct item = new CurrencyStruct(item2, cJBEMNNNHDM.ParseInt());
			currencies.Add(item);
		}
	}

	private ResistanceStruct FindResistance(string name)
	{
		foreach (ResistanceStruct item in resistances)
		{
			if (item.resistance.Name == name)
			{
				return item;
			}
		}
		return null;
	}

	public bool GetRaidRemindRandomRule()
	{
		return raidRemindRandomRule;
	}

	public void SetRaidRemindRandomRule(bool value)
	{
		raidRemindRandomRule = value;
		SetNodeAttribute("RaidRemindRandomRule", raidRemindRandomRule);
	}
}
