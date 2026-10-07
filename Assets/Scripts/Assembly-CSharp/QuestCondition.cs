using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;

public class QuestCondition : ConditionExtension
{
	public enum ComparisonType
	{
		QUEST_CONDITION_NONE = 0,
		QUEST_CONDITION_EQUAL = 1,
		QUEST_CONDITION_GREATER = 2,
		QUEST_CONDITION_GREATER_EQUAL = 3,
		QUEST_CONDITION_LESS = 4,
		QUEST_CONDITION_LESS_EQUAL = 5,
		QUEST_CONDITION_OPERATOR = 6
	}

	public enum LogicalOperator
	{
		QUEST_CONDITION_SUB_NONE = 0,
		QUEST_CONDITION_SUB_OR = 1,
		QUEST_CONDITION_SUB_AND = 2
	}

	public ComparisonType comparison;

	public LogicalOperator logicalOperator;

	public string value1;

	public string value2;

	public List<QuestCondition> conditions = new List<QuestCondition>();

	public bool isNot;
	private bool _compareVersions;

	private QuestParameters questParameters;

	private RosterQuest rosterQuest;

	public static ComparisonType ParseComparison(string comparisonText)
	{
		switch (comparisonText)
		{
		case "Equal":
			return ComparisonType.QUEST_CONDITION_EQUAL;
		case "Greater":
			return ComparisonType.QUEST_CONDITION_GREATER;
		case "GreaterEqual":
			return ComparisonType.QUEST_CONDITION_GREATER_EQUAL;
		case "Less":
			return ComparisonType.QUEST_CONDITION_LESS;
		case "LessEqual":
			return ComparisonType.QUEST_CONDITION_LESS_EQUAL;
		case "Operator":
			return ComparisonType.QUEST_CONDITION_OPERATOR;
		default:
			return ComparisonType.QUEST_CONDITION_NONE;
		}
	}

	public static LogicalOperator ParseLogicalOperator(string operatorText)
	{
		switch (operatorText)
		{
		case "Or":
			return LogicalOperator.QUEST_CONDITION_SUB_OR;
		case "And":
			return LogicalOperator.QUEST_CONDITION_SUB_AND;
		default:
			return LogicalOperator.QUEST_CONDITION_SUB_NONE;
		}
	}

	public virtual void Parse(XmlNode node)
	{
		isNot = XmlUtils.ParseBool(node.Attributes["Not"]);
		_compareVersions = XmlUtils.ParseString(node.Attributes["CompareType"]) == "Versions";
		comparison = ParseComparison(node.Name);
		logicalOperator = ParseLogicalOperator(XmlUtils.ParseString(node.Attributes["Type"]));
		value1 = ClearGaps(XmlUtils.ParseString(node.Attributes["Value1"]));
		value2 = ClearGaps(XmlUtils.ParseString(node.Attributes["Value2"]));
	}

	public bool Compare(QuestParameters parameters, RosterQuest quest)
	{
		bool isMatch = false;
		if (comparison != ComparisonType.QUEST_CONDITION_OPERATOR)
		{
			isMatch = IsCompare(parameters, quest);
		}
		else
		{
			foreach (QuestCondition item in conditions)
			{
				bool flag = item.Compare(parameters, quest);
				if (logicalOperator == LogicalOperator.QUEST_CONDITION_SUB_AND && !flag)
				{
					return IsNotCompare(false);
				}
				if (logicalOperator == LogicalOperator.QUEST_CONDITION_SUB_OR && flag)
				{
					return IsNotCompare(true);
				}
			}
			if (logicalOperator == LogicalOperator.QUEST_CONDITION_SUB_AND)
			{
				return IsNotCompare(true);
			}
			if (logicalOperator == LogicalOperator.QUEST_CONDITION_SUB_OR)
			{
				return IsNotCompare(false);
			}
		}
		return IsNotCompare(isMatch);
	}

	public void SetParameters(QuestParameters parameters)
	{
		questParameters = parameters;
	}

