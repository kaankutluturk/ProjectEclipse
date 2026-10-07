using System.Collections.Generic;
using System.Xml;
using Nekki.SF2.GUI.Map;
using UnityEngine;

public static class GameSettings
{
	public class VersionSettings
	{
		public bool IsFirstLaunch;

		public bool HasVersionData;

		public VersionSettings()
		{
			IsFirstLaunch = false;
			HasVersionData = false;
		}

		public bool Empty()
		{
			return !IsFirstLaunch && !HasVersionData;
		}
	}

	private static VersionSettings versionSettings = new VersionSettings();

	private static bool versionUpdatePending = false;

	private static bool userDataValidationEnabled = false;

	private static List<QualityOption> qualityOptions = new List<QualityOption>();

	public static bool UserDataValidationEnabled
	{
		get
		{
			return IsUserDataValidationEnabled();
		}
	}

	public static void InitUserDataValidation()
	{
		// Local, moddable saves are accepted in every player and in the editor.
		userDataValidationEnabled = false;
	}

	public static void LoadAllSettings()
	{
		LoadInternalSettings();
		LoadTacticSettings();
		LoadPerks();
		LoadCharacterProgress();
		LoadAchievements();
		GameUtils.InitSliderNames();
		GameUtils.InitRaidTutorialStepNames();
	}

