using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Fight
{
	public class DisplayModel : MonoBehaviour
	{
		[SerializeField]
		private ResolutionImageAvatar avatar;

		[SerializeField]
		private ResolutionImage complete;

		[SerializeField]
		private LayoutElement layoutElement;

		private string texturePath = SF2Paths.GetUsersUiPath();

		public ResolutionImageAvatar AvatarImage
		{
			get
			{
				return get_Avatar();
			}
		}

		public ResolutionImageAvatar get_Avatar()
		{
			return avatar;
		}

		public ResolutionImage get_Complete()
		{
			return complete;
		}

		public void SetAvatar(string spriteName)
		{
			if (avatar != null)
			{
				avatar.set_TexturePath(texturePath);
				avatar.set_SpriteName(spriteName);
				avatar.SetNativeSize();
			}
		}

		public void Completed()
		{
			if (complete != null)
			{
				complete.gameObject.SetActive(true);
			}
		}

		public void ScaleAvatar(Vector2 scale)
		{
			if (avatar != null)
			{
				avatar.transform.localScale = scale;
				RectTransform rectTransform = base.transform as RectTransform;
				if (rectTransform != null && layoutElement != null)
				{
					Vector2 size = avatar.rectTransform.rect.size;
					size.x *= scale.x;
					size.y *= scale.y;
					rectTransform.sizeDelta = size;
					layoutElement.minWidth = size.x;
					layoutElement.minHeight = size.y;
				}
			}
			if (complete != null)
			{
				complete.transform.localScale = scale;
			}
		}

		public void SetSizeDelta(Vector2 sizeDelta)
		{
			RectTransform rectTransform = base.transform as RectTransform;
			if (rectTransform != null && layoutElement != null)
			{
				rectTransform.sizeDelta = sizeDelta;
				layoutElement.minWidth = sizeDelta.x;
				layoutElement.minHeight = sizeDelta.y;
			}
		}
	}
}
