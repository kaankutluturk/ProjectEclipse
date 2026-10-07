using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public sealed class MethodCallMessage : IServerMessage
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string hub;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string method;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private object[] arguments;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private IDictionary<string, object> state;

	public string HubName
	{
		get
		{
			return GetHub();
		}
		private set
		{
			SetHub(value);
		}
	}

	public string Method
	{
		get
		{
			return GetMethod();
		}
		private set
		{
			SetMethod(value);
		}
	}

	public object[] MethodArguments
	{
		get
		{
			return GetArguments();
		}
		private set
		{
			set_Arguments(value);
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
		return MessageTypes.MethodCall;
	}

	public string GetHub()
	{
		return hub;
	}

	private void SetHub(string value)
	{
		hub = value;
	}

	public string GetMethod()
	{
		return method;
	}

	private void SetMethod(string value)
	{
		method = value;
	}

	public object[] GetArguments()
	{
		return arguments;
	}

	private void set_Arguments(object[] value)
	{
		arguments = value;
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
		SetHub(dictionary["H"].ToString());
		SetMethod(dictionary["M"].ToString());
		List<object> list = new List<object>();
		foreach (object item in dictionary["A"] as IEnumerable)
		{
			list.Add(item);
		}
		set_Arguments(list.ToArray());
		object value;
		if (dictionary.TryGetValue("S", out value))
		{
			set_State(value as IDictionary<string, object>);
		}
	}
}
