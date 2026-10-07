using System;
using System.IO;

internal static class HTTPProtocolFactory
{
	public static HTTPResponse Get(SupportedProtocols protocol, HTTPRequest request, Stream stream, bool isStreamed, bool isFromCache)
	{
		switch (protocol)
		{
		case SupportedProtocols.WebSocket:
			return new WebSocketResponse(request, stream, isStreamed, isFromCache);
		case SupportedProtocols.ServerSentEvents:
			return new EventSourceResponse(request, stream, isStreamed, isFromCache);
		default:
			return new HTTPResponse(request, stream, isStreamed, isFromCache);
		}
	}

	public static SupportedProtocols GetProtocolFromUri(Uri uri)
	{
		switch (uri.Scheme.ToLowerInvariant())
		{
		case "ws":
		case "wss":
			return SupportedProtocols.WebSocket;
		default:
			return SupportedProtocols.HTTP;
		}
	}

	public static bool IsSecureProtocol(Uri uri)
	{
		switch (uri.Scheme.ToLowerInvariant())
		{
		case "https":
		case "wss":
			return true;
		default:
			return false;
		}
	}
}
