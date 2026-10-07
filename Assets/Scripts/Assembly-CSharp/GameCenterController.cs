using System.Collections.Generic;

public static class GameCenterController
{
	private static GameCenterAbstract _Current;

	public static GameCenterAbstract BLOOLFFMKFI
	{
		get
		{
			return GetCurrent();
		}
	}

	public static bool IsSupported
	{
		get
		{
			return GetIsSupported();
		}
	}

	public static bool CanAutoSignIn
	{
		get
		{
			return GetCanAutoSignIn();
		}
	}

	public static bool IsAuthenticated
	{
		get
		{
			return GetIsAuthenticated();
		}
	}

	public static string UserId
	{
		get
		{
			return GetUserId();
		}
	}

	public static GameCenterAbstract GetCurrent()
	{
		if (_Current == null)
		{
			Init();
		}
		return _Current;
	}

	public static void Init()
	{
		if (_Current == null)
		{
			_Current = new GameCenter_Emulator();
			_Current.Init();
		}
	}

	public static void Free()
	{
		GetCurrent().Free();
	}

	public static bool GetIsSupported()
	{
		return GetCurrent().GetIsSupported();
	}

	public static bool GetCanAutoSignIn()
	{
		return GetCurrent().GetCanAutoSignIn();
	}

	public static void SignIn()
	{
		if (GetIsSupported())
		{
			GetCurrent().SignIn();
		}
	}

	public static void SignOut()
	{
		if (GetIsSupported())
		{
			GetCurrent().SignOut();
		}
	}

	public static bool GetIsAuthenticated()
	{
		if (GetIsSupported())
		{
			return GetCurrent().GetIsAuthenticated();
		}
		return false;
	}

	public static string GetUserId()
	{
		if (GetIsAuthenticated())
		{
			return SystemProperties.MakeIdentifier(GetCurrent().GetUserId());
		}
		return string.Empty;
	}

	public static void UnlockAchievement(string OKNNNLIPODI)
	{
		if (GetIsAuthenticated())
		{
			GetCurrent().UnlockAchievement(OKNNNLIPODI);
		}
	}

	public static void ReportAchievementProgress(string OKNNNLIPODI, double EPFBHJBNIHK)
	{
		if (GetIsAuthenticated())
		{
			GetCurrent().ReportAchievementProgress(OKNNNLIPODI, EPFBHJBNIHK);
		}
	}

	public static void ShowAchievements()
	{
		if (GetIsAuthenticated())
		{
			GetCurrent().ShowAchievements();
		}
	}

	public static void LoadAchievements()
	{
		if (GetIsAuthenticated())
		{
			GetCurrent().LoadAchievements();
		}
	}

	public static void ResetAchievements()
	{
		if (GetIsAuthenticated() || SystemProperties.IsEditorPlatform())
		{
			GetCurrent().ResetAchievements();
		}
	}

	public static bool IsFeatureAvailable()
	{
		return false;
	}

	public static void SyncAchievements(List<SocialAchievement> CIMGCGDDKCE)
	{
		foreach (SocialAchievement item in CIMGCGDDKCE)
		{
			if (item.value < item.TargetValue)
			{
				int num = 0;
				num = item.value;
				ReportAchievementProgress(item.name, num);
			}
			else
			{
				UnlockAchievement(item.name);
			}
		}
	}
}
