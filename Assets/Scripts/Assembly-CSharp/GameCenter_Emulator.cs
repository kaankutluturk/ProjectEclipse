using UnityEngine.SocialPlatforms;

public class GameCenter_Emulator : GameCenterAbstract
{
	public override bool IsSupported
	{
		get
		{
			return GetIsSupported();
		}
	}

	public override bool CanAutoSignIn
	{
		get
		{
			return GetCanAutoSignIn();
		}
	}

	public override bool IsAuthenticated
	{
		get
		{
			return GetIsAuthenticated();
		}
	}

	public override string UserId
	{
		get
		{
			return GetUserId();
		}
	}

	public override void Init()
	{
		Log("[GameCenter_Emulator]: Init");
	}

	public override void Free()
	{
		Log("[GameCenter_Emulator]: Free");
	}

	private void HandleAuthenticated(bool BPEIMKJIMOF)
	{
		Log("[GameCenter_Emulator]: CB_Authenticate, authed = " + BPEIMKJIMOF);
	}

	private void HandleAchievementsLoaded(IAchievement[] HELFDCAIJNE)
	{
		Log("[GameCenter_Emulator]: CB_LoadAchievements");
	}

	private void HandleAchievementUnlocked(string OKNNNLIPODI)
	{
		Log("[GameCenter_Emulator]: CB_AchievementUnlocked, id = " + OKNNNLIPODI);
	}

	private void HandleAchievementProgress(string HMDBGGEMICE, int EPFBHJBNIHK)
	{
		Log("[GameCenter_Emulator]: OnAchievementProgess id = " + HMDBGGEMICE + " progress = " + EPFBHJBNIHK);
	}

	public override bool GetIsSupported()
	{
		return false;
	}

	public override bool GetCanAutoSignIn()
	{
		return false;
	}

	public override void SignIn()
	{
		Log("[GameCenter_Emulator]: SignIn");
	}

	public override void SignOut()
	{
		Log("[GameCenter_Emulator]: SignOut");
	}

	public override bool GetIsAuthenticated()
	{
		return false;
	}

	public override string GetUserId()
	{
		return string.Empty;
	}

	public override void LoadAchievements()
	{
	}

	public override void ShowAchievements()
	{
		Log("[GameCenter_Emulator]: ShowAchievements");
	}

	public override void ResetAchievements()
	{
		Log("[GameCenter_Emulator]: ResetAchievements");
		GameCenterAbstract.OnResetAchievements(true);
	}

	public override void UnlockAchievement(string OKNNNLIPODI)
	{
		Log("[GameCenter_Emulator]: UnlockAchievement " + OKNNNLIPODI);
	}

	public override void ReportAchievementProgress(string OKNNNLIPODI, double EPFBHJBNIHK)
	{
		Log("[GameCenter_Emulator]: AchievementProgress id = " + OKNNNLIPODI + " ,progress = " + EPFBHJBNIHK);
	}
}
