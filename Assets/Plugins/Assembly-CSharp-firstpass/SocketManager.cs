using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

[DefaultMember("Item")]
public sealed class SocketManager : IHeartbeat, IManager
{
	public enum SocketManagerState
	{
		Initial = 0,
		Closed = 1,
		Opening = 2,
		Open = 3,
		Reconnecting = 4
	}

	public static ISocketJsonEncoder DefaultEncoder = new SocketIODefaultJsonEncoder();

	public const int MinProtocolVersion = 4;

	private SocketManagerState state;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SocketOptions options;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Uri uri;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HandshakeData handshake;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ITransport transport;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ulong requestCounter;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int reconnectAttempts;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ISocketJsonEncoder encoder;

	private int nextAckId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SocketManagerState previousState;

	private Dictionary<string, Socket> namespaces = new Dictionary<string, Socket>();

	private List<Socket> sockets = new List<Socket>();

	private List<Packet> offlinePackets;

	private DateTime lastHeartbeat = DateTime.MinValue;

	private DateTime lastPongReceived = DateTime.MinValue;

	private DateTime reconnectAt;

	private DateTime connectionStarted;

	public SocketManagerState CurrentState
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

	public SocketOptions Options
	{
		get
		{
			return GetOptions();
		}
		private set
		{
			SetOptions(value);
		}
	}

	public Uri ServerUri
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

	public HandshakeData HandshakeInfo
	{
		get
		{
			return GetHandshake();
		}
		private set
		{
			SetHandshake(value);
		}
	}

	public ITransport ActiveTransport
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

	public ulong RequestCount
	{
		get
		{
			return GetRequestCounter();
		}
		internal set
		{
			set_RequestCounter(value);
		}
	}

	public Socket RootSocket
	{
		get
		{
			return GetRootSocket();
		}
	}

	// C# has no syntax for parameterized property 'Item'.
	public Socket get_DLKPBAJDHBO(string namespaceName)
	{
		return get_Item(namespaceName);
	}

	public int ReconnectAttemptCount
	{
		get
		{
			return GetReconnectAttempts();
		}
		private set
		{
			set_ReconnectAttempts(value);
		}
	}

	public ISocketJsonEncoder Encoder
	{
		get
		{
			return GetEncoder();
		}
		set
		{
			SetEncoder(value);
		}
	}

	internal uint Timestamp
	{
		get
		{
			return GetTimestamp();
		}
	}

	internal int NextAckId
	{
		get
		{
			return GetNextAckId();
		}
	}

	internal SocketManagerState PreviousState
	{
		get
		{
			return GetPreviousState();
		}
		private set
		{
			SetPreviousState(value);
		}
	}

	public SocketManager(Uri uri)
		: this(uri, new SocketOptions())
	{
	}

	public SocketManager(Uri uri, SocketOptions options)
	{
		set_Uri(uri);
		SetOptions(options);
		set_State(SocketManagerState.Initial);
		SetPreviousState(SocketManagerState.Initial);
		SetEncoder(DefaultEncoder);
	}

	public SocketManagerState GetState()
	{
		return state;
	}

	private void set_State(SocketManagerState value)
	{
		SetPreviousState(state);
		state = value;
	}

	public SocketOptions GetOptions()
	{
		return options;
	}

	private void SetOptions(SocketOptions value)
	{
		options = value;
	}

	public Uri GetUri()
	{
		return uri;
	}

	private void set_Uri(Uri value)
	{
		uri = value;
	}

	public HandshakeData GetHandshake()
	{
		return handshake;
	}

	private void SetHandshake(HandshakeData value)
	{
		handshake = value;
	}

	public ITransport GetTransport()
	{
		return transport;
	}

	private void SetTransport(ITransport value)
	{
		transport = value;
	}

	public ulong GetRequestCounter()
	{
		return requestCounter;
	}

	internal void set_RequestCounter(ulong value)
	{
		requestCounter = value;
	}

	public Socket GetRootSocket()
	{
		return GetSocket();
	}

	public Socket get_Item(string namespaceName)
	{
		return GetSocket(namespaceName);
	}

	public int GetReconnectAttempts()
	{
		return reconnectAttempts;
	}

	private void set_ReconnectAttempts(int value)
	{
		reconnectAttempts = value;
	}

	public ISocketJsonEncoder GetEncoder()
	{
		return encoder;
	}

	public void SetEncoder(ISocketJsonEncoder value)
	{
		encoder = value;
	}

