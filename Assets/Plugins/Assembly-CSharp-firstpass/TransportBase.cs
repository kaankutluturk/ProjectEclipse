using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

public abstract class TransportBase
{
	private const int MaxRetryCount = 5;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string name;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private IConnection connection;

	public TransportStates CurrentStateValue;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private OnTransportStateChangedDelegate OnStateChanged;

	public string TransportName
	{
		get
		{
			return get_Name();
		}
		protected set
		{
			set_Name(value);
		}
	}

	public abstract bool SupportsKeepAlive { get; }

	public IConnection PEBFDIFIMBO
	{
		get
		{
			return GetConnection();
		}
		protected set
		{
			GNLCPJFBAJE(value);
		}
	}

	public TransportStates CurrentState
	{
		get
		{
			return GetState();
		}
		protected set
		{
			set_State(value);
		}
	}

	public event OnTransportStateChangedDelegate StateChanged
	{
		add
		{
			AddStateChangedHandler(value);
		}
		remove
		{
			RemoveStateChangedHandler(value);
		}
	}

	public TransportBase(string name, Connection MDGFGCDPGFI)
	{
		set_Name(name);
		GNLCPJFBAJE(MDGFGCDPGFI);
		set_State(TransportStates.Initial);
	}

	public string get_Name()
	{
		return name;
	}

	protected void set_Name(string value)
	{
		name = value;
	}

	public abstract bool GetSupportsKeepAlive();

	public abstract TransportTypes get_Type();

	public IConnection GetConnection()
	{
		return connection;
	}

	protected void GNLCPJFBAJE(IConnection value)
	{
		connection = value;
	}

	public TransportStates GetState()
	{
		return CurrentStateValue;
	}

	protected void set_State(TransportStates value)
	{
		TransportStates mAFFNGPOMJD = CurrentStateValue;
		CurrentStateValue = value;
		if (OnStateChanged != null)
		{
			OnStateChanged(this, mAFFNGPOMJD, CurrentStateValue);
		}
	}

	public void AddStateChangedHandler(OnTransportStateChangedDelegate value)
	{
		OnTransportStateChangedDelegate lCGIFKDMOMP = OnStateChanged;
		OnTransportStateChangedDelegate lCGIFKDMOMP2;
		do
		{
			lCGIFKDMOMP2 = lCGIFKDMOMP;
			lCGIFKDMOMP = Interlocked.CompareExchange(ref OnStateChanged, (OnTransportStateChangedDelegate)Delegate.Combine(lCGIFKDMOMP2, value), lCGIFKDMOMP);
		}
		while ((object)lCGIFKDMOMP != lCGIFKDMOMP2);
	}

	public void RemoveStateChangedHandler(OnTransportStateChangedDelegate value)
	{
		OnTransportStateChangedDelegate lCGIFKDMOMP = OnStateChanged;
		OnTransportStateChangedDelegate lCGIFKDMOMP2;
		do
		{
			lCGIFKDMOMP2 = lCGIFKDMOMP;
			lCGIFKDMOMP = Interlocked.CompareExchange(ref OnStateChanged, (OnTransportStateChangedDelegate)Delegate.Remove(lCGIFKDMOMP2, value), lCGIFKDMOMP);
		}
		while ((object)lCGIFKDMOMP != lCGIFKDMOMP2);
	}

	public abstract void Connect();

	public abstract void Stop();

	protected abstract void SendImpl(string EMDHMHOKGFP);

	protected abstract void OnStarted();

	protected abstract void OnAborted();

	protected void OnConnected()
	{
		if (GetState() != TransportStates.Reconnecting)
		{
			Start();
			return;
		}
		GetConnection().TransportReconnected();
		OnStarted();
		set_State(TransportStates.Started);
	}

	protected void Start()
	{
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Sending Start Request");
		set_State(TransportStates.Starting);
		HTTPRequest iPLGNIDJDCF = new HTTPRequest(GetConnection().BuildUri(SignalRRequestType.Start, this), HTTPMethods.Get, true, true, OnStartRequestFinished);
		iPLGNIDJDCF.set_Tag(0);
		iPLGNIDJDCF.SetDisableRetry(true);
		iPLGNIDJDCF.SetTimeout(GetConnection().GetNegotiationResult().GetConnectionTimeout() + TimeSpan.FromSeconds(10.0));
		GetConnection().PrepareRequest(iPLGNIDJDCF, SignalRRequestType.Start);
		iPLGNIDJDCF.Send();
	}

