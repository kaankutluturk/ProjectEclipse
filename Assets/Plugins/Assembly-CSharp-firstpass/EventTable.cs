using System.Collections.Generic;
using System.Diagnostics;

internal sealed class EventTable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Socket socket;

	private Dictionary<string, List<EventDescriptor>> Table = new Dictionary<string, List<EventDescriptor>>();

	private Socket OwnerSocket
	{
		get
		{
			return GetOwnerSocket();
		}
		set
		{
			SetOwnerSocket(value);
		}
	}

	public EventTable(Socket JLEACANCMJF)
	{
		SetOwnerSocket(JLEACANCMJF);
	}

	private Socket GetOwnerSocket()
	{
		return socket;
	}

	private void SetOwnerSocket(Socket value)
	{
		socket = value;
	}

	public void Register(string DOPHKKGNAEF, SocketIOCallback callback, bool ONOLLCMDGBO, bool EJDLINOJJIF)
	{
		List<EventDescriptor> value;
		if (!Table.TryGetValue(DOPHKKGNAEF, out value))
		{
			Table.Add(DOPHKKGNAEF, value = new List<EventDescriptor>(1));
		}
		EventDescriptor lBIMLJMCENN = value.Find((EventDescriptor d) => d.GetOnlyOnce() == ONOLLCMDGBO && d.GetAutoDecodePayload() == EJDLINOJJIF);
		if (lBIMLJMCENN == null)
		{
			value.Add(new EventDescriptor(ONOLLCMDGBO, EJDLINOJJIF, callback));
		}
		else
		{
			lBIMLJMCENN.GetCallbacks().Add(callback);
		}
	}

	public void Unregister(string DOPHKKGNAEF)
	{
		Table.Remove(DOPHKKGNAEF);
	}

	public void Unregister(string DOPHKKGNAEF, SocketIOCallback callback)
	{
		List<EventDescriptor> value;
		if (Table.TryGetValue(DOPHKKGNAEF, out value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i].GetCallbacks().Remove(callback);
			}
		}
	}

	public void Call(string DOPHKKGNAEF, Packet NPKADBPBKIG, params object[] LKIOKGCNKHE)
	{
		if (HTTPManager.GetLogger().GetLevel() <= Loglevels.All)
		{
			HTTPManager.GetLogger().Verbose("EventTable", "Call - " + DOPHKKGNAEF);
		}
		List<EventDescriptor> value;
		if (Table.TryGetValue(DOPHKKGNAEF, out value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i].Call(GetOwnerSocket(), NPKADBPBKIG, LKIOKGCNKHE);
			}
		}
	}

	public void Call(Packet NPKADBPBKIG)
	{
		string text = NPKADBPBKIG.DecodeEventName();
		string text2 = ((NPKADBPBKIG.GetSocketIOEvent() == SocketIOEventType.Unknown) ? EventNames.GetNameFor(NPKADBPBKIG.GetTransportEvent()) : EventNames.GetNameFor(NPKADBPBKIG.GetSocketIOEvent()));
		object[] lKIOKGCNKHE = null;
		if (HasSubscriber(text) || HasSubscriber(text2))
		{
			if (NPKADBPBKIG.GetTransportEvent() == TransportEventTypes.Message && (NPKADBPBKIG.GetSocketIOEvent() == SocketIOEventType.Event || NPKADBPBKIG.GetSocketIOEvent() == SocketIOEventType.BinaryEvent) && IsAutoDecode(text))
			{
				lKIOKGCNKHE = NPKADBPBKIG.Decode(GetOwnerSocket().GetManager().GetEncoder());
			}
			if (!string.IsNullOrEmpty(text))
			{
				Call(text, NPKADBPBKIG, lKIOKGCNKHE);
			}
			if (!NPKADBPBKIG.GetIsDecoded() && IsAutoDecode(text2))
			{
				lKIOKGCNKHE = NPKADBPBKIG.Decode(GetOwnerSocket().GetManager().GetEncoder());
			}
			if (!string.IsNullOrEmpty(text2))
			{
				Call(text2, NPKADBPBKIG, lKIOKGCNKHE);
			}
		}
	}

	public void Clear()
	{
		Table.Clear();
	}

	private bool IsAutoDecode(string DOPHKKGNAEF)
	{
		List<EventDescriptor> value;
		if (Table.TryGetValue(DOPHKKGNAEF, out value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].GetAutoDecodePayload() && value[i].GetCallbacks().Count > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasSubscriber(string DOPHKKGNAEF)
	{
		return Table.ContainsKey(DOPHKKGNAEF);
	}
}
