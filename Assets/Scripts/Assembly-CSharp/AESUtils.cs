using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class AESUtils
{
	public static void EncryptBytesToFile(byte[] data, byte[] key, byte[] iv, string outputPath)
	{
		byte[] bytes = EncryptBytes(data, key, iv);
		File.WriteAllBytes(outputPath, bytes);
	}

	public static byte[] DecryptFileToBytes(byte[] key, byte[] iv, string path, bool readFromDisk = false)
	{
		try
		{
			byte[] fileBytes = ((!readFromDisk) ? ResourceManager.GetBinary(path) : File.ReadAllBytes(path));
			return DecryptBytes(fileBytes, key, iv);
		}
		catch
		{
			return null;
		}
	}

	public static void DecryptFileToFile(byte[] key, byte[] iv, string path, string outputPath = null, bool readFromDisk = false)
	{
		if (outputPath == null)
		{
			outputPath = path;
		}
		byte[] fileBytes = ((!readFromDisk) ? ResourceManager.GetBinary(path) : File.ReadAllBytes(path));
		byte[] bytes = DecryptBytes(fileBytes, key, iv);
		File.WriteAllBytes(outputPath, bytes);
	}

	public static void EncryptFileToFile(byte[] key, byte[] iv, string path, string outputPath = null, bool readFromDisk = false)
	{
		if (outputPath == null)
		{
			outputPath = path;
		}
		byte[] fileBytes = ((!readFromDisk) ? ResourceManager.GetBinary(path) : File.ReadAllBytes(path));
		byte[] bytes = EncryptBytes(fileBytes, key, iv);
		File.WriteAllBytes(outputPath, bytes);
	}

	public static string EncryptStringToBase64(string text, byte[] key, byte[] iv)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		byte[] array = EncryptBytes(bytes, key, iv);
		if (array == null)
		{
			return string.Empty;
		}
		return Convert.ToBase64String(array);
	}

	public static string DecryptBase64ToString(string base64Text, byte[] key, byte[] iv)
	{
		byte[] encryptedBytes = Convert.FromBase64String(base64Text);
		byte[] array = DecryptBytes(encryptedBytes, key, iv);
		if (array == null)
		{
			return string.Empty;
		}
		return Encoding.UTF8.GetString(array);
	}

	public static byte[] EncryptBytes(byte[] data, byte[] key, byte[] iv)
	{
		try
		{
			using (Aes aes = Aes.Create())
			{
				aes.Key = key;
				aes.IV = iv;
				ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
				return TransformBytes(data, encryptor);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return null;
	}

	public static byte[] DecryptBytes(byte[] data, byte[] key, byte[] iv)
	{
		try
		{
			using (Aes aes = Aes.Create())
			{
				aes.Key = key;
				aes.IV = iv;
				ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
				return TransformBytes(data, decryptor);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return null;
	}

	private static byte[] TransformBytes(byte[] data, ICryptoTransform transform)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
			{
				cryptoStream.Write(data, 0, data.Length);
				cryptoStream.FlushFinalBlock();
				return memoryStream.ToArray();
			}
		}
	}
}
