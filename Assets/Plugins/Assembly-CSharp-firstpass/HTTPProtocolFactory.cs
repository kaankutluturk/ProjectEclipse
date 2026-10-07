using System;
using System.IO;

internal static class HTTPProtocolFactory
{
	public static HTTPResponse Get(SupportedProtocols ENLHAIGCCBO, HTTPRequest ONOCIELLAPL, Stream ABJIEFMMIEK, bool IBIIADCLKCH, bool PEAJIKCANHP)
	{
		switch (ENLHAIGCCBO)
		{
		case SupportedProtocols.WebSocket:
			return new WebSocketResponse(ONOCIELLAPL, ABJIEFMMIEK, IBIIADCLKCH, PEAJIKCANHP);
		case SupportedProtocols.ServerSentEvents:
			return new EventSourceResponse(ONOCIELLAPL, ABJIEFMMIEK, IBIIADCLKCH, PEAJIKCANHP);
		default:
			return new HTTPResponse(ONOCIELLAPL, ABJIEFMMIEK, IBIIADCLKCH, PEAJIKCANHP);
		}
	}

	public static SupportedProtocols GetProtocolFromUri(Uri KJHNCLAJMLO)
	{
		switch (KJHNCLAJMLO.Scheme.ToLowerInvariant())
		{
		case "ws":
		case "wss":
			return SupportedProtocols.WebSocket;
		default:
			return SupportedProtocols.HTTP;
		}
	}

	public static bool IsSecureProtocol(Uri KJHNCLAJMLO)
	{
		switch (KJHNCLAJMLO.Scheme.ToLowerInvariant())
		{
		case "https":
		case "wss":
			return true;
		default:
			return false;
		}
	}
}
