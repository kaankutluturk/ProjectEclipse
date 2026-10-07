using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ProgressButton : SFButton
{
	[SerializeField]
	private Image _picCircleProgressBar;

	[SerializeField]
	private Image _picComplete;

	private float percentage;

	public const float PercentageMin = 0f;

	public const float PercentageMax = 100f;

	private Tween _tween;

	// Eclipse: presentation-only easing of the circular fill. The gameplay percentage
	// (ILDDBLFPPPG) still changes immediately; only the drawn fill chases it.
	[System.NonSerialized]
	public float FillSmoothingRate;

	private float _displayedFill = -1f;

	public void Init()
	{
		if (_picComplete != null)
		{
			_picComplete.gameObject.SetActive(false);
		}
		ResetPercentage();
	}

	public float GetPercentage()
	{
		return percentage;
	}

	private void ApplyPercentage(float EJHLCDFHNPA)
	{
		percentage = Mathf.Clamp(EJHLCDFHNPA, 0f, 100f);
		if (_picComplete != null)
		{
			if (percentage == 0f)
			{
				_picComplete.gameObject.SetActive(false);
			}
			if (percentage == 100f)
			{
				_picComplete.gameObject.SetActive(true);
			}
		}
		if (_picCircleProgressBar != null)
		{
			if (FillSmoothingRate <= 0f || _displayedFill < 0f)
			{
				_displayedFill = percentage / 100f;
				_picCircleProgressBar.fillAmount = _displayedFill;
			}
		}
	}

	private void LateUpdate()
	{
		if (FillSmoothingRate <= 0f || _picCircleProgressBar == null)
		{
			return;
		}
		float target = percentage / 100f;
		if (_displayedFill == target)
		{
			return;
		}
		float t = 1f - Mathf.Exp(-FillSmoothingRate * Time.unscaledDeltaTime);
		_displayedFill = Mathf.Lerp(_displayedFill, target, t);
		if (Mathf.Abs(_displayedFill - target) < 0.002f)
		{
			_displayedFill = target;
		}
		_picCircleProgressBar.fillAmount = _displayedFill;
	}

	public void SetPercentage(float EJHLCDFHNPA, float BFJBKLCLIHP)
	{
		float num = BFJBKLCLIHP / 60f;
		KillTween();
		EJHLCDFHNPA = Mathf.Clamp(EJHLCDFHNPA, 0f, 100f);
		if (percentage == EJHLCDFHNPA)
		{
			return;
		}
		if (num == 0f)
		{
			ApplyPercentage(EJHLCDFHNPA);
			return;
		}
		_tween = DOTween.To(() => percentage, (float DHDMNHCIPEH) =>
		{
			ApplyPercentage(DHDMNHCIPEH);
		}, EJHLCDFHNPA, num);
	}

	public void AddPercentage(float FOIPKLDNGDL)
	{
		KillTween();
		ApplyPercentage(GetPercentage() + FOIPKLDNGDL);
	}

	public void ResetPercentage()
	{
		KillTween();
		ApplyPercentage(0f);
	}

	private void KillTween()
	{
		if (_tween != null)
		{
			_tween.Kill();
			_tween = null;
		}
	}
}
