using System;
using System.Collections.Generic;
using System.Diagnostics;

public sealed class HandshakeData
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string sid;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<string> upgrades;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan pingInterval;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan pingTimeout;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SocketManager manager;

	public Action<HandshakeData> OnReceived;

	public Action<HandshakeData, string> OnError;

	private HTTPRequest handshakeRequest;

	public string SessionId
	{
		get
		{
			return GetSid();
		}
		private set
		{
			set_Sid(value);
		}
	}

	public List<string> UpgradeTransports
	{
		get
		{
			return GetUpgrades();
		}
		private set
		{
			set_Upgrades(value);
		}
	}

	public TimeSpan PingInterval
	{
		get
		{
			return GetPingInterval();
		}
		private set
		{
			SetPingInterval(value);
		}
	}

	public TimeSpan PingTimeout
	{
		get
		{
			return GetPingTimeout();
		}
		private set
		{
			SetPingTimeout(value);
		}
	}

	public SocketManager HandshakeManager
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

	public HandshakeData(SocketManager manager)
	{
		SetManager(manager);
	}

	public string GetSid()
	{
		return sid;
	}

	private void set_Sid(string value)
	{
		sid = value;
	}

	public List<string> GetUpgrades()
	{
		return upgrades;
	}

	private void set_Upgrades(List<string> value)
	{
		upgrades = value;
	}

	public TimeSpan GetPingInterval()
	{
		return pingInterval;
	}

	private void SetPingInterval(TimeSpan value)
	{
		pingInterval = value;
	}

	public TimeSpan GetPingTimeout()
	{
		return pingTimeout;
	}

	private void SetPingTimeout(TimeSpan value)
	{
		pingTimeout = value;
	}

	public SocketManager GetManager()
	{
		return manager;
	}

	private void SetManager(SocketManager value)
	{
		manager = value;
	}

	internal void Start()
	{
		if (handshakeRequest == null)
		{
			object[] obj = new object[5]
			{
				GetManager().GetUri().ToString(),
				4,
				GetManager().GetTimestamp(),
				null,
				null
			};
			SocketManager socketManager = GetManager();
			ulong num;
			socketManager.set_RequestCounter((num = socketManager.GetRequestCounter()) + 1);
			obj[3] = num;
			obj[4] = GetManager().GetOptions().BuildQueryParams();
			handshakeRequest = new HTTPRequest(new Uri(string.Format("{0}?EIO={1}&transport=polling&t={2}-{3}{4}&b64=true", obj)), OnHandshakeCallback);
			handshakeRequest.SetDisableCache(true);
			handshakeRequest.Send();
			HTTPManager.GetLogger().Information("HandshakeData", "Handshake request sent");
		}
	}

	internal void Abort()
	{
		if (handshakeRequest != null)
		{
			handshakeRequest.Abort();
		}
		handshakeRequest = null;
		OnReceived = null;
		OnError = null;
	}

	private void OnHandshakeCallback(HTTPRequest request, HTTPResponse response)
	{
		handshakeRequest = null;
		switch (request.GetState())
		{
		case HTTPRequestStates.Finished:
			if (response.GetIsSuccess())
			{
				HTTPManager.GetLogger().Information("HandshakeData", "Handshake data arrived: " + response.GetDataAsText());
				int num = response.GetDataAsText().IndexOf("{");
				if (num < 0)
				{
					RaiseOnError("Invalid handshake text: " + response.GetDataAsText());
					break;
				}
				HandshakeData handshakeData = Parse(response.GetDataAsText().Substring(num));
				if (handshakeData == null)
				{
					RaiseOnError("Parsing Handshake data failed: " + response.GetDataAsText());
				}
				else if (OnReceived != null)
				{
					OnReceived(this);
					OnReceived = null;
				}
			}
			else
			{
				RaiseOnError(string.Format("Handshake request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}", response.GetStatusCode(), response.GetMessage(), response.GetDataAsText(), request.GetCurrentUri()));
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
		HTTPManager.GetLogger().Error("HandshakeData", "Handshake request failed with error: " + error);
		if (OnError != null)
		{
			OnError(this, error);
			OnError = null;
		}
	}

	private HandshakeData Parse(string json)
	{
		bool success = false;
		Dictionary<string, object> data = Json.Decode(json, ref success) as Dictionary<string, object>;
		if (!success)
		{
			return null;
		}
		try
		{
			set_Sid(GetString(data, "sid"));
			set_Upgrades(GetStringList(data, "upgrades"));
			SetPingInterval(TimeSpan.FromMilliseconds(GetInt(data, "pingInterval")));
			SetPingTimeout(TimeSpan.FromMilliseconds(GetInt(data, "pingTimeout")));
			return this;
		}
		catch
		{
			return null;
		}
	}

	private static object Get(Dictionary<string, object> data, string key)
	{
		object value;
		if (!data.TryGetValue(key, out value))
		{
			throw new Exception(string.Format("Can't get {0} from Handshake data!", key));
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
}
