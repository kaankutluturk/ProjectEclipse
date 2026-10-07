public sealed class KeepAliveMessage : IServerMessage
{
	public MessageTypes get_Type()
	{
		return MessageTypes.KeepAlive;
	}

	void IServerMessage.Parse(object data)
	{
	}
}
