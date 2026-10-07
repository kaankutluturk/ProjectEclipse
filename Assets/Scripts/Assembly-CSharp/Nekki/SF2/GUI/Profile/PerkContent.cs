using System;
using UnityEngine;

namespace Nekki.SF2.GUI.Profile
{
	public class PerkContent : Content
	{
		public enum PerkContentLayer
		{
			zText = 0,
			zIcon = 1
		}

		private const int HEADER_FONT_SIZE = 88;

		private const int TEXT_FONT_SIZE = 70;

		private const int IMPROVE_LABEL_FONT_SIZE = 104;

		private const float IMPROVE_BUTTON_Y = -294f;

		private const float IMPROVE_LABEL_Y = -294f;

		private const int KEY_ICON_SPACING = 110;

		[SerializeField]
		private GameObject _keys;

		[SerializeField]
		private LabelAlias _textLabel;

		[SerializeField]
		private LabelButton _btnImprove;

		[SerializeField]
		private LabelAlias _lblImprove;

		private Action<object> improveCallback;

		private InfoAnimation infoAnimation;

		private string _text;

		private string improveButtonAlias;

		private string improveLabelAlias;

		private ProfilePerk.ProfilePerkState perkState;

		private bool hasUpBorder;

		private float upBorder;

		private float textWidth;

		private void Start()
		{
			_btnImprove.onClick.AddListener(OnImproveClicked);
		}

		public void Init(string text, ProfilePerk.ProfilePerkState state, Action<object> improveAction = null, InfoAnimation animation = null, float labelWidth = -1f)
		{
			_text = text;
			improveCallback = improveAction;
			perkState = state;
			textWidth = labelWidth;
			infoAnimation = animation;
			improveButtonAlias = "profile_BtnImprove";
			improveLabelAlias = "profile_LblImprove";
			HeaderFontSize = 88;
			InitImproveButton();
			InitKeyIcons();
			InitLabels();
			LayoutLabels();
		}

		public override void SetUpBorder(float upBorderValue)
		{
			upBorder = upBorderValue;
			hasUpBorder = true;
		}

		public LabelButton GetBtnImprove()
		{
			return _btnImprove;
		}

		private void InitLabels()
		{
			_textLabel.color = Constants.DialogTextColor;
			_textLabel.rectTransform.sizeDelta = new Vector2(textWidth, _textLabel.rectTransform.rect.height);
			_textLabel.set_LabelFontSize(70);
			_textLabel.set_Alias(_text);
			_lblImprove.color = Constants.DialogTextColor;
			_lblImprove.rectTransform.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width - 120f, _lblImprove.rectTransform.rect.height);
			_lblImprove.set_LabelFontSize(104);
			_lblImprove.set_Alias(improveLabelAlias);
			_lblImprove.transform.SetLocalX(0f);
			_lblImprove.transform.SetLocalY(-294f);
			if (perkState == ProfilePerk.ProfilePerkState.PERK_SELECTED)
			{
				_lblImprove.gameObject.SetActive(true);
			}
			else
			{
				_lblImprove.gameObject.SetActive(false);
			}
		}

