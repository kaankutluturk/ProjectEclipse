using System;
using System.Runtime.InteropServices;
using System.Text;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public class RsaStringDecryptor
{
	private static byte[] publicKey = new byte[148]
	{
		6, 2, 0, 0, 0, 164, 0, 0, 82, 83,
		65, 49, 0, 4, 0, 0, 17, 0, 0, 0,
		253, 204, 69, 228, 171, 92, 184, 7, 144, 118,
		20, 89, 210, 43, 78, 187, 232, 229, 181, 226,
		100, 254, 168, 224, 123, 134, 52, 18, 93, 221,
		149, 56, 114, 214, 120, 160, 73, 76, 234, 126,
		212, 247, 163, 84, 61, 99, 132, 37, 57, 23,
		114, 89, 43, 217, 60, 111, 236, 105, 200, 45,
		39, 134, 68, 243, 115, 79, 141, 79, 195, 14,
		165, 2, 22, 12, 246, 168, 144, 54, 190, 54,
		143, 190, 251, 131, 75, 198, 92, 99, 255, 107,
		0, 162, 65, 221, 236, 130, 73, 115, 151, 166,
		141, 223, 9, 228, 53, 219, 62, 55, 88, 136,
		234, 224, 47, 136, 45, 251, 45, 79, 88, 32,
		71, 200, 98, 140, 162, 179, 134, 170
	};

	private static int blockLengthField = 128;

	private static int exponentField = publicKey[16] | (publicKey[17] << 8) | (publicKey[18] << 16);

	private static BigInteger nField;

	static RsaStringDecryptor()
	{
		byte[] array = new byte[blockLengthField];
		Buffer.BlockCopy(publicKey, 20, array, 0, blockLengthField);
		Array.Reverse(array);
		nField = new BigInteger(array);
	}

	private static string TrimLeadingZerosToString(byte[] data)
	{
		int i;
		for (i = 0; i < data.Length && data[i] == 0; i++)
		{
		}
		if (i != data.Length)
		{
			byte[] array = new byte[data.Length - i];
			Buffer.BlockCopy(data, i, array, 0, data.Length - i);
			return Encoding.UTF8.GetString(array);
		}
		return string.Empty;
	}

	public static string Decrypt(byte[] encryptedData, bool stripQuotes)
	{
		if (encryptedData.Length == blockLengthField)
		{
			BigInteger bigInteger = new BigInteger(encryptedData);
			byte[] bytes = bigInteger.ModPow(exponentField, nField).GetBytes();
			string text = TrimLeadingZerosToString(bytes);
			if (stripQuotes)
			{
				return text.Substring(1, text.Length - 2);
			}
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < encryptedData.Length / blockLengthField; i++)
		{
			byte[] array = new byte[blockLengthField];
			Buffer.BlockCopy(encryptedData, i * blockLengthField, array, 0, blockLengthField);
			BigInteger bigInteger2 = new BigInteger(array);
			byte[] bytes2 = bigInteger2.ModPow(exponentField, nField).GetBytes();
			stringBuilder.Append(TrimLeadingZerosToString(bytes2));
		}
		if (stripQuotes)
		{
			string text2 = stringBuilder.ToString();
			return text2.Substring(1, text2.Length - 2);
		}
		return stringBuilder.ToString();
	}
}
