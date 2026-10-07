using System;
using System.Collections.Generic;
using System.Text;
using Nekki.Utils;
using UnityEngine;

namespace Nekki.SF2.GUI
{
	public class TimerLabel : LabelAlias
	{
		public bool IsSeconds = true;

		public bool IsMinutes = true;

		public bool IsHours = true;

		public bool IsDays;

		public bool IsSecondsZero = true;

		public bool IsMinutesZero = true;

		public bool IsHoursZero = true;

		public bool IsDaysZero = true;

		public bool UseDaysDelimiter = true;

		public int SegmentsDate = -1;

		public string Delimiter = ":";

		private string _delimiterAlias;

		public string DaysString = "d ";

		private string _daysStringAlias;

		public string HoursString;

		private string _hoursStringAlias;

		public string MinutesString;

		private string _minutesStringAlias;

		public string SecondsString;

		private string _secondsStringAlias;

		[SerializeField]
		private long _currentTime;

		public string DelimiterAliasKey
		{
			get
			{
				return get_DelimiterAlias();
			}
			set
			{
				set_DelimiterAlias(value);
			}
		}

		public string DaysAliasKey
		{
			get
			{
				return get_DaysStringAlias();
			}
			set
			{
				set_DaysStringAlias(value);
			}
		}

		public string HoursAliasKey
		{
			get
			{
				return get_HoursStringAlias();
			}
			set
			{
				set_HoursStringAlias(value);
			}
		}

		public string MinutesAliasKey
		{
			get
			{
				return get_MinutesStringAlias();
			}
			set
			{
				set_MinutesStringAlias(value);
			}
		}

		public string SecondsAliasKey
		{
			get
			{
				return get_SecondsStringAlias();
			}
			set
			{
				set_SecondsStringAlias(value);
			}
		}

		public long RemainingTime
		{
			get
			{
				return get_CurrentTime();
			}
			set
			{
				set_CurrentTime(value);
			}
		}

		public string get_DelimiterAlias()
		{
			return _delimiterAlias;
		}

		public void set_DelimiterAlias(string value)
		{
			_delimiterAlias = value;
			Delimiter = LocalizationManager.GetString(_delimiterAlias);
		}

		public string get_DaysStringAlias()
		{
			return _daysStringAlias;
		}

		public void set_DaysStringAlias(string value)
		{
			_daysStringAlias = value;
			DaysString = LocalizationManager.GetString(_daysStringAlias);
		}

		public string get_HoursStringAlias()
		{
			return _hoursStringAlias;
		}

		public void set_HoursStringAlias(string value)
		{
			_hoursStringAlias = value;
			HoursString = LocalizationManager.GetString(_hoursStringAlias);
		}

		public string get_MinutesStringAlias()
		{
			return _minutesStringAlias;
		}

		public void set_MinutesStringAlias(string value)
		{
			_minutesStringAlias = value;
			MinutesString = LocalizationManager.GetString(_minutesStringAlias);
		}

		public string get_SecondsStringAlias()
		{
			return _secondsStringAlias;
		}

		public void set_SecondsStringAlias(string value)
		{
			_secondsStringAlias = value;
			SecondsString = LocalizationManager.GetString(_secondsStringAlias);
		}

		public long get_CurrentTime()
		{
			return _currentTime;
		}

		public void set_CurrentTime(long value)
		{
			if (_currentTime != value)
			{
				_currentTime = value;
				UpdateText();
			}
		}

		protected override void Awake()
		{
			base.Awake();
			GlobalTimer.get_Instance().addEventListener(0, OnTimerTick);
			LocalizationManager.AddLanguageChangedHandler(OnLanguageChanged);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GlobalTimer.get_Instance().removeEventListener(0, OnTimerTick);
			LocalizationManager.RemoveLanguageChangedHandler(OnLanguageChanged);
		}

		private void OnLanguageChanged()
		{
			if (!string.IsNullOrEmpty(_daysStringAlias))
			{
				set_DaysStringAlias(_daysStringAlias);
			}
			if (!string.IsNullOrEmpty(_hoursStringAlias))
			{
				set_HoursStringAlias(_hoursStringAlias);
			}
			if (!string.IsNullOrEmpty(_minutesStringAlias))
			{
				set_MinutesStringAlias(_minutesStringAlias);
			}
			if (!string.IsNullOrEmpty(_secondsStringAlias))
			{
				set_SecondsStringAlias(_secondsStringAlias);
			}
			if (!string.IsNullOrEmpty(_delimiterAlias))
			{
				set_DelimiterAlias(_delimiterAlias);
			}
		}

		public void OnTimerTick(object data)
		{
			if (0 < _currentTime)
			{
				_currentTime--;
				UpdateText();
			}
		}

		private void UpdateText()
		{
			set_text(GetTimeString(_currentTime, IsSeconds, IsMinutes, IsHours, IsDays, Delimiter, DaysString, UseDaysDelimiter, IsSecondsZero, IsMinutesZero, IsHoursZero, IsDaysZero, HoursString, MinutesString, SecondsString, SegmentsDate));
		}

		public static string GetTimeString(long time, bool showSeconds = true, bool showMinutes = true, bool showHours = true, bool showDays = false, string delimiter = ":", string daysSuffix = "", bool useDaysDelimiter = true, bool padSeconds = true, bool padMinutes = true, bool padHours = true, bool padDays = true, string hoursSuffix = "", string minutesSuffix = "", string secondsSuffix = "", int maxSegments = 3)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(time);
			List<object> list = new List<object>();
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			bool flag = showDays && timeSpan.TotalDays >= 1.0 && maxSegments > num;
			if (flag)
			{
				num++;
			}
			bool flag2 = showHours && timeSpan.TotalHours >= 1.0 && maxSegments > num;
			if (flag2)
			{
				num++;
			}
			bool flag3 = showMinutes && maxSegments > num;
			if (flag3)
			{
				num++;
			}
			bool flag4 = showSeconds && maxSegments > num;
			int num2 = 0;
			if (flag)
			{
				list.Add(timeSpan.Days);
				stringBuilder.Append('{');
				stringBuilder.Append(num2++);
				stringBuilder.Append((!padDays) ? ":D}" : ":D2}");
				stringBuilder.Append(daysSuffix);
				if (useDaysDelimiter && (flag2 || flag3 || flag4))
				{
					stringBuilder.Append(delimiter);
				}
			}
			if (flag2)
			{
				list.Add(timeSpan.Hours);
				stringBuilder.Append('{');
				stringBuilder.Append(num2++);
				stringBuilder.Append((!padHours) ? ":D}" : ":D2}");
				stringBuilder.Append(hoursSuffix);
				if (flag3 || flag4)
				{
					stringBuilder.Append(delimiter);
				}
			}
			if (flag3)
			{
				list.Add(timeSpan.Minutes);
				stringBuilder.Append('{');
				stringBuilder.Append(num2++);
				stringBuilder.Append((!padMinutes) ? ":D}" : ":D2}");
				stringBuilder.Append(minutesSuffix);
				if (flag4)
				{
					stringBuilder.Append(delimiter);
				}
			}
			if (flag4)
			{
				list.Add(timeSpan.Seconds);
				stringBuilder.Append('{');
				stringBuilder.Append(num2++);
				stringBuilder.Append((!padSeconds) ? ":D}" : ":D2}");
				stringBuilder.Append(secondsSuffix);
			}
			return string.Format(stringBuilder.ToString(), list.ToArray());
		}
	}
}
