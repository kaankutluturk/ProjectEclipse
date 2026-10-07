public sealed class Adler
{
	private static readonly uint BASE = 65521u;

	private static readonly int NMAX = 5552;

	public static uint Adler32(uint adler, byte[] buffer, int index, int length)
	{
		if (buffer == null)
		{
			return 1u;
		}
		uint num = adler & 0xFFFF;
		uint num2 = (adler >> 16) & 0xFFFF;
		while (length > 0)
		{
			int num3 = ((length >= NMAX) ? NMAX : length);
			length -= num3;
			while (num3 >= 16)
			{
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num += buffer[index++];
				num2 += num;
				num3 -= 16;
			}
			if (num3 != 0)
			{
				do
				{
					num += buffer[index++];
					num2 += num;
				}
				while (--num3 != 0);
			}
			num %= BASE;
			num2 %= BASE;
		}
		return (num2 << 16) | num;
	}
}
