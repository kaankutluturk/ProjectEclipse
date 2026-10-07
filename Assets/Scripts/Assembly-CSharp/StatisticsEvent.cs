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

	public static bool IsEventEnabled(EventType eventType)
	{
		return true;
	}

	public static bool IsEventLogged(EventType eventType)
	{
		return true;
	}

	public static string BuildEventJson(EventType eventType, ArgsDict eventArgs)
	{
		switch (eventType)
		{
		case EventType.User:
			ResetSessionCounters();
			return BuildUserJson(eventArgs);
		case EventType.Fight_End:
			StatisticsCollector.SetFightAmount(StatisticsCollector.GetFightAmount() + 1);
			return BuildFightEndJson(eventArgs);
		case EventType.Achievement:
			return BuildAchievementJson(eventArgs);
		case EventType.Gems_Changed:
			return BuildGemsChangedJson(eventArgs);
		case EventType.Level_Up:
			return BuildLevelUpJson(eventArgs);
		case EventType.Purchase:
			return BuildPurchaseJson(eventArgs);
		case EventType.Payment:
			return BuildPaymentJson(eventArgs);
		case EventType.Perk:
			return BuildPerkJson(eventArgs);
		case EventType.Session_End:
			StatisticsCollector.SetSessionId(StatisticsCollector.GetSessionId() + 1);
			return BuildSessionEndJson(eventArgs);
		case EventType.Pay_Request_Start:
			return BuildPayRequestStartJson(eventArgs);
		case EventType.Pay_Status_Update:
			return BuildPayStatusUpdateJson(eventArgs);
		case EventType.Pay_Handle_Start:
			return BuildPayHandleStartJson(eventArgs);
		case EventType.Pay_Handle_Finish:
			return BuildPayHandleFinishJson(eventArgs);
		case EventType.Pay_Transaction_Finish:
			return BuildPayTransactionFinishJson(eventArgs);
		case EventType.Pay_Handle_Error:
			return BuildPayHandleErrorJson(eventArgs);
		case EventType.Pay_Handle_Debug:
			return BuildPayHandleDebugJson(eventArgs);
		case EventType.Pay_Request_Fail:
			return BuildPayRequestFailJson(eventArgs);
		case EventType.Pay_Verification_Status_Change:
			return BuildPayVerificationJson(eventArgs);
		default:
			return null;
		}
	}

	private static JSONClass CreateHeadJson(EventType eventType)
	{
		JSONClass jSONClass = new JSONClass();
		string text = GetEventTypeName(eventType);
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

	private static JSONClass CreateHeadPayJSON(EventType eventType, string packageName, string receipt)
	{
		return CreateHeadPayJSON(GetEventTypeName(eventType), packageName, receipt);
	}

	private static JSONClass CreateHeadPayJSON(string subtype, string packageName, string receipt)
	{
		JSONClass jSONClass = new JSONClass();
		jSONClass["eid"] = StatisticsCollector.GetNextPayEventId();
		jSONClass["etype"] = "pay";
		jSONClass["build_version"] = SystemProperties.GetVersion().ToString();
		jSONClass["data_version"] = SystemProperties.GetDataVersion().ToString(true);
		jSONClass["subtype"] = subtype;
		jSONClass["package_id"] = ((!string.IsNullOrEmpty(packageName)) ? packageName : string.Empty);
		jSONClass["rid"] = ((!string.IsNullOrEmpty(receipt)) ? MD5Utils.MD5HashString(receipt) : string.Empty);
		jSONClass["paid_version"] = SystemProperties.IsPaidApp();
		if (SystemProperties.IsDebug())
		{
			jSONClass["time"] = ListSF.GetCurrentTime().ToString("X");
		}
		return jSONClass;
	}

	private static void AddTag(JSONClass data, string tag)
	{
		JSONArray asArray = data["tags"].AsArray;
		asArray.Add(tag);
		data["tags"] = asArray;
	}

	private static void AddRosterData(JSONClass data, string key)
	{
		Roster roster = ListSF.GetRoster();
		if (roster != null)
		{
		}
	}

	private static string GetEventTypeName(EventType eventType)
	{
		return eventType.ToString().ToLower();
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

	private static string BuildUserJson(ArgsDict eventArgs)
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

	private static string BuildFightEndJson(ArgsDict eventArgs)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Fight_End);
		StatisticsGeter.AddFightInfo(jSONClass, eventArgs);
		StatisticsGeter.AddFightResult(jSONClass, eventArgs);
		StatisticsGeter.AddFpsLimit(jSONClass);
		StatisticsGeter.AddEclipseMode(jSONClass);
		StatisticsGeter.AddRounds(jSONClass, eventArgs);
		StatisticsGeter.AddAverageFps(jSONClass, eventArgs);
		StatisticsGeter.AddFightTimeElapsed(jSONClass, eventArgs);
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		jSONClass["device_name"] = SystemProperties.GetDeviceInfo().Id;
		StatisticsGeter.AddPerks(jSONClass);
		return jSONClass.ToString();
	}

	private static string BuildAchievementJson(ArgsDict eventArgs)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Achievement);
		string text = ((!eventArgs.ContainsKey("name")) ? string.Empty : eventArgs["name"].ToString());
		jSONClass["name"] = text;
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		return jSONClass.ToString();
	}

	private static string BuildGemsChangedJson(ArgsDict eventArgs)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Gems_Changed);
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		StatisticsGeter.AddGemsChange(jSONClass, eventArgs);
		StatisticsGeter.AddBalances(jSONClass);
		return jSONClass.ToString();
	}

	private static string BuildLevelUpJson(ArgsDict eventArgs)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Level_Up);
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		StatisticsGeter.AddLevelUpItems(jSONClass);
		StatisticsGeter.AddFights(jSONClass, ListSF.GetRoster().GetLevel() - 1);
		return jSONClass.ToString();
	}

	private static string BuildPurchaseJson(ArgsDict eventArgs)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Purchase);
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		StatisticsGeter.AddPurchaseInfo(jSONClass, eventArgs);
		StatisticsGeter.AddBalances(jSONClass);
		return jSONClass.ToString();
	}

	private static string BuildPaymentJson(ArgsDict eventArgs)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Payment);
		string text = eventArgs["item"].ToString();
		jSONClass["item"] = text;
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		jSONClass["price"] = (float)eventArgs["price"];
		jSONClass["price_currency"] = eventArgs["price_currency"].ToString();
		jSONClass["money_changed"] = (long)eventArgs["money_changed"];
		jSONClass["gems_paid_changed"] = (long)eventArgs["gems_paid_changed"];
		jSONClass["total_payment_count"] = ListSF.GetRoster().GetPaymentCount();
		StatisticsGeter.AddPriceUsd(jSONClass, text);
		StatisticsGeter.AddBalances(jSONClass);
		return jSONClass.ToString();
	}

	private static string BuildPerkJson(ArgsDict eventArgs)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Perk);
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		jSONClass["reset_count"] = ListSF.GetRoster().GetPerks().GetPerkResetCount();
		jSONClass["level_sum"] = ListSF.GetRoster().GetPerks().GetTotalPerkLevels();
		StatisticsGeter.AddPerks(jSONClass, "user_perks");
		StatisticsGeter.AddPerkChoice(jSONClass, eventArgs);
		return jSONClass.ToString();
	}

	private static string BuildSessionEndJson(ArgsDict eventArgs)
	{
		JSONClass jSONClass = CreateHeadJson(EventType.Session_End);
		jSONClass["session_id"] = StatisticsCollector.GetSessionId();
		jSONClass["fight_amount"] = StatisticsCollector.GetFightAmount();
		jSONClass["time_length"] = StatisticsCollector.GetSessionLength();
		jSONClass["user_level"] = ListSF.GetRoster().GetLevel();
		jSONClass["energy"] = StatisticsCollector.GetEndEnergy();
		return jSONClass.ToString();
	}

	private static string BuildPayRequestStartJson(ArgsDict eventArgs)
	{
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Request_Start, eventArgs["packageName"].ToString(), null);
		return jSONClass.ToString();
	}

	private static string BuildPayStatusUpdateJson(ArgsDict eventArgs)
	{
		string packageName = eventArgs["packageName"].ToString();
		string text = eventArgs["receipt"].ToString();
		PayStatus payStatus = (PayStatus)eventArgs["status"];
		int num = (int)eventArgs["resultCode"];
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Status_Update, packageName, text);
		jSONClass["status"] = GetStatusName(payStatus);
		jSONClass["result_code"] = num;
		jSONClass["receipt"] = ((!string.IsNullOrEmpty(text)) ? text : string.Empty);
		return jSONClass.ToString();
	}

	private static string BuildPayHandleStartJson(ArgsDict eventArgs)
	{
		string packageName = eventArgs["packageName"].ToString();
		string receipt = eventArgs["receipt"].ToString();
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Handle_Start, packageName, receipt);
		return jSONClass.ToString();
	}

	private static string BuildPayHandleFinishJson(ArgsDict eventArgs)
	{
		string packageName = eventArgs["packageName"].ToString();
		string receipt = eventArgs["receipt"].ToString();
		long num = (long)eventArgs["money_changed"];
		long num2 = (long)eventArgs["gems_changed"];
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Handle_Finish, packageName, receipt);
		jSONClass["moneyChanged"] = num;
		jSONClass["gemsChanged"] = num2;
		return jSONClass.ToString();
	}

	private static string BuildPayTransactionFinishJson(ArgsDict eventArgs)
	{
		string packageName = eventArgs["packageName"].ToString();
		string receipt = eventArgs["receipt"].ToString();
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Transaction_Finish, packageName, receipt);
		return jSONClass.ToString();
	}

	private static string BuildPayHandleErrorJson(ArgsDict eventArgs)
	{
		string packageName = eventArgs["packageName"].ToString();
		string text = eventArgs["reason"].ToString();
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Handle_Error, packageName, null);
		jSONClass["reason"] = text;
		return jSONClass.ToString();
	}

	private static string BuildPayHandleDebugJson(ArgsDict eventArgs)
	{
		string packageName = eventArgs["packageName"].ToString();
		string text = eventArgs["message"].ToString();
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Handle_Debug, packageName, null);
		jSONClass["message"] = text;
		return jSONClass.ToString();
	}

	private static string BuildPayRequestFailJson(ArgsDict eventArgs)
	{
		string packageName = eventArgs["packageName"].ToString();
		JSONClass jSONClass = CreateHeadPayJSON(EventType.Pay_Request_Fail, packageName, null);
		return jSONClass.ToString();
	}

	private static string BuildPayVerificationJson(ArgsDict eventArgs)
	{
		string packageName = eventArgs["packageName"].ToString();
		string receipt = eventArgs["receipt"].ToString();
		VerificationStatus verificationStatus = (VerificationStatus)eventArgs["status"];
		string subtype = string.Format("pay_verify_{0}", GetStatusName(verificationStatus));
		JSONClass jSONClass = CreateHeadPayJSON(subtype, packageName, receipt);
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
