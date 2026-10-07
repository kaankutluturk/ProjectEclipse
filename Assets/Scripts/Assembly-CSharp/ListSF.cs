using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;
using Nekki.SF2.Core;
using Nekki.SF2.Core.Quests;
using Nekki.SF2.GUI.Menu;
using Nekki.Utils;
using UnityEngine;

public class ListSF
{
	public enum CheckItemType
	{
		CHECK_ITEM_NONE = 0,
		CHECK_ITEM_LEVEL = 1,
		CHECK_ITEM_MONEY = 2,
		CHECK_ITEM_BONUS = 3,
		CHECK_ITEM_NO_NETWORK = 4,
		CHECK_ITEM_MATERIALS = 5
	}

	public class CheckItems
	{
		public CheckItemType Type;

		public long Value;
	}

	public class TemplateUser
	{
		public ModelParameters Parameters;

		public XmlNode node;

		public string ParentTemplateName;

		public string name;
	}

	private class GroupsUser
	{
		public List<ModelParameters> Models = new List<ModelParameters>();

		public XmlNode node;

		public string name;

		public string TemplateName;

		public int Size
		{
			get
			{
				return GetCount();
			}
		}

		public ModelParameters GetRandomModel()
		{
			int index = NekkiMath.randomInt(Models.Count);
			return Models[index];
		}

		public ModelParameters GetRandomModel(List<int> AOKENODDIEN)
		{
			int num = NekkiMath.randomInt(Models.Count);
			bool flag;
			do
			{
				flag = false;
				foreach (int item in AOKENODDIEN)
				{
					if (item == num)
					{
						num = NekkiMath.randomInt(Models.Count);
						flag = true;
					}
				}
			}
			while (flag);
			AOKENODDIEN.Add(num);
			return Models[num];
		}

		public int GetCount()
		{
			return Models.Count;
		}
	}

	private static ListSF _instance = null;

	private static Roster _roster = null;

	private static Items _items = new Items();

	private QuestsManager _QuestsManager = QuestsManager.get_Instance();

	public const string CurrencyCoinsKey = "Coins";

	public const string CurrencyRubyKey = "Ruby";

	public const string ConnectionErrorKey = "Connection";

	public const string VerificationErrorKey = "Verification";

	public const string ServerNoResponseErrorKey = "ServerNoResponse";

	public const string MaterialsErrorKey = "Materials";

	public const string TryAgainKey = "TryAgain";

	public static bool UnusedStaticFlag;

	private IEnumerator authTimeoutRoutine;

	public QuestParameters LotteryQuestParameters = new QuestParameters();

	public AdvertConfig AdvertSettings = new AdvertConfig();

	public bool IsContentLoaded;

	private List<ModelParameters> mergedGroupModels = new List<ModelParameters>();

	private List<KeyValuePair<string, BattleType>> battleTypeNames = new List<KeyValuePair<string, BattleType>>();

	private static bool isServerAuthPending;

	private static bool isServerAuthFailed;

	private static bool isLoginStarted;

	private static bool isAuthorizeStarted;

	private static bool areZonesLoaded;

		private XmlDocument userDocument;

    // Lifetime ownership of the loaded document, retained even after returning
    // to the title so a delayed callback cannot persist a discarded local profile.
    private bool _localVersusProfile;

	private XmlNode _CurrentUserNode;

	private XmlDocument stagesDocument;

	private XmlDocument warriorsDocument;

	private static long currentTime;

	private long unusedTimestamp;

	private List<TemplateUser> templates = new List<TemplateUser>();

	private List<GroupsUser> warriorGroups = new List<GroupsUser>();

	private List<Zone> zones = new List<Zone>();

	private List<Zone> secondaryZones = new List<Zone>();

	private List<Zone> tertiaryZones = new List<Zone>();

	private List<Battle> _battles = new List<Battle>();

	private List<FightList> fights = new List<FightList>();

	private static List<BattleReplayable> replayableBattles = new List<BattleReplayable>();

	public static bool IsSoundEnabled = false;

	private bool isSaveRequested;

	private static bool unusedFlag;

	public static string UsersFilePath
	{
		get
		{
			return GetUsersFilePath();
		}
	}

	public static string UsersBackupFilePath
	{
		get
		{
			return GetUsersBackupFilePath();
		}
	}

	public static string LocalSettingsFilePath
	{
		get
		{
			return GetLocalSettingsFilePath();
		}
	}

	public static string PacksFilePath
	{
		get
		{
			return GetPacksFilePath();
		}
	}

	public static ListSF Instance
	{
		get
		{
			return GetInstance();
		}
	}

	public static Roster CurrentRoster
	{
		get
		{
			return GetRoster();
		}
	}

	public static Items ItemCatalog
	{
		get
		{
			return GetItems();
		}
	}

	public static ModelParameters PlayerParameters
	{
		get
		{
			return GetPlayerParameters();
		}
	}

	public static long ServerTime
	{
		get
		{
			return GetServerTime();
		}
	}

	public static Zone CurrentZone
	{
		get
		{
			return GetCurrentZone();
		}
	}

	private int CurrentUserId
	{
		get
		{
			return GetCurrentUserId();
		}
	}

	public List<Battle> Battles
	{
		get
		{
			return GetBattles();
		}
	}

	public static string GetUsersFilePath()
	{
		return string.Format("{0}/{1}", SF2Paths.GetUserDataDirectory(), Constants.UsersFileName);
	}

	public static string GetUsersBackupFilePath()
	{
		return string.Format("{0}/{1}", SF2Paths.GetUserDataDirectory(), Constants.UsersBackupFileName);
	}

	public static string GetLocalSettingsFilePath()
	{
		return string.Format("{0}/{1}", SF2Paths.GetUserDataDirectory(), Constants.LocalSettingsPath);
	}

	public static string GetPacksFilePath()
	{
		return string.Format("{0}/{1}", SF2Paths.GetUserDataDirectory(), "assets/packs.xml");
	}

	// best guess for name
	public static ListSF GetInstance()
	{
		if (_instance == null)
		{
			_instance = new ListSF();
		}
		return _instance;
	}

	public static Roster GetRoster()
	{
		return _roster;
	}

	// best guess for name
	public static Items GetItems()
	{
		return _items;
	}

	public static void Reset()
	{
		Eclipse.Modding.ModRuntime.UnbindProfile();
		if (_instance != null)
			GlobalTimer.get_Instance().removeEventListener(0, _instance.OnTimerTick);
		_instance = null;
		_roster = null;
		_items = new Items();
		QuestsManager.Reset();
	}

	// Eclipse: when set, receives each load step's name and duration (the title's preview
	// load reports where its time goes).
	internal static System.Action<string, long> StepTimer;

	private static void TimeStep(string name, System.Action step)
	{
		if (StepTimer == null) { step(); return; }
		var watch = System.Diagnostics.Stopwatch.StartNew();
		step();
		StepTimer(name, watch.ElapsedMilliseconds);
	}

	// Eclipse: the part of IIKDNMBIHCM the title's sparring fighters need (items with the
	// enabled mods' content, which often changes how fighters look, and a profile), without
	// warriors, zones, quests or the roster timer. Campaign entry retains this content
	// and replaces the sandbox profile before completing the remaining steps.
	private bool _titleContentReady;
	internal static bool CanResumeTitlePreview => _instance != null && _instance._titleContentReady;

	internal void LoadTitlePreview()
	{
		TimeStep("battle types", InitBattleTypes);
		TimeStep("conditions", () => GameUtils.ModeCounters.InitConditions());
		TimeStep("items", LoadItems);
		TimeStep("mod content", () => Eclipse.Modding.ModRuntime.StartGameContent());
		TimeStep("profile", LoadProfile);
		// Prices must remain in their source denomination until a real profile loads.
		_titleContentReady = true;
	}

	internal void DetachTitleProfile()
	{
		Eclipse.Modding.ModRuntime.UnbindProfile();
		if (_roster != null) _roster.RemoveEventListener(0, RequestSave);
		_roster = null;
		_CurrentUserNode = null;
		userDocument = null;
		isSaveRequested = false;
	}

	internal void ResumeTitlePreview()
	{
		if (!_titleContentReady) throw new InvalidOperationException("Title content is not available.");
		_titleContentReady = false;
		TimeStep("profile", LoadProfile);
		TimeStep("item denominations", () => ItemInfo.DenominateItems());
		CompleteGameContentLoad();
	}

	public void LoadGameContent()
	{
		TimeStep("battle types", InitBattleTypes);
		TimeStep("conditions", () => GameUtils.ModeCounters.InitConditions());
		TimeStep("items", LoadItems);
		TimeStep("mod content", () => Eclipse.Modding.ModRuntime.StartGameContent());
		TimeStep("profile", LoadProfile);
		TimeStep("item denominations", () => ItemInfo.DenominateItems());
		CompleteGameContentLoad();
	}

	private void CompleteGameContentLoad()
	{
		TimeStep("warriors", LoadWarriors);
		TimeStep("zones", LoadZones);
		TimeStep("mod stages", () => Eclipse.Modding.ModRuntime.ApplyStageContent());
		IsContentLoaded = true;
		TimeStep("quests", () => LoadQuests(string.Empty));
		TimeStep("mod quests", () => Eclipse.Modding.ModRuntime.ApplyQuestContent());
		TimeStep("periodic battles", InitPeriodicBattles);
		TimeStep("packs", () => PacksController.GetInstance().LoadPacks());
		stagesDocument = null;
		GlobalTimer.get_Instance().addEventListener(0, OnTimerTick);
	}

	public void RestartServerAuthorization()
	{
		isAuthorizeStarted = false;
		AuthorizeWithServer();
	}

	public void StartSocialLogin()
	{
		bool flag = true;
		bool flag2 = true;
		GameLog.Info("Login sequence: initSocial");
		if (flag)
		{
			isAuthorizeStarted = false;
			if (_roster.GetTrySocialLogin())
			{
				if ((_roster.GetGPlusAutoLogin() && !SystemProperties.IsMetroArmPlatform()) || GameCenterController.GetCanAutoSignIn())
				{
					Debug.Log("Login sequence: GameCenterController.SignIn");
					StartAuthTimeout();
					GameCenterAbstract.OnAuthenticate = (Action<bool>)Delegate.Combine(GameCenterAbstract.OnAuthenticate, new Action<bool>(OnAuthenticate));
					GameCenterController.SignIn();
				}
				else
				{
					Debug.Log("Login sequence: skip social");
					AuthorizeWithServer();
				}
			}
			else
			{
				Debug.Log("Login sequence: skip social");
				AuthorizeWithServer();
			}
		}
		if (!flag2)
		{
		}
	}

	public static ItemInfo FindAvailableItemByGroup(string GLLLLNHKCOF, long HNPMAENBMCO = 0L)
	{
		List<ItemInfo> list = new List<ItemInfo>();
		List<ItemInfo> list2 = GetItems().GetAllItems();
		foreach (ItemInfo item in list2)
		{
		}
		Roster nKGLHEGIKKP = GetRoster();
		if (nKGLHEGIKKP != null)
		{
			foreach (ItemInfo item2 in list)
			{
				if (item2.GroupId == string.Empty || nKGLHEGIKKP.HasShopLock(item2.GroupId))
				{
					return item2;
				}
			}
		}
		return null;
	}

	public static void ApplyRealMoneyPurchase(ItemInfo FAKOMBAIFPP)
	{
		UserItem dKCHDHMLKHN = ((FAKOMBAIFPP == null) ? null : GetUserItem(FAKOMBAIFPP.Name));
		if (FAKOMBAIFPP != null && dKCHDHMLKHN == null)
		{
			Roster aNEHEDFAPCH = _roster;
			aNEHEDFAPCH.SetTotalPaymentSum(aNEHEDFAPCH.GetTotalPaymentSum() + FAKOMBAIFPP.PriceAmountText.ToFloat());
			Roster aNEHEDFAPCH2 = _roster;
			aNEHEDFAPCH2.SetPaymentCount(aNEHEDFAPCH2.GetPaymentCount() + 1);
			if (FAKOMBAIFPP.IsPaid)
			{
				Roster aNEHEDFAPCH3 = _roster;
				aNEHEDFAPCH3.SetPaidMoney((ObscuredLong)((ObscuredLong)(aNEHEDFAPCH3.GetPaidMoney()) + (ObscuredLong)(FAKOMBAIFPP.ReceiveGold)));
			}
			_roster.SetMoney(_roster.GetMoney() + (ObscuredLong)(FAKOMBAIFPP.ReceiveGold));
			_roster.SetBonus(_roster.GetBonus() + (ObscuredLong)(FAKOMBAIFPP.ReceiveBonus), Roster.BalanceChangeType.CHANGE_PAYMENT, true);
			if (MainMenu.get_Instance() != null)
			{
				MainMenu.get_Instance().UpdateMoney();
			}
			switch (FAKOMBAIFPP.SubType)
			{
			case "StarterPack":
				AddItem(FAKOMBAIFPP, 1, 0L, false);
				break;
			case "UnlimitedEnergy":
				AddItem(FAKOMBAIFPP, 1, 0L, false);
				break;
			case "PerkReset":
				AddItem(FAKOMBAIFPP, 1, 0L, false);
				_roster.GetPerks().ResetPerks();
				break;
			}
			_roster.RequestSave(true);
		}
	}

