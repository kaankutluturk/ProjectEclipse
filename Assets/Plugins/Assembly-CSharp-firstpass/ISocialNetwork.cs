using Nekki.Social;

public interface ISocialNetwork
{
	bool IsSupported { get; }

	bool HasPayments { get; }

	SocialNetworkType NetworkType { get; }

	void Init(SocialWrapper socialWrapper);

	void GetUser(string userId);

	void GetUsers(string[] userIds);

	void RequestFriends();

	void InviteFriends();

	void CheckBookmark();

	void AddBookmark();

	void CheckGroupMembership();

	void PostToWall(string ownerId, string message, string attachments);

	void Buy(int item);

	void Buy(string item);

	bool GetIsSupported();

	bool GetHasPayments();

	SocialNetworkType GetNetworkType();
}
