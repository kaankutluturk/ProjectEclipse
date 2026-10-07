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

	public Hub(string name, Connection BJGMPDIKEJC)
	{
		set_Name(name);
		((IHub)this).GNLCPJFBAJE(BJGMPDIKEJC);
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
		OnMethodCallDelegate kOBOMHLOBON = OnMethodCall;
		OnMethodCallDelegate kOBOMHLOBON2;
		do
		{
			kOBOMHLOBON2 = kOBOMHLOBON;
			kOBOMHLOBON = Interlocked.CompareExchange(ref OnMethodCall, (OnMethodCallDelegate)Delegate.Combine(kOBOMHLOBON2, value), kOBOMHLOBON);
		}
		while ((object)kOBOMHLOBON != kOBOMHLOBON2);
	}

	public void RemoveOnMethodCall(OnMethodCallDelegate value)
	{
		OnMethodCallDelegate kOBOMHLOBON = OnMethodCall;
		OnMethodCallDelegate kOBOMHLOBON2;
		do
		{
			kOBOMHLOBON2 = kOBOMHLOBON;
			kOBOMHLOBON = Interlocked.CompareExchange(ref OnMethodCall, (OnMethodCallDelegate)Delegate.Remove(kOBOMHLOBON2, value), kOBOMHLOBON);
		}
		while ((object)kOBOMHLOBON != kOBOMHLOBON2);
	}

	Connection IHub.PEBFDIFIMBO
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

	void IHub.GNLCPJFBAJE(Connection value)
	{
		connection = value;
	}

	public void On(string FJLOLCPJACB, OnMethodCallCallbackDelegate callback)
	{
		methodTable[FJLOLCPJACB] = callback;
	}

	public void Off(string FJLOLCPJACB)
	{
		methodTable[FJLOLCPJACB] = null;
	}

	public void Call(string FJLOLCPJACB, params object[] LKIOKGCNKHE)
	{
		Call(FJLOLCPJACB, null, null, null, LKIOKGCNKHE);
	}

	public void Call(string FJLOLCPJACB, OnMethodResultDelegate KGLHKHHFNOO, params object[] LKIOKGCNKHE)
	{
		Call(FJLOLCPJACB, KGLHKHHFNOO, null, null, LKIOKGCNKHE);
	}

	public void Call(string FJLOLCPJACB, OnMethodResultDelegate KGLHKHHFNOO, OnMethodFailedDelegate PLEIBDIHIFO, params object[] LKIOKGCNKHE)
	{
		Call(FJLOLCPJACB, KGLHKHHFNOO, PLEIBDIHIFO, null, LKIOKGCNKHE);
	}

	public void Call(string FJLOLCPJACB, OnMethodResultDelegate KGLHKHHFNOO, OnMethodProgressDelegate LFAIENNBBMK, params object[] LKIOKGCNKHE)
	{
		Call(FJLOLCPJACB, KGLHKHHFNOO, null, LFAIENNBBMK, LKIOKGCNKHE);
	}

	public void Call(string FJLOLCPJACB, OnMethodResultDelegate KGLHKHHFNOO, OnMethodFailedDelegate PLEIBDIHIFO, OnMethodProgressDelegate LFAIENNBBMK, params object[] LKIOKGCNKHE)
	{
		lock (((IHub)this).HubConnection.SyncRoot)
		{
			Connection hDMLLEEKKLF = ((IHub)this).HubConnection;
			hDMLLEEKKLF.set_ClientMessageCounter(hDMLLEEKKLF.GetClientMessageCounter() % ulong.MaxValue);
			Connection hDMLLEEKKLF2 = ((IHub)this).HubConnection;
			ulong kKAADAAPLDC;
			hDMLLEEKKLF2.set_ClientMessageCounter((kKAADAAPLDC = hDMLLEEKKLF2.GetClientMessageCounter()) + 1);
			((IHub)this).Call(new ClientMessage(this, FJLOLCPJACB, LKIOKGCNKHE, kKAADAAPLDC, KGLHKHHFNOO, PLEIBDIHIFO, LFAIENNBBMK));
		}
	}

	void IHub.Call(ClientMessage CKEHOEGLMBM)
	{
		lock (((IHub)this).HubConnection.SyncRoot)
		{
			sentMessages.Add(CKEHOEGLMBM.CallIdx, CKEHOEGLMBM);
			((IHub)this).HubConnection.SendJson(BuildMessage(CKEHOEGLMBM));
		}
	}

	bool IHub.HasSentMessageId(ulong OKNNNLIPODI)
	{
		return sentMessages.ContainsKey(OKNNNLIPODI);
	}

	void IHub.Close()
	{
		sentMessages.Clear();
	}

	void IHub.OnMethod(MethodCallMessage CKEHOEGLMBM)
	{
		MergeState(CKEHOEGLMBM.GetState());
		if (OnMethodCall != null)
		{
			try
			{
				OnMethodCall(this, CKEHOEGLMBM.GetMethod(), CKEHOEGLMBM.GetArguments());
			}
			catch (Exception mPFFFAOGBJE)
			{
				HTTPManager.GetLogger().Exception("Hub - " + get_Name(), "IHub.OnMethod - OnMethodCall", mPFFFAOGBJE);
			}
		}
		OnMethodCallCallbackDelegate value;
		if (methodTable.TryGetValue(CKEHOEGLMBM.GetMethod(), out value) && value != null)
		{
			try
			{
				value(this, CKEHOEGLMBM);
				return;
			}
			catch (Exception mPFFFAOGBJE2)
			{
				HTTPManager.GetLogger().Exception("Hub - " + get_Name(), "IHub.OnMethod - callback", mPFFFAOGBJE2);
				return;
			}
		}
		HTTPManager.GetLogger().Information("Hub - " + get_Name(), string.Format("[Client] {0}.{1} (args: {2})", get_Name(), CKEHOEGLMBM.GetMethod(), CKEHOEGLMBM.GetArguments().Length));
	}

	void IHub.OnMessage(IServerMessage CKEHOEGLMBM)
	{
		ulong key = (CKEHOEGLMBM as IHubMessage).GetInvocationId();
		ClientMessage value;
		if (!sentMessages.TryGetValue(key, out value))
		{
			HTTPManager.GetLogger().Warning("Hub - " + get_Name(), "OnMessage - Sent message not found with id: " + key);
			return;
		}
		switch (CKEHOEGLMBM.get_Type())
		{
		case MessageTypes.Result:
		{
			ResultMessage mKINDKDMCJO = CKEHOEGLMBM as ResultMessage;
			MergeState(mKINDKDMCJO.GetState());
			if (value.ResultCallback != null)
			{
				value.ResultCallback(this, value, mKINDKDMCJO);
			}
			sentMessages.Remove(key);
			break;
		}
		case MessageTypes.Failure:
		{
			FailureMessage kGPJFMCLKDJ = CKEHOEGLMBM as FailureMessage;
			MergeState(kGPJFMCLKDJ.GetState());
			if (value.ResultErrorCallback != null)
			{
				value.ResultErrorCallback(this, value, kGPJFMCLKDJ);
			}
			sentMessages.Remove(key);
			break;
		}
		case MessageTypes.Progress:
			if (value.ProgressCallback != null)
			{
				value.ProgressCallback(this, value, CKEHOEGLMBM as ProgressMessage);
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

	private string BuildMessage(ClientMessage CKEHOEGLMBM)
	{
		try
		{
			builder.Append("{\"H\":\"");
			builder.Append(get_Name());
			builder.Append("\",\"M\":\"");
			builder.Append(CKEHOEGLMBM.Method);
			builder.Append("\",\"A\":");
			string empty = string.Empty;
			empty = ((CKEHOEGLMBM.Args == null || CKEHOEGLMBM.Args.Length <= 0) ? "[]" : ((IHub)this).HubConnection.GetJsonEncoder().Encode(CKEHOEGLMBM.Args));
			builder.Append(empty);
			builder.Append(",\"I\":\"");
			builder.Append(CKEHOEGLMBM.CallIdx.ToString());
			builder.Append("\"");
			if (CKEHOEGLMBM.OwnerHub.state != null && CKEHOEGLMBM.OwnerHub.state.Count > 0)
			{
				builder.Append(",\"S\":");
				empty = ((IHub)this).HubConnection.GetJsonEncoder().Encode(CKEHOEGLMBM.OwnerHub.state);
				builder.Append(empty);
			}
			builder.Append("}");
			return builder.ToString();
		}
		catch (Exception mPFFFAOGBJE)
		{
			HTTPManager.GetLogger().Exception("Hub - " + get_Name(), "Send", mPFFFAOGBJE);
			return null;
		}
		finally
		{
			builder.Length = 0;
		}
	}
}
