using System;
using System.Collections.Generic;

public class Callbacks
{
	public Action<SocialNetworkType, string> OnInitialized;

	public Action<UserInfo> OnUserInfo;

	public Action<Dictionary<string, UserInfo>> OnFriendsInfo;

	public Action<Dictionary<string, UserInfo>> OnAppFriendsInfo;

	public Action<bool> OnBookmarkState;

	public Action<bool> OnGroupMembership;

	public Action<string> OnWallPostResult;

	public Action<string> OnBuyResult;

	public Action<bool> OnOrderCompleted;
}
