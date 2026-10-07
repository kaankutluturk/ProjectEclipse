using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

[DefaultMember("Item")]
public sealed class Connection : IHeartbeat, IConnection
{
	public static IJsonEncoder DefaultEncoder = new DefaultJsonEncoder();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Uri uri;

	private ConnectionStates state;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private NegotiationData negotiationResult;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Hub[] hubs;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TransportBase transport;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Dictionary<string, string> additionalQueryParams;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool queryParamsOnlyForHandshake;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private IJsonEncoder jsonEncoder;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private IAuthenticationProvider authenticationProvider;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private OnConnectedDelegate OnConnected;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private OnClosedDelegate onClosedField;

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private OnConnectionErrorDelegate onErrorField;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private OnConnectedDelegate OnReconnecting;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private OnConnectedDelegate OnReconnected;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private OnConnectionStateChangedDelegate OnStateChanged;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private OnNonHubMessageDelegate OnNonHubMessage;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private OnPrepareRequestDelegate requestPreparator;

	internal object SyncRoot = new object();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ulong clientMessageCounter;

	private readonly string clientProtocol = "1.5";

	private ulong requestCounter;

	private MultiMessage lastReceivedMessage;

	private string groupsToken;

	private List<IServerMessage> bufferedMessages;

	private DateTime lastMessageReceivedAt;

	private DateTime? reconnectStartedAt;

	private DateTime lastPingSentAt;

	private TimeSpan PingInterval;

	private HTTPRequest pingRequest;

	private DateTime? transportConnectionStartedAt;

	private StringBuilder queryBuilder = new StringBuilder();

	private string builtConnectionData;

	private string BuiltQueryParams;

	private SupportedProtocols nextProtocolToTry;

	public Uri ConnectionUri
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

	public ConnectionStates ConnectionState
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

	public NegotiationData NegotiationResult
	{
		get
		{
			return GetNegotiationResult();
		}
		private set
		{
			SetNegotiationResult(value);
		}
	}

	public Hub[] Hubs
	{
		get
		{
			return GetHubs();
		}
		private set
		{
			SetHubs(value);
		}
	}

	public TransportBase ActiveTransport
	{
		get
		{
			return GetTransport();
		}
		private set
		{
			SetTransport(value);
		}
	}

	public Dictionary<string, string> ExtraQueryParams
	{
		get
		{
			return GetAdditionalQueryParams();
		}
		set
		{
			set_AdditionalQueryParams(value);
		}
	}

	public bool QueryParamsHandshakeOnly
	{
		get
		{
			return GetQueryParamsOnlyForHandshake();
		}
		set
		{
			set_QueryParamsOnlyForHandshake(value);
		}
	}

	public IJsonEncoder JsonEncoder
	{
		get
		{
			return GetJsonEncoder();
		}
		set
		{
			SetJsonEncoder(value);
		}
	}

	public IAuthenticationProvider AuthenticationProvider
	{
		get
		{
			return GetAuthenticationProvider();
		}
		set
		{
			SetAuthenticationProvider(value);
		}
	}

	public OnPrepareRequestDelegate RequestPreparator
	{
		get
		{
			return GetRequestPreparator();
		}
		set
		{
			SetRequestPreparator(value);
		}
	}

	// C# has no syntax for parameterized property 'DLKPBAJDHBO'.
	public Hub get_DLKPBAJDHBO(int index)
	{
		return get_Item(index);
	}

	// C# has no syntax for parameterized property 'DLKPBAJDHBO'.
	public Hub get_DLKPBAJDHBO(string name)
	{
		return get_Item(name);
	}

	internal ulong ClientMessageIdCounter
	{
		get
		{
			return GetClientMessageCounter();
		}
		set
		{
			set_ClientMessageCounter(value);
		}
	}

	private uint Timestamp
	{
		get
		{
			return GetTimestamp();
		}
	}

	private string ConnectionData
	{
		get
		{
			return GetConnectionData();
		}
	}

	private string BuiltQueryString
	{
		get
		{
			return GetQueryParams();
		}
	}

	public event OnConnectedDelegate Connected
	{
		add
		{
			AddConnectedHandler(value);
		}
		remove
		{
			RemoveConnectedHandler(value);
		}
	}

	public event OnClosedDelegate OnClosed
	{
		add
		{
			AddClosedHandler(value);
		}
		remove
		{
			RemoveClosedHandler(value);
		}
	}

