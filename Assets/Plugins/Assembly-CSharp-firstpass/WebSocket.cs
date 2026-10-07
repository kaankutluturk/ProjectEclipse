using System;
using System.Diagnostics;

public sealed class WebSocket
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HTTPRequest internalRequest;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool startPingThread;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int pingFrequency;

	public OnWebSocketOpenDelegate OnOpen;

	public OnWebSocketMessageDelegate OnMessage;

	public OnWebSocketBinaryDelegate OnBinary;

	public OnWebSocketClosedDelegate OnClosed;

	public OnWebSocketErrorDelegate OnError;

	public OnWebSocketErrorDescriptionDelegate OnErrorDesc;

	public OnWebSocketIncompleteFrameDelegate OnIncompleteFrame;

	private bool requestSent;

	private WebSocketResponse webSocket;

	public HTTPRequest InternalRequest
	{
		get
		{
			return GetInternalRequest();
		}
		private set
		{
			SetInternalRequest(value);
		}
	}

	public bool IsOpen
	{
		get
		{
			return GetIsOpen();
		}
	}

	public bool EnablePingThread
	{
		get
		{
			return GetStartPingThread();
		}
		set
		{
			set_StartPingThread(value);
		}
	}

	public int PingFrequencyMs
	{
		get
		{
			return GetPingFrequency();
		}
		set
		{
			set_PingFrequency(value);
		}
	}

	public WebSocket(Uri uri)
		: this(uri, string.Empty, string.Empty)
	{
	}

	public WebSocket(Uri uri, string origin, string protocol = "")
	{
		set_PingFrequency(1000);
		if (uri.Port == -1)
		{
			uri = new Uri(uri.Scheme + "://" + uri.Host + ":" + ((!uri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase)) ? "80" : "443") + uri.PathAndQuery);
		}
		SetInternalRequest(new HTTPRequest(uri, OnInternalRequestCallback));
		GetInternalRequest().OnUpgraded = OnInternalRequestUpgraded;
		GetInternalRequest().SetHeader("Host", uri.Host + ":" + uri.Port);
		GetInternalRequest().SetHeader("Upgrade", "websocket");
		GetInternalRequest().SetHeader("Connection", "keep-alive, Upgrade");
		GetInternalRequest().SetHeader("Sec-WebSocket-Key", GetSecKey(new object[4]
		{
			this,
			GetInternalRequest(),
			uri,
			new object()
		}));
		if (!string.IsNullOrEmpty(origin))
		{
			GetInternalRequest().SetHeader("Origin", origin);
		}
		GetInternalRequest().SetHeader("Sec-WebSocket-Version", "13");
		if (!string.IsNullOrEmpty(protocol))
		{
			GetInternalRequest().SetHeader("Sec-WebSocket-Protocol", protocol);
		}
		GetInternalRequest().SetHeader("Cache-Control", "no-cache");
		GetInternalRequest().SetHeader("Pragma", "no-cache");
		GetInternalRequest().SetDisableCache(true);
		if (HTTPManager.GetProxy() != null)
		{
			GetInternalRequest().SetProxy(new HTTPProxy(HTTPManager.GetProxy().GetAddress(), HTTPManager.GetProxy().GetCredentials(), false, false, HTTPManager.GetProxy().GetNonTransparentForHTTPS()));
		}
	}

	public HTTPRequest GetInternalRequest()
	{
		return internalRequest;
	}

	private void SetInternalRequest(HTTPRequest value)
	{
		internalRequest = value;
	}

	public bool GetIsOpen()
	{
		return webSocket != null && !webSocket.GetIsClosed();
	}

	public bool GetStartPingThread()
	{
		return startPingThread;
	}

	public void set_StartPingThread(bool value)
	{
		startPingThread = value;
	}

	public int GetPingFrequency()
	{
		return pingFrequency;
	}

	public void set_PingFrequency(int value)
	{
		pingFrequency = value;
	}

	private void OnInternalRequestCallback(HTTPRequest request, HTTPResponse response)
	{
		string empty = string.Empty;
		switch (request.GetState())
		{
		default:
			return;
		case HTTPRequestStates.Finished:
			if (response.GetIsSuccess() || response.GetStatusCode() == 101)
			{
				HTTPManager.GetLogger().Information("WebSocket", string.Format("Request finished. Status Code: {0} Message: {1}", response.GetStatusCode().ToString(), response.GetMessage()));
				return;
			}
			empty = string.Format("Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText());
			break;
		case HTTPRequestStates.Error:
			empty = "Request Finished with Error! " + ((request.GetException() == null) ? string.Empty : ("Exception: " + request.GetException().Message + request.GetException().StackTrace));
			break;
		case HTTPRequestStates.Aborted:
			empty = "Request Aborted!";
			break;
		case HTTPRequestStates.ConnectionTimedOut:
			empty = "Connection Timed Out!";
			break;
		case HTTPRequestStates.TimedOut:
			empty = "Processing the request Timed Out!";
			break;
		}
		if (OnError != null)
		{
			OnError(this, request.GetException());
		}
		if (OnErrorDesc != null)
		{
			OnErrorDesc(this, empty);
		}
		if (OnError == null && OnErrorDesc == null)
		{
			HTTPManager.GetLogger().Error("WebSocket", empty);
		}
	}

	private void OnInternalRequestUpgraded(HTTPRequest request, HTTPResponse response)
	{
		webSocket = response as WebSocketResponse;
		if (webSocket == null)
		{
			if (OnError != null)
			{
				OnError(this, request.GetException());
			}
			if (OnErrorDesc != null)
			{
				string errorDescription = string.Empty;
				if (request.GetException() != null)
				{
					errorDescription = request.GetException().Message + " " + request.GetException().StackTrace;
				}
				OnErrorDesc(this, errorDescription);
			}
			return;
		}
		if (OnOpen != null)
		{
			try
			{
				OnOpen(this);
			}
			catch (Exception exception)
			{
				HTTPManager.GetLogger().Exception("WebSocket", "OnOpen", exception);
			}
		}
		webSocket.OnText = (WebSocketResponse response, string message) =>
		{
			if (OnMessage != null)
			{
				OnMessage(this, message);
			}
		};
		webSocket.OnBinary = (WebSocketResponse response, byte[] data) =>
		{
			if (OnBinary != null)
			{
				OnBinary(this, data);
			}
		};
		webSocket.OnClosed = (WebSocketResponse response, ushort code, string message) =>
		{
			if (OnClosed != null)
			{
				OnClosed(this, code, message);
			}
		};
		if (OnIncompleteFrame != null)
		{
			webSocket.OnIncompleteFrame = (WebSocketResponse response, WebSocketFrameReader frame) =>
			{
				if (OnIncompleteFrame != null)
				{
					OnIncompleteFrame(this, frame);
				}
			};
		}
		if (GetStartPingThread())
		{
			webSocket.StartPinging(Math.Min(GetPingFrequency(), 100));
		}
		webSocket.StartReceive();
	}

	public void OpenWebSocket()
	{
		if (!requestSent && GetInternalRequest() != null)
		{
			GetInternalRequest().Send();
			requestSent = true;
		}
	}

	public void Send(string message)
	{
		if (GetIsOpen())
		{
			webSocket.Send(message);
		}
	}

	public void Send(byte[] buffer)
	{
		if (GetIsOpen())
		{
			webSocket.Send(buffer);
		}
	}

	public void Send(byte[] buffer, ulong offset, ulong count)
	{
		if (GetIsOpen())
		{
			webSocket.Send(buffer, offset, count);
		}
	}

	public void Send(IWebSocketFrameWriter frame)
	{
		if (GetIsOpen())
		{
			webSocket.Send(frame);
		}
	}

	public void Close()
	{
		if (GetIsOpen())
		{
			webSocket.Close();
		}
	}

	public void Close(ushort code, string message)
	{
		if (GetIsOpen())
		{
			webSocket.Close(code, message);
		}
	}

	private string GetSecKey(object[] parts)
	{
		byte[] array = new byte[16];
		int num = 0;
		for (int i = 0; i < parts.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes(parts[i].GetHashCode());
			for (int j = 0; j < bytes.Length; j++)
			{
				if (num >= array.Length)
				{
					break;
				}
				array[num++] = bytes[j];
			}
		}
		return Convert.ToBase64String(array);
	}
}
