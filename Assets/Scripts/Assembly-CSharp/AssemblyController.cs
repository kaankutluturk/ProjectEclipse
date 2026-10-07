using System.Diagnostics;
using System.Xml;

public static class AssemblyController
{
	public const int DefaultTimeout30000Ms = 30000;

	public const int DefaultTimeout3000Ms = 3000;

	public const int DefaultSocialAuthorizeTimeoutMs = 15000;

	public const int DefaultTimeout10000Ms = 10000;

	public const int DefaultTimeout5000MsA = 5000;

	public const int DefaultTimeout5000MsB = 5000;

	public const int DefaultTimeout5000MsC = 5000;

	public const int DefaultTimeout5000MsD = 5000;

	public const int DefaultTimeout5000MsE = 5000;

	public const int DefaultTimeout8000Ms = 8000;

	public const int DefaultTimeout5000MsF = 5000;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool debugStatistics;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool cacheTexturesLog;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool showIntro;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool showController;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool debugOverlayEnabled;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool showPvp;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool aiEnabled;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool skipContentDownload;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool skipPayment;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool etcEnabled;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool showSensitiveArea;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool showTimeResults;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static float controllerPrimaryAngle;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static float controllerGripRelativeRadius;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool enableNotifications;

	private static MarketSettings market = new MarketSettings();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool showCrashButtons;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool gamepadEnabled;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool useLocalRaidConfig;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static int timeout1Ms;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static int timeout2Ms;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static int timeout3Ms;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static int timeout4Ms;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static int socialAuthorizeTimeoutMs;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static int timeout6Ms;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static int timeout7Ms;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static int timeout8Ms;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static int timeout9Ms;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static int timeout10Ms;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static int timeout11Ms;

	public static bool DebugStatistics
	{
		get
		{
			return GetDebugStatistics();
		}
		private set
		{
			SetDebugStatistics(value);
		}
	}

	public static bool CacheTexturesLog
	{
		get
		{
			return GetCacheTexturesLog();
		}
		private set
		{
			SetCacheTexturesLog(value);
		}
	}

	public static bool ShowIntro
	{
		get
		{
			return GetShowIntro();
		}
		private set
		{
			SetShowIntro(value);
		}
	}

	public static bool ShowController
	{
		get
		{
			return GetShowController();
		}
		private set
		{
			SetShowController(value);
		}
	}

	public static bool DebugOverlayEnabled
	{
		get
		{
			return GetDebugOverlayEnabled();
		}
		private set
		{
			SetDebugOverlayEnabled(value);
		}
	}

	public static bool ShowPvp
	{
		get
		{
			return GetShowPvp();
		}
		private set
		{
			SetShowPvp(value);
		}
	}

	public static bool AiEnabled
	{
		get
		{
			return GetAiEnabled();
		}
		private set
		{
			SetAiEnabled(value);
		}
	}

	public static bool SkipContentDownload
	{
		get
		{
			return GetSkipContentDownload();
		}
		private set
		{
			SetSkipContentDownload(value);
		}
	}

	public static bool SkipPayment
	{
		get
		{
			return GetSkipPayment();
		}
		private set
		{
			SetSkipPayment(value);
		}
	}

	public static bool EtcEnabled
	{
		get
		{
			return GetEtcEnabled();
		}
		private set
		{
			SetEtcEnabled(value);
		}
	}

	public static bool ShowSensitiveArea
	{
		get
		{
			return GetShowSensitiveArea();
		}
		private set
		{
			SetShowSensitiveArea(value);
		}
	}

	public static bool ShowTimeResults
	{
		get
		{
			return GetShowTimeResults();
		}
		private set
		{
			SetShowTimeResults(value);
		}
	}

	public static float ControllerPrimaryAngle
	{
		get
		{
			return GetControllerPrimaryAngle();
		}
		private set
		{
			SetControllerPrimaryAngle(value);
		}
	}

	public static float ControllerGripRelativeRadius
	{
		get
		{
			return GetControllerGripRelativeRadius();
		}
		private set
		{
			SetControllerGripRelativeRadius(value);
		}
	}

	public static bool EnableNotifications
	{
		get
		{
			return GetEnableNotifications();
		}
		private set
		{
			SetEnableNotifications(value);
		}
	}

	public static MarketSettings Market
	{
		get
		{
			return GetMarket();
		}
	}

	public static bool ShowCrashButtons
	{
		get
		{
			return GetShowCrashButtons();
		}
		private set
		{
			SetShowCrashButtons(value);
		}
	}

	public static bool GamepadEnabled
	{
		get
		{
			return GetGamepadEnabled();
		}
		private set
		{
			SetGamepadEnabled(value);
		}
	}

