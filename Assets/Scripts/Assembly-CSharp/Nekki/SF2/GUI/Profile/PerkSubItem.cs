using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nekki.SF2.GUI.Profile
{
	public class PerkSubItem : SubItem
	{
		public enum PerkSubItemEvent
		{
			onPerkImprove = 12
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ProfilePerk perk;

		protected Action<object> _dlg;

		protected InfoAnimation infoAnimation;

		[SerializeField]
		private ResolutionImage _upgradeLevelIcon;

		protected PerkContentData perkInfo;

		protected bool isForcedLocked;

		protected bool isPerkChosen;

		public ProfilePerk CurrentPerk
		{
			get
			{
				return get_Perk();
			}
			private set
			{
				SetPerk(value);
			}
		}

		public ProfilePerk get_Perk()
		{
			return perk;
		}

		private void SetPerk(ProfilePerk value)
		{
			perk = value;
		}

		public void Init(ProfilePerk profilePerk, int buttonId)
		{
			Init(buttonId);
			_upgradeLevelIcon.gameObject.SetActive(false);
			Clear();
			SetPerk(profilePerk);
			if (get_Perk() != null)
			{
				get_Perk().AddEventListener(0, OnPerkStateChanged);
				get_Perk().AddEventListener(1, OnPerkRemoved);
				get_Perk().AddEventListener(2, OnPerkUpgraded);
				get_Perk().AddEventListener(3, OnPerkChanged);
			}
			_dlg = OnImproveClicked;
			infoAnimation = FindInfoAnimation();
			if (get_Perk() != null)
			{
				perkInfo = new PerkContentData(get_Perk().GetPerkName(), get_Perk().GetDescription(), get_Perk().GetState(), _dlg, infoAnimation);
				Data = perkInfo;
			}
			spriteName = ((get_Perk() == null) ? string.Empty : get_Perk().GetImageName());
			isPerkChosen = false;
			isForcedLocked = false;
			iconMaxOpacity = ProfileGUI.PerkOpacity.Max / 255f;
			iconMinOpacity = ProfileGUI.PerkOpacity.Min / 255f;
			SetBackPictureVisible(true);
			UpdateIcon();
			UpdateState();
			UpdateLevelIcon();
			if (ListSF.GetRoster().GetLevel() < profilePerk.GetLevel())
			{
				SetLock(true);
			}
		}

		private new void OnDestroy()
		{
			Clear();
			RemoveAllEventListener();
		}

		public void Clear()
		{
			if (get_Perk() != null)
			{
				get_Perk().RemoveEventListener(0, OnPerkStateChanged);
				get_Perk().RemoveEventListener(1, OnPerkRemoved);
				get_Perk().RemoveEventListener(2, OnPerkUpgraded);
				get_Perk().RemoveEventListener(3, OnPerkChanged);
			}
		}

		public override void SetLock(bool locked)
		{
			isForcedLocked = locked;
			base.SetLock(isForcedLocked || (get_Perk() != null && get_Perk().GetState() == ProfilePerk.ProfilePerkState.PERK_LOCK));
		}

		public override bool GetLock()
		{
			return isForcedLocked || get_Perk() == null || get_Perk().GetState() == ProfilePerk.ProfilePerkState.PERK_LOCK;
		}

		public override void Choose()
		{
			if (get_Perk() != null)
			{
				((PerkContentData)Data).state = get_Perk().GetState();
			}
			base.Choose();
		}

		public override void UpdateState()
		{
			if (get_Perk() == null)
			{
				return;
			}
			switch (get_Perk().GetState())
			{
			case ProfilePerk.ProfilePerkState.PERK_AVAILABLE:
				isPerkChosen = false;
				SetActive(true);
				break;
			case ProfilePerk.ProfilePerkState.PERK_UNAVAILABLE:
				isPerkChosen = false;
				if ((bool)_icon)
				{
					UIExtensions.SetAlpha(_icon, iconMaxOpacity);
				}
				SetActive(false);
				break;
			case ProfilePerk.ProfilePerkState.PERK_SELECTED:
				isPerkChosen = true;
				if ((bool)_icon)
				{
					UIExtensions.SetAlpha(_icon, iconMaxOpacity);
				}
				SetActive(true);
				break;
			case ProfilePerk.ProfilePerkState.PERK_LOCK:
				SetActive(true);
				SetSelected(false);
				break;
			}
			SetLock(isForcedLocked);
			if (!isLocked)
			{
			}
		}

		public bool IsInfoAnimation()
		{
			return infoAnimation != null;
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			if (!GetLock())
			{
				base.OnPointerDown(eventData);
			}
		}

		protected override void UpdateSelectedFlash()
		{
			base.UpdateSelectedFlash();
			if (get_Perk() != null && get_Perk().GetState() == ProfilePerk.ProfilePerkState.PERK_AVAILABLE)
			{
				UpdateIconFlash();
			}
		}

		protected InfoAnimation FindInfoAnimation()
		{
			if (get_Perk() == null)
			{
				return null;
			}
			List<Trick> list = AnimationData.GetTricks();
			for (int i = 0; i < list.Count; i++)
			{
				if (get_Perk().GetPerkInfo() != null && list[i].Name == get_Perk().GetPerkInfo().MoveName)
				{
					return list[i].Animation;
				}
			}
			return null;
		}

		protected bool IsPerkSelected()
		{
			return get_Perk() != null && get_Perk().GetState() == ProfilePerk.ProfilePerkState.PERK_SELECTED;
		}

		protected bool IsPerkUnavailable()
		{
			return get_Perk() != null && get_Perk().GetState() == ProfilePerk.ProfilePerkState.PERK_UNAVAILABLE;
		}

		protected string GetLevelSpriteName()
		{
			int num = ((get_Perk() == null) ? 1 : get_Perk().GetUpgradeLevel());
			string text = "ProfilePieces.level";
			return text + num;
		}

		protected void UpdateLevelIcon()
		{
			if (get_Perk() != null)
			{
				bool flag = get_Perk().GetUpgradeLevel() <= 0;
				ProfilePerk.ProfilePerkType perkType = get_Perk().get_Type();
				bool flag2 = get_Perk().GetUpgradeLevel() == 1 && perkType != ProfilePerk.ProfilePerkType.TYPE_UPGRADE && perkType != ProfilePerk.ProfilePerkType.TYPE_PERK_SELETED;
				if (!flag && !flag2)
				{
					_upgradeLevelIcon.gameObject.SetActive(true);
					string spriteName = GetLevelSpriteName();
					_upgradeLevelIcon.set_SpriteName(spriteName);
					float num = 95f;
					float num2 = -34f;
					_upgradeLevelIcon.transform.SetLocalX(num + num2);
					_upgradeLevelIcon.transform.SetLocalY(0f - (num + num2));
				}
			}
		}

		protected void RefreshPerkInfo()
		{
			infoAnimation = FindInfoAnimation();
			if (perkInfo != null && get_Perk() != null)
			{
				perkInfo.name = get_Perk().GetPerkName();
				perkInfo.description = get_Perk().GetDescription();
				perkInfo.state = get_Perk().GetState();
				perkInfo.Animation = infoAnimation;
			}
		}

		private void OnImproveClicked(object data)
		{
			CallEvent(12, this);
		}

		private void OnPerkStateChanged(object data)
		{
			UpdateState();
		}

		private void OnPerkRemoved(object data)
		{
			SetPerk(null);
		}

		private void OnPerkUpgraded(object data)
		{
			UpdateIcon();
			UpdateState();
			RefreshPerkInfo();
			UpdateLevelIcon();
		}

		private void OnPerkChanged(object data)
		{
			RefreshPerkInfo();
			UpdateLevelIcon();
		}
	}
}
