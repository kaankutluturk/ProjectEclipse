using System;
using System.Diagnostics;

public sealed class HTTPCacheMaintananceParams
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan deleteOlder;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ulong maxCacheSize;

	public TimeSpan DeleteOlderThan
	{
		get
		{
			return GetDeleteOlder();
		}
		private set
		{
			set_DeleteOlder(value);
		}
	}

	public ulong MaxCacheSizeLimit
	{
		get
		{
			return GetMaxCacheSize();
		}
		private set
		{
			set_MaxCacheSize(value);
		}
	}

	public HTTPCacheMaintananceParams(TimeSpan HJELDPKENPC, ulong EBFOBGKCGJP)
	{
		set_DeleteOlder(HJELDPKENPC);
		set_MaxCacheSize(EBFOBGKCGJP);
	}

	public TimeSpan GetDeleteOlder()
	{
		return deleteOlder;
	}

	private void set_DeleteOlder(TimeSpan value)
	{
		deleteOlder = value;
	}

	public ulong GetMaxCacheSize()
	{
		return maxCacheSize;
	}

	private void set_MaxCacheSize(ulong value)
	{
		maxCacheSize = value;
	}
}