	public event OnConnectionErrorDelegate OnError
	{
		add
		{
			AddErrorHandler(value);
		}
		remove
		{
			RemoveErrorHandler(value);
		}
	}

	public event OnConnectedDelegate Reconnecting
	{
		add
		{
			AddReconnectingHandler(value);
		}
		remove
		{
			RemoveReconnectingHandler(value);
		}
	}

	public event OnConnectedDelegate Reconnected
	{
		add
		{
			AddReconnectedHandler(value);
		}
		remove
		{
			RemoveReconnectedHandler(value);
		}
	}

	public event OnConnectionStateChangedDelegate StateChanged
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

	public event OnNonHubMessageDelegate NonHubMessageReceived
	{
		add
		{
			AddNonHubMessageHandler(value);
		}
		remove
		{
			RemoveNonHubMessageHandler(value);
		}
	}

	public Connection(Uri uri, params string[] hubNames)
		: this(uri)
	{
		if (hubNames != null && hubNames.Length > 0)
		{
			SetHubs(new Hub[hubNames.Length]);
			for (int i = 0; i < hubNames.Length; i++)
			{
				GetHubs()[i] = new Hub(hubNames[i], this);
			}
		}
	}

	public Connection(Uri uri, params Hub[] hubs)
		: this(uri)
	{
		SetHubs(hubs);
		if (hubs != null)
		{
			for (int i = 0; i < hubs.Length; i++)
			{
				((IHub)hubs[i]).GNLCPJFBAJE(this);
			}
		}
	}

	public Connection(Uri uri)
	{
		set_State(ConnectionStates.Initial);
		set_Uri(uri);
		SetJsonEncoder(DefaultEncoder);
		PingInterval = TimeSpan.FromMinutes(5.0);
	}

	public Uri GetUri()
	{
		return uri;
	}

	private void set_Uri(Uri value)
	{
		uri = value;
	}

	public ConnectionStates GetState()
	{
		return state;
	}

	private void set_State(ConnectionStates value)
	{
		ConnectionStates oldState = state;
		state = value;
		if (OnStateChanged != null)
		{
			OnStateChanged(this, oldState, state);
		}
	}

	public NegotiationData GetNegotiationResult()
	{
		return negotiationResult;
	}

	private void SetNegotiationResult(NegotiationData value)
	{
		negotiationResult = value;
	}

	public Hub[] GetHubs()
	{
		return hubs;
	}

	private void SetHubs(Hub[] value)
	{
		hubs = value;
	}

	public TransportBase GetTransport()
	{
		return transport;
	}

	private void SetTransport(TransportBase value)
	{
		transport = value;
	}

	public Dictionary<string, string> GetAdditionalQueryParams()
	{
		return additionalQueryParams;
	}

	public void set_AdditionalQueryParams(Dictionary<string, string> value)
	{
		additionalQueryParams = value;
	}

	public bool GetQueryParamsOnlyForHandshake()
	{
		return queryParamsOnlyForHandshake;
	}

	public void set_QueryParamsOnlyForHandshake(bool value)
	{
		queryParamsOnlyForHandshake = value;
	}

	public IJsonEncoder GetJsonEncoder()
	{
		return jsonEncoder;
	}

	public void SetJsonEncoder(IJsonEncoder value)
	{
		jsonEncoder = value;
	}

	public IAuthenticationProvider GetAuthenticationProvider()
	{
		return authenticationProvider;
	}

	public void SetAuthenticationProvider(IAuthenticationProvider value)
	{
		authenticationProvider = value;
	}

