using System;
using System.Timers;

public class CThreadTimer
{
	public delegate void TimerElapsedCallback();

	private TimerElapsedCallback elapsedCallback;

	private Timer _myTimer;

	private bool _completed;

	public bool IsCompleted
	{
		get
		{
			return GetCompleted();
		}
	}

	public CThreadTimer(TimerElapsedCallback callback, float interval, bool autoReset)
	{
		elapsedCallback = (TimerElapsedCallback)Delegate.Combine(elapsedCallback, callback);
		_myTimer = new Timer();
		_myTimer.Elapsed += OnTimerElapsed;
		_myTimer.Interval = interval;
		if (autoReset)
		{
			this.StartTimer();
		}
	}

	public bool GetCompleted()
	{
		return _completed;
	}

	public void StartTimer()
	{
		_completed = false;
		_myTimer.Start();
	}

	public void StartTimer(float interval)
	{
		_completed = false;
		_myTimer.Interval = interval;
		_myTimer.Start();
	}

	public bool Pause()
	{
		if (!_completed)
		{
			_myTimer.Enabled = false;
		}
		return !_completed;
	}

	public bool Resume()
	{
		if (!_completed)
		{
			_myTimer.Enabled = true;
		}
		return !_completed;
	}

	private void OnTimerElapsed(object sender, ElapsedEventArgs eventArgs)
	{
		_completed = false;
		elapsedCallback();
		_myTimer.Dispose();
	}
}
