using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class Extensions
{
	public static string AsciiToString(this byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder(bytes.Length);
		foreach (byte b in bytes)
		{
			stringBuilder.Append((char)((b > 127) ? 63 : b));
		}
		return stringBuilder.ToString();
	}

	public static byte[] GetASCIIBytes(this string text)
	{
		byte[] array = new byte[text.Length];
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			array[i] = (byte)((c >= '\u0080') ? '?' : c);
		}
		return array;
	}

	public static void SendAsASCII(this BinaryWriter writer, string text)
	{
		foreach (char c in text)
		{
			writer.Write((byte)((c >= '\u0080') ? '?' : c));
		}
	}

	public static void WriteLine(this FileStream stream)
	{
		stream.Write(HTTPRequest.EOL, 0, 2);
	}

	public static void WriteLine(this FileStream stream, string text)
	{
		byte[] array = text.GetASCIIBytes();
		stream.Write(array, 0, array.Length);
		stream.WriteLine();
	}

	public static void WriteLine(this FileStream stream, string format, params object[] args)
	{
		byte[] array = string.Format(format, args).GetASCIIBytes();
		stream.Write(array, 0, array.Length);
		stream.WriteLine();
	}

	public static string[] FindOption(this string text, string option)
	{
		string[] array = text.ToLower().Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		option = option.ToLower();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Contains(option))
			{
				return array[i].Split(new char[1] { '=' }, StringSplitOptions.RemoveEmptyEntries);
			}
		}
		return null;
	}

	public static int ToInt32(this string text, int defaultValue = 0)
	{
		if (text == null)
		{
			return defaultValue;
		}
		try
		{
			return int.Parse(text);
		}
		catch
		{
			return defaultValue;
		}
	}

	public static long ToInt64(this string text, long defaultValue = 0L)
	{
		if (text == null)
		{
			return defaultValue;
		}
		try
		{
			return long.Parse(text);
		}
		catch
		{
			return defaultValue;
		}
	}

	public static DateTime ToDateTime(this string text, DateTime defaultValue = default(DateTime))
	{
		if (text == null)
		{
			return defaultValue;
		}
		try
		{
			DateTime.TryParse(text, out defaultValue);
			return defaultValue.ToUniversalTime();
		}
		catch
		{
			return defaultValue;
		}
	}

	public static string ToStrOrEmpty(this string text)
	{
		if (text == null)
		{
			return string.Empty;
		}
		return text;
	}

	public static string CalculateMD5Hash(this string text)
	{
		return text.GetASCIIBytes().CalculateMD5Hash();
	}

	public static string CalculateMD5Hash(this byte[] data)
	{
		byte[] array = MD5.Create().ComputeHash(data);
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	internal static string Read(this string text, ref int LCCLEFMKLPB, char delimiter, bool returnText = true)
	{
		return text.Read(ref LCCLEFMKLPB, (char KDFCGMMKAME) => KDFCGMMKAME != delimiter, returnText);
	}

	internal static string Read(this string text, ref int LCCLEFMKLPB, Func<char, bool> predicate, bool returnText = true)
	{
		if (LCCLEFMKLPB >= text.Length)
		{
			return string.Empty;
		}
		text.SkipWhiteSpace(ref LCCLEFMKLPB);
		int num = LCCLEFMKLPB;
		while (LCCLEFMKLPB < text.Length && predicate(text[LCCLEFMKLPB]))
		{
			LCCLEFMKLPB++;
		}
		string result = ((!returnText) ? null : text.Substring(num, LCCLEFMKLPB - num));
		LCCLEFMKLPB++;
		return result;
	}

	internal static string ReadQuotedText(this string text, ref int LCCLEFMKLPB)
	{
		string empty = string.Empty;
		if (text == null)
		{
			return empty;
		}
		if (text[LCCLEFMKLPB] == '"')
		{
			text.Read(ref LCCLEFMKLPB, '"', false);
			empty = text.Read(ref LCCLEFMKLPB, '"');
			text.Read(ref LCCLEFMKLPB, ',', false);
		}
		else
		{
			empty = text.Read(ref LCCLEFMKLPB, ',');
		}
		return empty;
	}

	internal static void SkipWhiteSpace(this string text, ref int LCCLEFMKLPB)
	{
		if (LCCLEFMKLPB < text.Length)
		{
			while (LCCLEFMKLPB < text.Length && char.IsWhiteSpace(text[LCCLEFMKLPB]))
			{
				LCCLEFMKLPB++;
			}
		}
	}

	internal static string TrimAndLower(this string text)
	{
		if (text == null)
		{
			return null;
		}
		char[] array = new char[text.Length];
		int length = 0;
		foreach (char c in text)
		{
			if (!char.IsWhiteSpace(c) && !char.IsControl(c))
			{
				array[length++] = char.ToLowerInvariant(c);
			}
		}
		return new string(array, 0, length);
	}

	internal static List<KeyValuePair> ParseOptionalHeader(this string text)
	{
		List<KeyValuePair> list = new List<KeyValuePair>();
		if (text == null)
		{
			return list;
		}
		int LCCLEFMKLPB = 0;
		while (LCCLEFMKLPB < text.Length)
		{
			string key = text.Read(ref LCCLEFMKLPB, (char KDFCGMMKAME) => KDFCGMMKAME != '=' && KDFCGMMKAME != ',').TrimAndLower();
			KeyValuePair pair = new KeyValuePair(key);
			if (text[LCCLEFMKLPB - 1] == '=')
			{
				pair.set_Value(text.ReadQuotedText(ref LCCLEFMKLPB));
			}
			list.Add(pair);
		}
		return list;
	}

	internal static List<KeyValuePair> ParseQualityParams(this string text)
	{
		List<KeyValuePair> list = new List<KeyValuePair>();
		if (text == null)
		{
			return list;
		}
		int LCCLEFMKLPB = 0;
		while (LCCLEFMKLPB < text.Length)
		{
			string key = text.Read(ref LCCLEFMKLPB, (char KDFCGMMKAME) => KDFCGMMKAME != ',' && KDFCGMMKAME != ';').TrimAndLower();
			KeyValuePair pair = new KeyValuePair(key);
			if (text[LCCLEFMKLPB - 1] == ';')
			{
				text.Read(ref LCCLEFMKLPB, '=', false);
				pair.set_Value(text.Read(ref LCCLEFMKLPB, ','));
			}
			list.Add(pair);
		}
		return list;
	}

	public static void ReadBuffer(this Stream stream, byte[] buffer)
	{
		int num = 0;
		do
		{
			num += stream.Read(buffer, num, buffer.Length - num);
		}
		while (num < buffer.Length);
	}

	public static void WriteAll(this MemoryStream stream, byte[] buffer)
	{
		stream.Write(buffer, 0, buffer.Length);
	}

	public static void WriteString(this MemoryStream stream, string text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		stream.WriteAll(bytes);
	}

	public static void WriteLine(this MemoryStream stream)
	{
		stream.WriteAll(HTTPRequest.EOL);
	}

	public static void WriteLine(this MemoryStream stream, string text)
	{
		stream.WriteString(text);
		stream.WriteLine();
	}
}
