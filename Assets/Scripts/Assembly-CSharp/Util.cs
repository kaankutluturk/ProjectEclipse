public static class Util
{
	public static void Swap<T>(ref T first, ref T second)
	{
		T val = first;
		first = second;
		second = val;
	}
}
