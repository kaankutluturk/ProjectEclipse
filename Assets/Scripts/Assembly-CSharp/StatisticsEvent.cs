using System;
using SimpleJSON;

public static class StatisticsEvent
{
	public enum PayStatus
	{
		Purchased = 0,
		Restored = 1,
		Cancelled = 2,
		Failed = 3,
		Pending = 4,
		Already_Owned = 5
	}

	public enum VerificationStatus
	{
		Start = 0,
		Finish = 1,
		Confirm_Start = 2,
		Confirm_Finish = 3
	}

	public enum EventType
	{
		Unknown = 0,
		User = 1,
		Fight_End = 2,
		Achievement = 3,
		Gems_Changed = 4,
		Level_Up = 5,
		Purchase = 6,
		Perk = 7,
		Session_End = 8,
		Payment = 9,
		Discount = 10,
		Enchantment = 11,
		Set_Acquisition = 12,
		Raid_Timeout = 13,
		Pay_Request_Start = 14,
		Pay_Status_Update = 15,
		Pay_Handle_Start = 16,
		Pay_Handle_Finish = 17,
		Pay_Transaction_Finish = 18,
		Pay_Handle_Error = 19,
		Pay_Handle_Debug = 20,
		Pay_Request_Fail = 21,
		Pay_Verification_Status_Change = 22
	}

	private static int Timestamp
	{
		get
		{
			return GetUnixTimestamp();
		}
	}

	private static string PlatformCode
	{
		get
		{
			return GetPlatformCode();
		}
	}

	public static bool IsEventEnabled(EventType IGABHEMGKKE)
	{
		return true;
	}

	public static bool IsEventLogged(EventType IGABHEMGKKE)
	{
		return true;
	}

	public static string BuildEventJson(EventType IGABHEMGKKE, ArgsDict PCJAKPJMKGN)
	{
		switch (IGABHEMGKKE)
		{
		case EventType.User:
			ResetSessionCounters();
			return BuildUserJson(PCJAKPJMKGN);
		case EventType.Fight_End:
			StatisticsCollector.SetFightAmount(StatisticsCollector.GetFightAmount() + 1);
			return BuildFightEndJson(PCJAKPJMKGN);
		case EventType.Achievement:
			return BuildAchievementJson(PCJAKPJMKGN);
		case EventType.Gems_Changed:
			return BuildGemsChangedJson(PCJAKPJMKGN);
		case EventType.Level_Up:
			return BuildLevelUpJson(PCJAKPJMKGN);
		case EventType.Purchase:
			return BuildPurchaseJson(PCJAKPJMKGN);
		case EventType.Payment:
			return BuildPaymentJson(PCJAKPJMKGN);
		case EventType.Perk:
			return BuildPerkJson(PCJAKPJMKGN);
		case EventType.Session_End:
			StatisticsCollector.SetSessionId(StatisticsCollector.GetSessionId() + 1);
			return BuildSessionEndJson(PCJAKPJMKGN);
		case EventType.Pay_Request_Start:
			return BuildPayRequestStartJson(PCJAKPJMKGN);
		case EventType.Pay_Status_Update:
			return BuildPayStatusUpdateJson(PCJAKPJMKGN);
		case EventType.Pay_Handle_Start:
			return BuildPayHandleStartJson(PCJAKPJMKGN);
		case EventType.Pay_Handle_Finish:
			return BuildPayHandleFinishJson(PCJAKPJMKGN);
		case EventType.Pay_Transaction_Finish:
			return BuildPayTransactionFinishJson(PCJAKPJMKGN);
		case EventType.Pay_Handle_Error:
			return BuildPayHandleErrorJson(PCJAKPJMKGN);
		case EventType.Pay_Handle_Debug:
			return BuildPayHandleDebugJson(PCJAKPJMKGN);
		case EventType.Pay_Request_Fail:
			return BuildPayRequestFailJson(PCJAKPJMKGN);
		case EventType.Pay_Verification_Status_Change:
			return BuildPayVerificationJson(PCJAKPJMKGN);
		default:
			return null;
		}
	}

