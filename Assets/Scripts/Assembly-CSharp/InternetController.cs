using System.Diagnostics;
using System.Xml;

public static class InternetController
{
	public class LikeUrlInfo
	{
		public string Url;

		public string AltUrl;
	}

	public enum PlatformType
	{
		ANDROID = 0,
		IOS = 1,
		WINPHONE = 2,
		PC = 3
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static string postPictureUrl;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static LikeUrlInfo likeUrls;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static string postLinkUrl;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static bool isPostAchievements;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static string musicStoreUrl;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static string rateUrl;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static string defaultConfigUrl;

	public static string PostPictureUrl
	{
		get
		{
			return GetPostPictureUrl();
		}
		private set
		{
			SetPostPictureUrl(value);
		}
	}

	public static LikeUrlInfo LikeUrls
	{
		get
		{
			return GetLikeUrls();
		}
		private set
		{
			SetLikeUrls(value);
		}
	}

	public static string PostLinkUrl
	{
		get
		{
			return GetPostLinkUrl();
		}
		private set
		{
			SetPostLinkUrl(value);
		}
	}

	public static bool PostAchievementsEnabled
	{
		get
		{
			return IsPostAchievementsEnabled();
		}
		private set
		{
			set_IsPostAchievements(value);
		}
	}

	public static string MusicStoreUrl
	{
		get
		{
			return GetMusicStoreUrl();
		}
		private set
		{
			SetMusicStoreUrl(value);
		}
	}

	public static string RateUrl
	{
		get
		{
			return GetRateUrl();
		}
		private set
		{
			SetRateUrl(value);
		}
	}

	public static string DefaultConfigUrl
	{
		get
		{
			return GetDefaultConfigUrl();
		}
		private set
		{
			SetDefaultConfigUrl(value);
		}
	}

	public static string GetPostPictureUrl()
	{
		return postPictureUrl;
	}

	private static void SetPostPictureUrl(string value)
	{
		postPictureUrl = value;
	}

	public static LikeUrlInfo GetLikeUrls()
	{
		return likeUrls;
	}

	private static void SetLikeUrls(LikeUrlInfo value)
	{
		likeUrls = value;
	}

	public static string GetPostLinkUrl()
	{
		return postLinkUrl;
	}

	private static void SetPostLinkUrl(string value)
	{
		postLinkUrl = value;
	}

	public static bool IsPostAchievementsEnabled()
	{
		return isPostAchievements;
	}

	private static void set_IsPostAchievements(bool value)
	{
		isPostAchievements = value;
	}

	public static string GetMusicStoreUrl()
	{
		return musicStoreUrl;
	}

	private static void SetMusicStoreUrl(string value)
	{
		musicStoreUrl = value;
	}

	public static string GetRateUrl()
	{
		return rateUrl;
	}

	private static void SetRateUrl(string value)
	{
		rateUrl = value;
	}

	public static string GetDefaultConfigUrl()
	{
		return defaultConfigUrl;
	}

	private static void SetDefaultConfigUrl(string value)
	{
		defaultConfigUrl = value;
	}

	public static void Parse(XmlNode node)
	{
		SetPostPictureUrl(node["FBPostPicture"].Attributes["Url"].GetStringOrDefault(string.Empty));
		XmlNode hKPPBKPJOEO = node["Android"];
		PlatformType jGJNNAHDPBA = PlatformType.ANDROID;
		ParsePlatformNode(hKPPBKPJOEO, jGJNNAHDPBA);
		ParseServer(node["Server"]);
	}

	private static void ParsePlatformNode(XmlNode node, PlatformType JGJNNAHDPBA)
	{
		SetLikeUrls(new LikeUrlInfo());
		if (node["FBLikeUrl"] != null)
		{
			GetLikeUrls().Url = node["FBLikeUrl"].Attributes["Url"].GetStringOrDefault(string.Empty);
			GetLikeUrls().AltUrl = node["FBLikeUrl"].Attributes["AltUrl"].GetStringOrDefault(string.Empty);
		}
		if (node["FBPostLink"] != null)
		{
			if (AssemblyController.GetMarket().GetIsAmazonMobileMarket())
			{
				SetPostLinkUrl(node["FBPostLink"].Attributes["Amazon"].GetStringOrDefault(string.Empty));
			}
			else if (SystemProperties.IsAndroidPlatform())
			{
				SetPostLinkUrl(node["FBPostLink"].Attributes["PlayMarket"].GetStringOrDefault(string.Empty));
			}
			else
			{
				SetPostLinkUrl(node["FBPostLink"].Attributes["Url"].GetStringOrDefault(string.Empty));
			}
		}
		if (node["PostAchievements"] != null)
		{
			set_IsPostAchievements(node["PostAchievements"].Attributes["Value"].ParseBool());
		}
		if (node["MusicStore"] != null)
		{
			if (AssemblyController.GetMarket().GetIsAmazonMobileMarket())
			{
				SetMusicStoreUrl(node["MusicStore"].Attributes["Amazon"].GetStringOrDefault(string.Empty));
			}
			else
			{
				SetMusicStoreUrl(node["MusicStore"].Attributes["Url"].GetStringOrDefault(string.Empty));
			}
		}
		string name = "Url";
		if (JGJNNAHDPBA == PlatformType.ANDROID)
		{
			name = ((!AssemblyController.GetMarket().GetIsChinaMarket()) ? "PlayMarket" : "China360");
			name = ((!AssemblyController.GetMarket().GetIsAmazonMobileMarket()) ? name : "Amazon");
		}
		SetRateUrl(node["RateUrl"].Attributes[name].GetStringOrDefault(string.Empty));
	}

	private static void ParseServer(XmlNode node)
	{
		SetDefaultConfigUrl(node["DefaultConfigUrl"].Attributes["Url"].GetStringOrDefault(string.Empty));
	}
}
