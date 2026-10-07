using System;

public static class EventNames
{
	public const string Connect = "connect";

	public const string Disconnect = "disconnect";

	public const string Event = "event";

	public const string Ack = "ack";

	public const string Error = "error";

	public const string BinaryEvent = "binaryevent";

	public const string BinaryAck = "binaryack";

	private static string[] SocketIONames = new string[8] { "unknown", "connect", "disconnect", "event", "ack", "error", "binaryevent", "binaryack" };

	private static string[] TransportNames = new string[8] { "unknown", "open", "close", "ping", "pong", "message", "upgrade", "noop" };

	private static string[] BlacklistedEvents = new string[10] { "connect", "connect_error", "connect_timeout", "disconnect", "error", "reconnect", "reconnect_attempt", "reconnect_failed", "reconnect_error", "reconnecting" };

	public static string GetNameFor(SocketIOEventType LFLGCDNKNJI)
	{
		return SocketIONames[(int)(LFLGCDNKNJI + 1)];
	}

	public static string GetNameFor(TransportEventTypes AJONKGOAHJH)
	{
		return TransportNames[(int)(AJONKGOAHJH + 1)];
	}

	public static bool IsBlacklisted(string DOPHKKGNAEF)
	{
		for (int i = 0; i < BlacklistedEvents.Length; i++)
		{
			if (string.Compare(BlacklistedEvents[i], DOPHKKGNAEF, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
		}
		return false;
	}
}
