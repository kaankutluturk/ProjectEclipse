using UnityEngine;

public static class GameLog
{
	public static void Write(string message)
	{
		Debug.Log(message);
	}

	public static void Write(string format, params object[] args)
	{
		Debug.LogFormat(format, args);
	}

	public static void Error(string message)
	{
		Debug.LogError(message);
	}

	public static void Error(string format, params object[] args)
	{
		Debug.LogErrorFormat(format, args);
	}

	public static void Warning(string format, params object[] args)
	{
		Debug.LogWarningFormat(format, args);
	}

	public static void Info(string message)
	{
		Debug.Log(message);
	}

	public static void Info(string format, params object[] args)
	{
		Debug.LogFormat(format, args);
	}
}
