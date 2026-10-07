using System.IO;
using System.Xml;
using Nekki.SF2.Core.Exceptions;

public static class UserDataValidator
{
	private const string HashFileExtension = ".hash";

	private const string HashSecret = "wqO+Qchj|r*QXg7o_KNmLYvpGHdSwqxwlQI2vy618KaD^Pwt-h3H8*uJ";

	private static bool _IsValid = true;

	public static bool IsValid
	{
		get
		{
			return GetIsValid();
		}
	}

	public static string HashKey
	{
		get
		{
			return GetHashKey();
		}
	}

	public static bool GetIsValid()
	{
		return _IsValid;
	}

	public static string GetHashKey()
	{
		return SystemProperties.GetDeviceId() + "wqO+Qchj|r*QXg7o_KNmLYvpGHdSwqxwlQI2vy618KaD^Pwt-h3H8*uJ";
	}

	public static bool CheckFileHash(XmlDocument LOBFDOKFJIP, string ONEIGMLOGDC)
	{
		if (!GameSettings.IsUserDataValidationEnabled())
		{
			return true;
		}
		string text = ReadHash(ONEIGMLOGDC + ".hash");
		return CheckSnapshotHash(LOBFDOKFJIP, text, ONEIGMLOGDC);
	}

	internal static bool CheckSnapshotHash(XmlDocument LOBFDOKFJIP, string text, string ONEIGMLOGDC)
	{
		if (!GameSettings.IsUserDataValidationEnabled()) return true;
		if (string.IsNullOrEmpty(text))
		{
			_IsValid = false;
			throw new HackDetectedException("[UserDataValidator]: file is missing - " + Path.GetFileName(ONEIGMLOGDC + ".hash") + " !");
		}
		_IsValid = MD5Utils.CheckStringHash(LOBFDOKFJIP.OuterXml, text, GetHashKey());
		if (!_IsValid)
		{
			throw new HackDetectedException("[UserDataValidator]: incorrect file hash - " + Path.GetFileName(ONEIGMLOGDC) + " !");
		}
		return _IsValid;
	}

	private static string ReadHash(string ONEIGMLOGDC)
	{
		if (!File.Exists(ONEIGMLOGDC))
		{
			return null;
		}
		return File.ReadAllText(ONEIGMLOGDC);
	}

	public static void UpdateFileHash(string LOBFDOKFJIP)
	{
		if (GameSettings.IsUserDataValidationEnabled())
		{
			string iMMGBGKAMPK = MD5Utils.MD5HashFile(LOBFDOKFJIP, GetHashKey());
			WriteHashFile(LOBFDOKFJIP + ".hash", iMMGBGKAMPK);
		}
	}

	public static void UpdateFileHash(XmlDocument LOBFDOKFJIP, string ONEIGMLOGDC)
	{
		if (GameSettings.IsUserDataValidationEnabled())
		{
			string iMMGBGKAMPK = MD5Utils.MD5HashString(LOBFDOKFJIP.OuterXml, GetHashKey());
			WriteHashFile(ONEIGMLOGDC + ".hash", iMMGBGKAMPK);
		}
	}

	private static void WriteHashFile(string ONEIGMLOGDC, string IMMGBGKAMPK)
	{
		File.WriteAllText(ONEIGMLOGDC, IMMGBGKAMPK);
	}

	public static void DeleteHashFile(string ONEIGMLOGDC)
	{
		if (GameSettings.IsUserDataValidationEnabled())
		{
			ONEIGMLOGDC += ".hash";
			if (File.Exists(ONEIGMLOGDC))
			{
				File.Delete(ONEIGMLOGDC);
			}
		}
	}

	public static void CopyHashFile(string AMNCLCPADOO, string IFIOLDFCLIE)
	{
		if (GameSettings.IsUserDataValidationEnabled() && File.Exists(AMNCLCPADOO + ".hash"))
		{
			File.Copy(AMNCLCPADOO + ".hash", IFIOLDFCLIE + ".hash", true);
		}
	}
}
