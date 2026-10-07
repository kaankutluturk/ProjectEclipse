using System.Collections.Generic;
using UnityEngine;

namespace Nekki.SF2.GUI.Fight
{
	public class StylePanel : MonoBehaviour
	{
		[SerializeField]
		private StyleBar _styleBar;

		private ResolutionImage _styleName;

		private Dictionary<InfoAnimation, int> animationUseCounts = new Dictionary<InfoAnimation, int>();

		private int maxStyleStrip;

		private int currentStyleStrip;

		private const float StripSize = 25f;

		private const float UnusedOffset = 0f;

		public FightStatistics.FightStyle MaxStyleLevel
		{
			get
			{
				return get_MaximumStyleStrip();
			}
		}

		public int CurrentStripIndex
		{
			get
			{
				return get_CurrentStyleStrip();
			}
		}

		public string CurrentStyleLabel
		{
			get
			{
				return get_CurrentStyleName();
			}
		}

		public ResolutionImage get_StyleName()
		{
			return _styleName;
		}

		public RectTransform get_rectTransform()
		{
			return (RectTransform)base.transform;
		}

		public FightStatistics.FightStyle get_MaximumStyleStrip()
		{
			return (FightStatistics.FightStyle)maxStyleStrip;
		}

		public int get_CurrentStyleStrip()
		{
			return currentStyleStrip;
		}

		public string get_CurrentStyleName()
		{
			Style mHOJFHKHIIL = GameUtils.StyleLevelTable.GetStyle(currentStyleStrip);
			return (mHOJFHKHIIL == null) ? string.Empty : mHOJFHKHIIL.Name;
		}

		public void Init(ResolutionImage ECJDAIHCDBA)
		{
			_styleName = ECJDAIHCDBA;
			_styleBar.Init();
			GameUtils.StyleLevelTable.Styles.ForEach((Style DHDMNHCIPEH) =>
			{
				_styleBar.AddStrip(DHDMNHCIPEH.BarImage, string.Empty, 25f, DHDMNHCIPEH.Name);
			});
			_styleBar.SetValue(0f, 0);
		}

		public void UpdateStyleLabel()
		{
			if (!(_styleName == null))
			{
				Style mHOJFHKHIIL = GameUtils.StyleLevelTable.GetStyle(currentStyleStrip);
				if (mHOJFHKHIIL != null && !mHOJFHKHIIL.TextImage.Equals(string.Empty))
				{
					_styleName.gameObject.SetActive(true);
					_styleName.set_SpriteName(mHOJFHKHIIL.TextImage);
					_styleName.SetNativeSize();
				}
			}
		}

		public void SetCurrentStyleStrip(int index)
		{
			currentStyleStrip = index;
			maxStyleStrip = Mathf.Max(maxStyleStrip, currentStyleStrip);
			UpdateStyleLabel();
		}

		public float GetStyleIncrease(InfoAnimation IFPDGKDKJOD)
		{
			if (IFPDGKDKJOD == null)
			{
				return 0f;
			}
			int num = 0;
			if (animationUseCounts.ContainsKey(IFPDGKDKJOD))
			{
				num = ++animationUseCounts[IFPDGKDKJOD];
			}
			if (num == 0)
			{
				animationUseCounts[IFPDGKDKJOD] = num;
			}
			float eNKMAPMCMCM = GameUtils.StyleLevelTable.StylePerHit;
			float num2 = GameUtils.StyleLevelTable.GetStyleMultiplier(currentStyleStrip);
			float pKOFNMPOMKM = GameUtils.StyleLevelTable.Penalty;
			return eNKMAPMCMCM * num2 * Mathf.Pow(pKOFNMPOMKM, -num) * IFPDGKDKJOD.StyleFactor;
		}

		public void UpdateStyle(InfoAnimation IFPDGKDKJOD)
		{
			float styleIncrease = GetStyleIncrease(IFPDGKDKJOD);
			float value = _styleBar.GetValue(currentStyleStrip);
			float bAINMLLIKOL = value + styleIncrease;
			IncreaseStyleStripByValue(bAINMLLIKOL);
		}

		public void IncreaseStyleStripByValue(float value)
		{
			int num = currentStyleStrip + (int)value;
			value %= 1f;
			if (num >= GameUtils.StyleLevelTable.Styles.Count)
			{
				num = GameUtils.StyleLevelTable.Styles.Count - 1;
				value = 1f;
			}
			if (num != currentStyleStrip)
			{
				for (int num2 = num - 1; num2 >= 0; num2--)
				{
					_styleBar.SetValue(1f, 0, num2);
				}
				SetCurrentStyleStrip(num);
			}
			SetStyleValue(value);
		}

		public void SetStyleValue(float value)
		{
			_styleBar.SetValue(value, 0, currentStyleStrip);
		}

		public void SetAllStyleValue(float value)
		{
			_styleBar.SetValue(value, 0);
		}

		public float GetStyleValue(int KNECPEAFMIM)
		{
			return _styleBar.GetValue(KNECPEAFMIM);
		}

		public float GetStyleValue()
		{
			return _styleBar.GetValue(currentStyleStrip);
		}

		public void ClearStyleIncrease()
		{
			animationUseCounts.Clear();
		}

		public void ResetStyle()
		{
			SetCurrentStyleStrip(0);
			SetAllStyleValue(0f);
			ClearStyleIncrease();
			if (_styleName != null)
			{
				_styleName.gameObject.SetActive(false);
			}
		}

		public void Render()
		{
			float num = GameUtils.StyleLevelTable.DecreaseSpeed / 60f;
			num /= (float)GameUtils.GetSlowMode();
			float styleValue = Mathf.Max(0f, _styleBar.GetValue(currentStyleStrip) - num);
			SetStyleValue(styleValue);
		}
	}
}
