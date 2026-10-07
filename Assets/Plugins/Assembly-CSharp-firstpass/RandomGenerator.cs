public class RandomGenerator
{
	private const uint M = 2147483647u;

	private uint _state;

	private uint _seed;

	public RandomGenerator(uint OKGKLCLEDFN)
	{
		setSeed(OKGKLCLEDFN);
	}

	public uint NextRandom()
	{
		_state = randLCG(_state);
		return _state;
	}

	public uint GetSeed()
	{
		return _seed;
	}

	public void setSeed(uint OKGKLCLEDFN)
	{
		_seed = OKGKLCLEDFN;
		_state = OKGKLCLEDFN;
	}

	public uint randLCG(uint DGNDGHPMPJD)
	{
		DGNDGHPMPJD = (DGNDGHPMPJD * 1103515245 + 12345) & 0x7FFFFFFF;
		return DGNDGHPMPJD;
	}

	public static uint GetMaxValue()
	{
		return 2147483648u;
	}
}
