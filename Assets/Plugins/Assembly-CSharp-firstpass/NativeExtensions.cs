using System;
using System.Linq;
using UnityEngine;

public static class NativeExtensions
{
	public static void SafeInvoke(this Action IBODMPMJELJ)
	{
		if (IBODMPMJELJ != null)
		{
			IBODMPMJELJ();
		}
	}

	public static void SafeInvoke<T>(this Action<T> IBODMPMJELJ, T value)
	{
		if (IBODMPMJELJ != null)
		{
			IBODMPMJELJ(value);
		}
	}

	public static string DescribeInvocationList<T>(this Action<T> IBODMPMJELJ)
	{
		return "(" + IBODMPMJELJ.GetInvocationList().Length + " total) " + string.Join(", ", (from d in IBODMPMJELJ.GetInvocationList()
			select d.Method.Name).ToArray());
	}

	public static bool IsEmpty<T>(this Action<T> IBODMPMJELJ)
	{
		return IBODMPMJELJ == null || IBODMPMJELJ.GetInvocationList().Length == 0;
	}

	public static float MillisecondsToSeconds(this float FINAMGBHHDL)
	{
		return FINAMGBHHDL / 1000f;
	}

	public static bool IsEqual(this int number, Enum FOPOKALJIIJ)
	{
		try
		{
			return number == Convert.ToInt32(FOPOKALJIIJ);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			return false;
		}
	}
}
