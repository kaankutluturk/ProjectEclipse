using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class AESUtils
{
	public static void EncryptBytesToFile(byte[] GHDPPHAAPCA, byte[] BFADPFOIPLL, byte[] EJHBCOKHNNC, string BMDJOFHDOGF)
	{
		byte[] bytes = EncryptBytes(GHDPPHAAPCA, BFADPFOIPLL, EJHBCOKHNNC);
		File.WriteAllBytes(BMDJOFHDOGF, bytes);
	}

	public static byte[] DecryptFileToBytes(byte[] BFADPFOIPLL, byte[] EJHBCOKHNNC, string AMNCLCPADOO, bool GIEAPLJHHDK = false)
	{
		try
		{
			byte[] gHDPPHAAPCA = ((!GIEAPLJHHDK) ? ResourceManager.GetBinary(AMNCLCPADOO) : File.ReadAllBytes(AMNCLCPADOO));
			return DecryptBytes(gHDPPHAAPCA, BFADPFOIPLL, EJHBCOKHNNC);
		}
		catch
		{
			return null;
		}
	}

	public static void DecryptFileToFile(byte[] BFADPFOIPLL, byte[] EJHBCOKHNNC, string AMNCLCPADOO, string BMDJOFHDOGF = null, bool GIEAPLJHHDK = false)
	{
		if (BMDJOFHDOGF == null)
		{
			BMDJOFHDOGF = AMNCLCPADOO;
		}
		byte[] gHDPPHAAPCA = ((!GIEAPLJHHDK) ? ResourceManager.GetBinary(AMNCLCPADOO) : File.ReadAllBytes(AMNCLCPADOO));
		byte[] bytes = DecryptBytes(gHDPPHAAPCA, BFADPFOIPLL, EJHBCOKHNNC);
		File.WriteAllBytes(BMDJOFHDOGF, bytes);
	}

	public static void EncryptFileToFile(byte[] BFADPFOIPLL, byte[] EJHBCOKHNNC, string AMNCLCPADOO, string BMDJOFHDOGF = null, bool GIEAPLJHHDK = false)
	{
		if (BMDJOFHDOGF == null)
		{
			BMDJOFHDOGF = AMNCLCPADOO;
		}
		byte[] gHDPPHAAPCA = ((!GIEAPLJHHDK) ? ResourceManager.GetBinary(AMNCLCPADOO) : File.ReadAllBytes(AMNCLCPADOO));
		byte[] bytes = EncryptBytes(gHDPPHAAPCA, BFADPFOIPLL, EJHBCOKHNNC);
		File.WriteAllBytes(BMDJOFHDOGF, bytes);
	}

	public static string EncryptStringToBase64(string GHDPPHAAPCA, byte[] BFADPFOIPLL, byte[] EJHBCOKHNNC)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(GHDPPHAAPCA);
		byte[] array = EncryptBytes(bytes, BFADPFOIPLL, EJHBCOKHNNC);
		if (array == null)
		{
			return string.Empty;
		}
		return Convert.ToBase64String(array);
	}

	public static string DecryptBase64ToString(string GHDPPHAAPCA, byte[] BFADPFOIPLL, byte[] EJHBCOKHNNC)
	{
		byte[] gHDPPHAAPCA = Convert.FromBase64String(GHDPPHAAPCA);
		byte[] array = DecryptBytes(gHDPPHAAPCA, BFADPFOIPLL, EJHBCOKHNNC);
		if (array == null)
		{
			return string.Empty;
		}
		return Encoding.UTF8.GetString(array);
	}

	public static byte[] EncryptBytes(byte[] GHDPPHAAPCA, byte[] BFADPFOIPLL, byte[] EJHBCOKHNNC)
	{
		try
		{
			using (Aes aes = Aes.Create())
			{
				aes.Key = BFADPFOIPLL;
				aes.IV = EJHBCOKHNNC;
				ICryptoTransform pIDLNECOJBG = aes.CreateEncryptor(aes.Key, aes.IV);
				return TransformBytes(GHDPPHAAPCA, pIDLNECOJBG);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return null;
	}

	public static byte[] DecryptBytes(byte[] GHDPPHAAPCA, byte[] BFADPFOIPLL, byte[] EJHBCOKHNNC)
	{
		try
		{
			using (Aes aes = Aes.Create())
			{
				aes.Key = BFADPFOIPLL;
				aes.IV = EJHBCOKHNNC;
				ICryptoTransform pIDLNECOJBG = aes.CreateDecryptor(aes.Key, aes.IV);
				return TransformBytes(GHDPPHAAPCA, pIDLNECOJBG);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return null;
	}

	private static byte[] TransformBytes(byte[] GHDPPHAAPCA, ICryptoTransform PIDLNECOJBG)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (CryptoStream cryptoStream = new CryptoStream(memoryStream, PIDLNECOJBG, CryptoStreamMode.Write))
			{
				cryptoStream.Write(GHDPPHAAPCA, 0, GHDPPHAAPCA.Length);
				cryptoStream.FlushFinalBlock();
				return memoryStream.ToArray();
			}
		}
	}
}
