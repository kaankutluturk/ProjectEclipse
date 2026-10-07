using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

public class Hub : IHub
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Connection connection;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string name;

	private Dictionary<string, object> state;

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private OnMethodCallDelegate OnMethodCall;

	private Dictionary<ulong, ClientMessage> sentMessages = new Dictionary<ulong, ClientMessage>();

	private Dictionary<string, OnMethodCallCallbackDelegate> methodTable = new Dictionary<string, OnMethodCallCallbackDelegate>();

	private StringBuilder builder = new StringBuilder();

	public Connection HubConnection
	{
		get
		{
			return connection;
		}
		set
		{
			connection = value;
		}
	}

	public string HubName
	{
		get
		{
			return get_Name();
		}
		private set
		{
			set_Name(value);
		}
	}

	public Dictionary<string, object> HubState
	{
		get
		{
			return GetState();
		}
	}

	public event OnMethodCallDelegate MethodCalled
	{
		add
		{
			AddOnMethodCall(value);
		}
		remove
		{
			RemoveOnMethodCall(value);
		}
	}

	public Hub(string name)
		: this(name, null)
	{
	}

	public Hub(string name, Connection hubConnection)
	{
		set_Name(name);
		((IHub)this).AttachConnection(hubConnection);
	}

	public string get_Name()
	{
		return name;
	}

	private void set_Name(string value)
	{
		name = value;
	}

	public Dictionary<string, object> GetState()
	{
		if (state == null)
		{
			state = new Dictionary<string, object>();
		}
		return state;
	}

	public void AddOnMethodCall(OnMethodCallDelegate value)
	{
		OnMethodCallDelegate current = OnMethodCall;
		OnMethodCallDelegate previousHandler;
		do
		{
			previousHandler = current;
			current = Interlocked.CompareExchange(ref OnMethodCall, (OnMethodCallDelegate)Delegate.Combine(previousHandler, value), current);
		}
		while ((object)current != previousHandler);
	}

	public void RemoveOnMethodCall(OnMethodCallDelegate value)
	{
		OnMethodCallDelegate current = OnMethodCall;
		OnMethodCallDelegate previousHandler;
		do
		{
			previousHandler = current;
			current = Interlocked.CompareExchange(ref OnMethodCall, (OnMethodCallDelegate)Delegate.Remove(previousHandler, value), current);
		}
		while ((object)current != previousHandler);
	}

	Connection IHub.AttachedConnection
	{
		get
		{
			return connection;
		}
		set
		{
			connection = value;
		}
	}

	void IHub.AttachConnection(Connection value)
	{
		connection = value;
	}

	public void On(string method, OnMethodCallCallbackDelegate callback)
	{
		methodTable[method] = callback;
	}

	public void Off(string method)
	{
		methodTable[method] = null;
	}

	public void Call(string method, params object[] args)
	{
		Call(method, null, null, null, args);
	}

	public void Call(string method, OnMethodResultDelegate onResult, params object[] args)
	{
		Call(method, onResult, null, null, args);
	}

	public void Call(string method, OnMethodResultDelegate onResult, OnMethodFailedDelegate onFailed, params object[] args)
	{
		Call(method, onResult, onFailed, null, args);
	}

	public void Call(string method, OnMethodResultDelegate onResult, OnMethodProgressDelegate onProgress, params object[] args)
	{
		Call(method, onResult, null, onProgress, args);
	}

	public void Call(string method, OnMethodResultDelegate onResult, OnMethodFailedDelegate onFailed, OnMethodProgressDelegate onProgress, params object[] args)
	{
		lock (((IHub)this).HubConnection.SyncRoot)
		{
			Connection hubConnection = ((IHub)this).HubConnection;
			hubConnection.set_ClientMessageCounter(hubConnection.GetClientMessageCounter() % ulong.MaxValue);
			Connection messageConnection = ((IHub)this).HubConnection;
			ulong callIndex;
			messageConnection.set_ClientMessageCounter((callIndex = messageConnection.GetClientMessageCounter()) + 1);
			((IHub)this).Call(new ClientMessage(this, method, args, callIndex, onResult, onFailed, onProgress));
		}
	}

	void IHub.Call(ClientMessage message)
	{
		lock (((IHub)this).HubConnection.SyncRoot)
		{
			sentMessages.Add(message.CallIdx, message);
			((IHub)this).HubConnection.SendJson(BuildMessage(message));
		}
	}

	bool IHub.HasSentMessageId(ulong messageId)
	{
		return sentMessages.ContainsKey(messageId);
	}

	void IHub.Close()
	{
		sentMessages.Clear();
	}

	void IHub.OnMethod(MethodCallMessage message)
	{
		MergeState(message.GetState());
		if (OnMethodCall != null)
		{
			try
			{
				OnMethodCall(this, message.GetMethod(), message.GetArguments());
			}
			catch (Exception ex)
			{
				HTTPManager.GetLogger().Exception("Hub - " + get_Name(), "IHub.OnMethod - OnMethodCall", ex);
			}
		}
		OnMethodCallCallbackDelegate value;
		if (methodTable.TryGetValue(message.GetMethod(), out value) && value != null)
		{
			try
			{
				value(this, message);
				return;
			}
			catch (Exception methodException)
			{
				HTTPManager.GetLogger().Exception("Hub - " + get_Name(), "IHub.OnMethod - callback", methodException);
				return;
			}
		}
		HTTPManager.GetLogger().Information("Hub - " + get_Name(), string.Format("[Client] {0}.{1} (args: {2})", get_Name(), message.GetMethod(), message.GetArguments().Length));
	}

	void IHub.OnMessage(IServerMessage message)
	{
		ulong key = (message as IHubMessage).GetInvocationId();
		ClientMessage value;
		if (!sentMessages.TryGetValue(key, out value))
		{
			HTTPManager.GetLogger().Warning("Hub - " + get_Name(), "OnMessage - Sent message not found with id: " + key);
			return;
		}
		switch (message.get_Type())
		{
		case MessageTypes.Result:
		{
			ResultMessage resultMessage = message as ResultMessage;
			MergeState(resultMessage.GetState());
			if (value.ResultCallback != null)
			{
				value.ResultCallback(this, value, resultMessage);
			}
			sentMessages.Remove(key);
			break;
		}
		case MessageTypes.Failure:
		{
			FailureMessage failureMessage = message as FailureMessage;
			MergeState(failureMessage.GetState());
			if (value.ResultErrorCallback != null)
			{
				value.ResultErrorCallback(this, value, failureMessage);
			}
			sentMessages.Remove(key);
			break;
		}
		case MessageTypes.Progress:
			if (value.ProgressCallback != null)
			{
				value.ProgressCallback(this, value, message as ProgressMessage);
			}
			break;
		}
	}

	private void MergeState(IDictionary<string, object> state)
	{
		if (state == null || state.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<string, object> item in state)
		{
			GetState()[item.Key] = item.Value;
		}
	}

	private string BuildMessage(ClientMessage message)
	{
		try
		{
			builder.Append("{\"H\":\"");
			builder.Append(get_Name());
			builder.Append("\",\"M\":\"");
			builder.Append(message.Method);
			builder.Append("\",\"A\":");
			string empty = string.Empty;
			empty = ((message.Args == null || message.Args.Length <= 0) ? "[]" : ((IHub)this).HubConnection.GetJsonEncoder().Encode(message.Args));
			builder.Append(empty);
			builder.Append(",\"I\":\"");
			builder.Append(message.CallIdx.ToString());
			builder.Append("\"");
			if (message.OwnerHub.state != null && message.OwnerHub.state.Count > 0)
			{
				builder.Append(",\"S\":");
				empty = ((IHub)this).HubConnection.GetJsonEncoder().Encode(message.OwnerHub.state);
				builder.Append(empty);
			}
			builder.Append("}");
			return builder.ToString();
		}
		catch (Exception ex)
		{
			HTTPManager.GetLogger().Exception("Hub - " + get_Name(), "Send", ex);
			return null;
		}
		finally
		{
			builder.Length = 0;
		}
	}
}
