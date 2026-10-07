using System.Diagnostics;

public sealed class DataMessage : IServerMessage
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private object data;

	public MessageTypes get_Type()
	{
		return MessageTypes.Data;
	}

	public object GetData()
	{
		return data;
	}

	private void set_Data(object value)
	{
		data = value;
	}

	void IServerMessage.Parse(object data)
	{
		set_Data(data);
	}
}
