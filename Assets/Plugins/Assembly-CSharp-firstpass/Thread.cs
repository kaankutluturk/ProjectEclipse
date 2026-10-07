using System;

public class Thread
{
	public bool Background
	{
		get
		{
			return GetIsBackground();
		}
		set
		{
			set_IsBackground(value);
		}
	}

	public Thread(ThreadStartDelegate start)
	{
		throw new NotSupportedException();
	}

	public Thread(ParameterizedThreadStart start)
	{
		throw new NotSupportedException();
	}

	public bool GetIsBackground()
	{
		return true;
	}

	public void set_IsBackground(bool value)
	{
		throw new NotImplementedException("currently always on background");
	}

	public void Abort()
	{
		throw new NotSupportedException();
	}

	public bool Join(int milliseconds)
	{
		throw new NotSupportedException();
	}

	public void Start()
	{
		throw new NotSupportedException();
	}

	public void Start(object parameter)
	{
		throw new NotSupportedException();
	}

	public static void Sleep(int milliseconds)
	{
		throw new NotSupportedException();
	}
}
