using System.Collections.Generic;
using Nekki.SF2.GUI.Shop;
using Eclipse.Underworld;
using UnityEngine;

namespace Nekki.SF2.GUI.Map
{
	public class MapPanel : SFMonoBehaviour<object>
	{
		public enum MapPanelLayer
		{
			ZShadow = 0,
			ZMap = 1
		}

		public enum MapPanelEvent
		{
			onClickBattle = 0,
			onSelectZone = 1
		}

		private Scroll zoneScroll;

		[SerializeField]
		private GameObject _baseScrollContentPrefab;

		private BaseScrollContent _baseScrollContent;

		[SerializeField]
		private GameObject _scrollItemPrefab;

		public void Init()
		{
			zoneScroll = GetComponent<Scroll>();
			if (_baseScrollContentPrefab != null)
			{
				GameObject gameObject = Object.Instantiate(_baseScrollContentPrefab);
				_baseScrollContent = gameObject.GetComponent<BaseScrollContent>();
			}
			InitScroll();
		}

		private void OnDestroy()
		{
			zoneScroll.ClearItems();
			_baseScrollContent.onClickItem = null;
			_baseScrollContent.onSelectItem = null;
		}

		~MapPanel()
		{
		}

		public int GetZoneIndex(ZoneScrollItem JEOIJBLAMIO)
		{
			List<ZoneScrollItem> zones = GetZones();
			return zones.IndexOf(JEOIJBLAMIO);
		}

		public int GetZoneIndexByName(string name)
		{
			int num = 0;
			List<ZoneScrollItem> zones = GetZones();
			foreach (ZoneScrollItem item in zones)
			{
				Zone zone = item.get_Zone();
				if (name == zone.get_Name())
				{
					break;
				}
				num++;
			}
			if (num >= zones.Count)
			{
				num = -1;
			}
			return num;
		}

		public void AddStoryZones()
		{
			List<Zone> hFPCBJLOJEM = ListSF.GetZones().FindAll(
				zone => !UnderworldZonePolicy.IsRaidZone(zone));
			AddZones(hFPCBJLOJEM);
		}

		public void AddRaidZones()
		{
            // Only registered offline raid zones have a supported entry path.
            List<Zone> raidZones = ListSF.GetZones().FindAll(
                zone => zone != null && Eclipse.Modding.ModPolicies.IsRaidZone(zone.get_Name()));
            UnderworldZonePolicy.MarkLocallyPlayable(raidZones);
			AddZones(raidZones);
		}

		public void SetRaidPowerMode(bool enabled)
		{
			foreach (ZoneScrollItem zone in GetZones())
			{
				zone.SetRaidPowerMode(enabled);
			}
		}

		public bool HasBattle(Battle DPOOIONCEOA)
		{
			List<ZoneScrollItem> zones = GetZones();
			foreach (ZoneScrollItem item in zones)
			{
				if ((bool)item.GetButtonByBattle(DPOOIONCEOA))
				{
					return true;
				}
			}
			return false;
		}

		public bool HasZone(ZoneScrollItem HLJKOKMKMLM)
		{
			List<ZoneScrollItem> zones = GetZones();
			foreach (ZoneScrollItem item in zones)
			{
				if (item == HLJKOKMKMLM)
				{
					return true;
				}
			}
			return false;
		}

		public void SelectBattle(Battle DPOOIONCEOA, float _Duration)
		{
			if (DPOOIONCEOA == null)
			{
				return;
			}
			List<ZoneScrollItem> zones = GetZones();
			foreach (ZoneScrollItem item in zones)
			{
				if ((bool)item.GetButtonByBattle(DPOOIONCEOA))
				{
					item.SetLastBattle(DPOOIONCEOA);
					if (zoneScroll.GetCurrentItem() != item)
					{
						zoneScroll.ScrollToItem(item, _Duration);
					}
					// Instant focus must publish selection before MapScene reads it.
					// The regular Update will handle animated scrolling as before.
					if (_Duration == 0f) _baseScrollContent.Update();
					break;
				}
			}
		}

		public List<ZoneScrollItem> GetZones()
		{
			List<ZoneScrollItem> list = new List<ZoneScrollItem>();
			foreach (BaseScrollItem item2 in zoneScroll.GetItems())
			{
				ZoneScrollItem item = item2 as ZoneScrollItem;
				list.Add(item);
			}
			return list;
		}

		public ZoneScrollItem GetZone(int index)
		{
			List<ZoneScrollItem> zones = GetZones();
			if (zones != null && zones.Count > index)
			{
				return zones[index];
			}
			return null;
		}

		public int GetCurrentItemIndex()
		{
			return zoneScroll.GetCurrentItemIndex();
		}

		public ZoneScrollItem GetCurrentZone()
		{
			return zoneScroll.GetCurrentItem() as ZoneScrollItem;
		}

		public void ScrollToZone(int index, float _Duration)
		{
			zoneScroll.ScrollToItem(index, _Duration);
		}

		public virtual void SetTouchEnabled(bool value)
		{
			zoneScroll.enabled = value;
			List<ZoneScrollItem> zones = GetZones();
			foreach (ZoneScrollItem item in zones)
			{
				item.Enabled(value);
			}
		}

		public void Clear()
		{
			// A zone reload replaces its objects, not just the scroll registry.
			// Otherwise old battle buttons remain visible and keep receiving clicks.
			_baseScrollContent.Clear();
		}

		private void InitScroll()
		{
			if (!(zoneScroll == null) && !(_baseScrollContent == null))
			{
				zoneScroll.Init(_baseScrollContent);
				_baseScrollContent.onSelectItem.AddListener(ForwardZoneSelect);
				zoneScroll.get_ItemsScroll().set_MinScrollVelocity(500f);
				zoneScroll.get_ItemsScroll().set_AutoscrollDuration(0.25f);
				zoneScroll.get_BaseScrollContent().Spacing = 0f;
				zoneScroll.ScrollToBegin();
			}
		}

		private void ForwardBattleClick(object data)
		{
			CallEvent(0, data);
		}

		private void ForwardZoneSelect(object data)
		{
			CallEvent(1, data);
		}

		private void AddZones(List<Zone> HFPCBJLOJEM)
		{
			foreach (Zone item in HFPCBJLOJEM)
			{
				AddZone(item);
			}
		}

		private void AddZone(Zone HLJKOKMKMLM)
		{
			if (zoneScroll == null || _scrollItemPrefab == null)
			{
				GameLog.Error("MapPanel.AddZone some field is null");
			}
			else
			{
				if (HLJKOKMKMLM.GetIsStart())
				{
					return;
				}
				bool flag = true;
				List<Battle> lGIIBNJFADA = HLJKOKMKMLM.Battles;
				for (int i = 0; i < lGIIBNJFADA.Count; i++)
				{
					Battle cGJCGEBPCAF = lGIIBNJFADA[i];
					bool dCHJDPCEODD = cGJCGEBPCAF.IsMapVisible;
					flag &= !dCHJDPCEODD;
				}
				if (!flag)
				{
					GameObject gameObject = Object.Instantiate(_scrollItemPrefab);
					ZoneScrollItem component = gameObject.GetComponent<ZoneScrollItem>();
					if (component != null)
					{
						component.Init(HLJKOKMKMLM);
						component.AddEventListener(0, ForwardBattleClick);
						component.SelectFirstBattle();
						zoneScroll.AddItem(component);
					}
					else
					{
						GameLog.Error("ShopScrollContent.SetItems scrollItem is null");
					}
					zoneScroll.ScrollToBegin();
				}
			}
		}
	}
}
