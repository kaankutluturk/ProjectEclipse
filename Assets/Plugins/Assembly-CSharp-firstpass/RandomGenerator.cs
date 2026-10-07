public class RandomGenerator
{
	private const uint M = 2147483647u;

	private uint _state;

	private uint _seed;

	public RandomGenerator(uint seed)
	{
		setSeed(seed);
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

	public void setSeed(uint seed)
	{
		_seed = seed;
		_state = seed;
	}

	public uint randLCG(uint current)
	{
		current = (current * 1103515245 + 12345) & 0x7FFFFFFF;
		return current;
	}

	public static uint GetMaxValue()
	{
		return 2147483648u;
	}
}
