using System.Collections.Generic;
using Nekki.SF2.GUI;
using UnityEngine;

namespace Nekki.SF2.Core.Quests
{
	public class QuestsManager : SFMonoBehaviour<object>
	{
		public enum QuestsManagerEvent
		{
			onComplete = 0
		}

		private static GameObject managerObject;

		private static QuestsManager _instance;

		// Host-only configuration. Keep definitions and roster progress intact so
		// disabling a content policy restores the original quest identity.
		private HashSet<string> EclipseSuppressedQuests = new HashSet<string>(System.StringComparer.Ordinal);

		public void SetEclipseSuppressedQuests(IEnumerable<string> names)
		{
			if (_isRunActions || questQueue.Count != 0)
				throw new System.InvalidOperationException("Quest suppression must be configured before queue restoration or execution.");
			var next = new HashSet<string>(System.StringComparer.Ordinal);
			if (names != null)
				foreach (string name in names)
				{
					if (string.IsNullOrWhiteSpace(name) || name.IndexOf('#') <= 0 || name.EndsWith("#"))
						throw new System.ArgumentException("Suppressed quests require source-file#quest-name identities.", "names");
					next.Add(name);
				}
			EclipseSuppressedQuests = next;
		}

		public void ClearEclipseQuestSuppression()
		{
			// Teardown restores eligibility without touching an active native queue.
			EclipseSuppressedQuests.Clear();
		}

		private bool IsEclipseSuppressed(QuestStage quest)
		{
			return quest != null && EclipseSuppressedQuests.Contains(quest.EclipseSourceFile + "#" + quest.get_Name());
		}

		public bool IsEclipseQuestSuppressed(string name, string sourceFile = null)
		{
			if (name == null || EclipseSuppressedQuests.Count == 0) return false;
			foreach (QuestStage quest in allQuests)
			{
				if (quest.get_Name() == name && (sourceFile == null ||
					quest.GetFileName().Replace('\\', '/') == sourceFile.Replace('\\', '/')))
					return IsEclipseSuppressed(quest);
			}
			return false;
		}

		private readonly List<QuestStage> EclipseRaidMapEnter = new List<QuestStage>();
        private readonly List<QuestStage> EclipseRaidFloorChanged = new List<QuestStage>();
        private readonly List<QuestStage> EclipseShowRaidLoot = new List<QuestStage>();
        private List<QuestStage> questQueue = new List<QuestStage>();

		private List<QuestStage> fightEnterQuests = new List<QuestStage>();

		private List<QuestStage> fightEndQuests = new List<QuestStage>();

		private List<QuestStage> raidFightEnterQuests = new List<QuestStage>();

		private List<QuestStage> raidFightEndQuests = new List<QuestStage>();

		private List<QuestStage> levelUpQuests = new List<QuestStage>();

		private List<QuestStage> gotItemQuests = new List<QuestStage>();

		private List<QuestStage> dialogQuests = new List<QuestStage>();

		private List<QuestStage> sessionQuests = new List<QuestStage>();

		private List<QuestStage> activateQuests = new List<QuestStage>();

		private List<QuestStage> purchaseQuests = new List<QuestStage>();

		private List<QuestStage> prePurchaseQuests = new List<QuestStage>();

		private List<QuestStage> loginFbQuests = new List<QuestStage>();

		private List<QuestStage> deliveryQuests = new List<QuestStage>();

		private List<QuestStage> energyQuests = new List<QuestStage>();

		private List<QuestStage> serverCurrencyQuests = new List<QuestStage>();

		private List<QuestStage> languageSwitchQuests = new List<QuestStage>();

		private List<QuestStage> freeSectionButtonQuests = new List<QuestStage>();

		private List<QuestStage> startApplicationQuests = new List<QuestStage>();

		private List<QuestStage> changeTabQuests = new List<QuestStage>();

		private List<QuestStage> purchaseUnsuccessfulQuests = new List<QuestStage>();

		private List<QuestStage> starterPackPressQuests = new List<QuestStage>();

