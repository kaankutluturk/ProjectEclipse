using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nekki.SF2.Core;
using Nekki.SF2.Core.Fights.Controller;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Common;
using Nekki.SF2.GUI.Menu;
using Nekki.SF2.GUI.Shop;
using UnityEngine;
using SF2.Offline;

public static class RaidCheatManager
{
	private static bool _isInited;

	public const string HelpCommandHelp = "help - show all commands";

	public const string ClearCommandHelp = "clear - clear console";

	public const string AddMoneyCommandHelp = "m1 (int) - add amount of money";

	public const string AddGemsCommandHelp = "m2 (int) - add amount of gems";

	public const string SetMoneyCommandHelp = "m1s (int) - set amount of money";

	public const string SetGemsCommandHelp = "m2s (int) - set amount of gems";

	public const string LevelCommandHelp = "level/lvl (int) - add levels";

	public const string UserCommandHelp = "user [unlock|reset|print] - actions by user ('unlock' - unlock all items in shop, 'reset' - reset user progress, 'print' - print user to console)";

	public const string SkipTutorialCommandHelp = "s - skip tutorial";

	public const string PaymentCommandHelp = "payment [unlock/restore/products/receipt/satb/verify/log/logu/purchase] - some test commands for payment";

	public const string ObfuscatorCommandHelp = "obfuscator/of [logi/logw/loge/logexp] - some test commands for obfuscator";

	public const string BundlesCommandHelp = "bundles [reset] - some test commands for bundles ('reset' - reset bundles cache)";

	public const string CheatCommandHelp = "cheat [win round|win fight|lose round|lose fight|reset round|reset fight]\ncheat [pause|next|min scale|magic|combo|style|crit]\ncheat [player godmode|bot godmode|debug|perks|slow]\ncheat (you can pass any kind of ControlQuadrant enum)";

	public const string NotifCommandHelp = "notif t1 (int) - run simple Test-notification-1\nnotif [Fa0|Faf | Fa1|Fat] - force show all notifications (Paid/non-Paid SF2) off/on\nnotif ra (int) - run all notifs\nnotif info - print to console notif. infos\nnotif kill - cancel all notifications\nnotif en (t|1 | f|0) - enable or disable all notifications\nlook SFIIU-48 or Wiki for details";

	public static void Init()
	{
		if (!_isInited)
		{
			_isInited = true;
			ConsoleUI.add_OnConsoleActive(OnConsoleActiveChanged);
			ConsoleDatabase.RegisterCommand("help", HelpCommand);
			ConsoleDatabase.RegisterCommand("clear", Clear);
			ConsoleDatabase.RegisterCommand("m1", AddMoneyCommand);
			ConsoleDatabase.RegisterCommand("m2", AddGemsCommand);
			ConsoleDatabase.RegisterCommand("m1s", SetMoneyCommand);
			ConsoleDatabase.RegisterCommand("m2s", SetGemsCommand);
			ConsoleDatabase.RegisterCommand("level", AddLevelsCommand);
			ConsoleDatabase.RegisterCommand("lvl", AddLevelsCommand);
			ConsoleDatabase.RegisterCommand("user", UserCommand);
			ConsoleDatabase.RegisterCommand("s", SkipTutorialCommand);
			ConsoleDatabase.RegisterCommand("payment", PaymentCommand);
			ConsoleDatabase.RegisterCommand("obfuscator", ObfuscatorCommand);
			ConsoleDatabase.RegisterCommand("of", ObfuscatorCommand);
			ConsoleDatabase.RegisterCommand("bundles", BundlesCommand);
			ConsoleDatabase.RegisterCommand("cheat", CheatCommand);
			ConsoleDatabase.RegisterCommand("notif", NotifCommand);
		}
	}

	public static void OnConsoleActiveChanged(bool isActive)
	{
		if (GameController.get_Current() != null)
		{
			GameController.get_Current().enabled = !isActive;
		}
	}

