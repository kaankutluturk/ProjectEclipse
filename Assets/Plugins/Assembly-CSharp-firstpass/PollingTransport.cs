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

	public PollingTransport(Connection MDGFGCDPGFI)
		: base("longPolling", MDGFGCDPGFI)
	{
		LastPoll = DateTime.MinValue;
		PollTimeout = MDGFGCDPGFI.GetNegotiationResult().GetConnectionTimeout() + TimeSpan.FromSeconds(10.0);
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
		SignalRRequestType lFLGCDNKNJI = ((GetState() != TransportStates.Reconnecting) ? SignalRRequestType.Connect : SignalRRequestType.Reconnect);
		HTTPRequest iPLGNIDJDCF = new HTTPRequest(GetConnection().BuildUri(lFLGCDNKNJI, this), HTTPMethods.Get, true, true, OnConnectRequestFinished);
		GetConnection().PrepareRequest(iPLGNIDJDCF, lFLGCDNKNJI);
		iPLGNIDJDCF.Send();
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

	private void OnConnectRequestFinished(HTTPRequest CGOIOKHEGOE, HTTPResponse BEIGFGCBICO)
	{
		string text = string.Empty;
		switch (CGOIOKHEGOE.GetState())
		{
		case HTTPRequestStates.Finished:
			if (BEIGFGCBICO.GetIsSuccess())
			{
				HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Connect - Request Finished Successfully! " + BEIGFGCBICO.GetDataAsText());
				OnConnected();
				IServerMessage bNGPAAAKBOP = TransportBase.Parse(GetConnection().GetJsonEncoder(), BEIGFGCBICO.GetDataAsText());
				if (bNGPAAAKBOP != null)
				{
					GetConnection().OnMessage(bNGPAAAKBOP);
					MultiMessage eIKBBLMECNO = bNGPAAAKBOP as MultiMessage;
					if (eIKBBLMECNO != null && eIKBBLMECNO.GetPollDelay().HasValue)
					{
						PollDelay = eIKBBLMECNO.GetPollDelay().Value;
					}
				}
			}
			else
			{
				text = string.Format("Connect - Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", BEIGFGCBICO.GetStatusCode(), BEIGFGCBICO.GetMessage(), BEIGFGCBICO.GetDataAsText());
			}
			break;
		case HTTPRequestStates.Error:
			text = "Connect - Request Finished with Error! " + ((CGOIOKHEGOE.GetException() == null) ? "No Exception" : (CGOIOKHEGOE.GetException().Message + "\n" + CGOIOKHEGOE.GetException().StackTrace));
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

	private void OnPollRequestFinished(HTTPRequest CGOIOKHEGOE, HTTPResponse BEIGFGCBICO)
	{
		if (CGOIOKHEGOE.GetState() == HTTPRequestStates.Aborted)
		{
			HTTPManager.GetLogger().Warning("Transport - " + get_Name(), "Poll - Request Aborted!");
			return;
		}
		pollRequest = null;
		string text = string.Empty;
		switch (CGOIOKHEGOE.GetState())
		{
		case HTTPRequestStates.Finished:
			if (BEIGFGCBICO.GetIsSuccess())
			{
				HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Poll - Request Finished Successfully! " + BEIGFGCBICO.GetDataAsText());
				IServerMessage bNGPAAAKBOP = TransportBase.Parse(GetConnection().GetJsonEncoder(), BEIGFGCBICO.GetDataAsText());
				if (bNGPAAAKBOP != null)
				{
					GetConnection().OnMessage(bNGPAAAKBOP);
					MultiMessage eIKBBLMECNO = bNGPAAAKBOP as MultiMessage;
					if (eIKBBLMECNO != null && eIKBBLMECNO.GetPollDelay().HasValue)
					{
						PollDelay = eIKBBLMECNO.GetPollDelay().Value;
					}
					LastPoll = DateTime.UtcNow;
				}
			}
			else
			{
				text = string.Format("Poll - Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", BEIGFGCBICO.GetStatusCode(), BEIGFGCBICO.GetMessage(), BEIGFGCBICO.GetDataAsText());
			}
			break;
		case HTTPRequestStates.Error:
			text = "Poll - Request Finished with Error! " + ((CGOIOKHEGOE.GetException() == null) ? "No Exception" : (CGOIOKHEGOE.GetException().Message + "\n" + CGOIOKHEGOE.GetException().StackTrace));
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

	void IHeartbeat.OnHeartbeatUpdate(TimeSpan OJOKANCMPLG)
	{
		TransportStates lJLKMCGDKJK = GetState();
		if (lJLKMCGDKJK == TransportStates.Started && pollRequest == null && DateTime.UtcNow >= LastPoll + PollDelay + GetConnection().GetNegotiationResult().GetLongPollDelay())
		{
			Poll();
		}
	}
}