	public static bool UseLocalRaidConfig
	{
		get
		{
			return GetUseLocalRaidConfig();
		}
		private set
		{
			SetUseLocalRaidConfig(value);
		}
	}

	public static int Timeout1Ms
	{
		get
		{
			return GetTimeout1Ms();
		}
		private set
		{
			SetTimeout1Ms(value);
		}
	}

	public static int Timeout2Ms
	{
		get
		{
			return GetTimeout2Ms();
		}
		private set
		{
			SetTimeout2Ms(value);
		}
	}

	public static int Timeout3Ms
	{
		get
		{
			return GetTimeout3Ms();
		}
		private set
		{
			SetTimeout3Ms(value);
		}
	}

	public static int Timeout4Ms
	{
		get
		{
			return GetTimeout4Ms();
		}
		private set
		{
			SetTimeout4Ms(value);
		}
	}

	public static int SocialAuthorizeTimeoutMs
	{
		get
		{
			return GetSocialAuthorizeTimeoutMs();
		}
		private set
		{
			SetSocialAuthorizeTimeoutMs(value);
		}
	}

	public static int Timeout6Ms
	{
		get
		{
			return GetTimeout6Ms();
		}
		private set
		{
			SetTimeout6Ms(value);
		}
	}

	public static int Timeout7Ms
	{
		get
		{
			return GetTimeout7Ms();
		}
		private set
		{
			SetTimeout7Ms(value);
		}
	}

	public static int Timeout8Ms
	{
		get
		{
			return GetTimeout8Ms();
		}
		private set
		{
			SetTimeout8Ms(value);
		}
	}

	public static int Timeout9Ms
	{
		get
		{
			return GetTimeout9Ms();
		}
		private set
		{
			SetTimeout9Ms(value);
		}
	}

	public static int Timeout10Ms
	{
		get
		{
			return GetTimeout10Ms();
		}
		private set
		{
			SetTimeout10Ms(value);
		}
	}

	public static int Timeout11Ms
	{
		get
		{
			return GetTimeout11Ms();
		}
		private set
		{
			SetTimeout11Ms(value);
		}
	}

	public static void Parse(XmlNode node)
	{
		SetDebugStatistics(node["DebugStatistics"].FirstAttribute().ParseBool());
		SetUseLocalRaidConfig(node["UseLocalRaidConfig"].FirstAttribute().ParseBool());
		AiData.TacticsEnabled = node["Tactics"].FirstAttribute().ParseBool();
		SetCacheTexturesLog(node["CacheTexturesLog"].FirstAttribute().ParseBool());
		SetShowIntro(node["ShowIntro"].FirstAttribute().ParseBool(true));
		SetShowController(node["ShowController"].FirstAttribute().ParseBool(true));
		SetShowPvp(node["ShowPVP"].FirstAttribute().ParseBool());
		SetAiEnabled(node["AiEnabled"].FirstAttribute().ParseBool(true));
		SetSkipContentDownload(node["SkipContentDownload"].FirstAttribute().ParseBool());
		SetSkipPayment(node["SkipPayment"].FirstAttribute().ParseBool());
		SetShowSensitiveArea(node["ShowSensitiveArea"].FirstAttribute().ParseBool());
		SetControllerPrimaryAngle(node["ControllerPrimaryAngle"].FirstAttribute().ParseFloat());
		SetControllerGripRelativeRadius(node["ControllerGripRelativeRadius"].FirstAttribute().ParseFloat());
		SetEtcEnabled(node["ETCEnabled"].FirstAttribute().ParseBool());
		SetEnableNotifications(node["EnableNotifications"].FirstAttribute().ParseBool());
		if (node["Market"] != null)
		{
			GetMarket().Parse(node["Market"]);
		}
		SetShowCrashButtons(node["ShowCrashButtons"].FirstAttribute().ParseBool());
		SetGamepadEnabled(node["Gamepad"].FirstAttribute().ParseBool());
		XmlElement xmlElement = node["SocialAuthorizeTimeout"];
		SetSocialAuthorizeTimeoutMs((xmlElement == null) ? 15000 : xmlElement.Attributes["Value"].ParseInt());
		SetShowTimeResults(node["ShowTimeResults"].FirstAttribute().ParseBool());
	}

	public static bool GetDebugStatistics()
	{
		return debugStatistics;
	}

	private static void SetDebugStatistics(bool value)
	{
		debugStatistics = value;
	}

	public static bool GetCacheTexturesLog()
	{
		return cacheTexturesLog;
	}

	private static void SetCacheTexturesLog(bool value)
	{
		cacheTexturesLog = value;
	}

	public static bool GetShowIntro()
	{
		return showIntro;
	}

	private static void SetShowIntro(bool value)
	{
		showIntro = value;
	}

	public static bool GetShowController()
	{
		return showController;
	}

