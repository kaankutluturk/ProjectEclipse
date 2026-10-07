using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Nekki.SF2.Core.Quests;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Map;
using UnityEngine;

public class QuestAction : global::EventDispatcher<object>
{
	public enum InputLockMode
	{
		LOCK_NONE = 0,
		LOCK_SILENT = 1,
		LOCK_VISIBLE = 2
	}

	public enum QuestActionEvent
	{
		OnRun = 0,
		OnComplete = 1,
		OnCreateRosterQuest = 2
	}

	public enum QuestActionType
	{
		QUEST_ACTION_NONE = 0,
		QUEST_ACTION_DIALOG = 1,
		QUEST_ACTION_DIALOG_CHECK_TICKETS = 2,
		QUEST_ACTION_DIALOG_LOTTERY = 3,
		QUEST_ACTION_FIGHT = 4,
		QUEST_ACTION_ACT = 5,
		QUEST_ACTION_UNLOCK_BATTLE = 6,
		QUEST_ACTION_CHECKPOINT = 7,
		QUEST_ACTION_VARIABLE = 8,
		QUEST_ACTION_GOTO_ZONE = 9,
		QUEST_ACTION_WAIT = 10,
		QUEST_ACTION_DOWNLOAD = 11,
		QUEST_ACTION_UPGRADES = 12,
		QUEST_ACTION_FORGE = 13,
		QUEST_ACTION_SHOP = 14,
		QUEST_ACTION_NEWS = 15,
		QUEST_ACTION_TOGGLE_ITEMS = 16,
		QUEST_ACTION_ACTIVATE = 17,
		QUEST_ACTION_DISCOUNT = 18,
		QUEST_ACTION_CHANGE_SCENE = 19,
		QUEST_ACTION_CHANGE_TAB = 20,
		QUEST_ACTION_GIVE_ITEM = 21,
		QUEST_ACTION_FORCE_EXECUTION = 22,
		QUEST_ACTION_GIVE_CURRENCY = 23,
		QUEST_ACTION_TAKE_CURRENCY = 24,
		QUEST_ACTION_MAP_FOCUS = 25,
		QUEST_ACTION_VERSION = 26,
		QUEST_ACTION_CLEAR_STACK = 27,
		QUEST_ACTION_FB = 28,
		QUEST_ACTION_FB_INDICATOR = 29,
		QUEST_ACTION_DELIVER = 30,
		QUEST_ACTION_ATTACH_FILE = 31,
		QUEST_ACTION_SET_PARAMETER = 32,
		QUEST_ACTION_FOREACH = 33,
		QUEST_ACTION_RECOUNT = 34,
		QUEST_ACTION_SHOW_AD = 35,
		QUEST_ACTION_SET_ENERGY = 36,
		QUEST_ACTION_OPEN_URL = 37,
		QUEST_ACTION_RESET_PERKS = 38,
		QUEST_ACTION_UPGRADES_CLEANUP = 39,
		QUEST_ACTION_SESSION_SETTINGS = 40,
		QUEST_ACTION_RESET_DUEL_TIMER = 41,
		QUEST_ACTION_SEND_STRANGER_STATS = 42,
		QUEST_ACTION_SEND_DISCOUNT_STATS = 43,
		QUEST_ACTION_SET_CURRENT_ZONE = 44,
		QUEST_ACTION_SET_LANGUAGE = 45,
		QUEST_ACTION_BUY_ITEM = 46,
		QUEST_ACTION_SHOW_STARTER_PACK_TIMER = 47,
		QUEST_ACTION_HIDE_STARTER_PACK_TIMER = 48,
		QUEST_ACTION_SHOW_VIDEO = 49,
		QUEST_ACTION_UPDATE_SCREEN = 50,
		QUEST_ACTION_DENOMINATION = 51,
		QUEST_ACTION_TAPJOY_CALL = 52,
		QUEST_ACTION_ACTIVATE_TIMER = 53,
		QUEST_ACTION_END_TIMER = 54,
		QUEST_ACTION_SHOW_MAP_BUTTON = 55,
		QUEST_ACTION_HIDE_MAP_BUTTON = 56,
		QUEST_ACTION_ECLIPSE_MODE = 57,
		QUEST_ACTION_ECLIPSE_MODE_TUTORIAL = 58,
		QUEST_ACTION_ECLIPSE_MODE_SWITCH_BACK_TUTORIAL = 59,
		QUEST_ACTION_ECLIPSE_MODE_REPLAY_TUTORIAL = 60,
		QUEST_ACTION_TOGGLE_GROUP = 61,
		QUEST_ACTION_REMOVE_PACK = 62,
		QUEST_ACTION_RESTART_APPLICATION = 63,
		QUEST_ACTION_SET_MAP_MASK = 64,
		QUEST_ACTION_RESUME_QUEST = 65,
		QUEST_ACTION_TOGGLE_BATTLE = 66,
		QUEST_ACTION_SHOW_FORGE_TUTORIAL = 67,
		QUEST_ACTION_SET_LOW_GRAPHICS = 68,
		QUEST_ACTION_RESET_CRASH_FLAG = 69,
		QUEST_ACTION_RESET_ENCHANTMENTS = 70,
		QUEST_ACTION_GIVE_PERK = 71,
		QUEST_ACTION_GIVE_FREE_RECIPE = 72,
		QUEST_ACTION_OPEN_FORGE = 73,
		QUEST_ACTION_CHANGE_PLAYER_AVATAR = 74,
		QUEST_ACTION_SHOW_SET_TUTORIAL = 75,
		QUEST_ACTION_SHOW_CREDITS = 76,
		QUEST_ACTION_GIVE_ACHIEVEMENT = 77,
		QUEST_ACTION_SHOW_RAID_TOGGLE_BTN = 78,
		QUEST_ACTION_SHOW_RAID_TUTORIAL = 79,
		QUEST_ACTION_OPEN_LEAGUE_DIALOG = 80,
		QUEST_ACTION_SHOW_RAID_FIGHT_TUTORIAL = 81,
		QUEST_ACTION_SHOW_RAID_LEAGUES_TUTORIAL = 82,
		QUEST_ACTION_SET_STORY_TUTORIAL_STEP = 83,
		QUEST_ACTION_SET_RAID_INFO_TUTORIAL_STEP = 84,
		QUEST_ACTION_SWITCH_TO_RAIDS = 85,
		QUEST_ACTION_UNZIP = 86,
		QUEST_ACTION_BUY_PACK = 87,
		QUEST_ACTION_MENU_BTN_FLASHING = 88,
		QUEST_ACTION_STORY_TUTORIAL_MOVE = 89,
		QUEST_ACTION_STORY_TUTORIAL_PUNCHBAG = 90,
		QUEST_ACTION_STORY_TUTORIAL_BUY_ITEM = 91,
		QUEST_ACTION_STORY_TUTORIAL_CLICK_FIGHT = 92,
		QUEST_ACTION_STORY_TUTORIAL_LEARN_PERK = 93,
		QUEST_ACTION_STORY_TUTORIAL_DOUBLE_SWEEP = 94,
		QUEST_ACTION_STORY_TUTORIAL_SHOW_BLOCK = 95,
		QUEST_ACTION_IF = 96,
		QUEST_ACTION_RUN = 97,
		QUEST_ACTION_CHANGE_DOJO_LOCATION = 98,
		QUEST_ACTION_UPDATE_ECLIPSE_BATTLES = 99
	}

