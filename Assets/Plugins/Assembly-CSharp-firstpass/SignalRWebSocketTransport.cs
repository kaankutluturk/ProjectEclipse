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

	public SignalRWebSocketTransport(Connection MDGFGCDPGFI)
		: base("webSockets", MDGFGCDPGFI)
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
		SignalRRequestType lFLGCDNKNJI = ((GetState() != TransportStates.Reconnecting) ? SignalRRequestType.Connect : SignalRRequestType.Reconnect);
		Uri kJHNCLAJMLO = GetConnection().BuildUri(lFLGCDNKNJI, this);
		wSocket = new WebSocket(kJHNCLAJMLO);
		WebSocket gPDLJHEAEDF = wSocket;
		gPDLJHEAEDF.OnOpen = (OnWebSocketOpenDelegate)Delegate.Combine(gPDLJHEAEDF.OnOpen, new OnWebSocketOpenDelegate(WSocket_OnOpen));
		WebSocket gPDLJHEAEDF2 = wSocket;
		gPDLJHEAEDF2.OnMessage = (OnWebSocketMessageDelegate)Delegate.Combine(gPDLJHEAEDF2.OnMessage, new OnWebSocketMessageDelegate(WSocket_OnMessage));
		WebSocket gPDLJHEAEDF3 = wSocket;
		gPDLJHEAEDF3.OnClosed = (OnWebSocketClosedDelegate)Delegate.Combine(gPDLJHEAEDF3.OnClosed, new OnWebSocketClosedDelegate(WSocket_OnClosed));
		WebSocket gPDLJHEAEDF4 = wSocket;
		gPDLJHEAEDF4.OnErrorDesc = (OnWebSocketErrorDescriptionDelegate)Delegate.Combine(gPDLJHEAEDF4.OnErrorDesc, new OnWebSocketErrorDescriptionDelegate(WSocket_OnError));
		GetConnection().PrepareRequest(wSocket.GetInternalRequest(), lFLGCDNKNJI);
		wSocket.OpenWebSocket();
	}

	protected override void SendImpl(string EMDHMHOKGFP)
	{
		if (wSocket != null && wSocket.GetIsOpen())
		{
			wSocket.Send(EMDHMHOKGFP);
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

	private void WSocket_OnOpen(WebSocket ILNFPNFEOCL)
	{
		if (ILNFPNFEOCL == wSocket)
		{
			HTTPManager.GetLogger().Information("WebSocketTransport", "WSocket_OnOpen");
			OnConnected();
		}
	}

	private void WSocket_OnMessage(WebSocket ILNFPNFEOCL, string LIOGIBJBHAH)
	{
		if (ILNFPNFEOCL == wSocket)
		{
			IServerMessage bNGPAAAKBOP = TransportBase.Parse(GetConnection().GetJsonEncoder(), LIOGIBJBHAH);
			if (bNGPAAAKBOP != null)
			{
				GetConnection().OnMessage(bNGPAAAKBOP);
			}
		}
	}

	private void WSocket_OnClosed(WebSocket ILNFPNFEOCL, ushort KJPGKHJNOMC, string LIOGIBJBHAH)
	{
		if (ILNFPNFEOCL == wSocket)
		{
			string text = KJPGKHJNOMC + " : " + LIOGIBJBHAH;
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

	private void WSocket_OnError(WebSocket ILNFPNFEOCL, string NEPOLDCKNJL)
	{
		if (ILNFPNFEOCL == wSocket)
		{
			if (GetState() == TransportStates.Closing || GetState() == TransportStates.Closed)
			{
				AbortFinished();
				return;
			}
			HTTPManager.GetLogger().Error("WebSocketTransport", "WSocket_OnError " + NEPOLDCKNJL);
			GetConnection().Error(NEPOLDCKNJL);
		}
	}
}
