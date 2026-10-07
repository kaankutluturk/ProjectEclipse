using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class MD5Utils
{
	public static string MD5HashFile(string filePath, string salt = null)
	{
		try
		{
			byte[] fileBytes = File.ReadAllBytes(filePath);
			return MD5HashBytes(fileBytes, salt);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return string.Empty;
	}

	public static string MD5HashString(string input, string salt = null)
	{
		using (MD5 mD = MD5.Create())
		{
			byte[] hashBytes = mD.ComputeHash(StringToByteArray(input));
			string text = ByteArrayToString(hashBytes);
			if (salt == null)
			{
				return text;
			}
			byte[] nCHAHPLJBMD2 = mD.ComputeHash(StringToByteArray(text + salt));
			return ByteArrayToString(nCHAHPLJBMD2);
		}
	}

	public static string MD5HashBytes(byte[] data, string salt = null)
	{
		using (MD5 mD = MD5.Create())
		{
			byte[] hashBytes = mD.ComputeHash(data);
			string text = ByteArrayToString(hashBytes);
			if (salt == null)
			{
				return text;
			}
			byte[] nCHAHPLJBMD2 = mD.ComputeHash(StringToByteArray(text + salt));
			return ByteArrayToString(nCHAHPLJBMD2);
		}
	}

	public static bool CheckFileHash(string filePath, string expectedHash, string salt = null)
	{
		return MD5HashFile(filePath, salt) == expectedHash;
	}

	public static bool CheckStringHash(string input, string expectedHash, string salt = null)
	{
		return MD5HashString(input, salt) == expectedHash;
	}

	public static bool CheckBytesHash(byte[] data, string expectedHash, string salt = null)
	{
		return MD5HashBytes(data, salt) == expectedHash;
	}

	public static string ByteArrayToString(byte[] bytes)
	{
		string text = BitConverter.ToString(bytes);
		return text.Replace("-", string.Empty);
	}

	public static byte[] StringToByteArray(string input)
	{
		return Encoding.UTF8.GetBytes(input);
	}
}
