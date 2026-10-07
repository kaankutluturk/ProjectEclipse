using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public abstract class GameCenterAbstract
{
	public static Action<bool> OnAuthenticate = delegate
	{
	};

	public static Action<IAchievement[]> OnLoadAchievements = delegate
	{
	};

	public static Action<bool> OnResetAchievements = delegate
	{
	};

	public static Action<string> OnAchievementUnlocked = delegate
	{
	};

	public static Action<string, double> OnAchievementProgress = delegate
	{
	};

	public abstract bool IsSupported { get; }

	public abstract bool CanAutoSignIn { get; }

	public abstract bool IsAuthenticated { get; }

	public abstract string UserId { get; }

	public abstract void Init();

	public abstract void Free();

	public abstract bool GetIsSupported();

	public abstract bool GetCanAutoSignIn();

	public abstract void SignIn();

	public abstract void SignOut();

	public abstract bool GetIsAuthenticated();

	public abstract void LoadAchievements();

	public abstract void ShowAchievements();

	public abstract void ResetAchievements();

	public abstract void UnlockAchievement(string OKNNNLIPODI);

	public abstract void ReportAchievementProgress(string HMDBGGEMICE, double EPFBHJBNIHK);

	public abstract string GetUserId();

	protected void Log(string DMKMNOINKFC)
	{
		Debug.Log(DMKMNOINKFC);
	}
}
