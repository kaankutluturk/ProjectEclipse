using Nekki.Social;

public interface ISocialNetwork
{
	bool IsSupported { get; }

	bool HasPayments { get; }

	SocialNetworkType NetworkType { get; }

	void Init(SocialWrapper OBPHBNCBNCN);

	void GetUser(string AOKMNKOIMHI);

	void GetUsers(string[] JAIEEFOCDAA);

	void RequestFriends();

	void InviteFriends();

	void CheckBookmark();

	void AddBookmark();

	void CheckGroupMembership();

	void PostToWall(string AOKMNKOIMHI, string LIOGIBJBHAH, string DMNBDBJNKME);

	void Buy(int item);

	void Buy(string item);

	bool GetIsSupported();

	bool GetHasPayments();

	SocialNetworkType GetNetworkType();
}
