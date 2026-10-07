using System.Xml;

public class QuestEvent
{
	public enum QuestEventType
	{
		QUEST_EVENT_NONE = 0,
		QUEST_EVENT_FIGHT_ENTER = 1,
		QUEST_EVENT_FIGHT_END = 2,
		QUEST_EVENT_RAID_FIGHT_ENTER = 3,
		QUEST_EVENT_RAID_FIGHT_END = 4,
		QUEST_EVENT_LEVEL_UP = 5,
		QUEST_EVENT_GOT_ITEM = 6,
		QUEST_EVENT_DIALOG = 7,
		QUEST_EVENT_SESSION = 8,
		QUEST_EVENT_ACTIVATE = 9,
		QUEST_EVENT_PURCHASE = 10,
		QUEST_EVENT_PREPURCHASE = 11,
		QUEST_EVENT_LOGIN_FB = 12,
		QUEST_EVENT_DELIVERY = 13,
		QUEST_EVENT_ENERGY = 14,
		QUEST_EVENT_SERVER_CURRENCY = 15,
		QUEST_EVENT_LANGUAGE_SWITCH = 16,
		QUEST_EVENT_FREE_SECTION_BUTTON = 17,
		QUEST_EVENT_START_APPLICATION = 18,
		QUEST_EVENT_CHANGE_TAB = 19,
		QUEST_EVENT_PURCHASE_UNSUCCESSFUL = 20,
		QUEST_EVENT_STARTER_PACK_PRESS = 21,
		QUEST_EVENT_ENERGY_BAR_PRESS = 22,
		QUEST_EVENT_VIDEO_BUTTON_PRESS = 23,
		QUEST_EVENT_TIMER_END = 24,
		QUEST_EVENT_MAP_BUTTON_PRESS = 25,
		QUEST_EVENT_RAID_MAP_BUTTON_PRESS = 26,
		QUEST_EVENT_ENCHANTMENT = 27,
		QUEST_EVENT_ENCHANTMENT_UNSUCCESSFUL = 28,
		QUEST_EVENT_ACTIVATE_PERK = 29,
		QUEST_EVENT_DIACTIVATE_PERK = 30,
		QUEST_EVENT_BUY_SPIN_GEMS = 31,
		QUEST_EVENT_RESET_ASCENSION = 32,
		QUEST_EVENT_SET_ITEM_ACQUIRED = 33,
		QUEST_EVENT_DUEL_UNLOCKED = 34,
		QUEST_EVENT_RAID_OPEN = 35,
		QUEST_EVENT_RAID_ENTER = 36,
		QUEST_EVENT_RAID_END = 37,
		QUEST_EVENT_BOSS_SHIELD_DESTR = 38,
		QUEST_EVENT_RAID_LOGIN = 39,
		QUEST_EVENT_SEAS_START_WITH_REST = 40,
		QUEST_EVENT_SEAS_START_WITHOUT_REST = 41,
		QUEST_EVENT_DAILY_WINDOW_OPEN = 42,
		QUEST_EVENT_LEADERBOARD_TAP = 43,
		QUEST_EVENT_SHOW_REWARDED_VIDEO = 44,
		QUEST_EVENT_LOGIN_END = 45,
		QUEST_EVENT_SHOP_ENTER = 46,
		QUEST_EVENT_SCENE_LOADED = 47,
		QUEST_EVENT_SHOP_BUTTON_PRESS = 48,
        QUEST_EVENT_RAID_MAP_ENTER = 49,
        QUEST_EVENT_RAID_FLOOR_CHANGED = 50,
        QUEST_EVENT_SHOW_RAID_LOOT = 51
	}

	public QuestEventType eventType;

	public virtual void Parse(XmlNode node)
	{
		eventType = ParseEventType(node.Name);
	}

	public bool IsEvent(QuestEventType LJICOHPCPKO)
	{
		return LJICOHPCPKO == eventType;
	}

