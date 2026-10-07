public class EventLog : BaseEventLog
{
	private static EventLog instance;

	public static EventLog Instance
	{
		get
		{
			return GetInstance();
		}
	}

	public static EventLog GetInstance()
	{
		if (instance == null)
		{
			instance = new EventLog();
		}
		return instance;
	}
}
