using System;
using System.Collections.Generic;
using DG.Tweening;
using Eclipse.Underworld.UI;
using UnityEngine;

namespace Nekki.SF2.GUI.Map
{
	public class ZoneScrollItem : BaseScrollItem, global::IEventDispatcher<object>, IComparable<ZoneScrollItem>
	{
		public enum ZoneScrollItemEvent
		{
			OnClickBattle = 0
		}

		private List<BattleButton> _buttons = new List<BattleButton>();

		private Zone zone;

		private Battle lastSelectedBattle;

		private Color _maskColor = Color.white;

		private bool _raidPowerMode;

		public GameObject BattleBtnPrefab;

		private global::EventDispatcher<object> eventDispatcher = new global::EventDispatcher<object>();

		public Zone ItemZone
		{
			get
			{
				return get_Zone();
			}
		}

		public Battle FocusedBattle
		{
			get
			{
				return get_LastBattle();
			}
		}

		public Zone get_Zone()
		{
			return zone;
		}

		public Battle get_LastBattle()
		{
			return lastSelectedBattle;
		}

		public int AddEventListener(int name, Action<object> callback)
		{
			return eventDispatcher.AddEventListener(name, callback);
		}

		public int CallEvent(int name, object eventData)
		{
			return eventDispatcher.CallEvent(name, eventData);
		}

		public int RemoveAllEventListener()
		{
			return eventDispatcher.RemoveAllEventListener();
		}

		public int RemoveEvent(int name)
		{
			return eventDispatcher.RemoveEvent(name);
		}

		public int RemoveEventListener(int name, Action<object> callback)
		{
			return eventDispatcher.RemoveEventListener(name, callback);
		}

		public void Init(Zone newZone)
		{
			zone = newZone;
			ApplyZoneSprite();
			CreateBattleButtons();
		}

		public BattleButton GetBattleButtonByBattleName(string _BattleName)
		{
			Battle battle = get_Zone().FindBattle(_BattleName);
			if (battle != null)
			{
				return GetButtonByBattle(battle);
			}
			return null;
		}

		public BattleButton GetButtonByBattle(Battle battle)
		{
			foreach (BattleButton item in _buttons)
			{
				if (item.get_Battle() == battle)
				{
					return item;
				}
			}
			return null;
		}

		public void SelectFirstBattle()
		{
			List<Battle> battles = zone.Battles;
			if (battles.Count <= 0)
			{
				return;
			}
			bool flag = false;
			int num = 0;
			foreach (Battle item in battles)
			{
				if (item.IsMapVisible && UnderworldMapBattlePresentation.IsBattleVisible(item, zone, _raidPowerMode))
				{
					flag = true;
					break;
				}
				num++;
			}
			SetLastBattle(battles[flag ? num : 0]);
		}

		public void SelectBattle()
		{
			if (lastSelectedBattle == null || !lastSelectedBattle.IsMapVisible || !UnderworldMapBattlePresentation.IsBattleVisible(lastSelectedBattle, zone, _raidPowerMode))
			{
				SelectFirstBattle();
			}
			RefreshButtons();
			CallEvent(0, lastSelectedBattle);
		}

		public void UpdateBattleHidden(Battle battle)
		{
			BattleButton buttonByBattle = GetButtonByBattle(battle);
			if (null != buttonByBattle)
			{
				buttonByBattle.set_Hidden(battle.IsHidden());
			}
			RefreshButtons();
		}

		public void SetLastBattle(string _BattleName)
		{
			Battle lastBattle = zone.FindBattle(_BattleName);
			SetLastBattle(lastBattle);
		}

		public void SetLastBattle(Battle battle)
		{
			lastSelectedBattle = battle;
		}

		public void UpdateBattleFocus()
		{
			if (lastSelectedBattle != null && lastSelectedBattle.IsHidden())
			{
				Battle lastBattle = FindNearestVisibleBattle(lastSelectedBattle);
				SetLastBattle(lastBattle);
				SelectBattle();
			}
		}

		private DG.Tweening.Tween _maskTween;

		// Eclipse: eased mask change for a mode toggled while the map is on screen.
		public void FadeBackgroundColor(Color color, float duration)
		{
			ResolutionImage component = GetComponent<ResolutionImage>();
			if (duration <= 0f || component == null || !isActiveAndEnabled)
			{
				SetBackgroundColor(color);
				return;
			}
			if (_maskTween != null)
			{
				_maskTween.Kill();
			}
			_maskColor = color;
			Color initialColor = component.color;
			float progress = 0f;
			DG.Tweening.Tween tween = DG.Tweening.DOTween.To(() => progress, value =>
			{
				progress = value;
				if (component != null)
				{
					component.color = Color.Lerp(initialColor, color, value);
				}
			}, 1f, duration);
			DG.Tweening.TweenSettingsExtensions.SetEase(tween, DG.Tweening.Ease.InOutSine);
			DG.Tweening.TweenSettingsExtensions.SetUpdate(tween, true);
			_maskTween = tween;
			tween.OnKill(() => _maskTween = null);
		}

