using System;
using System.Diagnostics;

public sealed class Message
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string id;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string eventName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string data;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan retry;

	public string Id
	{
		get
		{
			return GetMessageId();
		}
		internal set
		{
			SetId(value);
		}
	}

	public string EventName
	{
		get
		{
			return GetEvent();
		}
		internal set
		{
			set_Event(value);
		}
	}

	public TimeSpan RetryInterval
	{
		get
		{
			return GetRetry();
		}
		internal set
		{
			set_Retry(value);
		}
	}

	public string GetMessageId()
	{
		return id;
	}

	internal void SetId(string value)
	{
		id = value;
	}

	public string GetEvent()
	{
		return eventName;
	}

	internal void set_Event(string value)
	{
		eventName = value;
	}

	public string GetData()
	{
		return data;
	}

	internal void set_Data(string value)
	{
		data = value;
	}

	public TimeSpan GetRetry()
	{
		return retry;
	}

	internal void set_Retry(TimeSpan value)
	{
		retry = value;
	}

	public override string ToString()
	{
		return string.Format("\"{0}\": \"{1}\"", GetEvent(), GetData());
	}
}
