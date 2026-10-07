using System.Xml;

public class Achievement
{
	public int Priority;

	public int CounterValue;

	public int MoneyPrize;

	public int BonusPrize;

	public string Name;

	public string Description;

	public string IconName;

	public string PlatformId;

	public bool IsUnlocked;

	public bool RewardClaimed;

	public bool IsHidden;

	private bool isNew;

	public bool IsNew
	{
		get
		{
			return GetIsNew();
		}
		set
		{
			SetIsNew(value);
		}
	}

	public Achievement(XmlNode node)
	{
		CounterValue = node.Attributes["CounterValue"].ParseInt();
		Priority = node.Attributes["Priority"].ParseInt();
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		Description = node.Attributes["Description"].GetStringOrDefault(string.Empty);
		IconName = node.Attributes["Icon"].GetStringOrDefault(string.Empty);
		MoneyPrize = node.Attributes["MoneyPrize"].ParseInt();
		BonusPrize = node.Attributes["BonusPrize"].ParseInt();
		IsHidden = node.Attributes["Hidden"].ParseBool();
		IsUnlocked = false;
		RewardClaimed = false;
		isNew = false;
		if (SystemProperties.IsAndroidPlatform())
		{
			PlatformId = node.Attributes["GooglePlayID"].GetStringOrDefault(string.Empty);
		}
		else
		{
			PlatformId = node.Attributes["GameCenterID"].GetStringOrDefault(string.Empty);
		}
	}

	public void SetIsNew(bool value)
	{
		isNew = value && (MoneyPrize > 0 || BonusPrize > 0);
	}

	public bool GetIsNew()
	{
		return isNew;
	}

	public string GetIconName()
	{
		return IconName;
	}
}
