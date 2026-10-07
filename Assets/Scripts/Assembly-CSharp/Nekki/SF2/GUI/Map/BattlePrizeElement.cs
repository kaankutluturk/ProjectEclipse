using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Map
{
	public class BattlePrizeElement : SFMonoBehaviour<object>
	{
		public const string ICONS_ATLAS = "MiscSprites";


		[SerializeField]
		private ResolutionImage _icon;

		[SerializeField]
		private Text _value;

		public void Init(string iconName, long value, int fontSize, float scale = 1f)
		{
			_icon.set_TexturePath("MiscSprites");
			_icon.set_SpriteName(iconName);
			_icon.SetNativeSize();
			float nativeWidth = _icon.rectTransform.rect.width;
			float nativeHeight = _icon.rectTransform.rect.height;
			float nativeMax = Mathf.Max(nativeWidth, nativeHeight);
			float iconScale = (nativeMax > fontSize) ? (float)fontSize / nativeMax : 1f;
			iconScale *= scale;
			_icon.rectTransform.localScale = new Vector3(iconScale, iconScale);
			_value.font = LocalizationManager.GetContentFont();
			_value.fontSize = fontSize;
			_value.color = Constants.DialogTextColor;
			_value.text = value.ToString();
			UpdateLayout();
		}

		private void UpdateLayout()
		{
			float num = 10f;
			float num2 = _icon.rectTransform.rect.width * _icon.rectTransform.localScale.x;
			float num3 = _icon.rectTransform.rect.height * _icon.rectTransform.localScale.y;
			float preferredWidth = LayoutUtility.GetPreferredWidth(_value.rectTransform);
			float preferredHeight = LayoutUtility.GetPreferredHeight(_value.rectTransform);
			_icon.transform.SetLocalX((0f - preferredWidth) / 2f - num / 2f);
			_value.transform.SetLocalX(num2 / 2f + num / 2f);
			float num4 = num2 + preferredWidth + num;
			float num5 = ((!(num3 > preferredHeight)) ? preferredHeight : num3);
			LayoutElement component = GetComponent<LayoutElement>();
			component.minWidth = num4;
			component.preferredWidth = num4;
			component.minHeight = num5;
			component.preferredHeight = num5;
		}
	}
}