	private static JSONClass CreateHeadJson(EventType IGABHEMGKKE)
	{
		JSONClass jSONClass = new JSONClass();
		string text = GetEventTypeName(IGABHEMGKKE);
		jSONClass["eid"] = StatisticsCollector.GetNextEventId();
		jSONClass["etype"] = text;
		jSONClass["build_version"] = SystemProperties.GetVersion().ToString();
		jSONClass["data_version"] = SystemProperties.GetDataVersion().ToString(true);
		jSONClass["install_id"] = ListSF.GetRoster().GetInstallId();
		jSONClass["total_payment_sum"] = ListSF.GetRoster().GetTotalPaymentSum();
		jSONClass["timestamp"] = GetUnixTimestamp();
		jSONClass["paid_version"] = SystemProperties.IsPaidApp();
		if (SystemProperties.IsDebug())
		{
			AddTag(jSONClass, "debug");
		}
		if (SystemProperties.IsEditorPlatform())
		{
			AddTag(jSONClass, "simulator");
		}
		AddRosterData(jSONClass, text);
		return jSONClass;
	}

	private static JSONClass CreateHeadPayJSON(EventType GIGAFKGDKNH, string HOKDOMALLDB, string JDHJMKDOAMO)
	{
		return CreateHeadPayJSON(GetEventTypeName(GIGAFKGDKNH), HOKDOMALLDB, JDHJMKDOAMO);
	}

	private static JSONClass CreateHeadPayJSON(string GIGAFKGDKNH, string HOKDOMALLDB, string JDHJMKDOAMO)
	{
		JSONClass jSONClass = new JSONClass();
		jSONClass["eid"] = StatisticsCollector.GetNextPayEventId();
		jSONClass["etype"] = "pay";
		jSONClass["build_version"] = SystemProperties.GetVersion().ToString();
		jSONClass["data_version"] = SystemProperties.GetDataVersion().ToString(true);
		jSONClass["subtype"] = GIGAFKGDKNH;
		jSONClass["package_id"] = ((!string.IsNullOrEmpty(HOKDOMALLDB)) ? HOKDOMALLDB : string.Empty);
		jSONClass["rid"] = ((!string.IsNullOrEmpty(JDHJMKDOAMO)) ? MD5Utils.MD5HashString(JDHJMKDOAMO) : string.Empty);
		jSONClass["paid_version"] = SystemProperties.IsPaidApp();
		if (SystemProperties.IsDebug())
		{
			jSONClass["time"] = ListSF.GetCurrentTime().ToString("X");
		}
		return jSONClass;
	}

	private static void AddTag(JSONClass data, string EDLADAAKMDF)
	{
		JSONArray asArray = data["tags"].AsArray;
		asArray.Add(EDLADAAKMDF);
		data["tags"] = asArray;
	}

