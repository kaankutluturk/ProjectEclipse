using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Nekki.SF2.Core.Network;
using Nekki.Utils;

public class GeneralConfig
{
	private const string ConfigCdnFileName = "/config_cdn.xml";

	public static News CurrentNews = new News();

	public static Packs DownloadPacks = new Packs();

	public static PricesDataContainer Prices = new PricesDataContainer();

	private Action<bool> _Callback;

	private Action<object> progressCallback;

	private List<DownloadPack> pendingItems;

	private int downloadTimeout;

	private int reservedParam;

	private XmlDocument configDocument;

	private bool callbackResult;

	private string platformName;

	private bool isConfigValid = true;

	private bool isVersionSupported = true;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private LedgerSettings ledgerSettings;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private VerificationSettings verificationSettings;

	private static GeneralConfig _Instance = null;

	public bool IsConfigValid
	{
		get
		{
			return GetIsConfigValid();
		}
	}

	public bool IsVersionSupported
	{
		get
		{
			return GetIsVersionSupported();
		}
	}

	public LedgerSettings LedgerConfig
	{
		get
		{
			return GetLedgerSettings();
		}
		private set
		{
			SetLedgerSettings(value);
		}
	}

	public VerificationSettings VerificationConfig
	{
		get
		{
			return GetVerificationSettings();
		}
		private set
		{
			SetVerificationSettings(value);
		}
	}

	public static GeneralConfig Instance
	{
		get
		{
			return GetInstance();
		}
	}

	public bool GetIsConfigValid()
	{
		return isConfigValid;
	}

	public bool GetIsVersionSupported()
	{
		return isVersionSupported;
	}

	public LedgerSettings GetLedgerSettings()
	{
		return ledgerSettings;
	}

	private void SetLedgerSettings(LedgerSettings value)
	{
		ledgerSettings = value;
	}

	public VerificationSettings GetVerificationSettings()
	{
		return verificationSettings;
	}

	private void SetVerificationSettings(VerificationSettings value)
	{
		verificationSettings = value;
	}

	public static GeneralConfig GetInstance()
	{
		if (_Instance == null)
		{
			_Instance = new GeneralConfig();
		}
		return _Instance;
	}

	public void LoadConfig(string path, Action<bool> callback, Action<object> onProgress, List<DownloadPack> packList, int timeoutSeconds, int reserved)
	{
		_Callback = callback;
		progressCallback = onProgress;
		pendingItems = packList;
		downloadTimeout = timeoutSeconds;
		reservedParam = reserved;
		if (string.IsNullOrEmpty(path))
		{
			InvokeCallback();
			return;
		}
		string arg = NekkiMath.randomInt(1000000).ToString();
		string requestUrl = string.Format("{0}?{1}", path, arg);
		ServerProvider.get_Instance().DownloadFile(requestUrl, OnLoadConfig, null, downloadTimeout);
	}

	private void OnLoadConfig(byte[] data, string error, string sourceUrl)
	{
		if (string.IsNullOrEmpty(error) && data != null)
		{
			File.WriteAllBytes(SF2Paths.GetWritableGameDataPath() + "/config_cdn.xml", data);
		}
		else
		{
			if (error == "offline build")
			{
				UnityEngine.Debug.LogWarning("[Config] Offline build; using the recovered local config.");
			}
			else
			{
				GameLog.Error("[Config]: failed to download config because " + error);
			}
		}
		Parse(true);
	}

