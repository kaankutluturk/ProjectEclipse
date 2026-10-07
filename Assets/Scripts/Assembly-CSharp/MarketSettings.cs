using System.Xml;

public class MarketSettings
{
	private bool isChinaMarket;

	private bool isJapanMarket;

	private bool isKoreaMarket;

	private bool isAmazonMarket;

	private bool isAmazonMobileMarket;

	private bool isSteamMarket;

	private bool isAndroidTvMarket;

	private bool isWinStoreMarket;

	public bool IsChinaMarket
	{
		get
		{
			return GetIsChinaMarket();
		}
	}

	public bool IsJapanMarket
	{
		get
		{
			return GetIsJapanMarket();
		}
	}

	public bool IsKoreaMarket
	{
		get
		{
			return GetIsKoreaMarket();
		}
	}

	public bool IsAmazonMarket
	{
		get
		{
			return GetIsAmazonMarket();
		}
	}

	public bool IsAmazonMobileMarket
	{
		get
		{
			return GetIsAmazonMobileMarket();
		}
	}

	public bool IsSteamMarket
	{
		get
		{
			return GetIsSteamMarket();
		}
	}

	public bool IsAndroidTvMarket
	{
		get
		{
			return GetIsAndroidTvMarket();
		}
	}

	public bool IsWinStoreMarket
	{
		get
		{
			return GetIsWinStoreMarket();
		}
	}

	public void Parse(XmlNode node)
	{
		isChinaMarket = node["China"].FirstAttribute().ParseBool();
		isJapanMarket = node["Japan"].FirstAttribute().ParseBool();
		isKoreaMarket = node["Korea"].FirstAttribute().ParseBool();
		isAmazonMarket = node["Amazon"].FirstAttribute().ParseBool();
		isAmazonMobileMarket = node["AmazonMobile"].FirstAttribute().ParseBool();
		isSteamMarket = node["Steam"].FirstAttribute().ParseBool();
		isAndroidTvMarket = node["AndroidTV"].FirstAttribute().ParseBool();
		isWinStoreMarket = node["WinStore"].FirstAttribute().ParseBool();
	}

	public bool GetIsChinaMarket()
	{
		return isChinaMarket;
	}

	public bool GetIsJapanMarket()
	{
		return isJapanMarket;
	}

	public bool GetIsKoreaMarket()
	{
		return isKoreaMarket;
	}

	public bool GetIsAmazonMarket()
	{
		return isAmazonMarket;
	}

	public bool GetIsAmazonMobileMarket()
	{
		return isAmazonMobileMarket;
	}

	public bool GetIsSteamMarket()
	{
		return isSteamMarket;
	}

	public bool GetIsAndroidTvMarket()
	{
		return isAndroidTvMarket;
	}

	public bool GetIsWinStoreMarket()
	{
		return isWinStoreMarket;
	}
}
