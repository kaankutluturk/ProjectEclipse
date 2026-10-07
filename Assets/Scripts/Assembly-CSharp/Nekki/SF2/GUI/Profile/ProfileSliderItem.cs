using System.Collections.Generic;
using UnityEngine;

namespace Nekki.SF2.GUI.Profile
{
	public class ProfileSliderItem : BaseScrollItem
	{
		public enum IconAlignment
		{
			NONE_ALIGNMENT = 0,
			LEFT_ALIGNMENT = 1
		}

		private List<SubItem> icons = new List<SubItem>();

		private float iconSpacing = 95f;

		[SerializeField]
		private PerkTreeLines _perkLines;

		public void Init(float PEIAPNNLFFL = 95f)
		{
			iconSpacing = PEIAPNNLFFL;
			_perkLines.gameObject.SetActive(false);
		}

		public void AddIcons(SubItem ADONPNOBBDE, IconAlignment LJFADBBKKPH = IconAlignment.NONE_ALIGNMENT)
		{
			if (ADONPNOBBDE != null)
			{
				AddIcon(ADONPNOBBDE);
				switch (LJFADBBKKPH)
				{
				case IconAlignment.NONE_ALIGNMENT:
					LayoutIcons();
					break;
				case IconAlignment.LEFT_ALIGNMENT:
					ADONPNOBBDE.transform.SetLocalX(60f - GetComponent<RectTransform>().rect.width / 2f);
					break;
				}
			}
		}

		public void AddIcons(List<SubItem> BAOPCLKCLAF)
		{
			int count = BAOPCLKCLAF.Count;
			for (int i = 0; i < count; i++)
			{
				AddIcon(BAOPCLKCLAF[i]);
			}
			LayoutIcons();
		}

		public List<SubItem> GetIcons()
		{
			return icons;
		}

		public bool IsUnlokedItem()
		{
			bool result = false;
			for (int i = 0; i < icons.Count; i++)
			{
				if (!icons[i].GetLock())
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public virtual void UpdateState()
		{
			foreach (SubItem item in icons)
			{
				item.UpdateState();
			}
		}

		public void Clear()
		{
			foreach (SubItem item in icons)
			{
				item.ParentCell = null;
				Object.Destroy(item.gameObject);
			}
			icons.Clear();
		}

		private void AddIcon(SubItem ADONPNOBBDE)
		{
			ADONPNOBBDE.transform.SetLocalY(0f);
			icons.Add(ADONPNOBBDE);
		}

		private void LayoutIcons()
		{
			int count = icons.Count;
			switch (count)
			{
			case 0:
				break;
			case 2:
			{
				SubItem subItem = icons[0];
				float num2 = subItem.GetComponent<RectTransform>().rect.width / 2f + iconSpacing;
				subItem.transform.SetLocalX(0f - num2);
				icons[1].transform.SetLocalX(num2);
				break;
			}
			default:
			{
				float num = (0f - GetComponent<RectTransform>().rect.width) / 2f + GetComponent<RectTransform>().rect.width / 2f / (float)count;
				for (int i = 0; i < count; i++)
				{
					icons[i].transform.SetLocalX(num + GetComponent<RectTransform>().rect.width / (float)count * (float)i);
				}
				break;
			}
			}
		}

		public void AddPerkLines(bool NMBEADHHHFH, bool IBMGAPMHMOB)
		{
			_perkLines.gameObject.SetActive(true);
			int count = GetIcons().Count;
			_perkLines.Init(count >= 2, !NMBEADHHHFH, !IBMGAPMHMOB);
		}
	}
}
