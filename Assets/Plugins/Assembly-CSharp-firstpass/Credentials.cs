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

	public Credentials(string IFCOOFDKDGL, string AODNGDGJCMD)
		: this(AuthenticationTypes.Unknown, IFCOOFDKDGL, AODNGDGJCMD)
	{
	}

	public Credentials(AuthenticationTypes LFLGCDNKNJI, string IFCOOFDKDGL, string AODNGDGJCMD)
	{
		set_Type(LFLGCDNKNJI);
		SetUserName(IFCOOFDKDGL);
		SetPassword(AODNGDGJCMD);
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
