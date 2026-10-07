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

	private void HandleAuthenticated(bool isAuthenticated)
	{
		Log("[GameCenter_Emulator]: CB_Authenticate, authed = " + isAuthenticated);
	}

	private void HandleAchievementsLoaded(IAchievement[] achievements)
	{
		Log("[GameCenter_Emulator]: CB_LoadAchievements");
	}

	private void HandleAchievementUnlocked(string achievementId)
	{
		Log("[GameCenter_Emulator]: CB_AchievementUnlocked, id = " + achievementId);
	}

	private void HandleAchievementProgress(string achievementId, int progress)
	{
		Log("[GameCenter_Emulator]: OnAchievementProgess id = " + achievementId + " progress = " + progress);
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

	public override void UnlockAchievement(string achievementId)
	{
		Log("[GameCenter_Emulator]: UnlockAchievement " + achievementId);
	}

	public override void ReportAchievementProgress(string achievementId, double progress)
	{
		Log("[GameCenter_Emulator]: AchievementProgress id = " + achievementId + " ,progress = " + progress);
	}
}
