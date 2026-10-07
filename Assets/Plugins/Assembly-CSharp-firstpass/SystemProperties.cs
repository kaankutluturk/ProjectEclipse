using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.Profiling;

public class SystemProperties
{
	public enum PathType
	{
		PATH_SMALL = 0,
		PATH_BIG = 1,
		PATH_DEFAULT = -1,
		PATH_NONE = 2
	}

	private static Vector2 smallResolution = new Vector2(1024f, 768f);

	private static Vector2 bigResolution = new Vector2(2048f, 1536f);

	private static Vector2 wideResolution = new Vector2(1365f, 768f);

	private static DeviceInfo deviceInfo = new DeviceInfo();

	private static int _numOfDisplayModes = 0;

	private static List<QualityCondition> qualityConditions = new List<QualityCondition>();

	public static Vector2 PictureResolution = default(Vector2);

	public static float ScaleX = 1f;

	public static float ScaleY = 1f;

	public static float VirtualWidth = 0f;

	public static float VirtualHeight = 0f;

	private static bool? isPaidCached;

	private static string IdentifierSuffix
	{
		get
		{
			return GetIdentifierSuffix();
		}
	}

	public static bool IsDebugMode
	{
		get
		{
			return IsDebug();
		}
	}

	public static bool Enabled
	{
		get
		{
			return IsEnabled();
		}
	}

	public static bool Available
	{
		get
		{
			return IsAvailable();
		}
	}

	public static int NumOfDisplayModes
	{
		get
		{
			return GetNumOfDisplayModes();
		}
	}

	public static int ScreenWidth
	{
		get
		{
			return GetScreenWidth();
		}
	}

	public static int ScreenHeight
	{
		get
		{
			return GetScreenHeight();
		}
	}

	public static bool IsEditor
	{
		get
		{
			return IsEditorPlatform();
		}
	}

	public static bool IsWindowsEditor
	{
		get
		{
			return IsWindowsEditorPlatform();
		}
	}

	public static bool IsIos
	{
		get
		{
			return IsIosPlatform();
		}
	}

	public static bool IsAndroid
	{
		get
		{
			return IsAndroidPlatform();
		}
	}

	public static bool IsWindows
	{
		get
		{
			return IsWindowsPlatform();
		}
	}

	public static bool IsMac
	{
		get
		{
			return IsMacPlatform();
		}
	}

	public static bool IsMetroArm
	{
		get
		{
			return IsMetroArmPlatform();
		}
	}

	public static bool IsWindowsStore
	{
		get
		{
			return IsWindowsStorePlatform();
		}
	}

	public static bool IsWp8
	{
		get
		{
			return IsWp8Platform();
		}
	}

	public static bool IsTizen
	{
		get
		{
			return IsTizenPlatform();
		}
	}

	public static bool IsMobile
	{
		get
		{
			return IsMobilePlatform();
		}
	}

	public static bool HasConnection
	{
		get
		{
			return CheckConnection();
		}
	}

	public static bool IsOnline
	{
		get
		{
			return CheckOnline();
		}
	}

	public static bool IsTablet
	{
		get
		{
			return IsTabletDevice();
		}
	}

	public static bool IsNarrowAspect
	{
		get
		{
			return IsNarrowAspectRatio();
		}
	}

	public static string PlatformName
	{
		get
		{
			return GetPlatformName();
		}
	}

	public static string PlatformDeviceType
	{
		get
		{
			return GetPlatformDeviceType();
		}
	}

	public static string DeviceId
	{
		get
		{
			return GetDeviceId();
		}
	}

	public static string PlatformDeviceId
	{
		get
		{
			return GetPlatformDeviceId();
		}
	}

	public static string UserIdentifier
	{
		get
		{
			return GetUserIdentifier();
		}
		set
		{
			SetUserIdentifier(value);
		}
	}

	public static string DeviceUniqueIdentifier
	{
		get
		{
			return GetDeviceUniqueId();
		}
	}

	public static string OsVersion
	{
		get
		{
			return GetOsVersion();
		}
	}

	public static long UtcOffsetSeconds
	{
		get
		{
			return GetUtcOffsetSeconds();
		}
	}

	public static bool IsPaidVersion
	{
		get
		{
			return IsPaidApp();
		}
	}

	public static string InstallGuid
	{
		get
		{
			return GetInstallGuid();
		}
	}

