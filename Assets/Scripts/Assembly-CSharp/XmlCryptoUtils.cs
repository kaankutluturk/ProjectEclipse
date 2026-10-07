using System;
using System.IO;
using UnityEngine;

public class XmlCryptoUtils
{
	private static byte[] aesKey = new byte[32]
	{
		235, 183, 115, 127, 190, 102, 8, 38, 143, 83,
		145, 154, 166, 165, 4, 62, 149, 139, 150, 98,
		116, 121, 168, 0, 229, 206, 150, 77, 179, 200,
		129, 115
	};

	private static byte[] aesIv = new byte[16]
	{
		169, 159, 94, 17, 115, 17, 231, 206, 86, 58,
		123, 161, 50, 39, 189, 209
	};

	public static bool IsEncryptionEnabled
	{
		get
		{
			return GetIsEncryptionEnabled();
		}
	}

	public static bool GetIsEncryptionEnabled()
	{
		if (Application.isEditor)
		{
			return true;
		}
		return SystemProperties.IsAndroidPlatform() || SystemProperties.IsIosPlatform();
	}

	public static string LoadAndEncryptResource(string ONEIGMLOGDC)
	{
		ONEIGMLOGDC = StripExtension(ONEIGMLOGDC);
		TextAsset textAsset = ResourcesAndBundles.Load<TextAsset>(ONEIGMLOGDC);
		string result = null;
		try
		{
			if (textAsset != null)
			{
				result = AESUtils.EncryptStringToBase64(textAsset.text, aesKey, aesIv);
			}
		}
		catch (Exception ex)
		{
			GameLog.Error(ex.ToString());
		}
		return result;
	}

	public static string LoadAndDecryptResource(string ONEIGMLOGDC)
	{
		ONEIGMLOGDC = StripExtension(ONEIGMLOGDC);
		TextAsset textAsset = ResourcesAndBundles.Load<TextAsset>(ONEIGMLOGDC);
		string result = null;
		try
		{
			if (textAsset != null)
			{
				result = AESUtils.DecryptBase64ToString(textAsset.text, aesKey, aesIv);
			}
		}
		catch (Exception ex)
		{
			GameLog.Error(ex.ToString());
		}
		return result;
	}

	public static string EncryptString(string HCPNFPMHFCM)
	{
		string result = null;
		try
		{
			result = AESUtils.EncryptStringToBase64(HCPNFPMHFCM, aesKey, aesIv);
		}
		catch (Exception ex)
		{
			GameLog.Error(ex.ToString());
		}
		return result;
	}

	public static string DecryptStringOrPassThrough(string HCPNFPMHFCM)
	{
		string result = null;
		try
		{
			result = AESUtils.DecryptBase64ToString(HCPNFPMHFCM, aesKey, aesIv);
		}
		catch (Exception ex)
		{
			GameLog.Error(ex.ToString());
		}
		if (string.IsNullOrEmpty(result))
		{
			result = HCPNFPMHFCM;
		}
		return result;
	}

	private static string StripExtension(string ONEIGMLOGDC)
	{
		if (Path.HasExtension(ONEIGMLOGDC))
		{
			return Path.ChangeExtension(ONEIGMLOGDC, null);
		}
		return ONEIGMLOGDC;
	}
}
