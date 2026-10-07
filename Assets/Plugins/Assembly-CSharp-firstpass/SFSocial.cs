using System.Collections.Generic;

public class SFSocial : global::EventDispatcher<object>
{
	public class Achievement
	{
		public string name;

		public int progress;

		public bool complete;

		public Achievement()
		{
			name = string.Empty;
			progress = 0;
			complete = false;
		}

		public Achievement(string _name, int JLHFNCKLMDI)
		{
			name = _name;
			progress = JLHFNCKLMDI;
			complete = false;
		}
	}

	public enum SocialEvent
	{
		EVENT_AUTHORIZE_FINISH = 0,
		EVENT_AUTHORIZE_CANCEL = 1,
		EVENT_POST_ACHIEVEMENT_FAILED = 2,
		EVENT_REQUEST_VERIFICATION_DATA_SUCCSESS = 3,
		EVENT_REQUEST_VERIFICATION_DATA_FAILED = 4
	}

	public enum SocialType
	{
		SOCIAL_NONE = 0,
		SOCIAL_NEKKI = 1,
		SOCIAL_GOOGLE = 2,
		SOCIAL_GAME_CENTER = 3
	}

	private static SFSocial instance;

	private static SFSocial secondaryInstance;

	public static void SetInstance(SFSocial ENMMMPLLLCD)
	{
		instance = ENMMMPLLLCD;
	}

	public static void SetSecondaryInstance(SFSocial ENMMMPLLLCD)
	{
		secondaryInstance = ENMMMPLLLCD;
	}

	public static SFSocial GetInstance()
	{
		instance = new SFSocial();
		return instance;
	}

	public virtual void Authorize()
	{
		CallEvent(0, 0);
	}

	public virtual void Login()
	{
		Authorize();
	}

	public virtual string GetPlayerId()
	{
		return string.Empty;
	}

	public virtual string GetSecondaryPlayerId()
	{
		if (secondaryInstance != null)
		{
			return secondaryInstance.GetPlayerId();
		}
		return string.Empty;
	}

	public virtual void PostAchievements(List<Achievement> CIMGCGDDKCE)
	{
	}

	public virtual void ResetAchievements()
	{
	}

	public virtual bool IsAuthorized()
	{
		return true;
	}

	public virtual void ShowAchievements()
	{
	}

	public virtual void ShowLeaderboards()
	{
	}

	public virtual bool IsAvailable()
	{
		return false;
	}

	public virtual string GetPlayerName()
	{
		return string.Empty;
	}

	public virtual void RequestVerificationData()
	{
	}

	protected void OnPostAchievementFailed(Achievement PGAGNLJABIE)
	{
		CallEvent(2, PGAGNLJABIE);
	}
}
