using DG.Tweening;
using Nekki.SF2.GUI;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
	[SerializeField]
	public ResolutionImage Background;

	[SerializeField]
	public ResolutionImage Stripe;

	private float minValue;

	private float maxValue = 1f;

	private float currentValue = 1f;

	private Tween _tween;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public virtual void Init()
	{
		Stripe.type = Image.Type.Filled;
		Stripe.fillMethod = Image.FillMethod.Horizontal;
	}

	public void SetValueBorders(float newMin, float newMax)
	{
		minValue = newMin;
		maxValue = newMax;
		UpdateFill();
	}

	public virtual void SetValue(float newValue, float _Duration = 0f)
	{
		newValue = Mathf.Clamp(newValue, minValue, maxValue);
		if (currentValue != newValue)
		{
			if (_tween != null)
			{
				_tween.Kill();
				_tween = null;
			}
			_tween = DOTween.To(() => currentValue, (float animatedValue) =>
			{
				currentValue = animatedValue;
				UpdateFill();
			}, newValue, _Duration);
			UpdateFill();
		}
	}

	public virtual float GetValue()
	{
		return currentValue;
	}

	private void UpdateFill()
	{
		Stripe.fillAmount = (currentValue - minValue) / (maxValue - minValue);
	}
}
