using Nekki.Social;

public class VK : ISocialNetwork
{
	private SocialWrapper _wrap;

	public bool IsSupported
	{
		get
		{
			return GetIsSupported();
		}
	}

	public bool HasPayments
	{
		get
		{
			return GetHasPayments();
		}
	}

	public SocialNetworkType NetworkType
	{
		get
		{
			return GetNetworkType();
		}
	}

	public void Init(SocialWrapper socialWrapper)
	{
		_wrap = socialWrapper;
	}

	public void GetUser(string userId)
	{
		if (!_wrap.get_Initialized())
		{
			AdvLog.LogWarning("you must call Social.Init(..) method first");
			return;
		}
		_wrap.RequestUsersInfo(new string[1] { userId });
	}

	public void GetUsers(string[] userIds)
	{
		if (!_wrap.get_Initialized())
		{
			AdvLog.LogWarning("you must call Social.Init(..) method first");
		}
		else
		{
			_wrap.RequestUsersInfo(userIds);
		}
	}

	public void RequestFriends()
	{
		if (!_wrap.get_Initialized())
		{
			AdvLog.LogWarning("you must call Social.Init(..) method first");
		}
		else
		{
			_wrap.RequestFriends();
		}
	}

	public void InviteFriends()
	{
		if (!_wrap.get_Initialized())
		{
			AdvLog.LogWarning("you must call Social.Init(..) method first");
		}
		else
		{
			_wrap.Invite();
		}
	}

	public void CheckBookmark()
	{
		if (!_wrap.get_Initialized())
		{
			AdvLog.LogWarning("you must call Social.Init(..) method first");
		}
		else
		{
			_wrap.RequestBookmark(false);
		}
	}

	public void AddBookmark()
	{
		if (!_wrap.get_Initialized())
		{
			AdvLog.LogWarning("you must call Social.Init(..) method first");
		}
		else
		{
			_wrap.RequestBookmark(true);
		}
	}

	public void CheckGroupMembership()
	{
		if (!_wrap.get_Initialized())
		{
			AdvLog.LogWarning("you must call Social.Init(..) method first");
		}
		else
		{
			_wrap.RequestCheckGroupMembership();
		}
	}

	public void PostToWall(string userId, string message, string picture)
	{
		if (!_wrap.get_Initialized())
		{
			AdvLog.LogWarning("you must call Social.Init(..) method first");
		}
		else
		{
			_wrap.RequestWallPost(userId, message, picture);
		}
	}

	public void Buy(int item)
	{
		if (!_wrap.get_Initialized())
		{
			AdvLog.LogWarning("you must call Social.Init(..) method first");
		}
		else
		{
			_wrap.Buy(item.ToString());
		}
	}

	public void Buy(string item)
	{
		if (!_wrap.get_Initialized())
		{
			AdvLog.LogWarning("you must call Social.Init(..) method first");
		}
		else
		{
			_wrap.Buy(item);
		}
	}

	public bool GetIsSupported()
	{
		return true;
	}

	public bool GetHasPayments()
	{
		return true;
	}

	public SocialNetworkType GetNetworkType()
	{
		return SocialNetworkType.VKontakte;
	}
}
