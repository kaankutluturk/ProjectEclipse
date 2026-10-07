using System.Collections.Generic;
using UnityEngine;

namespace Nekki.SF2.GUI.Profile
{
	public class PerkCell : ProfileCell
	{
		public enum PerkCellEvent
		{
			OnSubItemClick = 0
		}

		private const int CELL_KIND_ID = 1;

		private const int ICON_SPACING = 95;

		[SerializeField]
		private PerkSubItem _iconLeft;

		[SerializeField]
		private PerkSubItem _iconRight;

		[SerializeField]
		private PerkTreeLines _perkLines;

		private bool hasTwoPerks;

		private void BindSubItem(PerkSubItem subItem)
		{
			subItem.ParentCell = this;
			subItem.transform.SetLocalY(0f);
			subItem.SetSelectFlashing(true);
			subItem.SetSelectFlashingMinOpacity(1f / 3f);
			subItem.RemoveAllEventListener();
			subItem.AddEventListener(2, OnSubItemClick);
			subItem.AddEventListener(10, Scene<ProfileScene>.get_Current().OnSubItemClick);
			subItem.AddEventListener(12, Scene<ProfileScene>.get_Current().OnPerkImprove);
		}

		public void Init(ProfilePerkContainer perkContainer, int rowIndex, bool isFirst, bool isLast)
		{
			Clear();
			BindSubItem(_iconLeft);
			BindSubItem(_iconRight);
			SetPerks(perkContainer.Perks, rowIndex);
			_perkLines.Init(hasTwoPerks, isFirst, isLast);
		}

		public override SubItem GetFirstIcon()
		{
			return _iconLeft;
		}

		public override void UpdateState()
		{
			_iconLeft.UpdateState();
			if (hasTwoPerks)
			{
				_iconRight.UpdateState();
			}
		}

		public override void Clear()
		{
			hasTwoPerks = false;
			Scene<ProfileScene>.get_Current().SubItems.Remove(_iconLeft);
			Scene<ProfileScene>.get_Current().SubItems.Remove(_iconRight);
			_iconLeft.Clear();
			_iconRight.Clear();
			_iconRight.gameObject.SetActive(false);
		}

		private void LayoutIcons()
		{
			if (!hasTwoPerks)
			{
				_iconLeft.transform.SetLocalX(0f);
				return;
			}
			float num = _iconLeft.GetComponent<RectTransform>().rect.width / 2f + 95f;
			_iconLeft.transform.SetLocalX(0f - num);
			_iconRight.transform.SetLocalX(num);
		}

		private void SetPerks(List<ProfilePerk> perks, int cellIndex)
		{
			if (perks == null || perks.Count == 0)
			{
				_iconLeft.gameObject.SetActive(false);
				_iconRight.gameObject.SetActive(false);
				return;
			}
			_iconLeft.gameObject.SetActive(true);
			hasTwoPerks = perks.Count > 1;
			int num = 10000 + cellIndex * 10;
			InitSubItem(_iconLeft, perks[0], num);
			if (hasTwoPerks)
			{
				_iconRight.gameObject.SetActive(true);
				int rightButtonId = num + 1;
				InitSubItem(_iconRight, perks[1], rightButtonId);
			}
			LayoutIcons();
		}

		private void InitSubItem(PerkSubItem subItem, ProfilePerk perk, int buttonId)
		{
			subItem.Init(perk, buttonId);
			Scene<ProfileScene>.get_Current().SubItems.Add(subItem);
		}

		public void ChoosePerkByName(string perkName)
		{
			if (_iconLeft.get_Perk().GetPerkName() == perkName)
			{
				_iconLeft.Choose();
			}
			else
			{
				_iconRight.Choose();
			}
		}

		public void OnSubItemClick(object data)
		{
			CallEvent(0, get_RowNumber());
		}

		private void OnDestroy()
		{
			RemoveAllEventListener();
		}
	}
}