	public int Index;

	public bool CheckPoint;

	public string ActionName;

	public string QuestName;

	public string QuestFileName;

	public int StageIndex;

	public QuestParameters Parameters;

	private QuestStage stage;

	private string soundName;

	private InputLockMode lockMode;

	public static QuestActionType GetActionTypeByName(string actionName)
	{
		switch (actionName)
		{
		case "Dialog":
			return QuestActionType.QUEST_ACTION_DIALOG;
		case "DialogCheckTickets":
			return QuestActionType.QUEST_ACTION_DIALOG_CHECK_TICKETS;
		case "DialogLottery":
			return QuestActionType.QUEST_ACTION_DIALOG_LOTTERY;
		case "Fight":
			return QuestActionType.QUEST_ACTION_FIGHT;
		case "ActScreen":
			return QuestActionType.QUEST_ACTION_ACT;
		case "ShowBattle":
		case "HideBattle":
			return QuestActionType.QUEST_ACTION_UNLOCK_BATTLE;
		case "Checkpoint":
			return QuestActionType.QUEST_ACTION_CHECKPOINT;
		case "SetVariable":
			return QuestActionType.QUEST_ACTION_VARIABLE;
		case "OpenZone":
			return QuestActionType.QUEST_ACTION_GOTO_ZONE;
		case "Wait":
			return QuestActionType.QUEST_ACTION_WAIT;
		case "Download":
			return QuestActionType.QUEST_ACTION_DOWNLOAD;
		case "ShowUpgrades":
			return QuestActionType.QUEST_ACTION_UPGRADES;
		case "ShowForge":
			return QuestActionType.QUEST_ACTION_FORGE;
		case "OpenShop":
			return QuestActionType.QUEST_ACTION_SHOP;
		case "ShowNews":
			return QuestActionType.QUEST_ACTION_NEWS;
		case "ToggleItems":
			return QuestActionType.QUEST_ACTION_TOGGLE_ITEMS;
		case "Activate":
			return QuestActionType.QUEST_ACTION_ACTIVATE;
		case "Discount":
			return QuestActionType.QUEST_ACTION_DISCOUNT;
		case "ChangeScene":
			return QuestActionType.QUEST_ACTION_CHANGE_SCENE;
		case "GiveItem":
			return QuestActionType.QUEST_ACTION_GIVE_ITEM;
		case "ForceExecution":
			return QuestActionType.QUEST_ACTION_FORCE_EXECUTION;
		case "GiveCurrency":
			return QuestActionType.QUEST_ACTION_GIVE_CURRENCY;
		case "TakeCurrency":
			return QuestActionType.QUEST_ACTION_TAKE_CURRENCY;
		case "SetMapFocus":
			return QuestActionType.QUEST_ACTION_MAP_FOCUS;
		case "SetDataVersion":
			return QuestActionType.QUEST_ACTION_VERSION;
		case "ClearQuestQueue":
			return QuestActionType.QUEST_ACTION_CLEAR_STACK;
		case "FacebookAPICall":
			return QuestActionType.QUEST_ACTION_FB;
		case "SetFBIndicator":
			return QuestActionType.QUEST_ACTION_FB_INDICATOR;
		case "Deliver":
			return QuestActionType.QUEST_ACTION_DELIVER;
		case "AttachQuestFile":
			return QuestActionType.QUEST_ACTION_ATTACH_FILE;
		case "SetParameter":
			return QuestActionType.QUEST_ACTION_SET_PARAMETER;
		case "Foreach":
			return QuestActionType.QUEST_ACTION_FOREACH;
		case "Recount":
			return QuestActionType.QUEST_ACTION_RECOUNT;
		case "ShowAd":
			return QuestActionType.QUEST_ACTION_SHOW_AD;
		case "SetEnergy":
			return QuestActionType.QUEST_ACTION_SET_ENERGY;
		case "OpenUrl":
			return QuestActionType.QUEST_ACTION_OPEN_URL;
		case "ResetPerks":
			return QuestActionType.QUEST_ACTION_RESET_PERKS;
		case "UpgradesCleanup":
			return QuestActionType.QUEST_ACTION_UPGRADES_CLEANUP;
		case "SetSessionSettings":
			return QuestActionType.QUEST_ACTION_SESSION_SETTINGS;
		case "ResetDuelTimer":
			return QuestActionType.QUEST_ACTION_RESET_DUEL_TIMER;
		case "SendStrangerStats":
			return QuestActionType.QUEST_ACTION_SEND_STRANGER_STATS;
		case "SendDiscountStats":
			return QuestActionType.QUEST_ACTION_SEND_DISCOUNT_STATS;
		case "SetCurrentZone":
			return QuestActionType.QUEST_ACTION_SET_CURRENT_ZONE;
		case "SetLanguage":
			return QuestActionType.QUEST_ACTION_SET_LANGUAGE;
		case "BuyItem":
			return QuestActionType.QUEST_ACTION_BUY_ITEM;
		case "ShowStarterPackTimer":
			return QuestActionType.QUEST_ACTION_SHOW_STARTER_PACK_TIMER;
		case "HideStarterPackTimer":
			return QuestActionType.QUEST_ACTION_HIDE_STARTER_PACK_TIMER;
		case "ChangeTab":
			return QuestActionType.QUEST_ACTION_CHANGE_TAB;
		case "ShowVideo":
			return QuestActionType.QUEST_ACTION_SHOW_VIDEO;
		case "UpdateScene":
			return QuestActionType.QUEST_ACTION_UPDATE_SCREEN;
		case "Denomination":
			return QuestActionType.QUEST_ACTION_DENOMINATION;
		case "TapjoyActionCall":
			return QuestActionType.QUEST_ACTION_TAPJOY_CALL;
		case "ActivateTimer":
			return QuestActionType.QUEST_ACTION_ACTIVATE_TIMER;
		case "EndTimer":
			return QuestActionType.QUEST_ACTION_END_TIMER;
		case "ShowMapButton":
			return QuestActionType.QUEST_ACTION_SHOW_MAP_BUTTON;
		case "HideMapButton":
			return QuestActionType.QUEST_ACTION_HIDE_MAP_BUTTON;
		case "ToggleEclipseMode":
			return QuestActionType.QUEST_ACTION_ECLIPSE_MODE;
		case "ShowEclipseModeTutorial":
			return QuestActionType.QUEST_ACTION_ECLIPSE_MODE_TUTORIAL;
		case "ShowEclipseModeSwitchBackTutorial":
			return QuestActionType.QUEST_ACTION_ECLIPSE_MODE_SWITCH_BACK_TUTORIAL;
		case "ShowEclipseModeReplayTutorial":
			return QuestActionType.QUEST_ACTION_ECLIPSE_MODE_REPLAY_TUTORIAL;
		case "ToggleGroup":
			return QuestActionType.QUEST_ACTION_TOGGLE_GROUP;
		case "RemovePack":
			return QuestActionType.QUEST_ACTION_REMOVE_PACK;
		case "ApplicationRestart":
			return QuestActionType.QUEST_ACTION_RESTART_APPLICATION;
		case "SetMapMask":
			return QuestActionType.QUEST_ACTION_SET_MAP_MASK;
		case "ResumeQuests":
			return QuestActionType.QUEST_ACTION_RESUME_QUEST;
		case "ToggleBattle":
			return QuestActionType.QUEST_ACTION_TOGGLE_BATTLE;
		case "ShowForgeTutorial":
			return QuestActionType.QUEST_ACTION_SHOW_FORGE_TUTORIAL;
		case "ResetCrashFlag":
			return QuestActionType.QUEST_ACTION_RESET_CRASH_FLAG;
		case "SetLowGraphics":
			return QuestActionType.QUEST_ACTION_SET_LOW_GRAPHICS;
		case "ResetEnchantments":
			return QuestActionType.QUEST_ACTION_RESET_ENCHANTMENTS;
		case "GivePerk":
			return QuestActionType.QUEST_ACTION_GIVE_PERK;
		case "GiveFreeRecipe":
			return QuestActionType.QUEST_ACTION_GIVE_FREE_RECIPE;
		case "OpenForge":
			return QuestActionType.QUEST_ACTION_OPEN_FORGE;
		case "ChangePlayerAvatar":
			return QuestActionType.QUEST_ACTION_CHANGE_PLAYER_AVATAR;
		case "ShowSetTutorial":
			return QuestActionType.QUEST_ACTION_SHOW_SET_TUTORIAL;
		case "ShowCredits":
			return QuestActionType.QUEST_ACTION_SHOW_CREDITS;
		case "GiveAchievement":
			return QuestActionType.QUEST_ACTION_GIVE_ACHIEVEMENT;
		case "ShowRaidsGag":
			return QuestActionType.QUEST_ACTION_SHOW_RAID_TOGGLE_BTN;
		case "RaidsButtonTutorial":
			return QuestActionType.QUEST_ACTION_SHOW_RAID_TUTORIAL;
		case "OpenLeagueDialog":
			return QuestActionType.QUEST_ACTION_OPEN_LEAGUE_DIALOG;
		case "ShowRaidFightTutorial":
			return QuestActionType.QUEST_ACTION_SHOW_RAID_FIGHT_TUTORIAL;
		case "ShowLeagueWindow":
			return QuestActionType.QUEST_ACTION_SHOW_RAID_LEAGUES_TUTORIAL;
		case "SetStoryTutorialStep":
			return QuestActionType.QUEST_ACTION_SET_STORY_TUTORIAL_STEP;
		case "SetRaidInfoTutorialStep":
			return QuestActionType.QUEST_ACTION_SET_RAID_INFO_TUTORIAL_STEP;
		case "SwitchToRaids":
			return QuestActionType.QUEST_ACTION_SWITCH_TO_RAIDS;
		case "Unzip":
			return QuestActionType.QUEST_ACTION_UNZIP;
		case "BuyPack":
			return QuestActionType.QUEST_ACTION_BUY_PACK;
		case "Timer":
			return QuestActionType.QUEST_ACTION_ACTIVATE_TIMER;
		case "MenuBtnFlashing":
			return QuestActionType.QUEST_ACTION_MENU_BTN_FLASHING;
		case "StoryTutorialMove":
			return QuestActionType.QUEST_ACTION_STORY_TUTORIAL_MOVE;
		case "StoryTutorialPunchbag":
			return QuestActionType.QUEST_ACTION_STORY_TUTORIAL_PUNCHBAG;
		case "StoryTutorialBuyItem":
			return QuestActionType.QUEST_ACTION_STORY_TUTORIAL_BUY_ITEM;
		case "StoryTutorialClickFight":
			return QuestActionType.QUEST_ACTION_STORY_TUTORIAL_CLICK_FIGHT;
		case "StoryTutorialLearnPerk":
			return QuestActionType.QUEST_ACTION_STORY_TUTORIAL_LEARN_PERK;
		case "StoryTutorialDoubleSweep":
			return QuestActionType.QUEST_ACTION_STORY_TUTORIAL_DOUBLE_SWEEP;
		case "StoryTutorialShowBlock":
			return QuestActionType.QUEST_ACTION_STORY_TUTORIAL_SHOW_BLOCK;
		case "If":
			return QuestActionType.QUEST_ACTION_IF;
		case "Run":
			return QuestActionType.QUEST_ACTION_RUN;
		case "ChangeDojoLocation":
			return QuestActionType.QUEST_ACTION_CHANGE_DOJO_LOCATION;
		case "UpdateEclipseBattles":
			return QuestActionType.QUEST_ACTION_UPDATE_ECLIPSE_BATTLES;
		default:
			GameLog.Error(string.Format("{0} {1}", "Unknown quest type: ", actionName));
			return QuestActionType.QUEST_ACTION_NONE;
		}
	}

