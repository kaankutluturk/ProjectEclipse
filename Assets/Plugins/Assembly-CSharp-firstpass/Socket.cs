using System;
using System.Collections.Generic;
using System.Diagnostics;

public sealed class Socket : ISocket
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SocketManager manager;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string namespaceName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isOpen;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool autoDecodePayload;

	private Dictionary<int, SocketIOAckCallback> ackCallbacks;

	private EventTable eventCallbacks;

	private List<object> arguments = new List<object>();

	public SocketManager OwnerManager
	{
		get
		{
			return GetManager();
		}
		private set
		{
			SetManager(value);
		}
	}

	public bool IsOpen
	{
		get
		{
			return GetIsOpen();
		}
		private set
		{
			SetIsOpen(value);
		}
	}

	public bool AutoDecodePayload
	{
		get
		{
			return GetAutoDecodePayload();
		}
		set
		{
			SetAutoDecodePayload(value);
		}
	}

	internal Socket(string namespaceName, SocketManager socketManager)
	{
		set_Namespace(namespaceName);
		SetManager(socketManager);
		SetIsOpen(false);
		SetAutoDecodePayload(true);
		eventCallbacks = new EventTable(this);
	}

	public SocketManager GetManager()
	{
		return manager;
	}

	private void SetManager(SocketManager value)
	{
		manager = value;
	}

	public string GetNamespace()
	{
		return namespaceName;
	}

	private void set_Namespace(string value)
	{
		namespaceName = value;
	}

	public bool GetIsOpen()
	{
		return isOpen;
	}

	private void SetIsOpen(bool value)
	{
		isOpen = value;
	}

	public bool GetAutoDecodePayload()
	{
		return autoDecodePayload;
	}

	public void SetAutoDecodePayload(bool value)
	{
		autoDecodePayload = value;
	}

	void ISocket.Open()
	{
		if (GetManager().GetState() == SocketManager.SocketManagerState.Open)
		{
			OnTransportOpen(GetManager().GetRootSocket(), null);
			return;
		}
		GetManager().GetRootSocket().Off("connect", OnTransportOpen);
		GetManager().GetRootSocket().On("connect", OnTransportOpen);
		if (GetManager().GetOptions().GetAutoConnect() && GetManager().GetState() == SocketManager.SocketManagerState.Initial)
		{
			GetManager().Open();
		}
	}

	public void Disconnect()
	{
		((ISocket)this).Disconnect(true);
	}

	void ISocket.Disconnect(bool removeFromManager)
	{
		if (GetIsOpen())
		{
			Packet packet = new Packet(TransportEventTypes.Message, SocketIOEventType.Disconnect, GetNamespace(), string.Empty);
			((IManager)GetManager()).SendPacket(packet);
			SetIsOpen(false);
			((ISocket)this).OnPacket(packet);
		}
		if (ackCallbacks != null)
		{
			ackCallbacks.Clear();
		}
		if (removeFromManager)
		{
			eventCallbacks.Clear();
			((IManager)GetManager()).Remove(this);
		}
	}

	public Socket Emit(string eventName, params object[] args)
	{
		return Emit(eventName, null, args);
	}

	public Socket Emit(string eventName, SocketIOAckCallback callback, params object[] args)
	{
		if (EventNames.IsBlacklisted(eventName))
		{
			throw new ArgumentException("Blacklisted event: " + eventName);
		}
		arguments.Clear();
		arguments.Add(eventName);
		List<byte[]> list = null;
		if (args != null && args.Length > 0)
		{
			int num = 0;
			for (int i = 0; i < args.Length; i++)
			{
				byte[] array = args[i] as byte[];
				if (array != null)
				{
					if (list == null)
					{
						list = new List<byte[]>();
					}
					arguments.Add(string.Format("{{\"_placeholder\":true,\"num\":{0}}}", num++.ToString()));
					list.Add(array);
				}
				else
				{
					arguments.Add(args[i]);
				}
			}
		}
		string text = null;
		try
		{
			text = GetManager().GetEncoder().Encode(arguments);
		}
		catch (Exception ex)
		{
			((ISocket)this).EmitError(SocketIOErrors.Internal, "Error while encoding payload: " + ex.Message + " " + ex.StackTrace);
			return this;
		}
		arguments.Clear();
		if (text == null)
		{
			throw new ArgumentException("Encoding the arguments to JSON failed!");
		}
		int num2 = 0;
		if (callback != null)
		{
			num2 = GetManager().GetNextAckId();
			if (ackCallbacks == null)
			{
				ackCallbacks = new Dictionary<int, SocketIOAckCallback>();
			}
			ackCallbacks[num2] = callback;
		}
		Packet packet = new Packet(TransportEventTypes.Message, (list != null) ? SocketIOEventType.BinaryEvent : SocketIOEventType.Event, GetNamespace(), text, 0, num2);
		if (list != null)
		{
			packet.set_Attachments(list);
		}
		((IManager)GetManager()).SendPacket(packet);
		return this;
	}

	public Socket EmitAck(Packet originalPacket, params object[] args)
	{
		if (originalPacket == null)
		{
			throw new ArgumentNullException("originalPacket == null!");
		}
		if (originalPacket.GetSocketIOEvent() != SocketIOEventType.Event && originalPacket.GetSocketIOEvent() != SocketIOEventType.BinaryEvent)
		{
			throw new ArgumentException("Wrong packet - you can't send an Ack for a packet with id == 0 and SocketIOEvent != Event or SocketIOEvent != BinaryEvent!");
		}
		arguments.Clear();
		if (args != null && args.Length > 0)
		{
			arguments.AddRange(args);
		}
		string text = null;
		try
		{
			text = GetManager().GetEncoder().Encode(arguments);
		}
		catch (Exception ex)
		{
			((ISocket)this).EmitError(SocketIOErrors.Internal, "Error while encoding payload: " + ex.Message + " " + ex.StackTrace);
			return this;
		}
		if (text == null)
		{
			throw new ArgumentException("Encoding the arguments to JSON failed!");
		}
		Packet packet = new Packet(TransportEventTypes.Message, (originalPacket.GetSocketIOEvent() != SocketIOEventType.Event) ? SocketIOEventType.BinaryAck : SocketIOEventType.Ack, GetNamespace(), text, 0, originalPacket.GetPacketId());
		((IManager)GetManager()).SendPacket(packet);
		return this;
	}

	public void On(string eventName, SocketIOCallback callback)
	{
		eventCallbacks.Register(eventName, callback, false, GetAutoDecodePayload());
	}

	public void On(SocketIOEventType eventType, SocketIOCallback callback)
	{
		string eventName = EventNames.GetNameFor(eventType);
		eventCallbacks.Register(eventName, callback, false, GetAutoDecodePayload());
	}

	public void On(string eventName, SocketIOCallback callback, bool autoDecodePayload)
	{
		eventCallbacks.Register(eventName, callback, false, autoDecodePayload);
	}

	public void On(SocketIOEventType eventType, SocketIOCallback callback, bool autoDecodePayload)
	{
		string eventName = EventNames.GetNameFor(eventType);
		eventCallbacks.Register(eventName, callback, false, autoDecodePayload);
	}

	public void Once(string eventName, SocketIOCallback callback)
	{
		eventCallbacks.Register(eventName, callback, true, GetAutoDecodePayload());
	}

	public void Once(SocketIOEventType eventType, SocketIOCallback callback)
	{
		eventCallbacks.Register(EventNames.GetNameFor(eventType), callback, true, GetAutoDecodePayload());
	}

	public void Once(string eventName, SocketIOCallback callback, bool autoDecodePayload)
	{
		eventCallbacks.Register(eventName, callback, true, autoDecodePayload);
	}

	public void Once(SocketIOEventType eventType, SocketIOCallback callback, bool autoDecodePayload)
	{
		eventCallbacks.Register(EventNames.GetNameFor(eventType), callback, true, autoDecodePayload);
	}

	public void Off()
	{
		eventCallbacks.Clear();
	}

	public void Off(string eventName)
	{
		eventCallbacks.Unregister(eventName);
	}

	public void Off(SocketIOEventType eventType)
	{
		Off(EventNames.GetNameFor(eventType));
	}

	public void Off(string eventName, SocketIOCallback callback)
	{
		eventCallbacks.Unregister(eventName, callback);
	}

	public void Off(SocketIOEventType eventType, SocketIOCallback callback)
	{
		eventCallbacks.Unregister(EventNames.GetNameFor(eventType), callback);
	}

	void ISocket.OnPacket(Packet packet)
	{
		switch (packet.GetSocketIOEvent())
		{
		case SocketIOEventType.Disconnect:
			if (GetIsOpen())
			{
				SetIsOpen(false);
				Disconnect();
			}
			break;
		case SocketIOEventType.Error:
		{
			bool isValidJson = false;
			Dictionary<string, object> dictionary = Json.Decode(packet.GetPayload(), ref isValidJson) as Dictionary<string, object>;
			if (isValidJson)
			{
				Error error = new Error((SocketIOErrors)Convert.ToInt32(dictionary["code"]), dictionary["message"] as string);
				eventCallbacks.Call(EventNames.GetNameFor(SocketIOEventType.Error), packet, error);
				return;
			}
			break;
		}
		}
		eventCallbacks.Call(packet);
		if ((packet.GetSocketIOEvent() != SocketIOEventType.Ack && packet.GetSocketIOEvent() != SocketIOEventType.BinaryAck) || ackCallbacks == null)
		{
			return;
		}
		SocketIOAckCallback value = null;
		if (ackCallbacks.TryGetValue(packet.GetPacketId(), out value) && value != null)
		{
			try
			{
				value(this, packet, packet.Decode(GetManager().GetEncoder()));
			}
			catch (Exception exception)
			{
				HTTPManager.GetLogger().Exception("Socket", "ackCallback", exception);
			}
		}
		ackCallbacks.Remove(packet.GetPacketId());
	}

	void ISocket.EmitEvent(SocketIOEventType eventType, params object[] args)
	{
		((ISocket)this).EmitEvent(EventNames.GetNameFor(eventType), args);
	}

	void ISocket.EmitEvent(string eventName, params object[] args)
	{
		if (!string.IsNullOrEmpty(eventName))
		{
			eventCallbacks.Call(eventName, null, args);
		}
	}

	void ISocket.EmitError(SocketIOErrors errorType, string errorMessage)
	{
		((ISocket)this).EmitEvent(SocketIOEventType.Error, new object[1]
		{
			new Error(errorType, errorMessage)
		});
	}

	private void OnTransportOpen(Socket socket, Packet packet, params object[] args)
	{
		if (GetNamespace() != "/")
		{
			((IManager)GetManager()).SendPacket(new Packet(TransportEventTypes.Message, SocketIOEventType.Connect, GetNamespace(), string.Empty));
		}
		SetIsOpen(true);
	}
}
