using System.Xml;

public static class GameLoader
{
	private const int MaxPlayableSounds = 10;

	public static void LoadSettings()
	{
		GameSettings.InitUserDataValidation();
		GameSettings.CheckVersions();
		GameSettings.InitVersion();
	}

	public static void PreloadArmorSound()
	{
		Sound.PlaySound("snd_armor", 0f);
	}

	public static void SetSound(uint maxPlayableSounds)
	{
		Sound.MaxPlayableSounds = maxPlayableSounds;
	}

	public static void SetSound()
	{
		Sound.Init();
		SetSound(10u);
		PreloadArmorSound();
	}

	public static void LoadAnimations()
	{
		AnimationData.Load(SF2Paths.GetAnimationsPath(), SystemProperties.IsDebug());
	}

	public static void LoadAi()
	{
		AiData.Load();
	}

	public static void SetVersion(string version)
	{
		XmlDocument xmlDocument = XmlUtils.LoadDocumentWithHashCheck(SF2Paths.GetUserDataDirectory(), Constants.UsersFileName);
		if (xmlDocument != null)
		{
			xmlDocument["Root"]["Versions"]["Version"].SetAttribute("Value", version);
			string savePath = string.Format("{0}/{1}", SF2Paths.GetUserDataDirectory(), Constants.UsersFileName);
			string kPFELJFPGHJ2 = string.Format("{0}/{1}", SF2Paths.GetUserDataDirectory(), Constants.UsersBackupFileName);
			XmlUtils.SaveDocumentWithHash(xmlDocument, savePath);
			XmlUtils.SaveDocumentWithHash(xmlDocument, kPFELJFPGHJ2);
		}
	}
}
