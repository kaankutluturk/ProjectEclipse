public static class GraphicsController
{
	private static string cachedQualityCondition = string.Empty;

	// best guess for name

	public static void ToggleControlSize()
	{
		bool flag = LargeControlsEnabled();
		SetIsBigController(!flag);
	}

	public static void SetIsBigController(bool value)
	{
		UnityEngine.PlayerPrefs.SetInt("Eclipse.LargeControls", value ? 1 : 0);
		UnityEngine.PlayerPrefs.Save();
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP == null) return;
		nKGLHEGIKKP.SessionSettings("ControllerScale", value.ToString());
		ListSF.GetInstance().RequestSave();
	}

	// best guess for name

	public static bool LargeControlsEnabled()
	{
		return ReadLargeControlsSetting();
	}

	public static string GetEffectiveQualityCondition()
	{
		string text = GetSavedQualityCondition();
		if (text == string.Empty)
		{
			return SystemProperties.GetQualityConditionName();
		}
		QualityOption.QualityLevel hPNJCDGIHLI = QualityOption.ParseQualityLevel(text);
		QualityOption.QualityLevel hPNJCDGIHLI2 = QualityOption.ParseQualityLevel(SystemProperties.GetQualityConditionName());
		if (hPNJCDGIHLI <= hPNJCDGIHLI2)
		{
			return text;
		}
		return SystemProperties.GetQualityConditionName();
	}

	public static void SetQualityCondition(string value)
	{
		cachedQualityCondition = value;
		ListSF.GetRoster().SessionSettings("QualityCondition", cachedQualityCondition);
		ListSF.GetInstance().RequestSave(1);
	}

	public static string GetSavedQualityCondition()
	{
		if (cachedQualityCondition == string.Empty)
		{
			cachedQualityCondition = LoadQualityConditionSetting();
		}
		return cachedQualityCondition;
	}

	public static bool CycleQualityCondition()
	{
		string text = GetEffectiveQualityCondition();
		string text2 = GetNextGraphicsQuality(text);
		bool flag = text != text2;
		if (flag)
		{
			SetQualityCondition(text2);
		}
		return flag;
	}

	public static string GetNextGraphicsQuality(string HEPNIDFNHBA)
	{
		string text = SystemProperties.GetQualityConditionName();
		string text2 = QualityOption.GetNextQualityCondition(HEPNIDFNHBA, text);
		if (QualityOption.CompareQualityCondition(text2, text))
		{
			return HEPNIDFNHBA;
		}
		return text2;
	}

	public static void ToggleLocationResolution()
	{
		SystemProperties.PathType bAINMLLIKOL = ((GetLocationResolution() == SystemProperties.PathType.PATH_SMALL) ? SystemProperties.PathType.PATH_BIG : SystemProperties.PathType.PATH_SMALL);
		SetLocationResolution(bAINMLLIKOL);
	}

	public static void SetLocationResolution(SystemProperties.PathType value)
	{
		string text = LocationResolutionToString(value);
		if (text != string.Empty)
		{
			SaveLocationResolution(text);
		}
	}

	public static SystemProperties.PathType GetLocationResolution()
	{
		string bAINMLLIKOL = LoadLocationResolutionSetting();
		return ParseLocationResolution(bAINMLLIKOL);
	}

	public static SystemProperties.PathType ParseLocationResolution(string value)
	{
		SystemProperties.PathType result = SystemProperties.PathType.PATH_DEFAULT;
		if (value == "HIGH")
		{
			result = SystemProperties.PathType.PATH_BIG;
		}
		else if (value == "LOW")
		{
			result = SystemProperties.PathType.PATH_SMALL;
		}
		return result;
	}

	public static string LocationResolutionToString(SystemProperties.PathType value)
	{
		string empty = string.Empty;
		switch (value)
		{
		case SystemProperties.PathType.PATH_BIG:
			return "HIGH";
		case SystemProperties.PathType.PATH_SMALL:
			return "LOW";
		default:
			return string.Empty;
		}
	}

	private static bool ReadLargeControlsSetting()
	{
		if (UnityEngine.PlayerPrefs.HasKey("Eclipse.LargeControls"))
			return UnityEngine.PlayerPrefs.GetInt("Eclipse.LargeControls") != 0;
		bool result = !SystemProperties.IsTabletDevice();
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP != null && nKGLHEGIKKP.HasSessionSetting("ControllerScale"))
		{
			result = nKGLHEGIKKP.GetSettingsXML("ControllerScale") == "True" || nKGLHEGIKKP.GetSettingsXML("ControllerScale") == "1";
		}
		return result;
	}

	private static string LoadQualityConditionSetting()
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (!nKGLHEGIKKP.HasSessionSetting("QualityCondition"))
		{
			SetQualityCondition(SystemProperties.GetQualityConditionName());
		}
		return nKGLHEGIKKP.GetSettingsXML("QualityCondition");
	}

	private static void SaveLocationResolution(string value)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		nKGLHEGIKKP.SessionSettings("LocationResolution", value);
		ListSF.GetInstance().RequestSave();
	}

	private static string LoadLocationResolutionSetting()
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (!nKGLHEGIKKP.HasSessionSetting("LocationResolution"))
		{
			string bAINMLLIKOL = LocationResolutionToString(SystemProperties.GetLocationPathType());
			SaveLocationResolution(bAINMLLIKOL);
		}
		return nKGLHEGIKKP.GetSettingsXML("LocationResolution");
	}
}
