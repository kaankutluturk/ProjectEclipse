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

	public void LoadConfig(string path, Action<bool> callback, Action<object> IPDNNACNOEN, List<DownloadPack> DEJEBFLAOIB, int HCCLKJOCHGP, int INGCPFFHBOG)
	{
		_Callback = callback;
		progressCallback = IPDNNACNOEN;
		pendingItems = DEJEBFLAOIB;
		downloadTimeout = HCCLKJOCHGP;
		reservedParam = INGCPFFHBOG;
		if (string.IsNullOrEmpty(path))
		{
			InvokeCallback();
			return;
		}
		string arg = NekkiMath.randomInt(1000000).ToString();
		string mGPGDPOOCBK = string.Format("{0}?{1}", path, arg);
		ServerProvider.get_Instance().DownloadFile(mGPGDPOOCBK, OnLoadConfig, null, downloadTimeout);
	}

	private void OnLoadConfig(byte[] data, string JDONBAPIJCG, string BEPKJNKCKPH)
	{
		if (string.IsNullOrEmpty(JDONBAPIJCG) && data != null)
		{
			File.WriteAllBytes(SF2Paths.GetWritableGameDataPath() + "/config_cdn.xml", data);
		}
		else
		{
			if (JDONBAPIJCG == "offline build")
			{
				UnityEngine.Debug.LogWarning("[Config] Offline build; using the recovered local config.");
			}
			else
			{
				GameLog.Error("[Config]: failed to download config because " + JDONBAPIJCG);
			}
		}
		Parse(true);
	}

	private bool Parse(bool EJGPPDALIOJ)
	{
		bool flag = true;
		if (EJGPPDALIOJ)
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

	private void ParsePlatform(XmlNode GLCBJNIIPDG)
	{
		platformName = "unknown";
		foreach (XmlNode childNode in GLCBJNIIPDG.ChildNodes)
		{
			int bAINMLLIKOL = childNode.Attributes["PlatformID"].ParseInt();
			if (CheckPlatform(bAINMLLIKOL))
			{
				platformName = childNode.Attributes["Name"].GetStringOrDefault(platformName);
			}
		}
	}

	private void ParseVersions(XmlNode BPDFMKIGEKF)
	{
		bool oDHPOJMNFIN = false;
		VersionContainer aAOIAEJJINO = SystemProperties.GetVersion();
		foreach (XmlNode childNode in BPDFMKIGEKF.ChildNodes)
		{
			int bAINMLLIKOL = childNode.Attributes["PlatformID"].ParseInt();
			if (CheckPlatform(bAINMLLIKOL))
			{
				VersionContainer pAMHFPMEPCH = new VersionContainer();
				pAMHFPMEPCH.SetVersion(childNode.Attributes["Version"].GetStringOrDefault());
				if (VersionContainer.IsGreater(pAMHFPMEPCH, aAOIAEJJINO))
				{
					oDHPOJMNFIN = true;
					break;
				}
			}
		}
		isVersionSupported = oDHPOJMNFIN;
	}

	private void ParseSettings(XmlNode node)
	{
		XmlNode aIDFCDDECJB = node["time"];
		XmlNode aIDFCDDECJB2 = node["dumps"];
		XmlNode aIDFCDDECJB3 = node["server"];
		KeyValuePair<string, string> hFCAPMDHLJN = ParseUrlPair(aIDFCDDECJB);
		KeyValuePair<string, string> hFCAPMDHLJN2 = ParseUrlPair(aIDFCDDECJB2);
		KeyValuePair<string, string> hFCAPMDHLJN3 = ParseUrlPair(aIDFCDDECJB3);
		if (!IsEmptyPair(hFCAPMDHLJN))
		{
			ServerProvider.set_TimeServerURL(hFCAPMDHLJN.Value);
		}
		if (!IsEmptyPair(hFCAPMDHLJN2))
		{
			ServerProvider.set_DumpPutURL(hFCAPMDHLJN2.Key);
			ServerProvider.set_DumpGetURL(hFCAPMDHLJN2.Value);
		}
		if (!IsEmptyPair(hFCAPMDHLJN3))
		{
			ServerProvider.set_PutURL(hFCAPMDHLJN3.Key);
			ServerProvider.set_GetURL(hFCAPMDHLJN3.Value);
		}
		XmlNode hKPPBKPJOEO = node["verification"];
		ParseVerification(hKPPBKPJOEO);
		XmlNode hKPPBKPJOEO2 = node["ledger"];
		ParseLedger(hKPPBKPJOEO2);
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

	private void ParsePackGroup(XmlNode MEEAKLDGLDF)
	{
		Dictionary<string, List<XmlNode>> dictionary = new Dictionary<string, List<XmlNode>>();
		foreach (XmlNode childNode in MEEAKLDGLDF.ChildNodes)
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
				int bAINMLLIKOL = item2.Attributes["PlatformID"].ParseInt();
				if (CheckPlatform(bAINMLLIKOL))
				{
					xmlNode2.AppendImportedClone(item2);
				}
			}
			XmlNode xmlNode3 = ClosestVersion(xmlNode2);
			string text = ((xmlNode3 == null) ? null : xmlNode3.Attributes["Url"].GetStringOrDefault());
			string pEEOEOMEBFG = ((xmlNode3 == null) ? null : xmlNode3.Attributes["Size"].GetStringOrDefault());
			bool lCDCAKLKHMI = xmlNode3 != null && xmlNode3.Attributes["Reload"].ParseBool();
			string hDPBNCNCMOH = ((xmlNode3 == null) ? null : xmlNode3.Attributes["Hash"].GetStringOrDefault());
			bool aHDLCJFCJMJ = xmlNode3 != null && xmlNode3.Attributes["Attach"].ParseBool();
			if (!string.IsNullOrEmpty(text))
			{
				DownloadPacks.AddPack(key2, text, pEEOEOMEBFG, lCDCAKLKHMI, hDPBNCNCMOH, aHDLCJFCJMJ);
			}
		}
	}

	private XmlNode ClosestVersion(XmlNode nodes, bool DFOOHEFGEBG = false, bool MGMDADDKPMP = false)
	{
		XmlNode result = null;
		VersionContainer aAOIAEJJINO = new VersionContainer();
		VersionContainer pAMHFPMEPCH = SystemProperties.GetVersion();
		foreach (XmlNode childNode in nodes.ChildNodes)
		{
			if (MGMDADDKPMP)
			{
				int bAINMLLIKOL = childNode.Attributes["PlatformID"].ParseInt();
				if (!CheckPlatform(bAINMLLIKOL))
				{
					continue;
				}
			}
			VersionContainer pAMHFPMEPCH2 = new VersionContainer();
			pAMHFPMEPCH2.SetVersion(childNode.Attributes["MinVersion"].GetStringOrDefault());
			if (!DFOOHEFGEBG)
			{
				if (VersionContainer.IsGreaterOrEqual(pAMHFPMEPCH2, aAOIAEJJINO) && VersionContainer.IsLessOrEqual(pAMHFPMEPCH2, pAMHFPMEPCH))
				{
					aAOIAEJJINO = pAMHFPMEPCH2;
					result = childNode;
				}
			}
			else if (VersionContainer.IsEqual(pAMHFPMEPCH, pAMHFPMEPCH2))
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

	private bool ParseNewsForLocale(XmlNode MEEAKLDGLDF, string EADIFEPJKJK)
	{
		bool result = false;
		VersionContainer lHBNIMGFKIB = SystemProperties.GetVersion();
		foreach (XmlNode childNode in MEEAKLDGLDF.ChildNodes)
		{
			if (childNode.Name != "item")
			{
				continue;
			}
			int bAINMLLIKOL = childNode.Attributes["PlatformID"].ParseInt();
			if (!CheckPlatform(bAINMLLIKOL) || !IsOkLocale(childNode.Attributes["LangID"].GetStringOrDefault(), EADIFEPJKJK))
			{
				continue;
			}
			VersionContainer pAMHFPMEPCH = new VersionContainer();
			pAMHFPMEPCH.SetVersion(childNode.Attributes["MinVersion"].GetStringOrDefault());
			if (VersionContainer.IsLess(lHBNIMGFKIB, pAMHFPMEPCH))
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
					string pEMOECLNECD = childNode.Attributes["Title"].GetStringOrDefault(string.Empty);
					string gOHIIMFFFJI = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
					string bEPKJNKCKPH = childNode.Attributes["Url"].GetStringOrDefault(string.Empty);
					string mDDOAGNHAHE = childNode.Attributes["ImageURL"].GetStringOrDefault(string.Empty);
					int oKNNNLIPODI = childNode.Attributes["ID"].ParseInt();
					bool eIKKPDKMMHK = childNode.Attributes["GoShop"].ParseBool();
					string kINPMPFPFHD = childNode.Attributes["RedirectShop"].GetStringOrDefault(string.Empty);
					string eJENJNPEDOH = childNode.Attributes["SpenderTypeID"].GetStringOrDefault(string.Empty);
					bool hNJDHGDLLPD = childNode.Attributes["Active"].ParseBool();
					List<NewsButton> hJNAHNICGMH = ParseNewsButtons(childNode);
					CurrentNews.AddOrReplaceItem(gOHIIMFFFJI, bEPKJNKCKPH, mDDOAGNHAHE, oKNNNLIPODI, hNJDHGDLLPD, num2, hJNAHNICGMH, pEMOECLNECD, eIKKPDKMMHK, kINPMPFPFHD, eJENJNPEDOH);
					result = true;
				}
			}
		}
		return result;
	}

	private List<NewsButton> ParseNewsButtons(XmlNode MEEAKLDGLDF)
	{
		List<NewsButton> list = new List<NewsButton>();
		foreach (XmlNode item in MEEAKLDGLDF)
		{
			NewsButton fBKMFDJBJIB = new NewsButton();
			fBKMFDJBJIB.LabelAliasName = item.Attributes["Text"].GetStringOrDefault(string.Empty);
			fBKMFDJBJIB.Color = LabelButton.GetBtnColor(item.Attributes["Color"].GetStringOrDefault(string.Empty));
			fBKMFDJBJIB.Url = item.Attributes["Url"].GetStringOrDefault(string.Empty);
			fBKMFDJBJIB.GoShop = item.Attributes["GoShop"].ParseBool();
			fBKMFDJBJIB.BuyItem = item.Attributes["BuyItem"].ParseBool();
			fBKMFDJBJIB.RedirectShop = item.Attributes["RedirectShop"].GetStringOrDefault(string.Empty);
			list.Add(fBKMFDJBJIB);
		}
		return list;
	}

	private bool IsEmptyPair(KeyValuePair<string, string> HFCAPMDHLJN)
	{
		return string.IsNullOrEmpty(HFCAPMDHLJN.Key) && string.IsNullOrEmpty(HFCAPMDHLJN.Value);
	}

	private KeyValuePair<string, string> ParseUrlPair(XmlNode AIDFCDDECJB)
	{
		string key = null;
		string value = null;
		foreach (XmlNode childNode in AIDFCDDECJB.ChildNodes)
		{
			int bAINMLLIKOL = childNode.Attributes["PlatformID"].ParseInt();
			if (CheckPlatform(bAINMLLIKOL))
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

	private void ParsePricesForLocale(XmlNode node, string EADIFEPJKJK)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (!CheckPlatform(childNode.Attributes["PlatformID"].ParseInt()) || !childNode.Attributes["MobileOperator"].Empty())
			{
				continue;
			}
			PricesData bEOLBLGJCKA = ParsePriceNode(childNode);
			if (!string.IsNullOrEmpty(bEOLBLGJCKA.Locale) && !IsOkLocale(bEOLBLGJCKA.Locale, EADIFEPJKJK))
			{
				continue;
			}
			PricesData bEOLBLGJCKA2 = Prices.FindByProductId(bEOLBLGJCKA.ProductId);
			if (bEOLBLGJCKA2 == null)
			{
				Prices.GetPrices().Add(bEOLBLGJCKA);
				continue;
			}
			bool flag = bEOLBLGJCKA.GroupId == bEOLBLGJCKA2.GroupId;
			bool flag2 = bEOLBLGJCKA.SpenderTypeId == bEOLBLGJCKA2.SpenderTypeId || string.IsNullOrEmpty(bEOLBLGJCKA2.SpenderTypeId);
			if (flag && flag2)
			{
				if (string.IsNullOrEmpty(bEOLBLGJCKA2.Locale))
				{
					Prices.GetPrices().Remove(bEOLBLGJCKA2);
					Prices.GetPrices().Add(bEOLBLGJCKA);
				}
			}
			else
			{
				Prices.GetPrices().Add(bEOLBLGJCKA);
			}
		}
	}

	private PricesData ParsePriceNode(XmlNode node)
	{
		PricesData bEOLBLGJCKA = new PricesData();
		bEOLBLGJCKA.ProductId = node.Attributes["ProductID"].GetStringOrDefault();
		bEOLBLGJCKA.NewProductId = node.Attributes["NewProductID"].GetStringOrDefault();
		bEOLBLGJCKA.Amount = node.Attributes["Amount"].ParseLong(0L);
		bEOLBLGJCKA.NewAmount = node.Attributes["NewAmount"].ParseLong(0L);
		bEOLBLGJCKA.AddAmount = node.Attributes["AddAmount"].ParseLong(0L);
		bEOLBLGJCKA.NewAddAmount = node.Attributes["NewAddAmount"].ParseLong(0L);
		bEOLBLGJCKA.Price = node.Attributes["Price"].GetStringOrDefault();
		bEOLBLGJCKA.NewPrice = node.Attributes["NewPrice"].GetStringOrDefault();
		bEOLBLGJCKA.Currency = node.Attributes["Currency"].ParseInt();
		bEOLBLGJCKA.AddCurrency = node.Attributes["AddCurrency"].GetStringOrDefault();
		bEOLBLGJCKA.name = node.Attributes["Name"].GetStringOrDefault();
		bEOLBLGJCKA.StartDate = node.Attributes["StartDate"].ParseInt();
		bEOLBLGJCKA.EndDate = node.Attributes["EndDate"].ParseInt();
		bEOLBLGJCKA.Sign = node.Attributes["Sign"].GetStringOrDefault();
		bEOLBLGJCKA.SignCode = node.Attributes["SignCode"].GetStringOrDefault("USD");
		bEOLBLGJCKA.Label = node.Attributes["Label"].GetStringOrDefault(string.Empty);
		bEOLBLGJCKA.GroupId = node.Attributes["GroupID"].GetStringOrDefault(string.Empty);
		bEOLBLGJCKA.AddPercent = node.Attributes["AddPercent"].ParseInt();
		bEOLBLGJCKA.Locale = node.Attributes["Locale"].GetStringOrDefault(string.Empty);
		bEOLBLGJCKA.Focus = node.Attributes["Focus"].ParseBool();
		bEOLBLGJCKA.MobileOperator = node.Attributes["MobileOperator"].GetStringOrDefault(string.Empty);
		bEOLBLGJCKA.SpenderTypeId = node.Attributes["SpenderTypeID"].GetStringOrDefault(string.Empty);
		PricesData bEOLBLGJCKA2 = bEOLBLGJCKA;
		bEOLBLGJCKA2.IsConsumable = node.Attributes["ProductType"].ParseInt(1) != 2;
		return bEOLBLGJCKA2;
	}

	private bool IsOkLocale(string EOMNCDDELLB, string CNGMIFIJKDB)
	{
		if (string.IsNullOrEmpty(EOMNCDDELLB) && string.IsNullOrEmpty(CNGMIFIJKDB))
		{
			return true;
		}
		string[] array = EOMNCDDELLB.Split('|');
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text == CNGMIFIJKDB)
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