	private static void LoadInternalSettings()
	{
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "internalSettings.xml");
		if (xmlDocument != null)
		{
			XmlNode xmlNode = xmlDocument["Settings"];
			if (xmlNode != null)
			{
				ParseInternalSettings(xmlNode);
			}
			else
			{
				GameLog.Error("ERROR: GameSettings.LoadInternalSettings - wrong file");
			}
		}
		else
		{
			GameLog.Error("ERROR: loadInternalSettings - file internalSettings.xml doesn't exist");
		}
	}

	private static void ParseInternalSettings(XmlNode BAMDEPGMGEN)
	{
		XmlNode xmlNode = BAMDEPGMGEN["Attributes"];
		if (xmlNode != null)
		{
			GameUtils.WarriorAttributeList.Parse(xmlNode);
		}
		XmlNode xmlNode2 = BAMDEPGMGEN["RatingEvaluation"];
		if (xmlNode2 != null)
		{
			ModelParameters.ParseRatingConfig(xmlNode2);
		}
		XmlNode xmlNode3 = BAMDEPGMGEN["DifficultyEvaluation"];
		if (xmlNode3 != null)
		{
			DifficultyPanel.DifficultyEvaluationParse(xmlNode3);
		}
		GameUtils.SlowModeSpeed = BAMDEPGMGEN["SlowMode"].Attributes["Value"].ParseInt(10);
		GameUtils.SetSlowMotionDefense(BAMDEPGMGEN["SlowMotion"].Attributes["Defense"].GetStringOrDefault(string.Empty));
		GameUtils.SetDefaultAvatar(BAMDEPGMGEN["Avatar"].Attributes["Name"].GetStringOrDefault("avatar_hero"));
		GameUtils.SetDefaultSkeleton(BAMDEPGMGEN["Skeleton"].Attributes["Player"].GetStringOrDefault("Skeleton"));
		XmlNode xmlNode4 = BAMDEPGMGEN["DefaultItems"];
		if (xmlNode4 != null)
		{
			GameUtils.ParseDefaultItems(xmlNode4);
		}
		GameUtils.DefaultLocation = BAMDEPGMGEN["Location"].Attributes["Name"].GetStringOrDefault("dojo");
		GameUtils.TutorialSettings.Parse(BAMDEPGMGEN["Tutorial"]);
		GameUtils.PivotNodeName = BAMDEPGMGEN["PivotNode"].Attributes["Name"].GetStringOrDefault("NPivot");
		GameUtils.SellPriceFactor = BAMDEPGMGEN["SellItems"].Attributes["Value"].ParseFloat(0.5f);
		GameUtils.SetComboMinHits(BAMDEPGMGEN["Combo"].Attributes["MinHits"].ParseInt(3));
		GameUtils.SetComboTime(BAMDEPGMGEN["Combo"].Attributes["Time"].ParseInt(90));
		GameUtils.SetAnnouncementTime(BAMDEPGMGEN["Announcements"].Attributes["Time"].ParseInt(60));
		GameUtils.SetHotGroundTime(BAMDEPGMGEN["HotGroundTimer"].Attributes["Time"].ParseInt(90));
		PhysicsController.Parse(BAMDEPGMGEN["Physics"]);
		GameUtils.SetGreatMaxHealth(BAMDEPGMGEN["Great"].Attributes["MaxHealth"].ParseFloat(0.3f));
		GameUtils.SetDamageFactorBase(BAMDEPGMGEN["DamageFactor"].Attributes["Base"].ParseFloat());
		GameUtils.SetDamageFactorMaxValue(BAMDEPGMGEN["DamageFactor"].Attributes["MaxValue"].ParseInt(20000));
		GameUtils.SetDamageFactorAttribute(BAMDEPGMGEN["DamageFactor"].Attributes["Attribute"].GetStringOrDefault(string.Empty));
		XmlNode hKPPBKPJOEO = BAMDEPGMGEN["BlockDamageFactor"];
		GameUtils.GetBlockDamageFactor().Parse(hKPPBKPJOEO);
		XmlNode hKPPBKPJOEO2 = BAMDEPGMGEN["CriticalHit"]["Damage"];
		GameUtils.GetCriticalHitDamage().Parse(hKPPBKPJOEO2);
		GameUtils.SetBlockDefenseAttribute(BAMDEPGMGEN["BlockDefense"].Attributes["Attribute"].GetStringOrDefault(string.Empty));
		GameUtils.DamageDoublingRange = BAMDEPGMGEN["DamageDoublingRange"].Attributes["Value"].ParseFloat();
		GameUtils.SetResistanceDoublingRange(BAMDEPGMGEN["ResistanceDoublingRange"].Attributes["Value"].ParseFloat());
		GameUtils.SetStartingMagicAttribute((BAMDEPGMGEN["StartingMagic"] == null) ? null : BAMDEPGMGEN["StartingMagic"].Attributes["Attribute"].GetStringOrDefault(string.Empty));
		GameUtils.SetMaxPower((BAMDEPGMGEN["Power"] == null) ? 10 : BAMDEPGMGEN["Power"].Attributes["Max"].ParseInt(10));
		GameUtils.PowerMaxTime = ((BAMDEPGMGEN["Power"] == null) ? 600 : BAMDEPGMGEN["Power"].Attributes["TimeMax"].ParseInt(600));
		GameUtils.SetLifeBarValue((BAMDEPGMGEN["LifeBar"] == null) ? 0f : BAMDEPGMGEN["LifeBar"].Attributes["Value"].ParseFloat());
		GameUtils.PushRetentionTime = ((BAMDEPGMGEN["PushRetantionTime"] == null) ? 172800 : BAMDEPGMGEN["PushRetantionTime"].Attributes["Value"].ParseInt(172800));
		XmlNode xmlNode5 = BAMDEPGMGEN["OutdateLevels"];
		if (xmlNode5 != null)
		{
			GameUtils.OutdateLevelTable.Parse(xmlNode5);
		}
		XmlNode xmlNode6 = BAMDEPGMGEN["AlignTargetAttributes"];
		if (xmlNode6 != null)
		{
			GameUtils.AlignTargetAttributes.Clear();
			GameUtils.AlignTargetAttribute.Parse(xmlNode6, GameUtils.AlignTargetAttributes);
		}
		GameUtils.CounterPunches = BAMDEPGMGEN["CounterPunches"].Attributes["Value"].ParseInt(2);
		XmlNode hKPPBKPJOEO3 = BAMDEPGMGEN["RewardsPrize"];
		GameUtils.RewardsPrizeSettings.Parse(hKPPBKPJOEO3);
		XmlNode hKPPBKPJOEO4 = BAMDEPGMGEN["CriticalHit"];
		GameUtils.CriticalHitDefaults.Parse(hKPPBKPJOEO4);
		XmlNode hKPPBKPJOEO5 = BAMDEPGMGEN["HitEffects"];
		GameUtils.GetHitEffects().Parse(hKPPBKPJOEO5);
		XmlNode hKPPBKPJOEO6 = BAMDEPGMGEN["Shock"];
		GameUtils.ShockSettings.Parse(hKPPBKPJOEO6);
		XmlNode hKPPBKPJOEO7 = BAMDEPGMGEN["Camera"];
		GameUtils.GetCameraSettings().Parse(hKPPBKPJOEO7);
		GameUtils.SupportChoices.Parse(BAMDEPGMGEN["Supports"]);
		XmlNode hKPPBKPJOEO8 = BAMDEPGMGEN["Shop"];
		GameUtils.ShopOverrides.Parse(hKPPBKPJOEO8);
		XmlNode hKPPBKPJOEO9 = BAMDEPGMGEN["Currencies"];
		GameUtils.GameCurrencies.Parse(hKPPBKPJOEO9);
		XmlNode eBLIGDMALEA = BAMDEPGMGEN["Resistances"];
		GameUtils.GameResistances.Parse(eBLIGDMALEA);
		XmlNode hKPPBKPJOEO10 = BAMDEPGMGEN["BarScales"];
		GameUtils.BarScaleTable.Parse(hKPPBKPJOEO10);
		XmlNode hKPPBKPJOEO11 = BAMDEPGMGEN["Magic"];
		GameUtils.MagicConfig.Parse(hKPPBKPJOEO11);
		XmlNode eBLIGDMALEA2 = BAMDEPGMGEN["AchievementCounter"];
		GameUtils.ModeCounters.Parse(eBLIGDMALEA2);
		XmlNode hKPPBKPJOEO12 = BAMDEPGMGEN["Regeneration"];
		GameUtils.GetRegeneration().Parse(hKPPBKPJOEO12);
		XmlNode hKPPBKPJOEO13 = BAMDEPGMGEN["Lifesteal"];
		GameUtils.GetLifesteal().Parse(hKPPBKPJOEO13);
		GameUtils.FrameRate = BAMDEPGMGEN["FrameRate"].Attributes["Value"].ParseInt(60);
		BasicGUI.Parse(BAMDEPGMGEN["GUI"]["Basic"]);
		MapGUI.Parse(BAMDEPGMGEN["GUI"]["Map"]);
		FightGUI.Parse(BAMDEPGMGEN["GUI"]["Fight"]);
		ProfileGUI.Parse(BAMDEPGMGEN["GUI"]["Profile"]);
		InternetController.Parse(BAMDEPGMGEN["Internet"]);
		if (!Debug.isDebugBuild)
		{
			GameUtils.AlwaysMagicMode = false;
		}
		else
		{
			GameUtils.AlwaysMagicMode = BAMDEPGMGEN["AlwaysMagicMode"].Attributes["Value"].ParseBool();
		}
		GameUtils.DailyDebugMode = BAMDEPGMGEN["DailyDebugMode"].Attributes["Value"].ParseBool();
		GameUtils.DailyDebugTime = BAMDEPGMGEN["DailyDebugTime"].Attributes["Value"].ParseInt();
		SystemProperties.SetTargetFrameRate(GameUtils.FrameRate);
		ParseQualityOptions(BAMDEPGMGEN["QualityOptions"]);
		XmlNode xmlNode7 = BAMDEPGMGEN["Aspects"];
		if (xmlNode7 != null)
		{
			GameUtils.ParseAspects(xmlNode7);
		}
		XmlNode xmlNode8 = BAMDEPGMGEN["Aspect"];
		if (xmlNode8 != null)
		{
			GameUtils.ParseAspectConstants(xmlNode8);
		}
		GameUtils.RegisterAspectAttributes();
		XmlNode xmlNode9 = BAMDEPGMGEN["StyleLevels"];
		if (xmlNode9 != null)
		{
			GameUtils.StyleLevelTable.Parse(xmlNode9);
		}
		GameUtils.MaximumExperience = BAMDEPGMGEN["MaximumExperience"].Attributes["Value"].ParseUint(30000000u);
	}

	public static void CheckVersions()
	{
		bool flag = false;
		versionSettings.IsFirstLaunch = false;
		versionSettings.HasVersionData = false;
		VersionContainer pAMHFPMEPCH = new VersionContainer();
		VersionContainer pAMHFPMEPCH2 = new VersionContainer();
		VersionContainer pAMHFPMEPCH3 = new VersionContainer();
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "versionController.xml");
		XmlNode xmlNode = ((xmlDocument == null) ? null : xmlDocument["Versions"]["Version"]);
		if (xmlNode != null)
		{
			pAMHFPMEPCH.SetVersion(xmlNode.Attributes["Value"].GetStringOrDefault(string.Empty));
			pAMHFPMEPCH.SetRevision(0);
		}
		XmlDocument xmlDocument2 = XmlUtils.LoadDocumentWithHashCheck(SF2Paths.GetUserDataDirectory(), Constants.UsersFileName);
		if (xmlDocument2 == null)
		{
			xmlDocument2 = XmlUtils.LoadDocumentWithHashCheck(SF2Paths.GetUserDataDirectory(), Constants.UsersBackupFileName);
			if (xmlDocument2 != null)
			{
				string kPFELJFPGHJ = string.Format("{0}/{1}", SF2Paths.GetUserDataDirectory(), Constants.UsersFileName);
				XmlUtils.SaveDocumentWithHash(xmlDocument2, kPFELJFPGHJ);
			}
		}
		if (xmlDocument2 == null)
		{
			flag = true;
			versionSettings.IsFirstLaunch = true;
		}
		else if (xmlDocument2["Root"]["Versions"] != null)
		{
			versionSettings.HasVersionData = true;
		}
		if (versionSettings.HasVersionData)
		{
			string aHLPODLKBEP = xmlDocument2["Root"]["Versions"]["Version"].Attributes["Value"].GetStringOrDefault(string.Empty);
			string aHLPODLKBEP2 = xmlDocument2["Root"]["Versions"]["DataVersion"].Attributes["Value"].GetStringOrDefault(string.Empty);
			pAMHFPMEPCH2.SetVersion(aHLPODLKBEP);
			pAMHFPMEPCH3.SetVersion(aHLPODLKBEP2);
			if (VersionContainer.IsGreater(pAMHFPMEPCH, pAMHFPMEPCH2))
			{
				flag = true;
			}
		}
		if (flag)
		{
			versionUpdatePending = true;
		}
		SystemProperties.SetVersions(pAMHFPMEPCH, pAMHFPMEPCH3);
	}

	public static void InitVersion()
	{
		if (versionUpdatePending)
		{
			VersionContainer pAMHFPMEPCH = SystemProperties.GetVersion();
			VersionContainer pAMHFPMEPCH2 = new VersionContainer();
			string oNEIGMLOGDC = ((!versionSettings.IsFirstLaunch) ? SF2Paths.GetUserDataDirectory() : SF2Paths.GetGameDataPath());
			XmlDocument xmlDocument = null;
			xmlDocument = ((!versionSettings.IsFirstLaunch) ? XmlUtils.OpenXMLDocument(oNEIGMLOGDC, Constants.UsersFileName) : XmlUtils.OpenXMLDocument(oNEIGMLOGDC, "usersDefault.xml", XmlUtils.XmlSourceMode.Normal, true, XmlCryptoUtils.GetIsEncryptionEnabled()));
			if (xmlDocument != null)
			{
				XmlNode xmlNode = xmlDocument["Root"]["Versions"];
				if (xmlNode != null)
				{
					string value = pAMHFPMEPCH.ToString();
					xmlNode["Version"].Attributes["Value"].Value = value;
					string aHLPODLKBEP = xmlNode["DataVersion"].Attributes["Value"].GetStringOrDefault(string.Empty);
					pAMHFPMEPCH2.SetVersion(aHLPODLKBEP);
				}
				SystemProperties.SetVersions(pAMHFPMEPCH, pAMHFPMEPCH2);
				string kPFELJFPGHJ = string.Format("{0}/{1}", SF2Paths.GetUserDataDirectory(), Constants.UsersFileName);
				XmlUtils.SaveDocumentWithHash(xmlDocument, kPFELJFPGHJ);
			}
			else
			{
				GameLog.Error("GameSettings.InitVersion userXML is null");
			}
			GeneralConfig.WipeExternalConfig();
		}
		versionUpdatePending = false;
	}

	public static string GetBundledVersion()
	{
		XmlDocument xmlDocument = XmlUtils.LoadDocumentWithHashCheck(SF2Paths.GetUserDataDirectory(), Constants.UsersFileName);
		if (xmlDocument != null)
		{
			XmlDocument xmlDocument2 = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "versionController.xml");
			if (xmlDocument2 != null)
			{
				return xmlDocument2["Versions"]["Version"].Attributes["Value"].GetStringOrDefault();
			}
		}
		return string.Empty;
	}

	public static void LoadAssemblySettings()
	{
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "internalSettings.xml");
		if (xmlDocument != null)
		{
			XmlNode xmlNode = xmlDocument["Settings"];
			if (xmlNode != null)
			{
				XmlNode xmlNode2 = xmlNode["AssemblySettings"];
				if (xmlNode2 != null)
				{
					AssemblyController.Parse(xmlNode2);
				}
				else
				{
					GameLog.Write("ERROR: loadInternalSettings - AssemblySettings section is missing");
				}
			}
			else
			{
				GameLog.Write("ERROR: loadInternalSettings - wrong file");
			}
		}
		else
		{
			GameLog.Write("ERROR: loadInternalSettings - file \"{0}\" doesn't exist", "internalSettings.xml");
		}
	}

	public static void ApplyQualityOptions()
	{
		for (int i = 0; i < qualityOptions.Count; i++)
		{
			qualityOptions[i].ApplyIfConditionMatches();
		}
	}

	private static void LoadTacticSettings()
	{
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "tacticSettings.xml");
		if (xmlDocument != null)
		{
			XmlNode xmlNode = xmlDocument["TacticsSettings"];
			if (xmlNode != null)
			{
				ParseTacticSettings(xmlNode);
			}
			else
			{
				GameLog.Write("ERROR: loadInternalSettings - wrong file");
			}
		}
	}

	private static void LoadCharacterProgress()
	{
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "CharacterProgress.xml");
		if (xmlDocument != null)
		{
			XmlNode xmlNode = xmlDocument["Progress"];
			if (xmlNode != null)
			{
				ParseCharacterProgress(xmlNode);
			}
			else
			{
				GameLog.Write("ERROR: loadCharacterProgress - wrong file");
			}
		}
		else
		{
			GameLog.Write("ERROR: loadCharacterProgress - file CharacterProgress.xml doesn't exist");
		}
	}

	private static void LoadAchievements()
	{
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "Achievements.xml");
		if (xmlDocument != null)
		{
			XmlNode xmlNode = xmlDocument["Achievements"];
			if (xmlNode != null)
			{
				ParseAchievements(xmlNode);
			}
			else
			{
				GameLog.Write("ERROR: loadAchievements - wrong file");
			}
		}
		else
		{
			GameLog.Write("ERROR: loadAchievements - file Achievements.xml doesn't exist");
		}
	}

	private static void LoadPerks()
	{
		XmlDocument EELFNMOHGJL = null;
		PerksCompiler.CompilePerks(ref EELFNMOHGJL, string.Format("{0}/{1}", SF2Paths.GetGameDataPath(), "perks.xml"));
		if (EELFNMOHGJL != null)
		{
			XmlNode xmlNode = EELFNMOHGJL["Perks"];
			if (xmlNode != null)
			{
				GameUtils.PerkItemList.Parse(xmlNode);
			}
			else
			{
				GameLog.Error("ERROR: GameSettings.LoadPerks - wrong file");
			}
		}
		else
		{
			GameLog.Error("ERROR: LoadPerks - file perks.xml doesn't exist");
		}
	}

	private static void ParseAchievements(XmlNode node)
	{
		GameUtils.AchievementDefinitions.Parse(node);
	}

	private static void ParseCharacterProgress(XmlNode BAMDEPGMGEN)
	{
		XmlNode eBLIGDMALEA = BAMDEPGMGEN["Thresholds"];
		XmlNode xmlNode = BAMDEPGMGEN["LotteryThresholds"];
		XmlNode hKPPBKPJOEO = BAMDEPGMGEN["Perks"];
		XmlNode hKPPBKPJOEO2 = BAMDEPGMGEN["LevelAttributeGain"];
		XmlNode hKPPBKPJOEO3 = BAMDEPGMGEN["StartingAttributes"];
		XmlNode hKPPBKPJOEO4 = BAMDEPGMGEN["PerkTree"];
		XmlNode hKPPBKPJOEO5 = BAMDEPGMGEN["CurrencyBaseValues"];
		XmlNode hKPPBKPJOEO6 = BAMDEPGMGEN["MoneyBaseValues"];
		GameUtils.LevelThresholdTable.Parse(eBLIGDMALEA);
		GameUtils.PerkItemList.ParseProgression(hKPPBKPJOEO);
		GameUtils.LevelAttributeGains.Parse(hKPPBKPJOEO2);
		GameUtils.StartingAttributes.Parse(hKPPBKPJOEO3);
		GameUtils.CurrencyBaseValueTable.Parse(hKPPBKPJOEO5);
		GameUtils.MoneyBaseValueTable.Parse(hKPPBKPJOEO6);
		PerkTree.GetInstance().Parse(hKPPBKPJOEO4);
	}

	private static void ParseTacticSettings(XmlNode node)
	{
		GameUtils.RandomTactics.Parse(node["Random"]);
	}

	public static bool IsUserDataValidationEnabled()
	{
		return userDataValidationEnabled;
	}

	private static void ParseQualityOptions(XmlNode node)
	{
		qualityOptions.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			QualityOption item = new QualityOption(childNode);
			qualityOptions.Add(item);
		}
	}
}
