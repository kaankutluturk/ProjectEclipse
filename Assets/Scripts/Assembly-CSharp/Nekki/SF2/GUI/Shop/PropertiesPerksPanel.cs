using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Shop
{
	public class PropertiesPerksPanel : PerksPanel
	{
		[SerializeField]
		private GameObject _itemPrefab;

		private List<ParameterScrollItem> _items = new List<ParameterScrollItem>();

		public override void Clear()
		{
			foreach (ParameterScrollItem item in _items)
			{
				item.gameObject.SetActive(false);
				Object.Destroy(item.gameObject);
			}
			_items.Clear();
		}

		public override void SetPerks(List<PerkInfoItem> properties)
		{
			Clear();
			if (!(_itemPrefab != null))
			{
				return;
			}
			foreach (PerkInfoItem item in properties)
			{
				GameObject gameObject = Object.Instantiate(_itemPrefab);
				gameObject.transform.SetParent(base.gameObject.transform, false);
				ParameterScrollItem component = gameObject.GetComponent<ParameterScrollItem>();
				if (component != null)
				{
					component.set_AtlasName(iconAtlasPrefix);
					bool eIAKNKDEEKA = false;
					component.Init(string.Empty, item.ImageName, item.BarValue, item.BarValue, eIAKNKDEEKA, item.BarScale);
					component.set_MinHeight(100f);
					component.interactable = false;
					_items.Add(component);
					GameObject gameObject2 = new GameObject("TouchHandler");
					RectTransform rectTransform = gameObject2.AddComponent<RectTransform>();
					TouchHandler fPDGFGEEJEA = gameObject2.AddComponent<TouchHandler>();
					Image image = gameObject2.AddComponent<Image>();
					image.color = new Color(1f, 1f, 1f, 0f);
					if (rectTransform != null)
					{
						Vector2 size = component.get_Size();
						size.x = get_RectTransform().sizeDelta.x;
						rectTransform.sizeDelta = size;
					}
					gameObject2.transform.SetParent(component.transform, false);
					gameObject2.transform.SetAsLastSibling();
					AddTouchListener(fPDGFGEEJEA, item);
				}
			}
		}

		private void AddTouchListener(TouchHandler FPDGFGEEJEA, PerkInfoItem AEFFHJGMNFI)
		{
			if (FPDGFGEEJEA != null)
			{
				FPDGFGEEJEA.get_OnTouch().AddListener(() =>
				{
					Vector3 position = FPDGFGEEJEA.transform.position;
					onPerksClick.Invoke(AEFFHJGMNFI, position, hintOffset, FPDGFGEEJEA.gameObject);
				});
			}
		}
	}
}