		private void LayoutLabels()
		{
			_textLabel.rectTransform.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width - 120f, _textLabel.rectTransform.rect.height);
			_lblImprove.rectTransform.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width - 120f, _lblImprove.rectTransform.rect.height);
			float keysY = 0f;
			if (hasUpBorder)
			{
				float contentBottom = upBorder;
				float num = _btnImprove.transform.localPosition.y + _btnImprove.GetComponent<RectTransform>().rect.height / 2f;
				keysY = contentBottom - (contentBottom - num) / 2f;
			}
			_lblImprove.transform.SetLocalX(0f);
			_lblImprove.transform.SetLocalY(-294f);
			if (_keys.gameObject.activeSelf)
			{
				_keys.transform.SetLocalY(keysY);
				_textLabel.gameObject.SetActive(false);
			}
			else
			{
				_textLabel.gameObject.SetActive(true);
			}
		}

		private void InitImproveButton()
		{
			_btnImprove.gameObject.SetActive(true);
			_btnImprove.SetAlias(improveButtonAlias);
			if (perkState == ProfilePerk.ProfilePerkState.PERK_LOCK || perkState == ProfilePerk.ProfilePerkState.PERK_SELECTED || perkState == ProfilePerk.ProfilePerkState.PERK_UNAVAILABLE)
			{
				_btnImprove.gameObject.SetActive(false);
			}
			_btnImprove.transform.SetLocalX(0f);
			_btnImprove.transform.SetLocalY(-294f);
		}

		private void InitKeyIcons()
		{
			foreach (Transform item in _keys.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
				_keys.gameObject.SetActive(false);
			}
			if (infoAnimation == null)
			{
				return;
			}
			_keys.gameObject.SetActive(true);
			float num = 0f;
			KeyData keyData = infoAnimation.GetFirstKeysCondition().RequiredKeys;
			for (int i = 0; i < keyData.AdditionalKeys.Count; i++)
			{
				ResolutionImage keyIcon = GetKeyIcon(keyData.AdditionalKeys[i]);
				if (keyIcon != null)
				{
					keyIcon.transform.SetParent(_keys.transform, false);
					keyIcon.transform.SetLocalX(num);
					num += 110f;
				}
			}
			if (keyData.AdditionalKeys.Count > 0)
			{
				GameObject gameObject = new GameObject("KeyIcon");
				ResolutionImage resolutionImage = gameObject.AddComponent<ResolutionImage>();
				resolutionImage.set_SpriteName("ComboButtons.icon_plus");
				resolutionImage.SetNativeSize();
				resolutionImage.transform.SetParent(_keys.transform, false);
				resolutionImage.transform.SetLocalX(num);
				num += 110f;
			}
			for (int j = 0; j < keyData.StarterKeys.Count; j++)
			{
				ResolutionImage keyIcon2 = GetKeyIcon(keyData.StarterKeys[j]);
				if (keyIcon2 != null)
				{
					keyIcon2.transform.SetParent(_keys.transform, false);
					keyIcon2.transform.SetLocalX(num);
					num += 110f;
				}
			}
			_keys.transform.SetLocalX((0f - (num - 110f)) / 2f);
		}

		public static ResolutionImage GetKeyIcon(int keyId)
		{
			ResolutionImage resolutionImage = null;
			string spriteName = string.Empty;
			float num = 0f;
			switch (keyId)
			{
			case 5:
				spriteName = "ComboButtons.icon_left";
				num = 270f;
				break;
			case 10:
				spriteName = "ComboButtons.icon_kick";
				break;
			case 7:
				spriteName = "ComboButtons.icon_left";
				break;
			case 9:
				spriteName = "ComboButtons.icon_punch";
				break;
			case 3:
				spriteName = "ComboButtons.icon_left";
				num = 180f;
				break;
			case 1:
				spriteName = "ComboButtons.icon_left";
				num = 90f;
				break;
			case 8:
				spriteName = "ComboButtons.icon_left";
				num = 45f;
				break;
			case 2:
				spriteName = "ComboButtons.icon_left";
				num = 135f;
				break;
			case 6:
				spriteName = "ComboButtons.icon_left";
				num = 315f;
				break;
			case 4:
				spriteName = "ComboButtons.icon_left";
				num = 225f;
				break;
			default:
				GameLog.Error("PerkContent::getKeyIcon - unknown type: %i", (FightCID)keyId);
				break;
			}
			GameObject gameObject = new GameObject("KeyIcon");
			resolutionImage = gameObject.AddComponent<ResolutionImage>();
			resolutionImage.set_SpriteName(spriteName);
			resolutionImage.SetNativeSize();
			resolutionImage.raycastTarget = false;
			if (resolutionImage != null)
			{
				resolutionImage.transform.localEulerAngles = new Vector3(0f, 0f, 0f - num);
			}
			return resolutionImage;
		}

		private void OnImproveClicked()
		{
			if (improveCallback != null)
			{
				improveCallback(null);
			}
		}
	}
}