	public static void Log(string message)
	{
		ConsoleUI.Log(message);
		Debug.Log(message);
	}

	private static string HelpCommand(params string[] args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("All commands:\n");
		stringBuilder.AppendLine("help - show all commands");
		stringBuilder.AppendLine("clear - clear console");
		stringBuilder.AppendLine("m1 (int) - add amount of money");
		stringBuilder.AppendLine("m2 (int) - add amount of gems");
		stringBuilder.AppendLine("m1s (int) - set amount of money");
		stringBuilder.AppendLine("m2s (int) - set amount of gems");
		stringBuilder.AppendLine("level/lvl (int) - add levels");
		stringBuilder.AppendLine("user [unlock|reset|print] - actions by user ('unlock' - unlock all items in shop, 'reset' - reset user progress, 'print' - print user to console)");
		stringBuilder.AppendLine("s - skip tutorial");
		stringBuilder.AppendLine("payment [unlock/restore/products/receipt/satb/verify/log/logu/purchase] - some test commands for payment");
		stringBuilder.AppendLine("obfuscator/of [logi/logw/loge/logexp] - some test commands for obfuscator");
		stringBuilder.AppendLine("cheat [win round|win fight|lose round|lose fight|reset round|reset fight]\ncheat [pause|next|min scale|magic|combo|style|crit]\ncheat [player godmode|bot godmode|debug|perks|slow]\ncheat (you can pass any kind of ControlQuadrant enum)");
		stringBuilder.AppendLine("notif t1 (int) - run simple Test-notification-1\nnotif [Fa0|Faf | Fa1|Fat] - force show all notifications (Paid/non-Paid SF2) off/on\nnotif ra (int) - run all notifs\nnotif info - print to console notif. infos\nnotif kill - cancel all notifications\nnotif en (t|1 | f|0) - enable or disable all notifications\nlook SFIIU-48 or Wiki for details");
		return stringBuilder.ToString().TrimEnd('\r', '\n');
	}

	public static string Clear(params string[] args)
	{
		ConsoleUI.Clear();
		return "Ok";
	}

	private static int ParseAmountArg(params string[] args)
	{
		int result;
		if (args.Length > 0 && int.TryParse(args[0], out result))
		{
			return result;
		}
		return 1000000;
	}

	public static string AddMoneyCommand(params string[] args)
	{
		ListSF.AddCoins(ParseAmountArg(args));
		if (MainMenu.get_Instance() != null)
		{
			MainMenu.get_Instance().UpdateMoney();
		}
		return string.Format("Ok. Money={0}", ListSF.GetRoster().GetMoney());
	}

	public static string AddGemsCommand(params string[] args)
	{
		ListSF.AddGems(ParseAmountArg(args), Roster.BalanceChangeType.CHANGE_CHEAT);
		if (MainMenu.get_Instance() != null)
		{
			MainMenu.get_Instance().UpdateMoney();
		}
		return string.Format("Ok. Gems={0}", ListSF.GetRoster().GetBonus());
	}

	public static string SetMoneyCommand(params string[] args)
	{
		int num = ParseAmountArg(args);
		if (num < 0)
		{
			return "Must be >= 0";
		}
		ListSF.SetCoins(num);
		if (MainMenu.get_Instance() != null)
		{
			MainMenu.get_Instance().UpdateMoney();
		}
		return string.Format("Ok. Money={0}", ListSF.GetRoster().GetMoney());
	}

	public static string SetGemsCommand(params string[] args)
	{
		int num = ParseAmountArg(args);
		if (num < 0)
		{
			return "Must be >= 0";
		}
		ListSF.SetGems(num, Roster.BalanceChangeType.CHANGE_CHEAT);
		if (MainMenu.get_Instance() != null)
		{
			MainMenu.get_Instance().UpdateMoney();
		}
		return string.Format("Ok. Gems={0}", ListSF.GetRoster().GetBonus());
	}

