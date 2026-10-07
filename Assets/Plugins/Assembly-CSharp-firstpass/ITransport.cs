using System.Collections.Generic;

public interface ITransport
{
	SocketIOTransportState CurrentState { get; }

	SocketManager TransportManager { get; }

	bool HasPendingRequest { get; }

	SocketIOTransportState GetState();

	SocketManager GetManager();

	bool GetIsRequestInProgress();

	void OpenTransport();

	void Poll();

	void Send(Packet NPKADBPBKIG);

	void Send(List<Packet> DPGGBKDLDJE);

	void Close();
}
