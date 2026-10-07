using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Fight
{
	public class ComboItem : MonoBehaviour
	{
		private const string CriticalSprite = "FightUI.Critical";

		private const string FirstStrikeSprite = "FightUI.First_Strike";

		private const string HeadHitSprite = "FightUI.Head_Hit";

		private const string ComboSprite = "FightUI.Combo";

		private const string HotGroundSprite = "FightUI.hot_ground";

		private const string ShockSprite = "FightUI.shock";

		[SerializeField]
		private Color _labelColorHotground = new Color32(254, 253, 131, byte.MaxValue);

		[SerializeField]
		private int _labelFontSizeHotground = 120;

		[SerializeField]
		private ResolutionImage _image;

		[SerializeField]
		private LabelAlias _label;

		[SerializeField]
		private Shadow _labelShadow;

		[SerializeField]
		private HorizontalLayoutGroup _layout;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ComboTypes comboType;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ScreenModel.ScreenSide modelSide;

		public ComboTypes ItemType
		{
			get
			{
				return get_ComboType();
			}
			private set
			{
				SetComboType(value);
			}
		}

		public ScreenModel.ScreenSide ModelSide
		{
			get
			{
				return get_ModelType();
			}
			private set
			{
				SetModelSide(value);
			}
		}

		public ComboTypes get_ComboType()
		{
			return comboType;
		}

		private void SetComboType(ComboTypes value)
		{
			comboType = value;
		}

		public ScreenModel.ScreenSide get_ModelType()
		{
			return modelSide;
		}

		private void SetModelSide(ScreenModel.ScreenSide value)
		{
			modelSide = value;
		}

		public RectTransform get_rectTransform()
		{
			return base.transform as RectTransform;
		}

		public void Init(ComboTypes LFLGCDNKNJI, ScreenModel.ScreenSide NPEAOKLDJHA)
		{
			SetModelSide(NPEAOKLDJHA);
			SetComboType(LFLGCDNKNJI);
			if (_image != null)
			{
				_image.set_SpriteName(GetSpriteName(LFLGCDNKNJI));
				_image.SetNativeSize();
				LayoutElement component = _image.GetComponent<LayoutElement>();
				component.minWidth = _image.rectTransform.rect.width;
				component.minHeight = _image.rectTransform.rect.height;
			}
			if (_labelShadow != null)
			{
				_labelShadow.gameObject.SetActive(LFLGCDNKNJI == ComboTypes.TypeHotGroundTimer);
			}
			if (_label != null)
			{
				if (LFLGCDNKNJI == ComboTypes.TypeHotGroundTimer)
				{
					_label.color = _labelColorHotground;
					_label.set_LabelFontSize(_labelFontSizeHotground);
				}
				if (NPEAOKLDJHA == ScreenModel.ScreenSide.TYPE_LEFT)
				{
					_label.transform.SetAsLastSibling();
				}
				else
				{
					_label.transform.SetAsFirstSibling();
				}
			}
			UpdateSize();
		}

		private string GetSpriteName(ComboTypes LFLGCDNKNJI)
		{
			switch (LFLGCDNKNJI)
			{
			case ComboTypes.TypeFirstStrike:
				return "FightUI.First_Strike";
			case ComboTypes.TypeHead:
				return "FightUI.Head_Hit";
			case ComboTypes.TypeCritical:
				return "FightUI.Critical";
			case ComboTypes.TypeCombo:
				return "FightUI.Combo";
			case ComboTypes.TypeHotGroundTimer:
				return "FightUI.hot_ground";
			case ComboTypes.TypeShock:
				return "FightUI.shock";
			default:
				return string.Empty;
			}
		}

		public void UpdateCount(int count)
		{
			_label.set_text(count.ToString());
			_label.gameObject.SetActive(true);
			UpdateSize();
		}

		public void UpdateSize()
		{
			if (_image != null && _label != null && _layout != null)
			{
				Vector2 sizeDelta = new Vector2(0f, 0f);
				sizeDelta.x = _image.rectTransform.rect.width;
				sizeDelta.y = _image.rectTransform.rect.height;
				if (!_label.get_text().Equals(string.Empty))
				{
					sizeDelta.x += _label.preferredWidth;
					sizeDelta.x += _layout.spacing;
				}
				get_rectTransform().sizeDelta = sizeDelta;
			}
		}
	}
}
