using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class StringExtension
{
	private static class FnvConstants
	{
		public static readonly uint Prime32 = 16777619u;

		public static readonly ulong Prime64 = 1099511628211uL;

		public static readonly uint OffsetBasis32 = 2166136261u;

		public static readonly ulong OffsetBasis64 = 14695981039346656037uL;
	}

	public static int ToInt(this string text, int defaultValue = 0)
	{
		int result;
		if (text != null && int.TryParse(text, out result))
		{
			return result;
		}
		return defaultValue;
	}

	public static long ToLong(this string text, long defaultValue = 0L)
	{
		long result;
		if (text != null && long.TryParse(text, out result))
		{
			return result;
		}
		return defaultValue;
	}

	public static float ToFloat(this string text, float defaultValue = 0f)
	{
		float result;
		if (text != null && float.TryParse(text, out result))
		{
			return result;
		}
		return defaultValue;
	}

	public static double ToDouble(this string text, double defaultValue = 0.0)
	{
		double result;
		if (text != null && double.TryParse(text, out result))
		{
			return result;
		}
		return defaultValue;
	}

	public static T ToEnum<T>(this string text)
	{
		return text.ToEnum((T)Enum.GetValues(typeof(T)).GetValue(0));
	}

	public static T ToEnum<T>(this string text, T defaultValue)
	{
		try
		{
			return (T)Enum.Parse(typeof(T), text, true);
		}
		catch
		{
			return defaultValue;
		}
	}

	public static string DoubleStrToIntegerStr(this string encoded)
	{
		byte[] bytes = Convert.FromBase64String(encoded);
		return Encoding.UTF8.GetString(bytes);
	}

	public static uint GetFnv1aHash(this string text, bool useTwoBytes = false)
	{
		IEnumerable<byte> enumerable = ((!useTwoBytes) ? text.ToCharArray().Select(Convert.ToByte) : (from character in text.ToCharArray()
			select new byte[2]
			{
				(byte)(character - (byte)character >> 8),
				(byte)character
			}).SelectMany((byte[] charBytes) => charBytes));
		uint num = FnvConstants.OffsetBasis32;
		foreach (byte item in enumerable)
		{
			num ^= item;
			num *= FnvConstants.Prime32;
		}
		return num;
	}
}
