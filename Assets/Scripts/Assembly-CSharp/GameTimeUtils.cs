using System;

public static class GameTimeUtils
{
	private static long gameEpochMs = 1262304000000L;

	public static long CurrentUnixTimeMs
	{
		get
		{
			return GetUnixTimeMs();
		}
	}

	public static TimeZone LocalTimeZone
	{
		get
		{
			return GetLocalTimeZone();
		}
	}

	public static TimeSpan UtcOffset
	{
		get
		{
			return GetUtcOffset();
		}
	}

	public static long GetUnixTimeMs()
	{
		return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
	}

	public static int ToGameSeconds(long unixMs)
	{
		return (int)((double)(unixMs - gameEpochMs) * 0.001);
	}

	public static long FromGameSeconds(int gameSeconds)
	{
		return (long)gameSeconds * 1000L + gameEpochMs;
	}

	public static TimeZone GetLocalTimeZone()
	{
		return TimeZone.CurrentTimeZone;
	}

	public static TimeSpan GetUtcOffset()
	{
		return GetLocalTimeZone().GetUtcOffset(DateTime.Now);
	}
}
