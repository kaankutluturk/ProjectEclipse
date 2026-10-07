using System.Diagnostics;
using System.Xml;
using UnityEngine;

public static class PerkGUI
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static Vector2 _fadeFrames;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static Vector2 _pulseAccel;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static Vector2 _pulseFrames;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static Vector2 _spacing;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static float _pulseAmplitude;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static float _rowCapacity;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static float _expirationOpacity;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static float _stackShiftX;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static float _stackShiftY;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static float _fontScale;

	public static Vector2 FadeFrames
	{
		get
		{
			return GetFadeFrames();
		}
		private set
		{
			SetFadeFrames(value);
		}
	}

	public static Vector2 PulseAccel
	{
		get
		{
			return GetPulseAccel();
		}
		private set
		{
			SetPulseAccel(value);
		}
	}

	public static Vector2 PulseFrames
	{
		get
		{
			return GetPulseFrames();
		}
		private set
		{
			SetPulseFrames(value);
		}
	}

	public static Vector2 IconSpacing
	{
		get
		{
			return GetSpacing();
		}
		private set
		{
			set_Spacing(value);
		}
	}

	public static float PulseAmplitude
	{
		get
		{
			return GetPulseAmplitude();
		}
		private set
		{
			SetPulseAmplitude(value);
		}
	}

	public static float RowCapacity
	{
		get
		{
			return GetRowCapacity();
		}
		private set
		{
			SetRowCapacity(value);
		}
	}

	public static float ExpirationOpacity
	{
		get
		{
			return GetExpirationOpacity();
		}
		private set
		{
			SetExpirationOpacity(value);
		}
	}

	public static float StackShiftX
	{
		get
		{
			return GetStackShiftX();
		}
		private set
		{
			SetStackShiftX(value);
		}
	}

	public static float StackShiftY
	{
		get
		{
			return GetStackShiftY();
		}
		private set
		{
			SetStackShiftY(value);
		}
	}

	public static float FontScale
	{
		get
		{
			return GetFontScale();
		}
		private set
		{
			SetFontScale(value);
		}
	}

	public static Vector2 GetFadeFrames()
	{
		return _fadeFrames;
	}

	private static void SetFadeFrames(Vector2 value)
	{
		_fadeFrames = value;
	}

	public static Vector2 GetPulseAccel()
	{
		return _pulseAccel;
	}

	private static void SetPulseAccel(Vector2 value)
	{
		_pulseAccel = value;
	}

	public static Vector2 GetPulseFrames()
	{
		return _pulseFrames;
	}

	private static void SetPulseFrames(Vector2 value)
	{
		_pulseFrames = value;
	}

	public static Vector2 GetSpacing()
	{
		return _spacing;
	}

	private static void set_Spacing(Vector2 value)
	{
		_spacing = value;
	}

	public static float GetPulseAmplitude()
	{
		return _pulseAmplitude;
	}

	private static void SetPulseAmplitude(float value)
	{
		_pulseAmplitude = value;
	}

	public static float GetRowCapacity()
	{
		return _rowCapacity;
	}

	private static void SetRowCapacity(float value)
	{
		_rowCapacity = value;
	}

	public static float GetExpirationOpacity()
	{
		return _expirationOpacity;
	}

	private static void SetExpirationOpacity(float value)
	{
		_expirationOpacity = value;
	}

	public static float GetStackShiftX()
	{
		return _stackShiftX;
	}

	private static void SetStackShiftX(float value)
	{
		_stackShiftX = value;
	}

	public static float GetStackShiftY()
	{
		return _stackShiftY;
	}

	private static void SetStackShiftY(float value)
	{
		_stackShiftY = value;
	}

	public static float GetFontScale()
	{
		return _fontScale;
	}

	private static void SetFontScale(float value)
	{
		_fontScale = value;
	}

	public static void Parse(XmlNode node)
	{
		SetFadeFrames(node["FadeFrames"].ParseInOut());
		SetPulseAccel(node["PulseAccel"].ParseInOut());
		SetPulseFrames(node["PulseFrames"].ParseInOut());
		set_Spacing(new Vector2
		{
			x = node["Spacing"].Attributes["X"].ParseFloat(),
			y = node["Spacing"].Attributes["Y"].ParseFloat()
		});
		SetPulseAmplitude(node["PulseAmp"].FirstAttribute().ParseFloat());
		SetRowCapacity(node["RowCapacity"].Attributes["Value"].ParseFloat());
		SetExpirationOpacity(node["ExpirationOpacity"].Attributes["Value"].ParseFloat());
		SetStackShiftX(node["StackShiftX"].Attributes["Value"].ParseFloat());
		SetStackShiftY(node["StackShiftY"].Attributes["Value"].ParseFloat());
		SetFontScale(node["FontScale"].Attributes["Value"].ParseFloat());
	}
}
