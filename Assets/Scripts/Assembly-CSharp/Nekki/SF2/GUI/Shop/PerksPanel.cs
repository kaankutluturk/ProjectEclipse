using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Shop
{
	public class PerksPanel : MonoBehaviour
	{
		public class PerkClickEvent : UnityEvent<PerkInfoItem, Vector2, Vector2, GameObject>
		{
		}

		public PerkClickEvent onPerksClick = new PerkClickEvent();

		protected string iconAtlasPrefix = "Enchantments.";

		protected Vector2 hintOffset = new Vector2(0f, -25f);

		private RectTransform _rectTransform;

		public RectTransform PanelRect
		{
			get
			{
				return get_RectTransform();
			}
		}

		public RectTransform get_RectTransform()
		{
			if (_rectTransform == null)
			{
				_rectTransform = base.transform as RectTransform;
			}
			return _rectTransform;
		}

		public virtual void Clear()
		{
			List<GameObject> list = new List<GameObject>();
			foreach (Transform item in base.transform)
			{
				item.gameObject.SetActive(false);
				list.Add(item.gameObject);
			}
			base.transform.DetachChildren();
			list.ForEach(Object.Destroy);
		}

		public virtual void SetPerks(List<PerkInfoItem> perks)
		{
			Clear();
			if (perks != null)
			{
				perks.ForEach(CreatePerkItem);
			}
		}

		public void CreatePerkItem(PerkInfoItem perkItem)
		{
			if (perkItem != null && perkItem.ImageName != null && !perkItem.ImageName.Equals(string.Empty))
			{
				GameObject perkObject = new GameObject(perkItem.Name);
				ResolutionImage resolutionImage = perkObject.AddComponent<ResolutionImage>();
				TouchHandler touchHandler = perkObject.AddComponent<TouchHandler>();
				LayoutElement layoutElement = perkObject.AddComponent<LayoutElement>();
				touchHandler.transition = Selectable.Transition.None;
				touchHandler.get_OnTouch().AddListener(() =>
				{
					Vector3 position = perkObject.transform.position;
					onPerksClick.Invoke(perkItem, position, hintOffset, perkObject);
				});
					string icon = perkItem.ImageName;
					resolutionImage.set_SpriteName((icon.IndexOf(':') > 0) ? icon : (iconAtlasPrefix + icon));
				layoutElement.minHeight = resolutionImage.rectTransform.rect.height;
				layoutElement.minWidth = resolutionImage.rectTransform.rect.width;
				perkObject.transform.SetParent(base.gameObject.transform, false);
			}
		}

		private void OnDestroy()
		{
			onPerksClick.RemoveAllListeners();
		}
	}
}