	protected override void ResolveSessionVariable(string value, CompareResult result)
	{
		if (questParameters != null)
		{
			switch (value)
			{
			case "_$Fight":
				result.resultSTR = ((questParameters.GetFightList() == null) ? string.Empty : questParameters.GetFightList().FightId.ToString());
				break;
			case "_$Raid":
				result.resultSTR = questParameters.raidId;
				break;
			case "_$FightResult":
				result.resultSTR = questParameters.fightResult;
				break;
			case "_$RaidResult":
				result.resultSTR = questParameters.raidResult;
				break;
			case "_$LevelUp":
				result.resultNumber = questParameters.levelUp;
				break;
			case "_$ActionID":
				result.resultSTR = ((questParameters.actionId == null) ? string.Empty : questParameters.actionId.Value);
				break;
			case "_$SceneFrom":
				result.resultSTR = questParameters.sceneFrom;
				break;
			case "_$SceneTo":
				result.resultSTR = questParameters.sceneTo;
				break;
			case "_$Purchase":
				result.resultSTR = ((questParameters.purchasedItem == null) ? string.Empty : questParameters.purchasedItem.Name);
				break;
			case "_$PurchaseUnsuccessful":
				result.resultSTR = GetPurchaseUnsuccessfulValue();
				break;
			case "_$Deliver":
				result.resultSTR = ((questParameters.purchasedItem == null) ? string.Empty : questParameters.purchasedItem.Name);
				break;
			case "_$EnergyChange":
				result.resultNumber = questParameters.energyChange;
				break;
			case "_$Iterator":
				result.resultSTR = questParameters.iteratorValue;
				break;
			case "_$ChosenLocale":
				result.resultSTR = ((questParameters.chosenLanguage == null) ? string.Empty : questParameters.chosenLanguage.name);
				break;
			case "_$ButtonType":
				result.resultSTR = questParameters.buttonType;
				break;
			case "_$TabTo":
				result.resultSTR = questParameters.tabTo;
				break;
			case "_$TabFrom":
				result.resultSTR = questParameters.tabFrom;
				break;
			case "_$FightAvgFPS":
				result.resultSTR = questParameters.fightAvgFps.ToString();
				break;
			case "_$TimerName":
				result.resultSTR = questParameters.timerName.ToString();
				break;
			case "_$ButtonName":
				result.resultSTR = questParameters.buttonName;
				break;
			case "_$PackName":
				result.resultSTR = questParameters.packName;
				break;
			case "_$GemsPrice":
				result.resultNumber = questParameters.gemsPrice;
				break;
			case "_$Enchantment":
				result.resultSTR = GetEnchantmentValue();
				break;
			case "_$PerkName":
				result.resultSTR = questParameters.perkName;
				break;
			case "_$LotteryLastSpinNumber":
				result.resultSTR = questParameters.lotteryLastSpinNumber.ToString();
				break;
			case "_$InLottery":
				result.resultSTR = questParameters.inLottery.ToString();
				break;
			case "_$SetItem":
				result.resultSTR = questParameters.setItemName.ToString();
				break;
			case "_$StoryTutorialStep":
				result.resultSTR = ListSF.GetRoster().GetTutorials().GetStoryStep();
				break;
			default:
			{
				Roster roster = ListSF.GetRoster();
				RosterQuest.QuestVariable questVariable = roster.FindQuestVariable(value);
				string variableValue = ((questVariable == null) ? value : questVariable.Value);
				SetLiteralValue(variableValue, result);
				break;
			}
			}
		}
		if (!result.resultSTR.Equals(string.Empty) && GetVariableType(result.resultSTR) == QuestVariableType.QUEST_CONDITION_VARIABLE_NUMBER)
		{
			string numberText = result.resultSTR;
			result.resultSTR = string.Empty;
			result.resultNumber = float.Parse(numberText);
		}
	}

	private bool IsCompare(QuestParameters parameters, RosterQuest quest)
	{
		this.questParameters = parameters;
		this.rosterQuest = quest;
		CompareResult result = new CompareResult();
		CompareResult rightResult = new CompareResult();
		SetValue(value1, result);
		SetValue(value2, rightResult);
		return CompareResults(result, rightResult);
	}

	private bool CompareResults(CompareResult leftResult, CompareResult rightResult)
	{
		if (_compareVersions)
		{
			VersionContainer left = new VersionContainer(leftResult.ToString());
			VersionContainer right = new VersionContainer(rightResult.ToString());
			int order = VersionContainer.IsEqual(left, right) ? 0 :
				VersionContainer.IsGreater(left, right) ? 1 : -1;
			return NumberCompare(order, 0);
		}
		if (!leftResult.IsNumber() && !rightResult.IsNumber())
		{
			return StringCompare(leftResult.resultSTR, rightResult.resultSTR);
		}
		if (leftResult.IsNumber() && rightResult.IsNumber())
		{
			return NumberCompare((int)leftResult.resultNumber, (int)rightResult.resultNumber);
		}
		string leftText = leftResult.ToString();
		string rightText = rightResult.ToString();
		return StringCompare(leftText, rightText);
	}

	private bool StringCompare(string leftText, string rightText)
	{
		return leftText.Equals(rightText);
	}

	private bool NumberCompare(int leftNumber, int rightNumber)
	{
		switch (comparison)
		{
		case ComparisonType.QUEST_CONDITION_EQUAL:
			return leftNumber == rightNumber;
		case ComparisonType.QUEST_CONDITION_GREATER:
			return leftNumber > rightNumber;
		case ComparisonType.QUEST_CONDITION_GREATER_EQUAL:
			return leftNumber >= rightNumber;
		case ComparisonType.QUEST_CONDITION_LESS:
			return leftNumber < rightNumber;
		case ComparisonType.QUEST_CONDITION_LESS_EQUAL:
			return leftNumber <= rightNumber;
		default:
			return false;
		}
	}

	private bool IsNotCompare(bool matches)
	{
		return isNot ? (!matches) : matches;
	}

