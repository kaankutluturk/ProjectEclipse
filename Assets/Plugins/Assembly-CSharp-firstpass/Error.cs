using System.Diagnostics;

public sealed class Error
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private SocketIOErrors code;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string message;

	public SocketIOErrors Code
	{
		get
		{
			return GetCode();
		}
		private set
		{
			SetCode(value);
		}
	}

	public Error(SocketIOErrors code, string message)
	{
		SetCode(code);
		set_Message(message);
	}

	public SocketIOErrors GetCode()
	{
		return code;
	}

	private void SetCode(SocketIOErrors value)
	{
		code = value;
	}

	public string GetMessage()
	{
		return message;
	}

	private void set_Message(string value)
	{
		message = value;
	}

	public override string ToString()
	{
		return string.Format("Code: {0} Message: \"{1}\"", GetCode().ToString(), GetMessage());
	}
}
