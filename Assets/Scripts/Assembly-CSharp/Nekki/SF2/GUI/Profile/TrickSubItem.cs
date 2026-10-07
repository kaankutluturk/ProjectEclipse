using System.Collections.Generic;
using UnityEngine;

namespace Nekki.SF2.GUI.Profile
{
	public class TrickSubItem : SubItem
	{
		public enum TrickSubItemEvent
		{
			onTrickShow = 12
		}

		private const int KEY_ICON_SPACING = 110;

		private const int KEYS_OFFSET_X = 100;

		private const int KEY_ICON_WIDTH = 110;

		protected Trick trick;

		private InfoAnimation infoAnimation;

		private TrickInfo trickInfo;

		private bool isNew;

		[SerializeField]
		private LabelAlias _keysDescription;

		[SerializeField]
		private GameObject _keys;

		public void Init(string iconName, Trick trickData, int buttonId)
		{
			Init(buttonId);
			_keys.gameObject.SetActive(false);
			_keysDescription.gameObject.SetActive(false);
			trick = trickData;
			spriteName = iconName;
			iconMaxOpacity = ProfileGUI.PerkOpacity.Max / 255f;
			iconMinOpacity = ProfileGUI.PerkOpacity.Min / 255f;
			isNew = trick != null && trick.IsNew;
			infoAnimation = trickData.Animation;
			trickInfo = new TrickInfo(trick.DisplayName, trickData.Animation, GetAttackDamages(), OnShowCallback, trick.EffectDescription);
			Data = trickInfo;
			UpdateIcon();
			SetActive(true);
			InitKeyIcons();
			InitKeysDescription();
			UpdatePositions();
		}

		public Trick GetTrick()
		{
			return trick;
		}

		public void ShowTrick()
		{
			if (infoAnimation != null && infoAnimation.ShowInTricks)
			{
				CallEvent(12, this);
			}
		}

		public void UpdatePositions()
		{
			if (_keys.activeSelf)
			{
				float num = 100f;
				if (_icon != null)
				{
					num += _icon.transform.localPosition.x + _icon.rectTransform.rect.width / 2f;
				}
				_keys.transform.SetLocalX(num);
			}
			if (_keysDescription.gameObject.activeSelf)
			{
				float num2 = GetComponent<RectTransform>().rect.width / 2f;
				if (_icon != null)
				{
					num2 += _icon.transform.localPosition.x + _icon.rectTransform.rect.width / 2f;
				}
				num2 += 100f - _keysDescription.rectTransform.rect.width / 2f;
				_keysDescription.transform.SetLocalX(num2);
			}
			if (_keys.gameObject.activeSelf && _keysDescription.gameObject.activeSelf)
			{
				_keys.transform.SetLocalY(55f);
				_keysDescription.transform.SetLocalY(-55f);
			}
			else
			{
				_keys.transform.SetLocalY(0f);
				_keysDescription.transform.SetLocalY(0f);
			}
		}

		public override void UpdateState()
		{
			isNew = trick != null && trick.IsNew;
		}

		protected void InitKeyIcons()
		{
			foreach (Transform item in _keys.transform)
			{
				Object.Destroy(item.gameObject);
			}
			ConditionKeys conditionKeys = trick.Animation.GetFirstKeysCondition();
			if (conditionKeys == null)
			{
				return;
			}
			_keys.gameObject.SetActive(true);
			float num = 0f;
			KeyData keyData = conditionKeys.RequiredKeys;
			for (int i = 0; i < keyData.AdditionalKeys.Count; i++)
			{
				ResolutionImage keyIcon = PerkContent.GetKeyIcon(keyData.AdditionalKeys[i]);
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
				resolutionImage.raycastTarget = false;
				resolutionImage.transform.SetParent(_keys.transform, false);
				resolutionImage.transform.SetLocalX(num);
				num += 110f;
			}
			for (int j = 0; j < keyData.StarterKeys.Count; j++)
			{
				ResolutionImage keyIcon2 = PerkContent.GetKeyIcon(keyData.StarterKeys[j]);
				if (keyIcon2 != null)
				{
					keyIcon2.transform.SetParent(_keys.transform, false);
					keyIcon2.transform.SetLocalX(num);
					num += 110f;
				}
			}
		}

		protected void InitKeysDescription()
		{
			if (trick != null && !(trick.KeysDescription == string.Empty))
			{
				_keysDescription.gameObject.SetActive(true);
				_keysDescription.set_LabelFontSize(104);
				_keysDescription.color = Constants.DialogTextColor;
				_keysDescription.set_Alias(trick.KeysDescription);
			}
		}

		protected override void UpdateSelectedFlash()
		{
			base.UpdateSelectedFlash();
			if (isNew)
			{
				UpdateIconFlash();
			}
		}

		private List<float> GetAttackDamages()
		{
			List<float> list = new List<float>();
			if (infoAnimation != null)
			{
				List<IntervalAnimation> intervals = infoAnimation.MoveData.Intervals;
				for (int i = 0; i < intervals.Count; i++)
				{
					if (intervals[i].Type == IntervalAnimation.IntervalType.INTERVAL_ATTACK)
					{
						list.Add(((IntervalAttack)intervals[i]).GetDamage());
					}
				}
			}
			return list;
		}

		private void OnShowCallback(object data)
		{
			ShowTrick();
		}
	}
}