	protected override void FullFunction(QuestFunctions function, CompareResult result)
	{
		result.Clear();
		switch (function.functionName)
		{
		case "Fight":
			EvaluateFightFunction(function, result);
			break;
		case "Player":
			EvaluatePlayerFunction(function, result);
			break;
		case "Item":
			EvaluateItemFunction(function, result);
			break;
		case "PackAssert":
			EvaluatePackAssertFunction(function, result);
			break;
		case "UniformIntRandom":
			MathFunction(function, result, MathFunctionType.MATH_RAND);
			break;
		case "RandomAspect":
			RandomAspect(function, result);
			break;
		case "PerkInfo":
			PerkInfo(function, result);
			break;
		case "Purchase":
			EvaluatePurchaseFunction(function, result);
			break;
		case "ItemsOfType":
			EvaluateItemsOfTypeFunction(function, result);
			break;
		case "Battle":
			EvaluateBattleFunction(function, result);
			break;
		case "DataVersion":
			EvaluateDataVersionFunction(function, result);
			break;
		case "VersionController":
			EvaluateVersionControllerFunction(function, result);
			break;
		case "SysInfo":
			EvaluateSysInfoFunction(function, result);
			break;
		case "Deliver":
			EvaluateDeliverFunction(function, result);
			break;
		case "SessionSettings":
			SessionSettings(function, result);
			break;
		case "Sub":
			MathFunction(function, result, MathFunctionType.MATH_SUB);
			break;
		case "Sum":
			MathFunction(function, result, MathFunctionType.MATH_SUM);
			break;
		case "Multi":
			MathFunction(function, result, MathFunctionType.MATH_MULTI);
			break;
		case "Div":
			MathFunction(function, result, MathFunctionType.MATH_DIVISION);
			break;
		case "NDiv":
			MathFunction(function, result, MathFunctionType.MATH_DIVISION_INT);
			break;
		case "Mod":
			MathFunction(function, result, MathFunctionType.MATH_MOD);
			break;
		case "Concat":
			StringFunction(function, result, StringFunctionType.STRING_CONCAT);
			break;
		case "Slice":
			StringFunction(function, result, StringFunctionType.STRING_SLICE);
			break;
		case "Gift":
			EvaluateGiftFunction(function, result);
			break;
		case "Timer":
			EvaluateTimerFunction(function, result);
			break;
		case "ABGroupExists":
			EvaluateAbGroupExistsFunction(function, result);
			break;
		case "Enchantment":
			EvaluateEnchantmentFunction(function, result);
			break;
		case "FightCurrencyCost":
			EvaluateFightCurrencyCostFunction(function, result);
			break;
		case "ShopAssert":
			EvaluateShopAssertFunction(function, result);
			break;
		case "GetSimOperator":
			EvaluateSimOperatorFunction(function, result);
			break;
		case "RaidInfo":
			EvaluateRaidInfoFunction(function, result);
			break;
		default:
			GameLog.Error(string.Format("{0},{1}", "QuestCondition::fullFunction - unknown function: ", function.functionName));
			break;
		}
	}

	private void RandomAspect(QuestFunctions function, CompareResult result)
	{
		if (function.arguments.Count < 2)
		{
			result.resultNumber = 0.0;
			return;
		}
		int minimum;
		int maximum;
		if (!int.TryParse(function.arguments[0].result, out minimum) ||
			!int.TryParse(function.arguments[1].result, out maximum))
		{
			result.resultNumber = 0.0;
			return;
		}
		ForgeManager forge = ForgeManager.GetInstance();
		int level = forge.ResolveAspectLevel(ListSF.GetRoster().GetLevel());
		int baseAspect = forge.GetAspectValueByLevel(level);
		result.resultNumber = baseAspect + NekkiMath.randomInt(minimum, maximum + 1);
	}

	private void PerkInfo(QuestFunctions function, CompareResult result)
	{
		string name = function.GetFirstArgument();
		PerkInfoItem perk = GameUtils.PerkItemList.FindUserPerk(name);
		if (perk == null)
		{
			perk = GameUtils.PerkItemList.FindProgressionPerk(name);
		}
		if (perk == null)
		{
			perk = GameUtils.PerkItemList.FindBasePerk(name);
		}
		if (perk == null)
		{
			return;
		}
		switch (function.property)
		{
		case "Icon":
		case "Image":
			result.resultSTR = perk.ImageName;
			break;
		case "Name":
			result.resultSTR = perk.Name;
			break;
		case "Description":
			result.resultSTR = perk.DescriptionKey;
			break;
		}
	}

