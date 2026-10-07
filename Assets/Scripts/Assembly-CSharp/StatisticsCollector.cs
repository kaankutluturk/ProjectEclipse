using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Nekki.SF2.Core;
using Nekki.SF2.Core.Network;
using Nekki.Utils;
using SimpleJSON;
using UnityEngine;

public class StatisticsCollector
{
	private enum LoggingState
	{
		NotLogging = 0,
		Logging = 1,
		Undecided = 2
	}

	public enum CurrencyType
	{
		Money = 0,
		Bonus = 1
	}

	private const int MaxLogLines = 200;

	private static StatisticsCollector _Current;

	private const string FullEventsFileName = "full_events.json";

	private const string EventsFileName = "events.json";

	private const string PaysFileName = "pays.json";

	private const string DataFileName = "events_data.xml";

	private const int MaxSendBytes = 2000000;

	private int _EventId;

	private int _PayEventId;

	private int _RunCount;

	private int _SessionId;

	private int _FightAmount;

	private int _SessionLength;

	private long _EndBonus;

	private long _EndDate;

	private int _EndLevel;

	private long _EndMoney;

	private uint _EndXp;

	private int _EndEnergy;

	private int _Payments;

	private string _CheatId;

	private string _Info;

	private int _Counter;

	private long _EventsFilePosition;

	private long _PaysFilePosition;

	private LoggingState _EventsLoggingState = LoggingState.Undecided;

	private LoggingState _PaysLoggingState = LoggingState.Undecided;

	private StringBuilder _EventsBuffer = new StringBuilder();

	private StringBuilder _PaysBuffer = new StringBuilder();

	private long _LastSendTime;

	private const int _DeltaSendTime = 900000;

	private long _FullLogPosition;

	private static string FullEventsPath
	{
		get
		{
			return GetFullEventsPath();
		}
	}

	private static string EventsPath
	{
		get
		{
			return GetEventsPath();
		}
	}

	private static string PaysPath
	{
		get
		{
			return GetPaysPath();
		}
	}

	private static string DataFilePath
	{
		get
		{
			return GetDataFilePath();
		}
	}

	public static StatisticsCollector Instance
	{
		get
		{
			return GetInstance();
		}
	}

	public static int NextEventId
	{
		get
		{
			return GetNextEventId();
		}
	}

	public static int NextPayEventId
	{
		get
		{
			return GetNextPayEventId();
		}
	}

	public static int SessionId
	{
		get
		{
			return GetSessionId();
		}
		set
		{
			SetSessionId(value);
		}
	}

	public static int FightAmount
	{
		get
		{
			return GetFightAmount();
		}
		set
		{
			SetFightAmount(value);
		}
	}

	public static int SessionLength
	{
		get
		{
			return GetSessionLength();
		}
		set
		{
			SetSessionLength(value);
		}
	}

	public static long EndBonus
	{
		get
		{
			return GetEndBonus();
		}
	}

	public static long EndDate
	{
		get
		{
			return GetEndDate();
		}
	}

	public static int EndLevel
	{
		get
		{
			return GetEndLevel();
		}
	}

	public static long EndMoney
	{
		get
		{
			return GetEndMoney();
		}
	}

	public static uint EndXp
	{
		get
		{
			return GetEndXp();
		}
	}

	public static int EndEnergy
	{
		get
		{
			return GetEndEnergy();
		}
	}

	public static int Payments
	{
		get
		{
			return GetPayments();
		}
	}

	public static string CheatId
	{
		get
		{
			return GetCheatId();
		}
	}

	public static string Info
	{
		get
		{
			return GetInfo();
		}
	}

	public static int CounterValue
	{
		get
		{
			return GetCounter();
		}
	}

	private StatisticsCollector()
	{
		LoadData();
		GlobalTimer.get_Instance().addEventListener(0, OnTimerTick);
		ApplicationController.add_OnPause(OnApplicationPause);
		Send();
	}

