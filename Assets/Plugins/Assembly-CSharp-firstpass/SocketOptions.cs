using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public sealed class SocketOptions
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool reconnection;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int reconnectionAttempts;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan reconnectionDelay;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan reconnectionDelayMax;

	private float randomizationFactor;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan timeout;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool autoConnect;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Dictionary<string, string> additionalQueryParams;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool queryParamsOnlyForHandshake;

	private string BuiltQueryParams;

	public bool Reconnection
	{
		get
		{
			return GetReconnection();
		}
		set
		{
			SetReconnection(value);
		}
	}

	public int MaxReconnectionAttempts
	{
		get
		{
			return GetReconnectionAttempts();
		}
		set
		{
			set_ReconnectionAttempts(value);
		}
	}

	public TimeSpan ReconnectionDelay
	{
		get
		{
			return GetReconnectionDelay();
		}
		set
		{
			SetReconnectionDelay(value);
		}
	}

	public TimeSpan ReconnectionDelayMax
	{
		get
		{
			return GetReconnectionDelayMax();
		}
		set
		{
			SetReconnectionDelayMax(value);
		}
	}

	public float Randomization
	{
		get
		{
			return GetRandomizationFactor();
		}
		set
		{
			set_RandomizationFactor(value);
		}
	}

	public TimeSpan Timeout
	{
		get
		{
			return GetTimeout();
		}
		set
		{
			SetTimeout(value);
		}
	}

	public bool AutoConnect
	{
		get
		{
			return GetAutoConnect();
		}
		set
		{
			SetAutoConnect(value);
		}
	}

	public Dictionary<string, string> ConnectQueryParams
	{
		get
		{
			return GetAdditionalQueryParams();
		}
		set
		{
			set_AdditionalQueryParams(value);
		}
	}

	public bool QueryParamsHandshakeOnly
	{
		get
		{
			return GetQueryParamsOnlyForHandshake();
		}
		set
		{
			set_QueryParamsOnlyForHandshake(value);
		}
	}

	public SocketOptions()
	{
		SetReconnection(true);
		set_ReconnectionAttempts(int.MaxValue);
		SetReconnectionDelay(TimeSpan.FromMilliseconds(1000.0));
		SetReconnectionDelayMax(TimeSpan.FromMilliseconds(5000.0));
		set_RandomizationFactor(0.5f);
		SetTimeout(TimeSpan.FromMilliseconds(20000.0));
		SetAutoConnect(true);
		set_QueryParamsOnlyForHandshake(true);
	}

	public bool GetReconnection()
	{
		return reconnection;
	}

	public void SetReconnection(bool value)
	{
		reconnection = value;
	}

	public int GetReconnectionAttempts()
	{
		return reconnectionAttempts;
	}

	public void set_ReconnectionAttempts(int value)
	{
		reconnectionAttempts = value;
	}

	public TimeSpan GetReconnectionDelay()
	{
		return reconnectionDelay;
	}

	public void SetReconnectionDelay(TimeSpan value)
	{
		reconnectionDelay = value;
	}

	public TimeSpan GetReconnectionDelayMax()
	{
		return reconnectionDelayMax;
	}

	public void SetReconnectionDelayMax(TimeSpan value)
	{
		reconnectionDelayMax = value;
	}

	public float GetRandomizationFactor()
	{
		return randomizationFactor;
	}

	public void set_RandomizationFactor(float value)
	{
		randomizationFactor = Math.Min(1f, Math.Max(0f, value));
	}

	public TimeSpan GetTimeout()
	{
		return timeout;
	}

	public void SetTimeout(TimeSpan value)
	{
		timeout = value;
	}

	public bool GetAutoConnect()
	{
		return autoConnect;
	}

	public void SetAutoConnect(bool value)
	{
		autoConnect = value;
	}

	public Dictionary<string, string> GetAdditionalQueryParams()
	{
		return additionalQueryParams;
	}

	public void set_AdditionalQueryParams(Dictionary<string, string> value)
	{
		additionalQueryParams = value;
	}

	public bool GetQueryParamsOnlyForHandshake()
	{
		return queryParamsOnlyForHandshake;
	}

	public void set_QueryParamsOnlyForHandshake(bool value)
	{
		queryParamsOnlyForHandshake = value;
	}

	internal string BuildQueryParams()
	{
		if (GetAdditionalQueryParams() == null || GetAdditionalQueryParams().Count == 0)
		{
			return string.Empty;
		}
		if (!string.IsNullOrEmpty(BuiltQueryParams))
		{
			return BuiltQueryParams;
		}
		StringBuilder stringBuilder = new StringBuilder(GetAdditionalQueryParams().Count * 4);
		foreach (KeyValuePair<string, string> item in GetAdditionalQueryParams())
		{
			stringBuilder.Append("&");
			stringBuilder.Append(item.Key);
			if (!string.IsNullOrEmpty(item.Value))
			{
				stringBuilder.Append("=");
				stringBuilder.Append(item.Value);
			}
		}
		return BuiltQueryParams = stringBuilder.ToString();
	}
}
