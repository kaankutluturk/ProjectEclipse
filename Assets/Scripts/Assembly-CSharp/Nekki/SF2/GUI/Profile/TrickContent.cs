using System;
using System.Collections.Generic;
using Nekki.SF2.GUI.Shop;
using UnityEngine;

namespace Nekki.SF2.GUI.Profile
{
	public class TrickContent : Content
	{
		public enum TrickContentLayer
		{
			zText = 0,
			zSlider = 1,
			zIcon = 2,
			zGradient = 3
		}

		private const int SHOW_BUTTON_Y = -294;

		private const int MAX_CONTENT_HEIGHT = 400;

		private const string DAMAGE_ICON_SPRITE = "ComboButtons.base_damage";

		private const int DAMAGE_LABEL_PADDING = 40;

		private const int DAMAGE_LABEL_X = -20;

		private const int DAMAGE_LABEL_Y = 10;

		private const int DESCRIPTION_OFFSET_Y = -20;

		[SerializeField]
		private LabelAlias _valueLabel;

		[SerializeField]
		private LabelAlias _descriptionLabel;

		[SerializeField]
		private LabelButton _btnShow;

		[SerializeField]
		private Scroll _slider;

		private List<float> _value = new List<float>();

		private string _description;

		private InfoAnimation infoAnimation;

		private Action<object> showCallback;

		private void Start()
		{
			_btnShow.onClick.AddListener(OnShowClicked);
		}

		public void Init(InfoAnimation animation, List<float> values, Action<object> callback = null, string description = "")
		{
			infoAnimation = animation;
			_value = values;
			showCallback = callback;
			_description = description;
			InitDescriptionLabel();
			InitShowButton();
			UpdateDamageLabel();
			bool flag = _descriptionLabel.gameObject.activeSelf && _descriptionLabel.preferredHeight + _valueLabel.preferredHeight > 400f;
			bool flag2 = _valueLabel.preferredHeight > 400f;
			if (flag || flag2)
			{
			}
			LayoutDescription();
		}

		public override void SetUpBorder(float upBorder)
		{
			if (_btnShow != null && _valueLabel != null)
			{
				float num = _btnShow.transform.localPosition.y + _btnShow.GetComponent<RectTransform>().rect.height / 2f;
				float valueY = upBorder - (upBorder - num) / 2f;
				_valueLabel.transform.SetLocalY(valueY);
			}
		}

		public LabelButton GetBtnShow()
		{
			return _btnShow;
		}

		private void InitDescriptionLabel()
		{
			_descriptionLabel.gameObject.SetActive(false);
			if (!(_description == string.Empty))
			{
				_descriptionLabel.set_LabelFontSize(104);
				_descriptionLabel.color = Constants.DialogTextColor;
				_descriptionLabel.set_Alias(_description);
			}
		}

		private void UpdateDamageLabel()
		{
			var hits = new List<string>();
			for (int i = 0; i < _value.Count;)
			{
				int damage = (int)(100f * _value[i]);
				int count = 1;
				while (i + count < _value.Count && (int)(100f * _value[i + count]) == damage) count++;
				hits.Add(count > 1 ? damage + " x " + count : damage.ToString());
				i += count;
			}
			string text = string.Join(" + ", hits);
			Transform existing = _valueLabel.transform.Find("DamageIcon");
			ResolutionImage icon;
			if (existing == null)
			{
				var node = new GameObject("DamageIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(ResolutionImage));
				node.layer = _valueLabel.gameObject.layer;
				node.transform.SetParent(_valueLabel.transform, false);
				icon = node.GetComponent<ResolutionImage>();
				icon.set_TexturePath("UI/Atlases/");
				icon.set_SpriteName(DAMAGE_ICON_SPRITE);
				icon.raycastTarget = false;
				icon.preserveAspect = true;
			}
			else icon = existing.GetComponent<ResolutionImage>();
			_valueLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
			_valueLabel.resizeTextForBestFit = true;
			_valueLabel.resizeTextMinSize = 24;
			icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			icon.rectTransform.sizeDelta = new Vector2(64f, 64f);
			icon.gameObject.SetActive(_value.Count > 0);
			_valueLabel.transform.SetLocalX(-20f);
			_valueLabel.transform.SetLocalY(10f);
			_valueLabel.color = Constants.DialogTextColor;
			_valueLabel.set_text(text);
			icon.rectTransform.anchoredPosition = new Vector2(-_valueLabel.preferredWidth * 0.5f - 42f, 0f);
			_valueLabel.SetVerticesDirty();
			OnDamageLabelUpdated();
			_slider.ScrollToItem(0);
		}

		private void InitShowButton()
		{
			_btnShow.transform.SetLocalX(0f);
			_btnShow.transform.SetLocalY(-294f);
			if (infoAnimation == null || !infoAnimation.ShowInTricks)
			{
				_btnShow.gameObject.SetActive(false);
			}
		}

		private void LayoutDescription()
		{
			if (_descriptionLabel != null && _slider != null && _value.Count > 0)
			{
				if (_valueLabel.rectTransform.rect.height + _descriptionLabel.rectTransform.rect.height < 400f)
				{
					_descriptionLabel.transform.SetLocalY(-20f + _valueLabel.rectTransform.rect.height / 2f);
				}
				else
				{
					_descriptionLabel.transform.SetLocalY(155f);
				}
			}
		}

		private void OnShowClicked()
		{
			if (showCallback != null)
			{
				showCallback(null);
			}
		}

		private void OnDamageLabelUpdated()
		{
		}
	}
}
