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

	private static void ApplicationLogSubsctiption(string IOFGGOCEIAM, string HHLCHHIFDCM, LogType LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
		case LogType.Log:
			WriteToFile(LogLevel.Log, IOFGGOCEIAM + " - " + HHLCHHIFDCM);
			break;
		case LogType.Warning:
			WriteToFile(LogLevel.Warn, IOFGGOCEIAM + " - " + HHLCHHIFDCM);
			break;
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			WriteToFile(LogLevel.Error, IOFGGOCEIAM + " - " + HHLCHHIFDCM);
			break;
		}
	}

	public static void EmailLog(string CCELBJICOKG, string IEJOMILJAOK)
	{
		if (!string.IsNullOrEmpty(_filePath))
		{
			MailMessage mailMessage = new MailMessage();
			mailMessage.From = new MailAddress("logs@nekkimobile.ru", IEJOMILJAOK);
			mailMessage.To.Add(CCELBJICOKG);
			mailMessage.Attachments.Add(new Attachment(_filePath));
			mailMessage.Subject = string.Format("Log from [{0}:{1}:{2}] {3}", SystemInfo.deviceModel, SystemInfo.deviceName, SystemInfo.deviceType, SystemInfo.deviceUniqueIdentifier);
			mailMessage.Body = "see log in attachment";
			SmtpClient smtpClient = new SmtpClient("mail.nekkimobile.ru");
			smtpClient.Port = 587;
			smtpClient.Credentials = new NetworkCredential("logs@nekkimobile.ru", "o99hSASo") as ICredentialsByHost;
			smtpClient.EnableSsl = true;
			ServicePointManager.ServerCertificateValidationCallback = (object JDCCBCNFENK, X509Certificate POHBEPBAMIO, X509Chain GCONPBMJDFL, SslPolicyErrors BFOEIHJDKEL) => true;
			smtpClient.Send(mailMessage);
			Debug.Log("success");
		}
	}

	private static void WriteToFile(LogLevel GNLOCMLBNHF, object LIOGIBJBHAH)
	{
		if (LIOGIBJBHAH != null && !string.IsNullOrEmpty(LIOGIBJBHAH.ToString()) && GetLogNow())
		{
			PushToFile(string.Format("[{0}:{1}] {2}", DateTime.Now, GNLOCMLBNHF, LIOGIBJBHAH));
		}
	}

	private static void PushToFile(string LIOGIBJBHAH)
	{
		File.AppendAllText(_filePath, LIOGIBJBHAH);
	}

	public static void Log(object LIOGIBJBHAH)
	{
		if (GetLogNow())
		{
			Debug.Log(LIOGIBJBHAH);
		}
	}

	public static void Log(object LIOGIBJBHAH, UnityEngine.Object PDCAHMPCPOC)
	{
		if (GetLogNow())
		{
			Debug.Log(LIOGIBJBHAH, PDCAHMPCPOC);
		}
	}

	public static void LogFormat(UnityEngine.Object PDCAHMPCPOC, string LBOHOKIBHOH, params object[] LKIOKGCNKHE)
	{
		if (GetLogNow())
		{
			Debug.LogFormat(PDCAHMPCPOC, LBOHOKIBHOH, LKIOKGCNKHE);
		}
	}

	public static void LogFormat(string LBOHOKIBHOH, params object[] LKIOKGCNKHE)
	{
		if (GetLogNow())
		{
			Debug.LogFormat(LBOHOKIBHOH, LKIOKGCNKHE);
		}
	}

	public static void LogWarning(object LIOGIBJBHAH)
	{
		if (GetLogNow())
		{
			Debug.LogWarning(LIOGIBJBHAH);
		}
	}

	public static void LogWarning(object LIOGIBJBHAH, UnityEngine.Object PDCAHMPCPOC)
	{
		if (GetLogNow())
		{
			Debug.LogWarning(LIOGIBJBHAH, PDCAHMPCPOC);
		}
	}

	public static void LogWarningFormat(UnityEngine.Object PDCAHMPCPOC, string LBOHOKIBHOH, params object[] LKIOKGCNKHE)
	{
		if (GetLogNow())
		{
			Debug.LogWarningFormat(PDCAHMPCPOC, LBOHOKIBHOH, LKIOKGCNKHE);
		}
	}

	public static void LogWarningFormat(string LBOHOKIBHOH, params object[] LKIOKGCNKHE)
	{
		if (GetLogNow())
		{
			Debug.LogWarningFormat(LBOHOKIBHOH, LKIOKGCNKHE);
		}
	}

	public static void LogError(object LIOGIBJBHAH)
	{
		if (GetLogNow())
		{
			Debug.LogError(LIOGIBJBHAH);
		}
	}

	public static void LogError(object LIOGIBJBHAH, UnityEngine.Object PDCAHMPCPOC)
	{
		if (GetLogNow())
		{
			Debug.LogError(LIOGIBJBHAH, PDCAHMPCPOC);
		}
	}

	public static void LogErrorFormat(UnityEngine.Object PDCAHMPCPOC, string LBOHOKIBHOH, params object[] LKIOKGCNKHE)
	{
		if (GetLogNow())
		{
			Debug.LogErrorFormat(PDCAHMPCPOC, LBOHOKIBHOH, LKIOKGCNKHE);
		}
	}

	public static void LogErrorFormat(string LBOHOKIBHOH, params object[] LKIOKGCNKHE)
	{
		if (GetLogNow())
		{
			Debug.LogErrorFormat(LBOHOKIBHOH, LKIOKGCNKHE);
		}
	}

	public static void LogException(Exception MPFFFAOGBJE)
	{
		if (GetLogNow())
		{
			Debug.LogException(MPFFFAOGBJE);
		}
	}

	public static void LogException(Exception MPFFFAOGBJE, UnityEngine.Object PDCAHMPCPOC)
	{
		if (GetLogNow())
		{
			Debug.LogException(MPFFFAOGBJE, PDCAHMPCPOC);
		}
	}

	public static void Assert(bool IOFGGOCEIAM)
	{
		if (GetLogNow())
		{
		}
	}

	public static void Assert(bool IOFGGOCEIAM, string LIOGIBJBHAH)
	{
		if (GetLogNow())
		{
		}
	}

	public static void Assert(bool IOFGGOCEIAM, string LBOHOKIBHOH, params object[] LKIOKGCNKHE)
	{
		if (GetLogNow())
		{
		}
	}
}
