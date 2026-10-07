internal interface IManager
{
	void Remove(Socket socket);

	void Close(bool removeSockets = true);

	void TryToReconnect();

	bool OnTransportConnected(ITransport transport);

	void OnTransportError(ITransport transport, string error);

	void SendPacket(Packet packet);

	void OnPacket(Packet packet);

	void EmitEvent(string eventName, params object[] args);

	void EmitEvent(SocketIOEventType eventType, params object[] args);

	void EmitError(SocketIOErrors errorCode, string message);

	void EmitAll(string eventName, params object[] args);
}
