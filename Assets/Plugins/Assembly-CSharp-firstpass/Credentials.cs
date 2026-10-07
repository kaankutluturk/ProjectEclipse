using System.Diagnostics;

public sealed class Credentials
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private AuthenticationTypes type;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string userName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string password;

	public string UserName
	{
		get
		{
			return GetUserName();
		}
		private set
		{
			SetUserName(value);
		}
	}

	public string Password
	{
		get
		{
			return GetPassword();
		}
		private set
		{
			SetPassword(value);
		}
	}

	public Credentials(string userName, string password)
		: this(AuthenticationTypes.Unknown, userName, password)
	{
	}

	public Credentials(AuthenticationTypes type, string userName, string password)
	{
		set_Type(type);
		SetUserName(userName);
		SetPassword(password);
	}

	public AuthenticationTypes get_Type()
	{
		return type;
	}

	private void set_Type(AuthenticationTypes value)
	{
		type = value;
	}

	public string GetUserName()
	{
		return userName;
	}

	private void SetUserName(string value)
	{
		userName = value;
	}

	public string GetPassword()
	{
		return password;
	}

	private void SetPassword(string value)
	{
		password = value;
	}
}
