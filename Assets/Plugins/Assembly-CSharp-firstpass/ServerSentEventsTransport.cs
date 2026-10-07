using System;

public sealed class ServerSentEventsTransport : PostSendTransportBase
{
	private EventSource eventSource;

	public override bool SupportsKeepAlive
	{
		get
		{
			return GetSupportsKeepAlive();
		}
	}

	public ServerSentEventsTransport(Connection EPDOEDFFPFD)
		: base("serverSentEvents", EPDOEDFFPFD)
	{
	}

	public override bool GetSupportsKeepAlive()
	{
		return true;
	}

	public override TransportTypes get_Type()
	{
		return TransportTypes.ServerSentEvents;
	}

	public override void Connect()
	{
		if (eventSource != null)
		{
			HTTPManager.GetLogger().Warning("ServerSentEventsTransport", "Start - EventSource already created!");
			return;
		}
		if (GetState() != TransportStates.Reconnecting)
		{
			set_State(TransportStates.Connecting);
		}
		SignalRRequestType lFLGCDNKNJI = ((GetState() != TransportStates.Reconnecting) ? SignalRRequestType.Connect : SignalRRequestType.Reconnect);
		Uri kJHNCLAJMLO = GetConnection().BuildUri(lFLGCDNKNJI, this);
		eventSource = new EventSource(kJHNCLAJMLO);
		eventSource.AddOnOpen(OnEventSourceOpen);
		eventSource.AddOnMessage(OnEventSourceMessage);
		eventSource.AddOnError(OnEventSourceError);
		eventSource.AddOnClosed(OnEventSourceClosed);
		eventSource.AddOnRetry((EventSource LDKKPKBGFOK) => false);
		eventSource.OpenEventSource();
	}

	public override void Stop()
	{
		eventSource.RemoveOnOpen(OnEventSourceOpen);
		eventSource.RemoveOnMessage(OnEventSourceMessage);
		eventSource.RemoveOnError(OnEventSourceError);
		eventSource.RemoveOnClosed(OnEventSourceClosed);
		eventSource.Close();
		eventSource = null;
	}

	protected override void OnStarted()
	{
	}

	public override void Abort()
	{
		base.Abort();
		eventSource.Close();
	}

	protected override void OnAborted()
	{
		if (GetState() == TransportStates.Closing)
		{
			set_State(TransportStates.Closed);
		}
	}

	private void OnEventSourceOpen(EventSource GLFHBCIPCBD)
	{
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "OnEventSourceOpen");
	}

	private void OnEventSourceMessage(EventSource GLFHBCIPCBD, Message LIOGIBJBHAH)
	{
		if (LIOGIBJBHAH.GetData().Equals("initialized"))
		{
			OnConnected();
			return;
		}
		IServerMessage bNGPAAAKBOP = TransportBase.Parse(GetConnection().GetJsonEncoder(), LIOGIBJBHAH.GetData());
		if (bNGPAAAKBOP != null)
		{
			GetConnection().OnMessage(bNGPAAAKBOP);
		}
	}

	private void OnEventSourceError(EventSource GLFHBCIPCBD, string JDONBAPIJCG)
	{
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "OnEventSourceError");
		if (GetState() == TransportStates.Reconnecting)
		{
			Connect();
		}
		else if (GetState() != TransportStates.Closed)
		{
			if (GetState() == TransportStates.Closing)
			{
				set_State(TransportStates.Closed);
			}
			else
			{
				GetConnection().Error(JDONBAPIJCG);
			}
		}
	}

	private void OnEventSourceClosed(EventSource GLFHBCIPCBD)
	{
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "OnEventSourceClosed");
		OnEventSourceError(GLFHBCIPCBD, "EventSource Closed!");
	}
}
