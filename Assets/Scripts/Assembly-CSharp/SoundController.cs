public class SoundController
{
    private static void SaveMusicSettings()
    {
        UnityEngine.PlayerPrefs.SetFloat("Eclipse.MusicVolume", Sound.GetMusicVolume());
        UnityEngine.PlayerPrefs.SetInt("Eclipse.MusicMuted", Sound.GetMusicMuted() ? 1 : 0);
        UnityEngine.PlayerPrefs.Save();
        ListSF.GetRoster()?.SaveMusicSettings();
    }

    private static void SaveSoundSettings()
    {
        UnityEngine.PlayerPrefs.SetFloat("Eclipse.SoundVolume", Sound.GetSoundVolume());
        UnityEngine.PlayerPrefs.SetInt("Eclipse.SoundMuted", Sound.GetSoundMuted() ? 1 : 0);
        UnityEngine.PlayerPrefs.Save();
        ListSF.GetRoster()?.SaveSoundSettings();
    }

    internal static void ApplySavedVolumes()
    {
        // Title-screen settings exist before a roster and take precedence once it loads.
        if (UnityEngine.PlayerPrefs.HasKey("Eclipse.MusicVolume"))
        {
            Sound.SetMusicVolume(UnityEngine.PlayerPrefs.GetFloat("Eclipse.MusicVolume"));
            Sound.SetMusicMuted(UnityEngine.PlayerPrefs.GetInt("Eclipse.MusicMuted") != 0);
            ListSF.IsSoundEnabled = Sound.GetMusicMuted();
        }
        if (UnityEngine.PlayerPrefs.HasKey("Eclipse.SoundVolume"))
        {
            Sound.SetSoundVolume(UnityEngine.PlayerPrefs.GetFloat("Eclipse.SoundVolume"));
            Sound.SetSoundMuted(UnityEngine.PlayerPrefs.GetInt("Eclipse.SoundMuted") != 0);
        }
    }

	public const string MUSIC_MENU = "menu";

	public static bool IsBackgroundMusicIntro;

	public static float MusicVolume
	{
		get
		{
			return GetMusicVolume();
		}
		set
		{
			SetMusicVolume(value);
		}
	}

	public static float SoundVolume
	{
		get
		{
			return GetSoundVolume();
		}
		set
		{
			SetSoundVolume(value);
		}
	}

	public static bool IsMusicMuted
	{
		get
		{
			return GetMusicMuted();
		}
		set
		{
			SetMusicMuted(value);
		}
	}

	public static bool IsSoundMuted
	{
		get
		{
			return GetSoundMuted();
		}
		set
		{
			SetSoundMuted(value);
		}
	}

	public static void StartBackgroundMusic(string name = "menu", bool KKHJAJFEPPA = true)
	{
		if (!IsBackgroundMusicIntro)
		{
			IsBackgroundMusicIntro = true;
			Sound.PlayMusic(name, KKHJAJFEPPA);
		}
	}

	public static void StopBackgroundMusic()
	{
		IsBackgroundMusicIntro = false;
		Sound.StopMusic();
	}

	// best guess for name

	public static float GetMusicVolume()
	{
		return (!Sound.GetMusicMuted()) ? Sound.GetMusicVolume() : 0f;
	}

	// best guess for name

	public static void SetMusicVolume(float value)
	{
		Sound.SetMusicVolume(value);
		bool flag = value <= 0f;
		bool flag2 = Sound.GetMusicMuted();
		if (flag && !flag2)
		{
			SetMusicMuted(true);
		}
		else if (!flag && flag2)
		{
			SetMusicMuted(false);
		}
		else
		{
			SaveMusicSettings();
		}
	}

	// best guess for name

	public static float GetSoundVolume()
	{
		return (!Sound.GetSoundMuted()) ? Sound.GetSoundVolume() : 0f;
	}

	// best guess for name

	public static void SetSoundVolume(float value)
	{
		Sound.SetSoundVolume(value);
		bool flag = value <= 0f;
		bool flag2 = Sound.GetSoundMuted();
		if (flag && !flag2)
		{
			SetSoundMuted(true);
		}
		else if (!flag && flag2)
		{
			SetSoundMuted(false);
		}
		else
		{
			SaveSoundSettings();
		}
	}

	public static bool GetMusicMuted()
	{
		return Sound.GetMusicMuted();
	}

	public static void SetMusicMuted(bool value)
	{
		if (value != Sound.GetMusicMuted())
		{
			if (!value && Sound.GetMusicVolume() == 0f)
			{
				Sound.SetMusicVolume(1f);
			}
			Sound.SetMusicMuted(value);
			ListSF.IsSoundEnabled = value;
			SaveMusicSettings();
		}
	}

	public static bool GetSoundMuted()
	{
		return Sound.GetSoundMuted();
	}

	public static void SetSoundMuted(bool value)
	{
		if (value != Sound.GetSoundMuted())
		{
			if (!value && Sound.GetSoundVolume() == 0f)
			{
				Sound.SetSoundVolume(1f);
			}
			Sound.SetSoundMuted(value);
			SaveSoundSettings();
		}
	}
}
