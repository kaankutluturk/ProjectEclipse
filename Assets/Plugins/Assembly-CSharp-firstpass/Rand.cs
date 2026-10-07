using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public static class Rand
{
	private static short _seed;

	private static List<short> _source;

	private static List<short> _used;

	public static short CurrentSeed
	{
		get
		{
			return GetSeed();
		}
		set
		{
			set_Seed(value);
		}
	}

	private static short NextValue
	{
		get
		{
			return TakeNext();
		}
	}

	static Rand()
	{
		_source = new List<short>();
		_used = new List<short>();
		Reset(0);
	}

	public static short GetSeed()
	{
		return _seed;
	}

	public static void set_Seed(short value)
	{
		if (_seed != (ushort)(value % 65535))
		{
			Reset(value);
		}
	}

	public static void Init(int OKGKLCLEDFN)
	{
		if (_seed != (short)(OKGKLCLEDFN % 32767))
		{
			Reset(OKGKLCLEDFN);
		}
	}

	private static void Reset(int OKGKLCLEDFN)
	{
		_seed = (short)(OKGKLCLEDFN % 32767);
		_used.Clear();
		_source.Clear();
		for (short num = 0; num < short.MaxValue; num++)
		{
			_source.Add(num);
		}
		_source.Sort(delegate
		{
			return Random.Range(-1, 1);
		});
		_used.AddRange(_source.GetRange(0, _seed));
		_source.RemoveRange(0, _seed);
	}

	private static void SwapPools()
	{
		_used = Interlocked.Exchange(ref _source, _used);
		AdvLog.Log("after dirty: " + _used.Count + " source: " + _source.Count);
	}

	private static short TakeNext()
	{
		if (_source.Count == 0)
		{
			SwapPools();
		}
		int index = Random.Range(0, _source.Count);
		short num = _source[index];
		_source.RemoveAt(index);
		_used.Add(num);
		return num;
	}

	public static int Range(int IOFHCAAOELD, int IPMPAMAHLJG)
	{
		if (IOFHCAAOELD > IPMPAMAHLJG)
		{
			IPMPAMAHLJG = Interlocked.Exchange(ref IOFHCAAOELD, IPMPAMAHLJG);
		}
		int num = IPMPAMAHLJG - IOFHCAAOELD;
		return (num != 0) ? (IOFHCAAOELD + TakeNext() % num) : 0;
	}
}