		public void SetBackgroundColor(Color color)
		{
			if (_maskTween != null)
			{
				_maskTween.Kill();
				_maskTween = null;
			}
			_maskColor = color;
			ResolutionImage component = GetComponent<ResolutionImage>();
			if (component != null) component.color = _maskColor;
		}

		public void Enabled(bool value)
		{
			int i = 0;
			for (int count = _buttons.Count; i < count; i++)
			{
				Battle battle = _buttons[i].get_Battle();
				bool flag = value && !battle.IsHidden() && UnderworldMapBattlePresentation.IsBattleVisible(battle, zone, _raidPowerMode);
				_buttons[i].enabled = flag;
			}
		}

		public void SetRaidPowerMode(bool enabled)
		{
			_raidPowerMode = enabled;
			foreach (BattleButton button in _buttons)
			{
				Battle battle = button.get_Battle();
				button.gameObject.SetActive(battle.IsMapVisible && !battle.IsHidden() &&
					UnderworldMapBattlePresentation.IsBattleVisible(battle, zone, _raidPowerMode));
			}
			if (lastSelectedBattle == null || !UnderworldMapBattlePresentation.IsBattleVisible(lastSelectedBattle, zone, _raidPowerMode))
			{
				SelectFirstBattle();
			}
		}

		public void ActiveBattle(string battleName, bool isVisible, bool shouldSelect = true, bool skipFade = false)
		{
			int num = 0;
			Battle battle = null;
			List<Battle> battles = zone.Battles;
			foreach (Battle item in battles)
			{
				string text = item.get_Name();
				if (text == battleName)
				{
					battle = item;
					break;
				}
				if (item.get_Type() != BattleType.FightUnregister)
				{
					num++;
				}
			}
			if (battle != null && num < battles.Count)
			{
				ActivateBattleAtIndex(battle, num, isVisible, shouldSelect, skipFade);
			}
			else
			{
				GameLog.Error("DisplayZone::activeBattle - cant find name " + battleName);
			}
		}

		private void ActivateBattleByType(BattleType battleType, bool isVisible, bool shouldSelect = true, bool skipFade = false)
		{
			int num = 0;
			Battle battle = null;
			List<Battle> battles = zone.Battles;
			foreach (Battle item in battles)
			{
				if (item.get_Type() == battleType)
				{
					battle = item;
					break;
				}
				if (item.get_Type() != BattleType.FightUnregister)
				{
					num++;
				}
			}
			if (num < battles.Count)
			{
				ActivateBattleAtIndex(battle, num, isVisible, shouldSelect, skipFade);
			}
			else
			{
				GameLog.Error("DisplayZone::activeBattle - cant find type " + battleType);
			}
		}

		private void ActivateBattleAtIndex(Battle battle, int buttonIndex, bool isVisible, bool shouldSelect = true, bool skipFade = false)
		{
			if (battle == null)
			{
				return;
			}
			if (buttonIndex < _buttons.Count)
			{
				BattleButton battleButton = _buttons[buttonIndex];
				if (battleButton.Locked != battle.IsLocked())
				{
					battleButton = RecreateButtonForBattle(battle);
				}
				battle.IsMapVisible = isVisible;
				if (isVisible)
				{
					if (battleButton != null)
					{
						if (skipFade)
						{
							battleButton.gameObject.SetActive(!battle.IsHidden());
						}
						else
						{
							battleButton.SetAlpha(0f);
							battleButton.gameObject.SetActive(true);
							battleButton.SetAlpha(1f, 0.5f);
						}
					}
					SetLastBattle(battle);
				}
				else
				{
					if ((bool)battleButton)
					{
						battleButton.gameObject.SetActive(false);
					}
					if (battle == lastSelectedBattle)
					{
						SelectFirstBattle();
					}
				}
				if (shouldSelect)
				{
					SelectBattle();
				}
			}
			else
			{
				GameLog.Error("DisplayZone::activeBattle - no button " + buttonIndex);
			}
		}

		private BattleButton RecreateButtonForBattle(Battle battle)
		{
			BattleButton buttonByBattle = GetButtonByBattle(battle);
			if (buttonByBattle == null)
			{
				GameLog.Error("DisplayZone::recreateButtonByBattle ERROR - btn is NULL");
				return null;
			}
			int num = 0;
			foreach (BattleButton item in _buttons)
			{
				if (item == buttonByBattle)
				{
					break;
				}
				num++;
			}
			UnityEngine.Object.Destroy(buttonByBattle.gameObject);
			buttonByBattle = CreateBattleButton(battle);
			buttonByBattle.transform.localPosition = new Vector3(battle.GetPosition().x * 2f, battle.GetPosition().y * 2f, buttonByBattle.transform.position.z);
			buttonByBattle.set_Hidden(battle.IsHidden());
			_buttons[num] = buttonByBattle;
			return buttonByBattle;
		}

