using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public static class BasicGUI
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static KeyValuePair<int, int> _buttonWidth;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static KeyValuePair<int, int> _hintWidth;

	private static int _defaultButtonCenterWidth = 0;

	private static float _hintTimeout = 0f;

	private static int _arrowFlashingFrames = 0;

	private static float _creditsScrollSpeed = 0f;

	private static bool _ShowMenuTime = false;

	private static int _defaultCounterRollTime = 0;

	private static Dictionary<int, int> _CounterRollTime = new Dictionary<int, int>();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static float _notificationReadTime;

	public static KeyValuePair<int, int> ButtonWidth
	{
		get
		{
			return GetButtonWidth();
		}
		private set
		{
			SetButtonWidth(value);
		}
	}

	public static KeyValuePair<int, int> HintWidth
	{
		get
		{
			return GetHintWidth();
		}
		private set
		{
			SetHintWidth(value);
		}
	}

	public static int DefaultButtonCenterWidth
	{
		get
		{
			return GetDefaultButtonCenterWidth();
		}
	}

	public static float HintTimeout
	{
		get
		{
			return GetHintTimeout();
		}
	}

	public static int ArrowFlashingFrames
	{
		get
		{
			return GetArrowFlashingFrames();
		}
	}

	public static float CreditsScrollSpeed
	{
		get
		{
			return GetCreditsScrollSpeed();
		}
	}

	public static bool ShowMenuTime
	{
		get
		{
			return GetShowMenuTime();
		}
	}

	public static int DefaultCounterRollTime
	{
		get
		{
			return GetDefaultCounterRollTime();
		}
	}

	public static Dictionary<int, int> CounterRollTime
	{
		get
		{
			return GetCounterRollTime();
		}
	}

	public static float NotificationDefaultReadTime
	{
		get
		{
			return GetNotificationDefaultReadTime();
		}
		private set
		{
			set_NotificationReadTime(value);
		}
	}

	public static void Parse(XmlNode node)
	{
		SetButtonWidth(node["ButtonWidth"].ParseMinMax(242, 342));
		SetHintWidth(node["HintWidth"].ParseMinMax(242, 342));
		_defaultButtonCenterWidth = node["DefaultButtonCenterWidth"].FirstAttribute().ParseInt();
		_hintTimeout = node["HintTimeout"].FirstAttribute().ParseFloat(1f);
		_arrowFlashingFrames = node["ArrowFlashingFrames"].FirstAttribute().ParseInt(120);
		_creditsScrollSpeed = node["CreditsScrollSpeed"].FirstAttribute().ParseFloat(2f);
		_ShowMenuTime = node["ShowMenuTime"].FirstAttribute().ParseBool();
		set_NotificationReadTime(node["NotificationDlgDefaultReadTime"].FirstAttribute().ParseFloat());
		XmlNode xmlNode = node["CurrencyCounterRollTime"];
		if (xmlNode == null)
		{
			return;
		}
		_CounterRollTime.Clear();
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			if (childNode.Name.Equals("DefaultRollTime"))
			{
				_defaultCounterRollTime = childNode.LastAttribute().ParseInt(120);
				continue;
			}
			int key = childNode.FirstAttribute().ParseInt();
			int value = childNode.LastAttribute().ParseInt();
			_CounterRollTime[key] = value;
		}
	}

	public static KeyValuePair<int, int> GetButtonWidth()
	{
		return _buttonWidth;
	}

	private static void SetButtonWidth(KeyValuePair<int, int> value)
	{
		_buttonWidth = value;
	}

	public static KeyValuePair<int, int> GetHintWidth()
	{
		return _hintWidth;
	}

	private static void SetHintWidth(KeyValuePair<int, int> value)
	{
		_hintWidth = value;
	}

	public static int GetDefaultButtonCenterWidth()
	{
		return _defaultButtonCenterWidth;
	}

	public static float GetHintTimeout()
	{
		return _hintTimeout;
	}

	public static int GetArrowFlashingFrames()
	{
		return _arrowFlashingFrames;
	}

	public static float GetCreditsScrollSpeed()
	{
		return GetCreditsScrollSpeed();
	}

	public static bool GetShowMenuTime()
	{
		return GetShowMenuTime();
	}

	public static int GetDefaultCounterRollTime()
	{
		return _defaultCounterRollTime;
	}

	public static Dictionary<int, int> GetCounterRollTime()
	{
		return _CounterRollTime;
	}

	public static float GetNotificationDefaultReadTime()
	{
		return _notificationReadTime;
	}

	private static void set_NotificationReadTime(float value)
	{
		_notificationReadTime = value;
	}
}