	public static string AddLevelsCommand(params string[] args)
	{
		int result = 1;
		if (args.Length > 0)
		{
			int.TryParse(args[0], out result);
		}
		while (result > 0)
		{
			ListSF.GetRoster().SetExperience(ListSF.GetRoster().GetExperienceToNextLevel());
			result--;
		}
		if (MainMenu.get_Instance() != null)
		{
			MainMenu.get_Instance().UpdateLevel();
		}
		if (Scene<ShopScene>.get_Current() != null)
		{
			Scene<ShopScene>.get_Current().RefreshItems();
		}
		return string.Format("Ok. Level={0}", ListSF.GetRoster().GetLevel());
	}

	public static string UserCommand(params string[] args)
	{
		if (args.Length == 0)
		{
			return "No Arguments! Try: user unlock/reset/print";
		}
		switch (args[0])
		{
		case "unlock":
			ListSF.GetRoster().AddShopLock("ZONE_2", true);
			ListSF.GetRoster().AddShopLock("ZONE_3", true);
			ListSF.GetRoster().AddShopLock("ZONE_4", true);
			ListSF.GetRoster().AddShopLock("ZONE_5", true);
			ListSF.GetRoster().AddShopLock("ZONE_6", true);
			ListSF.GetRoster().AddShopLock("ZONE_IM", true);
			ListSF.GetRoster().AddShopLock("ZONE_7_1", true);
			ListSF.GetRoster().AddShopLock("ZONE_7_2", true);
			ListSF.GetRoster().AddShopLock("ZONE_7_3", true);
			ListSF.GetRoster().RequestSave();
			return "Ok";
		case "reset":
			ListSF.DeleteUserDataAndQuit();
			return "Ok";
		case "print":
			return ListSF.ReadUsersFile();
		default:
			return string.Format("unknown argument {0}", args[0]);
		}
	}

	public static string SkipTutorialCommand(params string[] args)
	{
		if (MainMenu.get_Instance() != null && Module.GetInstance() != null && Module.GetInstance().IsUserTutorialComplete())
		{
			MainMenu.get_Instance().SkipTutorial();
		}
		return "Ok";
	}

