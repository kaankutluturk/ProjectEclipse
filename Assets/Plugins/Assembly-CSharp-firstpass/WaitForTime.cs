using System.Diagnostics;
using UnityEngine;

public class WaitForTime : CustomYieldInstruction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float timeLeft;

	public float TimeLeft
	{
		get
		{
			return GetTimeLeft();
		}
		private set
		{
			SetTimeLeft(value);
		}
	}

	public WaitForTime(float duration)
	{
		SetTimeLeft(duration);
	}

	public float GetTimeLeft()
	{
		return timeLeft;
	}

	private void SetTimeLeft(float value)
	{
		timeLeft = value;
	}

	public override bool keepWaiting
	{
		get
		{
			if (!CoroutineManager.get_Current().get_IsPaused())
			{
				SetTimeLeft(GetTimeLeft() - Time.deltaTime);
			}
			return GetTimeLeft() >= 1E-07f;
		}
	}
}
