using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

public class NekkiLog : MonoBehaviour
{
	public enum LogLevel : byte
	{
		Log = 0,
		Warning = 1,
		Error = 2,
		Exception = 3,
		Assert = 4
	}

	private static string _directory = string.Empty;

	private static StringBuilder _items = new StringBuilder();

	private static DateTime _now;

	private static NekkiLog _instance;

	private static bool _captureUnityLog;

	private static string _fileName = "log";

	private static bool _notInitialized = true;

	private static bool _dateInFileName = true;

	private static bool _writeTimestamps = true;

	private static LogLevel _minLevel = LogLevel.Log;

	private bool _isWriting;

	private static readonly object LOCKER = new object();

	private static string FileName
	{
		get
		{
			return GetFileName();
		}
	}

	private static string FilePath
	{
		get
		{
			return GetFilePath();
		}
	}

	private static string GetFileName()
	{
		if (!_dateInFileName)
		{
			return string.Format("{0}.nekkilog", _fileName);
		}
		return string.Format("{3} {0}.{1}.{2}.nekkilog", _now.Year.ToString("0000"), _now.Month.ToString("00"), _now.Day.ToString("00"), _fileName);
	}

	private static string GetFilePath()
	{
		if (string.IsNullOrEmpty(_directory))
		{
			return Path.Combine(Application.persistentDataPath, GetFileName());
		}
		return Path.Combine(_directory, GetFileName());
	}

	public static void Init(string directory, string fileName, bool captureUnityLog, bool dateInFileName, bool writeTimestamps, LogLevel minLevel)
	{
		if ((bool)_instance)
		{
			_instance.Write();
		}
		_directory = directory.TrimEnd('/').TrimEnd('\\');
		_minLevel = minLevel;
		_dateInFileName = dateInFileName;
		_writeTimestamps = writeTimestamps;
		_captureUnityLog = captureUnityLog;
		if (!string.IsNullOrEmpty(fileName))
		{
			_fileName = fileName;
		}
		EnsureInstance();
		_notInitialized = false;
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}
		FileInfo fileInfo = new FileInfo(GetFilePath());
		if (!fileInfo.Exists)
		{
			FileStream fileStream = fileInfo.Create();
			fileStream.Close();
		}
	}

	private static void EnsureInstance()
	{
		if (!_instance)
		{
			GameObject gameObject = new GameObject("_log");
			_instance = gameObject.AddComponent<NekkiLog>();
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			_instance.Update();
			_instance.StartCoroutine(_instance.WriteLoop());
			Application.logMessageReceived += _unityLogCallback;
		}
	}

	private void OnDestroy()
	{
		Stop();
	}

	private void OnEnable()
	{
		StopAllCoroutines();
		StartCoroutine(WriteLoop());
	}

	private static void _unityLogCallback(string condition, string stackTrace, LogType type)
	{
		if (_captureUnityLog)
		{
			switch (type)
			{
			case LogType.Error:
				Error(condition, stackTrace);
				break;
			case LogType.Assert:
				Assert(condition, stackTrace);
				break;
			case LogType.Warning:
				Warning(condition, stackTrace);
				break;
			case LogType.Log:
				Log(condition, stackTrace);
				break;
			case LogType.Exception:
				LogException(condition, stackTrace);
				break;
			}
		}
	}

	private void Update()
	{
		if (_writeTimestamps || _dateInFileName)
		{
			_now = DateTime.Now;
		}
	}

	public static void Log(object message, string stackTrace = null)
	{
		AppendEntry(LogLevel.Log, message, stackTrace);
	}

	public static void Warning(object message, string stackTrace = null)
	{
		AppendEntry(LogLevel.Warning, message, stackTrace);
	}

	public static void Error(object message, string stackTrace = null)
	{
		AppendEntry(LogLevel.Error, message, stackTrace);
	}

	public static void Exception(Exception exception)
	{
		AppendEntry(LogLevel.Exception, exception.Message, exception.StackTrace);
	}

	private static void LogException(object message, string stackTrace)
	{
		AppendEntry(LogLevel.Exception, message, stackTrace);
	}

	public static void Assert(object message, string stackTrace = null)
	{
		AppendEntry(LogLevel.Assert, message, stackTrace);
	}

	public static void Stop()
	{
		_instance.Write();
		_notInitialized = true;
	}

	private static string FormatEntry(LogLevel level, object message, string stackTrace = null)
	{
		object obj = ((!string.IsNullOrEmpty(stackTrace)) ? string.Format("{0} at: {1}", message, stackTrace) : message);
		if (!_writeTimestamps)
		{
			return string.Format("{0}\n", obj);
		}
		return string.Format("[{3}] [{0}:{1}:{2}] {4}\n", _now.Hour.ToString("00"), _now.Minute.ToString("00"), _now.Second.ToString("00"), level, obj);
	}

	private static void AppendEntry(LogLevel level, object message, string stackTrace = null)
	{
		if (_notInitialized)
		{
			AdvLog.LogWarning("you must init log system first!");
		}
		else if ((int)level >= (int)_minLevel)
		{
			EnsureInstance();
			lock (LOCKER)
			{
				_items.Append(FormatEntry(level, message, stackTrace));
			}
		}
	}

	private IEnumerator WriteLoop()
	{
		while ((bool)base.transform)
		{
			yield return new WaitForSeconds(1f);
			Write();
		}
	}

	private void Write()
	{
		if (_isWriting)
		{
			return;
		}
		_isWriting = true;
		string value;
		lock (LOCKER)
		{
			value = _items.ToString();
			_items = new StringBuilder();
		}
		try
		{
			FileInfo fileInfo = new FileInfo(GetFilePath());
			if (!fileInfo.Exists)
			{
				FileStream fileStream = fileInfo.Create();
				fileStream.Close();
			}
			StreamWriter streamWriter = fileInfo.AppendText();
			streamWriter.Write(value);
			streamWriter.Close();
		}
		catch (Exception ex)
		{
			MonoBehaviour.print(ex.Message);
		}
		_isWriting = false;
	}
}
