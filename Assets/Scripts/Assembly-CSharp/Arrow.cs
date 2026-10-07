using Nekki.SF2.GUI;
using UnityEngine;

public class Arrow : MonoBehaviour
{
	[SerializeField]
	private ResolutionImage _arrowImg;

	private const float DefaultAmplitude = 24f;

	private const float DefaultStep = 0.8f;

	protected float _amplitude;

	protected float _step;

	protected int _maxSteps;

	protected int _stepCount;

	protected bool _animationUp;

	public void Init(float amplitude = 24f)
	{
		_amplitude = amplitude;
		_step = 0.8f;
		_maxSteps = (int)(_amplitude / 0.8f);
	}

	private void Update()
	{
		if (_arrowImg == null)
		{
			return;
		}
		_arrowImg.transform.SetLocalY(_arrowImg.transform.localPosition.y - ((!_animationUp) ? (0f - _step) : _step));
		if (_animationUp)
		{
			if (_stepCount < _maxSteps)
			{
				_stepCount++;
			}
			else
			{
				_animationUp = false;
			}
		}
		else if (_stepCount > 0)
		{
			_stepCount--;
		}
		else
		{
			_animationUp = true;
		}
	}
}
