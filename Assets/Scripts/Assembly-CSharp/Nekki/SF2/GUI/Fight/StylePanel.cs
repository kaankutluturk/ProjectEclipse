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
			Style style = GameUtils.StyleLevelTable.GetStyle(currentStyleStrip);
			return (style == null) ? string.Empty : style.Name;
		}

		public void Init(ResolutionImage styleNameImage)
		{
			_styleName = styleNameImage;
			_styleBar.Init();
			GameUtils.StyleLevelTable.Styles.ForEach((Style style) =>
			{
				_styleBar.AddStrip(style.BarImage, string.Empty, 25f, style.Name);
			});
			_styleBar.SetValue(0f, 0);
		}

		public void UpdateStyleLabel()
		{
			if (!(_styleName == null))
			{
				Style style = GameUtils.StyleLevelTable.GetStyle(currentStyleStrip);
				if (style != null && !style.TextImage.Equals(string.Empty))
				{
					_styleName.gameObject.SetActive(true);
					_styleName.set_SpriteName(style.TextImage);
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

		public float GetStyleIncrease(InfoAnimation animation)
		{
			if (animation == null)
			{
				return 0f;
			}
			int num = 0;
			if (animationUseCounts.ContainsKey(animation))
			{
				num = ++animationUseCounts[animation];
			}
			if (num == 0)
			{
				animationUseCounts[animation] = num;
			}
			float stylePerHit = GameUtils.StyleLevelTable.StylePerHit;
			float num2 = GameUtils.StyleLevelTable.GetStyleMultiplier(currentStyleStrip);
			float penalty = GameUtils.StyleLevelTable.Penalty;
			return stylePerHit * num2 * Mathf.Pow(penalty, -num) * animation.StyleFactor;
		}

		public void UpdateStyle(InfoAnimation animation)
		{
			float styleIncrease = GetStyleIncrease(animation);
			float value = _styleBar.GetValue(currentStyleStrip);
			float increaseAmount = value + styleIncrease;
			IncreaseStyleStripByValue(increaseAmount);
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

		public float GetStyleValue(int stripIndex)
		{
			return _styleBar.GetValue(stripIndex);
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
