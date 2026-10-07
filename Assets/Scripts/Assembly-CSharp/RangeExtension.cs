using System;
using UnityEngine.SocialPlatforms;
using Range = UnityEngine.SocialPlatforms.Range;

public static class RangeExtension
{
	public static int GetLastIndex(this Range range)
	{
		if (range.count == 0)
		{
			throw new InvalidOperationException("Range is invalid");
		}
		return range.from + range.count - 1;
	}

	public static bool Contains(this Range range, int index)
	{
		return index >= range.from && index < range.from + range.count;
	}
}
