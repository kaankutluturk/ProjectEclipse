using System.IO;
using System.Text;

internal class SharedUtils
{
	public static int URShift(int number, int bits)
	{
		return (int)((uint)number >> bits);
	}

	public static int ReadInput(TextReader reader, byte[] target, int start, int count)
	{
		if (target.Length == 0)
		{
			return 0;
		}
		char[] array = new char[target.Length];
		int num = reader.Read(array, start, count);
		if (num == 0)
		{
			return -1;
		}
		for (int i = start; i < start + num; i++)
		{
			target[i] = (byte)array[i];
		}
		return num;
	}

	internal static byte[] ToByteArray(string text)
	{
		return Encoding.UTF8.GetBytes(text);
	}

	internal static char[] ToCharArray(byte[] bytes)
	{
		return Encoding.UTF8.GetChars(bytes);
	}
}