	private static void AddRosterData(JSONClass data, string DOPHKKGNAEF)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP != null)
		{
		}
	}

	private static string GetEventTypeName(EventType IGABHEMGKKE)
	{
		return IGABHEMGKKE.ToString().ToLower();
	}

	private static int GetUnixTimestamp()
	{
		return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
	}

	private static string GetPlatformCode()
	{
		if (SystemProperties.IsIosPlatform())
		{
			return "i";
		}
		if (SystemProperties.IsAndroidPlatform())
		{
			return "a";
		}
		return "u";
	}

	private static string BuildUserJson(ArgsDict PCJAKPJMKGN)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.User);
		jSONClass["device_token"] = SystemProperties.GetUserIdentifier();
		jSONClass["platform"] = SystemProperties.GetPlatformName();
		jSONClass["lang"] = SystemProperties.GetDeviceInfo().Locale;
		jSONClass["os"] = SystemProperties.GetOsVersion();
		jSONClass["device_name"] = SystemProperties.GetDeviceInfo().Id;
		jSONClass["level"] = ListSF.GetRoster().GetLevel();
		jSONClass["exp"] = ListSF.GetRoster().GetExperience().ToString();
		jSONClass["location"] = ListSF.GetRoster().GetCurrentZone();
		StatisticsGeter.AddCurrencies(jSONClass);
		StatisticsGeter.AddBalances(jSONClass);
		return jSONClass.ToString();
	}

	private static string BuildFightEndJson(ArgsDict PCJAKPJMKGN)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Fight_End);
		StatisticsGeter.AddFightInfo(jSONClass, PCJAKPJMKGN);
		StatisticsGeter.AddFightResult(jSONClass, PCJAKPJMKGN);
		StatisticsGeter.AddFpsLimit(jSONClass);
		StatisticsGeter.AddEclipseMode(jSONClass);
		StatisticsGeter.AddRounds(jSONClass, PCJAKPJMKGN);
		StatisticsGeter.AddAverageFps(jSONClass, PCJAKPJMKGN);
		StatisticsGeter.AddFightTimeElapsed(jSONClass, PCJAKPJMKGN);
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		jSONClass["device_name"] = SystemProperties.GetDeviceInfo().Id;
		StatisticsGeter.AddPerks(jSONClass);
		return jSONClass.ToString();
	}

	private static string BuildAchievementJson(ArgsDict PCJAKPJMKGN)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Achievement);
		string text = ((!PCJAKPJMKGN.ContainsKey("name")) ? string.Empty : PCJAKPJMKGN["name"].ToString());
		jSONClass["name"] = text;
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		return jSONClass.ToString();
	}

	private static string BuildGemsChangedJson(ArgsDict PCJAKPJMKGN)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Gems_Changed);
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		StatisticsGeter.AddGemsChange(jSONClass, PCJAKPJMKGN);
		StatisticsGeter.AddBalances(jSONClass);
		return jSONClass.ToString();
	}

	private static string BuildLevelUpJson(ArgsDict PCJAKPJMKGN)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Level_Up);
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		StatisticsGeter.AddLevelUpItems(jSONClass);
		StatisticsGeter.AddFights(jSONClass, ListSF.GetRoster().GetLevel() - 1);
		return jSONClass.ToString();
	}

	private static string BuildPurchaseJson(ArgsDict PCJAKPJMKGN)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Purchase);
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		StatisticsGeter.AddPurchaseInfo(jSONClass, PCJAKPJMKGN);
		StatisticsGeter.AddBalances(jSONClass);
		return jSONClass.ToString();
	}

	private static string BuildPaymentJson(ArgsDict PCJAKPJMKGN)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Payment);
		string text = PCJAKPJMKGN["item"].ToString();
		jSONClass["item"] = text;
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		jSONClass["price"] = (float)PCJAKPJMKGN["price"];
		jSONClass["price_currency"] = PCJAKPJMKGN["price_currency"].ToString();
		jSONClass["money_changed"] = (long)PCJAKPJMKGN["money_changed"];
		jSONClass["gems_paid_changed"] = (long)PCJAKPJMKGN["gems_paid_changed"];
		jSONClass["total_payment_count"] = ListSF.GetRoster().GetPaymentCount();
		StatisticsGeter.AddPriceUsd(jSONClass, text);
		StatisticsGeter.AddBalances(jSONClass);
		return jSONClass.ToString();
	}

	private static string BuildPerkJson(ArgsDict PCJAKPJMKGN)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Perk);
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		jSONClass["reset_count"] = ListSF.GetRoster().GetPerks().GetPerkResetCount();
		jSONClass["level_sum"] = ListSF.GetRoster().GetPerks().GetTotalPerkLevels();
		StatisticsGeter.AddPerks(jSONClass, "user_perks");
		StatisticsGeter.AddPerkChoice(jSONClass, PCJAKPJMKGN);
		return jSONClass.ToString();
	}

	private static string BuildSessionEndJson(ArgsDict PCJAKPJMKGN)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Session_End);
		jSONClass["session_id"] = StatisticsCollector.GetSessionId();
		jSONClass["fight_amount"] = StatisticsCollector.GetFightAmount();
		jSONClass["time_length"] = StatisticsCollector.GetSessionLength();
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		jSONClass["energy"] = StatisticsCollector.GetEndEnergy();
		return jSONClass.ToString();
	}

	private static string BuildPayRequestStartJson(ArgsDict PCJAKPJMKGN)
	{
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Request_Start, PCJAKPJMKGN["packageName"].ToString(), null);
		return jSONClass.ToString();
	}

	private static string BuildPayStatusUpdateJson(ArgsDict PCJAKPJMKGN)
	{
		string hOKDOMALLDB = PCJAKPJMKGN["packageName"].ToString();
		string text = PCJAKPJMKGN["receipt"].ToString();
		PayStatus fFFCCEEDMKI = (PayStatus)PCJAKPJMKGN["status"];
		int num = (int)PCJAKPJMKGN["resultCode"];
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Status_Update, hOKDOMALLDB, text);
		jSONClass["status"] = GetStatusName(fFFCCEEDMKI);
		jSONClass["result_code"] = num;
		jSONClass["receipt"] = ((!string.IsNullOrEmpty(text)) ? text : string.Empty);
		return jSONClass.ToString();
	}

	private static string BuildPayHandleStartJson(ArgsDict PCJAKPJMKGN)
	{
		string hOKDOMALLDB = PCJAKPJMKGN["packageName"].ToString();
		string jDHJMKDOAMO = PCJAKPJMKGN["receipt"].ToString();
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Handle_Start, hOKDOMALLDB, jDHJMKDOAMO);
		return jSONClass.ToString();
	}

	private static string BuildPayHandleFinishJson(ArgsDict PCJAKPJMKGN)
	{
		string hOKDOMALLDB = PCJAKPJMKGN["packageName"].ToString();
		string jDHJMKDOAMO = PCJAKPJMKGN["receipt"].ToString();
		long num = (long)PCJAKPJMKGN["money_changed"];
		long num2 = (long)PCJAKPJMKGN["gems_changed"];
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Handle_Finish, hOKDOMALLDB, jDHJMKDOAMO);
		jSONClass["moneyChanged"] = num;
		jSONClass["gemsChanged"] = num2;
		return jSONClass.ToString();
	}

	private static string BuildPayTransactionFinishJson(ArgsDict PCJAKPJMKGN)
	{
		string hOKDOMALLDB = PCJAKPJMKGN["packageName"].ToString();
		string jDHJMKDOAMO = PCJAKPJMKGN["receipt"].ToString();
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Transaction_Finish, hOKDOMALLDB, jDHJMKDOAMO);
		return jSONClass.ToString();
	}

	private static string BuildPayHandleErrorJson(ArgsDict PCJAKPJMKGN)
	{
		string hOKDOMALLDB = PCJAKPJMKGN["packageName"].ToString();
		string text = PCJAKPJMKGN["reason"].ToString();
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Handle_Error, hOKDOMALLDB, null);
		jSONClass["reason"] = text;
		return jSONClass.ToString();
	}

	private static string BuildPayHandleDebugJson(ArgsDict PCJAKPJMKGN)
	{
		string hOKDOMALLDB = PCJAKPJMKGN["packageName"].ToString();
		string text = PCJAKPJMKGN["message"].ToString();
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Handle_Debug, hOKDOMALLDB, null);
		jSONClass["message"] = text;
		return jSONClass.ToString();
	}

	private static string BuildPayRequestFailJson(ArgsDict PCJAKPJMKGN)
	{
		string hOKDOMALLDB = PCJAKPJMKGN["packageName"].ToString();
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Request_Fail, hOKDOMALLDB, null);
		return jSONClass.ToString();
	}

	private static string BuildPayVerificationJson(ArgsDict PCJAKPJMKGN)
	{
		string hOKDOMALLDB = PCJAKPJMKGN["packageName"].ToString();
		string jDHJMKDOAMO = PCJAKPJMKGN["receipt"].ToString();
		VerificationStatus fFFCCEEDMKI = (VerificationStatus)PCJAKPJMKGN["status"];
		string gIGAFKGDKNH = string.Format("pay_verify_{0}", GetStatusName(fFFCCEEDMKI));
		JSONClass jSONClass = CreateHeadPayJSON(gIGAFKGDKNH, hOKDOMALLDB, jDHJMKDOAMO);
		return jSONClass.ToString();
	}

	private static string GetStatusName(PayStatus status)
	{
		return status.ToString().ToLower();
	}

	private static string GetStatusName(VerificationStatus status)
	{
		return status.ToString().ToLower();
	}

	private static void ResetSessionCounters()
	{
		StatisticsCollector.SetFightAmount(0);
		StatisticsCollector.SetSessionLength(0);
	}
}