	internal uint GetTimestamp()
	{
		return (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
	}

	internal int GetNextAckId()
	{
		return Interlocked.Increment(ref nextAckId);
	}

	internal SocketManagerState GetPreviousState()
	{
		return previousState;
	}

	private void SetPreviousState(SocketManagerState value)
	{
		previousState = value;
	}

	public Socket GetSocket()
	{
		return GetSocket("/");
	}

	public Socket GetSocket(string namespaceName)
	{
		if (string.IsNullOrEmpty(namespaceName))
		{
			throw new ArgumentNullException("Namespace parameter is null or empty!");
		}
		Socket value = null;
		if (!namespaces.TryGetValue(namespaceName, out value))
		{
			value = new Socket(namespaceName, this);
			namespaces.Add(namespaceName, value);
			sockets.Add(value);
			((ISocket)value).Open();
		}
		return value;
	}

	void IManager.Remove(Socket socket)
	{
		namespaces.Remove(socket.GetNamespace());
		sockets.Remove(socket);
	}

	public void Open()
	{
		if (GetState() == SocketManagerState.Initial || GetState() == SocketManagerState.Closed || GetState() == SocketManagerState.Reconnecting)
		{
			HTTPManager.GetLogger().Information("SocketManager", "Opening");
			reconnectAt = DateTime.MinValue;
			SetHandshake(new HandshakeData(this));
			GetHandshake().OnReceived = (HandshakeData handshake) =>
			{
				OnHandshakeCallback();
			};
			GetHandshake().OnError = (HandshakeData handshake, string error) =>
			{
				((IManager)this).EmitError(SocketIOErrors.Internal, error);
				((IManager)this).TryToReconnect();
			};
			GetHandshake().Start();
			((IManager)this).EmitEvent("connecting", new object[0]);
			set_State(SocketManagerState.Opening);
			connectionStarted = DateTime.UtcNow;
			HTTPManager.GetHeartbeats().Subscribe(this);
			GetSocket("/");
		}
	}

	public void Close()
	{
		((IManager)this).Close(true);
	}

	void IManager.Close(bool removeSockets)
	{
		if (GetState() == SocketManagerState.Closed)
		{
			return;
		}
		HTTPManager.GetLogger().Information("SocketManager", "Closing");
		HTTPManager.GetHeartbeats().Unsubscribe(this);
		if (removeSockets)
		{
			while (sockets.Count > 0)
			{
				((ISocket)sockets[sockets.Count - 1]).Disconnect(removeSockets);
			}
		}
		else
		{
			for (int i = 0; i < sockets.Count; i++)
			{
				((ISocket)sockets[i]).Disconnect(removeSockets);
			}
		}
		set_State(SocketManagerState.Closed);
		lastHeartbeat = DateTime.MinValue;
		if (offlinePackets != null)
		{
			offlinePackets.Clear();
		}
		if (removeSockets)
		{
			namespaces.Clear();
		}
		if (GetHandshake() != null)
		{
			GetHandshake().Abort();
		}
		SetHandshake(null);
		if (GetTransport() != null)
		{
			GetTransport().Close();
		}
		SetTransport(null);
	}

	void IManager.TryToReconnect()
	{
		if (GetState() == SocketManagerState.Reconnecting || GetState() == SocketManagerState.Closed)
		{
			return;
		}
		if (!GetOptions().GetReconnection())
		{
			Close();
			return;
		}
		int num;
		set_ReconnectAttempts(num = GetReconnectAttempts() + 1);
		if (num >= GetOptions().GetReconnectionAttempts())
		{
			((IManager)this).EmitEvent("reconnect_failed", new object[0]);
			Close();
			return;
		}
		Random random = new Random();
		int num2 = (int)GetOptions().GetReconnectionDelay().TotalMilliseconds * GetReconnectAttempts();
		reconnectAt = DateTime.UtcNow + TimeSpan.FromMilliseconds(Math.Min(random.Next((int)((float)num2 - (float)num2 * GetOptions().GetRandomizationFactor()), (int)((float)num2 + (float)num2 * GetOptions().GetRandomizationFactor())), (int)GetOptions().GetReconnectionDelayMax().TotalMilliseconds));
		((IManager)this).Close(false);
		set_State(SocketManagerState.Reconnecting);
		for (int i = 0; i < sockets.Count; i++)
		{
			((ISocket)sockets[i]).Open();
		}
		HTTPManager.GetHeartbeats().Subscribe(this);
		HTTPManager.GetLogger().Information("SocketManager", "Reconnecting");
	}

	private void OnHandshakeCallback()
	{
		if (GetHandshake().GetUpgrades().Contains("websocket"))
		{
			SetTransport(new SocketIoWebSocketTransport(this));
		}
		else
		{
			SetTransport(new SocketIOPollingTransport(this));
		}
		GetTransport().OpenTransport();
	}

	bool IManager.OnTransportConnected(ITransport transport)
	{
		if (GetState() != SocketManagerState.Opening)
		{
			return false;
		}
		if (GetPreviousState() == SocketManagerState.Reconnecting)
		{
			((IManager)this).EmitEvent("reconnect", new object[0]);
		}
		set_State(SocketManagerState.Open);
		lastPongReceived = DateTime.UtcNow;
		set_ReconnectAttempts(0);
		SendOfflinePackets();
		HTTPManager.GetLogger().Information("SocketManager", "Open");
		return true;
	}

	void IManager.OnTransportError(ITransport transport, string error)
	{
		((IManager)this).EmitError(SocketIOErrors.Internal, error);
		if (transport.GetState() == SocketIOTransportState.Connecting || transport.GetState() == SocketIOTransportState.Opening)
		{
			if (transport is SocketIoWebSocketTransport)
			{
				transport.Close();
				SetTransport(new SocketIOPollingTransport(this));
				GetTransport().OpenTransport();
			}
			else
			{
				((IManager)this).TryToReconnect();
			}
		}
		else
		{
			transport.Close();
			((IManager)this).TryToReconnect();
		}
	}

	private ITransport SelectTransport()
	{
		if (GetState() != SocketManagerState.Open)
		{
			return null;
		}
		return (!GetTransport().GetIsRequestInProgress()) ? GetTransport() : null;
	}

	private void SendOfflinePackets()
	{
		ITransport transport = SelectTransport();
		if (offlinePackets != null && offlinePackets.Count > 0 && transport != null)
		{
			transport.Send(offlinePackets);
			offlinePackets.Clear();
		}
	}

	void IManager.SendPacket(Packet packet)
	{
		ITransport transport = SelectTransport();
		if (transport != null)
		{
			try
			{
				transport.Send(packet);
				return;
			}
			catch (Exception ex)
			{
				((IManager)this).EmitError(SocketIOErrors.Internal, ex.Message + " " + ex.StackTrace);
				return;
			}
		}
		if (offlinePackets == null)
		{
			offlinePackets = new List<Packet>();
		}
		offlinePackets.Add(packet.Clone());
	}

	void IManager.OnPacket(Packet packet)
	{
		if (GetState() != SocketManagerState.Closed)
		{
			switch (packet.GetTransportEvent())
			{
			case TransportEventTypes.Ping:
				((IManager)this).SendPacket(new Packet(TransportEventTypes.Pong, SocketIOEventType.Unknown, "/", string.Empty));
				break;
			case TransportEventTypes.Pong:
				lastPongReceived = DateTime.UtcNow;
				break;
			}
			Socket value = null;
			if (namespaces.TryGetValue(packet.GetNamespace(), out value))
			{
				((ISocket)value).OnPacket(packet);
			}
			else
			{
				HTTPManager.GetLogger().Warning("SocketManager", "Namespace \"" + packet.GetNamespace() + "\" not found!");
			}
		}
	}

	public void EmitAll(string eventName, params object[] args)
	{
		for (int i = 0; i < sockets.Count; i++)
		{
			sockets[i].Emit(eventName, args);
		}
	}

	void IManager.EmitEvent(string eventName, params object[] args)
	{
		Socket value = null;
		if (namespaces.TryGetValue("/", out value))
		{
			((ISocket)value).EmitEvent(eventName, args);
		}
	}

	void IManager.EmitEvent(SocketIOEventType eventType, params object[] args)
	{
		((IManager)this).EmitEvent(EventNames.GetNameFor(eventType), args);
	}

	void IManager.EmitError(SocketIOErrors errorType, string errorMessage)
	{
		((IManager)this).EmitEvent(SocketIOEventType.Error, new object[1]
		{
			new Error(errorType, errorMessage)
		});
	}

	void IManager.EmitAll(string eventName, params object[] args)
	{
		for (int i = 0; i < sockets.Count; i++)
		{
			((ISocket)sockets[i]).EmitEvent(eventName, args);
		}
	}

	void IHeartbeat.OnHeartbeatUpdate(TimeSpan delta)
	{
		switch (GetState())
		{
		case SocketManagerState.Opening:
			if (DateTime.UtcNow - connectionStarted >= GetOptions().GetTimeout())
			{
				((IManager)this).EmitEvent("connect_error", new object[0]);
				((IManager)this).EmitEvent("connect_timeout", new object[0]);
				((IManager)this).TryToReconnect();
			}
			break;
		case SocketManagerState.Reconnecting:
			if (reconnectAt != DateTime.MinValue && DateTime.UtcNow >= reconnectAt)
			{
				((IManager)this).EmitEvent("reconnect_attempt", new object[0]);
				((IManager)this).EmitEvent("reconnecting", new object[0]);
				Open();
			}
			break;
		case SocketManagerState.Open:
		{
			ITransport transport = null;
			if (GetTransport() != null && GetTransport().GetState() == SocketIOTransportState.Open)
			{
				transport = GetTransport();
			}
			if (transport == null || transport.GetState() != SocketIOTransportState.Open)
			{
				break;
			}
			transport.Poll();
			SendOfflinePackets();
			if (lastHeartbeat == DateTime.MinValue)
			{
				lastHeartbeat = DateTime.UtcNow;
				break;
			}
			if (DateTime.UtcNow - lastHeartbeat > GetHandshake().GetPingInterval())
			{
				((IManager)this).SendPacket(new Packet(TransportEventTypes.Ping, SocketIOEventType.Unknown, "/", string.Empty));
				lastHeartbeat = DateTime.UtcNow;
			}
			if (DateTime.UtcNow - lastPongReceived > GetHandshake().GetPingTimeout())
			{
				((IManager)this).TryToReconnect();
			}
			break;
		}
		}
	}
}