	private void OnStartRequestFinished(HTTPRequest CGOIOKHEGOE, HTTPResponse BEIGFGCBICO)
	{
		HTTPRequestStates cFGBMHKCENK = CGOIOKHEGOE.GetState();
		if (cFGBMHKCENK == HTTPRequestStates.Finished)
		{
			if (BEIGFGCBICO.GetIsSuccess())
			{
				HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Start - Returned: " + BEIGFGCBICO.GetDataAsText());
				string text = GetConnection().ParseResponse(BEIGFGCBICO.GetDataAsText());
				if (text != "started")
				{
					GetConnection().Error(string.Format("Expected 'started' response, but '{0}' found!", text));
					return;
				}
				set_State(TransportStates.Started);
				OnStarted();
				GetConnection().TransportStarted();
				return;
			}
			HTTPManager.GetLogger().Warning("Transport - " + get_Name(), string.Format("Start - request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}", BEIGFGCBICO.GetStatusCode(), BEIGFGCBICO.GetMessage(), BEIGFGCBICO.GetDataAsText(), CGOIOKHEGOE.GetCurrentUri()));
		}
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Start request state: " + CGOIOKHEGOE.GetState());
		int num = (int)CGOIOKHEGOE.GetTag();
		if (num++ < 5)
		{
			CGOIOKHEGOE.set_Tag(num);
			CGOIOKHEGOE.Send();
		}
		else
		{
			GetConnection().Error("Failed to send Start request.");
		}
	}

	public virtual void Abort()
	{
		if (GetState() == TransportStates.Started)
		{
			set_State(TransportStates.Closing);
			HTTPRequest iPLGNIDJDCF = new HTTPRequest(GetConnection().BuildUri(SignalRRequestType.Abort, this), HTTPMethods.Get, true, true, OnAbortRequestFinished);
			iPLGNIDJDCF.set_Tag(0);
			iPLGNIDJDCF.SetDisableRetry(true);
			GetConnection().PrepareRequest(iPLGNIDJDCF, SignalRRequestType.Abort);
			iPLGNIDJDCF.Send();
		}
	}

	protected void AbortFinished()
	{
		set_State(TransportStates.Closed);
		GetConnection().TransportAborted();
		OnAborted();
	}

	private void OnAbortRequestFinished(HTTPRequest CGOIOKHEGOE, HTTPResponse BEIGFGCBICO)
	{
		HTTPRequestStates cFGBMHKCENK = CGOIOKHEGOE.GetState();
		if (cFGBMHKCENK == HTTPRequestStates.Finished)
		{
			if (BEIGFGCBICO.GetIsSuccess())
			{
				HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Abort - Returned: " + BEIGFGCBICO.GetDataAsText());
				if (GetState() == TransportStates.Closing)
				{
					AbortFinished();
				}
				return;
			}
			HTTPManager.GetLogger().Warning("Transport - " + get_Name(), string.Format("Abort - Handshake request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}", BEIGFGCBICO.GetStatusCode(), BEIGFGCBICO.GetMessage(), BEIGFGCBICO.GetDataAsText(), CGOIOKHEGOE.GetCurrentUri()));
		}
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Abort request state: " + CGOIOKHEGOE.GetState());
		int num = (int)CGOIOKHEGOE.GetTag();
		if (num++ < 5)
		{
			CGOIOKHEGOE.set_Tag(num);
			CGOIOKHEGOE.Send();
		}
		else
		{
			GetConnection().Error("Failed to send Abort request!");
		}
	}

	public void Send(string DGNLDMDLKDA)
	{
		try
		{
			HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Sending: " + DGNLDMDLKDA);
			SendImpl(DGNLDMDLKDA);
		}
		catch (Exception mPFFFAOGBJE)
		{
			HTTPManager.GetLogger().Exception("Transport - " + get_Name(), "Send", mPFFFAOGBJE);
		}
	}

	public void Reconnect()
	{
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Reconnecting");
		Stop();
		set_State(TransportStates.Reconnecting);
		Connect();
	}

	public static IServerMessage Parse(IJsonEncoder GLOJHMAIFOK, string EMDHMHOKGFP)
	{
		if (string.IsNullOrEmpty(EMDHMHOKGFP))
		{
			HTTPManager.GetLogger().Error("MessageFactory", "Parse - called with empty or null string!");
			return null;
		}
		if (EMDHMHOKGFP.Length == 2 && EMDHMHOKGFP == "{}")
		{
			return new KeepAliveMessage();
		}
		IDictionary<string, object> dictionary = null;
		try
		{
			dictionary = GLOJHMAIFOK.DecodeMessage(EMDHMHOKGFP);
		}
		catch (Exception mPFFFAOGBJE)
		{
			HTTPManager.GetLogger().Exception("MessageFactory", "Parse - encoder.DecodeMessage", mPFFFAOGBJE);
			return null;
		}
		if (dictionary == null)
		{
			HTTPManager.GetLogger().Error("MessageFactory", "Parse - Json Decode failed for json string: \"" + EMDHMHOKGFP + "\"");
			return null;
		}
		IServerMessage bNGPAAAKBOP = null;
		bNGPAAAKBOP = (dictionary.ContainsKey("C") ? new MultiMessage() : (dictionary.ContainsKey("E") ? ((IServerMessage)new FailureMessage()) : ((IServerMessage)new ResultMessage())));
		bNGPAAAKBOP.Parse(dictionary);
		return bNGPAAAKBOP;
	}
}