		private void RefreshButtons()
		{
			foreach (BattleButton item in _buttons)
			{
				bool flag = false;
				RosterBattle rosterBattle = item.get_Battle().GetRosterBattle();
				if (rosterBattle != null)
				{
					flag = rosterBattle.IsHidden();
				}
				if (item.get_Hidden() != flag)
				{
					item.set_Hidden(flag);
				}
				item.gameObject.SetActive(item.get_Battle().IsMapVisible && !flag &&
					UnderworldMapBattlePresentation.IsBattleVisible(item.get_Battle(), zone, _raidPowerMode));
				bool activeBattle = item.get_Battle() == lastSelectedBattle;
				item.SetActiveBattle(activeBattle);
			}
		}

		private void ApplyZoneSprite()
		{
			ResolutionImage component = GetComponent<ResolutionImage>();
			component.set_TexturePath("UI/zones/");
			string text = zone.GetFileName();
			if (text == "Map0.1")
			{
				text = "Map1.1";
			}
			else if (text == "Map3.7")
			{
				text = "7";
			}
			text = UnderworldMapBattlePresentation.ResolveZoneSpriteName(text);
			component.set_SpriteName(text);
			component.color = _maskColor;
		}

		private void CreateBattleButtons()
		{
			List<Battle> battles = zone.Battles;
			foreach (Battle item in battles)
			{
				if (item.get_Type() != BattleType.FightUnregister)
				{
					BattleButton battleButton = CreateBattleButton(item);
					battleButton.transform.localPosition = new Vector3(item.GetPosition().x * 2f, item.GetPosition().y * 2f, battleButton.transform.localPosition.z);
					battleButton.CorrectLabel();
					_buttons.Add(battleButton);
				}
			}
		}

		private BattleButton CreateBattleButton(Battle battle)
		{
			RosterBattle rosterBattle = battle.GetRosterBattle();
			bool flag = rosterBattle != null && rosterBattle.IsLocked();
			bool hidden = rosterBattle != null && rosterBattle.IsHidden();
			string iconAtlas = UnderworldMapBattlePresentation.ResolveBattleIconAtlas(
				zone, battle.GetIconAtlas());
			BattleButton battleButton = InstantiateBattleButton(battle.GetAlias(), battle.GetBaseIconName(), battle.GetActiveIconName(), battle.GetLockedIconName(), battle.GetLockedActiveIconName(), flag, iconAtlas);
			if (Eclipse.Modding.ModPolicies.TryBattleIcons(battle.get_Name(), out var modIcons))
			{
				battleButton.ApplyModIcons(modIcons.Base, modIcons.Active, modIcons.Locked, modIcons.LockedActive, flag);
			}
			battleButton.onClick.AddListener(() =>
			{
				OnBattleButtonClicked(battle);
			});
			Eclipse.UI.PressBounce.Attach(battleButton.gameObject);
			battleButton.Locked = flag;
			battleButton.set_Battle(battle);
			battleButton.set_Hidden(hidden);
			return battleButton;
		}

		private BattleButton InstantiateBattleButton(string alias, string baseIconName, string activeIconName, string lockedIconName, string lockedActiveIconName, bool isLocked, string iconAtlas)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(BattleBtnPrefab);
			BattleButton component = gameObject.GetComponent<BattleButton>();
			component.gameObject.transform.SetParent(base.gameObject.transform, false);
			if (component != null)
			{
				component.Init(baseIconName, activeIconName, lockedIconName, lockedActiveIconName, isLocked, iconAtlas);
				component.SetAlias(alias);
			}
			return component;
		}

		private void OnBattleButtonClicked(object data)
		{
			Battle clickedBattle = (Battle)data;
			BattleButton buttonByBattle = GetButtonByBattle(clickedBattle);
			SetLastBattle(clickedBattle);
			SelectBattle();
		}

		private Battle FindNearestVisibleBattle(Battle battle)
		{
			BattleButton buttonByBattle = GetButtonByBattle(battle);
			float x = buttonByBattle.transform.position.x;
			float y = buttonByBattle.transform.position.y;
			double num = 2147483647.0;
			BattleButton battleButton = null;
			foreach (BattleButton item in _buttons)
			{
				if (item.get_Battle().IsMapVisible && !item.get_Battle().IsHidden())
				{
					float x2 = item.transform.position.x;
					float y2 = item.transform.position.y;
					double num2 = Math.Pow(x2 - x, 2.0) + Math.Pow(y2 - y, 2.0);
					if (num2 < num)
					{
						num = num2;
						battleButton = item;
					}
				}
			}
			if (null != battleButton)
			{
				return battleButton.get_Battle();
			}
			return null;
		}

		public int CompareTo(ZoneScrollItem otherItem)
		{
			string text = ((zone == null) ? string.Empty : zone.get_Name());
			string strB = ((otherItem.zone == null) ? string.Empty : otherItem.zone.get_Name());
			return text.CompareTo(strB);
		}

		private new void OnDestroy()
		{
			if (_maskTween != null) _maskTween.Kill();
			for (int i = 0; i < _buttons.Count; i++)
			{
				_buttons[i].onClick = null;
			}
			eventDispatcher.RemoveAllEventListener();
		}
	}
}