	public static QuestAction GetClassActionByName(string actionName)
	{
		QuestAction compatibilityAction = Eclipse.Content.QuestCompatibility.CreateRuntimeAction(actionName);
		if (compatibilityAction != null)
		{
			return compatibilityAction;
		}
		QuestActionType actionType = GetActionTypeByName(actionName);
		return CreateActionByType(actionType);
	}

	public static QuestAction CreateActionByType(QuestActionType actionType)
	{
		switch (actionType)
		{
		case QuestActionType.QUEST_ACTION_DIALOG:
			return new QuestActionDialog();
		case QuestActionType.QUEST_ACTION_DIALOG_CHECK_TICKETS:
			return new QuestActionDialogCheckTickets();
		case QuestActionType.QUEST_ACTION_DIALOG_LOTTERY:
			return new QuestActionDialogLottery();
		case QuestActionType.QUEST_ACTION_FIGHT:
			return new QuestActionFight();
		case QuestActionType.QUEST_ACTION_ACT:
			return new QuestActionAct();
		case QuestActionType.QUEST_ACTION_UNLOCK_BATTLE:
			return new QuestActionUnlockBattle();
		case QuestActionType.QUEST_ACTION_CHECKPOINT:
			return new QuestActionCheckPoint();
		case QuestActionType.QUEST_ACTION_VARIABLE:
			return new QuestActionVariable();
		case QuestActionType.QUEST_ACTION_GOTO_ZONE:
			return new QuestActionGotoZone();
		case QuestActionType.QUEST_ACTION_WAIT:
			return new QuestActionWait();
		case QuestActionType.QUEST_ACTION_DOWNLOAD:
			return new QuestActionDownload();
		case QuestActionType.QUEST_ACTION_UPGRADES:
			return new QuestActionUpgrades();
		case QuestActionType.QUEST_ACTION_FORGE:
			return new QuestActionForge();
		case QuestActionType.QUEST_ACTION_SHOP:
			return new QuestActionShop();
		case QuestActionType.QUEST_ACTION_NEWS:
			return new QuestActionNews();
		case QuestActionType.QUEST_ACTION_TOGGLE_ITEMS:
			return new QuestActionToggleItems();
		case QuestActionType.QUEST_ACTION_ACTIVATE:
			return new QuestActionActivate();
		case QuestActionType.QUEST_ACTION_DISCOUNT:
			return new QuestActionDiscount();
		case QuestActionType.QUEST_ACTION_CHANGE_SCENE:
			return new QuestActionChangeScene();
		case QuestActionType.QUEST_ACTION_CHANGE_TAB:
			return new QuestActionChangeTab();
		case QuestActionType.QUEST_ACTION_GIVE_ITEM:
			return new QuestActionGiveItem();
		case QuestActionType.QUEST_ACTION_FORCE_EXECUTION:
			return new QuestActionForceExecution();
		case QuestActionType.QUEST_ACTION_GIVE_CURRENCY:
			return new QuestActionGiveCurrency();
		case QuestActionType.QUEST_ACTION_TAKE_CURRENCY:
			return new QuestActionTakeCurrency();
		case QuestActionType.QUEST_ACTION_MAP_FOCUS:
			return new QuestActionMapFocus();
		case QuestActionType.QUEST_ACTION_VERSION:
			return new QuestActionCurrentVersion();
		case QuestActionType.QUEST_ACTION_CLEAR_STACK:
			return new QuestActionClearStack();
		case QuestActionType.QUEST_ACTION_FB:
			return new QuestActionFacebookAPICall();
		case QuestActionType.QUEST_ACTION_FB_INDICATOR:
			return new QuestActionSetFBIndicator();
		case QuestActionType.QUEST_ACTION_DELIVER:
			return new QuestActionDeliver();
		case QuestActionType.QUEST_ACTION_ATTACH_FILE:
			return new QuestActionAttachFile();
		case QuestActionType.QUEST_ACTION_SET_PARAMETER:
			return new QuestActionSetParameter();
		case QuestActionType.QUEST_ACTION_FOREACH:
			return new QuestActionForeach();
		case QuestActionType.QUEST_ACTION_RECOUNT:
			return new QuestActionRecount();
		case QuestActionType.QUEST_ACTION_SHOW_AD:
			return new QuestActionShowAd();
		case QuestActionType.QUEST_ACTION_SET_ENERGY:
			return new QuestActionSetEnergy();
		case QuestActionType.QUEST_ACTION_OPEN_URL:
			return new QuestActionOpenUrl();
		case QuestActionType.QUEST_ACTION_RESET_PERKS:
			return new QuestActionResetPerks();
		case QuestActionType.QUEST_ACTION_UPGRADES_CLEANUP:
			return new QuestActionUpgradesCleanup();
		case QuestActionType.QUEST_ACTION_SESSION_SETTINGS:
			return new QuestActionSessionSettings();
		case QuestActionType.QUEST_ACTION_RESET_DUEL_TIMER:
			return new QuestActionResetDuelTimer();
		case QuestActionType.QUEST_ACTION_SEND_STRANGER_STATS:
			return new QuestActionSendStrangerStats();
		case QuestActionType.QUEST_ACTION_SEND_DISCOUNT_STATS:
			return new QuestActionSendDiscountStats();
		case QuestActionType.QUEST_ACTION_SET_CURRENT_ZONE:
			return new QuestActionSetCurrentZone();
		case QuestActionType.QUEST_ACTION_SET_LANGUAGE:
			return new QuestActionSetLanguage();
		case QuestActionType.QUEST_ACTION_BUY_ITEM:
			return new QuestActionBuyItem();
		case QuestActionType.QUEST_ACTION_SHOW_STARTER_PACK_TIMER:
			return new QuestActionShowStarterPackTimer();
		case QuestActionType.QUEST_ACTION_HIDE_STARTER_PACK_TIMER:
			return new QuestActionHideStarterPackTimer();
		case QuestActionType.QUEST_ACTION_SHOW_VIDEO:
			return new QuestActionShowVideo();
		case QuestActionType.QUEST_ACTION_UPDATE_SCREEN:
			return new QuestActionUpdateScreen();
		case QuestActionType.QUEST_ACTION_DENOMINATION:
			return new QuestActionDenomination();
		case QuestActionType.QUEST_ACTION_TAPJOY_CALL:
			return new QuestActionTapjoyActionCall();
		case QuestActionType.QUEST_ACTION_ACTIVATE_TIMER:
			return new QuestActionActivateTimer();
		case QuestActionType.QUEST_ACTION_END_TIMER:
			return new QuestActionEndTimer();
		case QuestActionType.QUEST_ACTION_SHOW_MAP_BUTTON:
			return new QuestActionShowMapButton();
		case QuestActionType.QUEST_ACTION_HIDE_MAP_BUTTON:
			return new QuestActionHideMapButton();
		case QuestActionType.QUEST_ACTION_ECLIPSE_MODE:
			return new QuestActionEclipseMode();
		case QuestActionType.QUEST_ACTION_ECLIPSE_MODE_TUTORIAL:
			return new QuestActionEclipseModeTutorial();
		case QuestActionType.QUEST_ACTION_ECLIPSE_MODE_SWITCH_BACK_TUTORIAL:
			return new QuestActionEclipseModeSwitchBackTutorial();
		case QuestActionType.QUEST_ACTION_ECLIPSE_MODE_REPLAY_TUTORIAL:
			return new QuestActionEclipseModeReplayTutorial();
		case QuestActionType.QUEST_ACTION_TOGGLE_GROUP:
			return new QuestActionToggleGroup();
		case QuestActionType.QUEST_ACTION_REMOVE_PACK:
			return new QuestActionRemovePack();
		case QuestActionType.QUEST_ACTION_RESTART_APPLICATION:
			return new QuestActionRestartApplication();
		case QuestActionType.QUEST_ACTION_SET_MAP_MASK:
			return new QuestActionMapMask();
		case QuestActionType.QUEST_ACTION_RESUME_QUEST:
			return new QuestActionResumeQuests();
		case QuestActionType.QUEST_ACTION_TOGGLE_BATTLE:
			return new QuestActionToggleBattle();
		case QuestActionType.QUEST_ACTION_SHOW_FORGE_TUTORIAL:
			return new QuestActionShowForgeTutorial();
		case QuestActionType.QUEST_ACTION_RESET_CRASH_FLAG:
			return new QuestActionResetCrashFlag();
		case QuestActionType.QUEST_ACTION_SET_LOW_GRAPHICS:
			return new QuestActionSetLowGraphics();
		case QuestActionType.QUEST_ACTION_RESET_ENCHANTMENTS:
			return new QuestActionResetEnchantments();
		case QuestActionType.QUEST_ACTION_GIVE_PERK:
			return new QuestActionGivePerk();
		case QuestActionType.QUEST_ACTION_GIVE_FREE_RECIPE:
			return new QuestActionGiveFreeRecipe();
		case QuestActionType.QUEST_ACTION_OPEN_FORGE:
			return new QuestActionOpenForge();
		case QuestActionType.QUEST_ACTION_CHANGE_PLAYER_AVATAR:
			return new QuestActionChangePlayerAvatar();
		case QuestActionType.QUEST_ACTION_SHOW_SET_TUTORIAL:
			return new QuestActionShowSetTutorial();
		case QuestActionType.QUEST_ACTION_SHOW_CREDITS:
			return new QuestActionShowCredits();
		case QuestActionType.QUEST_ACTION_GIVE_ACHIEVEMENT:
			return new QuestActionGiveAchievement();
		case QuestActionType.QUEST_ACTION_SHOW_RAID_TOGGLE_BTN:
			return new QuestActionShowRaidToggleBtn();
		case QuestActionType.QUEST_ACTION_SHOW_RAID_TUTORIAL:
			return new QuestActionShowRaidTutorial();
		case QuestActionType.QUEST_ACTION_OPEN_LEAGUE_DIALOG:
			return new QuestActionOpenLeagueDialog();
		case QuestActionType.QUEST_ACTION_SHOW_RAID_FIGHT_TUTORIAL:
			return new QuestActionShowRaidFightTutorial();
		case QuestActionType.QUEST_ACTION_SHOW_RAID_LEAGUES_TUTORIAL:
			return new QuestActionShowRaidLeaguesTutorial();
		case QuestActionType.QUEST_ACTION_SET_STORY_TUTORIAL_STEP:
			return new QuestActionSetStoryTutorialStep();
		case QuestActionType.QUEST_ACTION_SET_RAID_INFO_TUTORIAL_STEP:
			return new QuestActionSetRaidInfoTutorialStep();
		case QuestActionType.QUEST_ACTION_SWITCH_TO_RAIDS:
			return new QuestActionSwitchToRaidsMap();
		case QuestActionType.QUEST_ACTION_UNZIP:
			return new QuestActionUnzip();
		case QuestActionType.QUEST_ACTION_BUY_PACK:
			return new QuestActionBuyPack();
		case QuestActionType.QUEST_ACTION_MENU_BTN_FLASHING:
			return new QuestActionMenuBtnFlashing();
		case QuestActionType.QUEST_ACTION_STORY_TUTORIAL_MOVE:
			return new QuestActionStoryTutorialMove();
		case QuestActionType.QUEST_ACTION_STORY_TUTORIAL_PUNCHBAG:
			return new QuestActionStoryTutorialPunchbag();
		case QuestActionType.QUEST_ACTION_STORY_TUTORIAL_BUY_ITEM:
			return new QuestActionStoryTutorialBuyItem();
		case QuestActionType.QUEST_ACTION_STORY_TUTORIAL_CLICK_FIGHT:
			return new QuestActionStoryTutorialClickFight();
		case QuestActionType.QUEST_ACTION_STORY_TUTORIAL_LEARN_PERK:
			return new QuestActionStoryTutorialLearnPerk();
		case QuestActionType.QUEST_ACTION_STORY_TUTORIAL_DOUBLE_SWEEP:
			return new QuestActionStoryTutorialDoubleSweep();
		case QuestActionType.QUEST_ACTION_STORY_TUTORIAL_SHOW_BLOCK:
			return new QuestActionStoryTutorialShowBlock();
		case QuestActionType.QUEST_ACTION_IF:
			return new QuestActionIf();
		case QuestActionType.QUEST_ACTION_RUN:
			return new QuestActionRun();
		case QuestActionType.QUEST_ACTION_CHANGE_DOJO_LOCATION:
			return new QuestActionChangeDojoLocation();
		case QuestActionType.QUEST_ACTION_UPDATE_ECLIPSE_BATTLES:
			return new QuestActionUpdateEclipseBattles();
		default:
			GameLog.Error(string.Format("{0} {1}", "QuestAction.getClassActionByType - type: ", actionType));
			return new QuestAction();
		}
	}

