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

	public WebSocket(Uri KJHNCLAJMLO)
		: this(KJHNCLAJMLO, string.Empty, string.Empty)
	{
	}

	public WebSocket(Uri KJHNCLAJMLO, string IKOOJMAOFOD, string ENLHAIGCCBO = "")
	{
		set_PingFrequency(1000);
		if (KJHNCLAJMLO.Port == -1)
		{
			KJHNCLAJMLO = new Uri(KJHNCLAJMLO.Scheme + "://" + KJHNCLAJMLO.Host + ":" + ((!KJHNCLAJMLO.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase)) ? "80" : "443") + KJHNCLAJMLO.PathAndQuery);
		}
		SetInternalRequest(new HTTPRequest(KJHNCLAJMLO, OnInternalRequestCallback));
		GetInternalRequest().OnUpgraded = OnInternalRequestUpgraded;
		GetInternalRequest().SetHeader("Host", KJHNCLAJMLO.Host + ":" + KJHNCLAJMLO.Port);
		GetInternalRequest().SetHeader("Upgrade", "websocket");
		GetInternalRequest().SetHeader("Connection", "keep-alive, Upgrade");
		GetInternalRequest().SetHeader("Sec-WebSocket-Key", GetSecKey(new object[4]
		{
			this,
			GetInternalRequest(),
			KJHNCLAJMLO,
			new object()
		}));
		if (!string.IsNullOrEmpty(IKOOJMAOFOD))
		{
			GetInternalRequest().SetHeader("Origin", IKOOJMAOFOD);
		}
		GetInternalRequest().SetHeader("Sec-WebSocket-Version", "13");
		if (!string.IsNullOrEmpty(ENLHAIGCCBO))
		{
			GetInternalRequest().SetHeader("Sec-WebSocket-Protocol", ENLHAIGCCBO);
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

	private void OnInternalRequestCallback(HTTPRequest CGOIOKHEGOE, HTTPResponse BEIGFGCBICO)
	{
		string empty = string.Empty;
		switch (CGOIOKHEGOE.GetState())
		{
		default:
			return;
		case HTTPRequestStates.Finished:
			if (BEIGFGCBICO.GetIsSuccess() || BEIGFGCBICO.GetStatusCode() == 101)
			{
				HTTPManager.GetLogger().Information("WebSocket", string.Format("Request finished. Status Code: {0} Message: {1}", BEIGFGCBICO.GetStatusCode().ToString(), BEIGFGCBICO.GetMessage()));
				return;
			}
			empty = string.Format("Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}", BEIGFGCBICO.GetStatusCode(), BEIGFGCBICO.GetMessage(), BEIGFGCBICO.GetDataAsText());
			break;
		case HTTPRequestStates.Error:
			empty = "Request Finished with Error! " + ((CGOIOKHEGOE.GetException() == null) ? string.Empty : ("Exception: " + CGOIOKHEGOE.GetException().Message + CGOIOKHEGOE.GetException().StackTrace));
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
			OnError(this, CGOIOKHEGOE.GetException());
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

	private void OnInternalRequestUpgraded(HTTPRequest CGOIOKHEGOE, HTTPResponse BEIGFGCBICO)
	{
		webSocket = BEIGFGCBICO as WebSocketResponse;
		if (webSocket == null)
		{
			if (OnError != null)
			{
				OnError(this, CGOIOKHEGOE.GetException());
			}
			if (OnErrorDesc != null)
			{
				string nEPOLDCKNJL = string.Empty;
				if (CGOIOKHEGOE.GetException() != null)
				{
					nEPOLDCKNJL = CGOIOKHEGOE.GetException().Message + " " + CGOIOKHEGOE.GetException().StackTrace;
				}
				OnErrorDesc(this, nEPOLDCKNJL);
			}
			return;
		}
		if (OnOpen != null)
		{
			try
			{
				OnOpen(this);
			}
			catch (Exception mPFFFAOGBJE)
			{
				HTTPManager.GetLogger().Exception("WebSocket", "OnOpen", mPFFFAOGBJE);
			}
		}
		webSocket.OnText = (WebSocketResponse IIBIPJJLEGJ, string CKEHOEGLMBM) =>
		{
			if (OnMessage != null)
			{
				OnMessage(this, CKEHOEGLMBM);
			}
		};
		webSocket.OnBinary = (WebSocketResponse IIBIPJJLEGJ, byte[] DOEJIOEKACH) =>
		{
			if (OnBinary != null)
			{
				OnBinary(this, DOEJIOEKACH);
			}
		};
		webSocket.OnClosed = (WebSocketResponse IIBIPJJLEGJ, ushort KJPGKHJNOMC, string CKEHOEGLMBM) =>
		{
			if (OnClosed != null)
			{
				OnClosed(this, KJPGKHJNOMC, CKEHOEGLMBM);
			}
		};
		if (OnIncompleteFrame != null)
		{
			webSocket.OnIncompleteFrame = (WebSocketResponse IIBIPJJLEGJ, WebSocketFrameReader frame) =>
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

	public void Send(string LIOGIBJBHAH)
	{
		if (GetIsOpen())
		{
			webSocket.Send(LIOGIBJBHAH);
		}
	}

	public void Send(byte[] buffer)
	{
		if (GetIsOpen())
		{
			webSocket.Send(buffer);
		}
	}

	public void Send(byte[] buffer, ulong IPCOBJBKNAO, ulong count)
	{
		if (GetIsOpen())
		{
			webSocket.Send(buffer, IPCOBJBKNAO, count);
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

	public void Close(ushort KJPGKHJNOMC, string LIOGIBJBHAH)
	{
		if (GetIsOpen())
		{
			webSocket.Close(KJPGKHJNOMC, LIOGIBJBHAH);
		}
	}

	private string GetSecKey(object[] IOFHCAAOELD)
	{
		byte[] array = new byte[16];
		int num = 0;
		for (int i = 0; i < IOFHCAAOELD.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes(IOFHCAAOELD[i].GetHashCode());
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
