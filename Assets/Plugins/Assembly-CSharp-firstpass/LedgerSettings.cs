using System.Diagnostics;

public class LedgerSettings
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string url;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int timeout;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int maxRetry;

	public int Timeout
	{
		get
		{
			return GetTimeout();
		}
		private set
		{
			SetTimeout(value);
		}
	}

	public int MaxRetry
	{
		get
		{
			return GetMaxRetry();
		}
		private set
		{
			SetMaxRetry(value);
		}
	}

	public LedgerSettings(string BEPKJNKCKPH, int DGDKHFPEHOG, int BDNJNFPONEF)
	{
		set_Url(BEPKJNKCKPH);
		SetTimeout(DGDKHFPEHOG);
		SetMaxRetry(BDNJNFPONEF);
		if (!GetUrl().EndsWith("/"))
		{
			set_Url(GetUrl() + "/");
		}
	}

	public string GetUrl()
	{
		return url;
	}

	private void set_Url(string value)
	{
		url = value;
	}

	public int GetTimeout()
	{
		return timeout;
	}

	private void SetTimeout(int value)
	{
		timeout = value;
	}

	public int GetMaxRetry()
	{
		return maxRetry;
	}

	private void SetMaxRetry(int value)
	{
		maxRetry = value;
	}
}