	public void FinishAction()
	{
		if (lockMode != InputLockMode.LOCK_NONE)
		{
			Module.GetInstance().SetQuestInputLock(false);
		}
		if (LogRules.GetInstance().GetLogQuestActions())
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("QuestAction ");
			stringBuilder.Append(ActionName);
			stringBuilder.Append(" completed");
			GameLog.Info(stringBuilder.ToString());
		}
		QuestsManager.get_Instance().CurrentActionName = string.Empty;
		CallEvent(1, Parameters);
	}

	public virtual void Parse(XmlNode node)
	{
		ActionName = node.Name;
		lockMode = ParseLockMode(XmlUtils.ParseString(node.Attributes["Lock"], string.Empty));
		soundName = XmlUtils.ParseString(node.Attributes["Sound"], string.Empty);
	}

	public virtual void Execute(QuestParameters parameters)
	{
		CallEvent(0, Parameters);
		if (LogRules.GetInstance().GetLogQuestActions())
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("QuestAction ");
			stringBuilder.Append(ActionName);
			stringBuilder.Append(" started");
			GameLog.Info(stringBuilder.ToString());
		}
		QuestsManager.get_Instance().CurrentActionName = ActionName;
		if (lockMode != InputLockMode.LOCK_NONE)
		{
			Module.GetInstance().SetQuestInputLock(true, lockMode == InputLockMode.LOCK_VISIBLE);
		}
		if (!soundName.Equals(string.Empty))
		{
			PlaySound();
		}
		Parameters = parameters;
	}

	public virtual void ResetSequences()
	{
	}

	public virtual void Render()
	{
	}

	public virtual void CompleteQuestStage()
	{
		QuestStage questStage = ListSF.GetInstance().GetQuestByName(QuestName);
		if (questStage != null)
		{
			questStage.FinishQuest();
		}
	}

	public virtual void ParseSequence(XmlNode node, QuestActionsSequence sequence, Action<object> onComplete)
	{
		if (node != null)
		{
			foreach (XmlNode childNode in node.ChildNodes)
			{
				string name = childNode.Name;
				QuestAction childAction = GetClassActionByName(name);
				childAction.QuestName = QuestName;
				childAction.Parse(childNode);
				sequence.AddAction(childAction);
			}
		}
		sequence.AddEventListener(1, onComplete);
	}

	public virtual void ParseSequenceWithUnlock(XmlNode node, QuestActionsSequence sequence, Action<object> onComplete)
	{
		ParseSequence(node, sequence, onComplete);
		sequence.AddEventListener(0, OnRunSuccessAndErrorAction);
	}

	public void OnRunSuccessAndErrorAction(object data)
	{
		if (lockMode != InputLockMode.LOCK_NONE)
		{
			Module.GetInstance().SetQuestInputLock(false);
		}
	}

	public virtual void SetStage(QuestStage questStage)
	{
		this.stage = questStage;
	}

	public QuestStage GetStage()
	{
		return stage;
	}

	public InputLockMode GetLockMode()
	{
		return lockMode;
	}

	private void PlaySound()
	{
		Sound.PlaySound(soundName);
	}

	private InputLockMode ParseLockMode(string value)
	{
		if (value.Equals("Silent"))
		{
			return InputLockMode.LOCK_SILENT;
		}
		if (value.Equals("Visible"))
		{
			return InputLockMode.LOCK_VISIBLE;
		}
		return InputLockMode.LOCK_NONE;
	}
}