	private static string GetFullEventsPath()
	{
		return SF2Paths.GetStatisticsPath() + "/full_events.json";
	}

	private static string GetEventsPath()
	{
		return SF2Paths.GetStatisticsPath() + "/events.json";
	}

	private static string GetPaysPath()
	{
		return SF2Paths.GetStatisticsPath() + "/pays.json";
	}

	private static string GetDataFilePath()
	{
		return SF2Paths.GetStatisticsPath() + "/events_data.xml";
	}

	public static StatisticsCollector GetInstance()
	{
		if (_Current == null)
		{
			_Current = new StatisticsCollector();
		}
		return _Current;
	}

	public static int GetNextEventId()
	{
		GetInstance()._EventId++;
		_Current.SaveData();
		return _Current._EventId;
	}

	public static int GetNextPayEventId()
	{
		GetInstance()._PayEventId++;
		_Current.SaveData();
		return _Current._PayEventId;
	}

	public static int GetSessionId()
	{
		return GetInstance()._SessionId;
	}

	public static void SetSessionId(int value)
	{
		GetInstance()._SessionId = value;
		_Current.SaveData();
	}

	public static int GetFightAmount()
	{
		return GetInstance()._FightAmount;
	}

	public static void SetFightAmount(int value)
	{
		GetInstance()._FightAmount = value;
		_Current.SaveData();
	}

	public static int GetSessionLength()
	{
		return GetInstance()._SessionLength;
	}

	public static void SetSessionLength(int value)
	{
		GetInstance()._SessionLength = value;
		_Current.SaveData();
	}

	public static long GetEndBonus()
	{
		return GetInstance()._EndBonus;
	}

	public static long GetEndDate()
	{
		return GetInstance()._EndDate;
	}

	public static int GetEndLevel()
	{
		return GetInstance()._EndLevel;
	}

	public static long GetEndMoney()
	{
		return GetInstance()._EndMoney;
	}

	public static uint GetEndXp()
	{
		return GetInstance()._EndXp;
	}

	public static int GetEndEnergy()
	{
		return GetInstance()._EndEnergy;
	}

	public static int GetPayments()
	{
		return GetInstance()._Payments;
	}

	public static string GetCheatId()
	{
		return GetInstance()._CheatId;
	}

	public static string GetInfo()
	{
		return GetInstance()._Info;
	}

	public static int GetCounter()
	{
		return GetInstance()._Counter;
	}

	public static void LogEvent(StatisticsEvent.EventType IGABHEMGKKE, ArgsDict LKIOKGCNKHE = null)
	{
		// Remote analytics removed. Combat/stat counters elsewhere remain local.
	}

	public static void LogPayEvent(StatisticsEvent.EventType IGABHEMGKKE, ArgsDict LKIOKGCNKHE = null)
	{
		// Remote analytics removed. Combat/stat counters elsewhere remain local.
	}

	public void SetEventsLogging(bool MAACIEHOLML)
	{
		if (MAACIEHOLML)
		{
			_EventsLoggingState = LoggingState.Logging;
			Send();
		}
		else
		{
			_EventsLoggingState = LoggingState.NotLogging;
		}
	}

	public void SetPaysLogging(bool MAACIEHOLML)
	{
		if (MAACIEHOLML)
		{
			_PaysLoggingState = LoggingState.Logging;
			SendPayLog();
		}
		else
		{
			_PaysLoggingState = LoggingState.NotLogging;
		}
	}

