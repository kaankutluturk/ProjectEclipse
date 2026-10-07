using System.Collections.Generic;

public class Interpolator
{
	private List<IntervalSet> intervals = new List<IntervalSet>();

	private float innerTime;

	private int nowInterval;

	private void RecalculateCoefficients()
	{
		for (int i = 0; i < intervals.Count; i++)
		{
			float startValue = intervals[i].value;
			float num = ((i != intervals.Count - 1) ? intervals[i + 1].value : intervals[0].value);
			if (intervals[i].Duration == 0f)
			{
				intervals[i].SlopeOrShift = (intervals[i].BaseValue = 0f);
				break;
			}
			if (intervals[i].Acceleration == 0f)
			{
				intervals[i].SlopeOrShift = (num - startValue) / intervals[i].Duration;
				intervals[i].BaseValue = startValue;
			}
			else
			{
				intervals[i].SlopeOrShift = (num - startValue - intervals[i].Acceleration * intervals[i].Duration * intervals[i].Duration) / (2f * intervals[i].Acceleration * intervals[i].Duration);
				intervals[i].BaseValue = startValue - intervals[i].Acceleration * intervals[i].SlopeOrShift * intervals[i].SlopeOrShift;
			}
		}
	}

	public bool AddInterval(float duration, float value, float acceleration)
	{
		if (duration < 0f)
		{
			return false;
		}
		IntervalSet interval = new IntervalSet();
		interval.Duration = duration;
		interval.value = value;
		interval.Acceleration = acceleration;
		intervals.Add(interval);
		RecalculateCoefficients();
		return true;
	}

	public bool AdvanceTime(float deltaTime)
	{
		if (deltaTime < 0f)
		{
			return false;
		}
		if (!HasIntervals())
		{
			innerTime += deltaTime;
			return true;
		}
		innerTime += deltaTime;
		while (innerTime > intervals[nowInterval].Duration)
		{
			if (innerTime > intervals[nowInterval].Duration)
			{
				innerTime -= intervals[nowInterval].Duration;
				nowInterval++;
			}
			if (nowInterval >= intervals.Count && intervals.Count > 0)
			{
				nowInterval = 0;
			}
		}
		return true;
	}

	public float GetCurrentValue()
	{
		if (intervals.Count == 0)
		{
			return 0f;
		}
		float num = ((intervals[nowInterval].Acceleration == 0f) ? (intervals[nowInterval].SlopeOrShift * innerTime) : (intervals[nowInterval].Acceleration * (innerTime + intervals[nowInterval].SlopeOrShift) * (innerTime + intervals[nowInterval].SlopeOrShift)));
		return num + intervals[nowInterval].BaseValue;
	}

	public bool HasIntervals()
	{
		if (intervals.Count > 0)
		{
			return true;
		}
		return false;
	}
}