	public static ItemInfo FindItemByMarketId(string FDKNIPNGFNF)
	{
		List<ItemInfo> list = GetItems().GetItemsByMarketId(FDKNIPNGFNF);
		if (list.Count > 0 && _roster != null)
		{
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (string.IsNullOrEmpty(list[i].GroupId) || _roster.HasShopLock(list[i].GroupId))
				{
					return list[i];
				}
			}
		}
		return null;
	}

	public List<ModelParameters> CreateOpponentParameters(ModelParameters KKNOCIPBIIK, int count)
	{
		List<ModelParameters> list = new List<ModelParameters>();
		if (!KKNOCIPBIIK.GetIsGroup())
		{
			for (int i = 0; i < count; i++)
			{
				list.Add(KKNOCIPBIIK.Clone());
			}
		}
		else
		{
			int num = 0;
			List<List<int>> list2 = new List<List<int>>();
			list2.Resize(KKNOCIPBIIK.GroupModels.Count);
			for (int j = 0; j < count; j++)
			{
				int num2 = 0;
				ModelParameters kIKOGDEPGHB = null;
				foreach (GroupModel item in KKNOCIPBIIK.GroupModels)
				{
					GroupsUser oFIHDFNGDLA = GetGroupByName(item.Name);
					if (oFIHDFNGDLA != null)
					{
						ModelParameters kIKOGDEPGHB2 = null;
						if (!item.IsRandom)
						{
							int index = j % oFIHDFNGDLA.GetCount();
							kIKOGDEPGHB2 = oFIHDFNGDLA.Models[index];
						}
						else if (!item.NoDoubles)
						{
							kIKOGDEPGHB2 = oFIHDFNGDLA.GetRandomModel();
						}
						else
						{
							kIKOGDEPGHB2 = oFIHDFNGDLA.GetRandomModel(list2[num2]);
							int count2 = oFIHDFNGDLA.Models.Count;
							if (num >= count2 - 1)
							{
								list2[num2].Clear();
								num = 0;
							}
							else
							{
								num++;
							}
						}
						if (kIKOGDEPGHB2 != null)
						{
							if (kIKOGDEPGHB == null)
							{
								kIKOGDEPGHB = kIKOGDEPGHB2;
							}
							else
							{
								kIKOGDEPGHB = ParseWarriorParameters(kIKOGDEPGHB2.Node, kIKOGDEPGHB);
								AddMergedGroupModel(kIKOGDEPGHB);
							}
						}
					}
					num2++;
				}
				if (kIKOGDEPGHB != null)
				{
					ModelParameters kIKOGDEPGHB3 = null;
					if (KKNOCIPBIIK.Node.Attributes["Template"] == null)
					{
						kIKOGDEPGHB3 = ParseWarriorParameters(KKNOCIPBIIK.Node, kIKOGDEPGHB);
					}
					else
					{
						kIKOGDEPGHB3 = ((kIKOGDEPGHB == null) ? new ModelParameters() : kIKOGDEPGHB.Clone());
						ApplyParameterOverrides(kIKOGDEPGHB3, KKNOCIPBIIK);
					}
					list.Add(kIKOGDEPGHB3);
				}
			}
		}
		if (list.Count == 0)
		{
			list.Add(KKNOCIPBIIK.Clone());
		}
		return list;
	}

	public static ModelParameters GetPlayerParameters()
	{
		return GetRoster().get_Parameters();
	}

	public static long GetCurrentTime()
	{
		return currentTime;
	}

	public static long GetServerTime()
	{
		return 0L;
	}

	public static CheckItems CheckItemPurchase(ItemInfo item, ItemAction LFLGCDNKNJI, int count = 1)
	{
		if (item == null || count <= 0) return new CheckItems { Value = -1L };
		if (!HasPurchaseCapacity(item, LFLGCDNKNJI, count)) return new CheckItems { Value = -1L };
		GetInstance().VerifyRosterBalance();
		CheckItems bJEBPDNMNAE = new CheckItems();
		Roster nKGLHEGIKKP = GetRoster();
		long num = 0L;
		long num2 = 0L;
		bool flag = true;
		switch (LFLGCDNKNJI)
		{
		case ItemAction.Item_Buy_Gold:
		case ItemAction.Item_Upgrade_Gold:
			num = nKGLHEGIKKP.GetMoney();
			num2 = CalculatePurchaseTotal(item.GetCoinPrice(), count);
			break;
		case ItemAction.Item_Buy_Ruby:
		case ItemAction.Item_Upgrade_Ruby:
			num = nKGLHEGIKKP.GetBonus();
			num2 = CalculatePurchaseTotal(item.GetGemPrice(), count);
			break;
		case ItemAction.Item_Buy_Real:
		case ItemAction.Item_Free:
			num = 0L;
			num2 = 0L;
			break;
		case ItemAction.Item_Order_Ruby:
		case ItemAction.Item_Delivery_Ruby:
		case ItemAction.Item_Recipe_Delivery_Ruby:
			num = nKGLHEGIKKP.GetBonus();
			num2 = (ObscuredLong)(item.DeliveryGemPrice);
			break;
		case ItemAction.Item_Consumable:
			num = nKGLHEGIKKP.GetBonus();
			num2 = CalculatePurchaseTotal(item.GetGemPrice(), count);
			break;
		case ItemAction.Item_Recipe:
		{
			RecipeItemInfo bNJOCBKNPMG = (RecipeItemInfo)item;
			Recipe iNODIOJPNJH = bNJOCBKNPMG.GetRecipe();
			UserItem nDMCFNGEPOA = bNJOCBKNPMG.GetUserItem();
			flag = iNODIOJPNJH.IsAvailableWithMaterials(nDMCFNGEPOA);
			break;
		}
		default:
			GameLog.Error("ListSF::isBuyItem - unknown type: %i", LFLGCDNKNJI);
			break;
		}
		// Invalid/overflowing totals must not wrap into an affordable price or balance.
		if (num < 0 || num2 < 0) return new CheckItems { Value = -1L };
		long num3 = num - num2;
		bJEBPDNMNAE.Type = CheckItemType.CHECK_ITEM_NONE;
		bJEBPDNMNAE.Value = num3;
		if ((LFLGCDNKNJI == ItemAction.Item_Buy_Real || LFLGCDNKNJI == ItemAction.Item_Free) && !SystemProperties.CheckConnection())
		{
			bJEBPDNMNAE.Type = CheckItemType.CHECK_ITEM_NO_NETWORK;
			bJEBPDNMNAE.Value = -1L;
		}
		else if (item.ItemLevel > nKGLHEGIKKP.GetLevel())
		{
			bJEBPDNMNAE.Type = CheckItemType.CHECK_ITEM_LEVEL;
			bJEBPDNMNAE.Value = -1L;
		}
		else if (num3 < 0)
		{
			bJEBPDNMNAE.Type = ((LFLGCDNKNJI != ItemAction.Item_Buy_Gold && LFLGCDNKNJI != ItemAction.Item_Upgrade_Gold) ? CheckItemType.CHECK_ITEM_BONUS : CheckItemType.CHECK_ITEM_MONEY);
			bJEBPDNMNAE.Value = -1L;
		}
		else if (num2 < 0)
		{
			bJEBPDNMNAE.Type = CheckItemType.CHECK_ITEM_NONE;
			bJEBPDNMNAE.Value = -1L;
		}
		else if (LFLGCDNKNJI == ItemAction.Item_Recipe && !flag)
		{
			bJEBPDNMNAE.Type = CheckItemType.CHECK_ITEM_MATERIALS;
			bJEBPDNMNAE.Value = -1L;
		}
		return bJEBPDNMNAE;
	}

	internal static long CalculatePurchaseTotal(long unitPrice, int count)
	{
		if (unitPrice < 0 || count <= 0 || unitPrice > long.MaxValue / count) return -1L;
		return unitPrice * count;
	}

	internal static bool CanIncrementItemCount(int existingCount, int count)
	{
		return existingCount >= 0 && count > 0 && count <= int.MaxValue - existingCount;
	}

	private static bool HasPurchaseCapacity(ItemInfo item, ItemAction action, int count)
	{
		// Upgrade/delivery routes do not acquire another copy of the base item.
		if (item.ParentItem != null || (action != ItemAction.Item_Buy_Gold &&
			action != ItemAction.Item_Buy_Ruby && action != ItemAction.Item_Consumable)) return true;
		UserItem existing = GetUserItem(item.Name);
		return CanIncrementItemCount(existing == null ? 0 : existing.GetCount(), count);
	}

	public static bool BuyItem(ItemInfo item, ItemAction LFLGCDNKNJI, long FLCBMGGIDDA, int count = 1, Action<object> callback = null)
	{
		if (item == null || count <= 0) return false;
		if (!HasPurchaseCapacity(item, LFLGCDNKNJI, count)) return false;
		if (LFLGCDNKNJI == ItemAction.Item_Buy_Gold || LFLGCDNKNJI == ItemAction.Item_Buy_Ruby || LFLGCDNKNJI == ItemAction.Item_Consumable)
			return Eclipse.Modding.ModRuntime.SettleItemPurchase(item, count,
				() => ApplyShopPurchase(item, LFLGCDNKNJI, FLCBMGGIDDA, count, callback));
		return ApplyShopPurchase(item, LFLGCDNKNJI, FLCBMGGIDDA, count, callback);
	}

	private static bool ApplyShopPurchase(ItemInfo item, ItemAction LFLGCDNKNJI, long FLCBMGGIDDA, int count, Action<object> callback)
	{
		if (item != null)
		{
			Roster nKGLHEGIKKP = GetRoster();
			long bMNFPNBAMAF = -1L;
			if (item.Type == "RaidItemPack")
			{
			}
			switch (LFLGCDNKNJI)
			{
			case ItemAction.Item_Buy_Gold:
			case ItemAction.Item_Upgrade_Gold:
				RecordCoinShortfall(item);
				nKGLHEGIKKP.SetMoney(FLCBMGGIDDA);
				// The offline runtime completes shop orders immediately; do not create
				// delivery timestamps that depend on the retired server timer.
				bMNFPNBAMAF = -1L;
				break;
			case ItemAction.Item_Buy_Ruby:
			case ItemAction.Item_Upgrade_Ruby:
				RecordGemShortfall(item, false);
				nKGLHEGIKKP.SetBonus(FLCBMGGIDDA, Roster.BalanceChangeType.CHANGE_BUY_ITEM);
				break;
			case ItemAction.Item_Buy_Real:
				return BuyRealMoneyItem(item);
			case ItemAction.Item_Free:
				return BuyFreeItem(item);
			case ItemAction.Item_Consumable:
				RecordGemShortfall(item, false);
				OnConsumablePurchased(item, FLCBMGGIDDA);
				break;
			case ItemAction.Item_Delivery_Ruby:
			case ItemAction.Item_Recipe_Delivery_Ruby:
				RecordGemShortfall(item, true);
				nKGLHEGIKKP.SetBonus(FLCBMGGIDDA, Roster.BalanceChangeType.CHANGE_BUY_DELIVERY);
				bMNFPNBAMAF = 0L;
				break;
			default:
				GameLog.Error("ListSF::buyItem - unknown type: %i", LFLGCDNKNJI);
				break;
			}
			if (LFLGCDNKNJI == ItemAction.Item_Recipe_Delivery_Ruby)
			{
				CompleteRecipeDelivery((RecipeItemInfo)item, LFLGCDNKNJI, FLCBMGGIDDA, bMNFPNBAMAF);
			}
			else
			{
				CompleteItemPurchase(item, LFLGCDNKNJI, FLCBMGGIDDA, bMNFPNBAMAF, count);
			}
			GameUtils.TrackEvent("Virtual Good Purchased");
			MenuController.RefreshMoney();
			PlayPurchaseSound(LFLGCDNKNJI);
		}
		return true;
	}

	public static bool CompleteItemPurchase(ItemInfo item, ItemAction LFLGCDNKNJI, long FLCBMGGIDDA, long BMNFPNBAMAF, int count = 1)
	{
		Roster nKGLHEGIKKP = GetRoster();
		UserItem dKCHDHMLKHN = nKGLHEGIKKP.GetInventory().FindItem(item.Name);
		long bMNFPNBAMAF = ((dKCHDHMLKHN == null) ? (-1) : dKCHDHMLKHN.GetDeliveryTimestamp());
		bool flag = BMNFPNBAMAF <= 0 || item.DeliveryTime == 0;
		UserItem dKCHDHMLKHN2 = AddItem(item, count, BMNFPNBAMAF, flag);
		bool jBCMFEPAKLK = dKCHDHMLKHN2.GetIsOwned() || flag;
		if (item.Type == "RealMoneyItem" || item.Type == "Consumable")
		{
			jBCMFEPAKLK = false;
		}
		UseItem(dKCHDHMLKHN2, jBCMFEPAKLK);
		GameUtils.TrackItemAction(item, LFLGCDNKNJI, count, bMNFPNBAMAF);
		QuestParameters hHKLFIIBIFF = GetInstance().GetQuestParameters();
		FightIDS jLGLBLDPAAF = hHKLFIIBIFF.fightIds;
		hHKLFIIBIFF.fightIds = FightIDS.Empty();
		hHKLFIIBIFF.fightResult = string.Empty;
		hHKLFIIBIFF.purchasedItem = dKCHDHMLKHN2.GetInfo();
		if (GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE))
		{
			GetInstance().RunQuestActions();
		}
		hHKLFIIBIFF.fightIds = jLGLBLDPAAF;
		return true;
	}

	public static bool CompleteRecipeDelivery(RecipeItemInfo AJBJAPPEAFH, ItemAction LFLGCDNKNJI, long FLCBMGGIDDA, long BMNFPNBAMAF, int count = 1)
	{
		if (AJBJAPPEAFH == null || LFLGCDNKNJI != ItemAction.Item_Recipe_Delivery_Ruby) return false;
		return ApplyRecipeToItem(AJBJAPPEAFH);
	}

	public static bool TryEnchantItem(UserItem userItem, Recipe recipe)
	{
		if (userItem == null || recipe == null) return false;
		RecipeItemInfo recipeItem;
		return ForgeManager.GetInstance().StartEnchant(userItem, recipe, out recipeItem);
	}

	public static bool ApplyRecipeToItem(RecipeItemInfo recipeItem)
	{
		Roster roster = GetRoster();
		return roster != null && roster.GetInventory().FinishDeliveryRecipe(recipeItem);
	}

	public static bool UseItem(UserItem NDMCFNGEPOA, bool JBCMFEPAKLK, bool CMDMHFKJBHB = true)
	{
		if (NDMCFNGEPOA == null)
		{
			return false;
		}
		NDMCFNGEPOA.RefreshUpgradeState(GetRoster().GetLevel());
		bool aNNCECNAEPN = NDMCFNGEPOA.GetInfo().SpendAfterUse;
		if (aNNCECNAEPN)
		{
			SpendItem(NDMCFNGEPOA, NDMCFNGEPOA.GetCount());
		}
		JBCMFEPAKLK = JBCMFEPAKLK && !aNNCECNAEPN;
		GameLog.Write("   UseItem: " + ((!JBCMFEPAKLK) ? "Unequiped " : "Equiped ") + NDMCFNGEPOA.get_Name());
		NDMCFNGEPOA.SetIsEquipped(JBCMFEPAKLK);
		if (CMDMHFKJBHB)
		{
			return GetRoster().EquipItem(NDMCFNGEPOA, JBCMFEPAKLK);
		}
		return true;
	}

	public static void EquipFallbackItem(ItemInfo item)
	{
		if (item == null)
		{
			return;
		}
		string kKJHFNGGFCG = item.Type;
		string mDPPNGIEJGD = item.SubType;
		switch (kKJHFNGGFCG)
		{
		case "Weapon":
		{
			UserItem nDMCFNGEPOA2 = GetUserItem("Fists");
			UseItem(nDMCFNGEPOA2, true);
			break;
		}
		case "Armor":
		{
			UserItem nDMCFNGEPOA6 = GetUserItem("Body");
			UseItem(nDMCFNGEPOA6, true);
			break;
		}
		case "Helm":
		{
			UserItem nDMCFNGEPOA5 = GetUserItem("Head");
			UseItem(nDMCFNGEPOA5, true);
			break;
		}
		case "Ranged":
		{
			UserItem nDMCFNGEPOA4 = GetUserItem("NoRanged");
			UseItem(nDMCFNGEPOA4, true);
			break;
		}
		case "Magic":
		{
			UserItem nDMCFNGEPOA3 = GetUserItem("NoMagic");
			UseItem(nDMCFNGEPOA3, true);
			break;
		}
		case "RaidConsumable":
			if (mDPPNGIEJGD == "RaidCharge")
			{
				UserItem nDMCFNGEPOA = GetUserItem("NoRaidCharge");
				UseItem(nDMCFNGEPOA, true);
			}
			break;
		}
	}

	public static bool SpendItem(UserItem item, int count = 1)
	{
		if (count > item.GetCount())
		{
			count = item.GetCount();
		}
		ItemInfo.SpendType mEFIBHIDOLA = ItemInfo.SpendType.SPEND_TYPE_NONE;
		if (item.GetInfo().Type == "Energy")
		{
			mEFIBHIDOLA = ItemInfo.SpendType.SPEND_TYPE_ENERGY;
		}
		if (mEFIBHIDOLA == ItemInfo.SpendType.SPEND_TYPE_ENERGY)
		{
			_roster.RefillPower();
			item.SetCount(item.GetCount() - count);
			return true;
		}
		GameLog.Write("Error - no such SpendItemType");
		return false;
	}

	public static void ClearEquippedSlot(ItemInfo PJDAGCBPLJE)
	{
		GetRoster().UnequipItem(PJDAGCBPLJE);
	}

	public static void UnequipOtherItemsOfType(ItemInfo item)
	{
		List<UserItem> list = GetRoster().GetInventory().FindItemsByType(item.Type, string.Empty, false);
		foreach (UserItem userItem in list)
		{
			if (userItem.GetIsEquipped() && userItem.get_Name() != userItem.Info.Name)
			{
				UseItem(userItem, false);
			}
		}
	}

	public static bool GiveItemUnequipped(ItemInfo item, int count = 1)
	{
		if (item == null)
		{
			return false;
		}
		UserItem mBIJKDIEFIF = AddItem(item, count, 0L, false);
		return GetRoster().EquipItem(mBIJKDIEFIF, false);
	}

	public static bool FinishItemDelivery(ItemInfo item)
	{
		if (item == null)
		{
			return false;
		}
		UserItem dKCHDHMLKHN = GetUserItem(item.Name);
		dKCHDHMLKHN.set_DeliveryTime(0L);
		dKCHDHMLKHN.SetDeliveryUpgradeLevel(-1);
		item.SetIsNew(true);
		return dKCHDHMLKHN != null && _roster.EquipItem(dKCHDHMLKHN, false);
	}

	public static UserItem AddItem(ItemInfo item, int count = 1, long CNIOCCCBDBJ = 0L, bool JBCMFEPAKLK = true, bool KGOIDDCKBKI = true)
	{
		int acquisitionProfile = Eclipse.Modding.ModRuntime.StoryEvents.ProfileGeneration;
		Roster nKGLHEGIKKP = GetRoster();
		if (item.SubType == "UnlimitedEnergy")
		{
			nKGLHEGIKKP.HasUnlimitedEnergy = true;
			MenuController.RefreshEnergyView();
		}
		UserItem dKCHDHMLKHN = GetUserItem(item.Name);
		int previousCount = dKCHDHMLKHN == null ? 0 : dKCHDHMLKHN.GetCount();
		int acquiredCount = previousCount;
		int oMHDLKNHNMJ = nKGLHEGIKKP.GetLevel();
		if (dKCHDHMLKHN == null)
		{
			XmlNode fMBDAPOMFGN = nKGLHEGIKKP.GetItemsNode();
			int bLJGEOEHIGP = ((CNIOCCCBDBJ <= 0) ? count : 0);
			UserItem dKCHDHMLKHN2 = new UserItem(fMBDAPOMFGN, item.Name, JBCMFEPAKLK, bLJGEOEHIGP, -1, CNIOCCCBDBJ);
			dKCHDHMLKHN2.SetInfo(item);
			dKCHDHMLKHN = nKGLHEGIKKP.GetInventory().AddItem(dKCHDHMLKHN2);
			acquiredCount = dKCHDHMLKHN.GetCount();
			dKCHDHMLKHN.RefreshUpgradeState(oMHDLKNHNMJ);
			dKCHDHMLKHN.SetIsUpgrade(false);
			if (KGOIDDCKBKI)
			{
				dKCHDHMLKHN.ApplyDefaultEnchantments();
			}
		}
		else if (dKCHDHMLKHN.GetCount() != 0 || CNIOCCCBDBJ <= 0)
		{
			if (dKCHDHMLKHN.GetCount() != 0)
			{
				dKCHDHMLKHN.SetIsUpgrade(true);
			}
			if (item.ParentItem == null)
			{
				dKCHDHMLKHN.SetCount(dKCHDHMLKHN.GetCount() + count);
				acquiredCount = dKCHDHMLKHN.GetCount();
			}
			if (CNIOCCCBDBJ <= 0)
			{
				dKCHDHMLKHN.SetUpgradeLevel(item.UpgradeLevel);
				dKCHDHMLKHN.RefreshUpgradeState(oMHDLKNHNMJ);
			}
			dKCHDHMLKHN.set_DeliveryTime(CNIOCCCBDBJ);
			dKCHDHMLKHN.SetDeliveryUpgradeLevel(item.UpgradeLevel);
		}
		else
		{
			dKCHDHMLKHN.RefreshUpgradeState(oMHDLKNHNMJ);
			dKCHDHMLKHN.set_DeliveryTime(CNIOCCCBDBJ);
			dKCHDHMLKHN.SetDeliveryUpgradeLevel(-1);
			dKCHDHMLKHN.SetIsUpgrade(false);
		}
		if (JBCMFEPAKLK)
		{
			UseItem(dKCHDHMLKHN, true);
		}
		Eclipse.Modding.ModRuntime.PublishItemAcquired(nKGLHEGIKKP, item, previousCount, acquiredCount, acquisitionProfile);
		return dKCHDHMLKHN;
	}

	public static void RemoveItem(UserItem NDMCFNGEPOA, int count = 1)
	{
		if (NDMCFNGEPOA == null)
		{
			return;
		}
		int num = NDMCFNGEPOA.GetCount();
		if (num < count)
		{
			count = num;
		}
		NDMCFNGEPOA.SetCount(NDMCFNGEPOA.GetCount() - count);
		if (NDMCFNGEPOA.GetCount() == 0 && GetRoster().IsItemEquipped(NDMCFNGEPOA.GetInfo()))
		{
			if (NDMCFNGEPOA.GetInfo() != null)
			{
				NDMCFNGEPOA.GetInfo().RestoreShopVisibility();
			}
			UseItem(NDMCFNGEPOA, false);
			EquipFallbackItem(NDMCFNGEPOA.GetInfo());
		}
	}

	public static bool IncreaseExp(uint value)
	{
		return GetRoster().AddExperience(value);
	}

	public static bool AddCoins(long CDCBEDDONKK)
	{
		long num = GetRoster().GetMoney() + CDCBEDDONKK;
		if (num < 0)
		{
			return false;
		}
		GetRoster().SetMoney(num);
		return true;
	}

	public static bool SetCoins(long OKEFHDDPMEC)
	{
		if (OKEFHDDPMEC < 0)
		{
			return false;
		}
		GetRoster().SetMoney(OKEFHDDPMEC);
		return true;
	}

	public static bool AddGems(long CDCBEDDONKK, Roster.BalanceChangeType LFLGCDNKNJI, bool JEEOLJIFIOF = false)
	{
		long num = GetRoster().GetBonus() + CDCBEDDONKK;
		if (num < 0)
		{
			return false;
		}
		GetRoster().SetBonus(num, LFLGCDNKNJI, JEEOLJIFIOF);
		return true;
	}

	public static bool SetGems(long value, Roster.BalanceChangeType LFLGCDNKNJI, bool JEEOLJIFIOF = false)
	{
		if (value < 0)
		{
			return false;
		}
		GetRoster().SetBonus(value, LFLGCDNKNJI, JEEOLJIFIOF);
		return true;
	}

	public static bool ChangeEnergy(int value)
	{
		return GetRoster().ChangePower(value);
	}

	public static Zone GetCurrentZone()
	{
		List<Zone> cMEABHLEKNH = GetInstance().zones;
		foreach (Zone item in cMEABHLEKNH)
		{
			if (item.GetIsStart())
			{
				return item;
			}
		}
		List<Zone> kHLBNALFOGN = GetInstance().secondaryZones;
		foreach (Zone item2 in kHLBNALFOGN)
		{
			if (item2.GetIsStart())
			{
				return item2;
			}
		}
		List<Zone> cNMGADBPPJK = GetInstance().tertiaryZones;
		foreach (Zone item3 in cNMGADBPPJK)
		{
			if (item3.GetIsStart())
			{
				return item3;
			}
		}
		return null;
	}

	public static Zone GetZoneByName(string name)
	{
		List<Zone> cMEABHLEKNH = GetInstance().zones;
		foreach (Zone item in cMEABHLEKNH)
		{
			if (item.get_Name() == name)
			{
				return item;
			}
		}
		List<Zone> kHLBNALFOGN = GetInstance().secondaryZones;
		foreach (Zone item2 in kHLBNALFOGN)
		{
			if (item2.get_Name() == name)
			{
				return item2;
			}
		}
		List<Zone> cNMGADBPPJK = GetInstance().tertiaryZones;
		foreach (Zone item3 in cNMGADBPPJK)
		{
			if (item3.get_Name() == name)
			{
				return item3;
			}
		}
		return null;
	}

	public static List<Zone> GetZones()
	{
		return GetInstance().zones;
	}

	public static List<FightList> GetFightsForBattle(Battle DPOOIONCEOA)
	{
		List<FightList> list = new List<FightList>();
		foreach (FightList item in GetInstance().fights)
		{
			if (item.Battle == DPOOIONCEOA)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static FightList GetFightById(FightIDS DIAIIPCBMFL)
	{
		if (DIAIIPCBMFL == null)
		{
			return null;
		}
		List<FightList> jNPMCNMEOLE = GetInstance().fights;
		foreach (FightList item in jNPMCNMEOLE)
		{
			if (item.FightId.Equals(DIAIIPCBMFL))
			{
				return item;
			}
		}
		Battle cGJCGEBPCAF = GetBattleById(DIAIIPCBMFL);
		return (cGJCGEBPCAF == null) ? null : cGJCGEBPCAF.GetFightByName(DIAIIPCBMFL.GetFight());
	}

	public FightList GetFightByIdString(string name)
	{
		FightIDS mOCEDDJOAEB = new FightIDS(name);
		mOCEDDJOAEB.SetFightIDSByString(name);
		return GetFightById(mOCEDDJOAEB);
	}

	public static Battle GetBattleById(FightIDS DIAIIPCBMFL)
	{
		if (DIAIIPCBMFL == null)
		{
			return null;
		}
		Zone pKCPOJKLMOK = GetZoneByName(DIAIIPCBMFL.GetZone());
		if (pKCPOJKLMOK != null)
		{
			return pKCPOJKLMOK.FindBattle(DIAIIPCBMFL.GetBattle());
		}
		return null;
	}

	public static UserItem GetUserItem(string name)
	{
		return GetRoster().GetInventory().FindItem(name);
	}

	public static RosterFight LoadRosterFight(FightList KGKDKENMAOA, bool FFIBGBMOMPD)
	{
		RosterFight pIGKOIFBOME = null;
		pIGKOIFBOME = ((!FFIBGBMOMPD) ? GetRoster().RecordFightLoss(KGKDKENMAOA.FightId) : GetRoster().RecordFightWin(KGKDKENMAOA.FightId));
		if (pIGKOIFBOME != null)
		{
			pIGKOIFBOME.SetLevel(GetRoster().GetLevel());
			KGKDKENMAOA.SetRosterFight(pIGKOIFBOME);
		}
		RefreshConditionStatuses();
		return pIGKOIFBOME;
	}

	public static bool IsConditionComplete(ConditionFight IOFGGOCEIAM)
	{
		ConditionType kKJHFNGGFCG = IOFGGOCEIAM.Type;
		if (kKJHFNGGFCG == ConditionType.ConditionFightCount)
		{
			if (!IsFightCountConditionMet(IOFGGOCEIAM))
			{
				return false;
			}
		}
		else
		{
			GameLog.Error("ListSF::isConditionComplete - No handler for this type: " + IOFGGOCEIAM.Type);
		}
		return true;
	}

	public static bool IsFightCountConditionMet(ConditionFight IOFGGOCEIAM)
	{
		int iICJGJJGIMC = IOFGGOCEIAM.Count;
		ConditionCompare iFNPGAFFDNC = IOFGGOCEIAM.Compare;
		int num = 0;
		bool result = true;
		ConditionSubType dCJPHFALIND = IOFGGOCEIAM.SubType;
		if (dCJPHFALIND == ConditionSubType.ConditionSubTypeFight)
		{
			RosterFight pIGKOIFBOME = GetRoster().FindSavedFightRecord(IOFGGOCEIAM.GetFightIds());
			if (pIGKOIFBOME != null)
			{
				num = pIGKOIFBOME.GetWinCount();
			}
		}
		else
		{
			GameLog.Error("ListSF::isConditionFightCount - No handler for this object type: " + IOFGGOCEIAM.SubType);
		}
		switch (iFNPGAFFDNC)
		{
		case ConditionCompare.ConditionMoreEqually:
			result = num >= iICJGJJGIMC;
			break;
		case ConditionCompare.ConditionLessEqually:
			result = num <= iICJGJJGIMC;
			break;
		case ConditionCompare.ConditionMore:
			result = num > iICJGJJGIMC;
			break;
		case ConditionCompare.ConditionLess:
			result = num < iICJGJJGIMC;
			break;
		case ConditionCompare.ConditionEqually:
			result = num == iICJGJJGIMC;
			break;
		default:
			GameLog.Error("ListSF::isConditionFightCount - No handler for this compare type: " + iFNPGAFFDNC);
			break;
		}
		return result;
	}

	public static bool AreConditionsComplete(List<ConditionFight> conditions)
	{
		int i = 0;
		for (int count = conditions.Count; i < count; i++)
		{
			if (!IsConditionComplete(conditions[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static void UpdateAllFightStatuses()
	{
		List<FightList> jNPMCNMEOLE = GetInstance().fights;
		foreach (FightList item in jNPMCNMEOLE)
		{
			UpdateFightStatus(item);
		}
	}

	public static void UpdateFightStatus(FightList fight)
	{
		int eJGGHHEOGPG = fight.ReplayCount;
		RosterFight pIGKOIFBOME = fight.GetRosterFight();
		BattleType pJMEMGHKKBM = fight.get_Type();
		if (eJGGHHEOGPG > 0 && pIGKOIFBOME != null)
		{
			if (pJMEMGHKKBM != BattleType.FightPeriodic && pJMEMGHKKBM != BattleType.FightAscension && pIGKOIFBOME.GetWinCount() >= eJGGHHEOGPG)
			{
				if (pJMEMGHKKBM == BattleType.FightReplayable || pJMEMGHKKBM == BattleType.FightBossesReplayable || pJMEMGHKKBM == BattleType.FightFinalReplayable)
				{
					BattleReplayable bKKPCBGAEHC = (BattleReplayable)fight.Battle;
					bKKPCBGAEHC.RefreshFightStatus(fight);
				}
				else
				{
					fight.Status = ConditionStatus.StatusComplete;
				}
				return;
			}
			if (pJMEMGHKKBM == BattleType.FightAscension)
			{
				BattleAscension bGFLODNGLPK = (BattleAscension)fight.Battle;
				bGFLODNGLPK.RefreshFightStatus(fight);
				return;
			}
		}
		if (AreConditionsComplete(fight.GetConditions()))
		{
			fight.Status = ConditionStatus.StatusOpen;
		}
		else
		{
			fight.Status = ConditionStatus.StatusIncomplete;
		}
	}

	public static void UpdateAllZoneStatuses()
	{
		List<Zone> cMEABHLEKNH = GetInstance().zones;
		foreach (Zone item in cMEABHLEKNH)
		{
			item.UpdateStatus();
		}
		List<Zone> kHLBNALFOGN = GetInstance().secondaryZones;
		foreach (Zone item2 in kHLBNALFOGN)
		{
			item2.UpdateStatus();
		}
		List<Zone> cNMGADBPPJK = GetInstance().tertiaryZones;
		foreach (Zone item3 in cNMGADBPPJK)
		{
			item3.UpdateStatus();
		}
	}

	public static void RefreshConditionStatuses()
	{
		UpdateAllFightStatuses();
		UpdateAllZoneStatuses();
	}

	public void RequestSave(object data = null)
	{
		if (data == null || (int)data == 0)
		{
			isSaveRequested = true;
		}
		else
		{
			OnAuthenticate(true);
		}
	}

		public void OnAuthenticate(bool BOFABDEJGFL = false)
		{
            if (_localVersusProfile || Eclipse.Multiplayer.LocalVersusSession.IsActive) return;
			if (Eclipse.Modding.ModRuntime.DeferProfileSave())
		{
			isSaveRequested = true;
			return;
		}
		if (isSaveRequested || BOFABDEJGFL)
		{
			GetRoster().SaveCounterItems();
			GetRoster().SavePaymentOrders();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(SF2Paths.GetUserDataDirectory());
			stringBuilder.Append("/");
			stringBuilder.Append(Constants.UsersFileName);
			XmlUtils.SaveDocumentWithHash(userDocument, stringBuilder.ToString());
			stringBuilder.Replace(Constants.UsersFileName, Constants.UsersBackupFileName);
			XmlUtils.SaveDocumentWithHash(userDocument, stringBuilder.ToString());
			isSaveRequested = false;
		}
	}

	public static void DeleteUserDataAndQuit()
	{
		Eclipse.Modding.ModProfileWriteJournal.Discard(GetUsersFilePath());
		Eclipse.Modding.ModProfileWriteJournal.Discard(GetUsersBackupFilePath());
		if (File.Exists(GetUsersFilePath()))
		{
			File.Delete(GetUsersFilePath());
			UserDataValidator.DeleteHashFile(GetUsersFilePath());
		}
		if (File.Exists(GetUsersBackupFilePath()))
		{
			File.Delete(GetUsersBackupFilePath());
			UserDataValidator.DeleteHashFile(GetUsersBackupFilePath());
		}
		if (File.Exists(GetLocalSettingsFilePath()))
		{
			File.Delete(GetLocalSettingsFilePath());
			UserDataValidator.DeleteHashFile(GetLocalSettingsFilePath());
		}
		PacksController.GetInstance().DeletePacksFile();
		ApplicationController.Quit();
	}

	public static string ReadUsersFile()
	{
		return (!File.Exists(GetUsersFilePath())) ? string.Empty : File.ReadAllText(GetUsersFilePath());
	}

	public static void SetRosterFileContent(string GHDPPHAAPCA)
	{
		Eclipse.Modding.ModProfileWriteJournal.Discard(GetUsersFilePath());
		Eclipse.Modding.ModProfileWriteJournal.Discard(GetUsersBackupFilePath());
		if (File.Exists(GetUsersFilePath()))
		{
			File.Delete(GetUsersFilePath());
			UserDataValidator.DeleteHashFile(GetUsersFilePath());
		}
		if (File.Exists(GetUsersBackupFilePath()))
		{
			File.Delete(GetUsersBackupFilePath());
			UserDataValidator.DeleteHashFile(GetUsersBackupFilePath());
		}
		if (File.Exists(GetLocalSettingsFilePath()))
		{
			File.Delete(GetLocalSettingsFilePath());
			UserDataValidator.DeleteHashFile(GetLocalSettingsFilePath());
		}
		if (File.Exists(GetPacksFilePath()))
		{
			File.Delete(GetPacksFilePath());
			UserDataValidator.DeleteHashFile(GetPacksFilePath());
		}
		XmlDocument xmlDocument = XmlUtils.LoadFromString(GHDPPHAAPCA);
		if (xmlDocument == null)
		{
			GameLog.Write("Trying to set incorrect user - " + GetUsersFilePath());
			ApplicationController.Quit();
		}
		else
		{
			XmlUtils.SaveDocumentWithHash(xmlDocument, GetUsersFilePath());
			GameLog.Write("User successfully replaced in - " + GetUsersFilePath());
			ApplicationController.Quit();
		}
	}

	public bool RaiseQuestEvent(QuestEvent.QuestEventType LFLGCDNKNJI)
	{
		int storyProfile = Eclipse.Modding.ModRuntime.StoryEvents.ProfileGeneration;
		var notification = Eclipse.Modding.ModRuntime.CaptureStoryEvent(LFLGCDNKNJI, GetQuestParameters());
		bool handled = _QuestsManager.ActionQuest(LFLGCDNKNJI);
		Eclipse.Modding.ModRuntime.PublishStoryEvent(notification, storyProfile);
		return handled;
	}

	public void RunQuestActions()
	{
		if (!AreQuestActionsBlocked())
		{
			_QuestsManager.RunActionsAll();
		}
	}

	// Acceptance checkpoints matching quests before the continuation is saved.
	internal bool QueueLotteryFightEnd(QuestParameters context, bool raid)
	{
		_QuestsManager.QuestParameters = context;
		return RaiseQuestEvent(raid ? QuestEvent.QuestEventType.QUEST_EVENT_RAID_FIGHT_END : QuestEvent.QuestEventType.QUEST_EVENT_FIGHT_END);
	}

	public void AddActionQuests(List<QuestStage> NKNMCOEBMNG)
	{
		_QuestsManager.AddActionQuest(NKNMCOEBMNG);
	}

	public bool IsTutorialComplete()
	{
		Roster nKGLHEGIKKP = GetRoster();
		if (nKGLHEGIKKP != null)
		{
			return nKGLHEGIKKP.GetTutorials().GetIsStoryTutorialActive();
		}
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetUserDataDirectory(), Constants.UsersFileName);
		int num = GetCurrentUserId();
		XmlNode xmlNode = xmlDocument["Users"];
		foreach (XmlNode item in xmlNode)
		{
			if (num == item.Attributes["ID"].ParseInt())
			{
				XmlNode xmlNode3 = item;
				string text = xmlNode3.Attributes["Tutorial"].GetStringOrDefault(string.Empty);
				return text == "END";
			}
		}
		return false;
	}

	public void HandleAuthenticateResult(bool CELFBNLILMA)
	{
		Debug.Log("OnAuthenticate, isAuth = " + CELFBNLILMA);
		StopAuthTimeout();
		if (CELFBNLILMA)
		{
			OnAuthenticateSucceeded();
		}
		else
		{
			OnAuthenticateFailed();
		}
		FinishAuthenticate();
	}

	private void StartAuthTimeout()
	{
		StopAuthTimeout();
		authTimeoutRoutine = AuthTimeoutRoutine();
		CoroutineManager.get_Current().StartRoutine(authTimeoutRoutine);
	}

	private void StopAuthTimeout()
	{
		if (authTimeoutRoutine != null)
		{
			CoroutineManager.get_Current().StopRoutine(authTimeoutRoutine);
		}
		authTimeoutRoutine = null;
	}

	private IEnumerator AuthTimeoutRoutine()
	{
		float num = 1f;
		Debug.LogFormat("Setup Authenticate Timeout: {0} sec", num);
		yield return new WaitForSeconds(num);
		Debug.Log("OnAuthenticateTimeout: time is up!");
		FinishAuthenticate();
	}

	private void FinishAuthenticate()
	{
		Debug.Log("OnAuthenticateFinished");
		GameCenterAbstract.OnAuthenticate = (Action<bool>)Delegate.Remove(GameCenterAbstract.OnAuthenticate, new Action<bool>(OnAuthenticate));
		AuthorizeWithServer();
	}

	public void AuthorizeWithServer()
	{
		GameLog.Info("Login sequence: ListSF::serverAPIAuthorize");
		if (!isAuthorizeStarted)
		{
			isAuthorizeStarted = true;
			isServerAuthFailed = false;
			isServerAuthPending = true;
			isLoginStarted = true;
			RequestServerSync();
		}
	}

	public void OnAuthenticateSucceeded()
	{
		GameUtils.SendRepostedAchievements();
	}

	public void OnAuthenticateFailed()
	{
		GetRoster().IncrementGPlusFailedLogins();
	}

	public bool AreQuestActionsBlocked()
	{
		return false;
	}

	public QuestParameters GetQuestParameters()
	{
		return _QuestsManager.QuestParameters;
	}

	public QuestStage GetQuestByName(string name)
	{
		return _QuestsManager.GetQuestByName(name);
	}

	public bool IsEclipseQuestSuppressed(string name, string sourceFile = null)
	{
		return _QuestsManager.IsEclipseQuestSuppressed(name, sourceFile);
	}

	public QuestStage FindEclipseSavedQuest(string name, string sourceFile)
	{
		return _QuestsManager.GetQuestByName(name, sourceFile);
	}

	public void SetEclipseSuppressedQuests(IEnumerable<string> keys)
	{
		_QuestsManager.SetEclipseSuppressedQuests(keys);
	}

	public void ClearEclipseQuestSuppression()
	{
		_QuestsManager.ClearEclipseQuestSuppression();
	}

	public bool AddQuestToStek(string name, bool OBJGGIPDKDF)
	{
		return _QuestsManager.AddQuestToStek(name, OBJGGIPDKDF);
	}

	public bool AddQuestToStek(QuestStage DOKAIKMLLDK, bool OBJGGIPDKDF)
	{
		return _QuestsManager.AddQuestToStek(DOKAIKMLLDK, OBJGGIPDKDF);
	}

	public BattleType GetBattleTypeByName(string name)
	{
		foreach (KeyValuePair<string, BattleType> item in battleTypeNames)
		{
			if (item.Key == name)
			{
				return item.Value;
			}
		}
		return BattleType.FightNone;
	}

	public string GetBattleTypeName(BattleType LFLGCDNKNJI)
	{
		foreach (KeyValuePair<string, BattleType> item in battleTypeNames)
		{
			if (item.Value == LFLGCDNKNJI)
			{
				return item.Key;
			}
		}
		return "DUMMY";
	}

	public void OnPacksChanged()
	{
	}

	public void SetDataVersion(string value)
	{
		XmlNode xmlNode = userDocument["Root"]["Versions"];
		if (xmlNode != null)
		{
			xmlNode["DataVersion"].Attributes["Value"].Value = value;
		}
		RequestSave();
	}

	public void SetDataVersion(VersionContainer version)
	{
		string bAINMLLIKOL = version.ToString(true);
		SetDataVersion(bAINMLLIKOL);
	}

	public void ClearQuestsStack(List<string> NIKHAICFGNM = null)
	{
		_QuestsManager.ClearStack(NIKHAICFGNM);
	}

	public void InitBattleTypes()
	{
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("DUMMY", BattleType.FightNone));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("TUTORIAL", BattleType.FightTutorial));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("CHALLENGE", BattleType.FightChallenge));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("ASCENSION", BattleType.FightAscension));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("BOSSES", BattleType.FightBosses));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("TOURNAMENT", BattleType.FightTournament));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("STORY", BattleType.FightStory));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("SURVIVAL", BattleType.FightSurvival));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("TACTICS", BattleType.FightFriendly));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("AUTO", BattleType.FightAuto));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("AI", BattleType.FightAi));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("HIDDEN", BattleType.FightUnregister));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("FAKE", BattleType.FightFake));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("PVP", BattleType.FightPVP));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("PERIODIC", BattleType.FightPeriodic));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("FINAL_BATTLE", BattleType.FightFinal));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("FINAL_BATTLE_REPLAYABLE", BattleType.FightFinalReplayable));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("BOSSES_INTERMISSION", BattleType.FightBossesIntermission));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("REPLAYABLE", BattleType.FightReplayable));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("BOSSES_REPLAYABLE", BattleType.FightBossesReplayable));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("FINAL_BATTLE_TITAN", BattleType.FightFinalTitan));
		battleTypeNames.Add(new KeyValuePair<string, BattleType>("RAID", BattleType.FightRaid));
	}

	public void LoadQuests(string EIDDAFDJJCJ = "")
	{
		string text = ((!(EIDDAFDJJCJ == string.Empty)) ? EIDDAFDJJCJ : "quests.xml");
		Roster nKGLHEGIKKP = GetRoster();
		if (nKGLHEGIKKP.HasLoadedQuestFile(text))
		{
			return;
		}
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), text, XmlUtils.XmlSourceMode.Normal, true, XmlCryptoUtils.GetIsEncryptionEnabled());
		if (xmlDocument == null)
		{
			return;
		}
		XmlNode xmlNode = xmlDocument["Quests"];
		_QuestsManager.set_QuestsAllCapacity(xmlNode.ChildNodes.Count);
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			_QuestsManager.AddQuest(new QuestStage(childNode, text));
		}
		nKGLHEGIKKP.AddLoadedQuestFile(text);
	}

	public QuestStage AddExternalQuest(XmlNode node, string ownerFile)
	{
		if (node == null || node.Name != "Quest")
			throw new System.ArgumentException("External quest content must provide one Quest node.", "node");
		string name = node.Attributes?["Name"]?.Value ?? string.Empty;
		if (string.IsNullOrEmpty(name)) throw new System.ArgumentException("External quest Name must not be empty.", "node");
		if (_QuestsManager.GetQuestByName(name) != null)
			throw new System.InvalidOperationException("Quest already exists: '" + name + "'.");
		QuestStage quest = new QuestStage(node, string.IsNullOrEmpty(ownerFile) ? "eclipse-mod" : ownerFile);
		_QuestsManager.set_QuestsAllCapacity(1);
		_QuestsManager.AddQuest(quest);
		return quest;
	}

	public void RemoveExternalQuest(QuestStage quest)
	{
		if (quest != null) _QuestsManager.RemoveExternalQuest(quest);
	}

	public void OnFightSelected(FightList KGKDKENMAOA)
	{
		if (Module.GetInstance().GetCurrentScreenType() != ScreenType.ModuleMap)
		{
		}
	}

	public bool IsAdvertGroupAllowed(string EJENJNPEDOH)
	{
		if (EJENJNPEDOH == string.Empty)
		{
			return true;
		}
		string[] collection = EJENJNPEDOH.Split(',');
		List<string> list = new List<string>(collection);
		int kEKGEBCKLJI = AdvertSettings.GroupId;
		foreach (string item in list)
		{
			if (kEKGEBCKLJI == item.ToInt())
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateFightsLevel(int PPGFCLBFLEK)
	{
		foreach (FightList item in fights)
		{
			item.UpdateLevel(PPGFCLBFLEK);
		}
	}

	public void CreateMissingAchievements()
	{
		List<Achievement> list = GameUtils.AchievementDefinitions.GetEarnedAchievements();
		for (int i = 0; i < list.Count; i++)
		{
			Achievement jNPIOKEKMII = list[i];
			if (_roster.GetAchievements().FindAchievement(jNPIOKEKMII.Name) == null)
			{
				_roster.GetAchievements().UnlockAchievement(jNPIOKEKMII, false, false);
				_roster.GetAchievements().CreateRepostAchievement(jNPIOKEKMII.Name);
			}
		}
	}

	public void ShowDataCorruptedDialog(string NEPOLDCKNJL)
	{
		GameLog.Error("{0}", NEPOLDCKNJL);
		string title = "ERROR 576";
		string message = "Your game data may be corrupted. Please restart the application.\r\nContact support if this does not help: http://support.nekki.com\r\nDon`t uninstall the game to avoid savegame loss.";
		string cancel = "Reload";
		if (LocalizationManager.IsLoaded)
		{
			title = LocalizationManager.GetString("HackTitle");
			message = LocalizationManager.GetString("HackMessage");
			cancel = LocalizationManager.GetString("HackButton");
		}
		DialogsOpener.OpenLocalAlertDialog(title, message, cancel, QuitApplication);
	}

	private void QuitApplication()
	{
		Application.Quit();
	}

	public void UpdateEnergyRefillTimer(object data)
	{
		if (data == null)
		{
			GameLog.Error("ListSF::energyRefillTimer ERROR - data is NULL");
			return;
		}
		TextTimer.TimerDataStruct eHMOCHBPAGE = (TextTimer.TimerDataStruct)data;
		eHMOCHBPAGE.SetTime(GetRoster().GetEnergyRefillTimer());
		if (0 >= eHMOCHBPAGE.GetTime())
		{
			eHMOCHBPAGE.SetTime(0L);
		}
	}

	public void UpdateDuelAccessibilityTimer(object data)
	{
		if (data == null)
		{
			GameLog.Error("ListSF::duelAccessibilityTimer ERROR - data is NULL");
			return;
		}
		TextTimer.TimerDataStruct eHMOCHBPAGE = (TextTimer.TimerDataStruct)data;
		if (BattlePeriodic.GetTime() > 0)
		{
			eHMOCHBPAGE.SetTime(BattlePeriodic.GetRepeatTime() - BattlePeriodic.GetTime());
		}
		else
		{
			eHMOCHBPAGE.SetTime(0L);
		}
		if (0 >= eHMOCHBPAGE.GetTime())
		{
			eHMOCHBPAGE.SetTime(0L);
		}
	}

	public void UpdateDeliveryTimer(object data)
	{
		if (data == null)
		{
			GameLog.Error("ListSF::deliveryTimer ERROR - data is NULL");
			return;
		}
		TextTimer.TimerDataStruct eHMOCHBPAGE = (TextTimer.TimerDataStruct)data;
		if (eHMOCHBPAGE.Data != null)
		{
			UserItem dKCHDHMLKHN = (UserItem)eHMOCHBPAGE.Data;
			eHMOCHBPAGE.SetTime(GameUtils.GetLeftTime(dKCHDHMLKHN.GetDeliveryTimestamp()));
			if (0 > eHMOCHBPAGE.GetTime())
			{
				eHMOCHBPAGE.SetTime(0L);
			}
		}
		else
		{
			eHMOCHBPAGE.SetTime(0L);
		}
	}

	public void UpdateStartPackTimer(object data)
	{
		if (data == null)
		{
			GameLog.Error("ListSF::startPackTimer ERROR - data is NULL");
			return;
		}
		TextTimer.TimerDataStruct eHMOCHBPAGE = (TextTimer.TimerDataStruct)data;
		eHMOCHBPAGE.SetTime(GetRoster().GetStarterPackTimerEndTime() - GetCurrentTime());
		if (0 > eHMOCHBPAGE.GetTime())
		{
			eHMOCHBPAGE.SetTime(0L);
		}
	}

	public void UpdateCustomRosterTimer(object data)
	{
		if (data == null)
		{
			GameLog.Error("ListSF::customRosterTimer ERROR - data is NULL");
			return;
		}
		TextTimer.TimerDataStruct eHMOCHBPAGE = (TextTimer.TimerDataStruct)data;
		if (eHMOCHBPAGE.Data != null)
		{
			string gOHIIMFFFJI = (string)eHMOCHBPAGE.Data;
			RosterTimerContainer kCMICMHCEBB = GetRoster().GetTimerContainer();
			RosterTimer fPNMILOHPMB = kCMICMHCEBB.FindTimer(gOHIIMFFFJI);
			if (fPNMILOHPMB != null)
			{
				eHMOCHBPAGE.SetTime(GameUtils.GetLeftTime(fPNMILOHPMB.GetEndTimeSeconds()));
				if (eHMOCHBPAGE.GetTime() < 0)
				{
					eHMOCHBPAGE.SetTime(0L);
				}
			}
			else
			{
				eHMOCHBPAGE.SetTime(0L);
			}
		}
		else
		{
			eHMOCHBPAGE.SetTime(0L);
		}
	}

	public static List<PerkInfoItem> GetItemEnchantments(ItemInfo PJDAGCBPLJE, bool EKBOGDKIHIH = true)
	{
		if (EKBOGDKIHIH)
		{
			UserItem dKCHDHMLKHN = GetRoster().GetInventory().FindItem(PJDAGCBPLJE.Name);
			if (dKCHDHMLKHN != null)
			{
				return dKCHDHMLKHN.GetEnchantments();
			}
		}
		return PJDAGCBPLJE.DefaultEnchantmentPreviews;
	}

	public void OnItemGiven(ItemInfo item)
	{
	}

	public void AddMergedGroupModel(ModelParameters JCICKLIMBEF)
	{
		mergedGroupModels.Add(JCICKLIMBEF);
	}

	public void EquipDefaultItem(ItemInfo item)
	{
		string text = string.Empty;
		if (item.Type == "Armor")
		{
			text = GameUtils.GetDefaultItem("Armor");
		}
		else if (item.Type == "Helm")
		{
			text = GameUtils.GetDefaultItem("Helm");
		}
		else if (item.Type == "Weapon")
		{
			text = GameUtils.GetDefaultItem("Weapon");
		}
		else if (item.Type == "Ranged")
		{
			text = GameUtils.GetDefaultItem("Ranged");
		}
		else if (item.Type == "Magic")
		{
			text = GameUtils.GetDefaultItem("Magic");
		}
		else if (item.Type == "RaidConsumable" && item.SubType == "RaidCharge")
		{
			text = GameUtils.GetDefaultItem("RaidCharge");
		}
		else
		{
			ClearEquippedSlot(item);
		}
		if (text != string.Empty)
		{
			UserItem dKCHDHMLKHN = GetUserItem(text);
			if (dKCHDHMLKHN != null)
			{
				UseItem(dKCHDHMLKHN, true);
			}
		}
	}

	public bool ApplyFightRewards(FightResult HEIADONEACH)
	{
		return ApplyFightRewards(HEIADONEACH.Prize);
	}

	public bool ApplyFightRewards(FightResult.ResultPrizeStruct PMIHPJFAJIO)
	{
		long gBGNFPNCGED = PMIHPJFAJIO.Money;
		long pNDAIFALIKF = PMIHPJFAJIO.Bonus;
		bool result = IncreaseExp(PMIHPJFAJIO.exp);
		if (gBGNFPNCGED > 0)
		{
			AddCoins(gBGNFPNCGED);
		}
		if (pNDAIFALIKF > 0)
		{
			AddGems(pNDAIFALIKF, Roster.BalanceChangeType.CHANGE_FIGHT_REWARD);
		}
		List<CurrencyStruct> list = PMIHPJFAJIO.GetCurrencies();
		if (list.Count > 0)
		{
			foreach (CurrencyStruct item in list)
			{
				if (GetRoster().GetShowForge() || item.Currency.Group == GameCurrency.CurrencyGroup.CURRENCY_GROUP_NONE)
				{
					_roster.AddCurrencyCount(item.Currency, (ObscuredInt)(item.Count));
				}
			}
		}
		List<ResistanceStruct> list2 = PMIHPJFAJIO.GetResistances();
		if (list2.Count > 0)
		{
			foreach (ResistanceStruct item2 in list2)
			{
			}
		}
		List<FightResult.ItemGrant> hELFDCAIJNE = PMIHPJFAJIO.Items;
		if (hELFDCAIJNE.Count > 0)
		{
			foreach (FightResult.ItemGrant item3 in hELFDCAIJNE)
			{
				ItemInfo dLKPBAJDHBO = item3.Item;
				RewardItem nAIEGGHELIH = item3.RewardSource;
				if (dLKPBAJDHBO.ParentItem != null)
				{
					AddItem(dLKPBAJDHBO.ParentItem, 1, 0L, false, false);
				}
				UserItem dKCHDHMLKHN = AddItem(dLKPBAJDHBO, 1, 0L, false, false);
				int mHGODOLNDLE = dLKPBAJDHBO.ItemLevel;
				int mHNCENBCECJ = _roster.GetLevel();
				dKCHDHMLKHN.ApplyEnchantments(nAIEGGHELIH.enchantments, mHGODOLNDLE, mHNCENBCECJ);
			}
		}
		RequestSave();
		return result;
	}

	public ModelParameters ParseWarriorParameters(XmlNode node, ModelParameters NENHLHHFDCN, bool CHOLFBIPDIM = true)
	{
		ModelParameters kIKOGDEPGHB = ((NENHLHHFDCN == null) ? new ModelParameters() : NENHLHHFDCN.Clone());
		if (node.Attributes["Voice"] != null)
		{
			kIKOGDEPGHB.Voice = node.Attributes["Voice"].GetStringOrDefault(string.Empty);
		}
		if (node.Attributes["Number"] != null)
		{
			kIKOGDEPGHB.OpponentCount = node.Attributes["Number"].ParseInt();
		}
		if (node.Attributes["NoDoubles"] != null)
		{
			kIKOGDEPGHB.NoDoubles = node.Attributes["NoDoubles"].ParseInt() > 0;
		}
		kIKOGDEPGHB.RandomValue = node.Attributes["Random"].ParseInt();
		kIKOGDEPGHB.AutoTuneFactor = node.Attributes["AutoTuneFactor"].ParseFloat();
		kIKOGDEPGHB.SetLevel((ObscuredInt)(node.Attributes["Level"].ParseInt((ObscuredInt)(kIKOGDEPGHB.GetLevel()))));
		kIKOGDEPGHB.Dan = node.Attributes["Dan"].ParseInt(kIKOGDEPGHB.Dan);
		kIKOGDEPGHB.Damage = node.Attributes["Damage"].ParseFloat(kIKOGDEPGHB.Damage);
		kIKOGDEPGHB.Difficulty = node.Attributes["Difficulty"].ParseFloat(kIKOGDEPGHB.Difficulty);
		kIKOGDEPGHB.BeginnerCheat = node.Attributes["BeginnerCheat"] != null && node.Attributes["BeginnerCheat"].ParseInt() > 0;
		kIKOGDEPGHB.WarriorPower = node.Attributes["WarriorPower"].ParseInt();
		if (node.Attributes["ShieldTotal"] != null)
		{
			kIKOGDEPGHB.ShieldTotal = Mathf.Max(0, node.Attributes["ShieldTotal"].ParseInt());
			kIKOGDEPGHB.HasShieldTotalOverride = true;
		}
		kIKOGDEPGHB.RatingCorrection = node.Attributes["RatingCorrection"].ParseInt();
		kIKOGDEPGHB.UnknownFlag = node.Attributes["Unknown"].ParseBool();
		List<WarriorAttribute> iBLHIAHECLK = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item in iBLHIAHECLK)
		{
			bool flag = node.Attributes[item.get_Name()] == null;
			if (flag && CHOLFBIPDIM)
			{
				int OEMALIFPGPO = 0;
				kIKOGDEPGHB.BaseAttributes.Get(item.get_Name(), ref OEMALIFPGPO);
				OEMALIFPGPO = ((!GameUtils.IsAlignTargetAttribute(item.get_Name())) ? OEMALIFPGPO : (OEMALIFPGPO + kIKOGDEPGHB.WarriorPower));
				kIKOGDEPGHB.BaseAttributes.Set(item.get_Name(), OEMALIFPGPO);
			}
			else if (!flag)
			{
				int num = node.Attributes[item.get_Name()].ParseInt();
				num = ((!GameUtils.IsAlignTargetAttribute(item.get_Name())) ? num : (num + kIKOGDEPGHB.WarriorPower));
				kIKOGDEPGHB.BaseAttributes.Set(item.get_Name(), num);
			}
		}
		if (node.Attributes["FirstName"] != null)
		{
			kIKOGDEPGHB.FirstName = node.Attributes["FirstName"].GetStringOrDefault(string.Empty);
		}
		if (node.Attributes["LastName"] != null)
		{
			kIKOGDEPGHB.LastName = node.Attributes["LastName"].GetStringOrDefault(string.Empty);
		}
		if (node.Attributes["Avatar"] != null)
		{
			kIKOGDEPGHB.Avatar = node.Attributes["Avatar"].GetStringOrDefault(string.Empty);
		}
		if (node.Attributes["PlayerRating"] != null)
		{
			kIKOGDEPGHB.SetPlayerRating(node.Attributes["PlayerRating"].ParseFloat());
		}
		if (node.Attributes["EnemyRating"] != null)
		{
			kIKOGDEPGHB.SetEnemyRating(node.Attributes["EnemyRating"].ParseFloat());
		}
		if (node.Attributes["PlayerRatingMagic"] != null)
		{
			kIKOGDEPGHB.SetPlayerRatingMagic(node.Attributes["PlayerRatingMagic"].ParseFloat());
		}
		if (node.Attributes["EnemyRatingMagic"] != null)
		{
			kIKOGDEPGHB.SetEnemyRatingMagic(node.Attributes["EnemyRatingMagic"].ParseFloat());
		}
		if (node.Attributes["PlayerRatingRanged"] != null)
		{
			kIKOGDEPGHB.SetPlayerRatingRanged(node.Attributes["PlayerRatingRanged"].ParseFloat());
		}
		if (node.Attributes["EnemyRatingRanged"] != null)
		{
			kIKOGDEPGHB.SetEnemyRatingRanged(node.Attributes["EnemyRatingRanged"].ParseFloat());
		}
		XmlNode xmlNode = node["AttributesAlign"];
		if (xmlNode != null)
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				AttributesAlign hLHDMKCPIJP = new AttributesAlign();
				hLHDMKCPIJP.Factor = childNode.Attributes["Factor"].ParseFloat();
				hLHDMKCPIJP.Shift = childNode.Attributes["Shift"].ParseFloat();
				hLHDMKCPIJP.Priority = childNode.Attributes["Priority"].ParseInt();
				if (childNode.Attributes["Eclipse"] == null)
				{
					hLHDMKCPIJP.DifficultyFilter = ModelParameters.DifficultyFilter.DFBoth;
				}
				else if (childNode.Attributes["Eclipse"].ParseInt() == 0)
				{
					hLHDMKCPIJP.DifficultyFilter = ModelParameters.DifficultyFilter.DFNormal;
				}
				else
				{
					hLHDMKCPIJP.DifficultyFilter = ModelParameters.DifficultyFilter.DFHard;
				}
				kIKOGDEPGHB.AttributeAlignments.Add(hLHDMKCPIJP);
			}
		}
		XmlNode xmlNode3 = node["Groups"];
		if (xmlNode3 != null)
		{
			foreach (XmlNode item2 in xmlNode3)
			{
				GroupModel fIHMEHKAOCP = new GroupModel();
				fIHMEHKAOCP.Name = item2.Attributes["Name"].GetStringOrDefault(string.Empty);
				fIHMEHKAOCP.IsRandom = item2.Attributes["Random"].ParseBool();
				fIHMEHKAOCP.NoDoubles = item2.Attributes["NoDoubles"].ParseBool();
				kIKOGDEPGHB.GroupModels.Add(fIHMEHKAOCP);
			}
		}
		if (!CHOLFBIPDIM)
		{
			if (node.Attributes["Skeleton"] != null)
			{
				kIKOGDEPGHB.Skeleton = GetItems().GetItemByName(node.Attributes["Skeleton"].GetStringOrDefault(string.Empty));
			}
			if (node.Attributes["Armor"] != null)
			{
				kIKOGDEPGHB.Armor = GetItems().GetItemByName(node.Attributes["Armor"].GetStringOrDefault(string.Empty));
			}
			if (node.Attributes["Helm"] != null)
			{
				kIKOGDEPGHB.Helm = GetItems().GetItemByName(node.Attributes["Helm"].GetStringOrDefault(string.Empty));
			}
			if (node.Attributes["Weapon"] != null)
			{
				kIKOGDEPGHB.Weapon = GetItems().GetItemByName(node.Attributes["Weapon"].GetStringOrDefault(string.Empty));
			}
			if (node.Attributes["Ranged"] != null)
			{
				kIKOGDEPGHB.Ranged = GetItems().GetItemByName(node.Attributes["Ranged"].GetStringOrDefault(string.Empty));
			}
			if (node.Attributes["Magic"] != null)
			{
				kIKOGDEPGHB.Magic = GetItems().GetItemByName(node.Attributes["Magic"].GetStringOrDefault(string.Empty));
			}
			if (node.Attributes["RaidCharge"] != null)
			{
				RaidModelParameters kAOPLEPILDH = kIKOGDEPGHB as RaidModelParameters;
				if (kAOPLEPILDH != null)
				{
					kAOPLEPILDH.RaidChargeItem = GetItems().GetItemByName(node.Attributes["RaidCharge"].GetStringOrDefault(string.Empty));
				}
			}
			if (node.Attributes["Seal"] != null)
			{
				kIKOGDEPGHB.Seal = GetItems().GetItemByName(node.Attributes["Seal"].GetStringOrDefault(string.Empty));
			}
		}
		else
		{
			XmlNode xmlNode5 = node["Items"];
			if (xmlNode5 != null)
			{
				kIKOGDEPGHB.DecorateItems.Clear();
				foreach (XmlNode item3 in xmlNode5)
				{
					ItemInfo dJKEECEOCJB = null;
					if (item3.Attributes["Name"] != null)
					{
						dJKEECEOCJB = GetItems().GetItemByName(item3.Attributes["Name"].GetStringOrDefault(string.Empty));
					}
					else if (item3.Attributes["Type"] != null)
					{
						dJKEECEOCJB = kIKOGDEPGHB.GetItemByType(item3.Attributes["Type"].GetStringOrDefault(string.Empty));
					}
					if (dJKEECEOCJB != null)
					{
						if (dJKEECEOCJB.Type.Equals("Decorate"))
						{
							kIKOGDEPGHB.DecorateItems.Add(dJKEECEOCJB);
							continue;
						}
						ItemInfo dJKEECEOCJB2 = dJKEECEOCJB.Clone();
						XmlNode hKPPBKPJOEO = item3["Enchantments"];
						dJKEECEOCJB2.DefaultEnchantments.Clear();
						dJKEECEOCJB2.SetParsedPerks(hKPPBKPJOEO);
						kIKOGDEPGHB.SetItemByType(dJKEECEOCJB2.Type, dJKEECEOCJB2);
						if (!IsContentLoaded)
						{
							kIKOGDEPGHB.AddConditionItem(dJKEECEOCJB2);
						}
					}
					else if (!string.IsNullOrEmpty(kIKOGDEPGHB.GroupName))
					{
						string text = item3.Attributes["Name"].GetStringOrDefault(string.Empty);
						string text2 = item3.Attributes["Type"].GetStringOrDefault(string.Empty);
						GameLog.Error("No item Wariror {0} ---- {1}", text, text2);
					}
				}
			}
			XmlNode xmlNode7 = node["Perks"];
			if (xmlNode7 != null)
			{
				foreach (XmlNode item4 in xmlNode7)
				{
					string gOHIIMFFFJI = item4.Attributes["Name"].GetStringOrDefault(string.Empty);
					PerkInfoItem aCONCDFDNJH = GameUtils.PerkItemList.FindBasePerk(gOHIIMFFFJI);
					if (aCONCDFDNJH != null && !kIKOGDEPGHB.WarriorPerks.Contains(aCONCDFDNJH))
					{
						if (item4["Set"] != null || item4["RatingEvaluation"] != null)
						{
							aCONCDFDNJH = aCONCDFDNJH.Clone(item4["Set"], item4["RatingEvaluation"]);
							kIKOGDEPGHB.AddWarriorPerk(aCONCDFDNJH);
						}
						kIKOGDEPGHB.WarriorPerks.AddIfNotExist(aCONCDFDNJH);
					}
				}
			}
		}
		Tactic hBFMBOHLKPJ = AiData.GetTacticByName(node.Attributes["Tactic"].GetStringOrDefault(string.Empty));
        if (node.Attributes["EclipseBodyModel"] != null) kIKOGDEPGHB.EclipseBodyModel = node.Attributes["EclipseBodyModel"].Value;
        if (node.Attributes["EclipseCharacterId"] != null) kIKOGDEPGHB.EclipseCharacterId = node.Attributes["EclipseCharacterId"].Value;
        if (node["EclipseSkinModels"] != null)
        {
            var models = new List<string>();
            foreach (XmlNode skin in node["EclipseSkinModels"].ChildNodes) models.Add(skin.Attributes["Asset"].Value);
            kIKOGDEPGHB.EclipseSkinModels = models.ToArray();
        }
		kIKOGDEPGHB.FightTactic = hBFMBOHLKPJ;
		kIKOGDEPGHB.AiControlled = node.Attributes["NotAI"] == null;
		kIKOGDEPGHB.AnimationEnabled = node.Attributes["NotAnimation"] == null;
		kIKOGDEPGHB.UserControlled = node.Attributes["Controlled"] != null;
		kIKOGDEPGHB.IsPlayer = false;
		kIKOGDEPGHB.IsWinner = false;
		kIKOGDEPGHB.RoundEnded = false;
		kIKOGDEPGHB.IsDead = false;
		kIKOGDEPGHB.RoundsWon = 0;
		kIKOGDEPGHB.MaxLife = 0f;
		if (!kIKOGDEPGHB.HasSourceNode)
		{
			kIKOGDEPGHB.Node = node;
			kIKOGDEPGHB.HasSourceNode = true;
		}
		return kIKOGDEPGHB;
	}

	public static void RegisterReplayableBattle(BattleReplayable BBAGBFDMNJE)
	{
		replayableBattles.AddIfNotExist(BBAGBFDMNJE);
	}

	public void ParseFight(FightList fight, XmlNode node, BattleType LFLGCDNKNJI, string LPJNEDFCBOI, string PINIIFIOECE, Battle DPOOIONCEOA)
	{
		fight.Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		fight.set_Type(LFLGCDNKNJI);
		fight.ReplayCount = node.Attributes["Replays"].ParseInt();
		fight.RepeatTime = node.Attributes["ReplayInterval"].ParseInt();
		fight.set_PowerRequired(node.Attributes["Power"].ParseInt(1));
		fight.RoundsToWin = node.Attributes["Rounds"].ParseInt(2);
		fight.RoundTime = (ObscuredInt)(node.Attributes["RoundTime"].ParseInt(60));
		fight.Location = node.Attributes["Location"].GetStringOrDefault(LPJNEDFCBOI);
		fight.Music = node.Attributes["Music"].GetStringOrDefault(PINIIFIOECE);
		fight.EvaluatedRating = node.Attributes["EvaluatedRating"].ParseFloat(-1f);
		fight.HealthRecovery = node.Attributes["HealthRecovery"].ParseFloat(1f);
		fight.PrizeBase = node.Attributes["PrizeBase"].ParseFloat(-1f);
		fight.set_Description(node.Attributes["Description"].GetStringOrDefault(string.Empty));
		fight.IsLocked = node.Attributes["Locked"].ParseBool();
		fight.RewardImage = node.Attributes["RewardImage"].GetStringOrDefault(string.Empty);
		fight.TrackFightProgress = LFLGCDNKNJI != BattleType.FightNone;
		ushort aNHLAHFDDCE = 0;
		ushort lPMDOHPIEOP = 0;
		if (!node.Attributes["RewardDigits"].Empty())
		{
			aNHLAHFDDCE = (ushort)node.Attributes["RewardDigits"].ParseUint();
		}
		else if (DPOOIONCEOA != null)
		{
			aNHLAHFDDCE = DPOOIONCEOA.GetRewardDigits();
		}
		if (!node.Attributes["PrizeBaseDigits"].Empty())
		{
			lPMDOHPIEOP = (ushort)node.Attributes["PrizeBaseDigits"].ParseUint();
		}
		else if (DPOOIONCEOA != null)
		{
			lPMDOHPIEOP = DPOOIONCEOA.GetPrizeBaseDigits();
		}
		fight.RewardDigits = aNHLAHFDDCE;
		fight.PrizeBaseDigits = lPMDOHPIEOP;
		ModelParameters kIKOGDEPGHB = null;
		XmlNode xmlNode = node["Warriors"];
		if (xmlNode != null)
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				if (!childNode.Attributes["Template"].Empty())
				{
					TemplateUser aHIIGFBIGAA = GetTemplateByName(childNode.Attributes["Template"].GetStringOrDefault());
					kIKOGDEPGHB = ((aHIIGFBIGAA == null) ? ParseWarriorParameters(childNode, null) : ParseWarriorParameters(childNode, aHIIGFBIGAA.Parameters));
				}
				else
				{
					kIKOGDEPGHB = ParseWarriorParameters(childNode, null);
				}
				kIKOGDEPGHB.GroupName = childNode.Attributes["Group"].GetStringOrDefault();
				kIKOGDEPGHB.RandomValue = childNode.Attributes["Random"].ParseInt();
				kIKOGDEPGHB.Node = childNode;
				fight.AddOpponent(kIKOGDEPGHB);
			}
		}
		XmlNode hKPPBKPJOEO = node["Rules"];
		XmlNode hKPPBKPJOEO2 = node["Rewards"];
		ParseFightRules(fight, hKPPBKPJOEO);
		// FightIDS is assigned after this parser returns; use the already registered
		// battle metadata to identify offline raids while building their rewards.
		if (LFLGCDNKNJI != BattleType.FightRaid ||
			(DPOOIONCEOA != null && Eclipse.Modding.ModPolicies.TryRaidBattle(DPOOIONCEOA.get_Name(), out _)))
		{
			ParseFightRewards(fight, hKPPBKPJOEO2);
		}
	}

	public void AddFight(FightList KGKDKENMAOA)
	{
		fights.AddIfNotExist(KGKDKENMAOA);
		if (areZonesLoaded)
		{
			LinkFightToRoster(KGKDKENMAOA);
			UpdateFightStatus(KGKDKENMAOA);
		}
	}

	public void RemoveFight(FightList KGKDKENMAOA)
	{
		fights.Remove(KGKDKENMAOA);
	}

	public static void RecordCoinShortfall(ItemInfo item)
	{
		long num = item.GetCoinPrice();
		Roster nKGLHEGIKKP = GetRoster();
		long num2 = nKGLHEGIKKP.GetMoney() - (ObscuredLong)(nKGLHEGIKKP.GetPaidMoney());
		long num3 = (item.MissingCoins = Math.Max(0L, num - num2));
		nKGLHEGIKKP.SetPaidMoney((ObscuredLong)((ObscuredLong)(nKGLHEGIKKP.GetPaidMoney()) - num3));
	}

	public static void RecordGemShortfall(ItemInfo item, bool CNIOCCCBDBJ)
	{
		long num = 0L;
		num = (CNIOCCCBDBJ ? (long)(ObscuredLong)(item.DeliveryGemPrice) : item.GetGemPrice());
		long pEGDPDINDDO = Math.Max(0L, num - GetRoster().GetUnpaidBonus());
		item.MissingGems = pEGDPDINDDO;
	}

	public TemplateUser GetTemplateByName(string name)
	{
		foreach (TemplateUser item in templates)
		{
			if (item.name == name)
			{
				return item;
			}
		}
		return null;
	}

	public void ApplyParameterOverrides(ModelParameters OEMALIFPGPO, ModelParameters BBNKIBKPBLO)
	{
		if (BBNKIBKPBLO.SceneType != SceneTypes.SceneNone)
		{
			OEMALIFPGPO.SceneType = BBNKIBKPBLO.SceneType;
		}
		if (BBNKIBKPBLO.AutoTuneFactor != 0f)
		{
			OEMALIFPGPO.AutoTuneFactor = BBNKIBKPBLO.AutoTuneFactor;
		}
		if (BBNKIBKPBLO.Voice.Length != 0)
		{
			OEMALIFPGPO.Voice = BBNKIBKPBLO.Voice;
		}
		if (BBNKIBKPBLO.Skeleton != null)
		{
			OEMALIFPGPO.Skeleton = BBNKIBKPBLO.Skeleton;
		}
		if (BBNKIBKPBLO.Weapon != null)
		{
			OEMALIFPGPO.Weapon = BBNKIBKPBLO.Weapon;
		}
		if (BBNKIBKPBLO.Armor != null)
		{
			OEMALIFPGPO.Armor = BBNKIBKPBLO.Armor;
		}
		if (BBNKIBKPBLO.Helm != null)
		{
			OEMALIFPGPO.Helm = BBNKIBKPBLO.Helm;
		}
		if (BBNKIBKPBLO.Ranged != null)
		{
			OEMALIFPGPO.Ranged = BBNKIBKPBLO.Ranged;
		}
		if (BBNKIBKPBLO.Magic != null)
		{
			OEMALIFPGPO.Magic = BBNKIBKPBLO.Magic;
		}
		if (BBNKIBKPBLO.Seal != null)
		{
			OEMALIFPGPO.Seal = BBNKIBKPBLO.Seal;
		}
		RaidModelParameters kAOPLEPILDH = OEMALIFPGPO as RaidModelParameters;
		RaidModelParameters kAOPLEPILDH2 = BBNKIBKPBLO as RaidModelParameters;
		if (kAOPLEPILDH != null && kAOPLEPILDH2 != null && kAOPLEPILDH2.RaidChargeItem != null)
		{
			kAOPLEPILDH.RaidChargeItem = kAOPLEPILDH2.RaidChargeItem;
		}
		if (BBNKIBKPBLO.EndRoundType != EndRoundType.EndRoundTypeNone)
		{
			OEMALIFPGPO.EndRoundType = BBNKIBKPBLO.EndRoundType;
		}
		if (BBNKIBKPBLO.WarriorPower != 0)
		{
			OEMALIFPGPO.WarriorPower = BBNKIBKPBLO.WarriorPower;
		}
		if (BBNKIBKPBLO.HasShieldTotalOverride)
		{
			OEMALIFPGPO.ShieldTotal = BBNKIBKPBLO.ShieldTotal;
			OEMALIFPGPO.HasShieldTotalOverride = true;
		}
		if (BBNKIBKPBLO.RatingCorrection != 0)
		{
			OEMALIFPGPO.RatingCorrection = BBNKIBKPBLO.RatingCorrection;
		}
		if (BBNKIBKPBLO.Damage != 0f)
		{
			OEMALIFPGPO.Damage = BBNKIBKPBLO.Damage;
		}
		if (BBNKIBKPBLO.Difficulty != 0f)
		{
			OEMALIFPGPO.Difficulty = BBNKIBKPBLO.Difficulty;
		}
		if (!string.IsNullOrEmpty(BBNKIBKPBLO.FirstName))
		{
			OEMALIFPGPO.FirstName = BBNKIBKPBLO.FirstName;
		}
		if (!string.IsNullOrEmpty(BBNKIBKPBLO.LastName))
		{
			OEMALIFPGPO.LastName = BBNKIBKPBLO.LastName;
		}
		if (!string.IsNullOrEmpty(BBNKIBKPBLO.Avatar))
		{
			OEMALIFPGPO.Avatar = BBNKIBKPBLO.Avatar;
		}
		if (BBNKIBKPBLO.Perks.Count != 0)
		{
			foreach (PerkInfoItem item in BBNKIBKPBLO.Perks)
			{
				OEMALIFPGPO.Perks.AddIfNotExist(item);
			}
		}
		if (BBNKIBKPBLO.WarriorPerks.Count != 0)
		{
			foreach (PerkInfoItem item2 in BBNKIBKPBLO.WarriorPerks)
			{
				OEMALIFPGPO.WarriorPerks.AddIfNotExist(item2);
			}
		}
		if (BBNKIBKPBLO.LearnedPerks.Count != 0)
		{
			foreach (PerkInfoItem item3 in BBNKIBKPBLO.LearnedPerks)
			{
				OEMALIFPGPO.LearnedPerks.AddIfNotExist(item3);
			}
		}
		if (BBNKIBKPBLO.GetPlayerRating() != -1f)
		{
			OEMALIFPGPO.SetPlayerRating(BBNKIBKPBLO.GetPlayerRating());
		}
		if (BBNKIBKPBLO.GetEnemyRating() != -1f)
		{
			OEMALIFPGPO.SetEnemyRating(BBNKIBKPBLO.GetEnemyRating());
		}
		if (BBNKIBKPBLO.GetPlayerRatingMagic() != -1f)
		{
			OEMALIFPGPO.SetPlayerRatingMagic(BBNKIBKPBLO.GetPlayerRatingMagic());
		}
		if (BBNKIBKPBLO.GetEnemyRatingMagic() != -1f)
		{
			OEMALIFPGPO.SetEnemyRatingMagic(BBNKIBKPBLO.GetEnemyRatingMagic());
		}
		if (BBNKIBKPBLO.GetPlayerRatingRanged() != -1f)
		{
			OEMALIFPGPO.SetPlayerRatingRanged(BBNKIBKPBLO.GetPlayerRatingRanged());
		}
		if (BBNKIBKPBLO.GetEnemyRatingRanged() != -1f)
		{
			OEMALIFPGPO.SetEnemyRatingRanged(BBNKIBKPBLO.GetEnemyRatingRanged());
		}
		if (BBNKIBKPBLO.FightTactic != null)
		{
			OEMALIFPGPO.FightTactic = BBNKIBKPBLO.FightTactic;
		}
		if (BBNKIBKPBLO.AttributeAlignments.Count != 0)
		{
			foreach (AttributesAlign item4 in BBNKIBKPBLO.AttributeAlignments)
			{
				OEMALIFPGPO.AttributeAlignments.Add(new AttributesAlign(item4));
			}
		}
		List<WarriorAttribute> iBLHIAHECLK = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item5 in iBLHIAHECLK)
		{
			string kGBGENDIMBC = item5.get_Name();
			int OEMALIFPGPO2 = 0;
			if (BBNKIBKPBLO.FinalAttributes.Get(kGBGENDIMBC, ref OEMALIFPGPO2))
			{
				OEMALIFPGPO.FinalAttributes.Set(kGBGENDIMBC, OEMALIFPGPO2);
			}
		}
	}

	private void OnTimerTick(object data)
	{
		long getTime = GlobalTimer.get_GetTime();
		currentTime = getTime;
		SetZonesTime(currentTime);
		UpdateRosterTime(currentTime);
		if (GameUtils.IsLoginComplete)
		{
			bool flag = Module.GetInstance().GetCurrentScreenType() != ScreenType.ModulePreloader;
			bool flag2 = Module.GetInstance().GetCurrentScreenType() != ScreenType.ModuleFight;
			bool flag3 = Fight.GetCurrentFight() != null && Fight.GetCurrentFight().get_isFightNone();
			if (flag && (flag2 || flag3))
			{
				DeliverReadyRecipes(currentTime);
			}
			RosterTimerContainer kCMICMHCEBB = _roster.GetTimerContainer();
			kCMICMHCEBB.CheckTimers(currentTime);
		}
		OnAuthenticate();
	}

	private void LoadItems()
	{
		_items.LoadItems(SF2Paths.GetGameDataPath());
	}

		private void LoadProfile()
		{
            _localVersusProfile = Eclipse.Multiplayer.LocalVersusSession.IsActive;
			Eclipse.Modding.ModRuntime.UnbindProfile();
		userDocument = XmlUtils.LoadDocumentWithHashCheck(SF2Paths.GetUserDataDirectory(), Constants.UsersFileName);
		if (userDocument == null)
		{
			_roster = new Roster(null, null);
			return;
		}
            // Roster normalization and mod-state migrations may update this
            // document during boot. Local play owns a disposable copy.
            if (_localVersusProfile) userDocument = (XmlDocument)userDocument.CloneNode(true);
			int num = GetCurrentUserId();
		XmlNode xmlNode = userDocument["Root"]["Warriors"];
		bool flag = false;
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			string text = childNode.Attributes["IsFake"].GetStringOrDefault(string.Empty);
			if (text == "True")
			{
				flag = true;
			}
			else if (num == childNode.Attributes["ID"].ParseInt())
			{
				_CurrentUserNode = childNode;
				_roster = CreateRoster(_CurrentUserNode);
				_roster.GetInventory().ApplyItemInfos(GetItems().GetAllItems());
				Eclipse.Modding.ModRuntime.RecordSaveContext(_CurrentUserNode, _roster);
				break;
			}
		}
		_roster.get_Parameters().CalculateAttributes();
		XmlNode hKPPBKPJOEO = userDocument["Root"]["Billing"];
		if (Eclipse.Saves.CampaignSaveSession.PreviewDirectory == null) ApplyBillingPrices(hKPPBKPJOEO);
	}

	private int GetCurrentUserId()
	{
		return userDocument["Root"]["CurrentUser"].Attributes["ID"].ParseInt();
	}

	private void ApplyBillingPrices(XmlNode node)
	{
		if (node == null)
		{
			return;
		}
		List<ItemInfo> list = GetItems().GetItemsByType("RealMoneyItem");
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string name = childNode.Attributes["Name"].GetStringOrDefault();
			if (!string.IsNullOrEmpty(name))
			{
				ItemInfo dJKEECEOCJB = list.Find((ItemInfo DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(name));
				if (dJKEECEOCJB != null)
				{
					dJKEECEOCJB.LocalizedPriceString = childNode.Attributes["RealPrice"].GetStringOrDefault(dJKEECEOCJB.LocalizedPriceString);
					dJKEECEOCJB.PriceAmountText = childNode.Attributes["RealPriceConst"].GetStringOrDefault(dJKEECEOCJB.PriceAmountText);
					dJKEECEOCJB.CurrencyCode = childNode.Attributes["RealPriceCurrency"].GetStringOrDefault(dJKEECEOCJB.CurrencyCode);
				}
			}
		}
	}

	private void LoadWarriors()
	{
		if (stagesDocument == null)
		{
			stagesDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "stages.xml", XmlUtils.XmlSourceMode.Normal, true, XmlCryptoUtils.GetIsEncryptionEnabled());
		}
		warriorsDocument = new XmlDocument();
		warriorsDocument.AppendImportedClone(stagesDocument["Stages"]["Warriors"]);
		XmlNode xmlNode = warriorsDocument["Warriors"]["Templates"];
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			ParseTemplate(childNode);
		}
		ResolveTemplateInheritance();
		xmlNode = warriorsDocument["Warriors"]["WarriorGroups"];
		ParseWarriorGroups(xmlNode);
	}

	// Eclipse modding seam: register a mod-owned warrior template after the base
	// templates were resolved. The parent must already be registered; inheritance
	// is merged exactly like the base resolver (EANLGFEDADB) does for one entry.
	public void AddExternalTemplate(XmlNode node)
	{
		if (node == null || node.Name != "Template")
		{
			throw new ArgumentException("An external warrior template requires a Template node.");
		}
		string name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		if (name == string.Empty || GetTemplateByName(name) != null)
		{
			throw new InvalidOperationException("Warrior template '" + name + "' is already registered.");
		}
		XmlNode owned = warriorsDocument.ImportNode(node, true);
		TemplateUser template = ParseTemplate(owned);
		if (template.ParentTemplateName != string.Empty)
		{
			TemplateUser parent = GetTemplateByName(template.ParentTemplateName);
			if (parent == null)
			{
				templates.Remove(template);
				throw new InvalidOperationException("Warrior template '" + name + "' has no registered parent '" + template.ParentTemplateName + "'.");
			}
			XmlNode merged = warriorsDocument.ImportNode(parent.Parameters.Node ?? parent.node, true);
			merged = MergeUserXML(merged, owned);
			// Parent parameters already contain inherited alignments and groups.
			// Parse only this template's overlay; parsing the merged node appends
			// inherited rows again (Default's seven became fourteen for DE bosses).
			template.Parameters = ParseWarriorParameters(owned, parent.Parameters);
			template.Parameters.Node = merged;
		}
	}

	public void RemoveExternalTemplate(string name)
	{
		TemplateUser template = GetTemplateByName(name);
		if (template != null)
		{
			templates.Remove(template);
		}
	}

	private TemplateUser ParseTemplate(XmlNode node)
	{
		TemplateUser aHIIGFBIGAA = new TemplateUser();
		aHIIGFBIGAA.node = node;
		aHIIGFBIGAA.ParentTemplateName = node.Attributes["Template"].GetStringOrDefault(string.Empty);
		aHIIGFBIGAA.name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		aHIIGFBIGAA.Parameters = ParseWarriorParameters(node, null);
		foreach (TemplateUser item in templates)
		{
			if (item.name == aHIIGFBIGAA.name)
			{
				templates.Remove(item);
				break;
			}
		}
		templates.Add(aHIIGFBIGAA);
		return aHIIGFBIGAA;
	}

	private GroupsUser GetGroupByName(string name)
	{
		foreach (GroupsUser item in warriorGroups)
		{
			if (item.name == name)
			{
				return item;
			}
		}
		return null;
	}

	private void ResolveTemplateInheritance()
	{
		ResolveTemplateInheritance(templates);
	}

	private void ResolveTemplateInheritance(List<TemplateUser> PHEDCOGJGLE)
	{
		foreach (TemplateUser item in templates)
		{
			if (item.ParentTemplateName == null || item.ParentTemplateName.Equals(string.Empty))
			{
				continue;
			}
			TemplateUser aHIIGFBIGAA = GetTemplateByName(item.ParentTemplateName);
			XmlNode xmlNode = warriorsDocument["Warriors"]["Templates_tmp"];
			if (xmlNode == null)
			{
				xmlNode = warriorsDocument["Warriors"].AppendElement("Templates_tmp");
			}
			bool flag = false;
			XmlNode xmlNode2 = null;
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				if (childNode.Attributes["Name"].GetStringOrDefault(string.Empty) == item.name)
				{
					flag = true;
					xmlNode2 = childNode;
					break;
				}
			}
			if (!flag)
			{
				xmlNode2 = xmlNode.AppendImportedClone(aHIIGFBIGAA.Parameters.Node);
				xmlNode2 = MergeUserXML(xmlNode2, item.node);
			}
			item.Parameters = ParseWarriorParameters(xmlNode2, aHIIGFBIGAA.Parameters);
			item.Parameters.Node = xmlNode2;
		}
	}

	private void ParseWarriorGroups(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			GroupsUser oFIHDFNGDLA = new GroupsUser();
			oFIHDFNGDLA.node = childNode;
			oFIHDFNGDLA.name = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			oFIHDFNGDLA.TemplateName = childNode.Attributes["Template"].GetStringOrDefault(string.Empty);
			ParseGroupWarriors(childNode, oFIHDFNGDLA.Models);
			warriorGroups.Add(oFIHDFNGDLA);
		}
	}

	private void ParseGroupWarriors(XmlNode node, List<ModelParameters> target)
	{
		int num = 0;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string text = childNode.Attributes["Template"].GetStringOrDefault(string.Empty);
			TemplateUser aHIIGFBIGAA = ((!(text == string.Empty)) ? GetTemplateByName(text) : null);
			ModelParameters kIKOGDEPGHB = ((aHIIGFBIGAA == null) ? ParseWarriorParameters(childNode, null) : CreateParametersFromTemplate(aHIIGFBIGAA.Parameters, childNode));
			if (kIKOGDEPGHB != null)
			{
				kIKOGDEPGHB.GroupName = childNode.Attributes["Group"].GetStringOrDefault(string.Empty);
				kIKOGDEPGHB.RandomValue = childNode.Attributes["Random"].ParseInt();
				kIKOGDEPGHB.OpponentIndex = num;
				target.Add(kIKOGDEPGHB);
			}
			num++;
		}
	}

    internal ModelParameters CreateFormParameters(XmlNode node, bool player)
    {
        if (node == null || node.Name != "Warrior")
            throw new ArgumentException("A form requires a Warrior definition.");
        // Own the projected node: native item selection can annotate it later.
        var document = new XmlDocument();
        var owned = document.ImportNode(node, true);
        document.AppendChild(owned);
        string templateName = owned.Attributes["Template"]?.Value ?? string.Empty;
        ModelParameters parameters;
        if (templateName.Length == 0) parameters = ParseWarriorParameters(owned, null);
        else
        {
            var template = GetTemplateByName(templateName);
            if (template == null || template.Parameters == null)
                throw new InvalidOperationException("Form template is unavailable: " + templateName);
            parameters = CreateParametersFromTemplate(template.Parameters, owned);
        }
        if (parameters == null) throw new InvalidOperationException("Form parameters could not be constructed.");
        parameters.IsPlayer = player;
        parameters.SceneType = SceneTypes.SceneFight;
        return parameters;
    }

	private ModelParameters CreateParametersFromTemplate(ModelParameters KPAICOOKACB, XmlNode node)
	{
		if (KPAICOOKACB != null)
		{
			ModelParameters kIKOGDEPGHB = ParseWarriorParameters(node, KPAICOOKACB);
			// Clone() shares the template's Node; merge into a copy so resolving a
			// warrior never rewrites its template (later template resolution, such
			// as mod templates layered on Default, reads that node).
			kIKOGDEPGHB.Node = MergeUserXML(kIKOGDEPGHB.Node.CloneNode(true), node);
			return kIKOGDEPGHB;
		}
		return null;
	}

	private Roster CreateRoster(XmlNode node)
	{
		XmlNode equipmentView = Eclipse.Modding.ModSaveData.CreateEquipmentView(node,
			name => GetItems().GetItemByName(name) != null, GameUtils.GetDefaultItem);
		ModelParameters parameters = ParseWarriorParameters(equipmentView, null, false);
		parameters.Node = node;
		Roster nKGLHEGIKKP = new Roster(node, parameters);
		nKGLHEGIKKP.AddEventListener(0, RequestSave);
		return nKGLHEGIKKP;
	}

	public List<Battle> GetBattles()
	{
		return _battles;
	}

	// Additive/removable stage seam for Eclipse's typed Mod API. The recovered parser remains
	// authoritative; callers provide one complete validated Zone node assembled by the host.
	public Zone AddExternalZone(XmlNode node)
	{
		if (node == null || node.Name != "Zone")
		{
			throw new System.ArgumentException("External stage content must provide one Zone node.", "node");
		}
		string name = node.Attributes?["Name"]?.Value ?? string.Empty;
		if (string.IsNullOrEmpty(name))
		{
			throw new System.ArgumentException("External zone Name must not be empty.", "node");
		}
		if (GetZoneByName(name) != null)
		{
			throw new System.InvalidOperationException("Zone already exists: '" + name + "'.");
		}
		Zone zone = ParseZone(node, false);
		zones.Add(zone);
		return zone;
	}

	public bool RemoveExternalZone(string name)
	{
		if (string.IsNullOrEmpty(name)) return false;
		Zone zone = zones.Find((Zone value) => value.get_Name() == name);
		if (zone == null) return false;
		foreach (Battle battle in zone.Battles)
		{
			List<FightList> fights = GetFightsForBattle(battle);
			for (int i = 0; i < fights.Count; i++) RemoveFight(fights[i]);
			_battles.Remove(battle);
		}
		zones.Remove(zone);
		return true;
	}

	public Battle FindBattleForModding(string zoneName, string battleName)
	{
		Zone zone = GetZoneByName(zoneName);
		return zone == null ? null : zone.Battles.Find((Battle value) => value.get_Name() == battleName);
	}

	public Battle AddExternalBattle(string zoneName, XmlNode node)
	{
		if (string.IsNullOrEmpty(zoneName)) throw new System.ArgumentException("Target zone must not be empty.", "zoneName");
		if (node == null || node.Name != "Battle")
			throw new System.ArgumentException("External stage content must provide one Battle node.", "node");
		Zone zone = GetZoneByName(zoneName);
		if (zone == null) throw new System.InvalidOperationException("Zone does not exist: '" + zoneName + "'.");
		string battleName = node.Attributes?["Name"]?.Value ?? string.Empty;
		if (string.IsNullOrEmpty(battleName)) throw new System.ArgumentException("External battle Name must not be empty.", "node");
		if (zone.Battles.Exists((Battle value) => value.get_Name() == battleName))
			throw new System.InvalidOperationException("Battle already exists in zone '" + zoneName + "': '" + battleName + "'.");
		Battle battle = ParseBattle(node, zone, false);
		LinkBattleToRoster(battle);
		battle.OnBattleCreated();
		FightIDS ids = new FightIDS(string.Copy(zone.get_Name()), string.Copy(battle.get_Name()), string.Empty);
		bool available = GetRoster().HasBattle(ids);
		if (battle.get_Type() == BattleType.FightRaid) available = true;
		battle.IsMapVisible = available;
		zone.Battles.Add(battle);
		return battle;
	}

	public bool RemoveExternalBattle(string zoneName, string battleName)
	{
		if (string.IsNullOrEmpty(zoneName) || string.IsNullOrEmpty(battleName)) return false;
		Zone zone = GetZoneByName(zoneName);
		if (zone == null) return false;
		Battle battle = zone.Battles.Find((Battle value) => value.get_Name() == battleName);
		if (battle == null) return false;
		List<FightList> fights = GetFightsForBattle(battle);
		for (int i = 0; i < fights.Count; i++) RemoveFight(fights[i]);
		zone.Battles.Remove(battle);
		_battles.Remove(battle);
		return true;
	}

	private void LoadZones()
	{
		stagesDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "stages.xml", XmlUtils.XmlSourceMode.Normal, true, XmlCryptoUtils.GetIsEncryptionEnabled());
		XmlNode xmlNode = stagesDocument["Stages"]["Zones"];
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			bool cBFACOIOIAK = false;
			zones.Add(ParseZone(childNode, cBFACOIOIAK));
		}
		foreach (FightList item in fights)
		{
			Battle cNAOMDMIGLJ = item.Battle;
			Zone pKCPOJKLMOK = cNAOMDMIGLJ.GetZone();
			item.FightId.SetFightIDSByZBF(string.Copy(pKCPOJKLMOK.get_Name()), string.Copy(cNAOMDMIGLJ.get_Name()), string.Copy(item.Name));
			RosterFight pIGKOIFBOME = _roster.FindSavedFightRecord(item.FightId);
			if (pIGKOIFBOME != null)
			{
				item.SetRosterFight(pIGKOIFBOME);
				item.ResetRandomRules();
			}
		}
		areZonesLoaded = true;
	}

	private Zone ParseZone(XmlNode node, bool CBFACOIOIAK = false)
	{
		string gOHIIMFFFJI = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		string pMFEIPCHENB = node.Attributes["FileName"].GetStringOrDefault(string.Empty);
		bool pENNHKHFEOM = 0 < node.Attributes["Start"].ParseInt();
		ConditionStatus fFFCCEEDMKI = ConditionStatus.StatusOpen;
		uint cDCJKJNGPOE = node.Attributes["RewardDigits"].ParseUint();
		uint mCDAHGPLLDO = node.Attributes["PrizeBaseDigits"].ParseUint();
		Zone pKCPOJKLMOK = new Zone(gOHIIMFFFJI, pMFEIPCHENB, pENNHKHFEOM, fFFCCEEDMKI, 0, cDCJKJNGPOE, mCDAHGPLLDO);
		Roster nKGLHEGIKKP = GetRoster();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			Battle cGJCGEBPCAF = ParseBattle(childNode, pKCPOJKLMOK, CBFACOIOIAK);
			LinkBattleToRoster(cGJCGEBPCAF);
			cGJCGEBPCAF.OnBattleCreated();
			FightIDS dIAIIPCBMFL = new FightIDS(string.Copy(pKCPOJKLMOK.get_Name()), string.Copy(cGJCGEBPCAF.get_Name()), string.Empty);
			bool dCHJDPCEODD = nKGLHEGIKKP.HasBattle(dIAIIPCBMFL);
			if (cGJCGEBPCAF.get_Type() == BattleType.FightRaid)
			{
				dCHJDPCEODD = true;
			}
			cGJCGEBPCAF.IsMapVisible = dCHJDPCEODD;
			pKCPOJKLMOK.Battles.Add(cGJCGEBPCAF);
		}
		return pKCPOJKLMOK;
	}

	private Battle ParseBattle(XmlNode node, Zone HLJKOKMKMLM = null, bool CBFACOIOIAK = false)
	{
		Vector2 mGMMDGFPBLP = new Vector2(node.Attributes["X"].ParseInt(), node.Attributes["Y"].ParseInt());
		string gOHIIMFFFJI = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		string lOKLDPLAPOL = node.Attributes["Alias"].GetStringOrDefault(string.Empty);
		string pEMOECLNECD = node.Attributes["Title"].GetStringOrDefault(string.Empty);
		string aDONPNOBBDE = ((!node.Attributes["Icon"].Empty()) ? node.Attributes["Icon"].GetStringOrDefault(string.Empty) : "training");
		string lHCFHAIDNDP = node.Attributes["Preview"].GetStringOrDefault(string.Empty);
		string eMDJGBHIAIA = node.Attributes["Description"].GetStringOrDefault(string.Empty);
		string lPJNEDFCBOI = node.Attributes["Location"].GetStringOrDefault(GameUtils.DefaultLocation);
		string pINIIFIOECE = node.Attributes["Music"].GetStringOrDefault();
		string text = node.Attributes["Type"].GetStringOrDefault();
		string oAPKHNPPGHP = node.Attributes["RewardImage"].GetStringOrDefault();
		string iHBMPGKIBAN = node.Attributes["ShowResistance"].GetStringOrDefault();
		ushort cDCJKJNGPOE = 0;
		ushort mCDAHGPLLDO = 0;
		if (!node.Attributes["RewardDigits"].Empty())
		{
			cDCJKJNGPOE = (ushort)node.Attributes["RewardDigits"].ParseUint();
		}
		else if (HLJKOKMKMLM != null)
		{
			cDCJKJNGPOE = (ushort)HLJKOKMKMLM.GetRewardDigits();
		}
		if (!node.Attributes["PrizeBaseDigits"].Empty())
		{
			mCDAHGPLLDO = (ushort)node.Attributes["PrizeBaseDigits"].ParseUint();
		}
		else if (HLJKOKMKMLM != null)
		{
			mCDAHGPLLDO = (ushort)HLJKOKMKMLM.GetPrizeBaseDigits();
		}
		Battle cGJCGEBPCAF;
		switch (GetBattleTypeByName(text))
		{
		case BattleType.FightPeriodic:
			cGJCGEBPCAF = new BattlePeriodic(text, mGMMDGFPBLP, gOHIIMFFFJI, aDONPNOBBDE, lHCFHAIDNDP, eMDJGBHIAIA, cDCJKJNGPOE, mCDAHGPLLDO, lOKLDPLAPOL, pEMOECLNECD, lPJNEDFCBOI, pINIIFIOECE, oAPKHNPPGHP, iHBMPGKIBAN);
			break;
		case BattleType.FightReplayable:
		case BattleType.FightBossesReplayable:
		case BattleType.FightFinalReplayable:
		{
			BattleReplayable bKKPCBGAEHC = new BattleReplayable(text, mGMMDGFPBLP, gOHIIMFFFJI, aDONPNOBBDE, lHCFHAIDNDP, eMDJGBHIAIA, cDCJKJNGPOE, mCDAHGPLLDO, lOKLDPLAPOL, pEMOECLNECD, lPJNEDFCBOI, pINIIFIOECE, oAPKHNPPGHP, iHBMPGKIBAN);
			bKKPCBGAEHC.Parse(node);
			cGJCGEBPCAF = bKKPCBGAEHC;
			break;
		}
		case BattleType.FightAscension:
		{
			BattleAscension bGFLODNGLPK = new BattleAscension(text, mGMMDGFPBLP, gOHIIMFFFJI, aDONPNOBBDE, lHCFHAIDNDP, eMDJGBHIAIA, cDCJKJNGPOE, mCDAHGPLLDO, lOKLDPLAPOL, pEMOECLNECD, lPJNEDFCBOI, pINIIFIOECE, oAPKHNPPGHP, iHBMPGKIBAN);
			bGFLODNGLPK.Parse(node);
			cGJCGEBPCAF = bGFLODNGLPK;
			break;
		}
		case BattleType.FightRaid:
		{
			BattleRaid pAHLFJIMKCL = new BattleRaid(text, mGMMDGFPBLP, gOHIIMFFFJI, aDONPNOBBDE, lHCFHAIDNDP, eMDJGBHIAIA, cDCJKJNGPOE, mCDAHGPLLDO, lOKLDPLAPOL, pEMOECLNECD, lPJNEDFCBOI, pINIIFIOECE, oAPKHNPPGHP, iHBMPGKIBAN);
			pAHLFJIMKCL.Parse(node);
			cGJCGEBPCAF = pAHLFJIMKCL;
			break;
		}
		default:
			cGJCGEBPCAF = new Battle(text, mGMMDGFPBLP, gOHIIMFFFJI, aDONPNOBBDE, lHCFHAIDNDP, eMDJGBHIAIA, cDCJKJNGPOE, mCDAHGPLLDO, lOKLDPLAPOL, pEMOECLNECD, lPJNEDFCBOI, pINIIFIOECE, oAPKHNPPGHP, iHBMPGKIBAN);
			break;
		}
		cGJCGEBPCAF.SetZone(HLJKOKMKMLM);
		cGJCGEBPCAF.SetSourceDefinition(node);
		_battles.Add(cGJCGEBPCAF);
		if (CBFACOIOIAK)
		{
			ParseBattleFights(cGJCGEBPCAF);
		}
		return cGJCGEBPCAF;
	}

	private void ParseBattleFights(Battle DPOOIONCEOA)
	{
		XmlNode xmlNode = DPOOIONCEOA.GetSourceDefinition().GetNode();
		string lPJNEDFCBOI = DPOOIONCEOA.GetLocation();
		string pINIIFIOECE = DPOOIONCEOA.GetMusic();
		int num = 0;
		XmlNodeList xmlNodeList = xmlNode.SelectNodes("Fight");
		foreach (XmlNode item in xmlNodeList)
		{
			FightList jDIPBIHBGPF = new FightList();
			ParseFight(jDIPBIHBGPF, item, DPOOIONCEOA.get_Type(), lPJNEDFCBOI, pINIIFIOECE, DPOOIONCEOA);
			DPOOIONCEOA.AddFight(jDIPBIHBGPF, num);
			num++;
		}
		DPOOIONCEOA.LoadedFightCount = 1;
	}

	private void LinkFightToRoster(FightList KGKDKENMAOA)
	{
		Battle cNAOMDMIGLJ = KGKDKENMAOA.Battle;
		Zone pKCPOJKLMOK = cNAOMDMIGLJ.GetZone();
		KGKDKENMAOA.FightId.SetFightIDSByZBF(string.Copy(pKCPOJKLMOK.get_Name()), string.Copy(cNAOMDMIGLJ.get_Name()), string.Copy(KGKDKENMAOA.Name));
		RosterFight pIGKOIFBOME = _roster.FindSavedFightRecord(KGKDKENMAOA.FightId);
		if (pIGKOIFBOME != null)
		{
			KGKDKENMAOA.SetRosterFight(pIGKOIFBOME);
			KGKDKENMAOA.ResetRandomRules();
		}
	}

	private void ParseFightRules(FightList fight, XmlNode node)
	{
		if (node == null)
		{
			return;
		}
		List<Rule> list = new List<Rule>();
		RuleParser.ParseRules(node, list);
		foreach (Rule item in list)
		{
			if (item != null)
			{
				fight.PutRule(item);
			}
		}
	}

	private void ParseFightRewards(FightList fight, XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			RewardStruct lGDIIADDFLH = new RewardStruct(childNode, fight.RewardDigits, fight.PrizeBaseDigits);
			fight.AddReward(lGDIIADDFLH);
		}
		fight.UpdateLevel(GetRoster().GetLevel());
	}

	private void LinkBattleToRoster(Battle DPOOIONCEOA)
	{
		if (DPOOIONCEOA == null)
		{
			GameLog.Error("ListSF::setRosterBattle - battle is NULL");
			return;
		}
		List<RosterBattle> list = _roster.GetSavedBattles();
		foreach (RosterBattle item in list)
		{
			FightIDS mOCEDDJOAEB = new FightIDS(item.GetBattleId());
			Zone pKCPOJKLMOK = DPOOIONCEOA.GetZone();
			if (mOCEDDJOAEB.GetZone() == pKCPOJKLMOK.get_Name() && mOCEDDJOAEB.GetBattle() == DPOOIONCEOA.get_Name())
			{
				DPOOIONCEOA.SetRosterBattle(item);
				item.LinkedBattle = DPOOIONCEOA;
				break;
			}
		}
	}

	private void SetZonesTime(long DGGEIIBGENC)
	{
		int i = 0;
		for (int count = zones.Count; i < count; i++)
		{
			zones[i].SetTime(DGGEIIBGENC);
		}
		int j = 0;
		for (int count2 = secondaryZones.Count; j < count2; j++)
		{
			secondaryZones[j].SetTime(DGGEIIBGENC);
		}
		int k = 0;
		for (int count3 = tertiaryZones.Count; k < count3; k++)
		{
			tertiaryZones[k].SetTime(DGGEIIBGENC);
		}
	}

	private void UpdateRosterTime(long time)
	{
		if (_roster != null)
		{
			_roster.UpdatePowerRegeneration(time);
		}
		MenuController.RefreshEnergyBar();
	}

	private void RequestServerSync(object data = null)
	{
		long num = GetServerTime();
		long aIJKJHMICNH = GetInstance().unusedTimestamp;
		NetworkController.GetInstance().CompleteLogin();
	}

	private void VerifyRosterBalance()
	{
		if (!GameSettings.IsUserDataValidationEnabled())
		{
			return;
		}
		float num = _roster.GetMoney();
		float num2 = _roster.GetBonus();
		int num3 = GetCurrentUserId();
		XmlNode xmlNode = userDocument["Users"];
		Roster nKGLHEGIKKP = null;
		if (xmlNode != null)
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				if (num3 == childNode.Attributes["ID"].ParseInt())
				{
					nKGLHEGIKKP = CreateRoster(childNode);
					break;
				}
			}
		}
		if (nKGLHEGIKKP != null && (float)nKGLHEGIKKP.GetMoney() == num && (float)nKGLHEGIKKP.GetBonus() == num2)
		{
		}
	}

	private void DeliverReadyRecipes(long time)
	{
		List<RecipeItemInfo> list = _roster.GetInventory().GetRecipeDeliveries();
		foreach (RecipeItemInfo item in list)
		{
			if (item != null && item.IsReadyForDelivery(time))
			{
				ApplyRecipeToItem(item);
			}
		}
	}

	private void InitPeriodicBattles()
	{
		BattlePeriodic.InitBattles(1, GetRoster().GetPeriodicPlayTime());
	}

	private XmlNode MergeUserXML(XmlNode FAIPFEKENIM, XmlNode node)
	{
		foreach (XmlAttribute attribute in node.Attributes)
		{
			string name = attribute.Name;
			string value = attribute.Value;
			if (FAIPFEKENIM.Attributes[name] == null)
			{
				FAIPFEKENIM.AppendAttribute(name).Value = value;
				continue;
			}
			string value2 = FAIPFEKENIM.Attributes[name].Value;
			if (value2 != value)
			{
				FAIPFEKENIM.Attributes[name].Value = value;
			}
		}
		if (node["Items"] != null)
		{
			if (FAIPFEKENIM["Items"] != null)
			{
				FAIPFEKENIM.RemoveChild(FAIPFEKENIM["Items"]);
			}
			FAIPFEKENIM.AppendImportedClone(node["Items"]);
		}
		if (node["Perks"] != null)
		{
			if (FAIPFEKENIM["Perks"] != null)
			{
				FAIPFEKENIM.RemoveChild(FAIPFEKENIM["Perks"]);
			}
			FAIPFEKENIM.AppendImportedClone(node["Perks"]);
		}
		if (node["AttributesAlign"] != null)
		{
			if (FAIPFEKENIM["AttributesAlign"] != null)
			{
				FAIPFEKENIM.RemoveChild(FAIPFEKENIM["AttributesAlign"]);
			}
			FAIPFEKENIM.AppendImportedClone(node["AttributesAlign"]);
		}
		return FAIPFEKENIM;
	}

	private static bool OnConsumablePurchased(ItemInfo item, long BCBAOELLEPB)
	{
		return true;
	}

	private static bool BuyRealMoneyItem(ItemInfo item)
	{
		return true;
	}

	private static bool BuyFreeItem(ItemInfo item)
	{
		return true;
	}

	private static void PlayPurchaseSound(ItemAction LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
		case ItemAction.Item_Buy_Gold:
		case ItemAction.Item_Buy_Ruby:
		case ItemAction.Item_Buy_Real:
		case ItemAction.Item_Consumable:
			Sound.PlaySound("snd_buy");
			break;
		case ItemAction.Item_Upgrade_Gold:
		case ItemAction.Item_Upgrade_Ruby:
		case ItemAction.Item_Delivery_Ruby:
			Sound.PlaySound("snd_upgrade");
			break;
		}
	}

	public static List<PerkInfoItem> GetItemEnchantmentsForSide(ItemInfo PJDAGCBPLJE, bool EKBOGDKIHIH)
	{
		if (EKBOGDKIHIH)
		{
			UserItem dKCHDHMLKHN = GetUserItem(PJDAGCBPLJE.Name);
			if (dKCHDHMLKHN != null)
			{
				return dKCHDHMLKHN.GetEnchantments();
			}
		}
		return PJDAGCBPLJE.DefaultEnchantmentPreviews;
	}

	public void RandomizeObscuredVars()
	{
		fights.ForEach((FightList DHDMNHCIPEH) =>
		{
			DHDMNHCIPEH.RandomizeObscuredVars();
		});
	}
}