		private List<QuestStage> energyBarPressQuests = new List<QuestStage>();

		private List<QuestStage> videoButtonPressQuests = new List<QuestStage>();

		private List<QuestStage> timerEndQuests = new List<QuestStage>();

		private List<QuestStage> mapButtonPressQuests = new List<QuestStage>();

		private List<QuestStage> raidMapButtonPressQuests = new List<QuestStage>();

		private List<QuestStage> enchantmentQuests = new List<QuestStage>();

		private List<QuestStage> enchantmentUnsuccessfulQuests = new List<QuestStage>();

		private List<QuestStage> activatePerkQuests = new List<QuestStage>();

		private List<QuestStage> deactivatePerkQuests = new List<QuestStage>();

		private List<QuestStage> buySpinGemsQuests = new List<QuestStage>();

		private List<QuestStage> resetAscensionQuests = new List<QuestStage>();

		private List<QuestStage> setItemAcquiredQuests = new List<QuestStage>();

		private List<QuestStage> duelUnlockedQuests = new List<QuestStage>();

		private List<QuestStage> raidOpenQuests = new List<QuestStage>();

		private List<QuestStage> raidEnterQuests = new List<QuestStage>();

		private List<QuestStage> raidEndQuests = new List<QuestStage>();

		private List<QuestStage> bossShieldDestroyedQuests = new List<QuestStage>();

		private List<QuestStage> raidLoginQuests = new List<QuestStage>();

		private List<QuestStage> seasonStartWithRestQuests = new List<QuestStage>();

		private List<QuestStage> seasonStartWithoutRestQuests = new List<QuestStage>();

		private List<QuestStage> dailyWindowOpenQuests = new List<QuestStage>();

		private List<QuestStage> leaderboardTapQuests = new List<QuestStage>();

		private List<QuestStage> showRewardedVideoQuests = new List<QuestStage>();

		private List<QuestStage> loginEndQuests = new List<QuestStage>();

		private List<QuestStage> shopEnterQuests = new List<QuestStage>();

		private List<QuestStage> DevXmlSceneLoaded = new List<QuestStage>();

		private List<QuestStage> DevXmlShopButtonPress = new List<QuestStage>();

		private List<QuestStage> allQuests = new List<QuestStage>();

		public QuestParameters QuestParameters = new QuestParameters();

		private int currentIndex;

		[SerializeField]
		private bool _isRunActions;

		[SerializeField]
		public string CurrentQuestName = string.Empty;

		[SerializeField]
		public string CurrentActionName = string.Empty;

		[SerializeField]
		public int QuestsEndedMaxCount = 30;

		[SerializeField]
		public List<string> QuestsEnded = new List<string>();

		[SerializeField]
		public List<string> QuestsInQueue = new List<string>();

		private bool runPending;

		public static QuestsManager SharedInstance
		{
			get
			{
				return get_Instance();
			}
		}

		public int ExtraQuestCapacity
		{
			set
			{
				set_QuestsAllCapacity(value);
			}
		}

		public bool IsActionsRunning
		{
			get
			{
				return get_IsRunActions();
			}
		}

		public static QuestsManager get_Instance()
		{
			if (_instance == null)
			{
				managerObject = new GameObject("QuestsManager");
				_instance = managerObject.AddComponent<QuestsManager>();
				Object.DontDestroyOnLoad(managerObject);
			}
			return _instance;
		}

		public static void Reset()
		{
			_instance = null;
			Object.Destroy(managerObject);
			managerObject = null;
		}

		public void set_QuestsAllCapacity(int value)
		{
			allQuests.Capacity += value;
		}

		public bool get_IsRunActions()
		{
			return _isRunActions;
		}

		private bool IsQuestQueued(QuestStage quest)
		{
			if (quest.allowDoubles)
			{
				return false;
			}
			string text = quest.get_Name();
			foreach (QuestStage item in questQueue)
			{
				if (text.Equals(item.get_Name()))
				{
					return true;
				}
			}
			return false;
		}

