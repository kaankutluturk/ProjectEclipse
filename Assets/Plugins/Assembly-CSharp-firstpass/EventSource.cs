using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

public class EventSource : IHeartbeat
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Uri uri;

	private EventSourceState state;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan reconnectionTime;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string lastEventId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HTTPRequest internalRequest;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private OnGeneralEventDelegate OnOpen;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private OnEventSourceMessageDelegate onMessageField;

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private OnErrorDelegate onErrorField;

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private OnRetryDelegate OnRetry;

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private OnGeneralEventDelegate onClosedField;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private OnStateChangedDelegate OnStateChanged;

	private Dictionary<string, OnEventDelegate> eventTable;

	private byte RetryCount;

	private DateTime RetryCalled;

	public Uri EventSourceUri
	{
		get
		{
			return GetUri();
		}
		private set
		{
			set_Uri(value);
		}
	}

	public EventSourceState ReadyState
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

	public TimeSpan ReconnectDelay
	{
		get
		{
			return GetReconnectionTime();
		}
		set
		{
			set_ReconnectionTime(value);
		}
	}

	public string LastReceivedEventId
	{
		get
		{
			return GetLastEventId();
		}
		private set
		{
			set_LastEventId(value);
		}
	}

	public HTTPRequest InternalRequest
	{
		get
		{
			return GetInternalRequest();
		}
		private set
		{
			SetInternalRequest(value);
		}
	}

	public event OnGeneralEventDelegate Opened
	{
		add
		{
			AddOnOpen(value);
		}
		remove
		{
			RemoveOnOpen(value);
		}
	}

	public event OnEventSourceMessageDelegate OnMessage
	{
		add
		{
			AddOnMessage(value);
		}
		remove
		{
			RemoveOnMessage(value);
		}
	}

	public event OnErrorDelegate OnError
	{
		add
		{
			AddOnError(value);
		}
		remove
		{
			RemoveOnError(value);
		}
	}

	public event OnRetryDelegate RetryRequested
	{
		add
		{
			AddOnRetry(value);
		}
		remove
		{
			RemoveOnRetry(value);
		}
	}

	public event OnGeneralEventDelegate OnClosed
	{
		add
		{
			AddOnClosed(value);
		}
		remove
		{
			RemoveOnClosed(value);
		}
	}

	public event OnStateChangedDelegate StateChanged
	{
		add
		{
			AddOnStateChanged(value);
		}
		remove
		{
			RemoveOnStateChanged(value);
		}
	}

	public EventSource(Uri uri)
	{
		set_Uri(uri);
		set_ReconnectionTime(TimeSpan.FromMilliseconds(2000.0));
		SetInternalRequest(new HTTPRequest(GetUri(), HTTPMethods.Get, false, true, OnRequestFinished));
		GetInternalRequest().SetHeader("Accept", "text/event-stream");
		GetInternalRequest().SetHeader("Cache-Control", "no-cache");
		GetInternalRequest().SetHeader("Accept-Encoding", "identity");
		GetInternalRequest().SetProtocolHandler(SupportedProtocols.ServerSentEvents);
		GetInternalRequest().OnUpgraded = OnUpgraded;
		GetInternalRequest().SetDisableRetry(true);
	}

	public Uri GetUri()
	{
		return uri;
	}

	private void set_Uri(Uri value)
	{
		uri = value;
	}

	public EventSourceState GetState()
	{
		return state;
	}

	private void set_State(EventSourceState value)
	{
		EventSourceState oldState = state;
		state = value;
		if (OnStateChanged != null)
		{
			try
			{
				OnStateChanged(this, oldState, state);
			}
			catch (Exception ex)
			{
				HTTPManager.GetLogger().Exception("EventSource", "OnStateChanged", ex);
			}
		}
	}

	public TimeSpan GetReconnectionTime()
	{
		return reconnectionTime;
	}

	public void set_ReconnectionTime(TimeSpan value)
	{
		reconnectionTime = value;
	}

	public string GetLastEventId()
	{
		return lastEventId;
	}

	private void set_LastEventId(string value)
	{
		lastEventId = value;
	}

	public HTTPRequest GetInternalRequest()
	{
		return internalRequest;
	}

	private void SetInternalRequest(HTTPRequest value)
	{
		internalRequest = value;
	}

	public void AddOnOpen(OnGeneralEventDelegate value)
	{
		OnGeneralEventDelegate current = OnOpen;
		OnGeneralEventDelegate bHJHIPILHJB2;
		do
		{
			bHJHIPILHJB2 = current;
			current = Interlocked.CompareExchange(ref OnOpen, (OnGeneralEventDelegate)Delegate.Combine(bHJHIPILHJB2, value), current);
		}
		while ((object)current != bHJHIPILHJB2);
	}

	public void RemoveOnOpen(OnGeneralEventDelegate value)
	{
		OnGeneralEventDelegate current = OnOpen;
		OnGeneralEventDelegate bHJHIPILHJB2;
		do
		{
			bHJHIPILHJB2 = current;
			current = Interlocked.CompareExchange(ref OnOpen, (OnGeneralEventDelegate)Delegate.Remove(bHJHIPILHJB2, value), current);
		}
		while ((object)current != bHJHIPILHJB2);
	}

	public void AddOnMessage(OnEventSourceMessageDelegate value)
	{
		OnEventSourceMessageDelegate current = onMessageField;
		OnEventSourceMessageDelegate iPIGAJKKJLN2;
		do
		{
			iPIGAJKKJLN2 = current;
			current = Interlocked.CompareExchange(ref onMessageField, (OnEventSourceMessageDelegate)Delegate.Combine(iPIGAJKKJLN2, value), current);
		}
		while ((object)current != iPIGAJKKJLN2);
	}

	public void RemoveOnMessage(OnEventSourceMessageDelegate value)
	{
		OnEventSourceMessageDelegate current = onMessageField;
		OnEventSourceMessageDelegate iPIGAJKKJLN2;
		do
		{
			iPIGAJKKJLN2 = current;
			current = Interlocked.CompareExchange(ref onMessageField, (OnEventSourceMessageDelegate)Delegate.Remove(iPIGAJKKJLN2, value), current);
		}
		while ((object)current != iPIGAJKKJLN2);
	}

	public void AddOnError(OnErrorDelegate value)
	{
		OnErrorDelegate current = onErrorField;
		OnErrorDelegate eGECAPOLBHF2;
		do
		{
			eGECAPOLBHF2 = current;
			current = Interlocked.CompareExchange(ref onErrorField, (OnErrorDelegate)Delegate.Combine(eGECAPOLBHF2, value), current);
		}
		while ((object)current != eGECAPOLBHF2);
	}

	public void RemoveOnError(OnErrorDelegate value)
	{
		OnErrorDelegate current = onErrorField;
		OnErrorDelegate eGECAPOLBHF2;
		do
		{
			eGECAPOLBHF2 = current;
			current = Interlocked.CompareExchange(ref onErrorField, (OnErrorDelegate)Delegate.Remove(eGECAPOLBHF2, value), current);
		}
		while ((object)current != eGECAPOLBHF2);
	}

	public void AddOnRetry(OnRetryDelegate value)
	{
		OnRetryDelegate current = OnRetry;
		OnRetryDelegate cPMLAEEAKNP2;
		do
		{
			cPMLAEEAKNP2 = current;
			current = Interlocked.CompareExchange(ref OnRetry, (OnRetryDelegate)Delegate.Combine(cPMLAEEAKNP2, value), current);
		}
		while ((object)current != cPMLAEEAKNP2);
	}

	public void RemoveOnRetry(OnRetryDelegate value)
	{
		OnRetryDelegate current = OnRetry;
		OnRetryDelegate cPMLAEEAKNP2;
		do
		{
			cPMLAEEAKNP2 = current;
			current = Interlocked.CompareExchange(ref OnRetry, (OnRetryDelegate)Delegate.Remove(cPMLAEEAKNP2, value), current);
		}
		while ((object)current != cPMLAEEAKNP2);
	}

	public void AddOnClosed(OnGeneralEventDelegate value)
	{
		OnGeneralEventDelegate current = onClosedField;
		OnGeneralEventDelegate bHJHIPILHJB2;
		do
		{
			bHJHIPILHJB2 = current;
			current = Interlocked.CompareExchange(ref onClosedField, (OnGeneralEventDelegate)Delegate.Combine(bHJHIPILHJB2, value), current);
		}
		while ((object)current != bHJHIPILHJB2);
	}

	public void RemoveOnClosed(OnGeneralEventDelegate value)
	{
		OnGeneralEventDelegate current = onClosedField;
		OnGeneralEventDelegate bHJHIPILHJB2;
		do
		{
			bHJHIPILHJB2 = current;
			current = Interlocked.CompareExchange(ref onClosedField, (OnGeneralEventDelegate)Delegate.Remove(bHJHIPILHJB2, value), current);
		}
		while ((object)current != bHJHIPILHJB2);
	}

	public void AddOnStateChanged(OnStateChangedDelegate value)
	{
		OnStateChangedDelegate current = OnStateChanged;
		OnStateChangedDelegate gAHJEMHNLNB2;
		do
		{
			gAHJEMHNLNB2 = current;
			current = Interlocked.CompareExchange(ref OnStateChanged, (OnStateChangedDelegate)Delegate.Combine(gAHJEMHNLNB2, value), current);
		}
		while ((object)current != gAHJEMHNLNB2);
	}

	public void RemoveOnStateChanged(OnStateChangedDelegate value)
	{
		OnStateChangedDelegate current = OnStateChanged;
		OnStateChangedDelegate gAHJEMHNLNB2;
		do
		{
			gAHJEMHNLNB2 = current;
			current = Interlocked.CompareExchange(ref OnStateChanged, (OnStateChangedDelegate)Delegate.Remove(gAHJEMHNLNB2, value), current);
		}
		while ((object)current != gAHJEMHNLNB2);
	}

	public void OpenEventSource()
	{
		if (GetState() == EventSourceState.Initial || GetState() == EventSourceState.Retrying || GetState() == EventSourceState.Closed)
		{
			set_State(EventSourceState.Connecting);
			if (!string.IsNullOrEmpty(GetLastEventId()))
			{
				GetInternalRequest().SetHeader("Last-Event-ID", GetLastEventId());
			}
			GetInternalRequest().Send();
		}
	}

	public void Close()
	{
		if (GetState() != EventSourceState.Closing && GetState() != EventSourceState.Closed)
		{
			set_State(EventSourceState.Closing);
			if (GetInternalRequest() != null)
			{
				GetInternalRequest().Abort();
			}
			else
			{
				set_State(EventSourceState.Closed);
			}
		}
	}

	public void On(string eventName, OnEventDelegate handler)
	{
		if (eventTable == null)
		{
			eventTable = new Dictionary<string, OnEventDelegate>();
		}
		eventTable[eventName] = handler;
	}

	public void Off(string eventName)
	{
		if (eventName != null)
		{
			eventTable.Remove(eventName);
		}
	}

	private void CallOnError(string error, string context)
	{
		if (onErrorField != null)
		{
			try
			{
				onErrorField(this, error);
			}
			catch (Exception ex)
			{
				HTTPManager.GetLogger().Exception("EventSource", context + " - OnError", ex);
			}
		}
	}

	private bool CallOnRetry()
	{
		if (OnRetry != null)
		{
			try
			{
				return OnRetry(this);
			}
			catch (Exception ex)
			{
				HTTPManager.GetLogger().Exception("EventSource", "CallOnRetry", ex);
			}
		}
		return true;
	}

	private void SetClosed(string context)
	{
		set_State(EventSourceState.Closed);
		if (onClosedField != null)
		{
			try
			{
				onClosedField(this);
			}
			catch (Exception ex)
			{
				HTTPManager.GetLogger().Exception("EventSource", context + " - OnClosed", ex);
			}
		}
	}

	private void Retry()
	{
		if (RetryCount > 0 || !CallOnRetry())
		{
			SetClosed("Retry");
			return;
		}
		RetryCount++;
		RetryCalled = DateTime.UtcNow;
		HTTPManager.GetHeartbeats().Subscribe(this);
		set_State(EventSourceState.Retrying);
	}

	private void OnUpgraded(HTTPRequest request, HTTPResponse response)
	{
		EventSourceResponse eventSourceResponse = response as EventSourceResponse;
		if (eventSourceResponse == null)
		{
			CallOnError("Not an EventSourceResponse!", "OnUpgraded");
			return;
		}
		if (OnOpen != null)
		{
			try
			{
				OnOpen(this);
			}
			catch (Exception ex)
			{
				HTTPManager.GetLogger().Exception("EventSource", "OnOpen", ex);
			}
		}
		eventSourceResponse.OnMessage = (Action<EventSourceResponse, Message>)Delegate.Combine(eventSourceResponse.OnMessage, new Action<EventSourceResponse, Message>(OnMessageReceived));
		eventSourceResponse.StartReceive();
		RetryCount = 0;
		set_State(EventSourceState.Open);
	}

	private void OnRequestFinished(HTTPRequest request, HTTPResponse response)
	{
		if (GetState() == EventSourceState.Closed)
		{
			return;
		}
		if (GetState() == EventSourceState.Closing)
		{
			SetClosed("OnRequestFinished");
			return;
		}
		string text = string.Empty;
		bool flag = true;
		switch (request.GetState())
		{
		case HTTPRequestStates.Processing:
			flag = !response.HasHeader("content-length");
			break;
		case HTTPRequestStates.Finished:
			if (response.GetStatusCode() == 200 && !response.HasHeaderWithValue("content-type", "text/event-stream"))
			{
				text = "No Content-Type header with value 'text/event-stream' present.";
				flag = false;
			}
			if (flag && response.GetStatusCode() != 500 && response.GetStatusCode() != 502 && response.GetStatusCode() != 503 && response.GetStatusCode() != 504)
			{
				flag = false;
				text = string.Format("Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText());
			}
			break;
		case HTTPRequestStates.Error:
			text = "Request Finished with Error! " + ((request.GetException() == null) ? "No Exception" : (request.GetException().Message + "\n" + request.GetException().StackTrace));
			break;
		case HTTPRequestStates.Aborted:
			text = "OnRequestFinished - Aborted without request. EventSource's State: " + GetState();
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			text = "Connection Timed Out!";
			break;
		case HTTPRequestStates.TimedOut:
			text = "Processing the request Timed Out!";
			break;
		}
		if (GetState() < EventSourceState.Closing)
		{
			if (!string.IsNullOrEmpty(text))
			{
				CallOnError(text, "OnRequestFinished");
			}
			if (flag)
			{
				Retry();
			}
			else
			{
				SetClosed("OnRequestFinished");
			}
		}
		else
		{
			SetClosed("OnRequestFinished");
		}
	}

	private void OnMessageReceived(EventSourceResponse response, Message message)
	{
		if (GetState() >= EventSourceState.Closing)
		{
			return;
		}
		if (message.GetMessageId() != null)
		{
			set_LastEventId(message.GetMessageId());
		}
		if (message.GetRetry().TotalMilliseconds > 0.0)
		{
			set_ReconnectionTime(message.GetRetry());
		}
		if (string.IsNullOrEmpty(message.GetData()))
		{
			return;
		}
		if (onMessageField != null)
		{
			try
			{
				onMessageField(this, message);
			}
			catch (Exception ex)
			{
				HTTPManager.GetLogger().Exception("EventSource", "OnMessageReceived - OnMessage", ex);
			}
		}
		OnEventDelegate value;
		if (string.IsNullOrEmpty(message.GetEvent()) || !eventTable.TryGetValue(message.GetEvent(), out value) || value == null)
		{
			return;
		}
		try
		{
			value(this, message);
		}
		catch (Exception mPFFFAOGBJE2)
		{
			HTTPManager.GetLogger().Exception("EventSource", "OnMessageReceived - action", mPFFFAOGBJE2);
		}
	}

	void IHeartbeat.OnHeartbeatUpdate(TimeSpan elapsed)
	{
		if (GetState() != EventSourceState.Retrying)
		{
			HTTPManager.GetHeartbeats().Unsubscribe(this);
		}
		else if (DateTime.UtcNow - RetryCalled >= GetReconnectionTime())
		{
			OpenEventSource();
			if (GetState() != EventSourceState.Connecting)
			{
				SetClosed("OnHeartbeatUpdate");
			}
			HTTPManager.GetHeartbeats().Unsubscribe(this);
		}
	}
}
