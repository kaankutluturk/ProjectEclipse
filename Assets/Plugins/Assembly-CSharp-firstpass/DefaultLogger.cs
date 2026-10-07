using System;
using System.Diagnostics;
using UnityEngine;

public class DefaultLogger : ILogger
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Loglevels level;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string formatVerbose;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string formatInfo;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string formatWarn;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string formatErr;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string formatEx;

	public Loglevels Level
	{
		get
		{
			return GetLevel();
		}
		set
		{
			SetLevel(value);
		}
	}

	public string FormatVerbose
	{
		get
		{
			return GetFormatVerbose();
		}
		set
		{
			SetFormatVerbose(value);
		}
	}

	public string FormatInfo
	{
		get
		{
			return GetFormatInfo();
		}
		set
		{
			SetFormatInfo(value);
		}
	}

	public string FormatWarn
	{
		get
		{
			return GetFormatWarn();
		}
		set
		{
			SetFormatWarn(value);
		}
	}

	public string FormatErr
	{
		get
		{
			return GetFormatErr();
		}
		set
		{
			SetFormatErr(value);
		}
	}

	public string FormatEx
	{
		get
		{
			return GetFormatEx();
		}
		set
		{
			SetFormatEx(value);
		}
	}

	public DefaultLogger()
	{
		SetFormatVerbose("I [{0}]: {1}");
		SetFormatInfo("I [{0}]: {1}");
		SetFormatWarn("W [{0}]: {1}");
		SetFormatErr("Err [{0}]: {1}");
		SetFormatEx("Ex [{0}]: {1} - Message: {2}  StackTrace: {3}");
		SetLevel((!UnityEngine.Debug.isDebugBuild) ? Loglevels.Error : Loglevels.Warning);
	}

	public Loglevels GetLevel()
	{
		return level;
	}

	public void SetLevel(Loglevels value)
	{
		level = value;
	}

	public string GetFormatVerbose()
	{
		return formatVerbose;
	}

	public void SetFormatVerbose(string value)
	{
		formatVerbose = value;
	}

	public string GetFormatInfo()
	{
		return formatInfo;
	}

	public void SetFormatInfo(string value)
	{
		formatInfo = value;
	}

	public string GetFormatWarn()
	{
		return formatWarn;
	}

	public void SetFormatWarn(string value)
	{
		formatWarn = value;
	}

	public string GetFormatErr()
	{
		return formatErr;
	}

	public void SetFormatErr(string value)
	{
		formatErr = value;
	}

	public string GetFormatEx()
	{
		return formatEx;
	}

	public void SetFormatEx(string value)
	{
		formatEx = value;
	}

	public void Verbose(string division, string message)
	{
		if (GetLevel() <= Loglevels.All)
		{
			try
			{
				AdvLog.Log(string.Format(GetFormatVerbose(), division, message));
			}
			catch
			{
			}
		}
	}

	public void Information(string division, string message)
	{
		if (GetLevel() <= Loglevels.Information)
		{
			try
			{
				AdvLog.Log(string.Format(GetFormatInfo(), division, message));
			}
			catch
			{
			}
		}
	}

	public void Warning(string division, string message)
	{
		if (GetLevel() <= Loglevels.Warning)
		{
			try
			{
				AdvLog.LogWarning(string.Format(GetFormatWarn(), division, message));
			}
			catch
			{
			}
		}
	}

	public void Error(string division, string message)
	{
		if (GetLevel() <= Loglevels.Error)
		{
			try
			{
				AdvLog.LogError(string.Format(GetFormatErr(), division, message));
			}
			catch
			{
			}
		}
	}

	public void Exception(string division, string message, Exception exception)
	{
		if (GetLevel() <= Loglevels.Exception)
		{
			try
			{
				AdvLog.LogError(string.Format(GetFormatEx(), division, message, (exception == null) ? "null" : exception.Message, (exception == null) ? "null" : exception.StackTrace));
			}
			catch
			{
			}
		}
	}
}