// Compatibility actions ported from the newer quest vocabulary used by the
// plaintext 2.41.x gamedata.
public class QuestActionIf : QuestAction
{
	private readonly List<QuestCondition> _conditions = new List<QuestCondition>();
	private readonly QuestActionsSequence _then = new QuestActionsSequence();
	private readonly QuestActionsSequence _else = new QuestActionsSequence();
	private QuestActionsSequence _running;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		ParseConditions(node["Conditions"], _conditions);
		ParseSequence(node["Then"], _then, OnBranchComplete);
		ParseSequence(node["Else"], _else, OnBranchComplete);
	}

	private static void ParseConditions(XmlNode container, List<QuestCondition> output)
	{
		if (container == null)
		{
			return;
		}
		foreach (XmlNode node in container.ChildNodes)
		{
			if (node.NodeType != XmlNodeType.Element)
			{
				continue;
			}
			QuestCondition condition = new QuestCondition();
			condition.Parse(node);
			if (condition.comparison == QuestCondition.ComparisonType.QUEST_CONDITION_OPERATOR)
			{
				ParseConditions(node, condition.conditions);
			}
			output.Add(condition);
		}
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		bool matches = true;
		foreach (QuestCondition condition in _conditions)
		{
			if (!condition.Compare(parameters, null))
			{
				matches = false;
				break;
			}
		}
		_running = matches ? _then : _else;
		_running.currentIndex = 0;
		_running.Reset();
		if (_running.actions.Count == 0)
		{
			FinishAction();
			return;
		}
		_running.Run(parameters);
	}

	private void OnBranchComplete(object data)
	{
		FinishAction();
	}

	public override void ResetSequences()
	{
		_then.Reset();
		_else.Reset();
		_running = null;
	}
}

