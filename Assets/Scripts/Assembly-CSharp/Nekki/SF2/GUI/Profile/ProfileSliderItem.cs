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

		public void Init(float spacing = 95f)
		{
			iconSpacing = spacing;
			_perkLines.gameObject.SetActive(false);
		}

		public void AddIcons(SubItem icon, IconAlignment alignment = IconAlignment.NONE_ALIGNMENT)
		{
			if (icon != null)
			{
				AddIcon(icon);
				switch (alignment)
				{
				case IconAlignment.NONE_ALIGNMENT:
					LayoutIcons();
					break;
				case IconAlignment.LEFT_ALIGNMENT:
					icon.transform.SetLocalX(60f - GetComponent<RectTransform>().rect.width / 2f);
					break;
				}
			}
		}

		public void AddIcons(List<SubItem> newIcons)
		{
			int count = newIcons.Count;
			for (int i = 0; i < count; i++)
			{
				AddIcon(newIcons[i]);
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

		private void AddIcon(SubItem icon)
		{
			icon.transform.SetLocalY(0f);
			icons.Add(icon);
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

		public void AddPerkLines(bool isFirst, bool isLast)
		{
			_perkLines.gameObject.SetActive(true);
			int count = GetIcons().Count;
			_perkLines.Init(count >= 2, !isFirst, !isLast);
		}
	}
}
