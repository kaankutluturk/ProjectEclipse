public class QuestParameters
{
	public class EnchantmentInfo
	{
		public string itemName;

		public string recipeName;

		public string costType;

		public long endTimestamp;

		public EnchantmentInfo()
		{
			endTimestamp = 0L;
			itemName = string.Empty;
			recipeName = string.Empty;
			costType = string.Empty;
		}
	}

	public ItemInfo purchasedItem;

	public FightIDS fightIds;

	public RosterQuest.QuestVariable actionId;

	public LocalizationManager.Language chosenLanguage;

	public EnchantmentInfo enchantment;

	public string fightResult;

	public string raidId;

	public string raidResult;

	public string sceneFrom;

	public string sceneTo;

	public string tabFrom;

	public string tabTo;

	public string currentSceneName;

	public string currentTabName;

	public string iteratorValue;

	public string buttonType;

	public string buttonName;

	public string packName;

	public string timerName;

	public string perkName;

	public string purchaseFailureReason;

	public string setItemName;

	public int energyChange;

	public int levelUp;

	public int gemsPrice;

	public int lotteryLastSpinNumber;

	public float fightAvgFps;

	public bool inLottery;

	internal QuestParameters SnapshotForQueue()
	{
		var snapshot = (QuestParameters)MemberwiseClone();
		snapshot.fightIds = fightIds == null ? null : new FightIDS(fightIds);
		if (enchantment != null)
			snapshot.enchantment = new EnchantmentInfo {
				itemName = enchantment.itemName, recipeName = enchantment.recipeName,
				costType = enchantment.costType, endTimestamp = enchantment.endTimestamp
			};
		return snapshot;
	}

	public QuestParameters()
	{
		chosenLanguage = null;
		purchasedItem = null;
		sceneFrom = string.Empty;
		sceneTo = string.Empty;
		tabFrom = string.Empty;
		tabTo = string.Empty;
		currentSceneName = "None";
		currentTabName = string.Empty;
		buttonType = string.Empty;
		buttonName = string.Empty;
		packName = string.Empty;
		setItemName = string.Empty;
		energyChange = 0;
		gemsPrice = 0;
		lotteryLastSpinNumber = 0;
		inLottery = false;
		fightIds = null;
		enchantment = new EnchantmentInfo();
	}

	public FightList GetFightList()
	{
		return ListSF.GetFightById(fightIds);
	}
}