	public void AddConnectedHandler(OnConnectedDelegate value)
	{
		OnConnectedDelegate currentHandler = OnConnected;
		OnConnectedDelegate pILIPIHGBEG2;
		do
		{
			pILIPIHGBEG2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnConnected, (OnConnectedDelegate)Delegate.Combine(pILIPIHGBEG2, value), currentHandler);
		}
		while ((object)currentHandler != pILIPIHGBEG2);
	}

	public void RemoveConnectedHandler(OnConnectedDelegate value)
	{
		OnConnectedDelegate currentHandler = OnConnected;
		OnConnectedDelegate pILIPIHGBEG2;
		do
		{
			pILIPIHGBEG2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnConnected, (OnConnectedDelegate)Delegate.Remove(pILIPIHGBEG2, value), currentHandler);
		}
		while ((object)currentHandler != pILIPIHGBEG2);
	}

	public void AddClosedHandler(OnClosedDelegate value)
	{
		OnClosedDelegate currentHandler = onClosedField;
		OnClosedDelegate kMBJIOLJJCE2;
		do
		{
			kMBJIOLJJCE2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref onClosedField, (OnClosedDelegate)Delegate.Combine(kMBJIOLJJCE2, value), currentHandler);
		}
		while ((object)currentHandler != kMBJIOLJJCE2);
	}

	public void RemoveClosedHandler(OnClosedDelegate value)
	{
		OnClosedDelegate currentHandler = onClosedField;
		OnClosedDelegate kMBJIOLJJCE2;
		do
		{
			kMBJIOLJJCE2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref onClosedField, (OnClosedDelegate)Delegate.Remove(kMBJIOLJJCE2, value), currentHandler);
		}
		while ((object)currentHandler != kMBJIOLJJCE2);
	}

	public void AddErrorHandler(OnConnectionErrorDelegate value)
	{
		OnConnectionErrorDelegate currentHandler = onErrorField;
		OnConnectionErrorDelegate dHGLHLDFDAC2;
		do
		{
			dHGLHLDFDAC2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref onErrorField, (OnConnectionErrorDelegate)Delegate.Combine(dHGLHLDFDAC2, value), currentHandler);
		}
		while ((object)currentHandler != dHGLHLDFDAC2);
	}

	public void RemoveErrorHandler(OnConnectionErrorDelegate value)
	{
		OnConnectionErrorDelegate currentHandler = onErrorField;
		OnConnectionErrorDelegate dHGLHLDFDAC2;
		do
		{
			dHGLHLDFDAC2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref onErrorField, (OnConnectionErrorDelegate)Delegate.Remove(dHGLHLDFDAC2, value), currentHandler);
		}
		while ((object)currentHandler != dHGLHLDFDAC2);
	}

	public void AddReconnectingHandler(OnConnectedDelegate value)
	{
		OnConnectedDelegate currentHandler = OnReconnecting;
		OnConnectedDelegate pILIPIHGBEG2;
		do
		{
			pILIPIHGBEG2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnReconnecting, (OnConnectedDelegate)Delegate.Combine(pILIPIHGBEG2, value), currentHandler);
		}
		while ((object)currentHandler != pILIPIHGBEG2);
	}

	public void RemoveReconnectingHandler(OnConnectedDelegate value)
	{
		OnConnectedDelegate currentHandler = OnReconnecting;
		OnConnectedDelegate pILIPIHGBEG2;
		do
		{
			pILIPIHGBEG2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnReconnecting, (OnConnectedDelegate)Delegate.Remove(pILIPIHGBEG2, value), currentHandler);
		}
		while ((object)currentHandler != pILIPIHGBEG2);
	}

	public void AddReconnectedHandler(OnConnectedDelegate value)
	{
		OnConnectedDelegate currentHandler = OnReconnected;
		OnConnectedDelegate pILIPIHGBEG2;
		do
		{
			pILIPIHGBEG2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnReconnected, (OnConnectedDelegate)Delegate.Combine(pILIPIHGBEG2, value), currentHandler);
		}
		while ((object)currentHandler != pILIPIHGBEG2);
	}

	public void RemoveReconnectedHandler(OnConnectedDelegate value)
	{
		OnConnectedDelegate currentHandler = OnReconnected;
		OnConnectedDelegate pILIPIHGBEG2;
		do
		{
			pILIPIHGBEG2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnReconnected, (OnConnectedDelegate)Delegate.Remove(pILIPIHGBEG2, value), currentHandler);
		}
		while ((object)currentHandler != pILIPIHGBEG2);
	}

	public void AddStateChangedHandler(OnConnectionStateChangedDelegate value)
	{
		OnConnectionStateChangedDelegate currentHandler = OnStateChanged;
		OnConnectionStateChangedDelegate aIBCPDGLFPB2;
		do
		{
			aIBCPDGLFPB2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnStateChanged, (OnConnectionStateChangedDelegate)Delegate.Combine(aIBCPDGLFPB2, value), currentHandler);
		}
		while ((object)currentHandler != aIBCPDGLFPB2);
	}

	public void RemoveStateChangedHandler(OnConnectionStateChangedDelegate value)
	{
		OnConnectionStateChangedDelegate currentHandler = OnStateChanged;
		OnConnectionStateChangedDelegate aIBCPDGLFPB2;
		do
		{
			aIBCPDGLFPB2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnStateChanged, (OnConnectionStateChangedDelegate)Delegate.Remove(aIBCPDGLFPB2, value), currentHandler);
		}
		while ((object)currentHandler != aIBCPDGLFPB2);
	}

	public void AddNonHubMessageHandler(OnNonHubMessageDelegate value)
	{
		OnNonHubMessageDelegate currentHandler = OnNonHubMessage;
		OnNonHubMessageDelegate gAGJEANDJEK2;
		do
		{
			gAGJEANDJEK2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnNonHubMessage, (OnNonHubMessageDelegate)Delegate.Combine(gAGJEANDJEK2, value), currentHandler);
		}
		while ((object)currentHandler != gAGJEANDJEK2);
	}

	public void RemoveNonHubMessageHandler(OnNonHubMessageDelegate value)
	{
		OnNonHubMessageDelegate currentHandler = OnNonHubMessage;
		OnNonHubMessageDelegate gAGJEANDJEK2;
		do
		{
			gAGJEANDJEK2 = currentHandler;
			currentHandler = Interlocked.CompareExchange(ref OnNonHubMessage, (OnNonHubMessageDelegate)Delegate.Remove(gAGJEANDJEK2, value), currentHandler);
		}
		while ((object)currentHandler != gAGJEANDJEK2);
	}

	public OnPrepareRequestDelegate GetRequestPreparator()
	{
		return requestPreparator;
	}

	public void SetRequestPreparator(OnPrepareRequestDelegate value)
	{
		requestPreparator = value;
	}

	public Hub get_Item(int index)
	{
		return GetHubs()[index];
	}

	public Hub get_Item(string name)
	{
		for (int i = 0; i < GetHubs().Length; i++)
		{
			Hub hub = GetHubs()[i];
			if (hub.get_Name().Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				return hub;
			}
		}
		return null;
	}

	internal ulong GetClientMessageCounter()
	{
		return clientMessageCounter;
	}

	internal void set_ClientMessageCounter(ulong value)
	{
		clientMessageCounter = value;
	}

	private uint GetTimestamp()
	{
		return (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Ticks;
	}

	private string GetConnectionData()
	{
		if (!string.IsNullOrEmpty(builtConnectionData))
		{
			return builtConnectionData;
		}
		StringBuilder stringBuilder = new StringBuilder("[", GetHubs().Length * 4);
		if (GetHubs() != null)
		{
			for (int i = 0; i < GetHubs().Length; i++)
			{
				stringBuilder.Append("{\"Name\":\"");
				stringBuilder.Append(GetHubs()[i].get_Name());
				stringBuilder.Append("\"}");
				if (i < GetHubs().Length - 1)
				{
					stringBuilder.Append(",");
				}
			}
		}
		stringBuilder.Append("]");
		return builtConnectionData = Uri.EscapeUriString(stringBuilder.ToString());
	}

	private string GetQueryParams()
	{
		if (GetAdditionalQueryParams() == null || GetAdditionalQueryParams().Count == 0)
		{
			return string.Empty;
		}
		if (!string.IsNullOrEmpty(BuiltQueryParams))
		{
			return BuiltQueryParams;
		}
		StringBuilder stringBuilder = new StringBuilder(GetAdditionalQueryParams().Count * 4);
		foreach (KeyValuePair<string, string> item in GetAdditionalQueryParams())
		{
			stringBuilder.Append("&");
			stringBuilder.Append(item.Key);
			if (!string.IsNullOrEmpty(item.Value))
			{
				stringBuilder.Append("=");
				stringBuilder.Append(Uri.EscapeDataString(item.Value));
			}
		}
		return BuiltQueryParams = stringBuilder.ToString();
	}

	public void OpenConnection()
	{
		if (GetState() == ConnectionStates.Initial || GetState() == ConnectionStates.Closed)
		{
			if (GetAuthenticationProvider() != null && GetAuthenticationProvider().GetIsPreAuthRequired())
			{
				set_State(ConnectionStates.Authenticating);
				GetAuthenticationProvider().AddAuthenticationSucceeded(OnAuthenticationSucceeded);
				GetAuthenticationProvider().AddAuthenticationFailed(OnAuthenticationFailed);
				GetAuthenticationProvider().StartAuthentication();
			}
			else
			{
				StartImpl();
			}
		}
	}

	private void OnAuthenticationSucceeded(IAuthenticationProvider provider)
	{
		provider.RemoveAuthenticationSucceeded(OnAuthenticationSucceeded);
		StartImpl();
	}

	private void OnAuthenticationFailed(IAuthenticationProvider provider, string error)
	{
		provider.RemoveAuthenticationFailed(OnAuthenticationFailed);
		((IConnection)this).Error(error);
	}

	private void StartImpl()
	{
		set_State(ConnectionStates.Negotiating);
		SetNegotiationResult(new NegotiationData(this));
		GetNegotiationResult().OnReceived = OnNegotiationDataReceived;
		GetNegotiationResult().OnError = OnNegotiationError;
		GetNegotiationResult().Start();
	}

	private void OnNegotiationDataReceived(NegotiationData data)
	{
		if (data.GetTryWebSockets())
		{
			SetTransport(new SignalRWebSocketTransport(this));
			nextProtocolToTry = SupportedProtocols.ServerSentEvents;
		}
		else
		{
			SetTransport(new ServerSentEventsTransport(this));
			nextProtocolToTry = SupportedProtocols.HTTP;
		}
		set_State(ConnectionStates.Connecting);
		transportConnectionStartedAt = DateTime.UtcNow;
		GetTransport().Connect();
	}

	private void OnNegotiationError(NegotiationData data, string error)
	{
		((IConnection)this).Error(error);
	}

	public void Close()
	{
		if (GetState() == ConnectionStates.Closed)
		{
			return;
		}
		set_State(ConnectionStates.Closed);
		reconnectStartedAt = null;
		transportConnectionStartedAt = null;
		if (GetTransport() != null)
		{
			GetTransport().Abort();
			SetTransport(null);
		}
		SetNegotiationResult(null);
		HTTPManager.GetHeartbeats().Unsubscribe(this);
		lastReceivedMessage = null;
		if (GetHubs() != null)
		{
			for (int i = 0; i < GetHubs().Length; i++)
			{
				((IHub)GetHubs()[i]).Close();
			}
		}
		if (bufferedMessages != null)
		{
			bufferedMessages.Clear();
			bufferedMessages = null;
		}
		if (onClosedField == null)
		{
			return;
		}
		try
		{
			onClosedField(this);
		}
		catch (Exception exception)
		{
			HTTPManager.GetLogger().Exception("SignalR Connection", "OnClosed", exception);
		}
	}

	public void Reconnect()
	{
		DateTime? reconnectStart = reconnectStartedAt;
		if (reconnectStart.HasValue)
		{
			return;
		}
		HTTPManager.GetLogger().Warning("SignalR Connection", "Reconnecting");
		set_State(ConnectionStates.Reconnecting);
		reconnectStartedAt = DateTime.UtcNow;
		GetTransport().Reconnect();
		if (pingRequest != null)
		{
			pingRequest.Abort();
		}
		if (OnReconnecting == null)
		{
			return;
		}
		try
		{
			OnReconnecting(this);
		}
		catch (Exception exception)
		{
			HTTPManager.GetLogger().Exception("SignalR Connection", "OnReconnecting", exception);
		}
	}

	public void Send(object data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("arg");
		}
		lock (SyncRoot)
		{
			if (GetState() == ConnectionStates.Connected)
			{
				string json = GetJsonEncoder().Encode(data);
				GetTransport().Send(json);
			}
		}
	}

	public void SendJson(string json)
	{
		if (json == null)
		{
			throw new ArgumentNullException("json");
		}
		lock (SyncRoot)
		{
			if (GetState() == ConnectionStates.Connected)
			{
				GetTransport().Send(json);
			}
		}
	}

	void IConnection.OnMessage(IServerMessage message)
	{
		if (GetState() == ConnectionStates.Closed)
		{
			return;
		}
		if (GetState() == ConnectionStates.Connecting)
		{
			if (bufferedMessages == null)
			{
				bufferedMessages = new List<IServerMessage>();
			}
			bufferedMessages.Add(message);
			return;
		}
		lastMessageReceivedAt = DateTime.UtcNow;
		switch (message.get_Type())
		{
		case MessageTypes.Multiple:
			lastReceivedMessage = message as MultiMessage;
			if (lastReceivedMessage.GetIsInitialization())
			{
				HTTPManager.GetLogger().Information("SignalR Connection", "OnMessage - Init");
			}
			if (lastReceivedMessage.GetGroupsToken() != null)
			{
				groupsToken = lastReceivedMessage.GetGroupsToken();
			}
			if (lastReceivedMessage.GetShouldReconnect())
			{
				HTTPManager.GetLogger().Information("SignalR Connection", "OnMessage - Should Reconnect");
				Reconnect();
			}
			if (lastReceivedMessage.GetData() != null)
			{
				for (int i = 0; i < lastReceivedMessage.GetData().Count; i++)
				{
					((IConnection)this).OnMessage(lastReceivedMessage.GetData()[i]);
				}
			}
			break;
		case MessageTypes.MethodCall:
		{
			MethodCallMessage methodCall = message as MethodCallMessage;
			Hub hub = get_Item(methodCall.GetHub());
			if (hub != null)
			{
				((IHub)hub).OnMethod(methodCall);
			}
			else
			{
				HTTPManager.GetLogger().Warning("SignalR Connection", string.Format("Hub \"{0}\" not found!", methodCall.GetHub()));
			}
			break;
		}
		case MessageTypes.Result:
		case MessageTypes.Failure:
		case MessageTypes.Progress:
		{
			ulong messageId = (message as IHubMessage).GetInvocationId();
			Hub hub = FindHub(messageId);
			if (hub != null)
			{
				((IHub)hub).OnMessage(message);
			}
			else
			{
				HTTPManager.GetLogger().Warning("SignalR Connection", string.Format("No Hub found for Progress message! Id: {0}", messageId.ToString()));
			}
			break;
		}
		case MessageTypes.Data:
			if (OnNonHubMessage != null)
			{
				OnNonHubMessage(this, (message as DataMessage).GetData());
			}
			break;
		case MessageTypes.KeepAlive:
			break;
		default:
			HTTPManager.GetLogger().Warning("SignalR Connection", "Unknown message type received: " + message.get_Type());
			break;
		}
	}

	void IConnection.TransportStarted()
	{
		if (GetState() != ConnectionStates.Connecting)
		{
			return;
		}
		InitOnStart();
		if (OnConnected != null)
		{
			try
			{
				OnConnected(this);
			}
			catch (Exception exception)
			{
				HTTPManager.GetLogger().Exception("SignalR Connection", "OnOpened", exception);
			}
		}
		if (bufferedMessages != null)
		{
			for (int i = 0; i < bufferedMessages.Count; i++)
			{
				((IConnection)this).OnMessage(bufferedMessages[i]);
			}
			bufferedMessages.Clear();
			bufferedMessages = null;
		}
	}

	void IConnection.TransportReconnected()
	{
		if (GetState() != ConnectionStates.Reconnecting)
		{
			return;
		}
		HTTPManager.GetLogger().Information("SignalR Connection", "Transport Reconnected");
		InitOnStart();
		if (OnReconnected == null)
		{
			return;
		}
		try
		{
			OnReconnected(this);
		}
		catch (Exception exception)
		{
			HTTPManager.GetLogger().Exception("SignalR Connection", "OnReconnected", exception);
		}
	}

	void IConnection.TransportAborted()
	{
		Close();
	}

	void IConnection.Error(string error)
	{
		if (GetState() != ConnectionStates.Closed)
		{
			HTTPManager.GetLogger().Error("SignalR Connection", error);
			if (onErrorField != null)
			{
				onErrorField(this, error);
			}
			if (GetState() == ConnectionStates.Connected || GetState() == ConnectionStates.Reconnecting)
			{
				Reconnect();
			}
			else if (GetState() != ConnectionStates.Connecting || !TryFallbackTransport())
			{
				Close();
			}
		}
	}

	Uri IConnection.BuildUri(SignalRRequestType requestType)
	{
		return ((IConnection)this).BuildUri(requestType, (TransportBase)null);
	}

	Uri IConnection.BuildUri(SignalRRequestType requestType, TransportBase transport)
	{
		lock (SyncRoot)
		{
			queryBuilder.Length = 0;
			UriBuilder uriBuilder = new UriBuilder(GetUri());
			if (!uriBuilder.Path.EndsWith("/"))
			{
				uriBuilder.Path += "/";
			}
			requestCounter %= ulong.MaxValue;
			switch (requestType)
			{
			case SignalRRequestType.Negotiate:
				uriBuilder.Path += "negotiate";
				goto default;
			case SignalRRequestType.Connect:
				if (transport != null && transport.get_Type() == TransportTypes.WebSocket)
				{
					uriBuilder.Scheme = ((!HTTPProtocolFactory.IsSecureProtocol(GetUri())) ? "ws" : "wss");
				}
				uriBuilder.Path += "connect";
				goto default;
			case SignalRRequestType.Start:
				uriBuilder.Path += "start";
				goto default;
			case SignalRRequestType.Poll:
				uriBuilder.Path += "poll";
				if (lastReceivedMessage != null)
				{
					queryBuilder.Append("messageId=");
					queryBuilder.Append(lastReceivedMessage.GetMessageId());
				}
				goto default;
			case SignalRRequestType.Send:
				uriBuilder.Path += "send";
				goto default;
			case SignalRRequestType.Reconnect:
				if (transport != null && transport.get_Type() == TransportTypes.WebSocket)
				{
					uriBuilder.Scheme = ((!HTTPProtocolFactory.IsSecureProtocol(GetUri())) ? "ws" : "wss");
				}
				uriBuilder.Path += "reconnect";
				if (lastReceivedMessage != null)
				{
					queryBuilder.Append("messageId=");
					queryBuilder.Append(lastReceivedMessage.GetMessageId());
				}
				if (!string.IsNullOrEmpty(groupsToken))
				{
					if (queryBuilder.Length > 0)
					{
						queryBuilder.Append("&");
					}
					queryBuilder.Append("groupsToken=");
					queryBuilder.Append(groupsToken);
				}
				goto default;
			case SignalRRequestType.Abort:
				uriBuilder.Path += "abort";
				goto default;
			case SignalRRequestType.Ping:
				uriBuilder.Path += "ping";
				queryBuilder.Append("&tid=");
				queryBuilder.Append(requestCounter++.ToString());
				queryBuilder.Append("&_=");
				queryBuilder.Append(GetTimestamp().ToString());
				break;
			default:
				if (queryBuilder.Length > 0)
				{
					queryBuilder.Append("&");
				}
				queryBuilder.Append("tid=");
				queryBuilder.Append(requestCounter++.ToString());
				queryBuilder.Append("&_=");
				queryBuilder.Append(GetTimestamp().ToString());
				if (transport != null)
				{
					queryBuilder.Append("&transport=");
					queryBuilder.Append(transport.get_Name());
				}
				queryBuilder.Append("&clientProtocol=");
				queryBuilder.Append(clientProtocol);
				if (GetNegotiationResult() != null && !string.IsNullOrEmpty(GetNegotiationResult().GetConnectionToken()))
				{
					queryBuilder.Append("&connectionToken=");
					queryBuilder.Append(GetNegotiationResult().GetConnectionToken());
				}
				if (GetHubs() != null && GetHubs().Length > 0)
				{
					queryBuilder.Append("&connectionData=");
					queryBuilder.Append(GetConnectionData());
				}
				break;
			}
			if (GetAdditionalQueryParams() != null && GetAdditionalQueryParams().Count > 0)
			{
				queryBuilder.Append(GetQueryParams());
			}
			uriBuilder.Query = queryBuilder.ToString();
			queryBuilder.Length = 0;
			return uriBuilder.Uri;
		}
	}

	HTTPRequest IConnection.PrepareRequest(HTTPRequest request, SignalRRequestType requestType)
	{
		if (request != null && GetAuthenticationProvider() != null)
		{
			GetAuthenticationProvider().PrepareRequest(request, requestType);
		}
		if (GetRequestPreparator() != null)
		{
			GetRequestPreparator()(this, request, requestType);
		}
		return request;
	}

	string IConnection.ParseResponse(string response)
	{
		Dictionary<string, object> dictionary = Json.Decode(response) as Dictionary<string, object>;
		if (dictionary == null)
		{
			((IConnection)this).Error("Failed to parse Start response: " + response);
			return string.Empty;
		}
		object value;
		if (!dictionary.TryGetValue("Response", out value) || value == null)
		{
			((IConnection)this).Error("No 'Response' key found in response: " + response);
			return string.Empty;
		}
		return value.ToString();
	}

	void IHeartbeat.OnHeartbeatUpdate(TimeSpan elapsed)
	{
		ConnectionStates state = GetState();
		if (state == ConnectionStates.Connected)
		{
			if (GetTransport().GetSupportsKeepAlive() && GetNegotiationResult().GetKeepAliveTimeout().HasValue)
			{
				TimeSpan? timeSpan = GetNegotiationResult().GetKeepAliveTimeout();
				if (timeSpan.HasValue && DateTime.UtcNow - lastMessageReceivedAt >= timeSpan.GetValueOrDefault())
				{
					Reconnect();
				}
			}
			if (pingRequest == null && DateTime.UtcNow - lastPingSentAt >= PingInterval)
			{
				Ping();
			}
			return;
		}
		DateTime? transportStart = transportConnectionStartedAt;
		if (transportStart.HasValue)
		{
			DateTime? jEAPOHAGCLL2 = transportConnectionStartedAt;
			TimeSpan? timeSpan2 = ((!jEAPOHAGCLL2.HasValue) ? ((TimeSpan?)null) : new TimeSpan?(DateTime.UtcNow - jEAPOHAGCLL2.GetValueOrDefault()));
			if (timeSpan2.HasValue && timeSpan2.GetValueOrDefault() >= GetNegotiationResult().GetTransportConnectTimeout())
			{
				HTTPManager.GetLogger().Warning("SignalR Connection", "OnHeartbeatUpdate - Transport failed to connect in the given time!");
				((IConnection)this).Error("Transport failed to connect in the given time!");
			}
		}
		DateTime? reconnectStart = reconnectStartedAt;
		if (reconnectStart.HasValue)
		{
			DateTime? jNPJOFDOAAG2 = reconnectStartedAt;
			TimeSpan? timeSpan3 = ((!jNPJOFDOAAG2.HasValue) ? ((TimeSpan?)null) : new TimeSpan?(DateTime.UtcNow - jNPJOFDOAAG2.GetValueOrDefault()));
			if (timeSpan3.HasValue && timeSpan3.GetValueOrDefault() >= GetNegotiationResult().GetDisconnectTimeout())
			{
				HTTPManager.GetLogger().Warning("SignalR Connection", "OnHeartbeatUpdate - Failed to reconnect in the given time!");
				Close();
			}
		}
	}

	private void InitOnStart()
	{
		set_State(ConnectionStates.Connected);
		reconnectStartedAt = null;
		transportConnectionStartedAt = null;
		lastPingSentAt = DateTime.UtcNow;
		lastMessageReceivedAt = DateTime.UtcNow;
		HTTPManager.GetHeartbeats().Subscribe(this);
	}

	private Hub FindHub(ulong messageId)
	{
		if (GetHubs() != null)
		{
			for (int i = 0; i < GetHubs().Length; i++)
			{
				if (((IHub)GetHubs()[i]).HasSentMessageId(messageId))
				{
					return GetHubs()[i];
				}
			}
		}
		return null;
	}

	private bool TryFallbackTransport()
	{
		if (GetState() == ConnectionStates.Connecting)
		{
			if (bufferedMessages != null)
			{
				bufferedMessages.Clear();
			}
			GetTransport().Stop();
			SetTransport(null);
			switch (nextProtocolToTry)
			{
			case SupportedProtocols.ServerSentEvents:
				SetTransport(new ServerSentEventsTransport(this));
				nextProtocolToTry = SupportedProtocols.HTTP;
				break;
			case SupportedProtocols.HTTP:
				SetTransport(new PollingTransport(this));
				nextProtocolToTry = SupportedProtocols.Unknown;
				break;
			case SupportedProtocols.Unknown:
				return false;
			}
			transportConnectionStartedAt = DateTime.UtcNow;
			GetTransport().Connect();
			if (pingRequest != null)
			{
				pingRequest.Abort();
			}
			return true;
		}
		return false;
	}

	private void Ping()
	{
		HTTPManager.GetLogger().Information("SignalR Connection", "Sending Ping request.");
		pingRequest = new HTTPRequest(((IConnection)this).BuildUri(SignalRRequestType.Ping), OnPingRequestFinished);
		pingRequest.SetConnectTimeout(PingInterval);
		pingRequest.Send();
		lastPingSentAt = DateTime.UtcNow;
	}

	private void OnPingRequestFinished(HTTPRequest request, HTTPResponse response)
	{
		pingRequest = null;
		string text = string.Empty;
		switch (request.GetState())
		{
		case HTTPRequestStates.Finished:
			if (response.GetIsSuccess())
			{
				string text2 = ((IConnection)this).ParseResponse(response.GetDataAsText());
				if (text2 != "pong")
				{
					text = "Wrong answer for ping request: " + text2;
				}
				else
				{
					HTTPManager.GetLogger().Information("SignalR Connection", "Pong received.");
				}
			}
			else
			{
				text = string.Format("Ping - Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText());
			}
			break;
		case HTTPRequestStates.Error:
			text = "Ping - Request Finished with Error! " + ((request.GetException() == null) ? "No Exception" : (request.GetException().Message + "\n" + request.GetException().StackTrace));
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			text = "Ping - Connection Timed Out!";
			break;
		case HTTPRequestStates.TimedOut:
			text = "Ping - Processing the request Timed Out!";
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			((IConnection)this).Error(text);
		}
	}
}
