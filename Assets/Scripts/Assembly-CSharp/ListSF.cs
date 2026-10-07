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

		public ModelParameters GetRandomModel(List<int> usedIndices)
		{
			int num = NekkiMath.randomInt(Models.Count);
			bool flag;
			do
			{
				flag = false;
				foreach (int item in usedIndices)
				{
					if (item == num)
					{
						num = NekkiMath.randomInt(Models.Count);
						flag = true;
					}
				}
			}
			while (flag);
			usedIndices.Add(num);
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

	public static ItemInfo FindAvailableItemByGroup(string groupId, long unusedTimestamp = 0L)
	{
		List<ItemInfo> list = new List<ItemInfo>();
		List<ItemInfo> list2 = GetItems().GetAllItems();
		foreach (ItemInfo item in list2)
		{
		}
		Roster roster = GetRoster();
		if (roster != null)
		{
			foreach (ItemInfo item2 in list)
			{
				if (item2.GroupId == string.Empty || roster.HasShopLock(item2.GroupId))
				{
					return item2;
				}
			}
		}
		return null;
	}

	public static void ApplyRealMoneyPurchase(ItemInfo item)
	{
		UserItem userItem = ((item == null) ? null : GetUserItem(item.Name));
		if (item != null && userItem == null)
		{
			Roster roster = _roster;
			roster.SetTotalPaymentSum(roster.GetTotalPaymentSum() + item.PriceAmountText.ToFloat());
			Roster aNEHEDFAPCH2 = _roster;
			aNEHEDFAPCH2.SetPaymentCount(aNEHEDFAPCH2.GetPaymentCount() + 1);
			if (item.IsPaid)
			{
				Roster aNEHEDFAPCH3 = _roster;
				aNEHEDFAPCH3.SetPaidMoney((ObscuredLong)((ObscuredLong)(aNEHEDFAPCH3.GetPaidMoney()) + (ObscuredLong)(item.ReceiveGold)));
			}
			_roster.SetMoney(_roster.GetMoney() + (ObscuredLong)(item.ReceiveGold));
			_roster.SetBonus(_roster.GetBonus() + (ObscuredLong)(item.ReceiveBonus), Roster.BalanceChangeType.CHANGE_PAYMENT, true);
			if (MainMenu.get_Instance() != null)
			{
				MainMenu.get_Instance().UpdateMoney();
			}
			switch (item.SubType)
			{
			case "StarterPack":
				AddItem(item, 1, 0L, false);
				break;
			case "UnlimitedEnergy":
				AddItem(item, 1, 0L, false);
				break;
			case "PerkReset":
				AddItem(item, 1, 0L, false);
				_roster.GetPerks().ResetPerks();
				break;
			}
			_roster.RequestSave(true);
		}
	}

	public static ItemInfo FindItemByMarketId(string marketId)
	{
		List<ItemInfo> list = GetItems().GetItemsByMarketId(marketId);
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

	public List<ModelParameters> CreateOpponentParameters(ModelParameters groupParameters, int count)
	{
		List<ModelParameters> list = new List<ModelParameters>();
		if (!groupParameters.GetIsGroup())
		{
			for (int i = 0; i < count; i++)
			{
				list.Add(groupParameters.Clone());
			}
		}
		else
		{
			int num = 0;
			List<List<int>> list2 = new List<List<int>>();
			list2.Resize(groupParameters.GroupModels.Count);
			for (int j = 0; j < count; j++)
			{
				int num2 = 0;
				ModelParameters mergedParameters = null;
				foreach (GroupModel item in groupParameters.GroupModels)
				{
					GroupsUser groupUser = GetGroupByName(item.Name);
					if (groupUser != null)
					{
						ModelParameters kIKOGDEPGHB2 = null;
						if (!item.IsRandom)
						{
							int index = j % groupUser.GetCount();
							kIKOGDEPGHB2 = groupUser.Models[index];
						}
						else if (!item.NoDoubles)
						{
							kIKOGDEPGHB2 = groupUser.GetRandomModel();
						}
						else
						{
							kIKOGDEPGHB2 = groupUser.GetRandomModel(list2[num2]);
							int count2 = groupUser.Models.Count;
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
							if (mergedParameters == null)
							{
								mergedParameters = kIKOGDEPGHB2;
							}
							else
							{
								mergedParameters = ParseWarriorParameters(kIKOGDEPGHB2.Node, mergedParameters);
								AddMergedGroupModel(mergedParameters);
							}
						}
					}
					num2++;
				}
				if (mergedParameters != null)
				{
					ModelParameters kIKOGDEPGHB3 = null;
					if (groupParameters.Node.Attributes["Template"] == null)
					{
						kIKOGDEPGHB3 = ParseWarriorParameters(groupParameters.Node, mergedParameters);
					}
					else
					{
						kIKOGDEPGHB3 = ((mergedParameters == null) ? new ModelParameters() : mergedParameters.Clone());
						ApplyParameterOverrides(kIKOGDEPGHB3, groupParameters);
					}
					list.Add(kIKOGDEPGHB3);
				}
			}
		}
		if (list.Count == 0)
		{
			list.Add(groupParameters.Clone());
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

	public static CheckItems CheckItemPurchase(ItemInfo item, ItemAction action, int count = 1)
	{
		if (item == null || count <= 0) return new CheckItems { Value = -1L };
		if (!HasPurchaseCapacity(item, action, count)) return new CheckItems { Value = -1L };
		GetInstance().VerifyRosterBalance();
		CheckItems checkResult = new CheckItems();
		Roster roster = GetRoster();
		long num = 0L;
		long num2 = 0L;
		bool flag = true;
		switch (action)
		{
		case ItemAction.Item_Buy_Gold:
		case ItemAction.Item_Upgrade_Gold:
			num = roster.GetMoney();
			num2 = CalculatePurchaseTotal(item.GetCoinPrice(), count);
			break;
		case ItemAction.Item_Buy_Ruby:
		case ItemAction.Item_Upgrade_Ruby:
			num = roster.GetBonus();
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
			num = roster.GetBonus();
			num2 = (ObscuredLong)(item.DeliveryGemPrice);
			break;
		case ItemAction.Item_Consumable:
			num = roster.GetBonus();
			num2 = CalculatePurchaseTotal(item.GetGemPrice(), count);
			break;
		case ItemAction.Item_Recipe:
		{
			RecipeItemInfo recipeItem = (RecipeItemInfo)item;
			Recipe recipe = recipeItem.GetRecipe();
			UserItem recipeUserItem = recipeItem.GetUserItem();
			flag = recipe.IsAvailableWithMaterials(recipeUserItem);
			break;
		}
		default:
			GameLog.Error("ListSF::isBuyItem - unknown type: %i", action);
			break;
		}
		// Invalid/overflowing totals must not wrap into an affordable price or balance.
		if (num < 0 || num2 < 0) return new CheckItems { Value = -1L };
		long num3 = num - num2;
		checkResult.Type = CheckItemType.CHECK_ITEM_NONE;
		checkResult.Value = num3;
		if ((action == ItemAction.Item_Buy_Real || action == ItemAction.Item_Free) && !SystemProperties.CheckConnection())
		{
			checkResult.Type = CheckItemType.CHECK_ITEM_NO_NETWORK;
			checkResult.Value = -1L;
		}
		else if (item.ItemLevel > roster.GetLevel())
		{
			checkResult.Type = CheckItemType.CHECK_ITEM_LEVEL;
			checkResult.Value = -1L;
		}
		else if (num3 < 0)
		{
			checkResult.Type = ((action != ItemAction.Item_Buy_Gold && action != ItemAction.Item_Upgrade_Gold) ? CheckItemType.CHECK_ITEM_BONUS : CheckItemType.CHECK_ITEM_MONEY);
			checkResult.Value = -1L;
		}
		else if (num2 < 0)
		{
			checkResult.Type = CheckItemType.CHECK_ITEM_NONE;
			checkResult.Value = -1L;
		}
		else if (action == ItemAction.Item_Recipe && !flag)
		{
			checkResult.Type = CheckItemType.CHECK_ITEM_MATERIALS;
			checkResult.Value = -1L;
		}
		return checkResult;
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

	public static bool BuyItem(ItemInfo item, ItemAction action, long remainingBalance, int count = 1, Action<object> callback = null)
	{
		if (item == null || count <= 0) return false;
		if (!HasPurchaseCapacity(item, action, count)) return false;
		if (action == ItemAction.Item_Buy_Gold || action == ItemAction.Item_Buy_Ruby || action == ItemAction.Item_Consumable)
			return Eclipse.Modding.ModRuntime.SettleItemPurchase(item, count,
				() => ApplyShopPurchase(item, action, remainingBalance, count, callback));
		return ApplyShopPurchase(item, action, remainingBalance, count, callback);
	}

	private static bool ApplyShopPurchase(ItemInfo item, ItemAction action, long remainingBalance, int count, Action<object> callback)
	{
		if (item != null)
		{
			Roster roster = GetRoster();
			long deliveryTime = -1L;
			if (item.Type == "RaidItemPack")
			{
			}
			switch (action)
			{
			case ItemAction.Item_Buy_Gold:
			case ItemAction.Item_Upgrade_Gold:
				RecordCoinShortfall(item);
				roster.SetMoney(remainingBalance);
				// The offline runtime completes shop orders immediately; do not create
				// delivery timestamps that depend on the retired server timer.
				deliveryTime = -1L;
				break;
			case ItemAction.Item_Buy_Ruby:
			case ItemAction.Item_Upgrade_Ruby:
				RecordGemShortfall(item, false);
				roster.SetBonus(remainingBalance, Roster.BalanceChangeType.CHANGE_BUY_ITEM);
				break;
			case ItemAction.Item_Buy_Real:
				return BuyRealMoneyItem(item);
			case ItemAction.Item_Free:
				return BuyFreeItem(item);
			case ItemAction.Item_Consumable:
				RecordGemShortfall(item, false);
				OnConsumablePurchased(item, remainingBalance);
				break;
			case ItemAction.Item_Delivery_Ruby:
			case ItemAction.Item_Recipe_Delivery_Ruby:
				RecordGemShortfall(item, true);
				roster.SetBonus(remainingBalance, Roster.BalanceChangeType.CHANGE_BUY_DELIVERY);
				deliveryTime = 0L;
				break;
			default:
				GameLog.Error("ListSF::buyItem - unknown type: %i", action);
				break;
			}
			if (action == ItemAction.Item_Recipe_Delivery_Ruby)
			{
				CompleteRecipeDelivery((RecipeItemInfo)item, action, remainingBalance, deliveryTime);
			}
			else
			{
				CompleteItemPurchase(item, action, remainingBalance, deliveryTime, count);
			}
			GameUtils.TrackEvent("Virtual Good Purchased");
			MenuController.RefreshMoney();
			PlayPurchaseSound(action);
		}
		return true;
	}

	public static bool CompleteItemPurchase(ItemInfo item, ItemAction action, long remainingBalance, long deliveryTime, int count = 1)
	{
		Roster roster = GetRoster();
		UserItem existingItem = roster.GetInventory().FindItem(item.Name);
		long existingDeliveryTimestamp = ((existingItem == null) ? (-1) : existingItem.GetDeliveryTimestamp());
		bool flag = deliveryTime <= 0 || item.DeliveryTime == 0;
		UserItem dKCHDHMLKHN2 = AddItem(item, count, deliveryTime, flag);
		bool equipAfterPurchase = dKCHDHMLKHN2.GetIsOwned() || flag;
		if (item.Type == "RealMoneyItem" || item.Type == "Consumable")
		{
			equipAfterPurchase = false;
		}
		UseItem(dKCHDHMLKHN2, equipAfterPurchase);
		GameUtils.TrackItemAction(item, action, count, existingDeliveryTimestamp);
		QuestParameters questParameters = GetInstance().GetQuestParameters();
		FightIDS savedFightIds = questParameters.fightIds;
		questParameters.fightIds = FightIDS.Empty();
		questParameters.fightResult = string.Empty;
		questParameters.purchasedItem = dKCHDHMLKHN2.GetInfo();
		if (GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE))
		{
			GetInstance().RunQuestActions();
		}
		questParameters.fightIds = savedFightIds;
		return true;
	}

	public static bool CompleteRecipeDelivery(RecipeItemInfo recipeItem, ItemAction action, long remainingBalance, long deliveryTime, int count = 1)
	{
		if (recipeItem == null || action != ItemAction.Item_Recipe_Delivery_Ruby) return false;
		return ApplyRecipeToItem(recipeItem);
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

	public static bool UseItem(UserItem userItem, bool equip, bool syncRoster = true)
	{
		if (userItem == null)
		{
			return false;
		}
		userItem.RefreshUpgradeState(GetRoster().GetLevel());
		bool spendAfterUse = userItem.GetInfo().SpendAfterUse;
		if (spendAfterUse)
		{
			SpendItem(userItem, userItem.GetCount());
		}
		equip = equip && !spendAfterUse;
		GameLog.Write("   UseItem: " + ((!equip) ? "Unequiped " : "Equiped ") + userItem.get_Name());
		userItem.SetIsEquipped(equip);
		if (syncRoster)
		{
			return GetRoster().EquipItem(userItem, equip);
		}
		return true;
	}

	public static void EquipFallbackItem(ItemInfo item)
	{
		if (item == null)
		{
			return;
		}
		string itemType = item.Type;
		string itemSubType = item.SubType;
		switch (itemType)
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
			if (itemSubType == "RaidCharge")
			{
				UserItem raidChargeItem = GetUserItem("NoRaidCharge");
				UseItem(raidChargeItem, true);
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
		ItemInfo.SpendType spendType = ItemInfo.SpendType.SPEND_TYPE_NONE;
		if (item.GetInfo().Type == "Energy")
		{
			spendType = ItemInfo.SpendType.SPEND_TYPE_ENERGY;
		}
		if (spendType == ItemInfo.SpendType.SPEND_TYPE_ENERGY)
		{
			_roster.RefillPower();
			item.SetCount(item.GetCount() - count);
			return true;
		}
		GameLog.Write("Error - no such SpendItemType");
		return false;
	}

	public static void ClearEquippedSlot(ItemInfo item)
	{
		GetRoster().UnequipItem(item);
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
		UserItem addedItem = AddItem(item, count, 0L, false);
		return GetRoster().EquipItem(addedItem, false);
	}

	public static bool FinishItemDelivery(ItemInfo item)
	{
		if (item == null)
		{
			return false;
		}
		UserItem existingItem = GetUserItem(item.Name);
		existingItem.set_DeliveryTime(0L);
		existingItem.SetDeliveryUpgradeLevel(-1);
		item.SetIsNew(true);
		return existingItem != null && _roster.EquipItem(existingItem, false);
	}

	public static UserItem AddItem(ItemInfo item, int count = 1, long deliveryTime = 0L, bool equip = true, bool applyDefaultEnchantments = true)
	{
		int acquisitionProfile = Eclipse.Modding.ModRuntime.StoryEvents.ProfileGeneration;
		Roster roster = GetRoster();
		if (item.SubType == "UnlimitedEnergy")
		{
			roster.HasUnlimitedEnergy = true;
			MenuController.RefreshEnergyView();
		}
		UserItem existingItem = GetUserItem(item.Name);
		int previousCount = existingItem == null ? 0 : existingItem.GetCount();
		int acquiredCount = previousCount;
		int rosterLevel = roster.GetLevel();
		if (existingItem == null)
		{
			XmlNode itemsNode = roster.GetItemsNode();
			int initialCount = ((deliveryTime <= 0) ? count : 0);
			UserItem dKCHDHMLKHN2 = new UserItem(itemsNode, item.Name, equip, initialCount, -1, deliveryTime);
			dKCHDHMLKHN2.SetInfo(item);
			existingItem = roster.GetInventory().AddItem(dKCHDHMLKHN2);
			acquiredCount = existingItem.GetCount();
			existingItem.RefreshUpgradeState(rosterLevel);
			existingItem.SetIsUpgrade(false);
			if (applyDefaultEnchantments)
			{
				existingItem.ApplyDefaultEnchantments();
			}
		}
		else if (existingItem.GetCount() != 0 || deliveryTime <= 0)
		{
			if (existingItem.GetCount() != 0)
			{
				existingItem.SetIsUpgrade(true);
			}
			if (item.ParentItem == null)
			{
				existingItem.SetCount(existingItem.GetCount() + count);
				acquiredCount = existingItem.GetCount();
			}
			if (deliveryTime <= 0)
			{
				existingItem.SetUpgradeLevel(item.UpgradeLevel);
				existingItem.RefreshUpgradeState(rosterLevel);
			}
			existingItem.set_DeliveryTime(deliveryTime);
			existingItem.SetDeliveryUpgradeLevel(item.UpgradeLevel);
		}
		else
		{
			existingItem.RefreshUpgradeState(rosterLevel);
			existingItem.set_DeliveryTime(deliveryTime);
			existingItem.SetDeliveryUpgradeLevel(-1);
			existingItem.SetIsUpgrade(false);
		}
		if (equip)
		{
			UseItem(existingItem, true);
		}
		Eclipse.Modding.ModRuntime.PublishItemAcquired(roster, item, previousCount, acquiredCount, acquisitionProfile);
		return existingItem;
	}

	public static void RemoveItem(UserItem userItem, int count = 1)
	{
		if (userItem == null)
		{
			return;
		}
		int num = userItem.GetCount();
		if (num < count)
		{
			count = num;
		}
		userItem.SetCount(userItem.GetCount() - count);
		if (userItem.GetCount() == 0 && GetRoster().IsItemEquipped(userItem.GetInfo()))
		{
			if (userItem.GetInfo() != null)
			{
				userItem.GetInfo().RestoreShopVisibility();
			}
			UseItem(userItem, false);
			EquipFallbackItem(userItem.GetInfo());
		}
	}

	public static bool IncreaseExp(uint value)
	{
		return GetRoster().AddExperience(value);
	}

	public static bool AddCoins(long amount)
	{
		long num = GetRoster().GetMoney() + amount;
		if (num < 0)
		{
			return false;
		}
		GetRoster().SetMoney(num);
		return true;
	}

	public static bool SetCoins(long amount)
	{
		if (amount < 0)
		{
			return false;
		}
		GetRoster().SetMoney(amount);
		return true;
	}

	public static bool AddGems(long amount, Roster.BalanceChangeType changeType, bool isForced = false)
	{
		long num = GetRoster().GetBonus() + amount;
		if (num < 0)
		{
			return false;
		}
		GetRoster().SetBonus(num, changeType, isForced);
		return true;
	}

	public static bool SetGems(long value, Roster.BalanceChangeType changeType, bool isForced = false)
	{
		if (value < 0)
		{
			return false;
		}
		GetRoster().SetBonus(value, changeType, isForced);
		return true;
	}

	public static bool ChangeEnergy(int value)
	{
		return GetRoster().ChangePower(value);
	}

	public static Zone GetCurrentZone()
	{
		List<Zone> primaryZoneList = GetInstance().zones;
		foreach (Zone item in primaryZoneList)
		{
			if (item.GetIsStart())
			{
				return item;
			}
		}
		List<Zone> secondaryZoneList = GetInstance().secondaryZones;
		foreach (Zone item2 in secondaryZoneList)
		{
			if (item2.GetIsStart())
			{
				return item2;
			}
		}
		List<Zone> tertiaryZoneList = GetInstance().tertiaryZones;
		foreach (Zone item3 in tertiaryZoneList)
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
		List<Zone> primaryZoneList = GetInstance().zones;
		foreach (Zone item in primaryZoneList)
		{
			if (item.get_Name() == name)
			{
				return item;
			}
		}
		List<Zone> secondaryZoneList = GetInstance().secondaryZones;
		foreach (Zone item2 in secondaryZoneList)
		{
			if (item2.get_Name() == name)
			{
				return item2;
			}
		}
		List<Zone> tertiaryZoneList = GetInstance().tertiaryZones;
		foreach (Zone item3 in tertiaryZoneList)
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

	public static List<FightList> GetFightsForBattle(Battle battle)
	{
		List<FightList> list = new List<FightList>();
		foreach (FightList item in GetInstance().fights)
		{
			if (item.Battle == battle)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static FightList GetFightById(FightIDS fightId)
	{
		if (fightId == null)
		{
			return null;
		}
		List<FightList> allFightLists = GetInstance().fights;
		foreach (FightList item in allFightLists)
		{
			if (item.FightId.Equals(fightId))
			{
				return item;
			}
		}
		Battle battle = GetBattleById(fightId);
		return (battle == null) ? null : battle.GetFightByName(fightId.GetFight());
	}

	public FightList GetFightByIdString(string name)
	{
		FightIDS fightId = new FightIDS(name);
		fightId.SetFightIDSByString(name);
		return GetFightById(fightId);
	}

	public static Battle GetBattleById(FightIDS fightId)
	{
		if (fightId == null)
		{
			return null;
		}
		Zone zone = GetZoneByName(fightId.GetZone());
		if (zone != null)
		{
			return zone.FindBattle(fightId.GetBattle());
		}
		return null;
	}

	public static UserItem GetUserItem(string name)
	{
		return GetRoster().GetInventory().FindItem(name);
	}

	public static RosterFight LoadRosterFight(FightList fight, bool isWin)
	{
		RosterFight rosterFight = null;
		rosterFight = ((!isWin) ? GetRoster().RecordFightLoss(fight.FightId) : GetRoster().RecordFightWin(fight.FightId));
		if (rosterFight != null)
		{
			rosterFight.SetLevel(GetRoster().GetLevel());
			fight.SetRosterFight(rosterFight);
		}
		RefreshConditionStatuses();
		return rosterFight;
	}

	public static bool IsConditionComplete(ConditionFight condition)
	{
		ConditionType conditionType = condition.Type;
		if (conditionType == ConditionType.ConditionFightCount)
		{
			if (!IsFightCountConditionMet(condition))
			{
				return false;
			}
		}
		else
		{
			GameLog.Error("ListSF::isConditionComplete - No handler for this type: " + condition.Type);
		}
		return true;
	}

	public static bool IsFightCountConditionMet(ConditionFight condition)
	{
		int requiredCount = condition.Count;
		ConditionCompare compareType = condition.Compare;
		int num = 0;
		bool result = true;
		ConditionSubType conditionSubType = condition.SubType;
		if (conditionSubType == ConditionSubType.ConditionSubTypeFight)
		{
			RosterFight rosterFight = GetRoster().FindSavedFightRecord(condition.GetFightIds());
			if (rosterFight != null)
			{
				num = rosterFight.GetWinCount();
			}
		}
		else
		{
			GameLog.Error("ListSF::isConditionFightCount - No handler for this object type: " + condition.SubType);
		}
		switch (compareType)
		{
		case ConditionCompare.ConditionMoreEqually:
			result = num >= requiredCount;
			break;
		case ConditionCompare.ConditionLessEqually:
			result = num <= requiredCount;
			break;
		case ConditionCompare.ConditionMore:
			result = num > requiredCount;
			break;
		case ConditionCompare.ConditionLess:
			result = num < requiredCount;
			break;
		case ConditionCompare.ConditionEqually:
			result = num == requiredCount;
			break;
		default:
			GameLog.Error("ListSF::isConditionFightCount - No handler for this compare type: " + compareType);
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
		List<FightList> allFightLists = GetInstance().fights;
		foreach (FightList item in allFightLists)
		{
			UpdateFightStatus(item);
		}
	}

	public static void UpdateFightStatus(FightList fight)
	{
		int replayCount = fight.ReplayCount;
		RosterFight rosterFight = fight.GetRosterFight();
		BattleType battleType = fight.get_Type();
		if (replayCount > 0 && rosterFight != null)
		{
			if (battleType != BattleType.FightPeriodic && battleType != BattleType.FightAscension && rosterFight.GetWinCount() >= replayCount)
			{
				if (battleType == BattleType.FightReplayable || battleType == BattleType.FightBossesReplayable || battleType == BattleType.FightFinalReplayable)
				{
					BattleReplayable replayableBattle = (BattleReplayable)fight.Battle;
					replayableBattle.RefreshFightStatus(fight);
				}
				else
				{
					fight.Status = ConditionStatus.StatusComplete;
				}
				return;
			}
			if (battleType == BattleType.FightAscension)
			{
				BattleAscension ascensionBattle = (BattleAscension)fight.Battle;
				ascensionBattle.RefreshFightStatus(fight);
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
		List<Zone> primaryZoneList = GetInstance().zones;
		foreach (Zone item in primaryZoneList)
		{
			item.UpdateStatus();
		}
		List<Zone> secondaryZoneList = GetInstance().secondaryZones;
		foreach (Zone item2 in secondaryZoneList)
		{
			item2.UpdateStatus();
		}
		List<Zone> tertiaryZoneList = GetInstance().tertiaryZones;
		foreach (Zone item3 in tertiaryZoneList)
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

		public void OnAuthenticate(bool forceSave = false)
		{
            if (_localVersusProfile || Eclipse.Multiplayer.LocalVersusSession.IsActive) return;
			if (Eclipse.Modding.ModRuntime.DeferProfileSave())
		{
			isSaveRequested = true;
			return;
		}
		if (isSaveRequested || forceSave)
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

	public static void SetRosterFileContent(string content)
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
		XmlDocument xmlDocument = XmlUtils.LoadFromString(content);
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

	public bool RaiseQuestEvent(QuestEvent.QuestEventType eventType)
	{
		int storyProfile = Eclipse.Modding.ModRuntime.StoryEvents.ProfileGeneration;
		var notification = Eclipse.Modding.ModRuntime.CaptureStoryEvent(eventType, GetQuestParameters());
		bool handled = _QuestsManager.ActionQuest(eventType);
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

	public void AddActionQuests(List<QuestStage> questStages)
	{
		_QuestsManager.AddActionQuest(questStages);
	}

	public bool IsTutorialComplete()
	{
		Roster roster = GetRoster();
		if (roster != null)
		{
			return roster.GetTutorials().GetIsStoryTutorialActive();
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

	public void HandleAuthenticateResult(bool isAuthenticated)
	{
		Debug.Log("OnAuthenticate, isAuth = " + isAuthenticated);
		StopAuthTimeout();
		if (isAuthenticated)
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

	public bool AddQuestToStek(string name, bool isForced)
	{
		return _QuestsManager.AddQuestToStek(name, isForced);
	}

	public bool AddQuestToStek(QuestStage questStage, bool isForced)
	{
		return _QuestsManager.AddQuestToStek(questStage, isForced);
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

	public string GetBattleTypeName(BattleType battleType)
	{
		foreach (KeyValuePair<string, BattleType> item in battleTypeNames)
		{
			if (item.Value == battleType)
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
		string versionText = version.ToString(true);
		SetDataVersion(versionText);
	}

	public void ClearQuestsStack(List<string> questNames = null)
	{
		_QuestsManager.ClearStack(questNames);
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

	public void LoadQuests(string fileName = "")
	{
		string text = ((!(fileName == string.Empty)) ? fileName : "quests.xml");
		Roster roster = GetRoster();
		if (roster.HasLoadedQuestFile(text))
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
		roster.AddLoadedQuestFile(text);
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

	public void OnFightSelected(FightList fight)
	{
		if (Module.GetInstance().GetCurrentScreenType() != ScreenType.ModuleMap)
		{
		}
	}

	public bool IsAdvertGroupAllowed(string groupList)
	{
		if (groupList == string.Empty)
		{
			return true;
		}
		string[] collection = groupList.Split(',');
		List<string> list = new List<string>(collection);
		int advertGroupId = AdvertSettings.GroupId;
		foreach (string item in list)
		{
			if (advertGroupId == item.ToInt())
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateFightsLevel(int level)
	{
		foreach (FightList item in fights)
		{
			item.UpdateLevel(level);
		}
	}

	public void CreateMissingAchievements()
	{
		List<Achievement> list = GameUtils.AchievementDefinitions.GetEarnedAchievements();
		for (int i = 0; i < list.Count; i++)
		{
			Achievement achievement = list[i];
			if (_roster.GetAchievements().FindAchievement(achievement.Name) == null)
			{
				_roster.GetAchievements().UnlockAchievement(achievement, false, false);
				_roster.GetAchievements().CreateRepostAchievement(achievement.Name);
			}
		}
	}

	public void ShowDataCorruptedDialog(string errorText)
	{
		GameLog.Error("{0}", errorText);
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
		TextTimer.TimerDataStruct timerData = (TextTimer.TimerDataStruct)data;
		timerData.SetTime(GetRoster().GetEnergyRefillTimer());
		if (0 >= timerData.GetTime())
		{
			timerData.SetTime(0L);
		}
	}

	public void UpdateDuelAccessibilityTimer(object data)
	{
		if (data == null)
		{
			GameLog.Error("ListSF::duelAccessibilityTimer ERROR - data is NULL");
			return;
		}
		TextTimer.TimerDataStruct timerData = (TextTimer.TimerDataStruct)data;
		if (BattlePeriodic.GetTime() > 0)
		{
			timerData.SetTime(BattlePeriodic.GetRepeatTime() - BattlePeriodic.GetTime());
		}
		else
		{
			timerData.SetTime(0L);
		}
		if (0 >= timerData.GetTime())
		{
			timerData.SetTime(0L);
		}
	}

	public void UpdateDeliveryTimer(object data)
	{
		if (data == null)
		{
			GameLog.Error("ListSF::deliveryTimer ERROR - data is NULL");
			return;
		}
		TextTimer.TimerDataStruct timerData = (TextTimer.TimerDataStruct)data;
		if (timerData.Data != null)
		{
			UserItem userItem = (UserItem)timerData.Data;
			timerData.SetTime(GameUtils.GetLeftTime(userItem.GetDeliveryTimestamp()));
			if (0 > timerData.GetTime())
			{
				timerData.SetTime(0L);
			}
		}
		else
		{
			timerData.SetTime(0L);
		}
	}

	public void UpdateStartPackTimer(object data)
	{
		if (data == null)
		{
			GameLog.Error("ListSF::startPackTimer ERROR - data is NULL");
			return;
		}
		TextTimer.TimerDataStruct timerData = (TextTimer.TimerDataStruct)data;
		timerData.SetTime(GetRoster().GetStarterPackTimerEndTime() - GetCurrentTime());
		if (0 > timerData.GetTime())
		{
			timerData.SetTime(0L);
		}
	}

	public void UpdateCustomRosterTimer(object data)
	{
		if (data == null)
		{
			GameLog.Error("ListSF::customRosterTimer ERROR - data is NULL");
			return;
		}
		TextTimer.TimerDataStruct timerData = (TextTimer.TimerDataStruct)data;
		if (timerData.Data != null)
		{
			string timerName = (string)timerData.Data;
			RosterTimerContainer timerContainer = GetRoster().GetTimerContainer();
			RosterTimer rosterTimer = timerContainer.FindTimer(timerName);
			if (rosterTimer != null)
			{
				timerData.SetTime(GameUtils.GetLeftTime(rosterTimer.GetEndTimeSeconds()));
				if (timerData.GetTime() < 0)
				{
					timerData.SetTime(0L);
				}
			}
			else
			{
				timerData.SetTime(0L);
			}
		}
		else
		{
			timerData.SetTime(0L);
		}
	}

	public static List<PerkInfoItem> GetItemEnchantments(ItemInfo item, bool useOwnedEnchantments = true)
	{
		if (useOwnedEnchantments)
		{
			UserItem userItem = GetRoster().GetInventory().FindItem(item.Name);
			if (userItem != null)
			{
				return userItem.GetEnchantments();
			}
		}
		return item.DefaultEnchantmentPreviews;
	}

	public void OnItemGiven(ItemInfo item)
	{
	}

	public void AddMergedGroupModel(ModelParameters model)
	{
		mergedGroupModels.Add(model);
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
			UserItem userItem = GetUserItem(text);
			if (userItem != null)
			{
				UseItem(userItem, true);
			}
		}
	}

	public bool ApplyFightRewards(FightResult fightResult)
	{
		return ApplyFightRewards(fightResult.Prize);
	}

	public bool ApplyFightRewards(FightResult.ResultPrizeStruct prize)
	{
		long money = prize.Money;
		long bonus = prize.Bonus;
		bool result = IncreaseExp(prize.exp);
		if (money > 0)
		{
			AddCoins(money);
		}
		if (bonus > 0)
		{
			AddGems(bonus, Roster.BalanceChangeType.CHANGE_FIGHT_REWARD);
		}
		List<CurrencyStruct> list = prize.GetCurrencies();
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
		List<ResistanceStruct> list2 = prize.GetResistances();
		if (list2.Count > 0)
		{
			foreach (ResistanceStruct item2 in list2)
			{
			}
		}
		List<FightResult.ItemGrant> itemGrants = prize.Items;
		if (itemGrants.Count > 0)
		{
			foreach (FightResult.ItemGrant item3 in itemGrants)
			{
				ItemInfo grantedItem = item3.Item;
				RewardItem rewardSource = item3.RewardSource;
				if (grantedItem.ParentItem != null)
				{
					AddItem(grantedItem.ParentItem, 1, 0L, false, false);
				}
				UserItem userItem = AddItem(grantedItem, 1, 0L, false, false);
				int itemLevel = grantedItem.ItemLevel;
				int rosterLevel = _roster.GetLevel();
				userItem.ApplyEnchantments(rewardSource.enchantments, itemLevel, rosterLevel);
			}
		}
		RequestSave();
		return result;
	}

	public ModelParameters ParseWarriorParameters(XmlNode node, ModelParameters baseParameters, bool fillDefaults = true)
	{
		ModelParameters parameters = ((baseParameters == null) ? new ModelParameters() : baseParameters.Clone());
		if (node.Attributes["Voice"] != null)
		{
			parameters.Voice = node.Attributes["Voice"].GetStringOrDefault(string.Empty);
		}
		if (node.Attributes["Number"] != null)
		{
			parameters.OpponentCount = node.Attributes["Number"].ParseInt();
		}
		if (node.Attributes["NoDoubles"] != null)
		{
			parameters.NoDoubles = node.Attributes["NoDoubles"].ParseInt() > 0;
		}
		parameters.RandomValue = node.Attributes["Random"].ParseInt();
		parameters.AutoTuneFactor = node.Attributes["AutoTuneFactor"].ParseFloat();
		parameters.SetLevel((ObscuredInt)(node.Attributes["Level"].ParseInt((ObscuredInt)(parameters.GetLevel()))));
		parameters.Dan = node.Attributes["Dan"].ParseInt(parameters.Dan);
		parameters.Damage = node.Attributes["Damage"].ParseFloat(parameters.Damage);
		parameters.Difficulty = node.Attributes["Difficulty"].ParseFloat(parameters.Difficulty);
		parameters.BeginnerCheat = node.Attributes["BeginnerCheat"] != null && node.Attributes["BeginnerCheat"].ParseInt() > 0;
		parameters.WarriorPower = node.Attributes["WarriorPower"].ParseInt();
		if (node.Attributes["ShieldTotal"] != null)
		{
			parameters.ShieldTotal = Mathf.Max(0, node.Attributes["ShieldTotal"].ParseInt());
			parameters.HasShieldTotalOverride = true;
		}
		parameters.RatingCorrection = node.Attributes["RatingCorrection"].ParseInt();
		parameters.UnknownFlag = node.Attributes["Unknown"].ParseBool();
		List<WarriorAttribute> attributeList = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item in attributeList)
		{
			bool flag = node.Attributes[item.get_Name()] == null;
			if (flag && fillDefaults)
			{
				int attributeValue = 0;
				parameters.BaseAttributes.Get(item.get_Name(), ref attributeValue);
				attributeValue = ((!GameUtils.IsAlignTargetAttribute(item.get_Name())) ? attributeValue : (attributeValue + parameters.WarriorPower));
				parameters.BaseAttributes.Set(item.get_Name(), attributeValue);
			}
			else if (!flag)
			{
				int num = node.Attributes[item.get_Name()].ParseInt();
				num = ((!GameUtils.IsAlignTargetAttribute(item.get_Name())) ? num : (num + parameters.WarriorPower));
				parameters.BaseAttributes.Set(item.get_Name(), num);
			}
		}
		if (node.Attributes["FirstName"] != null)
		{
			parameters.FirstName = node.Attributes["FirstName"].GetStringOrDefault(string.Empty);
		}
		if (node.Attributes["LastName"] != null)
		{
			parameters.LastName = node.Attributes["LastName"].GetStringOrDefault(string.Empty);
		}
		if (node.Attributes["Avatar"] != null)
		{
			parameters.Avatar = node.Attributes["Avatar"].GetStringOrDefault(string.Empty);
		}
		if (node.Attributes["PlayerRating"] != null)
		{
			parameters.SetPlayerRating(node.Attributes["PlayerRating"].ParseFloat());
		}
		if (node.Attributes["EnemyRating"] != null)
		{
			parameters.SetEnemyRating(node.Attributes["EnemyRating"].ParseFloat());
		}
		if (node.Attributes["PlayerRatingMagic"] != null)
		{
			parameters.SetPlayerRatingMagic(node.Attributes["PlayerRatingMagic"].ParseFloat());
		}
		if (node.Attributes["EnemyRatingMagic"] != null)
		{
			parameters.SetEnemyRatingMagic(node.Attributes["EnemyRatingMagic"].ParseFloat());
		}
		if (node.Attributes["PlayerRatingRanged"] != null)
		{
			parameters.SetPlayerRatingRanged(node.Attributes["PlayerRatingRanged"].ParseFloat());
		}
		if (node.Attributes["EnemyRatingRanged"] != null)
		{
			parameters.SetEnemyRatingRanged(node.Attributes["EnemyRatingRanged"].ParseFloat());
		}
		XmlNode xmlNode = node["AttributesAlign"];
		if (xmlNode != null)
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				AttributesAlign attributeAlign = new AttributesAlign();
				attributeAlign.Factor = childNode.Attributes["Factor"].ParseFloat();
				attributeAlign.Shift = childNode.Attributes["Shift"].ParseFloat();
				attributeAlign.Priority = childNode.Attributes["Priority"].ParseInt();
				if (childNode.Attributes["Eclipse"] == null)
				{
					attributeAlign.DifficultyFilter = ModelParameters.DifficultyFilter.DFBoth;
				}
				else if (childNode.Attributes["Eclipse"].ParseInt() == 0)
				{
					attributeAlign.DifficultyFilter = ModelParameters.DifficultyFilter.DFNormal;
				}
				else
				{
					attributeAlign.DifficultyFilter = ModelParameters.DifficultyFilter.DFHard;
				}
				parameters.AttributeAlignments.Add(attributeAlign);
			}
		}
		XmlNode xmlNode3 = node["Groups"];
		if (xmlNode3 != null)
		{
			foreach (XmlNode item2 in xmlNode3)
			{
				GroupModel groupModel = new GroupModel();
				groupModel.Name = item2.Attributes["Name"].GetStringOrDefault(string.Empty);
				groupModel.IsRandom = item2.Attributes["Random"].ParseBool();
				groupModel.NoDoubles = item2.Attributes["NoDoubles"].ParseBool();
				parameters.GroupModels.Add(groupModel);
			}
		}
		if (!fillDefaults)
		{
			if (node.Attributes["Skeleton"] != null)
			{
				parameters.Skeleton = GetItems().GetItemByName(node.Attributes["Skeleton"].GetStringOrDefault(string.Empty));
			}
			if (node.Attributes["Armor"] != null)
			{
				parameters.Armor = GetItems().GetItemByName(node.Attributes["Armor"].GetStringOrDefault(string.Empty));
			}
			if (node.Attributes["Helm"] != null)
			{
				parameters.Helm = GetItems().GetItemByName(node.Attributes["Helm"].GetStringOrDefault(string.Empty));
			}
			if (node.Attributes["Weapon"] != null)
			{
				parameters.Weapon = GetItems().GetItemByName(node.Attributes["Weapon"].GetStringOrDefault(string.Empty));
			}
			if (node.Attributes["Ranged"] != null)
			{
				parameters.Ranged = GetItems().GetItemByName(node.Attributes["Ranged"].GetStringOrDefault(string.Empty));
			}
			if (node.Attributes["Magic"] != null)
			{
				parameters.Magic = GetItems().GetItemByName(node.Attributes["Magic"].GetStringOrDefault(string.Empty));
			}
			if (node.Attributes["RaidCharge"] != null)
			{
				RaidModelParameters raidParameters = parameters as RaidModelParameters;
				if (raidParameters != null)
				{
					raidParameters.RaidChargeItem = GetItems().GetItemByName(node.Attributes["RaidCharge"].GetStringOrDefault(string.Empty));
				}
			}
			if (node.Attributes["Seal"] != null)
			{
				parameters.Seal = GetItems().GetItemByName(node.Attributes["Seal"].GetStringOrDefault(string.Empty));
			}
		}
		else
		{
			XmlNode xmlNode5 = node["Items"];
			if (xmlNode5 != null)
			{
				parameters.DecorateItems.Clear();
				foreach (XmlNode item3 in xmlNode5)
				{
					ItemInfo itemInfo = null;
					if (item3.Attributes["Name"] != null)
					{
						itemInfo = GetItems().GetItemByName(item3.Attributes["Name"].GetStringOrDefault(string.Empty));
					}
					else if (item3.Attributes["Type"] != null)
					{
						itemInfo = parameters.GetItemByType(item3.Attributes["Type"].GetStringOrDefault(string.Empty));
					}
					if (itemInfo != null)
					{
						if (itemInfo.Type.Equals("Decorate"))
						{
							parameters.DecorateItems.Add(itemInfo);
							continue;
						}
						ItemInfo dJKEECEOCJB2 = itemInfo.Clone();
						XmlNode enchantmentsNode = item3["Enchantments"];
						dJKEECEOCJB2.DefaultEnchantments.Clear();
						dJKEECEOCJB2.SetParsedPerks(enchantmentsNode);
						parameters.SetItemByType(dJKEECEOCJB2.Type, dJKEECEOCJB2);
						if (!IsContentLoaded)
						{
							parameters.AddConditionItem(dJKEECEOCJB2);
						}
					}
					else if (!string.IsNullOrEmpty(parameters.GroupName))
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
					string perkName = item4.Attributes["Name"].GetStringOrDefault(string.Empty);
					PerkInfoItem perk = GameUtils.PerkItemList.FindBasePerk(perkName);
					if (perk != null && !parameters.WarriorPerks.Contains(perk))
					{
						if (item4["Set"] != null || item4["RatingEvaluation"] != null)
						{
							perk = perk.Clone(item4["Set"], item4["RatingEvaluation"]);
							parameters.AddWarriorPerk(perk);
						}
						parameters.WarriorPerks.AddIfNotExist(perk);
					}
				}
			}
		}
		Tactic tactic = AiData.GetTacticByName(node.Attributes["Tactic"].GetStringOrDefault(string.Empty));
        if (node.Attributes["EclipseBodyModel"] != null) parameters.EclipseBodyModel = node.Attributes["EclipseBodyModel"].Value;
        if (node.Attributes["EclipseCharacterId"] != null) parameters.EclipseCharacterId = node.Attributes["EclipseCharacterId"].Value;
        if (node["EclipseSkinModels"] != null)
        {
            var models = new List<string>();
            foreach (XmlNode skin in node["EclipseSkinModels"].ChildNodes) models.Add(skin.Attributes["Asset"].Value);
            parameters.EclipseSkinModels = models.ToArray();
        }
		parameters.FightTactic = tactic;
		parameters.AiControlled = node.Attributes["NotAI"] == null;
		parameters.AnimationEnabled = node.Attributes["NotAnimation"] == null;
		parameters.UserControlled = node.Attributes["Controlled"] != null;
		parameters.IsPlayer = false;
		parameters.IsWinner = false;
		parameters.RoundEnded = false;
		parameters.IsDead = false;
		parameters.RoundsWon = 0;
		parameters.MaxLife = 0f;
		if (!parameters.HasSourceNode)
		{
			parameters.Node = node;
			parameters.HasSourceNode = true;
		}
		return parameters;
	}

	public static void RegisterReplayableBattle(BattleReplayable battle)
	{
		replayableBattles.AddIfNotExist(battle);
	}

	public void ParseFight(FightList fight, XmlNode node, BattleType battleType, string defaultLocation, string defaultMusic, Battle battle)
	{
		fight.Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		fight.set_Type(battleType);
		fight.ReplayCount = node.Attributes["Replays"].ParseInt();
		fight.RepeatTime = node.Attributes["ReplayInterval"].ParseInt();
		fight.set_PowerRequired(node.Attributes["Power"].ParseInt(1));
		fight.RoundsToWin = node.Attributes["Rounds"].ParseInt(2);
		fight.RoundTime = (ObscuredInt)(node.Attributes["RoundTime"].ParseInt(60));
		fight.Location = node.Attributes["Location"].GetStringOrDefault(defaultLocation);
		fight.Music = node.Attributes["Music"].GetStringOrDefault(defaultMusic);
		fight.EvaluatedRating = node.Attributes["EvaluatedRating"].ParseFloat(-1f);
		fight.HealthRecovery = node.Attributes["HealthRecovery"].ParseFloat(1f);
		fight.PrizeBase = node.Attributes["PrizeBase"].ParseFloat(-1f);
		fight.set_Description(node.Attributes["Description"].GetStringOrDefault(string.Empty));
		fight.IsLocked = node.Attributes["Locked"].ParseBool();
		fight.RewardImage = node.Attributes["RewardImage"].GetStringOrDefault(string.Empty);
		fight.TrackFightProgress = battleType != BattleType.FightNone;
		ushort rewardDigits = 0;
		ushort prizeBaseDigits = 0;
		if (!node.Attributes["RewardDigits"].Empty())
		{
			rewardDigits = (ushort)node.Attributes["RewardDigits"].ParseUint();
		}
		else if (battle != null)
		{
			rewardDigits = battle.GetRewardDigits();
		}
		if (!node.Attributes["PrizeBaseDigits"].Empty())
		{
			prizeBaseDigits = (ushort)node.Attributes["PrizeBaseDigits"].ParseUint();
		}
		else if (battle != null)
		{
			prizeBaseDigits = battle.GetPrizeBaseDigits();
		}
		fight.RewardDigits = rewardDigits;
		fight.PrizeBaseDigits = prizeBaseDigits;
		ModelParameters opponentParameters = null;
		XmlNode xmlNode = node["Warriors"];
		if (xmlNode != null)
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				if (!childNode.Attributes["Template"].Empty())
				{
					TemplateUser template = GetTemplateByName(childNode.Attributes["Template"].GetStringOrDefault());
					opponentParameters = ((template == null) ? ParseWarriorParameters(childNode, null) : ParseWarriorParameters(childNode, template.Parameters));
				}
				else
				{
					opponentParameters = ParseWarriorParameters(childNode, null);
				}
				opponentParameters.GroupName = childNode.Attributes["Group"].GetStringOrDefault();
				opponentParameters.RandomValue = childNode.Attributes["Random"].ParseInt();
				opponentParameters.Node = childNode;
				fight.AddOpponent(opponentParameters);
			}
		}
		XmlNode rulesNode = node["Rules"];
		XmlNode hKPPBKPJOEO2 = node["Rewards"];
		ParseFightRules(fight, rulesNode);
		// FightIDS is assigned after this parser returns; use the already registered
		// battle metadata to identify offline raids while building their rewards.
		if (battleType != BattleType.FightRaid ||
			(battle != null && Eclipse.Modding.ModPolicies.TryRaidBattle(battle.get_Name(), out _)))
		{
			ParseFightRewards(fight, hKPPBKPJOEO2);
		}
	}

	public void AddFight(FightList fight)
	{
		fights.AddIfNotExist(fight);
		if (areZonesLoaded)
		{
			LinkFightToRoster(fight);
			UpdateFightStatus(fight);
		}
	}

	public void RemoveFight(FightList fight)
	{
		fights.Remove(fight);
	}

	public static void RecordCoinShortfall(ItemInfo item)
	{
		long num = item.GetCoinPrice();
		Roster roster = GetRoster();
		long num2 = roster.GetMoney() - (ObscuredLong)(roster.GetPaidMoney());
		long num3 = (item.MissingCoins = Math.Max(0L, num - num2));
		roster.SetPaidMoney((ObscuredLong)((ObscuredLong)(roster.GetPaidMoney()) - num3));
	}

	public static void RecordGemShortfall(ItemInfo item, bool isDelivery)
	{
		long num = 0L;
		num = (isDelivery ? (long)(ObscuredLong)(item.DeliveryGemPrice) : item.GetGemPrice());
		long missingGems = Math.Max(0L, num - GetRoster().GetUnpaidBonus());
		item.MissingGems = missingGems;
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

	public void ApplyParameterOverrides(ModelParameters targetParameters, ModelParameters sourceParameters)
	{
		if (sourceParameters.SceneType != SceneTypes.SceneNone)
		{
			targetParameters.SceneType = sourceParameters.SceneType;
		}
		if (sourceParameters.AutoTuneFactor != 0f)
		{
			targetParameters.AutoTuneFactor = sourceParameters.AutoTuneFactor;
		}
		if (sourceParameters.Voice.Length != 0)
		{
			targetParameters.Voice = sourceParameters.Voice;
		}
		if (sourceParameters.Skeleton != null)
		{
			targetParameters.Skeleton = sourceParameters.Skeleton;
		}
		if (sourceParameters.Weapon != null)
		{
			targetParameters.Weapon = sourceParameters.Weapon;
		}
		if (sourceParameters.Armor != null)
		{
			targetParameters.Armor = sourceParameters.Armor;
		}
		if (sourceParameters.Helm != null)
		{
			targetParameters.Helm = sourceParameters.Helm;
		}
		if (sourceParameters.Ranged != null)
		{
			targetParameters.Ranged = sourceParameters.Ranged;
		}
		if (sourceParameters.Magic != null)
		{
			targetParameters.Magic = sourceParameters.Magic;
		}
		if (sourceParameters.Seal != null)
		{
			targetParameters.Seal = sourceParameters.Seal;
		}
		RaidModelParameters raidParameters = targetParameters as RaidModelParameters;
		RaidModelParameters kAOPLEPILDH2 = sourceParameters as RaidModelParameters;
		if (raidParameters != null && kAOPLEPILDH2 != null && kAOPLEPILDH2.RaidChargeItem != null)
		{
			raidParameters.RaidChargeItem = kAOPLEPILDH2.RaidChargeItem;
		}
		if (sourceParameters.EndRoundType != EndRoundType.EndRoundTypeNone)
		{
			targetParameters.EndRoundType = sourceParameters.EndRoundType;
		}
		if (sourceParameters.WarriorPower != 0)
		{
			targetParameters.WarriorPower = sourceParameters.WarriorPower;
		}
		if (sourceParameters.HasShieldTotalOverride)
		{
			targetParameters.ShieldTotal = sourceParameters.ShieldTotal;
			targetParameters.HasShieldTotalOverride = true;
		}
		if (sourceParameters.RatingCorrection != 0)
		{
			targetParameters.RatingCorrection = sourceParameters.RatingCorrection;
		}
		if (sourceParameters.Damage != 0f)
		{
			targetParameters.Damage = sourceParameters.Damage;
		}
		if (sourceParameters.Difficulty != 0f)
		{
			targetParameters.Difficulty = sourceParameters.Difficulty;
		}
		if (!string.IsNullOrEmpty(sourceParameters.FirstName))
		{
			targetParameters.FirstName = sourceParameters.FirstName;
		}
		if (!string.IsNullOrEmpty(sourceParameters.LastName))
		{
			targetParameters.LastName = sourceParameters.LastName;
		}
		if (!string.IsNullOrEmpty(sourceParameters.Avatar))
		{
			targetParameters.Avatar = sourceParameters.Avatar;
		}
		if (sourceParameters.Perks.Count != 0)
		{
			foreach (PerkInfoItem item in sourceParameters.Perks)
			{
				targetParameters.Perks.AddIfNotExist(item);
			}
		}
		if (sourceParameters.WarriorPerks.Count != 0)
		{
			foreach (PerkInfoItem item2 in sourceParameters.WarriorPerks)
			{
				targetParameters.WarriorPerks.AddIfNotExist(item2);
			}
		}
		if (sourceParameters.LearnedPerks.Count != 0)
		{
			foreach (PerkInfoItem item3 in sourceParameters.LearnedPerks)
			{
				targetParameters.LearnedPerks.AddIfNotExist(item3);
			}
		}
		if (sourceParameters.GetPlayerRating() != -1f)
		{
			targetParameters.SetPlayerRating(sourceParameters.GetPlayerRating());
		}
		if (sourceParameters.GetEnemyRating() != -1f)
		{
			targetParameters.SetEnemyRating(sourceParameters.GetEnemyRating());
		}
		if (sourceParameters.GetPlayerRatingMagic() != -1f)
		{
			targetParameters.SetPlayerRatingMagic(sourceParameters.GetPlayerRatingMagic());
		}
		if (sourceParameters.GetEnemyRatingMagic() != -1f)
		{
			targetParameters.SetEnemyRatingMagic(sourceParameters.GetEnemyRatingMagic());
		}
		if (sourceParameters.GetPlayerRatingRanged() != -1f)
		{
			targetParameters.SetPlayerRatingRanged(sourceParameters.GetPlayerRatingRanged());
		}
		if (sourceParameters.GetEnemyRatingRanged() != -1f)
		{
			targetParameters.SetEnemyRatingRanged(sourceParameters.GetEnemyRatingRanged());
		}
		if (sourceParameters.FightTactic != null)
		{
			targetParameters.FightTactic = sourceParameters.FightTactic;
		}
		if (sourceParameters.AttributeAlignments.Count != 0)
		{
			foreach (AttributesAlign item4 in sourceParameters.AttributeAlignments)
			{
				targetParameters.AttributeAlignments.Add(new AttributesAlign(item4));
			}
		}
		List<WarriorAttribute> attributeList = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item5 in attributeList)
		{
			string attributeName = item5.get_Name();
			int OEMALIFPGPO2 = 0;
			if (sourceParameters.FinalAttributes.Get(attributeName, ref OEMALIFPGPO2))
			{
				targetParameters.FinalAttributes.Set(attributeName, OEMALIFPGPO2);
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
			RosterTimerContainer timerContainer = _roster.GetTimerContainer();
			timerContainer.CheckTimers(currentTime);
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
		XmlNode billingNode = userDocument["Root"]["Billing"];
		if (Eclipse.Saves.CampaignSaveSession.PreviewDirectory == null) ApplyBillingPrices(billingNode);
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
				ItemInfo itemInfo = list.Find((ItemInfo candidate) => candidate.Name.Equals(name));
				if (itemInfo != null)
				{
					itemInfo.LocalizedPriceString = childNode.Attributes["RealPrice"].GetStringOrDefault(itemInfo.LocalizedPriceString);
					itemInfo.PriceAmountText = childNode.Attributes["RealPriceConst"].GetStringOrDefault(itemInfo.PriceAmountText);
					itemInfo.CurrencyCode = childNode.Attributes["RealPriceCurrency"].GetStringOrDefault(itemInfo.CurrencyCode);
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
		TemplateUser template = new TemplateUser();
		template.node = node;
		template.ParentTemplateName = node.Attributes["Template"].GetStringOrDefault(string.Empty);
		template.name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		template.Parameters = ParseWarriorParameters(node, null);
		foreach (TemplateUser item in templates)
		{
			if (item.name == template.name)
			{
				templates.Remove(item);
				break;
			}
		}
		templates.Add(template);
		return template;
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

	private void ResolveTemplateInheritance(List<TemplateUser> templateList)
	{
		foreach (TemplateUser item in templates)
		{
			if (item.ParentTemplateName == null || item.ParentTemplateName.Equals(string.Empty))
			{
				continue;
			}
			TemplateUser template = GetTemplateByName(item.ParentTemplateName);
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
				xmlNode2 = xmlNode.AppendImportedClone(template.Parameters.Node);
				xmlNode2 = MergeUserXML(xmlNode2, item.node);
			}
			item.Parameters = ParseWarriorParameters(xmlNode2, template.Parameters);
			item.Parameters.Node = xmlNode2;
		}
	}

	private void ParseWarriorGroups(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			GroupsUser group = new GroupsUser();
			group.node = childNode;
			group.name = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			group.TemplateName = childNode.Attributes["Template"].GetStringOrDefault(string.Empty);
			ParseGroupWarriors(childNode, group.Models);
			warriorGroups.Add(group);
		}
	}

	private void ParseGroupWarriors(XmlNode node, List<ModelParameters> target)
	{
		int num = 0;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string text = childNode.Attributes["Template"].GetStringOrDefault(string.Empty);
			TemplateUser template = ((!(text == string.Empty)) ? GetTemplateByName(text) : null);
			ModelParameters parameters = ((template == null) ? ParseWarriorParameters(childNode, null) : CreateParametersFromTemplate(template.Parameters, childNode));
			if (parameters != null)
			{
				parameters.GroupName = childNode.Attributes["Group"].GetStringOrDefault(string.Empty);
				parameters.RandomValue = childNode.Attributes["Random"].ParseInt();
				parameters.OpponentIndex = num;
				target.Add(parameters);
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

	private ModelParameters CreateParametersFromTemplate(ModelParameters templateParameters, XmlNode node)
	{
		if (templateParameters != null)
		{
			ModelParameters parameters = ParseWarriorParameters(node, templateParameters);
			// Clone() shares the template's Node; merge into a copy so resolving a
			// warrior never rewrites its template (later template resolution, such
			// as mod templates layered on Default, reads that node).
			parameters.Node = MergeUserXML(parameters.Node.CloneNode(true), node);
			return parameters;
		}
		return null;
	}

	private Roster CreateRoster(XmlNode node)
	{
		XmlNode equipmentView = Eclipse.Modding.ModSaveData.CreateEquipmentView(node,
			name => GetItems().GetItemByName(name) != null, GameUtils.GetDefaultItem);
		ModelParameters parameters = ParseWarriorParameters(equipmentView, null, false);
		parameters.Node = node;
		Roster roster = new Roster(node, parameters);
		roster.AddEventListener(0, RequestSave);
		return roster;
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
			bool parseFights = false;
			zones.Add(ParseZone(childNode, parseFights));
		}
		foreach (FightList item in fights)
		{
			Battle battle = item.Battle;
			Zone zone = battle.GetZone();
			item.FightId.SetFightIDSByZBF(string.Copy(zone.get_Name()), string.Copy(battle.get_Name()), string.Copy(item.Name));
			RosterFight rosterFight = _roster.FindSavedFightRecord(item.FightId);
			if (rosterFight != null)
			{
				item.SetRosterFight(rosterFight);
				item.ResetRandomRules();
			}
		}
		areZonesLoaded = true;
	}

	private Zone ParseZone(XmlNode node, bool parseFights = false)
	{
		string zoneName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		string fileName = node.Attributes["FileName"].GetStringOrDefault(string.Empty);
		bool isStart = 0 < node.Attributes["Start"].ParseInt();
		ConditionStatus status = ConditionStatus.StatusOpen;
		uint rewardDigits = node.Attributes["RewardDigits"].ParseUint();
		uint prizeBaseDigits = node.Attributes["PrizeBaseDigits"].ParseUint();
		Zone zone = new Zone(zoneName, fileName, isStart, status, 0, rewardDigits, prizeBaseDigits);
		Roster roster = GetRoster();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			Battle battle = ParseBattle(childNode, zone, parseFights);
			LinkBattleToRoster(battle);
			battle.OnBattleCreated();
			FightIDS battleFightId = new FightIDS(string.Copy(zone.get_Name()), string.Copy(battle.get_Name()), string.Empty);
			bool isMapVisible = roster.HasBattle(battleFightId);
			if (battle.get_Type() == BattleType.FightRaid)
			{
				isMapVisible = true;
			}
			battle.IsMapVisible = isMapVisible;
			zone.Battles.Add(battle);
		}
		return zone;
	}

	private Battle ParseBattle(XmlNode node, Zone parentZone = null, bool parseFights = false)
	{
		Vector2 mGMMDGFPBLP = new Vector2(node.Attributes["X"].ParseInt(), node.Attributes["Y"].ParseInt());
		string battleName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		string alias = node.Attributes["Alias"].GetStringOrDefault(string.Empty);
		string title = node.Attributes["Title"].GetStringOrDefault(string.Empty);
		string icon = ((!node.Attributes["Icon"].Empty()) ? node.Attributes["Icon"].GetStringOrDefault(string.Empty) : "training");
		string preview = node.Attributes["Preview"].GetStringOrDefault(string.Empty);
		string description = node.Attributes["Description"].GetStringOrDefault(string.Empty);
		string location = node.Attributes["Location"].GetStringOrDefault(GameUtils.DefaultLocation);
		string music = node.Attributes["Music"].GetStringOrDefault();
		string text = node.Attributes["Type"].GetStringOrDefault();
		string rewardImage = node.Attributes["RewardImage"].GetStringOrDefault();
		string showResistance = node.Attributes["ShowResistance"].GetStringOrDefault();
		ushort rewardDigits = 0;
		ushort prizeBaseDigits = 0;
		if (!node.Attributes["RewardDigits"].Empty())
		{
			rewardDigits = (ushort)node.Attributes["RewardDigits"].ParseUint();
		}
		else if (parentZone != null)
		{
			rewardDigits = (ushort)parentZone.GetRewardDigits();
		}
		if (!node.Attributes["PrizeBaseDigits"].Empty())
		{
			prizeBaseDigits = (ushort)node.Attributes["PrizeBaseDigits"].ParseUint();
		}
		else if (parentZone != null)
		{
			prizeBaseDigits = (ushort)parentZone.GetPrizeBaseDigits();
		}
		Battle battle;
		switch (GetBattleTypeByName(text))
		{
		case BattleType.FightPeriodic:
			battle = new BattlePeriodic(text, mGMMDGFPBLP, battleName, icon, preview, description, rewardDigits, prizeBaseDigits, alias, title, location, music, rewardImage, showResistance);
			break;
		case BattleType.FightReplayable:
		case BattleType.FightBossesReplayable:
		case BattleType.FightFinalReplayable:
		{
			BattleReplayable replayableBattle = new BattleReplayable(text, mGMMDGFPBLP, battleName, icon, preview, description, rewardDigits, prizeBaseDigits, alias, title, location, music, rewardImage, showResistance);
			replayableBattle.Parse(node);
			battle = replayableBattle;
			break;
		}
		case BattleType.FightAscension:
		{
			BattleAscension ascensionBattle = new BattleAscension(text, mGMMDGFPBLP, battleName, icon, preview, description, rewardDigits, prizeBaseDigits, alias, title, location, music, rewardImage, showResistance);
			ascensionBattle.Parse(node);
			battle = ascensionBattle;
			break;
		}
		case BattleType.FightRaid:
		{
			BattleRaid raidBattle = new BattleRaid(text, mGMMDGFPBLP, battleName, icon, preview, description, rewardDigits, prizeBaseDigits, alias, title, location, music, rewardImage, showResistance);
			raidBattle.Parse(node);
			battle = raidBattle;
			break;
		}
		default:
			battle = new Battle(text, mGMMDGFPBLP, battleName, icon, preview, description, rewardDigits, prizeBaseDigits, alias, title, location, music, rewardImage, showResistance);
			break;
		}
		battle.SetZone(parentZone);
		battle.SetSourceDefinition(node);
		_battles.Add(battle);
		if (parseFights)
		{
			ParseBattleFights(battle);
		}
		return battle;
	}

	private void ParseBattleFights(Battle battle)
	{
		XmlNode xmlNode = battle.GetSourceDefinition().GetNode();
		string location = battle.GetLocation();
		string music = battle.GetMusic();
		int num = 0;
		XmlNodeList xmlNodeList = xmlNode.SelectNodes("Fight");
		foreach (XmlNode item in xmlNodeList)
		{
			FightList fight = new FightList();
			ParseFight(fight, item, battle.get_Type(), location, music, battle);
			battle.AddFight(fight, num);
			num++;
		}
		battle.LoadedFightCount = 1;
	}

	private void LinkFightToRoster(FightList fight)
	{
		Battle battle = fight.Battle;
		Zone zone = battle.GetZone();
		fight.FightId.SetFightIDSByZBF(string.Copy(zone.get_Name()), string.Copy(battle.get_Name()), string.Copy(fight.Name));
		RosterFight rosterFight = _roster.FindSavedFightRecord(fight.FightId);
		if (rosterFight != null)
		{
			fight.SetRosterFight(rosterFight);
			fight.ResetRandomRules();
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
			RewardStruct reward = new RewardStruct(childNode, fight.RewardDigits, fight.PrizeBaseDigits);
			fight.AddReward(reward);
		}
		fight.UpdateLevel(GetRoster().GetLevel());
	}

	private void LinkBattleToRoster(Battle battle)
	{
		if (battle == null)
		{
			GameLog.Error("ListSF::setRosterBattle - battle is NULL");
			return;
		}
		List<RosterBattle> list = _roster.GetSavedBattles();
		foreach (RosterBattle item in list)
		{
			FightIDS fightId = new FightIDS(item.GetBattleId());
			Zone zone = battle.GetZone();
			if (fightId.GetZone() == zone.get_Name() && fightId.GetBattle() == battle.get_Name())
			{
				battle.SetRosterBattle(item);
				item.LinkedBattle = battle;
				break;
			}
		}
	}

	private void SetZonesTime(long time)
	{
		int i = 0;
		for (int count = zones.Count; i < count; i++)
		{
			zones[i].SetTime(time);
		}
		int j = 0;
		for (int count2 = secondaryZones.Count; j < count2; j++)
		{
			secondaryZones[j].SetTime(time);
		}
		int k = 0;
		for (int count3 = tertiaryZones.Count; k < count3; k++)
		{
			tertiaryZones[k].SetTime(time);
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
		long lastTimestamp = GetInstance().unusedTimestamp;
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
		Roster roster = null;
		if (xmlNode != null)
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				if (num3 == childNode.Attributes["ID"].ParseInt())
				{
					roster = CreateRoster(childNode);
					break;
				}
			}
		}
		if (roster != null && (float)roster.GetMoney() == num && (float)roster.GetBonus() == num2)
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

	private XmlNode MergeUserXML(XmlNode targetNode, XmlNode node)
	{
		foreach (XmlAttribute attribute in node.Attributes)
		{
			string name = attribute.Name;
			string value = attribute.Value;
			if (targetNode.Attributes[name] == null)
			{
				targetNode.AppendAttribute(name).Value = value;
				continue;
			}
			string value2 = targetNode.Attributes[name].Value;
			if (value2 != value)
			{
				targetNode.Attributes[name].Value = value;
			}
		}
		if (node["Items"] != null)
		{
			if (targetNode["Items"] != null)
			{
				targetNode.RemoveChild(targetNode["Items"]);
			}
			targetNode.AppendImportedClone(node["Items"]);
		}
		if (node["Perks"] != null)
		{
			if (targetNode["Perks"] != null)
			{
				targetNode.RemoveChild(targetNode["Perks"]);
			}
			targetNode.AppendImportedClone(node["Perks"]);
		}
		if (node["AttributesAlign"] != null)
		{
			if (targetNode["AttributesAlign"] != null)
			{
				targetNode.RemoveChild(targetNode["AttributesAlign"]);
			}
			targetNode.AppendImportedClone(node["AttributesAlign"]);
		}
		return targetNode;
	}

	private static bool OnConsumablePurchased(ItemInfo item, long remainingBalance)
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

	private static void PlayPurchaseSound(ItemAction action)
	{
		switch (action)
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

	public static List<PerkInfoItem> GetItemEnchantmentsForSide(ItemInfo item, bool useOwnedEnchantments)
	{
		if (useOwnedEnchantments)
		{
			UserItem userItem = GetUserItem(item.Name);
			if (userItem != null)
			{
				return userItem.GetEnchantments();
			}
		}
		return item.DefaultEnchantmentPreviews;
	}

	public void RandomizeObscuredVars()
	{
		fights.ForEach((FightList candidate) =>
		{
			candidate.RandomizeObscuredVars();
		});
	}
}
