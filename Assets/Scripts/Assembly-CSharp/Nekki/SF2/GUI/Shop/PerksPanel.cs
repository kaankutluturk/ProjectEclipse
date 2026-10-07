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

		public virtual void SetPerks(List<PerkInfoItem> JOGBKOJCINM)
		{
			Clear();
			if (JOGBKOJCINM != null)
			{
				JOGBKOJCINM.ForEach(CreatePerkItem);
			}
		}

		public void CreatePerkItem(PerkInfoItem CBINHDDCIEA)
		{
			if (CBINHDDCIEA != null && CBINHDDCIEA.ImageName != null && !CBINHDDCIEA.ImageName.Equals(string.Empty))
			{
				GameObject AOMLCBHAJJH = new GameObject(CBINHDDCIEA.Name);
				ResolutionImage resolutionImage = AOMLCBHAJJH.AddComponent<ResolutionImage>();
				TouchHandler touchHandler = AOMLCBHAJJH.AddComponent<TouchHandler>();
				LayoutElement layoutElement = AOMLCBHAJJH.AddComponent<LayoutElement>();
				touchHandler.transition = Selectable.Transition.None;
				touchHandler.get_OnTouch().AddListener(() =>
				{
					Vector3 position = AOMLCBHAJJH.transform.position;
					onPerksClick.Invoke(CBINHDDCIEA, position, hintOffset, AOMLCBHAJJH);
				});
					string icon = CBINHDDCIEA.ImageName;
					resolutionImage.set_SpriteName((icon.IndexOf(':') > 0) ? icon : (iconAtlasPrefix + icon));
				layoutElement.minHeight = resolutionImage.rectTransform.rect.height;
				layoutElement.minWidth = resolutionImage.rectTransform.rect.width;
				AOMLCBHAJJH.transform.SetParent(base.gameObject.transform, false);
			}
		}

		private void OnDestroy()
		{
			onPerksClick.RemoveAllListeners();
		}
	}
}
