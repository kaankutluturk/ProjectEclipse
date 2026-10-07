using System.Collections.Generic;
using System.Diagnostics;

public sealed class ResultMessage : IServerMessage, IHubMessage
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ulong invocationId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private object returnValue;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private IDictionary<string, object> state;

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

	public object Result
	{
		get
		{
			return GetReturnValue();
		}
		private set
		{
			set_ReturnValue(value);
		}
	}

	public IDictionary<string, object> HubState
	{
		get
		{
			return GetState();
		}
		private set
		{
			set_State(value);
		}
	}

	public MessageTypes get_Type()
	{
		return MessageTypes.Result;
	}

	public ulong GetInvocationId()
	{
		return invocationId;
	}

	private void set_InvocationId(ulong value)
	{
		invocationId = value;
	}

	public object GetReturnValue()
	{
		return returnValue;
	}

	private void set_ReturnValue(object value)
	{
		returnValue = value;
	}

	public IDictionary<string, object> GetState()
	{
		return state;
	}

	private void set_State(IDictionary<string, object> value)
	{
		state = value;
	}

	void IServerMessage.Parse(object data)
	{
		IDictionary<string, object> dictionary = data as IDictionary<string, object>;
		set_InvocationId(ulong.Parse(dictionary["I"].ToString()));
		object value;
		if (dictionary.TryGetValue("R", out value))
		{
			set_ReturnValue(value);
		}
		if (dictionary.TryGetValue("S", out value))
		{
			set_State(value as IDictionary<string, object>);
		}
	}
}
