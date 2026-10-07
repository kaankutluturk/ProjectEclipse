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

	public static void Init(string KOBDDMHGOPJ, string PMFEIPCHENB, bool DHFCJMJBFDP, bool AJDGNMMKEBE, bool AODCELGDHPO, LogLevel JJAFNMOOCKJ)
	{
		if ((bool)_instance)
		{
			_instance.Write();
		}
		_directory = KOBDDMHGOPJ.TrimEnd('/').TrimEnd('\\');
		_minLevel = JJAFNMOOCKJ;
		_dateInFileName = AJDGNMMKEBE;
		_writeTimestamps = AODCELGDHPO;
		_captureUnityLog = DHFCJMJBFDP;
		if (!string.IsNullOrEmpty(PMFEIPCHENB))
		{
			_fileName = PMFEIPCHENB;
		}
		EnsureInstance();
		_notInitialized = false;
		if (!Directory.Exists(KOBDDMHGOPJ))
		{
			Directory.CreateDirectory(KOBDDMHGOPJ);
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

	private static void _unityLogCallback(string IOFGGOCEIAM, string HHLCHHIFDCM, LogType LFLGCDNKNJI)
	{
		if (_captureUnityLog)
		{
			switch (LFLGCDNKNJI)
			{
			case LogType.Error:
				Error(IOFGGOCEIAM, HHLCHHIFDCM);
				break;
			case LogType.Assert:
				Assert(IOFGGOCEIAM, HHLCHHIFDCM);
				break;
			case LogType.Warning:
				Warning(IOFGGOCEIAM, HHLCHHIFDCM);
				break;
			case LogType.Log:
				Log(IOFGGOCEIAM, HHLCHHIFDCM);
				break;
			case LogType.Exception:
				LogException(IOFGGOCEIAM, HHLCHHIFDCM);
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

	public static void Log(object LIOGIBJBHAH, string HHLCHHIFDCM = null)
	{
		AppendEntry(LogLevel.Log, LIOGIBJBHAH, HHLCHHIFDCM);
	}

	public static void Warning(object LIOGIBJBHAH, string HHLCHHIFDCM = null)
	{
		AppendEntry(LogLevel.Warning, LIOGIBJBHAH, HHLCHHIFDCM);
	}

	public static void Error(object LIOGIBJBHAH, string HHLCHHIFDCM = null)
	{
		AppendEntry(LogLevel.Error, LIOGIBJBHAH, HHLCHHIFDCM);
	}

	public static void Exception(Exception MPFFFAOGBJE)
	{
		AppendEntry(LogLevel.Exception, MPFFFAOGBJE.Message, MPFFFAOGBJE.StackTrace);
	}

	private static void LogException(object LIOGIBJBHAH, string HHLCHHIFDCM)
	{
		AppendEntry(LogLevel.Exception, LIOGIBJBHAH, HHLCHHIFDCM);
	}

	public static void Assert(object LIOGIBJBHAH, string HHLCHHIFDCM = null)
	{
		AppendEntry(LogLevel.Assert, LIOGIBJBHAH, HHLCHHIFDCM);
	}

	public static void Stop()
	{
		_instance.Write();
		_notInitialized = true;
	}

	private static string FormatEntry(LogLevel GNLOCMLBNHF, object IOFGGOCEIAM, string HHLCHHIFDCM = null)
	{
		object obj = ((!string.IsNullOrEmpty(HHLCHHIFDCM)) ? string.Format("{0} at: {1}", IOFGGOCEIAM, HHLCHHIFDCM) : IOFGGOCEIAM);
		if (!_writeTimestamps)
		{
			return string.Format("{0}\n", obj);
		}
		return string.Format("[{3}] [{0}:{1}:{2}] {4}\n", _now.Hour.ToString("00"), _now.Minute.ToString("00"), _now.Second.ToString("00"), GNLOCMLBNHF, obj);
	}

	private static void AppendEntry(LogLevel GNLOCMLBNHF, object LIOGIBJBHAH, string HHLCHHIFDCM = null)
	{
		if (_notInitialized)
		{
			AdvLog.LogWarning("you must init log system first!");
		}
		else if ((int)GNLOCMLBNHF >= (int)_minLevel)
		{
			EnsureInstance();
			lock (LOCKER)
			{
				_items.Append(FormatEntry(GNLOCMLBNHF, LIOGIBJBHAH, HHLCHHIFDCM));
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
