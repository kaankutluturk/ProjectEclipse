using System;

public sealed class SignalRWebSocketTransport : TransportBase
{
	private WebSocket wSocket;

	public override bool SupportsKeepAlive
	{
		get
		{
			return GetSupportsKeepAlive();
		}
	}

	public SignalRWebSocketTransport(Connection connection)
		: base("webSockets", connection)
	{
	}

	public override bool GetSupportsKeepAlive()
	{
		return true;
	}

	public override TransportTypes get_Type()
	{
		return TransportTypes.WebSocket;
	}

	public override void Connect()
	{
		if (wSocket != null)
		{
			HTTPManager.GetLogger().Warning("WebSocketTransport", "Start - WebSocket already created!");
			return;
		}
		if (GetState() != TransportStates.Reconnecting)
		{
			set_State(TransportStates.Connecting);
		}
		SignalRRequestType requestType = ((GetState() != TransportStates.Reconnecting) ? SignalRRequestType.Connect : SignalRRequestType.Reconnect);
		Uri uri = GetConnection().BuildUri(requestType, this);
		wSocket = new WebSocket(uri);
		WebSocket webSocket = wSocket;
		webSocket.OnOpen = (OnWebSocketOpenDelegate)Delegate.Combine(webSocket.OnOpen, new OnWebSocketOpenDelegate(WSocket_OnOpen));
		WebSocket gPDLJHEAEDF2 = wSocket;
		gPDLJHEAEDF2.OnMessage = (OnWebSocketMessageDelegate)Delegate.Combine(gPDLJHEAEDF2.OnMessage, new OnWebSocketMessageDelegate(WSocket_OnMessage));
		WebSocket gPDLJHEAEDF3 = wSocket;
		gPDLJHEAEDF3.OnClosed = (OnWebSocketClosedDelegate)Delegate.Combine(gPDLJHEAEDF3.OnClosed, new OnWebSocketClosedDelegate(WSocket_OnClosed));
		WebSocket gPDLJHEAEDF4 = wSocket;
		gPDLJHEAEDF4.OnErrorDesc = (OnWebSocketErrorDescriptionDelegate)Delegate.Combine(gPDLJHEAEDF4.OnErrorDesc, new OnWebSocketErrorDescriptionDelegate(WSocket_OnError));
		GetConnection().PrepareRequest(wSocket.GetInternalRequest(), requestType);
		wSocket.OpenWebSocket();
	}

	protected override void SendImpl(string payload)
	{
		if (wSocket != null && wSocket.GetIsOpen())
		{
			wSocket.Send(payload);
		}
	}

	public override void Stop()
	{
		if (wSocket != null && wSocket.GetIsOpen())
		{
			wSocket.OnOpen = null;
			wSocket.OnMessage = null;
			wSocket.OnClosed = null;
			wSocket.OnErrorDesc = null;
			wSocket.Close();
			wSocket = null;
		}
	}

	protected override void OnStarted()
	{
	}

	protected override void OnAborted()
	{
		if (wSocket != null && wSocket.GetIsOpen())
		{
			wSocket.Close();
			wSocket = null;
		}
	}

	private void WSocket_OnOpen(WebSocket webSocket)
	{
		if (webSocket == wSocket)
		{
			HTTPManager.GetLogger().Information("WebSocketTransport", "WSocket_OnOpen");
			OnConnected();
		}
	}

	private void WSocket_OnMessage(WebSocket webSocket, string message)
	{
		if (webSocket == wSocket)
		{
			IServerMessage serverMessage = TransportBase.Parse(GetConnection().GetJsonEncoder(), message);
			if (serverMessage != null)
			{
				GetConnection().OnMessage(serverMessage);
			}
		}
	}

	private void WSocket_OnClosed(WebSocket webSocket, ushort code, string message)
	{
		if (webSocket == wSocket)
		{
			string text = code + " : " + message;
			HTTPManager.GetLogger().Information("WebSocketTransport", "WSocket_OnClosed " + text);
			if (GetState() == TransportStates.Closing)
			{
				set_State(TransportStates.Closed);
			}
			else
			{
				GetConnection().Error(text);
			}
		}
	}

	private void WSocket_OnError(WebSocket webSocket, string error)
	{
		if (webSocket == wSocket)
		{
			if (GetState() == TransportStates.Closing || GetState() == TransportStates.Closed)
			{
				AbortFinished();
				return;
			}
			HTTPManager.GetLogger().Error("WebSocketTransport", "WSocket_OnError " + error);
			GetConnection().Error(error);
		}
	}
}
