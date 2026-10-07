using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Map
{
	public class MapContainer : SFMonoBehaviour<object>
	{
		public enum MapContainerEvent
		{
			onBattleClick = 0,
			onZoneSelect = 1,
			onOpened = 2,
			onClosed = 3
		}

		public static float LAMP_FRAMES_PER_ZONE = 20f;

		public static float LAMP_MAX_FRAMES = 50f;

		[SerializeField]
		private MoveableMapPanel _moveableMapPanel;

		[SerializeField]
		private MapPanel _mapPanel;

		[SerializeField]
		private LampsPanel _lampsPanel;

		public void Init()
		{
			_mapPanel.Init();
			_mapPanel.AddEventListener(0, ForwardBattleClick);
			_mapPanel.AddEventListener(1, ForwardZoneSelect);
			_lampsPanel.Init();
			_lampsPanel.AddEventListener(0, OnLampClicked);
		}

		private void OnDestroy()
		{
			_mapPanel.RemoveEventListener(0, ForwardBattleClick);
			_mapPanel.RemoveEventListener(1, ForwardZoneSelect);
			_mapPanel = null;
			_lampsPanel.RemoveEventListener(0, OnLampClicked);
		}

		public void Open()
		{
			_moveableMapPanel.Open();
		}

		public void Close()
		{
			_moveableMapPanel.Close();
		}

		public void OpenRightNow()
		{
			_moveableMapPanel.OpenRightNow();
		}

		public void CloseRightNow()
		{
			_moveableMapPanel.CloseRightNow();
		}

		public int GetZoneIndex(ZoneScrollItem zoneItem)
		{
			return _mapPanel.GetZoneIndex(zoneItem);
		}

		public int GetZoneIndexByName(string name)
		{
			return _mapPanel.GetZoneIndexByName(name);
		}

		public void SelectBattle(Battle battle, float _Duration)
		{
			_mapPanel.SelectBattle(battle, _Duration);
		}

		private Color _backgroundMask = Color.white;

		public void AddStoryZones()
		{
			_mapPanel.AddStoryZones();
			SetZonesBackgroundMask(_backgroundMask);
			RefreshLamps();
		}

		public void AddRaidZones()
		{
			_mapPanel.AddRaidZones();
			RefreshLamps(false);
		}

		private void RefreshLamps(bool checkOpenZones = true)
		{
			List<ZoneScrollItem> zones = _mapPanel.GetZones();
			_lampsPanel.ClearLamps();
			_lampsPanel.AddLamps(zones.Count);
			if (checkOpenZones)
			{
				_lampsPanel.CheckOpenZones(zones);
			}
		}

		public void SetRaidPowerMode(bool enabled)
		{
			_mapPanel.SetRaidPowerMode(enabled);
		}

		public void Clear()
		{
			_mapPanel.Clear();
		}

		public List<Button> GetLamps()
		{
			return _lampsPanel.GetLamps();
		}

		public LampsPanel GetLampsPanel()
		{
			return _lampsPanel;
		}

		public ZoneScrollItem GetZone(int index)
		{
			return _mapPanel.GetZone(index);
		}

		public void SetCurrentZone(int index, string zoneAlias)
		{
			_lampsPanel.SetCurrentZone(index, zoneAlias);
		}

		public void ScrollToZone(int index, float _Duration)
		{
			_mapPanel.ScrollToZone(index, _Duration);
		}

		public void Flashing()
		{
			_lampsPanel.Flashing();
		}

		public int GetCurrentItemIndex()
		{
			return _mapPanel.GetCurrentItemIndex();
		}

		public ZoneScrollItem GetCurrentZone()
		{
			return _mapPanel.GetCurrentZone();
		}

		public int GetZonesCount()
		{
			return _mapPanel.GetZones().Count;
		}

		public void SetLampsEnable(bool isLampsEnabled)
		{
			_lampsPanel.gameObject.SetActive(isLampsEnabled);
		}

		public bool HasBattle(Battle battle)
		{
			return _mapPanel.HasBattle(battle);
		}

		public bool HasZone(ZoneScrollItem zoneItem)
		{
			return _mapPanel.HasZone(zoneItem);
		}

		public void FadeZonesBackgroundMask(Color color, float duration)
		{
			_backgroundMask = color;
			foreach (ZoneScrollItem item in _mapPanel.GetZones())
			{
				item.FadeBackgroundColor(color, duration);
			}
		}

		public void SetZonesBackgroundMask(Color color)
		{
			_backgroundMask = color;
			List<ZoneScrollItem> zones = _mapPanel.GetZones();
			foreach (ZoneScrollItem item in zones)
			{
				item.SetBackgroundColor(color);
			}
		}

		public virtual void SetTouchEnabled(bool isTouchEnabled)
		{
			_mapPanel.SetTouchEnabled(isTouchEnabled);
		}

		public bool IsMoveNow()
		{
			return false;
		}

		private void ForwardBattleClick(object data)
		{
			CallEvent(0, data);
		}

		private void ForwardZoneSelect(object data)
		{
			CallEvent(1, data);
		}

		private void ForwardOpened(object data)
		{
			CallEvent(2, data);
		}

		private void ForwardClosed(object data)
		{
			CallEvent(3, data);
		}

		private void OnLampClicked(object data)
		{
			int num = (int)data;
			int num2 = (int)Mathf.Min(LAMP_MAX_FRAMES, (float)Mathf.Abs(_lampsPanel.GetCurrentLamp() - num) * LAMP_FRAMES_PER_ZONE);
			float scrollDuration = (float)num2 / 60f;
			int count = _mapPanel.GetZones().Count;
			if (num >= 0 && num < count)
			{
				ZoneScrollItem zone = GetZone(num);
				ScrollToZone(num, scrollDuration);
				Zone zone2 = zone.get_Zone();
				if (zone2 != null && MapScene.IsZoneOpen(zone2) && MapScene.IsZoneHaveDontCompleteBattle(zone2))
				{
					SelectFirstOpenBattle(zone2, zone);
				}
			}
		}

		private void SelectFirstOpenBattle(Zone zone, ZoneScrollItem zoneItem)
		{
			List<string> battleNames = MapGUI.ZoneSwitchFade.BattleTypeNames;
			List<Battle> battles = zone.Battles;
			foreach (string item in battleNames)
			{
				foreach (Battle item2 in battles)
				{
					if (IsOpenUncompletedBattleNamed(item, item2))
					{
						zoneItem.SetLastBattle(item);
						return;
					}
				}
			}
		}

		private static bool IsOpenUncompletedBattleNamed(string name, Battle battle)
		{
			if (battle == null)
			{
				return false;
			}
			bool flag = name == battle.get_Name();
			bool flag2 = battle.GetStatus() == ConditionStatus.StatusOpen && !battle.IsLocked();
			return flag && flag2;
		}
	}
}
