using System;
using System.Collections.Generic;

public class NekkiMath
{
	private const double RadToDegFactor = 180.0 / Math.PI;

	private const double DegToRadFactor = Math.PI / 180.0;

	private const double FloatEpsilon = 1.1920929E-07;

	private static NekkiRandom random = new NekkiRandom();

	public static float round(float number, int HCHKHLJLJBG)
	{
		return (float)(int)(number * (float)HCHKHLJLJBG) / (float)HCHKHLJLJBG;
	}

	public static float CeilToDecimals(float value, int OMGDBOOMDKP)
	{
		return 0f - FloorToDecimals(0f - value, OMGDBOOMDKP);
	}

	public static float FloorToDecimals(float value, int OMGDBOOMDKP)
	{
		float num = TruncateToDecimals(value, OMGDBOOMDKP);
		if (value < num && value < 0f)
		{
			return num - (float)Math.Pow(10.0, -OMGDBOOMDKP);
		}
		return num;
	}

	public static float RoundAwayFromZero(float value, int OMGDBOOMDKP)
	{
		int num = ((value > 0f) ? 1 : (-1));
		return (float)num * CeilToDecimals(Math.Abs(value), OMGDBOOMDKP);
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

	public static float LogBase(float EHCLMBADLKH, float _base)
	{
		float result = 0f;
		if (_base > 0f && _base != 1f && EHCLMBADLKH > 0f)
		{
			result = (float)(Math.Log(EHCLMBADLKH) / Math.Log(_base));
		}
		return result;
	}

	public static float RadToDeg(float LMKNPGGDOCO)
	{
		return (float)((double)LMKNPGGDOCO * (180.0 / Math.PI));
	}

	public static float DegToRad(float NGMLOAFJDAP)
	{
		return (float)((double)NGMLOAFJDAP * (Math.PI / 180.0));
	}

	public static float randomFloat()
	{
		return random.randomFloat();
	}

	public static float randomFloat(float KAEPJHHLLPK)
	{
		return random.randomFloat(KAEPJHHLLPK);
	}

	public static float randomFloat(float LHNCHOAEGEA, float KAEPJHHLLPK)
	{
		return random.randomFloat(LHNCHOAEGEA, KAEPJHHLLPK);
	}

	public static int randomInt(int KAEPJHHLLPK)
	{
		return (int)random.randomInt((uint)KAEPJHHLLPK);
	}

	public static int randomInt(int LHNCHOAEGEA, int KAEPJHHLLPK)
	{
		return (int)random.randomInt((uint)LHNCHOAEGEA, (uint)KAEPJHHLLPK);
	}

	public static bool randomChance(float AMBMJABLPFE, float BCCEJBCHNHC = 100f)
	{
		return random.randomChance(AMBMJABLPFE, BCCEJBCHNHC);
	}

	public static uint SetSeed()
	{
		uint num = (uint)DateTime.UtcNow.Ticks;
		random.setSeed(num);
		return num;
	}

	public static void SetSeed(int OKGKLCLEDFN)
	{
		random.setSeed((uint)OKGKLCLEDFN);
	}

	public static T RandomElement<T>(List<T> HCMPBOCKJOP)
	{
		int count = HCMPBOCKJOP.Count;
		if (count == 0)
		{
			AdvLog.LogError("NekkiMath::randomElement - empty vector");
		}
		return HCMPBOCKJOP[randomInt(count)];
	}

	private static int GetRandMax()
	{
		return (int)random.GetRandMax();
	}
}
