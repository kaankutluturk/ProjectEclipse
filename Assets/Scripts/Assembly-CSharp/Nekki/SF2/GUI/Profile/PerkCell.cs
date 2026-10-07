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

		private void BindSubItem(PerkSubItem MDPMIEBJMMD)
		{
			MDPMIEBJMMD.ParentCell = this;
			MDPMIEBJMMD.transform.SetLocalY(0f);
			MDPMIEBJMMD.SetSelectFlashing(true);
			MDPMIEBJMMD.SetSelectFlashingMinOpacity(1f / 3f);
			MDPMIEBJMMD.RemoveAllEventListener();
			MDPMIEBJMMD.AddEventListener(2, OnSubItemClick);
			MDPMIEBJMMD.AddEventListener(10, Scene<ProfileScene>.get_Current().OnSubItemClick);
			MDPMIEBJMMD.AddEventListener(12, Scene<ProfileScene>.get_Current().OnPerkImprove);
		}

		public void Init(ProfilePerkContainer IFIEEAGMMMF, int BIPGPCAHKIG, bool NMBEADHHHFH, bool IBMGAPMHMOB)
		{
			Clear();
			BindSubItem(_iconLeft);
			BindSubItem(_iconRight);
			SetPerks(IFIEEAGMMMF.Perks, BIPGPCAHKIG);
			_perkLines.Init(hasTwoPerks, NMBEADHHHFH, IBMGAPMHMOB);
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

		private void SetPerks(List<ProfilePerk> JOGBKOJCINM, int IBAKGENOEPH)
		{
			if (JOGBKOJCINM == null || JOGBKOJCINM.Count == 0)
			{
				_iconLeft.gameObject.SetActive(false);
				_iconRight.gameObject.SetActive(false);
				return;
			}
			_iconLeft.gameObject.SetActive(true);
			hasTwoPerks = JOGBKOJCINM.Count > 1;
			int num = 10000 + IBAKGENOEPH * 10;
			InitSubItem(_iconLeft, JOGBKOJCINM[0], num);
			if (hasTwoPerks)
			{
				_iconRight.gameObject.SetActive(true);
				int iPFAAJAOIJL = num + 1;
				InitSubItem(_iconRight, JOGBKOJCINM[1], iPFAAJAOIJL);
			}
			LayoutIcons();
		}

		private void InitSubItem(PerkSubItem MDPMIEBJMMD, ProfilePerk CENAOGICAAK, int IPFAAJAOIJL)
		{
			MDPMIEBJMMD.Init(CENAOGICAAK, IPFAAJAOIJL);
			Scene<ProfileScene>.get_Current().SubItems.Add(MDPMIEBJMMD);
		}

		public void ChoosePerkByName(string NJDDPMPFCGB)
		{
			if (_iconLeft.get_Perk().GetPerkName() == NJDDPMPFCGB)
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