	private bool Parse(bool loadFromFile)
	{
		bool flag = true;
		if (loadFromFile)
		{
			configDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetWritableGameDataPath(), "/config_cdn.xml", XmlUtils.XmlSourceMode.ForcedExternal);
			if (configDocument == null)
			{
				configDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath() + "/config_cdn.xml", string.Empty);
			}
		}
		if (flag)
		{
			ParseXml();
		}
		else
		{
			isConfigValid = false;
			InvokeCallback();
		}
		configDocument = null;
		return flag;
	}

	private void ParseXml()
	{
		if (configDocument == null)
		{
			GameLog.Error("GeneralConfig.ParseXML: _DocConfig is null");
			return;
		}
		XmlNode xmlNode = configDocument["data"];
		XmlNode xmlNode2 = xmlNode["platform"];
		XmlNode xmlNode3 = xmlNode["versions"];
		XmlNode xmlNode4 = xmlNode["settings"];
		XmlNode xmlNode5 = xmlNode["news"];
		XmlNode xmlNode6 = xmlNode["price"];
		if (xmlNode2 != null)
		{
			ParsePlatform(xmlNode2);
		}
		if (xmlNode3 != null)
		{
			ParseVersions(xmlNode3);
		}
		if (xmlNode4 != null)
		{
			ParseSettings(xmlNode4);
		}
		if (xmlNode5 != null)
		{
			ParseNews(xmlNode5);
		}
		ParseDownloads(xmlNode);
		if (xmlNode6 != null)
		{
			ParsePrices(xmlNode6);
		}
		InvokeCallback();
	}

	private void ParsePlatform(XmlNode node)
	{
		platformName = "unknown";
		foreach (XmlNode childNode in node.ChildNodes)
		{
			int platformId = childNode.Attributes["PlatformID"].ParseInt();
			if (CheckPlatform(platformId))
			{
				platformName = childNode.Attributes["Name"].GetStringOrDefault(platformName);
			}
		}
	}

	private void ParseVersions(XmlNode node)
	{
		bool versionSupported = false;
		VersionContainer currentVersion = SystemProperties.GetVersion();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			int platformId = childNode.Attributes["PlatformID"].ParseInt();
			if (CheckPlatform(platformId))
			{
				VersionContainer configVersion = new VersionContainer();
				configVersion.SetVersion(childNode.Attributes["Version"].GetStringOrDefault());
				if (VersionContainer.IsGreater(configVersion, currentVersion))
				{
					versionSupported = true;
					break;
				}
			}
		}
		isVersionSupported = versionSupported;
	}

	private void ParseSettings(XmlNode node)
	{
		XmlNode urlNode = node["time"];
		XmlNode dumpUrlNode = node["dumps"];
		XmlNode serverUrlNode = node["server"];
		KeyValuePair<string, string> urlPair = ParseUrlPair(urlNode);
		KeyValuePair<string, string> dumpUrlPair = ParseUrlPair(dumpUrlNode);
		KeyValuePair<string, string> serverUrlPair = ParseUrlPair(serverUrlNode);
		if (!IsEmptyPair(urlPair))
		{
			ServerProvider.set_TimeServerURL(urlPair.Value);
		}
		if (!IsEmptyPair(dumpUrlPair))
		{
			ServerProvider.set_DumpPutURL(dumpUrlPair.Key);
			ServerProvider.set_DumpGetURL(dumpUrlPair.Value);
		}
		if (!IsEmptyPair(serverUrlPair))
		{
			ServerProvider.set_PutURL(serverUrlPair.Key);
			ServerProvider.set_GetURL(serverUrlPair.Value);
		}
		XmlNode verificationNode = node["verification"];
		ParseVerification(verificationNode);
		XmlNode ledgerNode = node["ledger"];
		ParseLedger(ledgerNode);
	}

	private void ParseVerification(XmlNode node)
	{
		SetVerificationSettings(new VerificationSettings(node.Attributes["Url"].GetStringOrDefault(string.Empty), node.Attributes["Timeout"].ParseInt(), node.Attributes["MaxRetry"].ParseInt(), node.Attributes["Frequency"].ParseInt()));
	}

	private void ParseLedger(XmlNode node)
	{
		SetLedgerSettings(new LedgerSettings(node.Attributes["Url"].GetStringOrDefault(string.Empty), node.Attributes["Timeout"].ParseInt(), node.Attributes["MaxRetry"].ParseInt()));
	}

	private void ParseDownloads(XmlNode node)
	{
		DownloadPacks.Reset();
		XmlNode xmlNode = node["packs"];
		XmlNode xmlNode2 = node["fonts"];
		XmlNode xmlNode3 = node["video"];
		if (xmlNode != null)
		{
			ParsePackGroup(xmlNode);
		}
		if (xmlNode2 != null)
		{
			ParsePackGroup(xmlNode2);
		}
		if (xmlNode3 != null)
		{
			ParsePackGroup(xmlNode3);
		}
	}

	private void ParsePackGroup(XmlNode node)
	{
		Dictionary<string, List<XmlNode>> dictionary = new Dictionary<string, List<XmlNode>>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string key = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			if (!dictionary.ContainsKey(key))
			{
				dictionary[key] = new List<XmlNode>();
			}
			dictionary[key].Add(childNode);
		}
		foreach (KeyValuePair<string, List<XmlNode>> item in dictionary)
		{
			string key2 = item.Key;
			List<XmlNode> value = item.Value;
			XmlDocument xmlDocument = new XmlDocument();
			XmlNode xmlNode2 = xmlDocument.CreateNode(XmlNodeType.Element, "TmpNode", null);
			foreach (XmlNode item2 in value)
			{
				int platformId = item2.Attributes["PlatformID"].ParseInt();
				if (CheckPlatform(platformId))
				{
					xmlNode2.AppendImportedClone(item2);
				}
			}
			XmlNode xmlNode3 = ClosestVersion(xmlNode2);
			string text = ((xmlNode3 == null) ? null : xmlNode3.Attributes["Url"].GetStringOrDefault());
			string size = ((xmlNode3 == null) ? null : xmlNode3.Attributes["Size"].GetStringOrDefault());
			bool reload = xmlNode3 != null && xmlNode3.Attributes["Reload"].ParseBool();
			string hash = ((xmlNode3 == null) ? null : xmlNode3.Attributes["Hash"].GetStringOrDefault());
			bool attach = xmlNode3 != null && xmlNode3.Attributes["Attach"].ParseBool();
			if (!string.IsNullOrEmpty(text))
			{
				DownloadPacks.AddPack(key2, text, size, reload, hash, attach);
			}
		}
	}

	private XmlNode ClosestVersion(XmlNode nodes, bool exactMatch = false, bool checkPlatform = false)
	{
		XmlNode result = null;
		VersionContainer bestVersion = new VersionContainer();
		VersionContainer currentVersion = SystemProperties.GetVersion();
		foreach (XmlNode childNode in nodes.ChildNodes)
		{
			if (checkPlatform)
			{
				int platformId = childNode.Attributes["PlatformID"].ParseInt();
				if (!CheckPlatform(platformId))
				{
					continue;
				}
			}
			VersionContainer candidateVersion = new VersionContainer();
			candidateVersion.SetVersion(childNode.Attributes["MinVersion"].GetStringOrDefault());
			if (!exactMatch)
			{
				if (VersionContainer.IsGreaterOrEqual(candidateVersion, bestVersion) && VersionContainer.IsLessOrEqual(candidateVersion, currentVersion))
				{
					bestVersion = candidateVersion;
					result = childNode;
				}
			}
			else if (VersionContainer.IsEqual(currentVersion, candidateVersion))
			{
				result = childNode;
				break;
			}
		}
		return result;
	}

	private void ParseNews(XmlNode node)
	{
		if (!ParseNewsForLocale(node, SystemProperties.GetDeviceInfo().Locale) && !ParseNewsForLocale(node, SystemProperties.GetDeviceInfo().GetLanguage()) && !ParseNewsForLocale(node, "Other"))
		{
		}
	}

	private bool ParseNewsForLocale(XmlNode node, string locale)
	{
		bool result = false;
		VersionContainer currentVersion = SystemProperties.GetVersion();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name != "item")
			{
				continue;
			}
			int platformId = childNode.Attributes["PlatformID"].ParseInt();
			if (!CheckPlatform(platformId) || !IsOkLocale(childNode.Attributes["LangID"].GetStringOrDefault(), locale))
			{
				continue;
			}
			VersionContainer minVersion = new VersionContainer();
			minVersion.SetVersion(childNode.Attributes["MinVersion"].GetStringOrDefault());
			if (VersionContainer.IsLess(currentVersion, minVersion))
			{
				continue;
			}
			long num = childNode.Attributes["StartDate"].ParseLong(0L);
			num += SystemProperties.GetUtcOffsetSeconds();
			if (GlobalTimer.get_LocalTimeUTC() >= num)
			{
				long num2 = childNode.Attributes["EndDate"].ParseLong(-1L);
				num2 += SystemProperties.GetUtcOffsetSeconds();
				if (num2 <= 0 || num2 >= GlobalTimer.get_LocalTimeUTC())
				{
					string title = childNode.Attributes["Title"].GetStringOrDefault(string.Empty);
					string name = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
					string url = childNode.Attributes["Url"].GetStringOrDefault(string.Empty);
					string imageUrl = childNode.Attributes["ImageURL"].GetStringOrDefault(string.Empty);
					int id = childNode.Attributes["ID"].ParseInt();
					bool goShop = childNode.Attributes["GoShop"].ParseBool();
					string redirectShop = childNode.Attributes["RedirectShop"].GetStringOrDefault(string.Empty);
					string spenderTypeId = childNode.Attributes["SpenderTypeID"].GetStringOrDefault(string.Empty);
					bool isActive = childNode.Attributes["Active"].ParseBool();
					List<NewsButton> buttons = ParseNewsButtons(childNode);
					CurrentNews.AddOrReplaceItem(name, url, imageUrl, id, isActive, num2, buttons, title, goShop, redirectShop, spenderTypeId);
					result = true;
				}
			}
		}
		return result;
	}

	private List<NewsButton> ParseNewsButtons(XmlNode node)
	{
		List<NewsButton> list = new List<NewsButton>();
		foreach (XmlNode item in node)
		{
			NewsButton button = new NewsButton();
			button.LabelAliasName = item.Attributes["Text"].GetStringOrDefault(string.Empty);
			button.Color = LabelButton.GetBtnColor(item.Attributes["Color"].GetStringOrDefault(string.Empty));
			button.Url = item.Attributes["Url"].GetStringOrDefault(string.Empty);
			button.GoShop = item.Attributes["GoShop"].ParseBool();
			button.BuyItem = item.Attributes["BuyItem"].ParseBool();
			button.RedirectShop = item.Attributes["RedirectShop"].GetStringOrDefault(string.Empty);
			list.Add(button);
		}
		return list;
	}

	private bool IsEmptyPair(KeyValuePair<string, string> pair)
	{
		return string.IsNullOrEmpty(pair.Key) && string.IsNullOrEmpty(pair.Value);
	}

	private KeyValuePair<string, string> ParseUrlPair(XmlNode node)
	{
		string key = null;
		string value = null;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			int platformId = childNode.Attributes["PlatformID"].ParseInt();
			if (CheckPlatform(platformId))
			{
				key = childNode.Attributes["PutUrl"].GetStringOrDefault(string.Empty);
				value = childNode.Attributes["GetUrl"].GetStringOrDefault(string.Empty);
			}
		}
		return new KeyValuePair<string, string>(key, value);
	}

	private void ParsePrices(XmlNode node)
	{
		if (node.ChildNodes.Count != 0)
		{
			Prices.GetPrices().Clear();
			ParsePricesForLocale(node, SystemProperties.GetDeviceInfo().Locale);
			ParsePricesForLocale(node, SystemProperties.GetDeviceInfo().GetLanguage());
		}
	}

	private void ParsePricesForLocale(XmlNode node, string locale)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (!CheckPlatform(childNode.Attributes["PlatformID"].ParseInt()) || !childNode.Attributes["MobileOperator"].Empty())
			{
				continue;
			}
			PricesData priceData = ParsePriceNode(childNode);
			if (!string.IsNullOrEmpty(priceData.Locale) && !IsOkLocale(priceData.Locale, locale))
			{
				continue;
			}
			PricesData basePrices = Prices.FindByProductId(priceData.ProductId);
			if (basePrices == null)
			{
				Prices.GetPrices().Add(priceData);
				continue;
			}
			bool flag = priceData.GroupId == basePrices.GroupId;
			bool flag2 = priceData.SpenderTypeId == basePrices.SpenderTypeId || string.IsNullOrEmpty(basePrices.SpenderTypeId);
			if (flag && flag2)
			{
				if (string.IsNullOrEmpty(basePrices.Locale))
				{
					Prices.GetPrices().Remove(basePrices);
					Prices.GetPrices().Add(priceData);
				}
			}
			else
			{
				Prices.GetPrices().Add(priceData);
			}
		}
	}

	private PricesData ParsePriceNode(XmlNode node)
	{
		PricesData priceData = new PricesData();
		priceData.ProductId = node.Attributes["ProductID"].GetStringOrDefault();
		priceData.NewProductId = node.Attributes["NewProductID"].GetStringOrDefault();
		priceData.Amount = node.Attributes["Amount"].ParseLong(0L);
		priceData.NewAmount = node.Attributes["NewAmount"].ParseLong(0L);
		priceData.AddAmount = node.Attributes["AddAmount"].ParseLong(0L);
		priceData.NewAddAmount = node.Attributes["NewAddAmount"].ParseLong(0L);
		priceData.Price = node.Attributes["Price"].GetStringOrDefault();
		priceData.NewPrice = node.Attributes["NewPrice"].GetStringOrDefault();
		priceData.Currency = node.Attributes["Currency"].ParseInt();
		priceData.AddCurrency = node.Attributes["AddCurrency"].GetStringOrDefault();
		priceData.name = node.Attributes["Name"].GetStringOrDefault();
		priceData.StartDate = node.Attributes["StartDate"].ParseInt();
		priceData.EndDate = node.Attributes["EndDate"].ParseInt();
		priceData.Sign = node.Attributes["Sign"].GetStringOrDefault();
		priceData.SignCode = node.Attributes["SignCode"].GetStringOrDefault("USD");
		priceData.Label = node.Attributes["Label"].GetStringOrDefault(string.Empty);
		priceData.GroupId = node.Attributes["GroupID"].GetStringOrDefault(string.Empty);
		priceData.AddPercent = node.Attributes["AddPercent"].ParseInt();
		priceData.Locale = node.Attributes["Locale"].GetStringOrDefault(string.Empty);
		priceData.Focus = node.Attributes["Focus"].ParseBool();
		priceData.MobileOperator = node.Attributes["MobileOperator"].GetStringOrDefault(string.Empty);
		priceData.SpenderTypeId = node.Attributes["SpenderTypeID"].GetStringOrDefault(string.Empty);
		PricesData pricesData = priceData;
		pricesData.IsConsumable = node.Attributes["ProductType"].ParseInt(1) != 2;
		return pricesData;
	}

	private bool IsOkLocale(string localeList, string locale)
	{
		if (string.IsNullOrEmpty(localeList) && string.IsNullOrEmpty(locale))
		{
			return true;
		}
		string[] array = localeList.Split('|');
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text == locale)
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckPlatform(int value)
	{
		return value == 0 || (SystemProperties.IsIosPlatform() && value == 1) || (SystemProperties.IsAndroidPlatform() && value == 2) || (SystemProperties.IsEditorPlatform() && value == 3) || (SystemProperties.IsMetroArmPlatform() && value == 4) || (SystemProperties.IsWindowsStorePlatform() && value == 5);
	}

	private void InvokeCallback()
	{
		if (_Callback != null)
		{
			_Callback(callbackResult);
			callbackResult = false;
			_Callback = null;
		}
	}

	public static void WipeExternalConfig()
	{
		GameLog.Write("[GeneralConfig] wipe external config");
		FileUtils.DeleteFile(SF2Paths.GetWritableGameDataPath() + "/config_cdn.xml");
	}
}
