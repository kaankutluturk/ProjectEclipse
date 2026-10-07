using System;
using System.Linq;
using UnityEngine;

public static class NativeExtensions
{
	public static void SafeInvoke(this Action action)
	{
		if (action != null)
		{
			action();
		}
	}

	public static void SafeInvoke<T>(this Action<T> action, T value)
	{
		if (action != null)
		{
			action(value);
		}
	}

	public static string DescribeInvocationList<T>(this Action<T> action)
	{
		return "(" + action.GetInvocationList().Length + " total) " + string.Join(", ", (from d in action.GetInvocationList()
			select d.Method.Name).ToArray());
	}

	public static bool IsEmpty<T>(this Action<T> action)
	{
		return action == null || action.GetInvocationList().Length == 0;
	}

	public static float MillisecondsToSeconds(this float milliseconds)
	{
		return milliseconds / 1000f;
	}

	public static bool IsEqual(this int number, Enum enumValue)
	{
		try
		{
			return number == Convert.ToInt32(enumValue);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			return false;
		}
	}
}
