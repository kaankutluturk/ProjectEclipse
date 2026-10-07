public class CrashBreadcrumbTracker : SessionCrashInfo
{
	private static CrashBreadcrumbTracker _instance;

	public static CrashBreadcrumbTracker SharedInstance
	{
		get
		{
			return GetInstance();
		}
	}

	public static CrashBreadcrumbTracker GetInstance()
	{
		if (_instance == null)
		{
			_instance = new CrashBreadcrumbTracker();
		}
		return _instance;
	}

	public void AddBreadcrumb(string IEEAOCEJHGK)
	{
	}
}
