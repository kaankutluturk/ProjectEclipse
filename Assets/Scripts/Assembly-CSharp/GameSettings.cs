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

	private static void ParseInternalSettings(XmlNode settingsNode)
	{
		XmlNode xmlNode = settingsNode["Attributes"];
		if (xmlNode != null)
		{
			GameUtils.WarriorAttributeList.Parse(xmlNode);
		}
		XmlNode xmlNode2 = settingsNode["RatingEvaluation"];
		if (xmlNode2 != null)
		{
			ModelParameters.ParseRatingConfig(xmlNode2);
		}
		XmlNode xmlNode3 = settingsNode["DifficultyEvaluation"];
		if (xmlNode3 != null)
		{
			DifficultyPanel.DifficultyEvaluationParse(xmlNode3);
		}
		GameUtils.SlowModeSpeed = settingsNode["SlowMode"].Attributes["Value"].ParseInt(10);
		GameUtils.SetSlowMotionDefense(settingsNode["SlowMotion"].Attributes["Defense"].GetStringOrDefault(string.Empty));
		GameUtils.SetDefaultAvatar(settingsNode["Avatar"].Attributes["Name"].GetStringOrDefault("avatar_hero"));
		GameUtils.SetDefaultSkeleton(settingsNode["Skeleton"].Attributes["Player"].GetStringOrDefault("Skeleton"));
		XmlNode xmlNode4 = settingsNode["DefaultItems"];
		if (xmlNode4 != null)
		{
			GameUtils.ParseDefaultItems(xmlNode4);
		}
		GameUtils.DefaultLocation = settingsNode["Location"].Attributes["Name"].GetStringOrDefault("dojo");
		GameUtils.TutorialSettings.Parse(settingsNode["Tutorial"]);
		GameUtils.PivotNodeName = settingsNode["PivotNode"].Attributes["Name"].GetStringOrDefault("NPivot");
		GameUtils.SellPriceFactor = settingsNode["SellItems"].Attributes["Value"].ParseFloat(0.5f);
		GameUtils.SetComboMinHits(settingsNode["Combo"].Attributes["MinHits"].ParseInt(3));
		GameUtils.SetComboTime(settingsNode["Combo"].Attributes["Time"].ParseInt(90));
		GameUtils.SetAnnouncementTime(settingsNode["Announcements"].Attributes["Time"].ParseInt(60));
		GameUtils.SetHotGroundTime(settingsNode["HotGroundTimer"].Attributes["Time"].ParseInt(90));
		PhysicsController.Parse(settingsNode["Physics"]);
		GameUtils.SetGreatMaxHealth(settingsNode["Great"].Attributes["MaxHealth"].ParseFloat(0.3f));
		GameUtils.SetDamageFactorBase(settingsNode["DamageFactor"].Attributes["Base"].ParseFloat());
		GameUtils.SetDamageFactorMaxValue(settingsNode["DamageFactor"].Attributes["MaxValue"].ParseInt(20000));
		GameUtils.SetDamageFactorAttribute(settingsNode["DamageFactor"].Attributes["Attribute"].GetStringOrDefault(string.Empty));
		XmlNode blockDamageFactorNode = settingsNode["BlockDamageFactor"];
		GameUtils.GetBlockDamageFactor().Parse(blockDamageFactorNode);
		XmlNode hKPPBKPJOEO2 = settingsNode["CriticalHit"]["Damage"];
		GameUtils.GetCriticalHitDamage().Parse(hKPPBKPJOEO2);
		GameUtils.SetBlockDefenseAttribute(settingsNode["BlockDefense"].Attributes["Attribute"].GetStringOrDefault(string.Empty));
		GameUtils.DamageDoublingRange = settingsNode["DamageDoublingRange"].Attributes["Value"].ParseFloat();
		GameUtils.SetResistanceDoublingRange(settingsNode["ResistanceDoublingRange"].Attributes["Value"].ParseFloat());
		GameUtils.SetStartingMagicAttribute((settingsNode["StartingMagic"] == null) ? null : settingsNode["StartingMagic"].Attributes["Attribute"].GetStringOrDefault(string.Empty));
		GameUtils.SetMaxPower((settingsNode["Power"] == null) ? 10 : settingsNode["Power"].Attributes["Max"].ParseInt(10));
		GameUtils.PowerMaxTime = ((settingsNode["Power"] == null) ? 600 : settingsNode["Power"].Attributes["TimeMax"].ParseInt(600));
		GameUtils.SetLifeBarValue((settingsNode["LifeBar"] == null) ? 0f : settingsNode["LifeBar"].Attributes["Value"].ParseFloat());
		GameUtils.PushRetentionTime = ((settingsNode["PushRetantionTime"] == null) ? 172800 : settingsNode["PushRetantionTime"].Attributes["Value"].ParseInt(172800));
		XmlNode xmlNode5 = settingsNode["OutdateLevels"];
		if (xmlNode5 != null)
		{
			GameUtils.OutdateLevelTable.Parse(xmlNode5);
		}
		XmlNode xmlNode6 = settingsNode["AlignTargetAttributes"];
		if (xmlNode6 != null)
		{
			GameUtils.AlignTargetAttributes.Clear();
			GameUtils.AlignTargetAttribute.Parse(xmlNode6, GameUtils.AlignTargetAttributes);
		}
		GameUtils.CounterPunches = settingsNode["CounterPunches"].Attributes["Value"].ParseInt(2);
		XmlNode hKPPBKPJOEO3 = settingsNode["RewardsPrize"];
		GameUtils.RewardsPrizeSettings.Parse(hKPPBKPJOEO3);
		XmlNode hKPPBKPJOEO4 = settingsNode["CriticalHit"];
		GameUtils.CriticalHitDefaults.Parse(hKPPBKPJOEO4);
		XmlNode hKPPBKPJOEO5 = settingsNode["HitEffects"];
		GameUtils.GetHitEffects().Parse(hKPPBKPJOEO5);
		XmlNode hKPPBKPJOEO6 = settingsNode["Shock"];
		GameUtils.ShockSettings.Parse(hKPPBKPJOEO6);
		XmlNode hKPPBKPJOEO7 = settingsNode["Camera"];
		GameUtils.GetCameraSettings().Parse(hKPPBKPJOEO7);
		GameUtils.SupportChoices.Parse(settingsNode["Supports"]);
		XmlNode hKPPBKPJOEO8 = settingsNode["Shop"];
		GameUtils.ShopOverrides.Parse(hKPPBKPJOEO8);
		XmlNode hKPPBKPJOEO9 = settingsNode["Currencies"];
		GameUtils.GameCurrencies.Parse(hKPPBKPJOEO9);
		XmlNode resistancesNode = settingsNode["Resistances"];
		GameUtils.GameResistances.Parse(resistancesNode);
		XmlNode hKPPBKPJOEO10 = settingsNode["BarScales"];
		GameUtils.BarScaleTable.Parse(hKPPBKPJOEO10);
		XmlNode hKPPBKPJOEO11 = settingsNode["Magic"];
		GameUtils.MagicConfig.Parse(hKPPBKPJOEO11);
		XmlNode eBLIGDMALEA2 = settingsNode["AchievementCounter"];
		GameUtils.ModeCounters.Parse(eBLIGDMALEA2);
		XmlNode hKPPBKPJOEO12 = settingsNode["Regeneration"];
		GameUtils.GetRegeneration().Parse(hKPPBKPJOEO12);
		XmlNode hKPPBKPJOEO13 = settingsNode["Lifesteal"];
		GameUtils.GetLifesteal().Parse(hKPPBKPJOEO13);
		GameUtils.FrameRate = settingsNode["FrameRate"].Attributes["Value"].ParseInt(60);
		BasicGUI.Parse(settingsNode["GUI"]["Basic"]);
		MapGUI.Parse(settingsNode["GUI"]["Map"]);
		FightGUI.Parse(settingsNode["GUI"]["Fight"]);
		ProfileGUI.Parse(settingsNode["GUI"]["Profile"]);
		InternetController.Parse(settingsNode["Internet"]);
		if (!Debug.isDebugBuild)
		{
			GameUtils.AlwaysMagicMode = false;
		}
		else
		{
			GameUtils.AlwaysMagicMode = settingsNode["AlwaysMagicMode"].Attributes["Value"].ParseBool();
		}
		GameUtils.DailyDebugMode = settingsNode["DailyDebugMode"].Attributes["Value"].ParseBool();
		GameUtils.DailyDebugTime = settingsNode["DailyDebugTime"].Attributes["Value"].ParseInt();
		SystemProperties.SetTargetFrameRate(GameUtils.FrameRate);
		ParseQualityOptions(settingsNode["QualityOptions"]);
		XmlNode xmlNode7 = settingsNode["Aspects"];
		if (xmlNode7 != null)
		{
			GameUtils.ParseAspects(xmlNode7);
		}
		XmlNode xmlNode8 = settingsNode["Aspect"];
		if (xmlNode8 != null)
		{
			GameUtils.ParseAspectConstants(xmlNode8);
		}
		GameUtils.RegisterAspectAttributes();
		XmlNode xmlNode9 = settingsNode["StyleLevels"];
		if (xmlNode9 != null)
		{
			GameUtils.StyleLevelTable.Parse(xmlNode9);
		}
		GameUtils.MaximumExperience = settingsNode["MaximumExperience"].Attributes["Value"].ParseUint(30000000u);
	}

	public static void CheckVersions()
	{
		bool flag = false;
		versionSettings.IsFirstLaunch = false;
		versionSettings.HasVersionData = false;
		VersionContainer bundledVersion = new VersionContainer();
		VersionContainer pAMHFPMEPCH2 = new VersionContainer();
		VersionContainer pAMHFPMEPCH3 = new VersionContainer();
		XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "versionController.xml");
		XmlNode xmlNode = ((xmlDocument == null) ? null : xmlDocument["Versions"]["Version"]);
		if (xmlNode != null)
		{
			bundledVersion.SetVersion(xmlNode.Attributes["Value"].GetStringOrDefault(string.Empty));
			bundledVersion.SetRevision(0);
		}
		XmlDocument xmlDocument2 = XmlUtils.LoadDocumentWithHashCheck(SF2Paths.GetUserDataDirectory(), Constants.UsersFileName);
		if (xmlDocument2 == null)
		{
			xmlDocument2 = XmlUtils.LoadDocumentWithHashCheck(SF2Paths.GetUserDataDirectory(), Constants.UsersBackupFileName);
			if (xmlDocument2 != null)
			{
				string userDataPath = string.Format("{0}/{1}", SF2Paths.GetUserDataDirectory(), Constants.UsersFileName);
				XmlUtils.SaveDocumentWithHash(xmlDocument2, userDataPath);
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
			string savedVersionText = xmlDocument2["Root"]["Versions"]["Version"].Attributes["Value"].GetStringOrDefault(string.Empty);
			string aHLPODLKBEP2 = xmlDocument2["Root"]["Versions"]["DataVersion"].Attributes["Value"].GetStringOrDefault(string.Empty);
			pAMHFPMEPCH2.SetVersion(savedVersionText);
			pAMHFPMEPCH3.SetVersion(aHLPODLKBEP2);
			if (VersionContainer.IsGreater(bundledVersion, pAMHFPMEPCH2))
			{
				flag = true;
			}
		}
		if (flag)
		{
			versionUpdatePending = true;
		}
		SystemProperties.SetVersions(bundledVersion, pAMHFPMEPCH3);
	}

	public static void InitVersion()
	{
		if (versionUpdatePending)
		{
			VersionContainer currentVersion = SystemProperties.GetVersion();
			VersionContainer pAMHFPMEPCH2 = new VersionContainer();
			string dataDirectory = ((!versionSettings.IsFirstLaunch) ? SF2Paths.GetUserDataDirectory() : SF2Paths.GetGameDataPath());
			XmlDocument xmlDocument = null;
			xmlDocument = ((!versionSettings.IsFirstLaunch) ? XmlUtils.OpenXMLDocument(dataDirectory, Constants.UsersFileName) : XmlUtils.OpenXMLDocument(dataDirectory, "usersDefault.xml", XmlUtils.XmlSourceMode.Normal, true, XmlCryptoUtils.GetIsEncryptionEnabled()));
			if (xmlDocument != null)
			{
				XmlNode xmlNode = xmlDocument["Root"]["Versions"];
				if (xmlNode != null)
				{
					string value = currentVersion.ToString();
					xmlNode["Version"].Attributes["Value"].Value = value;
					string dataVersionText = xmlNode["DataVersion"].Attributes["Value"].GetStringOrDefault(string.Empty);
					pAMHFPMEPCH2.SetVersion(dataVersionText);
				}
				SystemProperties.SetVersions(currentVersion, pAMHFPMEPCH2);
				string userDataPath = string.Format("{0}/{1}", SF2Paths.GetUserDataDirectory(), Constants.UsersFileName);
				XmlUtils.SaveDocumentWithHash(xmlDocument, userDataPath);
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
		XmlDocument perksDocument = null;
		PerksCompiler.CompilePerks(ref perksDocument, string.Format("{0}/{1}", SF2Paths.GetGameDataPath(), "perks.xml"));
		if (perksDocument != null)
		{
			XmlNode xmlNode = perksDocument["Perks"];
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

	private static void ParseCharacterProgress(XmlNode progressNode)
	{
		XmlNode thresholdsNode = progressNode["Thresholds"];
		XmlNode xmlNode = progressNode["LotteryThresholds"];
		XmlNode perksNode = progressNode["Perks"];
		XmlNode hKPPBKPJOEO2 = progressNode["LevelAttributeGain"];
		XmlNode hKPPBKPJOEO3 = progressNode["StartingAttributes"];
		XmlNode hKPPBKPJOEO4 = progressNode["PerkTree"];
		XmlNode hKPPBKPJOEO5 = progressNode["CurrencyBaseValues"];
		XmlNode hKPPBKPJOEO6 = progressNode["MoneyBaseValues"];
		GameUtils.LevelThresholdTable.Parse(thresholdsNode);
		GameUtils.PerkItemList.ParseProgression(perksNode);
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
