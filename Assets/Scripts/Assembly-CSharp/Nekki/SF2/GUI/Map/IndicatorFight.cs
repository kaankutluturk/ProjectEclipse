using UnityEngine;

namespace Nekki.SF2.GUI.Map
{
	public class IndicatorFight : SFMonoBehaviour<object>
	{
		public enum IndicatorState
		{
			IsOn = 0,
			IsOff = 1,
			IsLocked = 2
		}

		public const string FILE_INDICATOR_ATLAS = "MiscSprites";

		public const string FILE_INDICATOR_ON = "MiscSprites.indicatorOn";

		public const string FILE_INDICATOR_OFF = "MiscSprites.indicatorOff";

		public const string FILE_INDICATOR_LOCKED = "MiscSprites.indicatorLocked";

		private IndicatorState currentState = IndicatorState.IsOff;

		private float _scale = 1f;

		public IndicatorState State
		{
			get
			{
				return get_CurrentState();
			}
			set
			{
				set_CurrentState(value);
			}
		}

		public float IndicatorScale
		{
			get
			{
				return get_Scale();
			}
			set
			{
				set_Scale(value);
			}
		}

		public IndicatorState get_CurrentState()
		{
			return currentState;
		}

		public void set_CurrentState(IndicatorState value)
		{
			currentState = value;
			ResolutionImage component = GetComponent<ResolutionImage>();
			component.set_TexturePath("MiscSprites");
			switch (currentState)
			{
			case IndicatorState.IsOn:
				component.set_SpriteName("MiscSprites.indicatorOn");
				break;
			case IndicatorState.IsOff:
				component.set_SpriteName("MiscSprites.indicatorOff");
				break;
			case IndicatorState.IsLocked:
				component.set_SpriteName("MiscSprites.indicatorLocked");
				break;
			}
		}

		public float get_Scale()
		{
			return _scale;
		}

		public void set_Scale(float value)
		{
			_scale = value;
			ResolutionImage component = GetComponent<ResolutionImage>();
			component.transform.localScale = new Vector3(_scale, _scale);
		}
	}
}
