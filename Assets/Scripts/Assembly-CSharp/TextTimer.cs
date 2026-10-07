using System;
using Nekki.SF2.GUI;
using UnityEngine;

public class TextTimer : global::EventDispatcher<object>
{
	public class TimerDataStruct
	{
		public object Data;

		private TextTimer _timer;

		public long Time
		{
			get
			{
				return GetTime();
			}
			set
			{
				SetTime(value);
			}
		}

		public TimerDataStruct(TextTimer timer, object data = null)
		{
			Data = data;
			_timer = timer;
		}

		public long GetTime()
		{
			return _timer.Time;
		}

		public void SetTime(long value)
		{
			_timer.Time = value;
		}
	}

	public string Delimiter = ":";

	public string DaysString = "d";

	public string HoursString = string.Empty;

	public bool IsSeconds = true;

	public bool IsMinutes = true;

	public bool IsHours = true;

	public bool IsDays;

	public bool UseDaysDelimiter = true;

	public bool IsSecondsZero = true;

	public bool IsMinutesZero = true;

	public bool IsHoursZero = true;

	public bool IsDaysZero = true;

	private TimerDataStruct _timerData;

	public Color Color = Color.black;

	private object _data;

	public long Time;

	public Action<object> Delegate;

	private TimerLabel _timerLabel;

	public TextTimer(Action<object> callback = null)
	{
		Delegate = callback;
		_timerData = new TimerDataStruct(this, _data);
	}

	public object GetData()
	{
		return _data;
	}

	public void set_Data(object value)
	{
		_data = value;
		_timerData.Data = _data;
	}

	public TimerLabel GetLabel()
	{
		return _timerLabel;
	}

	public void set_Label(TimerLabel value)
	{
		_timerLabel = value;
		if (_timerLabel != null)
		{
			_timerLabel.set_CurrentTime(Time);
		}
	}

	public void Refresh()
	{
		if (Delegate != null)
		{
			Delegate(_timerData);
		}
		if (_timerLabel != null)
		{
			_timerLabel.set_CurrentTime(Time);
		}
	}
}
