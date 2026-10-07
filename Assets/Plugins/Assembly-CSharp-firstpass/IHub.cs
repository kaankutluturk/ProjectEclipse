public interface IHub
{
	Connection PEBFDIFIMBO { get; set; }

	Connection HubConnection { get; }

	void GNLCPJFBAJE(Connection value);

	void Call(ClientMessage message);

	bool HasSentMessageId(ulong messageId);

	void Close();

	void OnMethod(MethodCallMessage message);

	void OnMessage(IServerMessage message);
}