	private static void SetShowController(bool value)
	{
		showController = value;
	}

	public static bool GetDebugOverlayEnabled()
	{
		return debugOverlayEnabled;
	}

	private static void SetDebugOverlayEnabled(bool value)
	{
		debugOverlayEnabled = value;
	}

	public static bool GetShowPvp()
	{
		return showPvp;
	}

	private static void SetShowPvp(bool value)
	{
		showPvp = value;
	}

	public static bool GetAiEnabled()
	{
		return aiEnabled;
	}

	private static void SetAiEnabled(bool value)
	{
		aiEnabled = value;
	}

	public static bool GetSkipContentDownload()
	{
		return skipContentDownload;
	}

	private static void SetSkipContentDownload(bool value)
	{
		skipContentDownload = value;
	}

	public static bool GetSkipPayment()
	{
		return skipPayment;
	}

	private static void SetSkipPayment(bool value)
	{
		skipPayment = value;
	}

	public static bool GetEtcEnabled()
	{
		return etcEnabled;
	}

	private static void SetEtcEnabled(bool value)
	{
		etcEnabled = value;
	}

	public static bool GetShowSensitiveArea()
	{
		return showSensitiveArea;
	}

	private static void SetShowSensitiveArea(bool value)
	{
		showSensitiveArea = value;
	}

	public static bool GetShowTimeResults()
	{
		return showTimeResults;
	}

	private static void SetShowTimeResults(bool value)
	{
		showTimeResults = value;
	}

	public static float GetControllerPrimaryAngle()
	{
		return controllerPrimaryAngle;
	}

	private static void SetControllerPrimaryAngle(float value)
	{
		controllerPrimaryAngle = value;
	}

	public static float GetControllerGripRelativeRadius()
	{
		return controllerGripRelativeRadius;
	}

	private static void SetControllerGripRelativeRadius(float value)
	{
		controllerGripRelativeRadius = value;
	}

	public static bool GetEnableNotifications()
	{
		return enableNotifications;
	}

	private static void SetEnableNotifications(bool value)
	{
		enableNotifications = value;
	}

	public static MarketSettings GetMarket()
	{
		return market;
	}

	public static bool GetShowCrashButtons()
	{
		return showCrashButtons;
	}

	private static void SetShowCrashButtons(bool value)
	{
		showCrashButtons = value;
	}

	public static bool GetGamepadEnabled()
	{
		return gamepadEnabled;
	}

	private static void SetGamepadEnabled(bool value)
	{
		gamepadEnabled = value;
	}

	public static bool GetUseLocalRaidConfig()
	{
		return useLocalRaidConfig;
	}

	private static void SetUseLocalRaidConfig(bool value)
	{
		useLocalRaidConfig = value;
	}

	public static int GetTimeout1Ms()
	{
		return timeout1Ms;
	}

	private static void SetTimeout1Ms(int value)
	{
		timeout1Ms = value;
	}

	public static int GetTimeout2Ms()
	{
		return timeout2Ms;
	}

	private static void SetTimeout2Ms(int value)
	{
		timeout2Ms = value;
	}

	public static int GetTimeout3Ms()
	{
		return timeout3Ms;
	}

	private static void SetTimeout3Ms(int value)
	{
		timeout3Ms = value;
	}

	public static int GetTimeout4Ms()
	{
		return timeout4Ms;
	}

	private static void SetTimeout4Ms(int value)
	{
		timeout4Ms = value;
	}

	public static int GetSocialAuthorizeTimeoutMs()
	{
		return socialAuthorizeTimeoutMs;
	}

	private static void SetSocialAuthorizeTimeoutMs(int value)
	{
		socialAuthorizeTimeoutMs = value;
	}

	public static int GetTimeout6Ms()
	{
		return timeout6Ms;
	}

	private static void SetTimeout6Ms(int value)
	{
		timeout6Ms = value;
	}

	public static int GetTimeout7Ms()
	{
		return timeout7Ms;
	}

	private static void SetTimeout7Ms(int value)
	{
		timeout7Ms = value;
	}

	public static int GetTimeout8Ms()
	{
		return timeout8Ms;
	}

	private static void SetTimeout8Ms(int value)
	{
		timeout8Ms = value;
	}

	public static int GetTimeout9Ms()
	{
		return timeout9Ms;
	}

	private static void SetTimeout9Ms(int value)
	{
		timeout9Ms = value;
	}

	public static int GetTimeout10Ms()
	{
		return timeout10Ms;
	}

	private static void SetTimeout10Ms(int value)
	{
		timeout10Ms = value;
	}

	public static int GetTimeout11Ms()
	{
		return timeout11Ms;
	}

	private static void SetTimeout11Ms(int value)
	{
		timeout11Ms = value;
	}
}
