public interface IHub
{
	Connection AttachedConnection { get; set; }

	Connection HubConnection { get; }

	void AttachConnection(Connection value);

	void Call(ClientMessage message);

	bool HasSentMessageId(ulong messageId);

	void Close();

	void OnMethod(MethodCallMessage message);

	void OnMessage(IServerMessage message);
}
