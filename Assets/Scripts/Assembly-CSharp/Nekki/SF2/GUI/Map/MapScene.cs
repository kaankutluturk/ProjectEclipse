using System.Collections.Generic;
using Nekki.SF2.GUI.Menu;
using Eclipse.Underworld;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Map
{
	public class MapScene : Scene<MapScene>
	{
		public enum MapMode
		{
			StoryMode = 0,
			RaidMode = 1
		}

		public enum MapSceneEvent
		{
			ON_STORY_MAP_OPENED = 0,
			ON_STORY_MAP_CLOSED = 1,
			ON_RAID_MAP_RELOADED = 2
		}

		public class LastFight
		{
			private FightIDS fightIds;

			public LastFight(FightIDS JFIIJBAOOIK)
			{
				fightIds = JFIIJBAOOIK;
			}

			public FightIDS GetFightIds()
			{
				return fightIds;
			}

			public FightList GetFight()
			{
				return (GetBattle() == null) ? null : GetBattle().FindLoadedFightByName(fightIds.GetFight());
			}

			public Battle GetBattle()
			{
				return (GetZone() == null) ? null : GetZone().FindBattle(fightIds.GetBattle());
			}

			public Zone GetZone()
			{
				return ListSF.GetZoneByName(fightIds.GetZone());
			}
		}

		private enum MapSceneLayer
		{
			ZUnderRaidMap = 0,
			ZRaidMap = 1,
			ZOverRaidMap = 2,
			ZStoryMap = 3,
			ZLampsPanel = 4,
			ZRaidToggleBtn = 5,
			ZInfoBattle = 6,
			ZButtons = 7,
			ZKeys = 8
		}

		private enum TouchPriority
		{
			TOUCH_BATTLE_INFO = -128,
			TOUCH_MAP_PANEL = -124
		}

		private enum DebugButtonId
		{
			BUTTON_SKIP_ZONE = 100,
			BUTTON_RAID_CHEAT = 101
		}

		[SerializeField]
		private MainMenu _mainMenu;

		public const float STARTER_PACK_X = 70f;

		public const float STARTER_PACK_Y = 150f;

		public const float STARTER_PACK_TIMER_FONT_SIZE = 62f;

		public const float STARTER_PACK_TIMER_OFFSET_X = 21f;

		public const float STARTER_PACK_TIMER_OFFSET_Y = 8f;

		public const string STARTER_PACK_IMAGE = "textures/buttons/map/timer.png";

		[SerializeField]
		private MapContainer _storyContainer;

		[SerializeField]
		private InfoBattle _infoBattle;

		[SerializeField]
		private MapButtonsPanel _mapButtonsPanel;

		private ZoneScrollItem activeZone;

		private LastFight lastStoryFight;

		private LastFight lastRaidFight;

		private Sprite unusedSprite;

		private MapMode mapMode;

		private bool _raidPowerMode;
		private Color storyMask = Color.white;
		private Color raidNormalMask = Color.white;
		private Color raidPowerMask = Color.white;
		private float raidMaskDuration = .8f;

		private UnderworldMapControls _underworldControls;

		private UnderworldMapControls GetUnderworldControls()
		{
			if (_underworldControls == null)
			{
				_underworldControls = new UnderworldMapControls(_storyContainer.transform.parent, _infoBattle,
					ToggleRaidMap, ScrollRaidMapDown, ToggleRaidPowerMode);
			}
			return _underworldControls;
		}

		public override ScreenType SceneType
		{
			get
			{
				return get_SceneId();
			}
		}

		public override ScreenType get_SceneId()
		{
			return ScreenType.ModuleMap;
		}

		protected override void Init(object data)
		{
			base.Init(data);
			if (!SoundController.IsBackgroundMusicIntro)
			{
				SoundController.StartBackgroundMusic();
			}
			_mainMenu.Init();
			if (_mapButtonsPanel != null)
			{
				_mapButtonsPanel.Init();
			}
			InitInfoBattle();
			InitStoryContainer();
			SetStoryZonesBackgroundMask(ListSF.GetRoster().GetMapMaskColor());
			ListSF.GetRoster().AddEventListener(4, OnRosterEvent);
			RefreshLastFights();
			if (0 == 0)
			{
				mapMode = MapMode.StoryMode;
				_storyContainer.OpenRightNow();
				SelectLastFight(lastStoryFight);
			}
			UpdateCurrentZone();
			SyncCurrentZoneLamp();
			GetUnderworldControls().Initialize(mapMode == MapMode.RaidMode);
			UpdateRaidControls();
			PostInit();
			// Back from an Underworld fight: reopen the Underworld (and Power Mode) at its focus.
			bool returnPowerMode;
			if (Eclipse.Underworld.UnderworldZonePolicy.ConsumeMapReturn(out returnPowerMode))
			{
				SwitchToRaidMap();
				if (returnPowerMode)
				{
					ToggleRaidPowerMode();
					LastFight focus = lastRaidFight;
					Battle battle = focus == null ? null : focus.GetBattle();
					if (battle != null && _storyContainer.HasBattle(battle)) SelectBattle(battle, 0f);
				}
			}
		}

		protected override void OnDestroy()
		{
			if (ListSF.GetRoster() != null)
			{
				ListSF.GetRoster().RequestSave(true);
			}
			base.OnDestroy();
			ListSF.GetRoster().RemoveEventListener(4, OnRosterEvent);
			_storyContainer.RemoveEventListener(0, OnBattleClicked);
			_storyContainer.RemoveEventListener(1, OnZoneSelected);
		}

		public void UpdateInfoBattle()
		{
			_infoBattle.Refresh();
		}

		public void UpdateBattleButtonHidden(Battle DPOOIONCEOA)
		{
			RosterBattle dDNLCGOPAGC = DPOOIONCEOA.GetRosterBattle();
			if (dDNLCGOPAGC == null)
			{
				return;
			}
			Zone pKCPOJKLMOK = DPOOIONCEOA.GetZone();
			if (pKCPOJKLMOK != null)
			{
				ZoneScrollItem zoneScrollItem = GetZoneItem(pKCPOJKLMOK);
				if (null != zoneScrollItem)
				{
					zoneScrollItem.UpdateBattleHidden(DPOOIONCEOA);
				}
			}
		}

		private ZoneScrollItem _lastEclipseRaidZone;
        public void UpdateCurrentZone()
		{
			if ((bool)activeZone)
			{
				activeZone.Enabled(false);
				activeZone = null;
			}
			MapContainer mapContainer = _storyContainer;
			if (!(mapContainer == null))
			{
				ZoneScrollItem currentZone = mapContainer.GetCurrentZone();
				if (!(currentZone == null))
				{
					activeZone = currentZone;
					activeZone.Enabled(true);
                    if (mapMode == MapMode.RaidMode)
                    {
                        var previous = _lastEclipseRaidZone; _lastEclipseRaidZone = currentZone;
                        if (previous != null && previous != currentZone)
                            Eclipse.Modding.ModModeRuntime.Raise(QuestEvent.QuestEventType.QUEST_EVENT_RAID_FLOOR_CHANGED);
                    }
                    else _lastEclipseRaidZone = null;
				}
			}
		}

		public void ReloadZones()
		{
			_storyContainer.Clear();
			if (mapMode == MapMode.RaidMode)
			{
				_storyContainer.AddRaidZones();
				_storyContainer.SetRaidPowerMode(_raidPowerMode);
			}
			else
			{
				_storyContainer.AddStoryZones();
			}
			activeZone = null;
			ShowStoryMap();
			RefreshLastFights();
			SelectLastFight((mapMode == MapMode.RaidMode) ? lastRaidFight : lastStoryFight);
			UpdateCurrentZone();
			SyncCurrentZoneLamp();
			GetUnderworldControls().UpdateToggleSprite(mapMode == MapMode.RaidMode);
		}

		public InfoBattle GetInfoBattle()
		{
			return _infoBattle;
		}

		public LabelButton GetBtnFight()
		{
			return _infoBattle.GetBtnFight();
		}

		public Button GetMapButtonsByIndex(int index)
		{
			return null;
		}

		public ZoneScrollItem GetCurrentZone()
		{
			return activeZone;
		}

		public MapMode GetCurrentState()
		{
			return mapMode;
		}

		public void SetStoryZonesBackgroundMask(Color color)
		{
			storyMask = color;
			if (mapMode == MapMode.StoryMode) _storyContainer.SetZonesBackgroundMask(color);
		}

		public void FadeStoryZonesBackgroundMask(Color color, float duration)
		{
			storyMask = color;
			if (mapMode == MapMode.StoryMode) _storyContainer.FadeZonesBackgroundMask(color, duration);
		}

		public void SetRaidMapColors(Color normal, Color power, float duration)
		{
			raidNormalMask = normal;
			raidPowerMask = power;
			raidMaskDuration = duration;
			if (mapMode == MapMode.RaidMode)
				_storyContainer.FadeZonesBackgroundMask(_raidPowerMode ? power : normal, duration);
		}

		public void SelectFight(string IGGFGLLIGCG, int frames = 0)
		{
			FightIDS mOCEDDJOAEB = new FightIDS();
			mOCEDDJOAEB.SetFightIDSByString(IGGFGLLIGCG);
			FightList cPAOKGPGHEH = ListSF.GetFightById(mOCEDDJOAEB);
			SelectFight(cPAOKGPGHEH, frames);
		}

		public void SelectFight(FightList fight, float _Duration = 0f)
		{
			if (fight != null && fight.Battle != null)
			{
				SelectBattle(fight.Battle, _Duration);
			}
		}

		public void SelectBattle(Battle DPOOIONCEOA, float _Duration)
		{
			if (DPOOIONCEOA == null)
			{
				return;
			}
			MapContainer mapContainer = null;
			if (_storyContainer.HasBattle(DPOOIONCEOA))
			{
				mapContainer = _storyContainer;
			}
			if ((bool)mapContainer)
			{
				mapContainer.SelectBattle(DPOOIONCEOA, _Duration);
				UpdateCurrentZone();
				SyncCurrentZoneLamp();
			}
		}

		public void SelectZone(MapContainer FJANDPPJDIH, int IGNBCKPAENN, float _Duration)
		{
			if (!(FJANDPPJDIH == null))
			{
				ZoneScrollItem zone = FJANDPPJDIH.GetZone(IGNBCKPAENN);
				Battle lastBattle = zone.get_LastBattle();
				SelectBattle(lastBattle, _Duration);
			}
		}

		public void GotoZoneByName(string name, int frames = 0)
		{
			int zoneIndexByName = _storyContainer.GetZoneIndexByName(name);
			if (zoneIndexByName >= 0)
			{
				SelectZone(_storyContainer, zoneIndexByName, frames);
			}
		}

		public void ActiveBattleByFightIDS(FightIDS DIAIIPCBMFL, bool PEJELKNFEKJ, bool HCNBLJBAOHK = true, bool DPFMIACNGLL = false)
		{
			ZoneScrollItem zoneScrollItem = GetZoneItemByName(DIAIIPCBMFL.GetZone());
			if ((bool)zoneScrollItem)
			{
				zoneScrollItem.ActiveBattle(DIAIIPCBMFL.GetBattle(), PEJELKNFEKJ, HCNBLJBAOHK, DPFMIACNGLL);
			}
		}

		public static bool IsZoneHaveDontCompleteBattle(Zone HLJKOKMKMLM)
		{
			List<string> gBDHOPBMLHK = MapGUI.ZoneSwitchFade.BattleTypeNames;
			for (int i = 0; i < gBDHOPBMLHK.Count; i++)
			{
				Battle cGJCGEBPCAF = HLJKOKMKMLM.Battles.Find(battle => battle.get_Name() == gBDHOPBMLHK[i]);
				if (cGJCGEBPCAF != null)
				{
					bool flag = cGJCGEBPCAF.GetStatus() == ConditionStatus.StatusOpen;
					bool flag2 = !cGJCGEBPCAF.IsLocked();
					bool dCHJDPCEODD = cGJCGEBPCAF.IsMapVisible;
					if (flag && flag2 && dCHJDPCEODD)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsZoneOpen(Zone HLJKOKMKMLM)
		{
			if (HLJKOKMKMLM == null)
			{
				return false;
			}
			if (HLJKOKMKMLM.Battles.Exists(battle => battle.IsMapVisible && Eclipse.Modding.ModModeRuntime.OwnsBattle(battle))) return true;
			if (UnderworldZonePolicy.IsRaidZone(HLJKOKMKMLM))
				return HLJKOKMKMLM.Battles.Exists(battle => battle.IsMapVisible);
			List<Battle> list = HLJKOKMKMLM.Battles.FindAll(battle =>
				battle.get_Type() == BattleType.FightBosses || battle.get_Type() == BattleType.FightFinalTitan ||
				battle.get_Type() == BattleType.FightBossesIntermission);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsMapVisible)
				{
					return true;
				}
			}
			return false;
		}

		public void EnableMapButtons(bool IJHFJPBBNEJ)
		{
			if (!IJHFJPBBNEJ)
			{
				SetStoryButtonsEnabled(false);
				SetRaidButtonsEnabled(false);
			}
			else if (mapMode == MapMode.StoryMode)
			{
				SetStoryButtonsEnabled(true);
				SetRaidButtonsEnabled(false);
			}
			else
			{
				SetStoryButtonsEnabled(false);
				SetRaidButtonsEnabled(true);
			}
		}

		public void ScrollToItemByName(SliderType _sliderType)
		{
			if (_sliderType != SliderType.SliderRaidMap && _sliderType == SliderType.SliderStoryMap)
			{
				ShowStoryMap();
			}
		}

		private void InitStoryContainer()
		{
			_storyContainer.Init();
			_storyContainer.AddStoryZones();
			_storyContainer.AddEventListener(0, OnBattleClicked);
			_storyContainer.AddEventListener(1, OnZoneSelected);
		}

		private void InitInfoBattle()
		{
			_infoBattle.Init();
		}

		private void ShowBattleInfo(Battle DPOOIONCEOA)
		{
			if (!(_infoBattle != null))
			{
				return;
			}
			_infoBattle.UpdateBattleInfo(DPOOIONCEOA);
			FightList currentFight = _infoBattle.GetCurrentFight();
			if (currentFight != null)
			{
				string jFIIJBAOOIK = currentFight.FightId.ToString();
				if (mapMode != MapMode.RaidMode)
				{
					ListSF.GetRoster().SetMapFocus(jFIIJBAOOIK);
				}
				else
				{
					ListSF.GetRoster().SetRaidMapFocus(jFIIJBAOOIK);
				}
				return;
			}
			FightList jDIPBIHBGPF = GameUtils.GetLastFight(DPOOIONCEOA);
			if (jDIPBIHBGPF != null)
			{
				string jFIIJBAOOIK2 = jDIPBIHBGPF.FightId.ToString();
				if (mapMode != MapMode.RaidMode)
				{
					ListSF.GetRoster().SetMapFocus(jFIIJBAOOIK2);
				}
				else
				{
					ListSF.GetRoster().SetRaidMapFocus(jFIIJBAOOIK2);
				}
			}
		}

		private void SyncCurrentZoneLamp()
		{
			if (!(activeZone == null))
			{
				MapContainer mapContainer = _storyContainer;
				bool flag = mapMode == MapMode.StoryMode;
				if (!(mapContainer == null))
				{
					activeZone.SelectBattle();
					string aBJMDKJHJCP = activeZone.get_Zone().get_Name();
					int currentItemIndex = mapContainer.GetCurrentItemIndex();
					mapContainer.SetCurrentZone(currentItemIndex, aBJMDKJHJCP);
					mapContainer.GetLampsPanel().SetLampsVisible(true);
				}
			}
		}

		private void RefreshLastFights()
		{
			lastStoryFight = null;
			lastRaidFight = null;
			lastStoryFight = new LastFight(ListSF.GetRoster().GetMapFocus());
			lastRaidFight = new LastFight(ListSF.GetRoster().GetRaidMapFocus());
		}

		private void PostInit()
		{
		}

		private void ScrollRaidMapDown()
		{
			int count = _storyContainer.GetZonesCount();
			if (count <= 0)
			{
				return;
			}
			int next = (_storyContainer.GetCurrentItemIndex() + 1) % count;
			_storyContainer.ScrollToZone(next, 0.25f);
		}

		private void ToggleRaidPowerMode()
		{
			_raidPowerMode = !_raidPowerMode;
			_storyContainer.SetRaidPowerMode(_raidPowerMode);
			UpdateCurrentZone();
			_storyContainer.FadeZonesBackgroundMask(_raidPowerMode ? raidPowerMask : raidNormalMask, raidMaskDuration);
			SyncCurrentZoneLamp();
			UpdateRaidControls();
			Debug.Log("[Underworld] Power Mode " + (_raidPowerMode ? "enabled" : "disabled"));
		}

		private void UpdateRaidControls()
		{
			bool raid = mapMode == MapMode.RaidMode;
			GetUnderworldControls().UpdateState(raid, _raidPowerMode);
			if (_mapButtonsPanel != null)
			{
				_mapButtonsPanel.SetStoryButtonsVisible(!raid);
			}
		}

		public void SetRaidToggleVisible(bool visible)
		{
			GetUnderworldControls().SetToggleVisible(visible, mapMode == MapMode.RaidMode);
		}

		public void ToggleRaidMap()
		{
			if (mapMode == MapMode.RaidMode)
			{
				SwitchToStoryMap();
			}
			else
			{
				SwitchToRaidMap();
			}
		}

		public void SwitchToRaidMap()
		{
            bool changed = mapMode != MapMode.RaidMode;
			SwitchMapMode(MapMode.RaidMode);
            if (changed) Eclipse.Modding.ModModeRuntime.Raise(QuestEvent.QuestEventType.QUEST_EVENT_RAID_MAP_ENTER);
		}

		public void SwitchToStoryMap()
		{
			SwitchMapMode(MapMode.StoryMode);
		}

		private void SwitchMapMode(MapMode mode)
		{
			if (mapMode == mode)
			{
				return;
			}
			_storyContainer.Clear();
			activeZone = null;
			mapMode = mode;
			if (mode == MapMode.RaidMode)
			{
				_raidPowerMode = false;
				_storyContainer.AddRaidZones();
				_storyContainer.SetRaidPowerMode(false);
			}
			else
			{
				_storyContainer.AddStoryZones();
			}
			_storyContainer.SetZonesBackgroundMask(mode == MapMode.RaidMode ? raidNormalMask : storyMask);
			_storyContainer.OpenRightNow();
			RefreshLastFights();
			LastFight focus = (mode == MapMode.RaidMode) ? lastRaidFight : lastStoryFight;
			Battle battle = (focus == null) ? null : focus.GetBattle();
			if (battle != null && _storyContainer.HasBattle(battle))
			{
				SelectBattle(battle, 0f);
			}
			else if (_storyContainer.GetZonesCount() > 0)
			{
				_storyContainer.ScrollToZone(0, 0f);
				ZoneScrollItem firstZone = _storyContainer.GetZone(0);
				if (firstZone != null)
				{
					firstZone.SelectBattle();
				}
			}
			UpdateCurrentZone();
			SyncCurrentZoneLamp();
			UpdateRaidToggleSprite();
			UpdateRaidControls();
			Debug.Log("[Underworld] switched to " + mode + " with " +
				_storyContainer.GetZonesCount() + " map page(s)");
		}

		private void UpdateRaidToggleSprite()
		{
			GetUnderworldControls().UpdateToggleSprite(mapMode == MapMode.RaidMode);
		}

		private void OnBattleClicked(object data)
		{
			Battle cGJCGEBPCAF = (Battle)data;
			if (cGJCGEBPCAF != _infoBattle.GetCurrentBattle())
			{
				ShowBattleInfo(cGJCGEBPCAF);
				RefreshLastFights();
			}
		}

		private void OnZoneSelected(object data)
		{
			ZoneScrollItem jEOIJBLAMIO = (ZoneScrollItem)data;
			int zoneIndex = _storyContainer.GetZoneIndex(jEOIJBLAMIO);
			SelectZone(_storyContainer, zoneIndex, 0f);
		}

		private ZoneScrollItem GetZoneItemByName(string BCKMHHFHGNH)
		{
			int zoneIndexByName = _storyContainer.GetZoneIndexByName(BCKMHHFHGNH);
			if (zoneIndexByName >= 0)
			{
				return _storyContainer.GetZone(zoneIndexByName);
			}
			return null;
		}

		private void OnRosterEvent(object data)
		{
			if (activeZone != null)
			{
				activeZone.UpdateBattleFocus();
			}
		}

		private ZoneScrollItem GetZoneItem(Zone HLJKOKMKMLM)
		{
			return GetZoneItemByName(HLJKOKMKMLM.get_Name());
		}

		private void SetStoryButtonsEnabled(bool IJHFJPBBNEJ)
		{
		}

		private void SetRaidButtonsEnabled(bool IJHFJPBBNEJ)
		{
		}

		private void SelectLastFight(LastFight BEFANIBLCPI)
		{
			if (BEFANIBLCPI != null)
			{
				SelectBattle(BEFANIBLCPI.GetBattle(), 0f);
			}
		}

		private void ShowStoryMap()
		{
			SwitchToStoryMap();
		}

		private void EnsureStoryMap()
		{
			if (mapMode != MapMode.StoryMode)
			{
				ShowStoryMap();
			}
		}

		public override void UpdateScene(object data)
		{
		}
	}
}
