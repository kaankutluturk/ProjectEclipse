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

	public EventTable(Socket socket)
	{
		SetOwnerSocket(socket);
	}

	private Socket GetOwnerSocket()
	{
		return socket;
	}

	private void SetOwnerSocket(Socket value)
	{
		socket = value;
	}

	public void Register(string eventName, SocketIOCallback callback, bool onlyOnce, bool autoDecodePayload)
	{
		List<EventDescriptor> value;
		if (!Table.TryGetValue(eventName, out value))
		{
			Table.Add(eventName, value = new List<EventDescriptor>(1));
		}
		EventDescriptor descriptor = value.Find((EventDescriptor d) => d.GetOnlyOnce() == onlyOnce && d.GetAutoDecodePayload() == autoDecodePayload);
		if (descriptor == null)
		{
			value.Add(new EventDescriptor(onlyOnce, autoDecodePayload, callback));
		}
		else
		{
			descriptor.GetCallbacks().Add(callback);
		}
	}

	public void Unregister(string eventName)
	{
		Table.Remove(eventName);
	}

	public void Unregister(string eventName, SocketIOCallback callback)
	{
		List<EventDescriptor> value;
		if (Table.TryGetValue(eventName, out value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i].GetCallbacks().Remove(callback);
			}
		}
	}

	public void Call(string eventName, Packet packet, params object[] args)
	{
		if (HTTPManager.GetLogger().GetLevel() <= Loglevels.All)
		{
			HTTPManager.GetLogger().Verbose("EventTable", "Call - " + eventName);
		}
		List<EventDescriptor> value;
		if (Table.TryGetValue(eventName, out value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i].Call(GetOwnerSocket(), packet, args);
			}
		}
	}

	public void Call(Packet packet)
	{
		string text = packet.DecodeEventName();
		string text2 = ((packet.GetSocketIOEvent() == SocketIOEventType.Unknown) ? EventNames.GetNameFor(packet.GetTransportEvent()) : EventNames.GetNameFor(packet.GetSocketIOEvent()));
		object[] args = null;
		if (HasSubscriber(text) || HasSubscriber(text2))
		{
			if (packet.GetTransportEvent() == TransportEventTypes.Message && (packet.GetSocketIOEvent() == SocketIOEventType.Event || packet.GetSocketIOEvent() == SocketIOEventType.BinaryEvent) && IsAutoDecode(text))
			{
				args = packet.Decode(GetOwnerSocket().GetManager().GetEncoder());
			}
			if (!string.IsNullOrEmpty(text))
			{
				Call(text, packet, args);
			}
			if (!packet.GetIsDecoded() && IsAutoDecode(text2))
			{
				args = packet.Decode(GetOwnerSocket().GetManager().GetEncoder());
			}
			if (!string.IsNullOrEmpty(text2))
			{
				Call(text2, packet, args);
			}
		}
	}

	public void Clear()
	{
		Table.Clear();
	}

	private bool IsAutoDecode(string eventName)
	{
		List<EventDescriptor> value;
		if (Table.TryGetValue(eventName, out value))
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

	private bool HasSubscriber(string eventName)
	{
		return Table.ContainsKey(eventName);
	}
}
