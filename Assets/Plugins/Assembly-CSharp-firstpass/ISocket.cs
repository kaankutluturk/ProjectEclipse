internal interface ISocket
{
	void Open();

	void Disconnect(bool removeFromManager);

	void OnPacket(Packet packet);

	void EmitEvent(SocketIOEventType eventType, params object[] args);

	void EmitEvent(string eventName, params object[] args);

	void EmitError(SocketIOErrors errorType, string message);
}