	public static string PaymentCommand(params string[] args)
	{
		if (args.Length == 0)
		{
			return "No Arguments! Try: payment [unlock/restore/products/receipt/satb/verify/log/logu/purchase]";
		}
		switch (args[0])
		{
		case "unlock":
			ListSF.GetRoster().AddShopLock("ZONE_1_DONATE", true);
			ListSF.GetRoster().AddShopLock("COMMON_RUBY_DONATE", true);
			ListSF.GetRoster().AddShopLock("COMMON_RUBY_DONATE_CASKET", true);
			ListSF.GetRoster().RequestSave();
			return "Ok";
		case "restore":
			PaymentManager.GetStore().RestorePurchases();
			return "Ok";
		case "products":
			PaymentManager.GetStore().LoadProducts();
			return "Ok";
		case "receipt":
			if (SystemProperties.IsIosPlatform())
			{
				var appleExt = PaymentManager.GetStore().GetExtension<AppleStoreExtensions>();
				if (appleExt != null)
				{
					appleExt.RefreshAppReceipt();
				}
				return "Ok";
			}
			return "Can run 'receipt' only on iOS platform!";
		case "satb":
			if (SystemProperties.IsIosPlatform())
			{
				bool flag = false;
				if (args.Length > 1)
				{
					flag = args[1] == "on" || args[1] == "1";
				}
				var appleExtSim = PaymentManager.GetStore().GetExtension<AppleStoreExtensions>();
				if (appleExtSim != null)
				{
					appleExtSim.SetSimulateAskToBuy(flag);
				}
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.AppendLine("SimulateAskToBuy=" + ((!flag) ? "0" : "1"));
				stringBuilder2.AppendLine("Ok");
				return stringBuilder2.ToString();
			}
			return "Can run 'satb' only on iOS platform!";
		case "verify":
		{
			bool flag2 = false;
			if (args.Length > 1)
			{
				flag2 = args[1] == "on" || args[1] == "1";
			}
			PaymentVerifier.SetVerificationEnabled(flag2);
			if (flag2)
			{
				PaymentManager.ProcessPendingPayments();
			}
			return "Ok";
		}
		case "log":
		{
			if (PaymentManager.GetIsEmulator())
			{
				return "Can't log products in emulator";
			}
			StringBuilder stringBuilder = new StringBuilder();
			Product[] array2 = PaymentManager.GetStore().GetProducts();
			stringBuilder.AppendLine("Products:");
			Product[] array3 = array2;
			foreach (Product product in array3)
			{
				stringBuilder.AppendLine(product.definition.Log());
			}
			stringBuilder.AppendLine("Ok");
			return stringBuilder.ToString();
		}
		case "logu":
		{
			StringBuilder stringBuilder3 = new StringBuilder();
			List<PaymentInfo> list = PaymentManager.GetInProgressPayments();
			stringBuilder3.AppendLine("InProgress:");
			foreach (PaymentInfo item in list)
			{
				stringBuilder3.AppendLine(item.ToString());
			}
			List<PaymentInfo> list2 = PaymentManager.GetCompletedPayments();
			stringBuilder3.AppendLine("Completed:");
			foreach (PaymentInfo item2 in list2)
			{
				stringBuilder3.AppendLine(item2.ToString());
			}
			stringBuilder3.AppendLine("Ok");
			return stringBuilder3.ToString();
		}
		case "purchase":
		{
			if (args.Length < 2)
			{
				return "No Arguments! Try: payment [purchase] [int]";
			}
			int result = 0;
			int.TryParse(args[1], out result);
			Product[] array = PaymentManager.GetStore().GetProducts();
			if (result < 0 || result >= array.Length)
			{
				return "Incorrect index";
			}
			PaymentManager.GetStore().PurchaseProduct(array[result].definition.id);
			return "purchaseStart: " + array[result].definition.id;
		}
		default:
			return "Unknown payment action!";
		}
	}

