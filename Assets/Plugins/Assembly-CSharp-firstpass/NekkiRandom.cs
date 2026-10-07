using System;
using System.Collections.Generic;
using System.Text;

public class NekkiRandom
{
	private RandomGenerator generator;

	public NekkiRandom(uint seed)
	{
		generator = new RandomGenerator(seed);
	}

	public NekkiRandom()
	{
		generator = new RandomGenerator(0u);
		uint seed = (uint)DateTime.UtcNow.Ticks;
		generator.setSeed(seed);
	}

	public uint GetRandMax()
	{
		return RandomGenerator.GetMaxValue();
	}

	public uint NextRaw()
	{
		return generator.NextRandom();
	}

	public void setSeed(uint seed)
	{
		generator.setSeed(seed);
	}

	public uint GetSeed()
	{
		return generator.GetSeed();
	}

	public float randomFloat()
	{
		return (float)NextRaw() / (float)GetRandMax() + (float)NextRaw() / (float)GetRandMax() / (float)GetRandMax();
	}

	public float randomFloat(float max)
	{
		return randomFloat() * max;
	}

	public float randomFloat(float min, float max)
	{
		return min + randomFloat(max - min);
	}

	public uint randomInt(uint max)
	{
		return (uint)((float)max * randomFloat());
	}

	public uint randomInt(uint min, uint max)
	{
		return min + randomInt(max - min);
	}

	public bool randomChance(float chance, float outOf = 100f)
	{
		return randomFloat(outOf) < chance;
	}

	public string randomString(int charCount, string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ")
	{
		uint length = (uint)characters.Length;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < charCount; i++)
		{
			stringBuilder.Append(characters[(int)randomInt(length)]);
		}
		return stringBuilder.ToString();
	}

	public void ShuffleList<T>(List<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = (int)randomInt(0u, (uint)num);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public void ShuffleArray<T>(T[] array)
	{
		int num = array.Length;
		while (num > 1)
		{
			num--;
			int num2 = (int)randomInt(0u, (uint)num);
			T val = array[num2];
			array[num2] = array[num];
			array[num] = val;
		}
	}
}
