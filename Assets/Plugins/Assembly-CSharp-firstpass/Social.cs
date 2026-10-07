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

	public static void Init(Callbacks EODBKOHACMO)
	{
		_wrap = SocialWrapper.Init(EODBKOHACMO, OnNetworkSelected);
	}

	private static void OnNetworkSelected(SocialNetworkType KPJKACAJHDF)
	{
		if (KPJKACAJHDF != SocialNetworkType.None && KPJKACAJHDF == SocialNetworkType.VKontakte)
		{
			network = new VK();
			network.Init(_wrap);
		}
	}
}