	public static string ObfuscatorCommand(params string[] args)
	{
		if (args.Length == 0)
		{
			return "No Arguments! Try: obfuscator/of [logi/logw/loge/logexp]";
		}
		switch (args[0])
		{
		case "logi":
			Debug.Log("Info");
			return "Ok";
		case "logw":
			Debug.LogWarning("Warning");
			return "Ok";
		case "loge":
			Debug.LogError("Error");
			return "Ok";
		case "logexp":
			try
			{
				throw new Exception("Exception");
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return "Ok";
		default:
			return "Unknown obfuscator action!";
		}
	}

	public static string BundlesCommand(params string[] args)
	{
		if (args.Length == 0)
		{
			return "No Arguments! Try: bundles [reset]";
		}
		string text = args[0];
		if (text != null && text == "reset")
		{
			PacksController.GetInstance().DeletePacksFile();
			ApplicationController.Quit();
			return "Ok";
		}
		return "Unknown bundles action!";
	}

	public static string CheatCommand(params string[] args)
	{
		Fight fight = Fight.GetCurrentFight();
		if (fight == null)
		{
			return "NOT IN FIGHT";
		}
		string text = string.Join(string.Empty, args).ToLower();
		text = text.Replace("magic", "rechargemagic").Replace("lose", "loss").Replace("godmode", "immortality");
		string[] names = Enum.GetNames(typeof(FightCID));
		foreach (string text2 in names)
		{
			if (text2.ToLower().Contains(text))
			{
				FightCID controlId = (FightCID)Enum.Parse(typeof(FightCID), text2);
				fight.ReleaseAnyKey(controlId);
				return "OK - " + controlId;
			}
		}
		return "NOT RECOGNIZED\nUSAGE:\ncheat [win round|win fight|lose round|lose fight|reset round|reset fight]\ncheat [pause|next|min scale|magic|combo|style|crit]\ncheat [player godmode|bot godmode|debug|perks|slow]\ncheat (you can pass any kind of ControlQuadrant enum)";
	}

	private static long ParseDelayArg(params string[] args)
	{
		int result = 3;
		if (args.Length >= 2)
		{
			int.TryParse(args[1], out result);
		}
		return result;
	}

	private static bool? ParseBoolArg(int index, params string[] args)
	{
		if (args.Length <= index)
		{
			return null;
		}
		string text = args[index].ToLower();
		if (text.Equals("t") || text.Equals("1"))
		{
			return true;
		}
		if (text.Equals("f") || text.Equals("0"))
		{
			return false;
		}
		return null;
	}

	public static string NotifCommand(params string[] args)
	{
		if (args.Length == 0)
		{
			return "No Arguments! Try:\nnotif t1 (int) - run simple Test-notification-1\nnotif [Fa0|Faf | Fa1|Fat] - force show all notifications (Paid/non-Paid SF2) off/on\nnotif ra (int) - run all notifs\nnotif info - print to console notif. infos\nnotif kill - cancel all notifications\nnotif en (t|1 | f|0) - enable or disable all notifications\nlook SFIIU-48 or Wiki for details";
		}
		switch (args[0].ToLower())
		{
		case "kill":
			LocalNotificationManager.GetInstance().CancelAll();
			return "canceled all notifications";
		case "info":
			return LocalNotificationManager.GetInstance().GetUnsupportedPlatformMessage();
		case "t1":
		{
			long num = ParseDelayArg(args);
			LocalNotificationManager.GetInstance().CancelTest();
			LocalNotificationManager.GetInstance().ScheduleTest(num);
			return "Test-notification-1 launched, delay=" + num;
		}
		case "faf":
		case "fa0":
			LocalNotificationManager.GetInstance().SetForced(false);
			return "disabled force-All notifications";
		case "fat":
		case "fa1":
			LocalNotificationManager.GetInstance().SetForced(true);
			return "enabled force-All notifications";
		case "ra":
		{
			LocalNotificationManager.GetInstance().CancelAll();
			long num = ParseDelayArg(args);
			LocalNotificationManager.GetInstance().ScheduleEnergy(num);
			LocalNotificationManager.GetInstance().ScheduleEnergyFull(num + 2);
			LocalNotificationManager.GetInstance().ScheduleItem("Test item", num + 4);
			LocalNotificationManager.GetInstance().SchedulePeriodic(num + 6);
			LocalNotificationManager.GetInstance().ScheduleRecipe("Test recipe", num + 8);
			LocalNotificationManager.GetInstance().ScheduleRetention(num + 10);
			LocalNotificationManager.GetInstance().ScheduleTest(num + 12);
			int num2 = 7;
			return "Run all notifications(" + num2 + "), min delay=" + num + ", max delay=" + (num + 12);
		}
		case "en":
		{
			bool? flag = ParseBoolArg(1, args);
			if (!flag.HasValue)
			{
				break;
			}
			LocalNotificationManager.GetInstance().SetEnabled(flag.Value);
			return (!flag.Value) ? "Ok, notifs disabled (IOS-remotes to)" : "Ok, notifs enabled";
		}
		}
		return "NOT RECOGNIZED USAGE:\nnotif t1 (int) - run simple Test-notification-1\nnotif [Fa0|Faf | Fa1|Fat] - force show all notifications (Paid/non-Paid SF2) off/on\nnotif ra (int) - run all notifs\nnotif info - print to console notif. infos\nnotif kill - cancel all notifications\nnotif en (t|1 | f|0) - enable or disable all notifications\nlook SFIIU-48 or Wiki for details";
	}
}