	public static int[] UnconfirmedLedgers
	{
		get
		{
			return GetUnconfirmedLedgerIDs();
		}
		set
		{
			set_UnconfirmedLedgerIDs(value);
		}
	}

	public static string DeviceModel
	{
		get
		{
			return GetDeviceModel();
		}
	}

	public static VersionContainer DataVersion
	{
		get
		{
			return GetDataVersion();
		}
	}

	public static VersionContainer Version
	{
		get
		{
			return GetVersion();
		}
	}

	private static string GetIdentifierSuffix()
	{
		return "-UP";
	}

	public static string MakeIdentifier(string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			return name + GetIdentifierSuffix();
		}
		return name;
	}

	public static bool IsDebug()
	{
		return false;
	}

	public static bool IsEnabled()
	{
		return true;
	}

	public static bool IsAvailable()
	{
		return true;
	}

	public static bool IsTestMode()
	{
		return false;
	}

	public static int GetNumOfDisplayModes()
	{
		return _numOfDisplayModes;
	}

	public static void ApplyResolution(int resolution)
	{
		Vector2 baseResolution = smallResolution;
		ScaleX = (float)deviceInfo.DisplayWidth / baseResolution.x;
		ScaleY = (float)deviceInfo.DisplayHeight / wideResolution.y;
		VirtualWidth = (float)deviceInfo.DisplayWidth / ScaleY;
		VirtualHeight = (float)deviceInfo.DisplayHeight / ScaleY;
	}

	public static int GetScreenWidth()
	{
		return Screen.width;
	}

	public static int GetScreenHeight()
	{
		return Screen.height;
	}

	public static void InitDeviceInfo()
	{
		deviceInfo.Id = SystemInfo.deviceModel;
		GameLog.Write(deviceInfo.Id);
		deviceInfo.IsEditor = Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor;
		deviceInfo.IsWindowsEditor = Application.platform == RuntimePlatform.WindowsEditor;
		deviceInfo.Os = GetOperatingSystemName();
		deviceInfo.OsName = SystemInfo.operatingSystem;
		deviceInfo.Locale = PreciseLocale.GetLanguageID();
		deviceInfo.CpuCount = SystemInfo.processorCount;
		deviceInfo.TotalRam = (int)((float)SystemInfo.systemMemorySize / 1024f);
		deviceInfo.FreeRam = (int)((float)(SystemInfo.systemMemorySize - Profiler.GetTotalAllocatedMemoryLong()) / 1024f);
		deviceInfo.UniqueId = SystemInfo.deviceUniqueIdentifier;
		deviceInfo.SocialPlayerId = SFSocial.GetInstance().GetPlayerId();
		if (IsMetroArmPlatform())
		{
			int length = deviceInfo.Id.IndexOf('_');
			deviceInfo.Id = deviceInfo.Id.Substring(0, length);
		}
	}

	private static string GetOperatingSystemName()
	{
		if (IsAndroidPlatform())
		{
			string operatingSystem = SystemInfo.operatingSystem;
			int num = operatingSystem.IndexOf("(");
			if (num == -1)
			{
				return SystemInfo.operatingSystem;
			}
			return operatingSystem.Substring(0, num - 1);
		}
		return SystemInfo.operatingSystem;
	}

	public static void LoadDevicesConfig(XmlDocument document)
	{
		ParseDevicesXml(document);
		PathType guiPathType = GetResolutionPathType();
		if (guiPathType == PathType.PATH_DEFAULT)
		{
			guiPathType = (IsHighResolution() ? PathType.PATH_BIG : PathType.PATH_SMALL);
		}
		SetPicturePaths(guiPathType);
		PathType locationPathType = GetLocationPathType();
		if (locationPathType == PathType.PATH_DEFAULT)
		{
			locationPathType = (IsHighResolution() ? PathType.PATH_BIG : PathType.PATH_SMALL);
		}
		SetInverseLocationScale((locationPathType != PathType.PATH_SMALL) ? 1 : 2);
		Vector2 baseResolution = smallResolution;
		ScaleX = (float)GetScreenWidth() / baseResolution.x;
		ScaleY = (float)GetScreenHeight() / wideResolution.y;
		VirtualWidth = (float)GetScreenWidth() / ScaleY;
		VirtualHeight = (float)GetScreenHeight() / ScaleY;
	}

	public static bool IsEditorPlatform()
	{
		return deviceInfo.IsEditor;
	}

	public static bool IsWindowsEditorPlatform()
	{
		return deviceInfo.IsWindowsEditor;
	}

	public static bool IsIosPlatform()
	{
		return Application.platform == RuntimePlatform.IPhonePlayer;
	}

	public static bool IsAndroidPlatform()
	{
		return Application.platform == RuntimePlatform.Android;
	}

	public static bool IsWindowsPlatform()
	{
		return Application.platform == RuntimePlatform.WindowsPlayer;
	}

	public static bool IsMacPlatform()
	{
		return Application.platform == RuntimePlatform.OSXPlayer;
	}

	public static bool IsMetroArmPlatform()
	{
		return Application.platform == RuntimePlatform.MetroPlayerARM;
	}

	public static bool IsWindowsStorePlatform()
	{
		return Application.platform == RuntimePlatform.MetroPlayerX86 || Application.platform == RuntimePlatform.MetroPlayerX64;
	}

	public static bool IsWp8Platform()
	{
		return Application.platform == RuntimePlatform.WP8Player;
	}

	public static bool IsTizenPlatform()
	{
		return Application.platform == RuntimePlatform.TizenPlayer;
	}

	public static bool IsMobilePlatform()
	{
		return Application.isMobilePlatform;
	}

	public static bool CheckConnection()
	{
		return true;
	}

	public static bool CheckOnline()
	{
		return CheckConnection();
	}

	public static bool IsTabletDevice()
	{
		return deviceInfo.IsTablet;
	}

	public static bool IsNarrowAspectRatio()
	{
		return (float)Screen.width / (float)Screen.height < 1.66f;
	}

	public static bool IsHighResolution()
	{
		return (float)deviceInfo.DisplayHeight > smallResolution.y;
	}

	public static PathType GetResolutionPathType()
	{
		return deviceInfo.GuiResolution;
	}

	public static PathType GetLocationPathType()
	{
		return deviceInfo.LocationResolution;
	}

	public static void SetTargetFrameRate(int value)
	{
	}

	public static string GetPlatformName()
	{
		if (IsWindowsStorePlatform())
		{
			return "winstore";
		}
		if (IsMetroArmPlatform() || IsWp8Platform())
		{
			return "win";
		}
		if (IsAndroidPlatform())
		{
			return "and";
		}
		if (IsIosPlatform())
		{
			return "ios";
		}
		if (IsEditorPlatform())
		{
			return "tst";
		}
		if (IsMacPlatform())
		{
			return "mac";
		}
		return "unk";
	}

	public static string GetPlatformDeviceType()
	{
		return string.Format("{0}{1}", GetPlatformName(), (!IsTabletDevice()) ? "_phone" : "_pad");
	}

	public static string GetDeviceId()
	{
		string text = DeviceIdBridge.GetNativeDeviceId();
		if (string.IsNullOrEmpty(text))
		{
			text = SystemInfo.deviceUniqueIdentifier;
		}
		return text;
	}

	public static string GetPlatformDeviceId()
	{
		return string.Format("{0}_{1}", GetPlatformName(), GetDeviceId());
	}

	public static DeviceInfo GetDeviceInfo()
	{
		return deviceInfo;
	}

	public static string PathTypeToString(PathType pathType)
	{
		switch (pathType)
		{
		case PathType.PATH_DEFAULT:
			return (!IsHighResolution()) ? "LOW" : "HIGH";
		case PathType.PATH_BIG:
			return "HIGH";
		case PathType.PATH_SMALL:
			return "LOW";
		default:
			return "DEFAULT";
		}
	}

	public static PathType ParsePathType(string name)
	{
		switch (name)
		{
		case "DEFAULT":
			return PathType.PATH_DEFAULT;
		case "LOW":
			return PathType.PATH_SMALL;
		case "HIGH":
			return PathType.PATH_BIG;
		default:
			GameLog.Error("ERROR: SystemProperties::getPathType - %s", name);
			return PathType.PATH_DEFAULT;
		}
	}

	public static string GetUserIdentifier()
	{
		string text = deviceInfo.CustomUniqueId;
		if (text == null)
		{
			text = SystemInfo.deviceUniqueIdentifier;
		}
		return MakeIdentifier(text);
	}

	public static void SetUserIdentifier(string value)
	{
		deviceInfo.CustomUniqueId = value;
	}

	public static void SetVersions(VersionContainer version, VersionContainer dataVersion)
	{
		deviceInfo.Version = version;
		deviceInfo.DataVersion = dataVersion;
	}

	public static string RefreshSocialUserId()
	{
		deviceInfo.SocialPlayerId = SFSocial.GetInstance().GetPlayerId();
		return deviceInfo.SocialPlayerId;
	}

	public static string GetDeviceUniqueId()
	{
		return SystemInfo.deviceUniqueIdentifier;
	}

	public static string GetOsVersion()
	{
		return deviceInfo.Os;
	}

	public static long GetUtcOffsetSeconds()
	{
		return (long)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalSeconds;
	}

	public static void Clear()
	{
		qualityConditions.Clear();
	}

	public static string GetQualityConditionName()
	{
		if (!deviceInfo.QualityCondition.IsNullOrEmpty())
		{
			return deviceInfo.QualityCondition;
		}
		if (IsWindowsStorePlatform())
		{
			return qualityConditions[0].get_Name();
		}
		foreach (QualityCondition item in qualityConditions)
		{
			if (item.IsSatisfied())
			{
				return item.get_Name();
			}
		}
		return string.Empty;
	}

	public static void SetInverseLocationScale(float value)
	{
		deviceInfo.InverseLocationScale = value;
	}

	private static void ParseDevicesXml(XmlDocument document)
	{
		Clear();
		DetectScreenResolution();
		deviceInfo.IsTablet = IsHighResolution();
		deviceInfo.GuiResolution = ParsePathType("DEFAULT");
		deviceInfo.LocationResolution = ParsePathType("DEFAULT");
		XmlElement xmlElement = document["Root"];
		if (xmlElement == null)
		{
			return;
		}
		XmlNode xmlNode = xmlElement["Config"];
		if (xmlNode != null)
		{
			float f = (float)Screen.width / Screen.dpi;
			float f2 = (float)Screen.height / Screen.dpi;
			float num = Mathf.Sqrt(Mathf.Pow(f, 2f) + Mathf.Pow(f2, 2f));
			float num2 = float.Parse(xmlNode["TabletDiagonal"].Attributes["Value"].Value, System.Globalization.CultureInfo.InvariantCulture);
			deviceInfo.IsTablet = num >= num2;
		}
		XmlElement xmlElement2 = xmlElement["Devices"];
		if (xmlElement2 != null)
		{
			DeviceInfoForcibly forcedDevice = new DeviceInfoForcibly();
			for (int i = 0; i < xmlElement2.ChildNodes.Count; i++)
			{
				if (xmlElement2.ChildNodes[i].Attributes == null)
				{
					continue;
				}
				if (((xmlElement2.Attributes["Forcibly"] != null && int.Parse(xmlElement2.Attributes["Forcibly"].Value) != 0) ? 1 : 0) > (false ? 1 : 0))
				{
					if (xmlElement2.ChildNodes[i].Attributes["Tablet"] != null)
					{
						forcedDevice.Tablet = xmlElement2.ChildNodes[i].Attributes["Tablet"].Value;
					}
					if (xmlElement2.ChildNodes[i].Attributes["Resolution"] != null)
					{
						forcedDevice.Resolution = xmlElement2.ChildNodes[i].Attributes["Resolution"].Value;
					}
					if (xmlElement2.ChildNodes[i].Attributes["LocationResolution"] != null)
					{
						forcedDevice.LocationResolution = xmlElement2.ChildNodes[i].Attributes["LocationResolution"].Value;
					}
					if (xmlElement2.ChildNodes[i].Attributes["QualityCondition"] != null)
					{
						forcedDevice.QualityCondition = xmlElement2.ChildNodes[i].Attributes["QualityCondition"].Value;
					}
				}
				if (deviceInfo.Id == xmlElement2.ChildNodes[i].Attributes["Name"].Value)
				{
					deviceInfo.IsTablet = xmlElement2.ChildNodes[i].Attributes["Tablet"] != null && int.Parse(xmlElement2.ChildNodes[i].Attributes["Tablet"].Value) > 0;
					string guiResolutionName = ((xmlElement2.Attributes["Resolution"] == null) ? "DEFAULT" : xmlNode.Attributes["Resolution"].Value);
					deviceInfo.GuiResolution = ParsePathType(guiResolutionName);
					string locationResolutionName = ((xmlElement2.Attributes["LocationResolution"] == null) ? "DEFAULT" : xmlNode.Attributes["LocationResolution"].Value);
					deviceInfo.LocationResolution = ParsePathType(locationResolutionName);
					deviceInfo.QualityCondition = ((xmlElement2.Attributes["QualityCondition"] == null) ? string.Empty : xmlNode.Attributes["QualityCondition"].Value);
				}
			}
			if (!forcedDevice.IsEmpty())
			{
				if (!string.IsNullOrEmpty(forcedDevice.Tablet))
				{
					deviceInfo.IsTablet = int.Parse(forcedDevice.Tablet) > 0;
				}
				if (forcedDevice.Resolution != string.Empty)
				{
					deviceInfo.GuiResolution = ParsePathType(forcedDevice.Resolution);
				}
				if (forcedDevice.LocationResolution != string.Empty)
				{
					deviceInfo.LocationResolution = ParsePathType(forcedDevice.LocationResolution);
				}
				if (forcedDevice.QualityCondition != string.Empty)
				{
					deviceInfo.QualityCondition = forcedDevice.QualityCondition;
				}
			}
		}
		ParseQualityConditions(xmlElement["QualityConditions"]);
	}

	private static void SetPicturePaths(PathType pathType)
	{
		switch (pathType)
		{
		case PathType.PATH_SMALL:
			PictureResolution = smallResolution;
			return;
		case PathType.PATH_BIG:
			PictureResolution = bigResolution;
			return;
		}
		GameLog.Error("ERROR: SystemProperties::setPicturePaths - %i", pathType);
		PictureResolution = smallResolution;
	}

	private static void DetectScreenResolution()
	{
		int width = Screen.currentResolution.width;
		int height = Screen.currentResolution.height;
		if (height < width)
		{
			deviceInfo.DisplayWidth = width;
			deviceInfo.DisplayHeight = height;
		}
		else
		{
			deviceInfo.DisplayWidth = height;
			deviceInfo.DisplayHeight = width;
		}
	}

	private static void ParseQualityConditions(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			QualityCondition item = new QualityCondition(childNode);
			qualityConditions.Add(item);
		}
	}

	public static bool IsPaidApp()
	{
		if (!isPaidCached.HasValue)
		{
			isPaidCached = Application.identifier == "com.nekki.shadowfight2.paid" || Application.identifier == "com.nekki.shadowfight.paid";
		}
		return isPaidCached.Value;
	}

	public static string GetInstallGuid()
	{
		if (IsIosPlatform())
		{
			return GetDeviceId();
		}
		if (IsAndroidPlatform())
		{
			if (!PlayerPrefs.HasKey("AndroidGUID"))
			{
				PlayerPrefs.SetString("AndroidGUID", Guid.NewGuid().ToString());
			}
			return PlayerPrefs.GetString("AndroidGUID");
		}
		if (IsEditorPlatform() || IsWindowsPlatform() || IsMacPlatform())
		{
			if (!PlayerPrefs.HasKey("EmulatorGUID"))
			{
				PlayerPrefs.SetString("EmulatorGUID", Guid.NewGuid().ToString());
			}
			return PlayerPrefs.GetString("EmulatorGUID");
		}
		return null;
	}

	public static int[] GetUnconfirmedLedgerIDs()
	{
		string text = PlayerPrefs.GetString("UnconfirmedLedgerIDs", null);
		List<int> list = new List<int>();
		if (!string.IsNullOrEmpty(text))
		{
			string[] array = text.Split(',');
			foreach (string s in array)
			{
				int result;
				if (int.TryParse(s, out result))
				{
					list.Add(result);
				}
			}
		}
		return list.ToArray();
	}

	public static void set_UnconfirmedLedgerIDs(int[] value)
	{
		PlayerPrefs.SetString("UnconfirmedLedgerIDs", string.Join(",", value.Select((int ledgerId) => ledgerId.ToString()).ToArray()));
	}

	public static string GetDeviceModel()
	{
		return SystemInfo.deviceModel;
	}

	public static VersionContainer GetDataVersion()
	{
		return deviceInfo.DataVersion;
	}

	public static VersionContainer GetVersion()
	{
		return deviceInfo.Version;
	}
}
