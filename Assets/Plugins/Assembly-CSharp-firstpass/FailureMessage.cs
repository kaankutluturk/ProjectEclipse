using System.Collections.Generic;
using System.Diagnostics;

public sealed class FailureMessage : IServerMessage, IHubMessage
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ulong invocationId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isHubError;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string errorMessage;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private IDictionary<string, object> additionalData;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string stackTrace;

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

	public bool IsHubErrorValue
	{
		get
		{
			return GetIsHubError();
		}
		private set
		{
			set_IsHubError(value);
		}
	}

	public string ErrorMessage
	{
		get
		{
			return GetErrorMessage();
		}
		private set
		{
			SetErrorMessage(value);
		}
	}

	public IDictionary<string, object> AdditionalData
	{
		get
		{
			return GetAdditionalData();
		}
		private set
		{
			SetAdditionalData(value);
		}
	}

	public string StackTrace
	{
		get
		{
			return GetStackTrace();
		}
		private set
		{
			SetStackTrace(value);
		}
	}

	public IDictionary<string, object> StateData
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
		return MessageTypes.Failure;
	}

	public ulong GetInvocationId()
	{
		return invocationId;
	}

	private void set_InvocationId(ulong value)
	{
		invocationId = value;
	}

	public bool GetIsHubError()
	{
		return isHubError;
	}

	private void set_IsHubError(bool value)
	{
		isHubError = value;
	}

	public string GetErrorMessage()
	{
		return errorMessage;
	}

	private void SetErrorMessage(string value)
	{
		errorMessage = value;
	}

	public IDictionary<string, object> GetAdditionalData()
	{
		return additionalData;
	}

	private void SetAdditionalData(IDictionary<string, object> value)
	{
		additionalData = value;
	}

	public string GetStackTrace()
	{
		return stackTrace;
	}

	private void SetStackTrace(string value)
	{
		stackTrace = value;
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
		if (dictionary.TryGetValue("E", out value))
		{
			SetErrorMessage(value.ToString());
		}
		if (dictionary.TryGetValue("H", out value))
		{
			set_IsHubError(int.Parse(value.ToString()) == 1);
		}
		if (dictionary.TryGetValue("D", out value))
		{
			SetAdditionalData(value as IDictionary<string, object>);
		}
		if (dictionary.TryGetValue("T", out value))
		{
			SetStackTrace(value.ToString());
		}
		if (dictionary.TryGetValue("S", out value))
		{
			set_State(value as IDictionary<string, object>);
		}
	}
}
