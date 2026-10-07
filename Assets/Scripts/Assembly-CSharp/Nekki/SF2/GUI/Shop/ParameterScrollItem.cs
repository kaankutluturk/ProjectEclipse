using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Shop
{
	public class ParameterScrollItem : BaseScrollItem
	{
		public enum BarColor
		{
			ORANGE = 0,
			GREEN = 1,
			RED = 2
		}

		[SerializeField]
		protected ResolutionImage _icon;

		[SerializeField]
		protected LabelAlias _value;

		[SerializeField]
		protected LabelAlias _additionalValue;

		[SerializeField]
		protected ParameterProgressBar _progressBar;

		[SerializeField]
		protected LayoutElement _layoutElement;

		private string atlasName = "Attributes.";

		protected string linearTypeName = "Linear";

		protected string expTypeName = "Exp";

		protected float defaultPower;

		protected float defaultMinPercent;

		protected string attributeName;

		protected string barScaleName;

		protected string _type;

		protected float lowerLimit;

		protected float upperLimit;

		protected float levelMultiplier;

		protected float _power;

		protected float minPercent;

		protected int levelShift;

		protected int displayedValue;

		protected int displayedBonusValue;

		protected Tween valueTween;

		protected Tween additionalValueTween;

		public ResolutionImage IconImage
		{
			get
			{
				return get_Icon();
			}
		}

		public float ItemMinWidth
		{
			get
			{
				return get_MinWidth();
			}
			set
			{
				set_MinWidth(value);
			}
		}

		public float ItemMinHeight
		{
			get
			{
				return get_MinHeight();
			}
			set
			{
				set_MinHeight(value);
			}
		}

		public string SpriteAtlasName
		{
			get
			{
				return get_AtlasName();
			}
			set
			{
				set_AtlasName(value);
			}
		}

		public string ParameterName
		{
			get
			{
				return get_AttributeName();
			}
		}

		public float LeftLimitValue
		{
			get
			{
				return get_LeftLimit();
			}
		}

		public float RightLimitValue
		{
			get
			{
				return get_RightLimit();
			}
		}

		public ResolutionImage get_Icon()
		{
			return _icon;
		}

		public float get_MinWidth()
		{
			if (_layoutElement != null)
			{
				return _layoutElement.minWidth;
			}
			return 0f;
		}

		public void set_MinWidth(float value)
		{
			if (_layoutElement != null)
			{
				_layoutElement.minWidth = value;
			}
		}

		public float get_MinHeight()
		{
			if (_layoutElement != null)
			{
				return _layoutElement.minHeight;
			}
			return 0f;
		}

		public void set_MinHeight(float value)
		{
			if (_layoutElement != null)
			{
				_layoutElement.minHeight = value;
			}
		}

		public string get_AtlasName()
		{
			return atlasName;
		}

		public void set_AtlasName(string value)
		{
			atlasName = value;
		}

		public string get_AttributeName()
		{
			return attributeName;
		}

		public float get_LeftLimit()
		{
			if (lowerLimit >= 0f && upperLimit >= 0f)
			{
				return lowerLimit;
			}
			return (float)ListSF.GetRoster().GetLevel() * levelMultiplier + (float)levelShift;
		}

		public float get_RightLimit()
		{
			if (lowerLimit >= 0f && upperLimit >= 0f)
			{
				return upperLimit;
			}
			return (float)ListSF.GetRoster().GetLevel() * levelMultiplier + (float)levelShift;
		}

		public void Init(string name, string iconName, int value, int upgradedValue, bool isItemLimit, string barScale = null)
		{
			InitVariables(name, isItemLimit, barScale);
			if (_progressBar != null)
			{
				_progressBar.Init();
			}
			if (_icon != null)
			{
				_icon.set_SpriteName(atlasName + iconName);
			}
			SetValue(value, upgradedValue);
		}

		public void SetValue(int value, int upgradedValue, float _Duration = 0f)
		{
			SetTextValue(value, upgradedValue, _Duration);
			SetProgressBarValue(value, upgradedValue, _Duration);
		}

		public void SetTextValue(int value, int upgradedValue, float _Duration = 0f)
		{
			TweenValueText(value, _Duration);
			int additionalValue = upgradedValue - value;
			TweenAdditionalValue(additionalValue, _Duration);
		}

		protected void TweenValueText(int value, float _Duration)
		{
			if (!(_value == null))
			{
				KillTween(ref valueTween);
				valueTween = DOTween.To(() => displayedValue, (int tweenedValue) =>
				{
					displayedValue = tweenedValue;
					_value.set_text(displayedValue.ToString());
				}, value, _Duration);
			}
		}

		protected void TweenAdditionalValue(int value, float _Duration = 0f)
		{
			if (_additionalValue == null)
			{
				return;
			}
			KillTween(ref additionalValueTween);
			if (_Duration == 0f)
			{
				SetAdditionalValueText(value);
				return;
			}
			additionalValueTween = DOTween.To(() => displayedBonusValue, SetAdditionalValueText, value, _Duration);
		}

		protected void SetAdditionalValueText(int value)
		{
			displayedBonusValue = value;
			_additionalValue.set_text(string.Format((displayedBonusValue <= 0) ? "({0})" : "(+{0})", displayedBonusValue));
			_additionalValue.color = ((displayedBonusValue <= 0) ? Constants.NegativeValueColor : Constants.PositiveValueColor);
			bool active = displayedBonusValue != 0;
			_additionalValue.gameObject.SetActive(active);
		}

		protected void KillTween(ref Tween tween)
		{
			if (tween != null)
			{
				tween.Kill();
				tween = null;
			}
		}

		public void SetProgressBarValue(int baseValue, int upgradedValue, float _Duration = 0f)
		{
			if (upgradedValue < baseValue)
			{
				SetBarSegment(upgradedValue, BarColor.ORANGE, _Duration);
				SetBarSegment(baseValue, BarColor.RED, _Duration);
				SetBarSegment(upgradedValue, BarColor.GREEN, _Duration);
			}
			else
			{
				SetBarSegment(baseValue, BarColor.ORANGE, _Duration);
				SetBarSegment(upgradedValue, BarColor.GREEN, _Duration);
				SetBarSegment(baseValue, BarColor.RED, _Duration);
			}
		}

		public void ResetValue()
		{
			if (_value != null)
			{
				_value.set_text("0");
			}
			if (_additionalValue != null)
			{
				_additionalValue.set_text("0");
				_additionalValue.gameObject.SetActive(false);
			}
			if (_progressBar != null)
			{
				_progressBar.SetValue(0f);
			}
		}

		private void InitVariables(string newAttributeName, bool isItemLimit, string newBarScaleName = null)
		{
			attributeName = newAttributeName;
			barScaleName = newBarScaleName;
			if (string.IsNullOrEmpty(barScaleName))
			{
				WarriorAttribute warriorAttribute = GameUtils.WarriorAttributeList.GetAttribute(attributeName);
				barScaleName = ((warriorAttribute == null) ? attributeName : warriorAttribute.BarScale);
			}
			if (string.IsNullOrEmpty(barScaleName))
			{
				lowerLimit = -1f;
				upperLimit = -1f;
				levelMultiplier = 1f;
				levelShift = 0;
				return;
			}
			BarScale barScale = GameUtils.BarScaleTable.GetScaleByName(barScaleName);
			if (barScale == null)
			{
				GameLog.Error("Needed barScale not exist.");
				return;
			}
			Limit limit = ((!isItemLimit) ? barScale.GetAttributeLimitForLevel(ListSF.GetRoster().GetLevel()) : barScale.GetItemLimitForLevel(ListSF.GetRoster().GetLevel()));
			if (limit == null)
			{
				limit = ((!isItemLimit) ? barScale.GetDefaultAttributeLimit() : barScale.GetDefaultItemLimit());
			}
			lowerLimit = 0f;
			upperLimit = 0f;
			if (limit != null)
			{
				lowerLimit = limit.LeftLimit;
				upperLimit = limit.RightLimit;
				levelMultiplier = limit.LevelMultiplier;
				levelShift = limit.Shift;
			}
			_power = ((!(barScale.Power < 0f)) ? barScale.Power : defaultPower);
			minPercent = ((!(barScale.MinPower < 0f)) ? barScale.MinPower : defaultMinPercent);
			_type = ((!string.IsNullOrEmpty(barScale.Type)) ? barScale.Type : linearTypeName);
		}

		protected virtual void SetBarSegment(int value, BarColor index = BarColor.ORANGE, float _Duration = 0f)
		{
			float percent = GetPercentFromValue(value);
			if (_progressBar != null)
			{
				_progressBar.SetValue(percent, (int)index, _Duration);
			}
		}

		protected virtual float GetPercentFromValue(float value)
		{
			float rightLimit = get_RightLimit();
			float num;
			if (!string.IsNullOrEmpty(_type) && _type.Equals(linearTypeName))
			{
				num = Mathf.Pow(value / rightLimit, _power);
			}
			else
			{
				float damageDoublingRange = GameUtils.DamageDoublingRange;
				num = Mathf.Pow(2f, (value - rightLimit) * _power / damageDoublingRange);
			}
			if (num < 0f)
			{
				num = 0f;
			}
			else if (num > 1f)
			{
				num = 1f;
			}
			return Mathf.Max(num, minPercent);
		}
	}
}
