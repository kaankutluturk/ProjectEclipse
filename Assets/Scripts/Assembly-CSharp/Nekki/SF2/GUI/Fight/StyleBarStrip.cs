using UnityEngine.UI;

namespace Nekki.SF2.GUI.Fight
{
	public class StyleBarStrip : ResolutionImageSkew
	{
		private float currentValue;

		private float targetValue;

		private int _framesToEnd;

		public void Init(float value)
		{
			base.type = Type.Filled;
			base.fillMethod = FillMethod.Horizontal;
			base.fillAmount = value;
			currentValue = value;
			targetValue = value;
		}

		public void SetValue(float value, int frames)
		{
			targetValue = value;
			_framesToEnd = frames;
			if (frames <= 0)
			{
				currentValue = targetValue;
				base.fillAmount = currentValue;
			}
		}

		public void Render()
		{
			if (currentValue != targetValue)
			{
				if (_framesToEnd <= 0)
				{
					currentValue = targetValue;
					base.fillAmount = currentValue;
				}
				else
				{
					float num = currentValue - targetValue;
					float num2 = num / (float)_framesToEnd;
					currentValue -= num2;
					base.fillAmount = currentValue;
				}
				_framesToEnd--;
			}
		}
	}
}