		private void OnQuestComplete(object data)
		{
			QuestStage quest = data as QuestStage;
			if (quest != null)
			{
				quest.RemoveEventListener(0, OnQuestComplete);
				questQueue.RemoveAt(quest.index);
				UntrackQueuedQuest(quest.index);
				TrackEndedQuest(quest.get_Name());
				ReindexQueue();
				if (questQueue.Count > 0)
				{
					runPending = true;
					return;
				}
				ClearActions();
				CallEvent(0, 0);
			}
		}

		private void TrackQueuedQuest(string questName)
		{
			if (SystemProperties.IsDebug())
			{
				QuestsInQueue.Add(questName);
			}
		}

		private void UntrackQueuedQuest(int index)
		{
			if (SystemProperties.IsDebug() && index < QuestsInQueue.Count)
			{
				QuestsInQueue.RemoveAt(index);
			}
		}

		private void TrackEndedQuest(string questName)
		{
			if (SystemProperties.IsDebug())
			{
				QuestsEnded.Add(questName);
				if (QuestsEnded.Count > QuestsEndedMaxCount)
				{
					QuestsEnded.RemoveAt(0);
				}
			}
		}

		private void SortQueue()
		{
			questQueue.Sort();
			ReindexQueue();
		}

		private void ReindexQueue()
		{
			int num = 0;
			foreach (QuestStage item in questQueue)
			{
				item.index = num;
				num++;
			}
		}

		private void Add(QuestStage quest)
		{
			if (quest != null)
			{
				CrashBreadcrumbTracker.GetInstance().AddBreadcrumb(quest.get_Name());
				questQueue.Add(quest);
				TrackQueuedQuest(quest.get_Name());
				ScreenType screenType = Module.GetInstance().GetCurrentScreenType();
				if (!runPending && !_isRunActions && screenType != ScreenType.ModuleFight)
				{
					runPending = true;
				}
			}
		}

