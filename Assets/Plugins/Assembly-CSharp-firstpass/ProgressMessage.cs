using System.Collections.Generic;
using System.Diagnostics;

public sealed class ProgressMessage : IServerMessage, IHubMessage
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ulong invocationId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private double progress;

	public ulong InvocationIdValue
	{
		get
		{
			return GetInvocationId();
		}
		private set
		{
			set_InvocationId(value);
		}
	}

	public double ProgressValue
	{
		get
		{
			return GetProgress();
		}
		private set
		{
			set_Progress(value);
		}
	}

	public MessageTypes get_Type()
	{
		return MessageTypes.Progress;
	}

	public ulong GetInvocationId()
	{
		return invocationId;
	}

	private void set_InvocationId(ulong value)
	{
		invocationId = value;
	}

	public double GetProgress()
	{
		return progress;
	}

	private void set_Progress(double value)
	{
		progress = value;
	}

	void IServerMessage.Parse(object data)
	{
		IDictionary<string, object> dictionary = data as IDictionary<string, object>;
		IDictionary<string, object> dictionary2 = dictionary["P"] as IDictionary<string, object>;
		set_InvocationId(ulong.Parse(dictionary2["I"].ToString()));
		set_Progress(double.Parse(dictionary2["D"].ToString()));
	}
}
