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

	public static bool CheckFileHash(XmlDocument document, string filePath)
	{
		if (!GameSettings.IsUserDataValidationEnabled())
		{
			return true;
		}
		string text = ReadHash(filePath + ".hash");
		return CheckSnapshotHash(document, text, filePath);
	}

	internal static bool CheckSnapshotHash(XmlDocument document, string text, string filePath)
	{
		if (!GameSettings.IsUserDataValidationEnabled()) return true;
		if (string.IsNullOrEmpty(text))
		{
			_IsValid = false;
			throw new HackDetectedException("[UserDataValidator]: file is missing - " + Path.GetFileName(filePath + ".hash") + " !");
		}
		_IsValid = MD5Utils.CheckStringHash(document.OuterXml, text, GetHashKey());
		if (!_IsValid)
		{
			throw new HackDetectedException("[UserDataValidator]: incorrect file hash - " + Path.GetFileName(filePath) + " !");
		}
		return _IsValid;
	}

	private static string ReadHash(string hashFilePath)
	{
		if (!File.Exists(hashFilePath))
		{
			return null;
		}
		return File.ReadAllText(hashFilePath);
	}

	public static void UpdateFileHash(string filePath)
	{
		if (GameSettings.IsUserDataValidationEnabled())
		{
			string hash = MD5Utils.MD5HashFile(filePath, GetHashKey());
			WriteHashFile(filePath + ".hash", hash);
		}
	}

	public static void UpdateFileHash(XmlDocument document, string filePath)
	{
		if (GameSettings.IsUserDataValidationEnabled())
		{
			string hash = MD5Utils.MD5HashString(document.OuterXml, GetHashKey());
			WriteHashFile(filePath + ".hash", hash);
		}
	}

	private static void WriteHashFile(string hashFilePath, string hash)
	{
		File.WriteAllText(hashFilePath, hash);
	}

	public static void DeleteHashFile(string filePath)
	{
		if (GameSettings.IsUserDataValidationEnabled())
		{
			filePath += ".hash";
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}
	}

	public static void CopyHashFile(string sourcePath, string destinationPath)
	{
		if (GameSettings.IsUserDataValidationEnabled() && File.Exists(sourcePath + ".hash"))
		{
			File.Copy(sourcePath + ".hash", destinationPath + ".hash", true);
		}
	}
}