public class QuestActionRun : QuestAction
{
	private string _name = string.Empty;
	private QuestStage _runningQuest;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		if (ListSF.GetInstance().IsEclipseQuestSuppressed(_name))
		{
			FinishAction();
			return;
		}
		_runningQuest = ListSF.GetInstance().GetQuestByName(_name);
		if (_runningQuest == null)
		{
			Debug.LogWarning("[DevXml] Run action could not find quest: " + _name);
			FinishAction();
			return;
		}
		_runningQuest.AddEventListener(1, OnQuestComplete);
		_runningQuest.StartActions(parameters, false);
	}

	private void OnQuestComplete(object data)
	{
		if (_runningQuest != null)
		{
			_runningQuest.RemoveEventListener(1, OnQuestComplete);
			_runningQuest = null;
		}
		FinishAction();
	}
}

public class QuestActionChangeDojoLocation : QuestAction
{
	private string _name = "dojo";

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_name = node.Attributes["Name"].GetStringOrDefault("dojo");
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(_name, result);
		GameUtils.DefaultLocation = result.ToString();
		FinishAction();
	}
}

public class QuestActionUpdateEclipseBattles : QuestAction
{
	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		Roster roster = ListSF.GetRoster();
		ListSF listSF = ListSF.GetInstance();
		if (roster == null || listSF == null)
		{
			FinishAction();
			return;
		}
		bool eclipseMode = roster.IsEclipseMode();
		MapScene current = Scene<MapScene>.get_Current();
		Battle selectedBattle = GetSelectedBattle(current);
		Battle selectedReplacement = null;
		List<Battle> changedBattles = new List<Battle>();
		foreach (Battle normalBattle in listSF.GetBattles())
		{
			string eclipseBattleName = GetEclipseBattleName(normalBattle);
			if (string.IsNullOrEmpty(eclipseBattleName))
			{
				continue;
			}
			Zone zone = normalBattle.GetZone();
			Battle eclipseBattle = FindBattle(zone, eclipseBattleName);
			if (eclipseBattle == null)
			{
				continue;
			}
			// A mode switch may only exchange an already unlocked pair. If an old
			// save retained a hidden flag while the Eclipse counterpart was never
			// introduced, restore the normal entry instead of leaving no button.
			if (normalBattle.GetRosterBattle() == null)
			{
				continue;
			}
			// An active intermission entry owns the shared counterpart, including
			// its visibility. An obsolete base lock must not hide that counterpart.
			if (HasActiveIntermissionSource(normalBattle, zone, eclipseBattleName))
			{
				continue;
			}
			if (normalBattle.GetRosterBattle().IsLocked())
			{
				// Revealed future story entries are still gated. Do not introduce
				// an unlocked replay or expose one left behind by an older save.
				SetBattleHidden(normalBattle, false, changedBattles);
				SetBattleHidden(eclipseBattle, true, changedBattles);
				if (selectedBattle == eclipseBattle) selectedReplacement = normalBattle;
				continue;
			}
			if (normalBattle.GetStatus() != ConditionStatus.StatusComplete)
			{
				// An unfinished battle is fought as itself in Eclipse mode: its Eclipse="1"
				// rules make it the harder version. The replay counterpart only takes over
				// once every fight of the normal battle has been won (the archived Eclipse
				// tutorial deliberately sends the player to an unfinished tournament).
				SetBattleHidden(normalBattle, false, changedBattles);
				SetBattleHidden(eclipseBattle, true, changedBattles);
				if (selectedBattle == eclipseBattle) selectedReplacement = normalBattle;
				continue;
			}
			if (eclipseBattle.GetRosterBattle() == null)
			{
				// The newer UpdateEclipseBattles action also introduces the replay
				// counterpart for every battle that has already been unlocked.  The
				// recovered stub only toggled pre-existing roster entries, so a normal
				// playthrough never acquired any Eclipse tournament/challenge entries.
				roster.AddBattle(eclipseBattle, true, true, false, !eclipseMode, 0);
				eclipseBattle.IsMapVisible = true;
				if (eclipseBattle.GetRosterBattle() == null)
				{
					SetBattleHidden(normalBattle, false, changedBattles);
					continue;
				}
				changedBattles.Add(eclipseBattle);
			}
			// Run on fight return, mode switches and session initialization so
			// completed Eclipse segments (including old saves) remain replayable.
			BattleReplayable replayable = eclipseBattle as BattleReplayable;
			if (replayable != null && replayable.TryStartNextReplay())
			{
				changedBattles.Add(eclipseBattle);
				if (eclipseMode && eclipseBattle == selectedBattle)
				{
					selectedReplacement = eclipseBattle;
				}
			}
			SetBattleHidden(normalBattle, eclipseMode, changedBattles);
			SetBattleHidden(eclipseBattle, !eclipseMode, changedBattles);
			if (normalBattle == selectedBattle && eclipseMode)
			{
				selectedReplacement = eclipseBattle;
			}
			else if (eclipseBattle == selectedBattle && !eclipseMode)
			{
				selectedReplacement = normalBattle;
			}
		}
		if (changedBattles.Count != 0)
		{
			listSF.RequestSave();
			if (current != null)
			{
				foreach (Battle battle in changedBattles)
				{
					current.UpdateBattleButtonHidden(battle);
				}
				// The replacement button used to remain disabled until the player
				// visited another zone.  Reselecting the paired battle rebuilds the
				// preview and reapplies the current zone's input state immediately.
				if (selectedReplacement != null)
				{
					current.SelectBattle(selectedReplacement, 0f);
				}
				else
				{
					current.UpdateCurrentZone();
				}
			}
		}
		FinishAction();
	}

	private static string GetEclipseBattleName(Battle battle)
	{
		XmlNode node = battle.GetSourceDefinition().GetNode();
		if (node == null || node.Attributes == null)
		{
			return string.Empty;
		}
		XmlAttribute attribute = node.Attributes["EclipseToggleName"];
		return (attribute == null) ? string.Empty : attribute.Value;
	}

	private static Battle FindBattle(Zone zone, string name)
	{
		if (zone == null)
		{
			return null;
		}
		foreach (Battle battle in zone.Battles)
		{
			if (battle.get_Name() == name)
			{
				return battle;
			}
		}
		return null;
	}

	private static Battle GetSelectedBattle(MapScene mapScene)
	{
		if (mapScene == null)
		{
			return null;
		}
		ZoneScrollItem currentZone = mapScene.GetCurrentZone();
		return (currentZone == null) ? null : currentZone.get_LastBattle();
	}

	private static bool HasActiveIntermissionSource(Battle battle, Zone zone, string eclipseBattleName)
	{
		if (battle.get_Name().EndsWith("_INTERMISSION", StringComparison.Ordinal))
		{
			return false;
		}
		foreach (Battle candidate in zone.Battles)
		{
			if (candidate == battle || candidate.GetRosterBattle() == null || !candidate.get_Name().EndsWith("_INTERMISSION", StringComparison.Ordinal))
			{
				continue;
			}
			if (GetEclipseBattleName(candidate) == eclipseBattleName)
			{
				return true;
			}
		}
		return false;
	}

	private static void SetBattleHidden(Battle battle, bool hidden, List<Battle> changedBattles)
	{
		RosterBattle rosterBattle = battle.GetRosterBattle();
		if (rosterBattle == null || rosterBattle.IsHidden() == hidden)
		{
			return;
		}
		rosterBattle.SetHidden(hidden);
		changedBattles.Add(battle);
	}
}
