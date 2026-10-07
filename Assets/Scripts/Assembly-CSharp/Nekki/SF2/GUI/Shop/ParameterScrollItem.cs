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

		public void Init(string name, string ADONPNOBBDE, int value, int OKEFHDDPMEC, bool EIAKNKDEEKA, string MMOBJGKHPNA = null)
		{
			InitVariables(name, EIAKNKDEEKA, MMOBJGKHPNA);
			if (_progressBar != null)
			{
				_progressBar.Init();
			}
			if (_icon != null)
			{
				_icon.set_SpriteName(atlasName + ADONPNOBBDE);
			}
			SetValue(value, OKEFHDDPMEC);
		}

		public void SetValue(int value, int OKEFHDDPMEC, float _Duration = 0f)
		{
			SetTextValue(value, OKEFHDDPMEC, _Duration);
			SetProgressBarValue(value, OKEFHDDPMEC, _Duration);
		}

		public void SetTextValue(int value, int OKEFHDDPMEC, float _Duration = 0f)
		{
			TweenValueText(value, _Duration);
			int bAINMLLIKOL = OKEFHDDPMEC - value;
			TweenAdditionalValue(bAINMLLIKOL, _Duration);
		}

		protected void TweenValueText(int value, float _Duration)
		{
			if (!(_value == null))
			{
				KillTween(ref valueTween);
				valueTween = DOTween.To(() => displayedValue, (int DHDMNHCIPEH) =>
				{
					displayedValue = DHDMNHCIPEH;
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

		public void SetProgressBarValue(int MCOIPKLENOC, int OKEFHDDPMEC, float _Duration = 0f)
		{
			if (OKEFHDDPMEC < MCOIPKLENOC)
			{
				SetBarSegment(OKEFHDDPMEC, BarColor.ORANGE, _Duration);
				SetBarSegment(MCOIPKLENOC, BarColor.RED, _Duration);
				SetBarSegment(OKEFHDDPMEC, BarColor.GREEN, _Duration);
			}
			else
			{
				SetBarSegment(MCOIPKLENOC, BarColor.ORANGE, _Duration);
				SetBarSegment(OKEFHDDPMEC, BarColor.GREEN, _Duration);
				SetBarSegment(MCOIPKLENOC, BarColor.RED, _Duration);
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

		private void InitVariables(string CEELFMIPAII, bool EIAKNKDEEKA, string MMOBJGKHPNA = null)
		{
			attributeName = CEELFMIPAII;
			barScaleName = MMOBJGKHPNA;
			if (string.IsNullOrEmpty(barScaleName))
			{
				WarriorAttribute bCNOAOPGAEI = GameUtils.WarriorAttributeList.GetAttribute(attributeName);
				barScaleName = ((bCNOAOPGAEI == null) ? attributeName : bCNOAOPGAEI.BarScale);
			}
			if (string.IsNullOrEmpty(barScaleName))
			{
				lowerLimit = -1f;
				upperLimit = -1f;
				levelMultiplier = 1f;
				levelShift = 0;
				return;
			}
			BarScale bABKPEHINKF = GameUtils.BarScaleTable.GetScaleByName(barScaleName);
			if (bABKPEHINKF == null)
			{
				GameLog.Error("Needed barScale not exist.");
				return;
			}
			Limit pEKGEPHFCMN = ((!EIAKNKDEEKA) ? bABKPEHINKF.GetAttributeLimitForLevel(ListSF.GetRoster().GetLevel()) : bABKPEHINKF.GetItemLimitForLevel(ListSF.GetRoster().GetLevel()));
			if (pEKGEPHFCMN == null)
			{
				pEKGEPHFCMN = ((!EIAKNKDEEKA) ? bABKPEHINKF.GetDefaultAttributeLimit() : bABKPEHINKF.GetDefaultItemLimit());
			}
			lowerLimit = 0f;
			upperLimit = 0f;
			if (pEKGEPHFCMN != null)
			{
				lowerLimit = pEKGEPHFCMN.LeftLimit;
				upperLimit = pEKGEPHFCMN.RightLimit;
				levelMultiplier = pEKGEPHFCMN.LevelMultiplier;
				levelShift = pEKGEPHFCMN.Shift;
			}
			_power = ((!(bABKPEHINKF.Power < 0f)) ? bABKPEHINKF.Power : defaultPower);
			minPercent = ((!(bABKPEHINKF.MinPower < 0f)) ? bABKPEHINKF.MinPower : defaultMinPercent);
			_type = ((!string.IsNullOrEmpty(bABKPEHINKF.Type)) ? bABKPEHINKF.Type : linearTypeName);
		}

		protected virtual void SetBarSegment(int value, BarColor index = BarColor.ORANGE, float _Duration = 0f)
		{
			float oKEFHDDPMEC = GetPercentFromValue(value);
			if (_progressBar != null)
			{
				_progressBar.SetValue(oKEFHDDPMEC, (int)index, _Duration);
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
				float bGJPLNFFEOB = GameUtils.DamageDoublingRange;
				num = Mathf.Pow(2f, (value - rightLimit) * _power / bGJPLNFFEOB);
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