	private void EvaluateFightFunction(QuestFunctions function, CompareResult result)
	{
		string text = function.GetFirstArgument();
		FightList fight = ListSF.GetInstance().GetFightByIdString(text);
		if (fight == null)
		{
			GameLog.Write(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.FightFunction - cant fight fight: ", text));
			return;
		}
		string propertyName = function.property;
		switch (propertyName)
		{
		case "Name":
			result.resultSTR = fight.FightId.ToString();
			break;
		case "Zone":
			result.resultSTR = fight.FightId.GetZone();
			break;
		case "Battle":
			result.resultSTR = fight.FightId.GetBattle();
			break;
		case "Fight":
			result.resultSTR = fight.FightId.GetFight();
			break;
		case "Money":
			result.resultSTR = fight.PrizeMoney.ToString();
			break;
		case "Bonus":
			result.resultSTR = fight.PrizeBonus.ToString();
			break;
		case "Type":
			result.resultSTR = ListSF.GetInstance().GetBattleTypeName(fight.get_Type());
			break;
		case "LossCount":
			result.resultNumber = ((fight.GetRosterFight() != null) ? fight.GetRosterFight().GetLossCount() : 0);
			break;
		case "WinCount":
			result.resultNumber = ((fight.GetRosterFight() != null) ? fight.GetRosterFight().GetWinCount() : 0);
			break;
		case "TimeLeft":
			result.resultNumber = fight.GetTimeLeft();
			break;
		case "Difficulty":
			result.resultNumber = GameUtils.GetFightDifficulty(fight);
			break;
		case "Description":
			result.resultSTR = fight.GetDescription();
			break;
		case "Helm":
			result.resultSTR = fight.GetRuleItemName("Helm");
			break;
		case "Weapon":
			result.resultSTR = fight.GetRuleItemName("Weapon");
			break;
		case "Armor":
			result.resultSTR = fight.GetRuleItemName("Armor");
			break;
		case "Magic":
			result.resultSTR = fight.GetRuleItemName("Magic");
			break;
		case "RaidCharge":
			result.resultSTR = fight.GetRuleItemName("RaidCharge");
			break;
		case "Ranged":
			result.resultSTR = fight.GetRuleItemName("Ranged");
			break;
		case "HelmLevel":
			result.resultNumber = fight.GetRuleItemLevel("Helm");
			break;
		case "WeaponLevel":
			result.resultNumber = fight.GetRuleItemLevel("Weapon");
			break;
		case "ArmorLevel":
			result.resultNumber = fight.GetRuleItemLevel("Armor");
			break;
		case "MagicLevel":
			result.resultNumber = fight.GetRuleItemLevel("Magic");
			break;
		case "RaidChargeLevel":
			result.resultNumber = fight.GetRuleItemLevel("RaidCharge");
			break;
		case "RangedLevel":
			result.resultNumber = fight.GetRuleItemLevel("Ranged");
			break;
		case "CheckCurrency":
			result.resultNumber = Convert.ToDouble(fight.HasCurrencyCost());
			break;
		case "EnoughCurrency":
			result.resultNumber = Convert.ToDouble(GameUtils.HasEnoughCurrencyForFight(fight));
			break;
		case "Timestamp":
			result.resultNumber = ((fight.GetRosterFight() == null) ? 0 : fight.GetRosterFight().GetCompletionTimestamp());
			break;
		case "Level":
			result.resultNumber = ((fight.GetRosterFight() != null) ? fight.GetRosterFight().GetLevel() : 0);
			break;
		case "Power":
			result.resultNumber = fight.GetPowerRequired();
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.FightFunction - unknown property: ", propertyName));
			break;
		}
	}

	private void EvaluatePlayerFunction(QuestFunctions function, CompareResult result)
	{
		Roster roster = ListSF.GetRoster();
		if (roster == null)
		{
			return;
		}
		switch (function.property)
		{
		case "Skeleton":
		{
			ItemInfo skeletonItem = roster.get_Parameters().GetItemByType("Skeleton");
			if (skeletonItem != null)
			{
				result.resultSTR = skeletonItem.Name;
			}
			break;
		}
		case "Helm":
		{
			ItemInfo helmItem = roster.get_Parameters().GetItemByType("Helm");
			if (helmItem != null)
			{
				result.resultSTR = helmItem.Name;
			}
			break;
		}
		case "Armor":
		{
			ItemInfo armorItem = roster.get_Parameters().GetItemByType("Armor");
			if (armorItem != null)
			{
				result.resultSTR = armorItem.Name;
			}
			break;
		}
		case "Weapon":
		{
			ItemInfo weaponItem = roster.get_Parameters().GetItemByType("Weapon");
			if (weaponItem != null)
			{
				result.resultSTR = weaponItem.Name;
			}
			break;
		}
		case "Magic":
		{
			ItemInfo magicItem = roster.get_Parameters().GetItemByType("Magic");
			if (magicItem != null)
			{
				result.resultSTR = magicItem.Name;
			}
			break;
		}
		case "RaidCharge":
		{
			ItemInfo raidChargeItem = roster.get_Parameters().GetItemByType("RaidConsumable");
			if (raidChargeItem != null)
			{
				result.resultSTR = raidChargeItem.Name;
			}
			break;
		}
		case "Ranged":
		{
			ItemInfo itemInfo = roster.get_Parameters().GetItemByType("Ranged");
			if (itemInfo != null)
			{
				result.resultSTR = itemInfo.Name;
			}
			break;
		}
		case "Money":
			result.resultNumber = roster.GetMoney();
			break;
		case "Bonus":
			result.resultNumber = roster.GetBonus();
			break;
		case "Level":
			result.resultNumber = roster.GetLevel();
			break;
		case "Power":
			result.resultNumber = roster.GetMaxPower();
			break;
		case "Language":
			result.resultSTR = roster.GetLanguage();
			break;
		case "CoinIcon":
			result.resultSTR = roster.GetCoinIcon();
			break;
		case "MapFocus":
		{
			FightIDS fightIdSet = ListSF.GetRoster().GetMapFocus();
			result.resultSTR = fightIdSet.GetZoneBattle();
			break;
		}
		case "RaidMapFocus":
		{
			FightIDS fightIds = ListSF.GetRoster().GetRaidMapFocus();
			result.resultSTR = fightIds.GetZoneBattle();
			break;
		}
		case "IsLoggedInRaids":
			result.resultNumber = Convert.ToDouble(RaidLoginState.GetInstance().GetIsLoggedIn());
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.PlayerFunction - unknown property: ", function.property));
			break;
		}
	}

	private void EvaluateItemFunction(QuestFunctions function, CompareResult result)
	{
		string itemName = function.GetFirstArgument();
		ItemInfo itemInfo = ListSF.GetItems().GetItemByName(itemName);
		if (itemInfo == null)
		{
			return;
		}
		Roster roster = ListSF.GetRoster();
		switch (function.property)
		{
		case "Price":
			result.resultNumber = (ObscuredLong)(itemInfo.CoinPrice);
			break;
		case "BonusPrice":
			result.resultNumber = (ObscuredLong)(itemInfo.GemPrice);
			break;
		case "BonusDeliveryPrice":
			result.resultNumber = (ObscuredLong)(itemInfo.DeliveryGemPrice);
			break;
		case "MoneyDeliveryPrice":
			result.resultNumber = (ObscuredLong)(itemInfo.DeliveryCoinPrice);
			break;
		case "Level":
			result.resultNumber = itemInfo.ItemLevel;
			break;
		case "Equipped":
		{
			UserItem equippedItem = roster.GetInventory().FindItem(itemInfo);
			if (equippedItem != null)
			{
				result.resultNumber = Convert.ToDouble(equippedItem.GetIsEquipped());
			}
			break;
		}
		case "Quantity":
		{
			UserItem userItem = roster.GetInventory().FindItem(itemInfo);
			result.resultNumber = ((userItem != null) ? userItem.GetCount() : 0);
			break;
		}
		case "NextMoneyUpgradePrice":
		{
			ItemInfo nextUpgradeInfo = GetNextUpgradeInfo(itemInfo);
			result.resultNumber = GetItemPrice(nextUpgradeInfo, true);
			break;
		}
		case "NextBonusUpgradePrice":
		{
			ItemInfo nextBonusUpgrade = GetNextUpgradeInfo(itemInfo);
			result.resultNumber = GetItemPrice(nextBonusUpgrade, false);
			break;
		}
		case "NextUpgradeDeliveryPrice":
		{
			ItemInfo nextDeliveryUpgrade = GetNextUpgradeInfo(itemInfo);
			result.resultNumber = GetUpgradePrice(nextDeliveryUpgrade, false);
			break;
		}
		case "NextUpgradeDeliveryTime":
		{
			ItemInfo nextTimeUpgrade = GetNextUpgradeInfo(itemInfo);
			result.resultNumber = GetDeliveryUpgradeTime(nextTimeUpgrade);
			break;
		}
		case "Type":
			result.resultSTR = itemInfo.Type;
			break;
		case "SubType":
			result.resultSTR = itemInfo.SubType;
			break;
		case "Availability":
		{
			result.resultNumber = ShopAvailabilityPolicy.IsAvailable(itemInfo, roster) ? 1 : 0;
			break;
		}
		case "RealPrice":
			result.resultSTR = itemInfo.LocalizedPriceString;
			break;
		case "RecieveGold":
			result.resultSTR = itemInfo.ReceiveGold.ToString();
			break;
		case "RecieveBonus":
			result.resultSTR = itemInfo.ReceiveBonus.ToString();
			break;
		case "Image":
			result.resultSTR = itemInfo.FileName;
			break;
		case "PackLabel":
			result.resultSTR = itemInfo.GroupId;
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.ItemFunction - unknown property: ", function.property));
			break;
		}
	}

	private ItemInfo GetNextUpgradeInfo(ItemInfo item)
	{
		ItemInfo upgradeInfo = null;
		UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(item);
		if (userItem != null && userItem.GetDeliveryTimestamp() > 0 && userItem.GetDeliveryUpgradeLevel() > 0)
		{
			ItemInfo nextUpgrade = userItem.GetEffectiveInfo();
			upgradeInfo = ((nextUpgrade == null) ? null : nextUpgrade.Clone());
		}
		if (upgradeInfo != null)
		{
			upgradeInfo = ((userItem == null) ? item.GetUpgradeItemByIndex(0) : userItem.GetNextUpgradeItem().Clone());
		}
		return upgradeInfo;
	}

	private long GetItemPrice(ItemInfo item, bool useCoins)
	{
		long result = 2147483647L;
		if (item != null)
		{
			if ((useCoins && item.HasCoinPrice()) || (!useCoins && item.HasGemPrice()))
			{
				result = (ObscuredLong)((!useCoins) ? item.GemPrice : item.CoinPrice);
			}
			else
			{
				GameLog.Write(string.Format("{0},{2},{1},{3}", "QuestCondition::itemFunction ", " price - no price: ", (!useCoins) ? "bonus" : "money", item.Name));
			}
		}
		else
		{
			GameLog.Write(string.Format("{0},{2},{1}", "QuestCondition::itemFunction ", " price - no next upgrade", (!useCoins) ? "bonus" : "money"));
		}
		return result;
	}

	private long GetUpgradePrice(ItemInfo item, bool useCoins)
	{
		long result = 2147483647L;
		if (item != null)
		{
			result = (ObscuredLong)((!useCoins) ? item.DeliveryGemPrice : item.DeliveryCoinPrice);
		}
		else
		{
			GameLog.Write(string.Format("{0},{2},{1}", "QuestCondition::itemFunction ", " price - no next upgrade", (!useCoins) ? "bonus" : "money"));
		}
		return result;
	}

	private long GetDeliveryUpgradeTime(ItemInfo item)
	{
		long result = 2147483647L;
		if (item != null)
		{
			result = item.DeliveryTime;
		}
		else
		{
			GameLog.Write("QuestCondition::getDeliveryUpgradeTime ERROR - no upgrade found");
		}
		return result;
	}

	private void EvaluatePackAssertFunction(QuestFunctions function, CompareResult result)
	{
		string packName = function.GetFirstArgument();
		switch (function.property)
		{
		case "Availability":
		{
			bool flag = PacksController.GetInstance().IsPackByName(packName);
			result.resultNumber = (flag ? 1 : 0);
			break;
		}
		case "Existence":
		{
			DownloadPack downloadPack = PacksController.GetInstance().FindPack(packName);
			result.resultNumber = ((downloadPack != null) ? 1 : 0);
			break;
		}
		case "Size":
		{
			DownloadPack downloadPack = GeneralConfig.DownloadPacks.FindPack(packName);
			if (downloadPack != null)
			{
				result.resultSTR = downloadPack.Size;
			}
			break;
		}
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.PackFunction - unknown property: ", function.property));
			break;
		}
	}

	private void EvaluatePurchaseFunction(QuestFunctions function, CompareResult result)
	{
		string[] array = function.GetFirstArgument().Split('|');
		if (array.Length != 0)
		{
			string itemName = array[0];
			string failureText = ((array.Length <= 1) ? string.Empty : array[1]);
			UserItem userItem = ListSF.GetUserItem(itemName);
			ItemInfo itemInfo = ((userItem == null) ? ListSF.GetItems().GetItemByName(itemName) : userItem.GetInfo());
			switch (function.property)
			{
			case "Type":
				result.resultSTR = ((itemInfo == null) ? string.Empty : itemInfo.Type);
				break;
			case "Name":
				result.resultSTR = ((itemInfo == null) ? string.Empty : itemInfo.Name);
				break;
			case "UpgradeLevel":
				result.resultNumber = ((userItem != null) ? userItem.GetUpgradeLevel() : 0);
				break;
			case "Timeout":
			{
				long num = ((userItem == null) ? 0 : userItem.GetDeliveryTimestamp());
				long num2 = GameUtils.GetCurrentTime();
				long num3 = ((num <= num2) ? 0 : (num - num2));
				result.resultNumber = num3;
				break;
			}
			case "Failure":
				result.resultSTR = failureText;
				break;
			case "PaidItem":
				result.resultSTR = ((itemInfo == null) ? string.Empty : itemInfo.LegacyPaidItem);
				break;
			default:
				GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.PurchaseFunction - unknown property: ", function.property));
				break;
			}
		}
	}

	private void EvaluateItemsOfTypeFunction(QuestFunctions function, CompareResult result)
	{
		Roster roster = ListSF.GetRoster();
		if (roster != null)
		{
			string itemType = function.GetFirstArgument();
			List<UserItem> list = roster.GetInventory().FindItemsByType(itemType, string.Empty);
			if (function.property.Equals("Quantity"))
			{
				result.resultNumber = list.Count;
			}
		}
	}

	private void EvaluateBattleFunction(QuestFunctions function, CompareResult result)
	{
		string fightIdText = function.GetFirstArgument();
		FightIDS fightIds = new FightIDS(fightIdText);
		switch (function.property)
		{
		case "Available":
		{
			Battle availableBattle = ListSF.GetBattleById(fightIds);
			if (availableBattle != null)
			{
				Roster roster = ListSF.GetRoster();
				bool flag = roster.HasBattle(fightIds);
				result.resultNumber = (flag ? 1 : 0);
			}
			else
			{
				GameLog.Error(string.Format("{0},{1}", "Quest Error: no such battle in stages: ", fightIds.ToString()));
				result.resultNumber = 0.0;
			}
			break;
		}
		case "Locked":
		{
			Battle lockedBattle = ListSF.GetBattleById(fightIds);
			bool flag2 = true;
			if (lockedBattle != null)
			{
				flag2 = lockedBattle.GetRosterBattle() == null || lockedBattle.GetRosterBattle().IsLocked();
			}
			result.resultNumber = (flag2 ? 1 : 0);
			break;
		}
		case "Name":
		{
			Battle namedBattle = ListSF.GetBattleById(fightIds);
			if (namedBattle != null)
			{
				result.resultSTR = namedBattle.get_Name();
				break;
			}
			GameLog.Error(string.Format("{0},{1}", "Quest Error: no such battle in stages: ", fightIds.ToString()));
			result.resultSTR = string.Empty;
			break;
		}
		case "Type":
		{
			Battle typedBattle = ListSF.GetBattleById(fightIds);
			BattleType battleType = BattleType.FightDummy;
			if (typedBattle != null)
			{
				battleType = typedBattle.get_Type();
			}
			else
			{
				GameLog.Error(string.Format("{0},{1}", "Quest Error: no such battle in stages: ", fightIds.ToString()));
			}
			result.resultSTR = ListSF.GetInstance().GetBattleTypeName(battleType);
			break;
		}
		case "Zone":
		{
			Battle battle = ListSF.GetBattleById(fightIds);
			if (battle != null)
			{
				Zone zone = battle.GetZone();
				if (zone != null)
				{
					result.resultSTR = zone.get_Name();
				}
				else
				{
					result.resultSTR = string.Empty;
				}
			}
			else
			{
				GameLog.Error(string.Format("{0},{1}", "Quest Error: no such battle in stages: ", fightIds.ToString()));
				result.resultSTR = string.Empty;
			}
			break;
		}
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.BattleFunction - unknown property: ", function.property));
			break;
		}
	}

	private void EvaluateDataVersionFunction(QuestFunctions function, CompareResult result)
	{
		VersionContainer version = SystemProperties.GetDataVersion();
		switch (function.property)
		{
		case "Version":
			result.resultSTR = version.ToString();
			break;
		case "Production":
			result.resultNumber = version.GetMajor();
			break;
		case "Major":
			result.resultNumber = version.GetMinor();
			break;
		case "Minor":
			result.resultNumber = version.GetBuild();
			break;
		case "DataVersion":
			result.resultNumber = version.GetRevision();
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.UserVersionFunction - unknown property: ", function.property));
			break;
		}
	}

	private void EvaluateVersionControllerFunction(QuestFunctions function, CompareResult result)
	{
		VersionContainer version = SystemProperties.GetVersion();
		switch (function.property)
		{
		case "Version":
			result.resultSTR = version.ToString();
			break;
		case "Production":
			result.resultNumber = version.GetMajor();
			break;
		case "Major":
			result.resultNumber = version.GetMinor();
			break;
		case "Minor":
			result.resultNumber = version.GetBuild();
			break;
		case "DataVersion":
			result.resultNumber = version.GetRevision();
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.VersionControllerFunction - unknown property: ", function.property));
			break;
		}
	}

	private void EvaluateSysInfoFunction(QuestFunctions function, CompareResult result)
	{
		DeviceInfo deviceInfo = SystemProperties.GetDeviceInfo();
		switch (function.property)
		{
		case "DeviceType":
			if (SystemProperties.IsTabletDevice())
			{
				result.resultSTR = "Tablet";
			}
			else
			{
				result.resultSTR = "Phone";
			}
			break;
		case "ResolutionLocation":
			result.resultSTR = SystemProperties.PathTypeToString(deviceInfo.LocationResolution);
			break;
		case "ResolutionGUI":
			result.resultSTR = SystemProperties.PathTypeToString(deviceInfo.GuiResolution);
			break;
		case "Id":
			result.resultSTR = deviceInfo.Id;
			break;
		case "Os":
			result.resultSTR = deviceInfo.Os;
			break;
		case "OsName":
			result.resultSTR = deviceInfo.OsName;
			break;
		case "Language":
			result.resultSTR = deviceInfo.Locale;
			break;
		case "CpuCount":
			result.resultNumber = deviceInfo.CpuCount;
			break;
		case "Ram":
			result.resultNumber = deviceInfo.TotalRam;
			break;
		case "DisplayWidth":
			result.resultNumber = deviceInfo.DisplayWidth;
			break;
		case "DisplayHeight":
			result.resultNumber = deviceInfo.DisplayHeight;
			break;
		case "Account":
			result.resultSTR = GameCenterController.GetUserId();
			break;
		case "China":
			result.resultNumber = (AssemblyController.GetMarket().GetIsChinaMarket() ? 1 : 0);
			break;
		case "Korea":
			result.resultNumber = (AssemblyController.GetMarket().GetIsKoreaMarket() ? 1 : 0);
			break;
		case "Amazon":
			result.resultNumber = (AssemblyController.GetMarket().GetIsAmazonMarket() ? 1 : 0);
			break;
		case "Steam":
			result.resultNumber = (AssemblyController.GetMarket().GetIsSteamMarket() ? 1 : 0);
			break;
		case "Japan":
			result.resultNumber = (AssemblyController.GetMarket().GetIsJapanMarket() ? 1 : 0);
			break;
		case "AmazonMobile":
			result.resultNumber = (AssemblyController.GetMarket().GetIsAmazonMobileMarket() ? 1 : 0);
			break;
		case "AndroidTV":
			result.resultNumber = (AssemblyController.GetMarket().GetIsAndroidTvMarket() ? 1 : 0);
			break;
		case "WinStore":
			result.resultNumber = (AssemblyController.GetMarket().GetIsWinStoreMarket() ? 1 : 0);
			break;
		case "Time":
			result.resultSTR = ListSF.GetCurrentTime().ToString();
			result.resultSTR = "0";
			break;
		case "RatingUrl":
			result.resultSTR = InternetController.GetRateUrl();
			break;
		case "FacebookLiked":
			result.resultNumber = (ListSF.GetRoster().GetFacebookLiked() ? 1 : 0);
			break;
		case "FBLikeUrl":
			result.resultSTR = InternetController.GetLikeUrls().Url;
			break;
		case "FBLikeAltUrl":
			result.resultSTR = InternetController.GetLikeUrls().AltUrl;
			break;
		case "UserObserved":
			result.resultNumber = (EventLog.GetInstance().GetIsLogging() ? 1 : 0);
			break;
		case "Connection":
			result.resultNumber = (SystemProperties.CheckConnection() ? 1 : 0);
			break;
		case "QualityCondition":
			result.resultSTR = GraphicsController.GetEffectiveQualityCondition();
			break;
		case "DeviceTotalMem":
		{
			float num = SystemProperties.GetDeviceInfo().TotalRam / 1024;
			result.resultNumber = num;
			break;
		}
		case "LastSessionCrashed":
			result.resultNumber = (CrashBreadcrumbTracker.GetInstance().DidLastSessionCrash() ? 1 : 0);
			break;
		default:
			string compatibilityString;
			double compatibilityNumber;
			if (Eclipse.Content.QuestCompatibility.TryGetModernSysInfo(
				function.property, out compatibilityString, out compatibilityNumber))
			{
				result.resultSTR = compatibilityString;
				result.resultNumber = compatibilityNumber;
				break;
			}
			GameLog.Error(string.Format("{0},\"{1}\"", "QuestCondition.SysInfoFunction - unknown property ", function.property));
			break;
		}
	}

	private void EvaluateGiftFunction(QuestFunctions function, CompareResult result)
	{
		switch (function.property)
		{
		case "Exist":
			result.resultNumber = (NetworkController.GetInstance().GiveLoginService.WasGiveApplied ? 1 : 0);
			break;
		case "Money":
			result.resultNumber = NetworkController.GetInstance().GiveLoginService.MoneyAmount;
			break;
		case "Bonus":
			result.resultNumber = NetworkController.GetInstance().GiveLoginService.BonusAmount;
			break;
		case "Items":
			result.resultNumber = NetworkController.GetInstance().GiveLoginService.Items.Count;
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "QuestCondition.GiftFunction - unknown property ", function.property));
			break;
		}
	}

	private void EvaluateDeliverFunction(QuestFunctions function, CompareResult result)
	{
		EvaluatePurchaseFunction(function, result);
	}

	private void SessionSettings(QuestFunctions function, CompareResult result)
	{
		Roster roster = ListSF.GetRoster();
		if (roster.HasSessionSetting(function.property))
		{
			string text = roster.GetSettingsXML(function.property);
			result.resultNumber = (text.Equals(string.Empty) ? 0.0 : double.Parse(text));
		}
	}

	private void EvaluateTimerFunction(QuestFunctions function, CompareResult result)
	{
		string text = function.GetFirstArgument();
		ListSF listInstance = ListSF.GetInstance();
		Roster roster = ListSF.GetRoster();
		if (!function.property.Equals("Value"))
		{
			return;
		}
		if (text.Equals("EnergyRefillTimer"))
		{
			result.resultNumber = roster.GetEnergyRefillTimer();
			return;
		}
		if (text.Equals("DuelAccessibilityTimer"))
		{
			if (BattlePeriodic.GetTime() > 0)
			{
				result.resultNumber = BattlePeriodic.GetRepeatTime() - BattlePeriodic.GetTime();
			}
			return;
		}
		if (text.Equals("StarterPackTimer"))
		{
			result.resultNumber = GameUtils.GetLeftTime(roster.GetStarterPackTimerEndTime());
			return;
		}
		RosterTimerContainer timerContainer = roster.GetTimerContainer();
		RosterTimer timer = timerContainer.FindTimer(text);
		if (timer != null)
		{
			result.resultNumber = GameUtils.GetLeftTime(timer.GetEndTimeSeconds());
		}
	}

	private void EvaluateAbGroupExistsFunction(QuestFunctions function, CompareResult result)
	{
		Roster roster = ListSF.GetRoster();
		string groupName = function.GetFirstArgument();
		result.resultNumber = (roster.DoesAbGroupExist(groupName) ? 1 : 0);
	}

	private void EvaluateEnchantmentFunction(QuestFunctions function, CompareResult result)
	{
		string[] array = function.GetFirstArgument().Split('|');
		if (array.Length < 2)
		{
			return;
		}
		string expression = array[0];
		CompareResult valueResult = new CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(questParameters);
		condition.SetValue(expression, valueResult);
		expression = valueResult.resultSTR;
		string recipeName = array[1];
		condition.SetValue(recipeName, valueResult);
		recipeName = valueResult.resultSTR;
		long num = 0L;
		if (array.Length == 3)
		{
			string countText = array[2];
			condition.SetValue(countText, valueResult);
			num = (long)valueResult.resultNumber;
		}
		switch (function.property)
		{
		case "Item":
			result.resultSTR = expression;
			break;
		case "Recipe":
			result.resultSTR = recipeName;
			break;
		case "Timeout":
		{
			RecipePrice recipePrice = ForgeManager.GetInstance().GetPriceByItemName(expression, recipeName);
			result.resultNumber = ((recipePrice != null) ? recipePrice.DeliveryTimeSeconds : 0);
			break;
		}
		case "DeliveryTime":
			result.resultNumber = num;
			break;
		case "BonusDeliveryPrice":
		{
			RecipePrice recipePrice = ForgeManager.GetInstance().GetPriceByItemName(expression, recipeName);
			result.resultNumber = (int)((recipePrice != null) ? (ObscuredLong)(recipePrice.BonusDeliveryPriceValue) : 0);
			break;
		}
		case "Available":
		{
			Recipe recipe = ForgeManager.GetInstance().GetRecipeByName(recipeName);
			UserItem userItem = ListSF.GetUserItem(expression);
			bool flag = false;
			if (recipe != null && userItem != null)
			{
				flag = recipe.IsAvailableWithMaterials(userItem);
			}
			result.resultNumber = (flag ? 1 : 0);
			break;
		}
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "QuestCondition.EnchantmentFunction - unknown property ", function.property));
			break;
		}
	}

	private string GetPurchaseUnsuccessfulValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		ItemInfo itemInfo = questParameters.purchasedItem;
		if (itemInfo != null)
		{
			stringBuilder.Append(itemInfo.Name);
			if (!questParameters.purchaseFailureReason.Equals(string.Empty))
			{
				stringBuilder.Append("|");
				stringBuilder.Append(questParameters.purchaseFailureReason);
			}
		}
		return stringBuilder.ToString();
	}

	private string GetEnchantmentValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!questParameters.enchantment.itemName.Equals(string.Empty))
		{
			long num = 0L;
			if (questParameters.enchantment.endTimestamp > 0)
			{
				long num2 = ListSF.GetServerTime();
				num = questParameters.enchantment.endTimestamp - num2;
				if (num < 0)
				{
					num = 0L;
				}
			}
			stringBuilder.Append(questParameters.enchantment.itemName);
			stringBuilder.Append("|");
			stringBuilder.Append(questParameters.enchantment.recipeName);
			stringBuilder.Append("|");
			if (num > 0)
			{
				stringBuilder.Append(num);
			}
		}
		return stringBuilder.ToString();
	}

	private void EvaluateFightCurrencyCostFunction(QuestFunctions function, CompareResult result)
	{
		string text = ((function.arguments.Count <= 0) ? string.Empty : function.arguments[0].result);
		string text2 = ((function.arguments.Count <= 1) ? string.Empty : function.arguments[1].result);
		FightList fight = ListSF.GetInstance().GetFightByIdString(text);
		if (fight == null)
		{
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition::fightCurrencyCostFunction - cant fight fight: ", text));
		}
		else if (text2.Equals(string.Empty))
		{
			GameLog.Error("ERROR: QuestCondition::fightCurrencyCostFunction - currencyName is empty");
		}
		else
		{
			result.resultNumber = fight.GetCurrencyCost(text2);
		}
	}

	private void EvaluateShopAssertFunction(QuestFunctions function, CompareResult result)
	{
		string lockName = function.GetFirstArgument();
		string propertyName = function.property;
		if (propertyName.Equals("IsOpen"))
		{
			Roster roster = ListSF.GetRoster();
			bool flag = roster.HasShopLock(lockName);
			result.resultNumber = (flag ? 1 : 0);
		}
		else
		{
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition::shopAssertFunction - unknown property: ", propertyName));
		}
	}

	private void EvaluateSimOperatorFunction(QuestFunctions function, CompareResult result)
	{
		string text = function.GetFirstArgument();
		string propertyName = function.property;
		if (propertyName != null && propertyName == "Code")
		{
			result.resultSTR = string.Empty;
		}
		else
		{
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition::getSimOperator - unknown property: ", function.property));
		}
	}

	private void EvaluateRaidInfoFunction(QuestFunctions function, CompareResult result)
	{
		switch (function.property)
		{
		case "TutorialStep":
			result.resultSTR = GameUtils.RaidTutorialStepNames[ListSF.GetRoster().GetTutorials().GetRaidStep()];
			break;
		case "RestoreCurrencyValue":
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition::getSimOperator - unknown property: ", function.property));
			break;
		}
	}
}
