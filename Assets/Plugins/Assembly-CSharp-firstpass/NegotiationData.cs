using System;
using System.Collections.Generic;
using System.Diagnostics;

public sealed class NegotiationData
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string url;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string webSocketServerUrl;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string connectionToken;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string connectionId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan? keepAliveTimeout;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan disconnectTimeout;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan connectionTimeout;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool tryWebSockets;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string protocolVersion;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan transportConnectTimeout;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan longPollDelay;

	public Action<NegotiationData> OnReceived;

	public Action<NegotiationData, string> OnError;

	private HTTPRequest negotiationRequest;

	private IConnection connection;

	public string WebSocketServerUrl
	{
		get
		{
			return GetWebSocketServerUrl();
		}
		private set
		{
			SetWebSocketServerUrl(value);
		}
	}

	public string ConnectionToken
	{
		get
		{
			return GetConnectionToken();
		}
		private set
		{
			SetConnectionToken(value);
		}
	}

	public string ConnectionId
	{
		get
		{
			return GetConnectionId();
		}
		private set
		{
			SetConnectionId(value);
		}
	}

	public TimeSpan? KeepAliveInterval
	{
		get
		{
			return GetKeepAliveTimeout();
		}
		private set
		{
			set_KeepAliveTimeout(value);
		}
	}

	public TimeSpan DisconnectTimeout
	{
		get
		{
			return GetDisconnectTimeout();
		}
		private set
		{
			SetDisconnectTimeout(value);
		}
	}

	public TimeSpan ConnectionTimeout
	{
		get
		{
			return GetConnectionTimeout();
		}
		private set
		{
			SetConnectionTimeout(value);
		}
	}

	public bool ShouldTryWebSockets
	{
		get
		{
			return GetTryWebSockets();
		}
		private set
		{
			set_TryWebSockets(value);
		}
	}

	public string ProtocolVersion
	{
		get
		{
			return GetProtocolVersion();
		}
		private set
		{
			SetProtocolVersion(value);
		}
	}

	public TimeSpan TransportConnectTimeout
	{
		get
		{
			return GetTransportConnectTimeout();
		}
		private set
		{
			SetTransportConnectTimeout(value);
		}
	}

	public TimeSpan LongPollDelay
	{
		get
		{
			return GetLongPollDelay();
		}
		private set
		{
			SetLongPollDelay(value);
		}
	}

	public NegotiationData(Connection parentConnection)
	{
		connection = parentConnection;
	}

	public string GetUrl()
	{
		return url;
	}

	private void set_Url(string value)
	{
		url = value;
	}

	public string GetWebSocketServerUrl()
	{
		return webSocketServerUrl;
	}

	private void SetWebSocketServerUrl(string value)
	{
		webSocketServerUrl = value;
	}

	public string GetConnectionToken()
	{
		return connectionToken;
	}

	private void SetConnectionToken(string value)
	{
		connectionToken = value;
	}

	public string GetConnectionId()
	{
		return connectionId;
	}

	private void SetConnectionId(string value)
	{
		connectionId = value;
	}

	public TimeSpan? GetKeepAliveTimeout()
	{
		return keepAliveTimeout;
	}

	private void set_KeepAliveTimeout(TimeSpan? value)
	{
		keepAliveTimeout = value;
	}

	public TimeSpan GetDisconnectTimeout()
	{
		return disconnectTimeout;
	}

	private void SetDisconnectTimeout(TimeSpan value)
	{
		disconnectTimeout = value;
	}

	public TimeSpan GetConnectionTimeout()
	{
		return connectionTimeout;
	}

	private void SetConnectionTimeout(TimeSpan value)
	{
		connectionTimeout = value;
	}

	public bool GetTryWebSockets()
	{
		return tryWebSockets;
	}

	private void set_TryWebSockets(bool value)
	{
		tryWebSockets = value;
	}

	public string GetProtocolVersion()
	{
		return protocolVersion;
	}

	private void SetProtocolVersion(string value)
	{
		protocolVersion = value;
	}

	public TimeSpan GetTransportConnectTimeout()
	{
		return transportConnectTimeout;
	}

	private void SetTransportConnectTimeout(TimeSpan value)
	{
		transportConnectTimeout = value;
	}

	public TimeSpan GetLongPollDelay()
	{
		return longPollDelay;
	}

	private void SetLongPollDelay(TimeSpan value)
	{
		longPollDelay = value;
	}

	public void Start()
	{
		negotiationRequest = new HTTPRequest(connection.BuildUri(SignalRRequestType.Negotiate), HTTPMethods.Get, true, true, OnNegotiationRequestFinished);
		connection.PrepareRequest(negotiationRequest, SignalRRequestType.Negotiate);
		negotiationRequest.Send();
		HTTPManager.GetLogger().Information("NegotiationData", "Negotiation request sent");
	}

	public void Abort()
	{
		if (negotiationRequest != null)
		{
			OnReceived = null;
			OnError = null;
			negotiationRequest.Abort();
		}
	}

	private void OnNegotiationRequestFinished(HTTPRequest request, HTTPResponse response)
	{
		negotiationRequest = null;
		switch (request.GetState())
		{
		case HTTPRequestStates.Finished:
			if (response.GetIsSuccess())
			{
				HTTPManager.GetLogger().Information("NegotiationData", "Negotiation data arrived: " + response.GetDataAsText());
				int num = response.GetDataAsText().IndexOf("{");
				if (num < 0)
				{
					RaiseOnError("Invalid negotiation text: " + response.GetDataAsText());
					break;
				}
				NegotiationData parsedData = Parse(response.GetDataAsText().Substring(num));
				if (parsedData == null)
				{
					RaiseOnError("Parsing Negotiation data failed: " + response.GetDataAsText());
				}
				else if (OnReceived != null)
				{
					OnReceived(this);
					OnReceived = null;
				}
			}
			else
			{
				RaiseOnError(string.Format("Negotiation request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText(), request.GetCurrentUri()));
			}
			break;
		case HTTPRequestStates.Error:
			RaiseOnError((request.GetException() == null) ? string.Empty : (request.GetException().Message + " " + request.GetException().StackTrace));
			break;
		default:
			RaiseOnError(request.GetState().ToString());
			break;
		}
	}

	private void RaiseOnError(string error)
	{
		HTTPManager.GetLogger().Error("NegotiationData", "Negotiation request failed with error: " + error);
		if (OnError != null)
		{
			OnError(this, error);
			OnError = null;
		}
	}

	private NegotiationData Parse(string json)
	{
		bool success = false;
		Dictionary<string, object> dictionary = Json.Decode(json, ref success) as Dictionary<string, object>;
		if (!success)
		{
			return null;
		}
		try
		{
			set_Url(GetString(dictionary, "Url"));
			if (dictionary.ContainsKey("webSocketServerUrl"))
			{
				SetWebSocketServerUrl(GetString(dictionary, "webSocketServerUrl"));
			}
			SetConnectionToken(Uri.EscapeDataString(GetString(dictionary, "ConnectionToken")));
			SetConnectionId(GetString(dictionary, "ConnectionId"));
			if (dictionary.ContainsKey("KeepAliveTimeout"))
			{
				set_KeepAliveTimeout(TimeSpan.FromSeconds(GetDouble(dictionary, "KeepAliveTimeout")));
			}
			SetDisconnectTimeout(TimeSpan.FromSeconds(GetDouble(dictionary, "DisconnectTimeout")));
			SetConnectionTimeout(TimeSpan.FromSeconds(GetDouble(dictionary, "ConnectionTimeout")));
			set_TryWebSockets((bool)Get(dictionary, "TryWebSockets"));
			SetProtocolVersion(GetString(dictionary, "ProtocolVersion"));
			SetTransportConnectTimeout(TimeSpan.FromSeconds(GetDouble(dictionary, "TransportConnectTimeout")));
			SetLongPollDelay(TimeSpan.FromSeconds(GetDouble(dictionary, "LongPollDelay")));
			return this;
		}
		catch (Exception exception)
		{
			HTTPManager.GetLogger().Exception("NegotiationData", "Parse", exception);
			return null;
		}
	}

	private static object Get(Dictionary<string, object> data, string key)
	{
		object value;
		if (!data.TryGetValue(key, out value))
		{
			throw new Exception(string.Format("Can't get {0} from Negotiation data!", key));
		}
		return value;
	}

	private static string GetString(Dictionary<string, object> data, string key)
	{
		return Get(data, key) as string;
	}

	private static List<string> GetStringList(Dictionary<string, object> data, string key)
	{
		List<object> list = Get(data, key) as List<object>;
		List<string> list2 = new List<string>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			string text = list[i] as string;
			if (text != null)
			{
				list2.Add(text);
			}
		}
		return list2;
	}

	private static int GetInt(Dictionary<string, object> data, string key)
	{
		return (int)(double)Get(data, key);
	}

	private static double GetDouble(Dictionary<string, object> data, string key)
	{
		return (double)Get(data, key);
	}
}
