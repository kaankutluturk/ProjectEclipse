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

	public ServerSentEventsTransport(Connection connection)
		: base("serverSentEvents", connection)
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
		SignalRRequestType requestType = ((GetState() != TransportStates.Reconnecting) ? SignalRRequestType.Connect : SignalRRequestType.Reconnect);
		Uri uri = GetConnection().BuildUri(requestType, this);
		eventSource = new EventSource(uri);
		eventSource.AddOnOpen(OnEventSourceOpen);
		eventSource.AddOnMessage(OnEventSourceMessage);
		eventSource.AddOnError(OnEventSourceError);
		eventSource.AddOnClosed(OnEventSourceClosed);
		eventSource.AddOnRetry((EventSource source) => false);
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

	private void OnEventSourceOpen(EventSource eventSource)
	{
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "OnEventSourceOpen");
	}

	private void OnEventSourceMessage(EventSource eventSource, Message message)
	{
		if (message.GetData().Equals("initialized"))
		{
			OnConnected();
			return;
		}
		IServerMessage serverMessage = TransportBase.Parse(GetConnection().GetJsonEncoder(), message.GetData());
		if (serverMessage != null)
		{
			GetConnection().OnMessage(serverMessage);
		}
	}

	private void OnEventSourceError(EventSource eventSource, string error)
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
				GetConnection().Error(error);
			}
		}
	}

	private void OnEventSourceClosed(EventSource eventSource)
	{
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "OnEventSourceClosed");
		OnEventSourceError(eventSource, "EventSource Closed!");
	}
}
