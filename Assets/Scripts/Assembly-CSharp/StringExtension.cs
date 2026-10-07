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

	public static int ToInt(this string IGGFGLLIGCG, int AGADEMLBJGJ = 0)
	{
		int result;
		if (IGGFGLLIGCG != null && int.TryParse(IGGFGLLIGCG, out result))
		{
			return result;
		}
		return AGADEMLBJGJ;
	}

	public static long ToLong(this string IGGFGLLIGCG, long AGADEMLBJGJ = 0L)
	{
		long result;
		if (IGGFGLLIGCG != null && long.TryParse(IGGFGLLIGCG, out result))
		{
			return result;
		}
		return AGADEMLBJGJ;
	}

	public static float ToFloat(this string IGGFGLLIGCG, float AGADEMLBJGJ = 0f)
	{
		float result;
		if (IGGFGLLIGCG != null && float.TryParse(IGGFGLLIGCG, out result))
		{
			return result;
		}
		return AGADEMLBJGJ;
	}

	public static double ToDouble(this string IGGFGLLIGCG, double AGADEMLBJGJ = 0.0)
	{
		double result;
		if (IGGFGLLIGCG != null && double.TryParse(IGGFGLLIGCG, out result))
		{
			return result;
		}
		return AGADEMLBJGJ;
	}

	public static T ToEnum<T>(this string LIAILCGJBDK)
	{
		return LIAILCGJBDK.ToEnum((T)Enum.GetValues(typeof(T)).GetValue(0));
	}

	public static T ToEnum<T>(this string LIAILCGJBDK, T JEALBOJLKFM)
	{
		try
		{
			return (T)Enum.Parse(typeof(T), LIAILCGJBDK, true);
		}
		catch
		{
			return JEALBOJLKFM;
		}
	}

	public static string DoubleStrToIntegerStr(this string EKCJOMMPCJJ)
	{
		byte[] bytes = Convert.FromBase64String(EKCJOMMPCJJ);
		return Encoding.UTF8.GetString(bytes);
	}

	public static uint GetFnv1aHash(this string EMIAKCGJNHP, bool HHOEINLMDAB = false)
	{
		IEnumerable<byte> enumerable = ((!HHOEINLMDAB) ? EMIAKCGJNHP.ToCharArray().Select(Convert.ToByte) : (from ILHDJDNPFKH in EMIAKCGJNHP.ToCharArray()
			select new byte[2]
			{
				(byte)(ILHDJDNPFKH - (byte)ILHDJDNPFKH >> 8),
				(byte)ILHDJDNPFKH
			}).SelectMany((byte[] ILHDJDNPFKH) => ILHDJDNPFKH));
		uint num = FnvConstants.OffsetBasis32;
		foreach (byte item in enumerable)
		{
			num ^= item;
			num *= FnvConstants.Prime32;
		}
		return num;
	}
}