		public void AddQuest(QuestStage quest)
		{
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_FIGHT_ENTER))
			{
				fightEnterQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_FIGHT_END))
			{
				fightEndQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_FIGHT_ENTER))
			{
				raidFightEnterQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_FIGHT_END))
			{
				raidFightEndQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_LEVEL_UP))
			{
				levelUpQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_GOT_ITEM))
			{
				gotItemQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_DIALOG))
			{
				dialogQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_SESSION))
			{
				sessionQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_ACTIVATE))
			{
				activateQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE))
			{
				purchaseQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_PREPURCHASE))
			{
				prePurchaseQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_LOGIN_FB))
			{
				loginFbQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_DELIVERY))
			{
				deliveryQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_ENERGY))
			{
				energyQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_SERVER_CURRENCY))
			{
				serverCurrencyQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_LANGUAGE_SWITCH))
			{
				languageSwitchQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_FREE_SECTION_BUTTON))
			{
				freeSectionButtonQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_START_APPLICATION))
			{
				startApplicationQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_CHANGE_TAB))
			{
				changeTabQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE_UNSUCCESSFUL))
			{
				purchaseUnsuccessfulQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_STARTER_PACK_PRESS))
			{
				starterPackPressQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_ENERGY_BAR_PRESS))
			{
				energyBarPressQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_VIDEO_BUTTON_PRESS))
			{
				videoButtonPressQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_TIMER_END))
			{
				timerEndQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_MAP_BUTTON_PRESS))
			{
				mapButtonPressQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_MAP_BUTTON_PRESS))
			{
				raidMapButtonPressQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_ENCHANTMENT))
			{
				enchantmentQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_ENCHANTMENT_UNSUCCESSFUL))
			{
				enchantmentUnsuccessfulQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_ACTIVATE_PERK))
			{
				activatePerkQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_DIACTIVATE_PERK))
			{
				deactivatePerkQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_BUY_SPIN_GEMS))
			{
				buySpinGemsQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_RESET_ASCENSION))
			{
				resetAscensionQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_SET_ITEM_ACQUIRED))
			{
				setItemAcquiredQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_DUEL_UNLOCKED))
			{
				duelUnlockedQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_OPEN))
			{
				raidOpenQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_ENTER))
			{
				raidEnterQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_END))
			{
				raidEndQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_BOSS_SHIELD_DESTR))
			{
				bossShieldDestroyedQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_LOGIN))
			{
				raidLoginQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_SEAS_START_WITH_REST))
			{
				seasonStartWithRestQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_SEAS_START_WITHOUT_REST))
			{
				seasonStartWithoutRestQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_DAILY_WINDOW_OPEN))
			{
				dailyWindowOpenQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_LEADERBOARD_TAP))
			{
				leaderboardTapQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_SHOW_REWARDED_VIDEO))
			{
				showRewardedVideoQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_LOGIN_END))
			{
				loginEndQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_MAP_ENTER)) EclipseRaidMapEnter.Add(quest);
            if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_RAID_FLOOR_CHANGED)) EclipseRaidFloorChanged.Add(quest);
            if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_SHOW_RAID_LOOT)) EclipseShowRaidLoot.Add(quest);
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_SHOP_ENTER))
			{
				shopEnterQuests.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_SCENE_LOADED))
			{
				DevXmlSceneLoaded.Add(quest);
			}
			if (quest.IsEvent(QuestEvent.QuestEventType.QUEST_EVENT_SHOP_BUTTON_PRESS))
			{
				DevXmlShopButtonPress.Add(quest);
			}
				allQuests.Add(quest);
			}

			public void RemoveExternalQuest(QuestStage quest)
			{
				if (quest == null) return;
				List<QuestStage>[] lists = new List<QuestStage>[]
				{
					fightEnterQuests, fightEndQuests, raidFightEnterQuests, raidFightEndQuests, levelUpQuests, gotItemQuests,
					dialogQuests, sessionQuests, activateQuests, purchaseQuests, prePurchaseQuests, loginFbQuests,
					deliveryQuests, energyQuests, serverCurrencyQuests, languageSwitchQuests, freeSectionButtonQuests, startApplicationQuests,
					changeTabQuests, purchaseUnsuccessfulQuests, starterPackPressQuests, energyBarPressQuests, videoButtonPressQuests, timerEndQuests,
					mapButtonPressQuests, raidMapButtonPressQuests, enchantmentQuests, enchantmentUnsuccessfulQuests, activatePerkQuests, deactivatePerkQuests,
					buySpinGemsQuests, resetAscensionQuests, setItemAcquiredQuests, duelUnlockedQuests, raidOpenQuests, raidEnterQuests,
					raidEndQuests, bossShieldDestroyedQuests, raidLoginQuests, seasonStartWithRestQuests, seasonStartWithoutRestQuests, dailyWindowOpenQuests,
					leaderboardTapQuests, showRewardedVideoQuests, loginEndQuests, shopEnterQuests, DevXmlSceneLoaded,
					DevXmlShopButtonPress, allQuests
				};
				for (int i = 0; i < lists.Length; i++) lists[i].Remove(quest);
                EclipseRaidMapEnter.Remove(quest); EclipseRaidFloorChanged.Remove(quest); EclipseShowRaidLoot.Remove(quest);
				questQueue.Remove(quest);
				QuestsInQueue.Remove(quest.get_Name());
				ReindexQueue();
			}

			public bool ActionQuest(QuestEvent.QuestEventType eventType)
		{
			List<QuestStage> list = null;
			switch (eventType)
			{
            case QuestEvent.QuestEventType.QUEST_EVENT_RAID_MAP_ENTER: list = EclipseRaidMapEnter; break;
            case QuestEvent.QuestEventType.QUEST_EVENT_RAID_FLOOR_CHANGED: list = EclipseRaidFloorChanged; break;
            case QuestEvent.QuestEventType.QUEST_EVENT_SHOW_RAID_LOOT: list = EclipseShowRaidLoot; break;
			case QuestEvent.QuestEventType.QUEST_EVENT_FIGHT_ENTER:
				list = fightEnterQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_FIGHT_END:
				list = fightEndQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_RAID_FIGHT_ENTER:
				list = raidFightEnterQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_RAID_FIGHT_END:
				list = raidFightEndQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_LEVEL_UP:
				list = levelUpQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_GOT_ITEM:
				list = gotItemQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_DIALOG:
				list = dialogQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_SESSION:
				list = sessionQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_ACTIVATE:
				list = activateQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE:
				list = purchaseQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_PREPURCHASE:
				list = prePurchaseQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_LOGIN_FB:
				list = loginFbQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_DELIVERY:
				list = deliveryQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_ENERGY:
				list = energyQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_SERVER_CURRENCY:
				list = serverCurrencyQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_LANGUAGE_SWITCH:
				list = languageSwitchQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_FREE_SECTION_BUTTON:
				list = freeSectionButtonQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_START_APPLICATION:
				list = startApplicationQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_CHANGE_TAB:
				list = changeTabQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_PURCHASE_UNSUCCESSFUL:
				list = purchaseUnsuccessfulQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_STARTER_PACK_PRESS:
				list = starterPackPressQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_ENERGY_BAR_PRESS:
				list = energyBarPressQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_VIDEO_BUTTON_PRESS:
				list = videoButtonPressQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_TIMER_END:
				list = timerEndQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_MAP_BUTTON_PRESS:
				list = mapButtonPressQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_RAID_MAP_BUTTON_PRESS:
				list = raidMapButtonPressQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_ENCHANTMENT:
				list = enchantmentQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_ENCHANTMENT_UNSUCCESSFUL:
				list = enchantmentUnsuccessfulQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_ACTIVATE_PERK:
				list = activatePerkQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_DIACTIVATE_PERK:
				list = deactivatePerkQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_BUY_SPIN_GEMS:
				list = buySpinGemsQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_RESET_ASCENSION:
				list = resetAscensionQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_SET_ITEM_ACQUIRED:
				list = setItemAcquiredQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_DUEL_UNLOCKED:
				list = duelUnlockedQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_RAID_OPEN:
				list = raidOpenQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_RAID_ENTER:
				list = raidEnterQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_RAID_END:
				list = raidEndQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_BOSS_SHIELD_DESTR:
				list = bossShieldDestroyedQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_RAID_LOGIN:
				list = raidLoginQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_SEAS_START_WITH_REST:
				list = seasonStartWithRestQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_SEAS_START_WITHOUT_REST:
				list = seasonStartWithoutRestQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_DAILY_WINDOW_OPEN:
				list = dailyWindowOpenQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_LEADERBOARD_TAP:
				list = leaderboardTapQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_SHOW_REWARDED_VIDEO:
				list = showRewardedVideoQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_LOGIN_END:
				list = loginEndQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_SHOP_ENTER:
				list = shopEnterQuests;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_SCENE_LOADED:
				list = DevXmlSceneLoaded;
				break;
			case QuestEvent.QuestEventType.QUEST_EVENT_SHOP_BUTTON_PRESS:
				list = DevXmlShopButtonPress;
				break;
			default:
				GameLog.Error(string.Format("{0},{1}", "Quest::actionQuest - unknown type: ", eventType));
				break;
			}
			bool flag = false;
			if (list != null)
			{
				foreach (QuestStage item in list)
				{
					if (!IsEclipseSuppressed(item) && !IsQuestQueued(item))
					{
						if (item.Compare(QuestParameters))
						{
							AddQuestToStek(item);
							flag = true;
						}
						if (flag)
						{
							SortQueue();
						}
					}
				}
			}
			return flag;
		}

		public bool AddQuestToStek(string name, bool forceRun)
		{
			QuestStage questByName = GetQuestByName(name);
			if (questByName != null)
			{
				return AddQuestToStek(questByName, forceRun);
			}
			return false;
		}

		public bool AddQuestToStek(QuestStage quest, bool sortQueue = false)
		{
			if (quest == null || IsEclipseSuppressed(quest)) return false;
			quest.QueueForRun(QuestParameters);
			quest.index = questQueue.Count;
			Add(quest);
			if (sortQueue)
			{
				SortQueue();
			}
			return true;
		}

		public QuestStage GetQuestByName(string name)
		{
			return GetQuestByName(name, null);
		}

		public QuestStage GetQuestByName(string name, string sourceFile)
		{
			foreach (QuestStage item in allQuests)
			{
				if (item.get_Name().Equals(name) && (sourceFile == null ||
					item.GetFileName().Replace('\\', '/') == sourceFile.Replace('\\', '/')))
				{
					return item;
				}
			}
			return null;
		}

		public void Update()
		{
			if (runPending)
			{
				RunNextQuest();
			}
		}

		private void RunNextQuest()
		{
			int count = questQueue.Count;
			if (count <= 0)
			{
				return;
			}
			_isRunActions = true;
			runPending = false;
			bool flag = false;
			while (!flag && 0 < questQueue.Count)
			{
				QuestStage quest = questQueue[0];
				if (quest != null)
				{
					quest.AddEventListener(0, OnQuestComplete);
					quest.StartActions(quest.EclipseQueuedParameters ?? QuestParameters, quest.EclipseQueuedResume);
					flag = true;
					CurrentQuestName = quest.get_Name();
				}
				else
				{
					questQueue.RemoveAt(0);
					UntrackQueuedQuest(0);
				}
			}
			if (questQueue.Count == 0)
			{
				_isRunActions = false;
				CurrentQuestName = string.Empty;
				CallEvent(0, 0);
			}
		}

		public void RunActionsAll()
		{
			if (!_isRunActions)
			{
				RunNextQuest();
			}
		}

		public bool AddActionQuest(List<QuestStage> quests)
		{
			// Filter before restoring parameters or calling MHNEBBGMOLA: both touch
			// native quest state. Never remove suppressed entries from the roster.
			if (EclipseSuppressedQuests.Count != 0)
				quests = quests.FindAll(quest => !IsEclipseSuppressed(quest));
			if (quests.Count == 0) return false;
			foreach (QuestStage item in quests)
			{
				if (item.GetRosterQuest() != null && !IsQuestQueued(item))
				{
					var checkpoint = item.GetRosterQuest().get_Parameters();
					if (checkpoint == null) item.QueueForRun(QuestParameters);
					else
					{
						// Do not run the entry checkpoint again: that resets the saved action index.
						item.EclipseQueuedParameters = item.RestoreParameters(checkpoint);
						item.EclipseQueuedResume = true;
					}
					item.index = questQueue.Count;
					Add(item);
				}
			}
			SortQueue();
			return true;
		}

		public void ClearActions()
		{
			runPending = false;
			_isRunActions = false;
			CurrentQuestName = string.Empty;
			questQueue.Clear();
			QuestsInQueue.Clear();
		}

		public bool HaveCompareQuests()
		{
			return questQueue.Count > 0;
		}

		public void ClearStack(List<string> groupNames = null)
		{
			RemoveAllButThis(groupNames);
		}

		public void RemoveAllButThis(List<string> groupNames = null)
		{
			for (int i = 0; i < questQueue.Count; i++)
			{
				QuestStage quest = questQueue[i];
				if (quest.GetState() != QuestStage.QuestState.QUEST_ACTIONS && (groupNames == null || groupNames.Count == 0 || quest.IsGroup(groupNames)))
				{
					if (quest.GetRosterQuest() != null)
					{
						quest.GetRosterQuest().ClearParameters();
					}
					quest.RemoveEventListener(0, OnQuestComplete);
					questQueue.RemoveAt(i);
					UntrackQueuedQuest(i);
					i--;
				}
			}
			ReindexQueue();
			ListSF.GetInstance().RequestSave();
		}
	}
}