	private void LoadData()
	{
		if (!File.Exists(GetDataFilePath()))
		{
			_EventId = 0;
			_PayEventId = 0;
			_RunCount = 0;
			_SessionId = 0;
			_FightAmount = 0;
			_SessionLength = 0;
			_EndBonus = 0L;
			_EndDate = 0L;
			_EndLevel = 0;
			_EndMoney = 0L;
			_EndXp = 0u;
			_EndEnergy = 0;
			_Payments = 0;
			_CheatId = "0";
			_Info = string.Empty;
			_Counter = 0;
		}
		else
		{
			XmlDocument xmlDocument = XmlUtils.OpenXMLDocument(GetDataFilePath(), string.Empty, XmlUtils.XmlSourceMode.ForcedExternal);
			XmlNode xmlNode = xmlDocument["Data"];
			_EventId = XmlUtils.ParseInt(xmlNode["EventID"].Attribute("Value"));
			_PayEventId = XmlUtils.ParseInt(xmlNode["PayEventID"].Attribute("Value"));
			_RunCount = XmlUtils.ParseInt(xmlNode["RunCount"].Attribute("Value"));
			_SessionId = XmlUtils.ParseInt(xmlNode["SessionID"].Attribute("Value"));
			_FightAmount = XmlUtils.ParseInt(xmlNode["FightAmount"].Attribute("Value"));
			_SessionLength = XmlUtils.ParseInt(xmlNode["Length"].Attribute("Value"));
			_EndBonus = XmlUtils.ParseLong(xmlNode["EndBonus"].Attribute("Value"), 0L);
			_EndDate = XmlUtils.ParseInt(xmlNode["EndDate"].Attribute("Value"));
			_EndLevel = XmlUtils.ParseInt(xmlNode["EndLevel"].Attribute("Value"));
			_EndMoney = XmlUtils.ParseLong(xmlNode["EndMoney"].Attribute("Value"), 0L);
			_EndXp = XmlUtils.ParseUint(xmlNode["EndXp"].Attribute("Value"));
			_EndEnergy = XmlUtils.ParseInt(xmlNode["EndEnergy"].Attribute("Value"));
			_Payments = XmlUtils.ParseInt(xmlNode["Payments"].Attribute("Value"));
			_CheatId = XmlUtils.ParseString(xmlNode["CheatId"].Attribute("Value"), "0");
			_Info = XmlUtils.ParseString(xmlNode["Info"].Attribute("Value"), string.Empty);
			_Counter = XmlUtils.ParseInt(xmlNode["Counter"].Attribute("Value"));
			_EventsFilePosition = XmlUtils.ParseInt(xmlNode["FilePosition"].Attribute("Value"));
			_PaysFilePosition = XmlUtils.ParseInt(xmlNode["PayLogPosition"].Attribute("Value"));
		}
	}

