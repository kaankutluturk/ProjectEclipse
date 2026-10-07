using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public static class AdvLog
{
	public enum LogLevel
	{
		Log = 0,
		Warn = 1,
		Error = 2
	}

	private static bool _logNow;

	private static string _filePath;

	public static bool LoggingEnabled
	{
		get
		{
			return GetLogNow();
		}
		set
		{
			set_LogNow(value);
		}
	}

	public static bool GetLogNow()
	{
		return _logNow;
	}

	public static void set_LogNow(bool value)
	{
		if (_logNow != value)
		{
			_logNow = value;
			if (_logNow)
			{
				Init();
			}
		}
	}

	private static void Init()
	{
		if (string.IsNullOrEmpty(_filePath))
		{
			Application.logMessageReceived += ApplicationLogSubsctiption;
			_filePath = Path.Combine(GlobalPaths.GetExternalResourcesPath(), string.Format("log_{0}.log", DateTime.Now.ToString("yy_MM_dd__hh_mm_ss")));
			File.WriteAllText(_filePath, string.Empty);
		}
	}

	private static void ApplicationLogSubsctiption(string condition, string stackTrace, LogType logType)
	{
		switch (logType)
		{
		case LogType.Log:
			WriteToFile(LogLevel.Log, condition + " - " + stackTrace);
			break;
		case LogType.Warning:
			WriteToFile(LogLevel.Warn, condition + " - " + stackTrace);
			break;
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			WriteToFile(LogLevel.Error, condition + " - " + stackTrace);
			break;
		}
	}

	public static void EmailLog(string recipient, string senderName)
	{
		if (!string.IsNullOrEmpty(_filePath))
		{
			MailMessage mailMessage = new MailMessage();
			mailMessage.From = new MailAddress("logs@nekkimobile.ru", senderName);
			mailMessage.To.Add(recipient);
			mailMessage.Attachments.Add(new Attachment(_filePath));
			mailMessage.Subject = string.Format("Log from [{0}:{1}:{2}] {3}", SystemInfo.deviceModel, SystemInfo.deviceName, SystemInfo.deviceType, SystemInfo.deviceUniqueIdentifier);
			mailMessage.Body = "see log in attachment";
			SmtpClient smtpClient = new SmtpClient("mail.nekkimobile.ru");
			smtpClient.Port = 587;
			smtpClient.Credentials = new NetworkCredential("logs@nekkimobile.ru", "o99hSASo") as ICredentialsByHost;
			smtpClient.EnableSsl = true;
			ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
			smtpClient.Send(mailMessage);
			Debug.Log("success");
		}
	}

	private static void WriteToFile(LogLevel logLevel, object message)
	{
		if (message != null && !string.IsNullOrEmpty(message.ToString()) && GetLogNow())
		{
			PushToFile(string.Format("[{0}:{1}] {2}", DateTime.Now, logLevel, message));
		}
	}

	private static void PushToFile(string text)
	{
		File.AppendAllText(_filePath, text);
	}

	public static void Log(object message)
	{
		if (GetLogNow())
		{
			Debug.Log(message);
		}
	}

	public static void Log(object message, UnityEngine.Object context)
	{
		if (GetLogNow())
		{
			Debug.Log(message, context);
		}
	}

	public static void LogFormat(UnityEngine.Object context, string format, params object[] args)
	{
		if (GetLogNow())
		{
			Debug.LogFormat(context, format, args);
		}
	}

	public static void LogFormat(string format, params object[] args)
	{
		if (GetLogNow())
		{
			Debug.LogFormat(format, args);
		}
	}

	public static void LogWarning(object message)
	{
		if (GetLogNow())
		{
			Debug.LogWarning(message);
		}
	}

	public static void LogWarning(object message, UnityEngine.Object context)
	{
		if (GetLogNow())
		{
			Debug.LogWarning(message, context);
		}
	}

	public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
	{
		if (GetLogNow())
		{
			Debug.LogWarningFormat(context, format, args);
		}
	}

	public static void LogWarningFormat(string format, params object[] args)
	{
		if (GetLogNow())
		{
			Debug.LogWarningFormat(format, args);
		}
	}

	public static void LogError(object message)
	{
		if (GetLogNow())
		{
			Debug.LogError(message);
		}
	}

	public static void LogError(object message, UnityEngine.Object context)
	{
		if (GetLogNow())
		{
			Debug.LogError(message, context);
		}
	}

	public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
	{
		if (GetLogNow())
		{
			Debug.LogErrorFormat(context, format, args);
		}
	}

	public static void LogErrorFormat(string format, params object[] args)
	{
		if (GetLogNow())
		{
			Debug.LogErrorFormat(format, args);
		}
	}

	public static void LogException(Exception exception)
	{
		if (GetLogNow())
		{
			Debug.LogException(exception);
		}
	}

	public static void LogException(Exception exception, UnityEngine.Object context)
	{
		if (GetLogNow())
		{
			Debug.LogException(exception, context);
		}
	}

	public static void Assert(bool condition)
	{
		if (GetLogNow())
		{
		}
	}

	public static void Assert(bool condition, string message)
	{
		if (GetLogNow())
		{
		}
	}

	public static void Assert(bool condition, string format, params object[] args)
	{
		if (GetLogNow())
		{
		}
	}
}
