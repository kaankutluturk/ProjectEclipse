using System;
using System.Collections.Generic;

public class NekkiMath
{
	private const double RadToDegFactor = 180.0 / Math.PI;

	private const double DegToRadFactor = Math.PI / 180.0;

	private const double FloatEpsilon = 1.1920929E-07;

	private static NekkiRandom random = new NekkiRandom();

	public static float round(float number, int scale)
	{
		return (float)(int)(number * (float)scale) / (float)scale;
	}

	public static float CeilToDecimals(float value, int decimals)
	{
		return 0f - FloorToDecimals(0f - value, decimals);
	}

	public static float FloorToDecimals(float value, int decimals)
	{
		float num = TruncateToDecimals(value, decimals);
		if (value < num && value < 0f)
		{
			return num - (float)Math.Pow(10.0, -decimals);
		}
		return num;
	}

	public static float RoundAwayFromZero(float value, int decimals)
	{
		int num = ((value > 0f) ? 1 : (-1));
		return (float)num * CeilToDecimals(Math.Abs(value), decimals);
	}

	public static float TruncateToDecimals(float number, int order)
	{
		int num = (int)Math.Pow(10.0, order);
		return (float)(int)(number * (float)num) / (float)num;
	}

	public static string RoundToString(float number, int order)
	{
		return Math.Round(number, order).ToString();
	}

	public static float LogBase(float number, float _base)
	{
		float result = 0f;
		if (_base > 0f && _base != 1f && number > 0f)
		{
			result = (float)(Math.Log(number) / Math.Log(_base));
		}
		return result;
	}

	public static float RadToDeg(float radians)
	{
		return (float)((double)radians * (180.0 / Math.PI));
	}

	public static float DegToRad(float degrees)
	{
		return (float)((double)degrees * (Math.PI / 180.0));
	}

	public static float randomFloat()
	{
		return random.randomFloat();
	}

	public static float randomFloat(float max)
	{
		return random.randomFloat(max);
	}

	public static float randomFloat(float min, float max)
	{
		return random.randomFloat(min, max);
	}

	public static int randomInt(int max)
	{
		return (int)random.randomInt((uint)max);
	}

	public static int randomInt(int min, int max)
	{
		return (int)random.randomInt((uint)min, (uint)max);
	}

	public static bool randomChance(float chance, float outOf = 100f)
	{
		return random.randomChance(chance, outOf);
	}

	public static uint SetSeed()
	{
		uint num = (uint)DateTime.UtcNow.Ticks;
		random.setSeed(num);
		return num;
	}

	public static void SetSeed(int seed)
	{
		random.setSeed((uint)seed);
	}

	public static T RandomElement<T>(List<T> list)
	{
		int count = list.Count;
		if (count == 0)
		{
			AdvLog.LogError("NekkiMath::randomElement - empty vector");
		}
		return list[randomInt(count)];
	}

	private static int GetRandMax()
	{
		return (int)random.GetRandMax();
	}
}
