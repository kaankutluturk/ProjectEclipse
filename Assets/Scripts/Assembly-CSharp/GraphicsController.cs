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
		Roster roster = ListSF.GetRoster();
		if (roster == null) return;
		roster.SessionSettings("ControllerScale", value.ToString());
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
		QualityOption.QualityLevel savedLevel = QualityOption.ParseQualityLevel(text);
		QualityOption.QualityLevel maxQualityLevel = QualityOption.ParseQualityLevel(SystemProperties.GetQualityConditionName());
		if (savedLevel <= maxQualityLevel)
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

	public static string GetNextGraphicsQuality(string currentCondition)
	{
		string text = SystemProperties.GetQualityConditionName();
		string text2 = QualityOption.GetNextQualityCondition(currentCondition, text);
		if (QualityOption.CompareQualityCondition(text2, text))
		{
			return currentCondition;
		}
		return text2;
	}

	public static void ToggleLocationResolution()
	{
		SystemProperties.PathType newResolution = ((GetLocationResolution() == SystemProperties.PathType.PATH_SMALL) ? SystemProperties.PathType.PATH_BIG : SystemProperties.PathType.PATH_SMALL);
		SetLocationResolution(newResolution);
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
		string resolutionSetting = LoadLocationResolutionSetting();
		return ParseLocationResolution(resolutionSetting);
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
		Roster roster = ListSF.GetRoster();
		if (roster != null && roster.HasSessionSetting("ControllerScale"))
		{
			result = roster.GetSettingsXML("ControllerScale") == "True" || roster.GetSettingsXML("ControllerScale") == "1";
		}
		return result;
	}

	private static string LoadQualityConditionSetting()
	{
		Roster roster = ListSF.GetRoster();
		if (!roster.HasSessionSetting("QualityCondition"))
		{
			SetQualityCondition(SystemProperties.GetQualityConditionName());
		}
		return roster.GetSettingsXML("QualityCondition");
	}

	private static void SaveLocationResolution(string value)
	{
		Roster roster = ListSF.GetRoster();
		roster.SessionSettings("LocationResolution", value);
		ListSF.GetInstance().RequestSave();
	}

	private static string LoadLocationResolutionSetting()
	{
		Roster roster = ListSF.GetRoster();
		if (!roster.HasSessionSetting("LocationResolution"))
		{
			string defaultResolution = LocationResolutionToString(SystemProperties.GetLocationPathType());
			SaveLocationResolution(defaultResolution);
		}
		return roster.GetSettingsXML("LocationResolution");
	}
}