	public static QuestEventType ParseEventType(string LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
        case "RaidMapEnter": return QuestEventType.QUEST_EVENT_RAID_MAP_ENTER;
        case "RaidFloorChanged": return QuestEventType.QUEST_EVENT_RAID_FLOOR_CHANGED;
        case "ShowRaidLoot": return QuestEventType.QUEST_EVENT_SHOW_RAID_LOOT;

		case "FightEnter":
			return QuestEventType.QUEST_EVENT_FIGHT_ENTER;
		case "FightEnd":
			return QuestEventType.QUEST_EVENT_FIGHT_END;
		case "RaidFightEnter":
			return QuestEventType.QUEST_EVENT_RAID_FIGHT_ENTER;
		case "RaidFightEnd":
			return QuestEventType.QUEST_EVENT_RAID_FIGHT_END;
		case "LevelUp":
			return QuestEventType.QUEST_EVENT_LEVEL_UP;
		case "GotItem":
			return QuestEventType.QUEST_EVENT_GOT_ITEM;
		case "Dialog":
			return QuestEventType.QUEST_EVENT_DIALOG;
		case "SessionStart":
			return QuestEventType.QUEST_EVENT_SESSION;
		case "Activate":
			return QuestEventType.QUEST_EVENT_ACTIVATE;
		case "Purchase":
			return QuestEventType.QUEST_EVENT_PURCHASE;
		case "PrePurchase":
			return QuestEventType.QUEST_EVENT_PREPURCHASE;
		case "LoginFacebook":
			return QuestEventType.QUEST_EVENT_LOGIN_FB;
		case "Delivery":
			return QuestEventType.QUEST_EVENT_DELIVERY;
		case "EnergyChanged":
			return QuestEventType.QUEST_EVENT_ENERGY;
		case "ServerCurrencyMessage":
			return QuestEventType.QUEST_EVENT_SERVER_CURRENCY;
		case "SettingsLangSwitch":
			return QuestEventType.QUEST_EVENT_LANGUAGE_SWITCH;
		case "FreeSectionButton":
			return QuestEventType.QUEST_EVENT_FREE_SECTION_BUTTON;
		case "ApplicationStart":
			return QuestEventType.QUEST_EVENT_START_APPLICATION;
		case "ChangeTab":
			return QuestEventType.QUEST_EVENT_CHANGE_TAB;
		case "PurchaseUnsuccessful":
			return QuestEventType.QUEST_EVENT_PURCHASE_UNSUCCESSFUL;
		case "StarterPackPress":
			return QuestEventType.QUEST_EVENT_STARTER_PACK_PRESS;
		case "EnergyBarPress":
			return QuestEventType.QUEST_EVENT_ENERGY_BAR_PRESS;
		case "VideoButtonPress":
			return QuestEventType.QUEST_EVENT_VIDEO_BUTTON_PRESS;
		case "TimerEnd":
			return QuestEventType.QUEST_EVENT_TIMER_END;
		case "MapButtonPress":
			return QuestEventType.QUEST_EVENT_MAP_BUTTON_PRESS;
		case "RaidMapButtonPress":
			return QuestEventType.QUEST_EVENT_RAID_MAP_BUTTON_PRESS;
		case "Enchantment":
			return QuestEventType.QUEST_EVENT_ENCHANTMENT;
		case "EnchantmentUnsuccessful":
			return QuestEventType.QUEST_EVENT_ENCHANTMENT_UNSUCCESSFUL;
		case "ActivatePerk":
			return QuestEventType.QUEST_EVENT_ACTIVATE_PERK;
		case "DeactivatePerk":
			return QuestEventType.QUEST_EVENT_DIACTIVATE_PERK;
		case "BuySpinGems":
			return QuestEventType.QUEST_EVENT_BUY_SPIN_GEMS;
		case "AscensionReset":
			return QuestEventType.QUEST_EVENT_RESET_ASCENSION;
		case "SetItemAcquired":
			return QuestEventType.QUEST_EVENT_SET_ITEM_ACQUIRED;
		case "DuelUnlocked":
			return QuestEventType.QUEST_EVENT_DUEL_UNLOCKED;
		case "OpenRaids":
			return QuestEventType.QUEST_EVENT_RAID_OPEN;
		case "RaidEnter":
			return QuestEventType.QUEST_EVENT_RAID_ENTER;
		case "RaidEnd":
			return QuestEventType.QUEST_EVENT_RAID_END;
		case "BossShieldDestroyed":
			return QuestEventType.QUEST_EVENT_BOSS_SHIELD_DESTR;
		case "RaidLogin":
			return QuestEventType.QUEST_EVENT_RAID_LOGIN;
		case "SeasonStartWithRestore":
			return QuestEventType.QUEST_EVENT_SEAS_START_WITH_REST;
		case "SeasonStartWithoutRestore":
			return QuestEventType.QUEST_EVENT_SEAS_START_WITHOUT_REST;
		case "DailyWindowOpen":
			return QuestEventType.QUEST_EVENT_DAILY_WINDOW_OPEN;
		case "LeaderBoardTap":
			return QuestEventType.QUEST_EVENT_LEADERBOARD_TAP;
		case "ShowRewardedVideo":
			return QuestEventType.QUEST_EVENT_SHOW_REWARDED_VIDEO;
		case "LoginEnd":
			return QuestEventType.QUEST_EVENT_LOGIN_END;
		case "ShopEnter":
			return QuestEventType.QUEST_EVENT_SHOP_ENTER;
		case "SceneLoaded":
			return QuestEventType.QUEST_EVENT_SCENE_LOADED;
		case "ShopButtonPress":
			return QuestEventType.QUEST_EVENT_SHOP_BUTTON_PRESS;
		default:
			if (Eclipse.Content.QuestCompatibility.IsDeferredQuestEvent(LFLGCDNKNJI))
			{
				return QuestEventType.QUEST_EVENT_NONE;
			}
			GameLog.Error(string.Format("{0} {1}", "Unknown event type: ", LFLGCDNKNJI));
			return QuestEventType.QUEST_EVENT_NONE;
		}
	}
}
