using System.Diagnostics;

public class RaidLoginState
{
	private static RaidLoginState _instance;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isLoggedIn;

	public static RaidLoginState SharedInstance
	{
		get
		{
			return GetInstance();
		}
	}

	public bool IsLoggedIn
	{
		get
		{
			return GetIsLoggedIn();
		}
		set
		{
			SetIsLoggedIn(value);
		}
	}

	public static RaidLoginState GetInstance()
	{
		if (_instance == null)
		{
			_instance = new RaidLoginState();
		}
		return _instance;
	}

	public bool GetIsLoggedIn()
	{
		return isLoggedIn;
	}

	public void SetIsLoggedIn(bool value)
	{
		isLoggedIn = value;
	}
}
