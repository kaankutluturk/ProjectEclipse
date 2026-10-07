public class BaseEventLog : global::EventDispatcher<object>
{
	public enum LoggingState
	{
		NotLogging = 0,
		Logging = 1,
		Undecided = 2
	}

	private LoggingState loggingState;

	public bool IsLogging
	{
		get
		{
			return GetIsLogging();
		}
		set
		{
			SetIsLogging(value);
		}
	}

	public bool GetIsLogging()
	{
		return loggingState != LoggingState.NotLogging;
	}

	public void SetIsLogging(bool value)
	{
		if (value)
		{
			loggingState = LoggingState.Logging;
			Send();
		}
		else
		{
			loggingState = LoggingState.NotLogging;
		}
	}

	public void Send()
	{
	}
}
