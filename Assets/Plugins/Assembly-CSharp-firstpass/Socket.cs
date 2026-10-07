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

	internal Socket(string JBALIKEKHGL, SocketManager BJGMPDIKEJC)
	{
		set_Namespace(JBALIKEKHGL);
		SetManager(BJGMPDIKEJC);
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

	void ISocket.Disconnect(bool GGONLJPAABO)
	{
		if (GetIsOpen())
		{
			Packet nPKADBPBKIG = new Packet(TransportEventTypes.Message, SocketIOEventType.Disconnect, GetNamespace(), string.Empty);
			((IManager)GetManager()).SendPacket(nPKADBPBKIG);
			SetIsOpen(false);
			((ISocket)this).OnPacket(nPKADBPBKIG);
		}
		if (ackCallbacks != null)
		{
			ackCallbacks.Clear();
		}
		if (GGONLJPAABO)
		{
			eventCallbacks.Clear();
			((IManager)GetManager()).Remove(this);
		}
	}

	public Socket Emit(string DOPHKKGNAEF, params object[] LKIOKGCNKHE)
	{
		return Emit(DOPHKKGNAEF, null, LKIOKGCNKHE);
	}

	public Socket Emit(string DOPHKKGNAEF, SocketIOAckCallback callback, params object[] LKIOKGCNKHE)
	{
		if (EventNames.IsBlacklisted(DOPHKKGNAEF))
		{
			throw new ArgumentException("Blacklisted event: " + DOPHKKGNAEF);
		}
		arguments.Clear();
		arguments.Add(DOPHKKGNAEF);
		List<byte[]> list = null;
		if (LKIOKGCNKHE != null && LKIOKGCNKHE.Length > 0)
		{
			int num = 0;
			for (int i = 0; i < LKIOKGCNKHE.Length; i++)
			{
				byte[] array = LKIOKGCNKHE[i] as byte[];
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
					arguments.Add(LKIOKGCNKHE[i]);
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
		Packet cMPKPLIGKLC = new Packet(TransportEventTypes.Message, (list != null) ? SocketIOEventType.BinaryEvent : SocketIOEventType.Event, GetNamespace(), text, 0, num2);
		if (list != null)
		{
			cMPKPLIGKLC.set_Attachments(list);
		}
		((IManager)GetManager()).SendPacket(cMPKPLIGKLC);
		return this;
	}

	public Socket EmitAck(Packet FMAMCLDBKFM, params object[] LKIOKGCNKHE)
	{
		if (FMAMCLDBKFM == null)
		{
			throw new ArgumentNullException("originalPacket == null!");
		}
		if (FMAMCLDBKFM.GetSocketIOEvent() != SocketIOEventType.Event && FMAMCLDBKFM.GetSocketIOEvent() != SocketIOEventType.BinaryEvent)
		{
			throw new ArgumentException("Wrong packet - you can't send an Ack for a packet with id == 0 and SocketIOEvent != Event or SocketIOEvent != BinaryEvent!");
		}
		arguments.Clear();
		if (LKIOKGCNKHE != null && LKIOKGCNKHE.Length > 0)
		{
			arguments.AddRange(LKIOKGCNKHE);
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
		Packet nPKADBPBKIG = new Packet(TransportEventTypes.Message, (FMAMCLDBKFM.GetSocketIOEvent() != SocketIOEventType.Event) ? SocketIOEventType.BinaryAck : SocketIOEventType.Ack, GetNamespace(), text, 0, FMAMCLDBKFM.GetPacketId());
		((IManager)GetManager()).SendPacket(nPKADBPBKIG);
		return this;
	}

	public void On(string DOPHKKGNAEF, SocketIOCallback callback)
	{
		eventCallbacks.Register(DOPHKKGNAEF, callback, false, GetAutoDecodePayload());
	}

	public void On(SocketIOEventType LFLGCDNKNJI, SocketIOCallback callback)
	{
		string dOPHKKGNAEF = EventNames.GetNameFor(LFLGCDNKNJI);
		eventCallbacks.Register(dOPHKKGNAEF, callback, false, GetAutoDecodePayload());
	}

	public void On(string DOPHKKGNAEF, SocketIOCallback callback, bool EJDLINOJJIF)
	{
		eventCallbacks.Register(DOPHKKGNAEF, callback, false, EJDLINOJJIF);
	}

	public void On(SocketIOEventType LFLGCDNKNJI, SocketIOCallback callback, bool EJDLINOJJIF)
	{
		string dOPHKKGNAEF = EventNames.GetNameFor(LFLGCDNKNJI);
		eventCallbacks.Register(dOPHKKGNAEF, callback, false, EJDLINOJJIF);
	}

	public void Once(string DOPHKKGNAEF, SocketIOCallback callback)
	{
		eventCallbacks.Register(DOPHKKGNAEF, callback, true, GetAutoDecodePayload());
	}

	public void Once(SocketIOEventType LFLGCDNKNJI, SocketIOCallback callback)
	{
		eventCallbacks.Register(EventNames.GetNameFor(LFLGCDNKNJI), callback, true, GetAutoDecodePayload());
	}

	public void Once(string DOPHKKGNAEF, SocketIOCallback callback, bool EJDLINOJJIF)
	{
		eventCallbacks.Register(DOPHKKGNAEF, callback, true, EJDLINOJJIF);
	}

	public void Once(SocketIOEventType LFLGCDNKNJI, SocketIOCallback callback, bool EJDLINOJJIF)
	{
		eventCallbacks.Register(EventNames.GetNameFor(LFLGCDNKNJI), callback, true, EJDLINOJJIF);
	}

	public void Off()
	{
		eventCallbacks.Clear();
	}

	public void Off(string DOPHKKGNAEF)
	{
		eventCallbacks.Unregister(DOPHKKGNAEF);
	}

	public void Off(SocketIOEventType LFLGCDNKNJI)
	{
		Off(EventNames.GetNameFor(LFLGCDNKNJI));
	}

	public void Off(string DOPHKKGNAEF, SocketIOCallback callback)
	{
		eventCallbacks.Unregister(DOPHKKGNAEF, callback);
	}

	public void Off(SocketIOEventType LFLGCDNKNJI, SocketIOCallback callback)
	{
		eventCallbacks.Unregister(EventNames.GetNameFor(LFLGCDNKNJI), callback);
	}

	void ISocket.OnPacket(Packet NPKADBPBKIG)
	{
		switch (NPKADBPBKIG.GetSocketIOEvent())
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
			bool IBFAPIMOMBA = false;
			Dictionary<string, object> dictionary = Json.Decode(NPKADBPBKIG.GetPayload(), ref IBFAPIMOMBA) as Dictionary<string, object>;
			if (IBFAPIMOMBA)
			{
				Error eOFKDCNBPHO = new Error((SocketIOErrors)Convert.ToInt32(dictionary["code"]), dictionary["message"] as string);
				eventCallbacks.Call(EventNames.GetNameFor(SocketIOEventType.Error), NPKADBPBKIG, eOFKDCNBPHO);
				return;
			}
			break;
		}
		}
		eventCallbacks.Call(NPKADBPBKIG);
		if ((NPKADBPBKIG.GetSocketIOEvent() != SocketIOEventType.Ack && NPKADBPBKIG.GetSocketIOEvent() != SocketIOEventType.BinaryAck) || ackCallbacks == null)
		{
			return;
		}
		SocketIOAckCallback value = null;
		if (ackCallbacks.TryGetValue(NPKADBPBKIG.GetPacketId(), out value) && value != null)
		{
			try
			{
				value(this, NPKADBPBKIG, NPKADBPBKIG.Decode(GetManager().GetEncoder()));
			}
			catch (Exception mPFFFAOGBJE)
			{
				HTTPManager.GetLogger().Exception("Socket", "ackCallback", mPFFFAOGBJE);
			}
		}
		ackCallbacks.Remove(NPKADBPBKIG.GetPacketId());
	}

	void ISocket.EmitEvent(SocketIOEventType LFLGCDNKNJI, params object[] LKIOKGCNKHE)
	{
		((ISocket)this).EmitEvent(EventNames.GetNameFor(LFLGCDNKNJI), LKIOKGCNKHE);
	}

	void ISocket.EmitEvent(string DOPHKKGNAEF, params object[] LKIOKGCNKHE)
	{
		if (!string.IsNullOrEmpty(DOPHKKGNAEF))
		{
			eventCallbacks.Call(DOPHKKGNAEF, null, LKIOKGCNKHE);
		}
	}

	void ISocket.EmitError(SocketIOErrors GNKCGOGKAEK, string CKEHOEGLMBM)
	{
		((ISocket)this).EmitEvent(SocketIOEventType.Error, new object[1]
		{
			new Error(GNKCGOGKAEK, CKEHOEGLMBM)
		});
	}

	private void OnTransportOpen(Socket JLEACANCMJF, Packet NPKADBPBKIG, params object[] LKIOKGCNKHE)
	{
		if (GetNamespace() != "/")
		{
			((IManager)GetManager()).SendPacket(new Packet(TransportEventTypes.Message, SocketIOEventType.Connect, GetNamespace(), string.Empty));
		}
		SetIsOpen(true);
	}
}
