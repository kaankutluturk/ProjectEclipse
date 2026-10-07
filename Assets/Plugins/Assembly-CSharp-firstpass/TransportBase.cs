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

	public IConnection TransportConnection
	{
		get
		{
			return GetConnection();
		}
		protected set
		{
			SetConnection(value);
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

	public TransportBase(string name, Connection connection)
	{
		set_Name(name);
		SetConnection(connection);
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

	protected void SetConnection(IConnection value)
	{
		connection = value;
	}

	public TransportStates GetState()
	{
		return CurrentStateValue;
	}

	protected void set_State(TransportStates value)
	{
		TransportStates previousState = CurrentStateValue;
		CurrentStateValue = value;
		if (OnStateChanged != null)
		{
			OnStateChanged(this, previousState, CurrentStateValue);
		}
	}

	public void AddStateChangedHandler(OnTransportStateChangedDelegate value)
	{
		OnTransportStateChangedDelegate currentHandler = OnStateChanged;
		OnTransportStateChangedDelegate previousHandler;
		do
		{
			previousHandler = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnStateChanged, (OnTransportStateChangedDelegate)Delegate.Combine(previousHandler, value), currentHandler);
		}
		while ((object)currentHandler != previousHandler);
	}

	public void RemoveStateChangedHandler(OnTransportStateChangedDelegate value)
	{
		OnTransportStateChangedDelegate currentHandler = OnStateChanged;
		OnTransportStateChangedDelegate previousHandler;
		do
		{
			previousHandler = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnStateChanged, (OnTransportStateChangedDelegate)Delegate.Remove(previousHandler, value), currentHandler);
		}
		while ((object)currentHandler != previousHandler);
	}

	public abstract void Connect();

	public abstract void Stop();

	protected abstract void SendImpl(string message);

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
		HTTPRequest startRequest = new HTTPRequest(GetConnection().BuildUri(SignalRRequestType.Start, this), HTTPMethods.Get, true, true, OnStartRequestFinished);
		startRequest.set_Tag(0);
		startRequest.SetDisableRetry(true);
		startRequest.SetTimeout(GetConnection().GetNegotiationResult().GetConnectionTimeout() + TimeSpan.FromSeconds(10.0));
		GetConnection().PrepareRequest(startRequest, SignalRRequestType.Start);
		startRequest.Send();
	}

	private void OnStartRequestFinished(HTTPRequest request, HTTPResponse response)
	{
		HTTPRequestStates requestState = request.GetState();
		if (requestState == HTTPRequestStates.Finished)
		{
			if (response.GetIsSuccess())
			{
				HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Start - Returned: " + response.GetDataAsText());
				string text = GetConnection().ParseResponse(response.GetDataAsText());
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
			HTTPManager.GetLogger().Warning("Transport - " + get_Name(), string.Format("Start - request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText(), request.GetCurrentUri()));
		}
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Start request state: " + request.GetState());
		int num = (int)request.GetTag();
		if (num++ < 5)
		{
			request.set_Tag(num);
			request.Send();
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
			HTTPRequest abortRequest = new HTTPRequest(GetConnection().BuildUri(SignalRRequestType.Abort, this), HTTPMethods.Get, true, true, OnAbortRequestFinished);
			abortRequest.set_Tag(0);
			abortRequest.SetDisableRetry(true);
			GetConnection().PrepareRequest(abortRequest, SignalRRequestType.Abort);
			abortRequest.Send();
		}
	}

	protected void AbortFinished()
	{
		set_State(TransportStates.Closed);
		GetConnection().TransportAborted();
		OnAborted();
	}

	private void OnAbortRequestFinished(HTTPRequest request, HTTPResponse response)
	{
		HTTPRequestStates requestState = request.GetState();
		if (requestState == HTTPRequestStates.Finished)
		{
			if (response.GetIsSuccess())
			{
				HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Abort - Returned: " + response.GetDataAsText());
				if (GetState() == TransportStates.Closing)
				{
					AbortFinished();
				}
				return;
			}
			HTTPManager.GetLogger().Warning("Transport - " + get_Name(), string.Format("Abort - Handshake request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText(), request.GetCurrentUri()));
		}
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Abort request state: " + request.GetState());
		int num = (int)request.GetTag();
		if (num++ < 5)
		{
			request.set_Tag(num);
			request.Send();
		}
		else
		{
			GetConnection().Error("Failed to send Abort request!");
		}
	}

	public void Send(string message)
	{
		try
		{
			HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Sending: " + message);
			SendImpl(message);
		}
		catch (Exception exception)
		{
			HTTPManager.GetLogger().Exception("Transport - " + get_Name(), "Send", exception);
		}
	}

	public void Reconnect()
	{
		HTTPManager.GetLogger().Information("Transport - " + get_Name(), "Reconnecting");
		Stop();
		set_State(TransportStates.Reconnecting);
		Connect();
	}

	public static IServerMessage Parse(IJsonEncoder encoder, string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			HTTPManager.GetLogger().Error("MessageFactory", "Parse - called with empty or null string!");
			return null;
		}
		if (json.Length == 2 && json == "{}")
		{
			return new KeepAliveMessage();
		}
		IDictionary<string, object> dictionary = null;
		try
		{
			dictionary = encoder.DecodeMessage(json);
		}
		catch (Exception exception)
		{
			HTTPManager.GetLogger().Exception("MessageFactory", "Parse - encoder.DecodeMessage", exception);
			return null;
		}
		if (dictionary == null)
		{
			HTTPManager.GetLogger().Error("MessageFactory", "Parse - Json Decode failed for json string: \"" + json + "\"");
			return null;
		}
		IServerMessage message = null;
		message = (dictionary.ContainsKey("C") ? new MultiMessage() : (dictionary.ContainsKey("E") ? ((IServerMessage)new FailureMessage()) : ((IServerMessage)new ResultMessage())));
		message.Parse(dictionary);
		return message;
	}
}