	private void SaveData()
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null));
		XmlElement xmlElement = xmlDocument.CreateElement("Data");
		xmlDocument.AppendChild(xmlElement);
		XmlElement xmlElement2 = xmlDocument.CreateElement("EventID");
		xmlElement.AppendChild(xmlElement2);
		xmlElement2.SetAttribute("Value", _EventId.ToString());
		XmlElement newChild = xmlDocument.CreateElement("PayEventID");
		xmlElement.AppendChild(newChild);
		xmlElement2.SetAttribute("Value", _PayEventId.ToString());
		XmlElement xmlElement3 = xmlDocument.CreateElement("RunCount");
		xmlElement.AppendChild(xmlElement3);
		xmlElement3.SetAttribute("Value", _RunCount.ToString());
		XmlElement xmlElement4 = xmlDocument.CreateElement("SessionID");
		xmlElement.AppendChild(xmlElement4);
		xmlElement4.SetAttribute("Value", _SessionId.ToString());
		XmlElement xmlElement5 = xmlDocument.CreateElement("FightAmount");
		xmlElement.AppendChild(xmlElement5);
		xmlElement5.SetAttribute("Value", _FightAmount.ToString());
		XmlElement xmlElement6 = xmlDocument.CreateElement("TimeLength");
		xmlElement.AppendChild(xmlElement6);
		xmlElement6.SetAttribute("Value", _SessionLength.ToString());
		XmlElement xmlElement7 = xmlDocument.CreateElement("EndBonus");
		xmlElement.AppendChild(xmlElement7);
		xmlElement7.SetAttribute("Value", _EndBonus.ToString());
		XmlElement xmlElement8 = xmlDocument.CreateElement("EndDate");
		xmlElement.AppendChild(xmlElement8);
		xmlElement8.SetAttribute("Value", _EndDate.ToString());
		XmlElement xmlElement9 = xmlDocument.CreateElement("EndLevel");
		xmlElement.AppendChild(xmlElement9);
		xmlElement9.SetAttribute("Value", _EndLevel.ToString());
		XmlElement xmlElement10 = xmlDocument.CreateElement("EndMoney");
		xmlElement.AppendChild(xmlElement10);
		xmlElement10.SetAttribute("Value", _EndMoney.ToString());
		XmlElement xmlElement11 = xmlDocument.CreateElement("EndXp");
		xmlElement.AppendChild(xmlElement11);
		xmlElement11.SetAttribute("Value", _EndXp.ToString());
		XmlElement xmlElement12 = xmlDocument.CreateElement("EndEnergy");
		xmlElement.AppendChild(xmlElement12);
		xmlElement12.SetAttribute("Value", _EndEnergy.ToString());
		XmlElement xmlElement13 = xmlDocument.CreateElement("Payments");
		xmlElement.AppendChild(xmlElement13);
		xmlElement13.SetAttribute("Value", _Payments.ToString());
		XmlElement xmlElement14 = xmlDocument.CreateElement("CheatId");
		xmlElement.AppendChild(xmlElement14);
		xmlElement14.SetAttribute("Value", _CheatId.ToString());
		XmlElement xmlElement15 = xmlDocument.CreateElement("Info");
		xmlElement.AppendChild(xmlElement15);
		xmlElement15.SetAttribute("Value", _Info.ToString());
		XmlElement xmlElement16 = xmlDocument.CreateElement("Counter");
		xmlElement.AppendChild(xmlElement16);
		xmlElement16.SetAttribute("Value", _Counter.ToString());
		XmlElement xmlElement17 = xmlDocument.CreateElement("FilePosition");
		xmlElement.AppendChild(xmlElement17);
		xmlElement17.SetAttribute("Value", _EventsFilePosition.ToString());
		XmlElement xmlElement18 = xmlDocument.CreateElement("PayLogPosition");
		xmlElement.AppendChild(xmlElement18);
		xmlElement18.SetAttribute("Value", _PaysFilePosition.ToString());
		if (!Directory.Exists(SF2Paths.GetStatisticsPath()))
		{
			Directory.CreateDirectory(SF2Paths.GetStatisticsPath());
		}
		xmlDocument.Save(GetDataFilePath());
	}

	private void SaveFullLog()
	{
		try
		{
			FileInfo fileInfo = new FileInfo(GetFullEventsPath());
			if (!fileInfo.Exists)
			{
				fileInfo.Create().Close();
			}
			FileInfo fileInfo2 = new FileInfo(GetEventsPath());
			if (fileInfo2.Exists)
			{
				StreamWriter streamWriter = fileInfo.AppendText();
				StreamReader streamReader = fileInfo2.OpenText();
				while (!streamReader.EndOfStream)
				{
					streamWriter.WriteLine(streamReader.ReadLine());
				}
				streamWriter.Close();
				streamReader.Close();
				fileInfo2.Create().Close();
			}
		}
		catch (Exception ex)
		{
			GameLog.Error(ex.Message);
		}
	}

	private void SaveFullLog(string MMEHKDAMJBF)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(GetFullEventsPath());
			if (!fileInfo.Exists)
			{
				fileInfo.Create().Close();
			}
			using (StreamWriter streamWriter = fileInfo.AppendText())
			{
				streamWriter.Write(MMEHKDAMJBF);
			}
		}
		catch (Exception ex)
		{
			GameLog.Error(ex.Message);
		}
	}

	private void FlushEventsBuffer()
	{
		FlushBuffer(_EventsBuffer, GetEventsPath());
	}

	private void FlushPaysBuffer()
	{
		FlushBuffer(_PaysBuffer, GetPaysPath());
	}

	private void FlushBuffer(StringBuilder Data, string PDLAFCOODMM)
	{
		if (Data.Length == 0)
		{
			return;
		}
		string value = Data.ToString();
		Data.Length = 0;
		try
		{
			if (!Directory.Exists(SF2Paths.GetStatisticsPath()))
			{
				Directory.CreateDirectory(SF2Paths.GetStatisticsPath());
			}
			FileInfo fileInfo = new FileInfo(PDLAFCOODMM);
			if (!fileInfo.Exists)
			{
				FileStream fileStream = fileInfo.Create();
				fileStream.Close();
			}
			StreamWriter streamWriter = fileInfo.AppendText();
			if (fileInfo.Exists && fileInfo.Length != 0)
			{
				streamWriter.Write("\n");
			}
			streamWriter.Write(value);
			streamWriter.Close();
		}
		catch (Exception ex)
		{
			GameLog.Error(ex.Message);
		}
	}

	public void SendFullLog()
	{
		int pCOENEHCGNI = 2000000;
		FlushEventsBuffer();
		SaveFullLog();
		Send(GetFullEventsPath(), ref _FullLogPosition, (bool AMKKLMOONEP, string GHDPPHAAPCA, object JHJDJOFPHPH, string MDIJEPEOAJH) =>
		{
			OnFullLogSent(AMKKLMOONEP, GHDPPHAAPCA, JHJDJOFPHPH, MDIJEPEOAJH);
		}, pCOENEHCGNI, false, "save_full_json_log");
	}

	public void OnFullLogSent(bool AMKKLMOONEP, string GHDPPHAAPCA, object JHJDJOFPHPH, string CCNACAJIIGA)
	{
		if (AMKKLMOONEP)
		{
			JSONNode jSONNode = JSON.Parse(GHDPPHAAPCA);
			if (jSONNode != null && jSONNode["data"] != null && jSONNode["data"].Value == "ok")
			{
				SendOnNextFrame(SendFullLog);
			}
		}
	}

	private void SendPayLog()
	{
		if (_PaysLoggingState != LoggingState.NotLogging && _PaysLoggingState != LoggingState.Undecided)
		{
			int pCOENEHCGNI = 2000000;
			Send(GetPaysPath(), ref _PaysFilePosition, (bool AMKKLMOONEP, string GHDPPHAAPCA, object JHJDJOFPHPH, string MDIJEPEOAJH) =>
			{
				OnPayLogSent(AMKKLMOONEP, GHDPPHAAPCA, JHJDJOFPHPH, MDIJEPEOAJH);
			}, pCOENEHCGNI, false, "save_pay_log");
		}
	}

	public void OnPayLogSent(bool AMKKLMOONEP, string GHDPPHAAPCA, object JHJDJOFPHPH, string CCNACAJIIGA)
	{
		if (AMKKLMOONEP)
		{
			JSONNode jSONNode = JSON.Parse(GHDPPHAAPCA);
			if (jSONNode != null && jSONNode["data"] != null && jSONNode["data"].Value == "ok")
			{
				ClearPayLogIfSent();
				SendOnNextFrame(SendPayLog);
			}
		}
	}

	private void ClearPayLogIfSent()
	{
		FlushPaysBuffer();
		FileInfo fileInfo = new FileInfo(GetPaysPath());
		if (fileInfo.Exists)
		{
			bool flag = false;
			StreamReader streamReader = fileInfo.OpenText();
			flag = _PaysFilePosition >= streamReader.BaseStream.Length;
			streamReader.Close();
			if (flag)
			{
				fileInfo.Create().Close();
				_PaysFilePosition = 0L;
				SaveData();
			}
		}
	}

	private void Send()
	{
		int pCOENEHCGNI = 2000000;
		if (_EventsLoggingState != LoggingState.NotLogging && _EventsLoggingState != LoggingState.Undecided)
		{
			_LastSendTime = GameTimeUtils.GetUnixTimeMs();
			FlushEventsBuffer();
			Send(GetEventsPath(), ref _EventsFilePosition, (bool AMKKLMOONEP, string GHDPPHAAPCA, object JHJDJOFPHPH, string MDIJEPEOAJH) =>
			{
				OnEventsSent(AMKKLMOONEP, GHDPPHAAPCA, JHJDJOFPHPH, MDIJEPEOAJH);
			}, pCOENEHCGNI);
		}
	}

	private void Send(string EFGLOMANJHN, ref long DNGJNMNHIOB, Action<bool, string, object, string> p_delegate, int PCOENEHCGNI = 2000000, bool AOOKEDHEDHJ = true, string IBODMPMJELJ = "save_json_log")
	{
		// No telemetry transport.
	}

	public void OnEventsSent(bool AMKKLMOONEP, string GHDPPHAAPCA, object JHJDJOFPHPH, string CCNACAJIIGA)
	{
		if (AMKKLMOONEP)
		{
			JSONNode jSONNode = JSON.Parse(GHDPPHAAPCA);
			if (jSONNode != null && jSONNode["data"] != null && jSONNode["data"].Value == "ok")
			{
				SaveFullLog(CCNACAJIIGA);
				ClearEventsLogIfSent();
			}
		}
	}

	private void ClearEventsLogIfSent()
	{
		FlushEventsBuffer();
		FileInfo fileInfo = new FileInfo(GetEventsPath());
		if (fileInfo.Exists)
		{
			bool flag = false;
			StreamReader streamReader = fileInfo.OpenText();
			flag = _EventsFilePosition >= streamReader.BaseStream.Length;
			streamReader.Close();
			if (flag)
			{
				fileInfo.Create().Close();
				_EventsFilePosition = 0L;
				SaveData();
			}
			else
			{
				SendOnNextFrame(Send);
			}
		}
	}

	private void SendOnNextFrame(Action IBODMPMJELJ)
	{
		ServerProvider.get_Instance().StartCoroutine(SendOnNextFrameCorutine(IBODMPMJELJ));
	}

	private IEnumerator SendOnNextFrameCorutine(Action IBODMPMJELJ)
	{
		yield return new WaitForEndOfFrame();
		IBODMPMJELJ();
	}

	private void TrimEventsLog()
	{
		if (!File.Exists(GetEventsPath()))
		{
			return;
		}
		string[] array = File.ReadAllLines(GetEventsPath());
		if (array.Length <= 200)
		{
			return;
		}
		int count = array.Length - 200;
		IEnumerable<string> enumerable = array.Skip(count);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in enumerable)
		{
			stringBuilder.AppendLine(item);
		}
		stringBuilder.Length--;
		File.WriteAllText(GetEventsPath(), stringBuilder.ToString());
		_LastSendTime = GameTimeUtils.GetUnixTimeMs();
	}

	public void OnTimerTick(object data)
	{
		_SessionLength++;
	}

	public void OnApplicationPause(bool FILCEHABKLK)
	{
		if (!FILCEHABKLK && GameTimeUtils.GetUnixTimeMs() - _LastSendTime > 900000)
		{
			Send();
		}
		if (FILCEHABKLK)
		{
			SaveEndState();
		}
	}

	private void SaveEndState()
	{
		_EndDate = GlobalTimer.get_LocalTimeUTC();
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP != null)
		{
			_EndBonus = ListSF.GetRoster().GetBonus();
			_EndLevel = ListSF.GetRoster().GetLevel();
			_EndMoney = ListSF.GetRoster().GetMoney();
			_EndXp = ListSF.GetRoster().GetExperience();
			_EndEnergy = ListSF.GetRoster().GetMaxPower();
		}
		_CheatId = "0";
		_Info = string.Empty;
		_Counter = 0;
		SaveData();
	}
}
