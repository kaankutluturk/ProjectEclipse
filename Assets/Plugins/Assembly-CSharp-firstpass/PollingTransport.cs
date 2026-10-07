using System;

public sealed class PollingTransport : PostSendTransportBase, IHeartbeat
{
	private DateTime LastPoll;

	private TimeSpan PollDelay;

	private TimeSpan PollTimeout;

	private HTTPRequest pollRequest;

	public override bool SupportsKeepAlive
	{
		get
		{
			return GetSupportsKeepAlive();
		}
	}

	public PollingTransport(Connection connection)
		: base("longPolling", connection)
	{
		LastPoll = DateTime.MinValue;
		PollTimeout = connection.GetNegotiationResult().GetConnectionTimeout() + TimeSpan.FromSeconds(10.0);
	}

	public override bool GetSupportsKeepAlive()
	{
		return false;
	}

	public override TransportTypes get_Type()
	{
		return TransportTypes.LongPoll;
	}

	public override void Connect()
	{
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Sending Open Request");
		if (GetState() != TransportStates.Reconnecting)
		{
			set_State(TransportStates.Connecting);
		}
		SignalRRequestType requestType = ((GetState() != TransportStates.Reconnecting) ? SignalRRequestType.Connect : SignalRRequestType.Reconnect);
		HTTPRequest request = new HTTPRequest(GetConnection().BuildUri(requestType, this), HTTPMethods.Get, true, true, OnConnectRequestFinished);
		GetConnection().PrepareRequest(request, requestType);
		request.Send();
	}

	public override void Stop()
	{
		HTTPManager.GetHeartbeats().Unsubscribe(this);
		if (pollRequest != null)
		{
			pollRequest.Abort();
			pollRequest = null;
		}
	}

	protected override void OnStarted()
	{
		LastPoll = DateTime.UtcNow;
		HTTPManager.GetHeartbeats().Subscribe(this);
	}

	protected override void OnAborted()
	{
		HTTPManager.GetHeartbeats().Unsubscribe(this);
	}

	private void OnConnectRequestFinished(HTTPRequest request, HTTPResponse response)
	{
		string text = string.Empty;
		switch (request.GetState())
		{
		case HTTPRequestStates.Finished:
			if (response.GetIsSuccess())
			{
				HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Connect - Request Finished Successfully! " + response.GetDataAsText());
				OnConnected();
				IServerMessage serverMessage = TransportBase.Parse(GetConnection().GetJsonEncoder(), response.GetDataAsText());
				if (serverMessage != null)
				{
					GetConnection().OnMessage(serverMessage);
					MultiMessage multiMessage = serverMessage as MultiMessage;
					if (multiMessage != null && multiMessage.GetPollDelay().HasValue)
					{
						PollDelay = multiMessage.GetPollDelay().Value;
					}
				}
			}
			else
			{
				text = string.Format("Connect - Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText());
			}
			break;
		case HTTPRequestStates.Error:
			text = "Connect - Request Finished with Error! " + ((request.GetException() == null) ? "No Exception" : (request.GetException().Message + "\n" + request.GetException().StackTrace));
			break;
		case HTTPRequestStates.Aborted:
			text = "Connect - Request Aborted!";
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			text = "Connect - Connection Timed Out!";
			break;
		case HTTPRequestStates.TimedOut:
			text = "Connect - Processing the request Timed Out!";
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			GetConnection().Error(text);
		}
	}

	private void OnPollRequestFinished(HTTPRequest request, HTTPResponse response)
	{
		if (request.GetState() == HTTPRequestStates.Aborted)
		{
			HTTPManager.GetLogger().Warning("Transport - " + get_Name(), "Poll - Request Aborted!");
			return;
		}
		pollRequest = null;
		string text = string.Empty;
		switch (request.GetState())
		{
		case HTTPRequestStates.Finished:
			if (response.GetIsSuccess())
			{
				HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Poll - Request Finished Successfully! " + response.GetDataAsText());
				IServerMessage serverMessage = TransportBase.Parse(GetConnection().GetJsonEncoder(), response.GetDataAsText());
				if (serverMessage != null)
				{
					GetConnection().OnMessage(serverMessage);
					MultiMessage multiMessage = serverMessage as MultiMessage;
					if (multiMessage != null && multiMessage.GetPollDelay().HasValue)
					{
						PollDelay = multiMessage.GetPollDelay().Value;
					}
					LastPoll = DateTime.UtcNow;
				}
			}
			else
			{
				text = string.Format("Poll - Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText());
			}
			break;
		case HTTPRequestStates.Error:
			text = "Poll - Request Finished with Error! " + ((request.GetException() == null) ? "No Exception" : (request.GetException().Message + "\n" + request.GetException().StackTrace));
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			text = "Poll - Connection Timed Out!";
			break;
		case HTTPRequestStates.TimedOut:
			text = "Poll - Processing the request Timed Out!";
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			GetConnection().Error(text);
		}
	}

	private void Poll()
	{
		pollRequest = new HTTPRequest(GetConnection().BuildUri(SignalRRequestType.Poll, this), HTTPMethods.Get, true, true, OnPollRequestFinished);
		GetConnection().PrepareRequest(pollRequest, SignalRRequestType.Poll);
		pollRequest.SetTimeout(PollTimeout);
		pollRequest.Send();
	}

	void IHeartbeat.OnHeartbeatUpdate(TimeSpan delta)
	{
		TransportStates transportState = GetState();
		if (transportState == TransportStates.Started && pollRequest == null && DateTime.UtcNow >= LastPoll + PollDelay + GetConnection().GetNegotiationResult().GetLongPollDelay())
		{
			Poll();
		}
	}
}
