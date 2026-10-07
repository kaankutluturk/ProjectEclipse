using UnityEngine;

public class LocalNotificationManager
{
	private enum NotificationId
	{
		PushRetention = 1,
		PushPeriodic = 2,
		PushEnergy = 3,
		PushEnergyFull = 4,
		PushItem = 5,
		PushRecipe = 6,
		PushTest1 = 7
	}

	private static LocalNotificationManager _instance;

	private const string RetentionTextKey = "push_retention";

	private const string PeriodicTextKey = "push_periodic";

	private const string EnergyToFightTextKey = "push_energy_to_fight";

	private const string FullEnergyTextKey = "push_full_energy";

	private bool isEnabled;

	private const string IdKey = "id";

	private bool isForced;

	public static LocalNotificationManager Instance
	{
		get
		{
			return GetInstance();
		}
	}

	private string Title
	{
		get
		{
			return GetTitle();
		}
	}

	private LocalNotificationManager()
	{
		SetForced(false);
		isEnabled = false;
		SetEnabled(AssemblyController.GetEnableNotifications());
	}

	public static LocalNotificationManager GetInstance()
	{
		if (_instance == null)
		{
			_instance = new LocalNotificationManager();
		}
		return _instance;
	}

	public void SetEnabled(bool value)
	{
		if (isEnabled != value)
		{
			isEnabled = value;
		}
	}

	private string GetTitle()
	{
		return Application.productName;
	}

	public void SetForced(bool value)
	{
		isForced = value;
	}

	private bool IsSchedulingAllowed()
	{
		return isForced || !SystemProperties.IsPaidApp();
	}

	public void ScheduleRetention(long IHDMLLNEGIK)
	{
		Schedule(1, GetTitle(), LocalizationManager.GetString("push_retention"), IHDMLLNEGIK);
	}

	public void SchedulePeriodic(long IHDMLLNEGIK)
	{
		Schedule(2, GetTitle(), LocalizationManager.GetString("push_periodic"), IHDMLLNEGIK);
	}

	public void ScheduleTest(long IHDMLLNEGIK)
	{
		Schedule(7, GetTitle(), "Test1 notification, delay=" + IHDMLLNEGIK, IHDMLLNEGIK);
	}

	public void ScheduleEnergy(long IHDMLLNEGIK)
	{
		// Energy is disabled; never schedule refill reminders.
	}

	public void ScheduleEnergyFull(long IHDMLLNEGIK)
	{
		// Energy is disabled; never schedule refill reminders.
	}

	public void ScheduleRecipe(string LIOGIBJBHAH, long IHDMLLNEGIK)
	{
		if (IsSchedulingAllowed())
		{
			Schedule(6, GetTitle(), LIOGIBJBHAH, IHDMLLNEGIK);
		}
	}

	public void ScheduleItem(string LIOGIBJBHAH, long IHDMLLNEGIK)
	{
		if (IsSchedulingAllowed())
		{
			Schedule(5, GetTitle(), LIOGIBJBHAH, IHDMLLNEGIK);
		}
	}

	public void CancelRetention()
	{
		CancelNotification(NotificationId.PushRetention);
	}

	public void CancelPeriodic()
	{
		CancelNotification(NotificationId.PushPeriodic);
	}

	public void CancelTest()
	{
		CancelNotification(NotificationId.PushTest1);
	}

	public void CancelEnergy()
	{
		CancelNotification(NotificationId.PushEnergy);
	}

	public void CancelEnergyFull()
	{
		CancelNotification(NotificationId.PushEnergyFull);
	}

	public void CancelRecipe()
	{
		CancelNotification(NotificationId.PushRecipe);
	}

	public void CancelItem()
	{
		CancelNotification(NotificationId.PushItem);
	}

	public void CancelAll()
	{
		if (isEnabled)
		{
			CancelEnergy();
			CancelEnergyFull();
			CancelRetention();
			CancelPeriodic();
			CancelRecipe();
			CancelItem();
			CancelTest();
		}
	}

	private void CancelNotification(NotificationId KEMMPFEDLAJ)
	{
		if (isEnabled)
		{
			AndroidLocalNotification.CancelNotification((int)KEMMPFEDLAJ);
		}
	}

	private void Schedule(int OKNNNLIPODI, string PEMOECLNECD, string LIOGIBJBHAH, long ENDPMCNJPEA)
	{
		if (isEnabled && SystemProperties.IsAndroidPlatform())
		{
			AndroidLocalNotification.SendNotification(OKNNNLIPODI, PEMOECLNECD, LIOGIBJBHAH, ENDPMCNJPEA);
		}
	}

	public string GetUnsupportedPlatformMessage()
	{
		return string.Concat(Application.platform, " is not supported, only IOS devices");
	}
}
