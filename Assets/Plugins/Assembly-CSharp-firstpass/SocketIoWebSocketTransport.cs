using System;
using System.Collections.Generic;
using System.Diagnostics;

internal sealed class SocketIoWebSocketTransport : ITransport
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SocketIOTransportState state;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SocketManager manager;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isRequestInProgress;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private WebSocket implementation;

	private Packet packetWithAttachment;

	private byte[] Buffer;

	public SocketIOTransportState CurrentState
	{
		get
		{
			return GetState();
		}
		private set
		{
			set_State(value);
		}
	}

	public SocketManager TransportManager
	{
		get
		{
			return GetManager();
		}
		private set
		{
			SetManager(value);
		}
	}

	public bool HasPendingRequest
	{
		get
		{
			return GetIsRequestInProgress();
		}
		private set
		{
			set_IsRequestInProgress(value);
		}
	}

	public WebSocket Implementation
	{
		get
		{
			return GetImplementation();
		}
		private set
		{
			SetImplementation(value);
		}
	}

	public SocketIoWebSocketTransport(SocketManager socketManager)
	{
		set_State(SocketIOTransportState.Closed);
		SetManager(socketManager);
	}

	public SocketIOTransportState GetState()
	{
		return state;
	}

	private void set_State(SocketIOTransportState value)
	{
		state = value;
	}

	public SocketManager GetManager()
	{
		return manager;
	}

	private void SetManager(SocketManager value)
	{
		manager = value;
	}

	public bool GetIsRequestInProgress()
	{
		return isRequestInProgress;
	}

	private void set_IsRequestInProgress(bool value)
	{
		isRequestInProgress = value;
	}

	public WebSocket GetImplementation()
	{
		return implementation;
	}

	private void SetImplementation(WebSocket value)
	{
		implementation = value;
	}

	public void OpenTransport()
	{
		if (GetState() == SocketIOTransportState.Closed)
		{
			Uri uri = new Uri(string.Format("{0}?transport=websocket&sid={1}{2}", new UriBuilder("ws", GetManager().GetUri().Host, GetManager().GetUri().Port, GetManager().GetUri().PathAndQuery).Uri.ToString(), GetManager().GetHandshake().GetSid(), GetManager().GetOptions().GetQueryParamsOnlyForHandshake() ? string.Empty : GetManager().GetOptions().BuildQueryParams()));
			SetImplementation(new WebSocket(uri));
			GetImplementation().OnOpen = OnOpen;
			GetImplementation().OnMessage = OnMessage;
			GetImplementation().OnBinary = OnBinary;
			GetImplementation().OnError = OnError;
			GetImplementation().OnClosed = OnClosed;
			GetImplementation().OpenWebSocket();
			set_State(SocketIOTransportState.Connecting);
		}
	}

	public void Close()
	{
		if (GetState() != SocketIOTransportState.Closed)
		{
			set_State(SocketIOTransportState.Closed);
			GetImplementation().Close();
			SetImplementation(null);
		}
	}

	public void Poll()
	{
	}

	private void OnOpen(WebSocket webSocket)
	{
		HTTPManager.GetLogger().Information("WebSocketTransport", "OnOpen");
		set_State(SocketIOTransportState.Opening);
		Send(new Packet(TransportEventTypes.Ping, SocketIOEventType.Unknown, "/", "probe"));
	}

	private void OnMessage(WebSocket webSocket, string message)
	{
		if (HTTPManager.GetLogger().GetLevel() <= Loglevels.All)
		{
			HTTPManager.GetLogger().Verbose("WebSocketTransport", "OnMessage: " + message);
		}
		try
		{
			Packet packet = new Packet(message);
			if (packet.GetAttachmentCount() == 0)
			{
				OnPacket(packet);
			}
			else
			{
				packetWithAttachment = packet;
			}
		}
		catch (Exception exception)
		{
			HTTPManager.GetLogger().Exception("WebSocketTransport", "OnMessage", exception);
		}
	}

	private void OnBinary(WebSocket webSocket, byte[] data)
	{
		if (HTTPManager.GetLogger().GetLevel() <= Loglevels.All)
		{
			HTTPManager.GetLogger().Verbose("WebSocketTransport", "OnBinary");
		}
		if (packetWithAttachment == null)
		{
			return;
		}
		packetWithAttachment.AddAttachmentFromServer(data, false);
		if (!packetWithAttachment.GetHasAllAttachment())
		{
			return;
		}
		try
		{
			OnPacket(packetWithAttachment);
		}
		catch (Exception exception)
		{
			HTTPManager.GetLogger().Exception("WebSocketTransport", "OnBinary", exception);
		}
		finally
		{
			packetWithAttachment = null;
		}
	}

	private void OnError(WebSocket webSocket, Exception exception)
	{
		string text = string.Empty;
		if (exception != null)
		{
			text = exception.Message + " " + exception.StackTrace;
		}
		else
		{
			switch (webSocket.GetInternalRequest().GetState())
			{
			case HTTPRequestStates.Finished:
				text = ((!webSocket.GetInternalRequest().GetResponse().GetIsSuccess() && webSocket.GetInternalRequest().GetResponse().GetStatusCode() != 101) ? string.Format("Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", webSocket.GetInternalRequest().GetResponse().GetStatusCode(), webSocket.GetInternalRequest().GetResponse().GetMessage(), webSocket.GetInternalRequest().GetResponse().GetDataAsText()) : string.Format("Request finished. Status Code: {0} Message: {1}", webSocket.GetInternalRequest().GetResponse().GetStatusCode()
					.ToString(), webSocket.GetInternalRequest().GetResponse().GetMessage()));
				break;
			case HTTPRequestStates.Error:
				text = (("Request Finished with Error! : " + webSocket.GetInternalRequest().GetException() == null) ? string.Empty : (webSocket.GetInternalRequest().GetException().Message + " " + webSocket.GetInternalRequest().GetException().StackTrace));
				break;
			case HTTPRequestStates.Aborted:
				text = "Request Aborted!";
				break;
			case HTTPRequestStates.ConnectionTimedOut:
				text = "Connection Timed Out!";
				break;
			case HTTPRequestStates.TimedOut:
				text = "Processing the request Timed Out!";
				break;
			}
		}
		HTTPManager.GetLogger().Error("WebSocketTransport", "OnError: " + text);
		((IManager)GetManager()).OnTransportError((ITransport)this, text);
	}

	private void OnClosed(WebSocket webSocket, ushort code, string message)
	{
		HTTPManager.GetLogger().Information("WebSocketTransport", "OnClosed");
		Close();
		((IManager)GetManager()).TryToReconnect();
	}

	public void Send(Packet packet)
	{
		if (GetState() == SocketIOTransportState.Closed || GetState() == SocketIOTransportState.Paused)
		{
			return;
		}
		string text = packet.Encode();
		if (HTTPManager.GetLogger().GetLevel() <= Loglevels.All)
		{
			HTTPManager.GetLogger().Verbose("WebSocketTransport", "Send: " + text);
		}
		if (packet.GetAttachmentCount() != 0 || (packet.GetAttachments() != null && packet.GetAttachments().Count != 0))
		{
			if (packet.GetAttachments() == null)
			{
				throw new ArgumentException("packet.Attachments are null!");
			}
			if (packet.GetAttachmentCount() != packet.GetAttachments().Count)
			{
				throw new ArgumentException("packet.AttachmentCount != packet.Attachments.Count. Use the packet.AddAttachment function to add data to a packet!");
			}
		}
		GetImplementation().Send(text);
		if (packet.GetAttachmentCount() == 0)
		{
			return;
		}
		int num = packet.GetAttachments()[0].Length + 1;
		for (int i = 1; i < packet.GetAttachments().Count; i++)
		{
			if (packet.GetAttachments()[i].Length + 1 > num)
			{
				num = packet.GetAttachments()[i].Length + 1;
			}
		}
		if (Buffer == null || Buffer.Length < num)
		{
			Array.Resize(ref Buffer, num);
		}
		for (int j = 0; j < packet.GetAttachmentCount(); j++)
		{
			Buffer[0] = 4;
			Array.Copy(packet.GetAttachments()[j], 0, Buffer, 1, packet.GetAttachments()[j].Length);
			GetImplementation().Send(Buffer, 0uL, (ulong)packet.GetAttachments()[j].Length + 1uL);
		}
	}

	public void Send(List<Packet> packets)
	{
		for (int i = 0; i < packets.Count; i++)
		{
			Send(packets[i]);
		}
		packets.Clear();
	}

	private void OnPacket(Packet packet)
	{
		switch (packet.GetTransportEvent())
		{
		case TransportEventTypes.Message:
			if (packet.GetSocketIOEvent() == SocketIOEventType.Connect && GetState() == SocketIOTransportState.Opening)
			{
				set_State(SocketIOTransportState.Open);
				if (!((IManager)GetManager()).OnTransportConnected((ITransport)this))
				{
					return;
				}
			}
			break;
		case TransportEventTypes.Pong:
			if (packet.GetPayload() == "probe")
			{
				HTTPManager.GetLogger().Information("WebSocketTransport", "\"probe\" packet received, sending Upgrade packet");
				Send(new Packet(TransportEventTypes.Upgrade, SocketIOEventType.Event, "/", string.Empty));
			}
			break;
		}
		((IManager)GetManager()).OnPacket(packet);
	}
}
