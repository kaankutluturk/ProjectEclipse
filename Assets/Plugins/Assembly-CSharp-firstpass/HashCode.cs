internal static class HashCode
{
	public static int CombineHashCodes(int hash1, int hash2)
	{
		return ((hash1 << 5) + hash1) ^ hash2;
	}
}
