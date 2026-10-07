using System.Collections.Generic;
using UnityEngine;

namespace Nekki.SF2.GUI.Fight
{
	public class ActivePerkModel : MonoBehaviour
	{
		public enum PerkAlignment
		{
			ActivePerkLeft = 0,
			ActivePerkRight = 1
		}

		[SerializeField]
		private PerkAlignment _align;

		[SerializeField]
		private GameObject _activePerkItemPrefab;

		[SerializeField]
		private GameObject _activePerkItemContainerPrefab;

		private Quaternion _containerRotation = new Quaternion(0f, 180f, 0f, 0f);

		private Vector2 _spacing = new Vector2(0f, 0f);

		private List<ActivePerkItem> _activePerks = new List<ActivePerkItem>();

		private List<ActivePerkItemContainer> _activePerksContainer = new List<ActivePerkItemContainer>();

		public void Init()
		{
			_spacing = PerkGUI.GetSpacing();
		}

		// Perk icons are presentation and are not rebuilt by rollback re-simulation.
		public void AddEffectPerk(PerksStage.ActionPerk otherPerk, PerksStage.ActionPerk actionPerk)
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return;
			}
			PerkActionSetModEffect effectAction = (PerkActionSetModEffect)actionPerk.Action;
			PerkActionSetModEffect.ModEffectType effectType = effectAction.GetEffectType();
			foreach (ActivePerkItem item in _activePerks)
			{
				PerksStage.ActionPerk action = item.get_Action();
				if (action == otherPerk && effectType == PerkActionSetModEffect.ModEffectType.EFFECT_PULSE)
				{
					item.set_PulseCount(item.get_PulseCount() + 1);
				}
			}
		}

		public void AddActivePerkItem(PerksStage.ActionPerk actionPerk)
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return;
			}
			if (actionPerk != null)
			{
				CreateActivePerkItem(actionPerk);
			}
		}

		private void CreateActivePerkItem(PerksStage.ActionPerk actionPerk)
		{
			if (_activePerkItemPrefab == null)
			{
				GameLog.Error("ActivePerkModel.CreateActivePerkItem: _activePerkItemPrefab is null");
				return;
			}
			ActivePerkItem component = Object.Instantiate(_activePerkItemPrefab).GetComponent<ActivePerkItem>();
			if (component == null)
			{
				GameLog.Error("ActivePerkModel.CreateActivePerkItem: item is null");
				return;
			}
			component.Init(actionPerk);
			string stackKey = actionPerk.StackKey;
			ActivePerkItemContainer activePerkItemContainer = null;
			if (!stackKey.Equals(string.Empty))
			{
				activePerkItemContainer = _activePerksContainer.Find((ActivePerkItemContainer container) => container.get_Stack().Equals(stackKey));
			}
			if (activePerkItemContainer == null)
			{
				if (_activePerkItemContainerPrefab == null)
				{
					GameLog.Error("ActivePerkModel.CreateActivePerkItem: _activePerkItemContainerPrefab is null");
					return;
				}
				activePerkItemContainer = Object.Instantiate(_activePerkItemContainerPrefab).GetComponent<ActivePerkItemContainer>();
				if (activePerkItemContainer == null)
				{
					GameLog.Error("ActivePerkModel.CreateActivePerkItem: itemContainer is null");
					return;
				}
				if (_align == PerkAlignment.ActivePerkRight)
				{
					activePerkItemContainer.transform.localRotation = _containerRotation;
				}
				activePerkItemContainer.transform.SetParent(base.transform, false);
				activePerkItemContainer.Init(PerkGUI.GetStackShiftX(), PerkGUI.GetStackShiftY());
				activePerkItemContainer.set_Stack(stackKey);
				_activePerksContainer.Add(activePerkItemContainer);
			}
			activePerkItemContainer.AddActivePerk(component);
			_activePerks.Add(component);
		}

		public void RemoveActivePerkItem(PerksStage.ActionPerk actionPerk)
		{
			if (actionPerk == null)
			{
				return;
			}
			foreach (ActivePerkItem item in _activePerks)
			{
				if (item.get_Action() == actionPerk)
				{
					item.set_Show(false);
					break;
				}
			}
		}

		public void RemoveAllActivePerkItem()
		{
			_activePerks.ForEach((ActivePerkItem perkItem) =>
			{
				perkItem.set_Show(false);
			});
		}

		public void DestroyAllPerkItems()
		{
			_activePerks.ForEach((ActivePerkItem perkItem) =>
			{
				perkItem.Destroy();
			});
			_activePerks.Clear();
			_activePerksContainer.ForEach((ActivePerkItemContainer container) =>
			{
				container.Destroy();
			});
			_activePerksContainer.Clear();
		}

		private void LayoutContainers()
		{
			float num = 0f;
			float x = _spacing.x;
			foreach (ActivePerkItemContainer item in _activePerksContainer)
			{
				item.set_FinishPosX(num);
				num += x;
				RectTransform rectTransform = item.transform as RectTransform;
				if (rectTransform != null)
				{
					num += rectTransform.rect.width * rectTransform.localScale.x * 0.5f;
				}
			}
		}

		public void Render()
		{
			if (Eclipse.Multiplayer.VersusTickDriver.IsResimulating)
			{
				return;
			}
			for (int i = 0; i < _activePerksContainer.Count; i++)
			{
				ActivePerkItemContainer activePerkItemContainer = _activePerksContainer[i];
				activePerkItemContainer.Render();
				if (activePerkItemContainer.get_NeedDelete())
				{
					_activePerksContainer.RemoveAt(i);
					i--;
				}
			}
			for (int j = 0; j < _activePerks.Count; j++)
			{
				if (_activePerks[j].get_NeedDelete())
				{
					_activePerks.RemoveAt(j);
					j--;
				}
			}
			LayoutContainers();
		}
	}
}
