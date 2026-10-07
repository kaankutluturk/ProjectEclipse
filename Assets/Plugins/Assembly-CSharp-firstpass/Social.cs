using Nekki.Social;

public class Social
{
	private static ISocialNetwork network;

	private static SocialWrapper _wrap;

	public static UserInfo CurrentUser
	{
		get
		{
			return GetCurrentUser();
		}
	}

	public static ISocialNetwork Network
	{
		get
		{
			return GetNetwork();
		}
	}

	public static UserInfo GetCurrentUser()
	{
		return SocialWrapper.GetCurrentUser();
	}

	public static ISocialNetwork GetNetwork()
	{
		return network;
	}

	public static void Init(Callbacks callbacks)
	{
		_wrap = SocialWrapper.Init(callbacks, OnNetworkSelected);
	}

	private static void OnNetworkSelected(SocialNetworkType networkType)
	{
		if (networkType != SocialNetworkType.None && networkType == SocialNetworkType.VKontakte)
		{
			network = new VK();
			network.Init(_wrap);
		}
	}
}
