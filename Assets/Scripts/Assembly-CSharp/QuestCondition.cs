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

	public static ComparisonType ParseComparison(string LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
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

	public static LogicalOperator ParseLogicalOperator(string FDHOMBHPNEF)
	{
		switch (FDHOMBHPNEF)
		{
		case "Or":
			return LogicalOperator.QUEST_CONDITION_SUB_OR;
		case "And":
			return LogicalOperator.QUEST_CONDITION_SUB_AND;
		default:
			return LogicalOperator.QUEST_CONDITION_SUB_NONE;
		}
	}

	public virtual void Parse(XmlNode BGPKIKNPIKP)
	{
		isNot = XmlUtils.ParseBool(BGPKIKNPIKP.Attributes["Not"]);
		_compareVersions = XmlUtils.ParseString(BGPKIKNPIKP.Attributes["CompareType"]) == "Versions";
		comparison = ParseComparison(BGPKIKNPIKP.Name);
		logicalOperator = ParseLogicalOperator(XmlUtils.ParseString(BGPKIKNPIKP.Attributes["Type"]));
		value1 = ClearGaps(XmlUtils.ParseString(BGPKIKNPIKP.Attributes["Value1"]));
		value2 = ClearGaps(XmlUtils.ParseString(BGPKIKNPIKP.Attributes["Value2"]));
	}

	public bool Compare(QuestParameters GFIHPBCEEOB, RosterQuest HFHCJABFEPE)
	{
		bool dCJLKCFKCOM = false;
		if (comparison != ComparisonType.QUEST_CONDITION_OPERATOR)
		{
			dCJLKCFKCOM = IsCompare(GFIHPBCEEOB, HFHCJABFEPE);
		}
		else
		{
			foreach (QuestCondition item in conditions)
			{
				bool flag = item.Compare(GFIHPBCEEOB, HFHCJABFEPE);
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
		return IsNotCompare(dCJLKCFKCOM);
	}

	public void SetParameters(QuestParameters JCICKLIMBEF)
	{
		questParameters = JCICKLIMBEF;
	}

	protected override void ResolveSessionVariable(string value, CompareResult BMDEBHIHIAJ)
	{
		if (questParameters != null)
		{
			switch (value)
			{
			case "_$Fight":
				BMDEBHIHIAJ.resultSTR = ((questParameters.GetFightList() == null) ? string.Empty : questParameters.GetFightList().FightId.ToString());
				break;
			case "_$Raid":
				BMDEBHIHIAJ.resultSTR = questParameters.raidId;
				break;
			case "_$FightResult":
				BMDEBHIHIAJ.resultSTR = questParameters.fightResult;
				break;
			case "_$RaidResult":
				BMDEBHIHIAJ.resultSTR = questParameters.raidResult;
				break;
			case "_$LevelUp":
				BMDEBHIHIAJ.resultNumber = questParameters.levelUp;
				break;
			case "_$ActionID":
				BMDEBHIHIAJ.resultSTR = ((questParameters.actionId == null) ? string.Empty : questParameters.actionId.Value);
				break;
			case "_$SceneFrom":
				BMDEBHIHIAJ.resultSTR = questParameters.sceneFrom;
				break;
			case "_$SceneTo":
				BMDEBHIHIAJ.resultSTR = questParameters.sceneTo;
				break;
			case "_$Purchase":
				BMDEBHIHIAJ.resultSTR = ((questParameters.purchasedItem == null) ? string.Empty : questParameters.purchasedItem.Name);
				break;
			case "_$PurchaseUnsuccessful":
				BMDEBHIHIAJ.resultSTR = GetPurchaseUnsuccessfulValue();
				break;
			case "_$Deliver":
				BMDEBHIHIAJ.resultSTR = ((questParameters.purchasedItem == null) ? string.Empty : questParameters.purchasedItem.Name);
				break;
			case "_$EnergyChange":
				BMDEBHIHIAJ.resultNumber = questParameters.energyChange;
				break;
			case "_$Iterator":
				BMDEBHIHIAJ.resultSTR = questParameters.iteratorValue;
				break;
			case "_$ChosenLocale":
				BMDEBHIHIAJ.resultSTR = ((questParameters.chosenLanguage == null) ? string.Empty : questParameters.chosenLanguage.name);
				break;
			case "_$ButtonType":
				BMDEBHIHIAJ.resultSTR = questParameters.buttonType;
				break;
			case "_$TabTo":
				BMDEBHIHIAJ.resultSTR = questParameters.tabTo;
				break;
			case "_$TabFrom":
				BMDEBHIHIAJ.resultSTR = questParameters.tabFrom;
				break;
			case "_$FightAvgFPS":
				BMDEBHIHIAJ.resultSTR = questParameters.fightAvgFps.ToString();
				break;
			case "_$TimerName":
				BMDEBHIHIAJ.resultSTR = questParameters.timerName.ToString();
				break;
			case "_$ButtonName":
				BMDEBHIHIAJ.resultSTR = questParameters.buttonName;
				break;
			case "_$PackName":
				BMDEBHIHIAJ.resultSTR = questParameters.packName;
				break;
			case "_$GemsPrice":
				BMDEBHIHIAJ.resultNumber = questParameters.gemsPrice;
				break;
			case "_$Enchantment":
				BMDEBHIHIAJ.resultSTR = GetEnchantmentValue();
				break;
			case "_$PerkName":
				BMDEBHIHIAJ.resultSTR = questParameters.perkName;
				break;
			case "_$LotteryLastSpinNumber":
				BMDEBHIHIAJ.resultSTR = questParameters.lotteryLastSpinNumber.ToString();
				break;
			case "_$InLottery":
				BMDEBHIHIAJ.resultSTR = questParameters.inLottery.ToString();
				break;
			case "_$SetItem":
				BMDEBHIHIAJ.resultSTR = questParameters.setItemName.ToString();
				break;
			case "_$StoryTutorialStep":
				BMDEBHIHIAJ.resultSTR = ListSF.GetRoster().GetTutorials().GetStoryStep();
				break;
			default:
			{
				Roster nKGLHEGIKKP = ListSF.GetRoster();
				RosterQuest.QuestVariable nOKCOAHJIPB = nKGLHEGIKKP.FindQuestVariable(value);
				string bAINMLLIKOL = ((nOKCOAHJIPB == null) ? value : nOKCOAHJIPB.Value);
				SetLiteralValue(bAINMLLIKOL, BMDEBHIHIAJ);
				break;
			}
			}
		}
		if (!BMDEBHIHIAJ.resultSTR.Equals(string.Empty) && GetVariableType(BMDEBHIHIAJ.resultSTR) == QuestVariableType.QUEST_CONDITION_VARIABLE_NUMBER)
		{
			string iBBAMMHHBFE = BMDEBHIHIAJ.resultSTR;
			BMDEBHIHIAJ.resultSTR = string.Empty;
			BMDEBHIHIAJ.resultNumber = float.Parse(iBBAMMHHBFE);
		}
	}

	private bool IsCompare(QuestParameters GFIHPBCEEOB, RosterQuest HFHCJABFEPE)
	{
		this.questParameters = GFIHPBCEEOB;
		this.rosterQuest = HFHCJABFEPE;
		CompareResult lNIDLHOIHIM = new CompareResult();
		CompareResult lNIDLHOIHIM2 = new CompareResult();
		SetValue(value1, lNIDLHOIHIM);
		SetValue(value2, lNIDLHOIHIM2);
		return CompareResults(lNIDLHOIHIM, lNIDLHOIHIM2);
	}

	private bool CompareResults(CompareResult HJJBNECFJGO, CompareResult KAGCCGKOPFM)
	{
		if (_compareVersions)
		{
			VersionContainer left = new VersionContainer(HJJBNECFJGO.ToString());
			VersionContainer right = new VersionContainer(KAGCCGKOPFM.ToString());
			int order = VersionContainer.IsEqual(left, right) ? 0 :
				VersionContainer.IsGreater(left, right) ? 1 : -1;
			return NumberCompare(order, 0);
		}
		if (!HJJBNECFJGO.IsNumber() && !KAGCCGKOPFM.IsNumber())
		{
			return StringCompare(HJJBNECFJGO.resultSTR, KAGCCGKOPFM.resultSTR);
		}
		if (HJJBNECFJGO.IsNumber() && KAGCCGKOPFM.IsNumber())
		{
			return NumberCompare((int)HJJBNECFJGO.resultNumber, (int)KAGCCGKOPFM.resultNumber);
		}
		string oFJMLPGDNKP = HJJBNECFJGO.ToString();
		string iJHJOLLMOHA = KAGCCGKOPFM.ToString();
		return StringCompare(oFJMLPGDNKP, iJHJOLLMOHA);
	}

	private bool StringCompare(string OFJMLPGDNKP, string IJHJOLLMOHA)
	{
		return OFJMLPGDNKP.Equals(IJHJOLLMOHA);
	}

	private bool NumberCompare(int ADADNFFCFII, int ILCHIGNGLPL)
	{
		switch (comparison)
		{
		case ComparisonType.QUEST_CONDITION_EQUAL:
			return ADADNFFCFII == ILCHIGNGLPL;
		case ComparisonType.QUEST_CONDITION_GREATER:
			return ADADNFFCFII > ILCHIGNGLPL;
		case ComparisonType.QUEST_CONDITION_GREATER_EQUAL:
			return ADADNFFCFII >= ILCHIGNGLPL;
		case ComparisonType.QUEST_CONDITION_LESS:
			return ADADNFFCFII < ILCHIGNGLPL;
		case ComparisonType.QUEST_CONDITION_LESS_EQUAL:
			return ADADNFFCFII <= ILCHIGNGLPL;
		default:
			return false;
		}
	}

	private bool IsNotCompare(bool DCJLKCFKCOM)
	{
		return isNot ? (!DCJLKCFKCOM) : DCJLKCFKCOM;
	}

	protected override void FullFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		BMDEBHIHIAJ.Clear();
		switch (KJFKPMCPIBH.functionName)
		{
		case "Fight":
			EvaluateFightFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "Player":
			EvaluatePlayerFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "Item":
			EvaluateItemFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "PackAssert":
			EvaluatePackAssertFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "UniformIntRandom":
			MathFunction(KJFKPMCPIBH, BMDEBHIHIAJ, MathFunctionType.MATH_RAND);
			break;
		case "RandomAspect":
			RandomAspect(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "PerkInfo":
			PerkInfo(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "Purchase":
			EvaluatePurchaseFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "ItemsOfType":
			EvaluateItemsOfTypeFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "Battle":
			EvaluateBattleFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "DataVersion":
			EvaluateDataVersionFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "VersionController":
			EvaluateVersionControllerFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "SysInfo":
			EvaluateSysInfoFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "Deliver":
			EvaluateDeliverFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "SessionSettings":
			SessionSettings(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "Sub":
			MathFunction(KJFKPMCPIBH, BMDEBHIHIAJ, MathFunctionType.MATH_SUB);
			break;
		case "Sum":
			MathFunction(KJFKPMCPIBH, BMDEBHIHIAJ, MathFunctionType.MATH_SUM);
			break;
		case "Multi":
			MathFunction(KJFKPMCPIBH, BMDEBHIHIAJ, MathFunctionType.MATH_MULTI);
			break;
		case "Div":
			MathFunction(KJFKPMCPIBH, BMDEBHIHIAJ, MathFunctionType.MATH_DIVISION);
			break;
		case "NDiv":
			MathFunction(KJFKPMCPIBH, BMDEBHIHIAJ, MathFunctionType.MATH_DIVISION_INT);
			break;
		case "Mod":
			MathFunction(KJFKPMCPIBH, BMDEBHIHIAJ, MathFunctionType.MATH_MOD);
			break;
		case "Concat":
			StringFunction(KJFKPMCPIBH, BMDEBHIHIAJ, StringFunctionType.STRING_CONCAT);
			break;
		case "Slice":
			StringFunction(KJFKPMCPIBH, BMDEBHIHIAJ, StringFunctionType.STRING_SLICE);
			break;
		case "Gift":
			EvaluateGiftFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "Timer":
			EvaluateTimerFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "ABGroupExists":
			EvaluateAbGroupExistsFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "Enchantment":
			EvaluateEnchantmentFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "FightCurrencyCost":
			EvaluateFightCurrencyCostFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "ShopAssert":
			EvaluateShopAssertFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "GetSimOperator":
			EvaluateSimOperatorFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		case "RaidInfo":
			EvaluateRaidInfoFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
			break;
		default:
			GameLog.Error(string.Format("{0},{1}", "QuestCondition::fullFunction - unknown function: ", KJFKPMCPIBH.functionName));
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

	private void EvaluateFightFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		string text = KJFKPMCPIBH.GetFirstArgument();
		FightList jDIPBIHBGPF = ListSF.GetInstance().GetFightByIdString(text);
		if (jDIPBIHBGPF == null)
		{
			GameLog.Write(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.FightFunction - cant fight fight: ", text));
			return;
		}
		string hBDLDIKHFEG = KJFKPMCPIBH.property;
		switch (hBDLDIKHFEG)
		{
		case "Name":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.FightId.ToString();
			break;
		case "Zone":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.FightId.GetZone();
			break;
		case "Battle":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.FightId.GetBattle();
			break;
		case "Fight":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.FightId.GetFight();
			break;
		case "Money":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.PrizeMoney.ToString();
			break;
		case "Bonus":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.PrizeBonus.ToString();
			break;
		case "Type":
			BMDEBHIHIAJ.resultSTR = ListSF.GetInstance().GetBattleTypeName(jDIPBIHBGPF.get_Type());
			break;
		case "LossCount":
			BMDEBHIHIAJ.resultNumber = ((jDIPBIHBGPF.GetRosterFight() != null) ? jDIPBIHBGPF.GetRosterFight().GetLossCount() : 0);
			break;
		case "WinCount":
			BMDEBHIHIAJ.resultNumber = ((jDIPBIHBGPF.GetRosterFight() != null) ? jDIPBIHBGPF.GetRosterFight().GetWinCount() : 0);
			break;
		case "TimeLeft":
			BMDEBHIHIAJ.resultNumber = jDIPBIHBGPF.GetTimeLeft();
			break;
		case "Difficulty":
			BMDEBHIHIAJ.resultNumber = GameUtils.GetFightDifficulty(jDIPBIHBGPF);
			break;
		case "Description":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.GetDescription();
			break;
		case "Helm":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.GetRuleItemName("Helm");
			break;
		case "Weapon":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.GetRuleItemName("Weapon");
			break;
		case "Armor":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.GetRuleItemName("Armor");
			break;
		case "Magic":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.GetRuleItemName("Magic");
			break;
		case "RaidCharge":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.GetRuleItemName("RaidCharge");
			break;
		case "Ranged":
			BMDEBHIHIAJ.resultSTR = jDIPBIHBGPF.GetRuleItemName("Ranged");
			break;
		case "HelmLevel":
			BMDEBHIHIAJ.resultNumber = jDIPBIHBGPF.GetRuleItemLevel("Helm");
			break;
		case "WeaponLevel":
			BMDEBHIHIAJ.resultNumber = jDIPBIHBGPF.GetRuleItemLevel("Weapon");
			break;
		case "ArmorLevel":
			BMDEBHIHIAJ.resultNumber = jDIPBIHBGPF.GetRuleItemLevel("Armor");
			break;
		case "MagicLevel":
			BMDEBHIHIAJ.resultNumber = jDIPBIHBGPF.GetRuleItemLevel("Magic");
			break;
		case "RaidChargeLevel":
			BMDEBHIHIAJ.resultNumber = jDIPBIHBGPF.GetRuleItemLevel("RaidCharge");
			break;
		case "RangedLevel":
			BMDEBHIHIAJ.resultNumber = jDIPBIHBGPF.GetRuleItemLevel("Ranged");
			break;
		case "CheckCurrency":
			BMDEBHIHIAJ.resultNumber = Convert.ToDouble(jDIPBIHBGPF.HasCurrencyCost());
			break;
		case "EnoughCurrency":
			BMDEBHIHIAJ.resultNumber = Convert.ToDouble(GameUtils.HasEnoughCurrencyForFight(jDIPBIHBGPF));
			break;
		case "Timestamp":
			BMDEBHIHIAJ.resultNumber = ((jDIPBIHBGPF.GetRosterFight() == null) ? 0 : jDIPBIHBGPF.GetRosterFight().GetCompletionTimestamp());
			break;
		case "Level":
			BMDEBHIHIAJ.resultNumber = ((jDIPBIHBGPF.GetRosterFight() != null) ? jDIPBIHBGPF.GetRosterFight().GetLevel() : 0);
			break;
		case "Power":
			BMDEBHIHIAJ.resultNumber = jDIPBIHBGPF.GetPowerRequired();
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.FightFunction - unknown property: ", hBDLDIKHFEG));
			break;
		}
	}

	private void EvaluatePlayerFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP == null)
		{
			return;
		}
		switch (KJFKPMCPIBH.property)
		{
		case "Skeleton":
		{
			ItemInfo dJKEECEOCJB6 = nKGLHEGIKKP.get_Parameters().GetItemByType("Skeleton");
			if (dJKEECEOCJB6 != null)
			{
				BMDEBHIHIAJ.resultSTR = dJKEECEOCJB6.Name;
			}
			break;
		}
		case "Helm":
		{
			ItemInfo dJKEECEOCJB2 = nKGLHEGIKKP.get_Parameters().GetItemByType("Helm");
			if (dJKEECEOCJB2 != null)
			{
				BMDEBHIHIAJ.resultSTR = dJKEECEOCJB2.Name;
			}
			break;
		}
		case "Armor":
		{
			ItemInfo dJKEECEOCJB4 = nKGLHEGIKKP.get_Parameters().GetItemByType("Armor");
			if (dJKEECEOCJB4 != null)
			{
				BMDEBHIHIAJ.resultSTR = dJKEECEOCJB4.Name;
			}
			break;
		}
		case "Weapon":
		{
			ItemInfo dJKEECEOCJB7 = nKGLHEGIKKP.get_Parameters().GetItemByType("Weapon");
			if (dJKEECEOCJB7 != null)
			{
				BMDEBHIHIAJ.resultSTR = dJKEECEOCJB7.Name;
			}
			break;
		}
		case "Magic":
		{
			ItemInfo dJKEECEOCJB5 = nKGLHEGIKKP.get_Parameters().GetItemByType("Magic");
			if (dJKEECEOCJB5 != null)
			{
				BMDEBHIHIAJ.resultSTR = dJKEECEOCJB5.Name;
			}
			break;
		}
		case "RaidCharge":
		{
			ItemInfo dJKEECEOCJB3 = nKGLHEGIKKP.get_Parameters().GetItemByType("RaidConsumable");
			if (dJKEECEOCJB3 != null)
			{
				BMDEBHIHIAJ.resultSTR = dJKEECEOCJB3.Name;
			}
			break;
		}
		case "Ranged":
		{
			ItemInfo dJKEECEOCJB = nKGLHEGIKKP.get_Parameters().GetItemByType("Ranged");
			if (dJKEECEOCJB != null)
			{
				BMDEBHIHIAJ.resultSTR = dJKEECEOCJB.Name;
			}
			break;
		}
		case "Money":
			BMDEBHIHIAJ.resultNumber = nKGLHEGIKKP.GetMoney();
			break;
		case "Bonus":
			BMDEBHIHIAJ.resultNumber = nKGLHEGIKKP.GetBonus();
			break;
		case "Level":
			BMDEBHIHIAJ.resultNumber = nKGLHEGIKKP.GetLevel();
			break;
		case "Power":
			BMDEBHIHIAJ.resultNumber = nKGLHEGIKKP.GetMaxPower();
			break;
		case "Language":
			BMDEBHIHIAJ.resultSTR = nKGLHEGIKKP.GetLanguage();
			break;
		case "CoinIcon":
			BMDEBHIHIAJ.resultSTR = nKGLHEGIKKP.GetCoinIcon();
			break;
		case "MapFocus":
		{
			FightIDS mOCEDDJOAEB2 = ListSF.GetRoster().GetMapFocus();
			BMDEBHIHIAJ.resultSTR = mOCEDDJOAEB2.GetZoneBattle();
			break;
		}
		case "RaidMapFocus":
		{
			FightIDS mOCEDDJOAEB = ListSF.GetRoster().GetRaidMapFocus();
			BMDEBHIHIAJ.resultSTR = mOCEDDJOAEB.GetZoneBattle();
			break;
		}
		case "IsLoggedInRaids":
			BMDEBHIHIAJ.resultNumber = Convert.ToDouble(RaidLoginState.GetInstance().GetIsLoggedIn());
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.PlayerFunction - unknown property: ", KJFKPMCPIBH.property));
			break;
		}
	}

	private void EvaluateItemFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		string gOHIIMFFFJI = KJFKPMCPIBH.GetFirstArgument();
		ItemInfo dJKEECEOCJB = ListSF.GetItems().GetItemByName(gOHIIMFFFJI);
		if (dJKEECEOCJB == null)
		{
			return;
		}
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		switch (KJFKPMCPIBH.property)
		{
		case "Price":
			BMDEBHIHIAJ.resultNumber = (ObscuredLong)(dJKEECEOCJB.CoinPrice);
			break;
		case "BonusPrice":
			BMDEBHIHIAJ.resultNumber = (ObscuredLong)(dJKEECEOCJB.GemPrice);
			break;
		case "BonusDeliveryPrice":
			BMDEBHIHIAJ.resultNumber = (ObscuredLong)(dJKEECEOCJB.DeliveryGemPrice);
			break;
		case "MoneyDeliveryPrice":
			BMDEBHIHIAJ.resultNumber = (ObscuredLong)(dJKEECEOCJB.DeliveryCoinPrice);
			break;
		case "Level":
			BMDEBHIHIAJ.resultNumber = dJKEECEOCJB.ItemLevel;
			break;
		case "Equipped":
		{
			UserItem dKCHDHMLKHN2 = nKGLHEGIKKP.GetInventory().FindItem(dJKEECEOCJB);
			if (dKCHDHMLKHN2 != null)
			{
				BMDEBHIHIAJ.resultNumber = Convert.ToDouble(dKCHDHMLKHN2.GetIsEquipped());
			}
			break;
		}
		case "Quantity":
		{
			UserItem dKCHDHMLKHN = nKGLHEGIKKP.GetInventory().FindItem(dJKEECEOCJB);
			BMDEBHIHIAJ.resultNumber = ((dKCHDHMLKHN != null) ? dKCHDHMLKHN.GetCount() : 0);
			break;
		}
		case "NextMoneyUpgradePrice":
		{
			ItemInfo mBIJKDIEFIF = GetNextUpgradeInfo(dJKEECEOCJB);
			BMDEBHIHIAJ.resultNumber = GetItemPrice(mBIJKDIEFIF, true);
			break;
		}
		case "NextBonusUpgradePrice":
		{
			ItemInfo mBIJKDIEFIF2 = GetNextUpgradeInfo(dJKEECEOCJB);
			BMDEBHIHIAJ.resultNumber = GetItemPrice(mBIJKDIEFIF2, false);
			break;
		}
		case "NextUpgradeDeliveryPrice":
		{
			ItemInfo mBIJKDIEFIF4 = GetNextUpgradeInfo(dJKEECEOCJB);
			BMDEBHIHIAJ.resultNumber = GetUpgradePrice(mBIJKDIEFIF4, false);
			break;
		}
		case "NextUpgradeDeliveryTime":
		{
			ItemInfo mBIJKDIEFIF3 = GetNextUpgradeInfo(dJKEECEOCJB);
			BMDEBHIHIAJ.resultNumber = GetDeliveryUpgradeTime(mBIJKDIEFIF3);
			break;
		}
		case "Type":
			BMDEBHIHIAJ.resultSTR = dJKEECEOCJB.Type;
			break;
		case "SubType":
			BMDEBHIHIAJ.resultSTR = dJKEECEOCJB.SubType;
			break;
		case "Availability":
		{
			BMDEBHIHIAJ.resultNumber = ShopAvailabilityPolicy.IsAvailable(dJKEECEOCJB, nKGLHEGIKKP) ? 1 : 0;
			break;
		}
		case "RealPrice":
			BMDEBHIHIAJ.resultSTR = dJKEECEOCJB.LocalizedPriceString;
			break;
		case "RecieveGold":
			BMDEBHIHIAJ.resultSTR = dJKEECEOCJB.ReceiveGold.ToString();
			break;
		case "RecieveBonus":
			BMDEBHIHIAJ.resultSTR = dJKEECEOCJB.ReceiveBonus.ToString();
			break;
		case "Image":
			BMDEBHIHIAJ.resultSTR = dJKEECEOCJB.FileName;
			break;
		case "PackLabel":
			BMDEBHIHIAJ.resultSTR = dJKEECEOCJB.GroupId;
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.ItemFunction - unknown property: ", KJFKPMCPIBH.property));
			break;
		}
	}

	private ItemInfo GetNextUpgradeInfo(ItemInfo item)
	{
		ItemInfo dJKEECEOCJB = null;
		UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(item);
		if (dKCHDHMLKHN != null && dKCHDHMLKHN.GetDeliveryTimestamp() > 0 && dKCHDHMLKHN.GetDeliveryUpgradeLevel() > 0)
		{
			ItemInfo dJKEECEOCJB2 = dKCHDHMLKHN.GetEffectiveInfo();
			dJKEECEOCJB = ((dJKEECEOCJB2 == null) ? null : dJKEECEOCJB2.Clone());
		}
		if (dJKEECEOCJB != null)
		{
			dJKEECEOCJB = ((dKCHDHMLKHN == null) ? item.GetUpgradeItemByIndex(0) : dKCHDHMLKHN.GetNextUpgradeItem().Clone());
		}
		return dJKEECEOCJB;
	}

	private long GetItemPrice(ItemInfo item, bool EDNGDDEPAPA)
	{
		long result = 2147483647L;
		if (item != null)
		{
			if ((EDNGDDEPAPA && item.HasCoinPrice()) || (!EDNGDDEPAPA && item.HasGemPrice()))
			{
				result = (ObscuredLong)((!EDNGDDEPAPA) ? item.GemPrice : item.CoinPrice);
			}
			else
			{
				GameLog.Write(string.Format("{0},{2},{1},{3}", "QuestCondition::itemFunction ", " price - no price: ", (!EDNGDDEPAPA) ? "bonus" : "money", item.Name));
			}
		}
		else
		{
			GameLog.Write(string.Format("{0},{2},{1}", "QuestCondition::itemFunction ", " price - no next upgrade", (!EDNGDDEPAPA) ? "bonus" : "money"));
		}
		return result;
	}

	private long GetUpgradePrice(ItemInfo item, bool EDNGDDEPAPA)
	{
		long result = 2147483647L;
		if (item != null)
		{
			result = (ObscuredLong)((!EDNGDDEPAPA) ? item.DeliveryGemPrice : item.DeliveryCoinPrice);
		}
		else
		{
			GameLog.Write(string.Format("{0},{2},{1}", "QuestCondition::itemFunction ", " price - no next upgrade", (!EDNGDDEPAPA) ? "bonus" : "money"));
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

	private void EvaluatePackAssertFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		string gOHIIMFFFJI = KJFKPMCPIBH.GetFirstArgument();
		switch (KJFKPMCPIBH.property)
		{
		case "Availability":
		{
			bool flag = PacksController.GetInstance().IsPackByName(gOHIIMFFFJI);
			BMDEBHIHIAJ.resultNumber = (flag ? 1 : 0);
			break;
		}
		case "Existence":
		{
			DownloadPack jBKAOMLJCEL2 = PacksController.GetInstance().FindPack(gOHIIMFFFJI);
			BMDEBHIHIAJ.resultNumber = ((jBKAOMLJCEL2 != null) ? 1 : 0);
			break;
		}
		case "Size":
		{
			DownloadPack jBKAOMLJCEL = GeneralConfig.DownloadPacks.FindPack(gOHIIMFFFJI);
			if (jBKAOMLJCEL != null)
			{
				BMDEBHIHIAJ.resultSTR = jBKAOMLJCEL.Size;
			}
			break;
		}
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.PackFunction - unknown property: ", KJFKPMCPIBH.property));
			break;
		}
	}

	private void EvaluatePurchaseFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		string[] array = KJFKPMCPIBH.GetFirstArgument().Split('|');
		if (array.Length != 0)
		{
			string gOHIIMFFFJI = array[0];
			string iBBAMMHHBFE = ((array.Length <= 1) ? string.Empty : array[1]);
			UserItem dKCHDHMLKHN = ListSF.GetUserItem(gOHIIMFFFJI);
			ItemInfo dJKEECEOCJB = ((dKCHDHMLKHN == null) ? ListSF.GetItems().GetItemByName(gOHIIMFFFJI) : dKCHDHMLKHN.GetInfo());
			switch (KJFKPMCPIBH.property)
			{
			case "Type":
				BMDEBHIHIAJ.resultSTR = ((dJKEECEOCJB == null) ? string.Empty : dJKEECEOCJB.Type);
				break;
			case "Name":
				BMDEBHIHIAJ.resultSTR = ((dJKEECEOCJB == null) ? string.Empty : dJKEECEOCJB.Name);
				break;
			case "UpgradeLevel":
				BMDEBHIHIAJ.resultNumber = ((dKCHDHMLKHN != null) ? dKCHDHMLKHN.GetUpgradeLevel() : 0);
				break;
			case "Timeout":
			{
				long num = ((dKCHDHMLKHN == null) ? 0 : dKCHDHMLKHN.GetDeliveryTimestamp());
				long num2 = GameUtils.GetCurrentTime();
				long num3 = ((num <= num2) ? 0 : (num - num2));
				BMDEBHIHIAJ.resultNumber = num3;
				break;
			}
			case "Failure":
				BMDEBHIHIAJ.resultSTR = iBBAMMHHBFE;
				break;
			case "PaidItem":
				BMDEBHIHIAJ.resultSTR = ((dJKEECEOCJB == null) ? string.Empty : dJKEECEOCJB.LegacyPaidItem);
				break;
			default:
				GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.PurchaseFunction - unknown property: ", KJFKPMCPIBH.property));
				break;
			}
		}
	}

	private void EvaluateItemsOfTypeFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP != null)
		{
			string lFLGCDNKNJI = KJFKPMCPIBH.GetFirstArgument();
			List<UserItem> list = nKGLHEGIKKP.GetInventory().FindItemsByType(lFLGCDNKNJI, string.Empty);
			if (KJFKPMCPIBH.property.Equals("Quantity"))
			{
				BMDEBHIHIAJ.resultNumber = list.Count;
			}
		}
	}

	private void EvaluateBattleFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		string dIAIIPCBMFL = KJFKPMCPIBH.GetFirstArgument();
		FightIDS mOCEDDJOAEB = new FightIDS(dIAIIPCBMFL);
		switch (KJFKPMCPIBH.property)
		{
		case "Available":
		{
			Battle cGJCGEBPCAF3 = ListSF.GetBattleById(mOCEDDJOAEB);
			if (cGJCGEBPCAF3 != null)
			{
				Roster nKGLHEGIKKP = ListSF.GetRoster();
				bool flag = nKGLHEGIKKP.HasBattle(mOCEDDJOAEB);
				BMDEBHIHIAJ.resultNumber = (flag ? 1 : 0);
			}
			else
			{
				GameLog.Error(string.Format("{0},{1}", "Quest Error: no such battle in stages: ", mOCEDDJOAEB.ToString()));
				BMDEBHIHIAJ.resultNumber = 0.0;
			}
			break;
		}
		case "Locked":
		{
			Battle cGJCGEBPCAF5 = ListSF.GetBattleById(mOCEDDJOAEB);
			bool flag2 = true;
			if (cGJCGEBPCAF5 != null)
			{
				flag2 = cGJCGEBPCAF5.GetRosterBattle() == null || cGJCGEBPCAF5.GetRosterBattle().IsLocked();
			}
			BMDEBHIHIAJ.resultNumber = (flag2 ? 1 : 0);
			break;
		}
		case "Name":
		{
			Battle cGJCGEBPCAF4 = ListSF.GetBattleById(mOCEDDJOAEB);
			if (cGJCGEBPCAF4 != null)
			{
				BMDEBHIHIAJ.resultSTR = cGJCGEBPCAF4.get_Name();
				break;
			}
			GameLog.Error(string.Format("{0},{1}", "Quest Error: no such battle in stages: ", mOCEDDJOAEB.ToString()));
			BMDEBHIHIAJ.resultSTR = string.Empty;
			break;
		}
		case "Type":
		{
			Battle cGJCGEBPCAF2 = ListSF.GetBattleById(mOCEDDJOAEB);
			BattleType lFLGCDNKNJI = BattleType.FightDummy;
			if (cGJCGEBPCAF2 != null)
			{
				lFLGCDNKNJI = cGJCGEBPCAF2.get_Type();
			}
			else
			{
				GameLog.Error(string.Format("{0},{1}", "Quest Error: no such battle in stages: ", mOCEDDJOAEB.ToString()));
			}
			BMDEBHIHIAJ.resultSTR = ListSF.GetInstance().GetBattleTypeName(lFLGCDNKNJI);
			break;
		}
		case "Zone":
		{
			Battle cGJCGEBPCAF = ListSF.GetBattleById(mOCEDDJOAEB);
			if (cGJCGEBPCAF != null)
			{
				Zone pKCPOJKLMOK = cGJCGEBPCAF.GetZone();
				if (pKCPOJKLMOK != null)
				{
					BMDEBHIHIAJ.resultSTR = pKCPOJKLMOK.get_Name();
				}
				else
				{
					BMDEBHIHIAJ.resultSTR = string.Empty;
				}
			}
			else
			{
				GameLog.Error(string.Format("{0},{1}", "Quest Error: no such battle in stages: ", mOCEDDJOAEB.ToString()));
				BMDEBHIHIAJ.resultSTR = string.Empty;
			}
			break;
		}
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.BattleFunction - unknown property: ", KJFKPMCPIBH.property));
			break;
		}
	}

	private void EvaluateDataVersionFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		VersionContainer pAMHFPMEPCH = SystemProperties.GetDataVersion();
		switch (KJFKPMCPIBH.property)
		{
		case "Version":
			BMDEBHIHIAJ.resultSTR = pAMHFPMEPCH.ToString();
			break;
		case "Production":
			BMDEBHIHIAJ.resultNumber = pAMHFPMEPCH.GetMajor();
			break;
		case "Major":
			BMDEBHIHIAJ.resultNumber = pAMHFPMEPCH.GetMinor();
			break;
		case "Minor":
			BMDEBHIHIAJ.resultNumber = pAMHFPMEPCH.GetBuild();
			break;
		case "DataVersion":
			BMDEBHIHIAJ.resultNumber = pAMHFPMEPCH.GetRevision();
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.UserVersionFunction - unknown property: ", KJFKPMCPIBH.property));
			break;
		}
	}

	private void EvaluateVersionControllerFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		VersionContainer pAMHFPMEPCH = SystemProperties.GetVersion();
		switch (KJFKPMCPIBH.property)
		{
		case "Version":
			BMDEBHIHIAJ.resultSTR = pAMHFPMEPCH.ToString();
			break;
		case "Production":
			BMDEBHIHIAJ.resultNumber = pAMHFPMEPCH.GetMajor();
			break;
		case "Major":
			BMDEBHIHIAJ.resultNumber = pAMHFPMEPCH.GetMinor();
			break;
		case "Minor":
			BMDEBHIHIAJ.resultNumber = pAMHFPMEPCH.GetBuild();
			break;
		case "DataVersion":
			BMDEBHIHIAJ.resultNumber = pAMHFPMEPCH.GetRevision();
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition.VersionControllerFunction - unknown property: ", KJFKPMCPIBH.property));
			break;
		}
	}

	private void EvaluateSysInfoFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		DeviceInfo fFMKFOCMPBN = SystemProperties.GetDeviceInfo();
		switch (KJFKPMCPIBH.property)
		{
		case "DeviceType":
			if (SystemProperties.IsTabletDevice())
			{
				BMDEBHIHIAJ.resultSTR = "Tablet";
			}
			else
			{
				BMDEBHIHIAJ.resultSTR = "Phone";
			}
			break;
		case "ResolutionLocation":
			BMDEBHIHIAJ.resultSTR = SystemProperties.PathTypeToString(fFMKFOCMPBN.LocationResolution);
			break;
		case "ResolutionGUI":
			BMDEBHIHIAJ.resultSTR = SystemProperties.PathTypeToString(fFMKFOCMPBN.GuiResolution);
			break;
		case "Id":
			BMDEBHIHIAJ.resultSTR = fFMKFOCMPBN.Id;
			break;
		case "Os":
			BMDEBHIHIAJ.resultSTR = fFMKFOCMPBN.Os;
			break;
		case "OsName":
			BMDEBHIHIAJ.resultSTR = fFMKFOCMPBN.OsName;
			break;
		case "Language":
			BMDEBHIHIAJ.resultSTR = fFMKFOCMPBN.Locale;
			break;
		case "CpuCount":
			BMDEBHIHIAJ.resultNumber = fFMKFOCMPBN.CpuCount;
			break;
		case "Ram":
			BMDEBHIHIAJ.resultNumber = fFMKFOCMPBN.TotalRam;
			break;
		case "DisplayWidth":
			BMDEBHIHIAJ.resultNumber = fFMKFOCMPBN.DisplayWidth;
			break;
		case "DisplayHeight":
			BMDEBHIHIAJ.resultNumber = fFMKFOCMPBN.DisplayHeight;
			break;
		case "Account":
			BMDEBHIHIAJ.resultSTR = GameCenterController.GetUserId();
			break;
		case "China":
			BMDEBHIHIAJ.resultNumber = (AssemblyController.GetMarket().GetIsChinaMarket() ? 1 : 0);
			break;
		case "Korea":
			BMDEBHIHIAJ.resultNumber = (AssemblyController.GetMarket().GetIsKoreaMarket() ? 1 : 0);
			break;
		case "Amazon":
			BMDEBHIHIAJ.resultNumber = (AssemblyController.GetMarket().GetIsAmazonMarket() ? 1 : 0);
			break;
		case "Steam":
			BMDEBHIHIAJ.resultNumber = (AssemblyController.GetMarket().GetIsSteamMarket() ? 1 : 0);
			break;
		case "Japan":
			BMDEBHIHIAJ.resultNumber = (AssemblyController.GetMarket().GetIsJapanMarket() ? 1 : 0);
			break;
		case "AmazonMobile":
			BMDEBHIHIAJ.resultNumber = (AssemblyController.GetMarket().GetIsAmazonMobileMarket() ? 1 : 0);
			break;
		case "AndroidTV":
			BMDEBHIHIAJ.resultNumber = (AssemblyController.GetMarket().GetIsAndroidTvMarket() ? 1 : 0);
			break;
		case "WinStore":
			BMDEBHIHIAJ.resultNumber = (AssemblyController.GetMarket().GetIsWinStoreMarket() ? 1 : 0);
			break;
		case "Time":
			BMDEBHIHIAJ.resultSTR = ListSF.GetCurrentTime().ToString();
			BMDEBHIHIAJ.resultSTR = "0";
			break;
		case "RatingUrl":
			BMDEBHIHIAJ.resultSTR = InternetController.GetRateUrl();
			break;
		case "FacebookLiked":
			BMDEBHIHIAJ.resultNumber = (ListSF.GetRoster().GetFacebookLiked() ? 1 : 0);
			break;
		case "FBLikeUrl":
			BMDEBHIHIAJ.resultSTR = InternetController.GetLikeUrls().Url;
			break;
		case "FBLikeAltUrl":
			BMDEBHIHIAJ.resultSTR = InternetController.GetLikeUrls().AltUrl;
			break;
		case "UserObserved":
			BMDEBHIHIAJ.resultNumber = (EventLog.GetInstance().GetIsLogging() ? 1 : 0);
			break;
		case "Connection":
			BMDEBHIHIAJ.resultNumber = (SystemProperties.CheckConnection() ? 1 : 0);
			break;
		case "QualityCondition":
			BMDEBHIHIAJ.resultSTR = GraphicsController.GetEffectiveQualityCondition();
			break;
		case "DeviceTotalMem":
		{
			float num = SystemProperties.GetDeviceInfo().TotalRam / 1024;
			BMDEBHIHIAJ.resultNumber = num;
			break;
		}
		case "LastSessionCrashed":
			BMDEBHIHIAJ.resultNumber = (CrashBreadcrumbTracker.GetInstance().DidLastSessionCrash() ? 1 : 0);
			break;
		default:
			string compatibilityString;
			double compatibilityNumber;
			if (Eclipse.Content.QuestCompatibility.TryGetModernSysInfo(
				KJFKPMCPIBH.property, out compatibilityString, out compatibilityNumber))
			{
				BMDEBHIHIAJ.resultSTR = compatibilityString;
				BMDEBHIHIAJ.resultNumber = compatibilityNumber;
				break;
			}
			GameLog.Error(string.Format("{0},\"{1}\"", "QuestCondition.SysInfoFunction - unknown property ", KJFKPMCPIBH.property));
			break;
		}
	}

	private void EvaluateGiftFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		switch (KJFKPMCPIBH.property)
		{
		case "Exist":
			BMDEBHIHIAJ.resultNumber = (NetworkController.GetInstance().GiveLoginService.WasGiveApplied ? 1 : 0);
			break;
		case "Money":
			BMDEBHIHIAJ.resultNumber = NetworkController.GetInstance().GiveLoginService.MoneyAmount;
			break;
		case "Bonus":
			BMDEBHIHIAJ.resultNumber = NetworkController.GetInstance().GiveLoginService.BonusAmount;
			break;
		case "Items":
			BMDEBHIHIAJ.resultNumber = NetworkController.GetInstance().GiveLoginService.Items.Count;
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "QuestCondition.GiftFunction - unknown property ", KJFKPMCPIBH.property));
			break;
		}
	}

	private void EvaluateDeliverFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		EvaluatePurchaseFunction(KJFKPMCPIBH, BMDEBHIHIAJ);
	}

	private void SessionSettings(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP.HasSessionSetting(KJFKPMCPIBH.property))
		{
			string text = nKGLHEGIKKP.GetSettingsXML(KJFKPMCPIBH.property);
			BMDEBHIHIAJ.resultNumber = (text.Equals(string.Empty) ? 0.0 : double.Parse(text));
		}
	}

	private void EvaluateTimerFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		string text = KJFKPMCPIBH.GetFirstArgument();
		ListSF oPLPFMFAGMN = ListSF.GetInstance();
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (!KJFKPMCPIBH.property.Equals("Value"))
		{
			return;
		}
		if (text.Equals("EnergyRefillTimer"))
		{
			BMDEBHIHIAJ.resultNumber = nKGLHEGIKKP.GetEnergyRefillTimer();
			return;
		}
		if (text.Equals("DuelAccessibilityTimer"))
		{
			if (BattlePeriodic.GetTime() > 0)
			{
				BMDEBHIHIAJ.resultNumber = BattlePeriodic.GetRepeatTime() - BattlePeriodic.GetTime();
			}
			return;
		}
		if (text.Equals("StarterPackTimer"))
		{
			BMDEBHIHIAJ.resultNumber = GameUtils.GetLeftTime(nKGLHEGIKKP.GetStarterPackTimerEndTime());
			return;
		}
		RosterTimerContainer kCMICMHCEBB = nKGLHEGIKKP.GetTimerContainer();
		RosterTimer fPNMILOHPMB = kCMICMHCEBB.FindTimer(text);
		if (fPNMILOHPMB != null)
		{
			BMDEBHIHIAJ.resultNumber = GameUtils.GetLeftTime(fPNMILOHPMB.GetEndTimeSeconds());
		}
	}

	private void EvaluateAbGroupExistsFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		string gOHIIMFFFJI = KJFKPMCPIBH.GetFirstArgument();
		BMDEBHIHIAJ.resultNumber = (nKGLHEGIKKP.DoesAbGroupExist(gOHIIMFFFJI) ? 1 : 0);
	}

	private void EvaluateEnchantmentFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		string[] array = KJFKPMCPIBH.GetFirstArgument().Split('|');
		if (array.Length < 2)
		{
			return;
		}
		string bAINMLLIKOL = array[0];
		CompareResult lNIDLHOIHIM = new CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(questParameters);
		kKDGLNECFHA.SetValue(bAINMLLIKOL, lNIDLHOIHIM);
		bAINMLLIKOL = lNIDLHOIHIM.resultSTR;
		string bAINMLLIKOL2 = array[1];
		kKDGLNECFHA.SetValue(bAINMLLIKOL2, lNIDLHOIHIM);
		bAINMLLIKOL2 = lNIDLHOIHIM.resultSTR;
		long num = 0L;
		if (array.Length == 3)
		{
			string bAINMLLIKOL3 = array[2];
			kKDGLNECFHA.SetValue(bAINMLLIKOL3, lNIDLHOIHIM);
			num = (long)lNIDLHOIHIM.resultNumber;
		}
		switch (KJFKPMCPIBH.property)
		{
		case "Item":
			BMDEBHIHIAJ.resultSTR = bAINMLLIKOL;
			break;
		case "Recipe":
			BMDEBHIHIAJ.resultSTR = bAINMLLIKOL2;
			break;
		case "Timeout":
		{
			RecipePrice pANAKJICBKI2 = ForgeManager.GetInstance().GetPriceByItemName(bAINMLLIKOL, bAINMLLIKOL2);
			BMDEBHIHIAJ.resultNumber = ((pANAKJICBKI2 != null) ? pANAKJICBKI2.DeliveryTimeSeconds : 0);
			break;
		}
		case "DeliveryTime":
			BMDEBHIHIAJ.resultNumber = num;
			break;
		case "BonusDeliveryPrice":
		{
			RecipePrice pANAKJICBKI = ForgeManager.GetInstance().GetPriceByItemName(bAINMLLIKOL, bAINMLLIKOL2);
			BMDEBHIHIAJ.resultNumber = (int)((pANAKJICBKI != null) ? (ObscuredLong)(pANAKJICBKI.BonusDeliveryPriceValue) : 0);
			break;
		}
		case "Available":
		{
			Recipe iNODIOJPNJH = ForgeManager.GetInstance().GetRecipeByName(bAINMLLIKOL2);
			UserItem dKCHDHMLKHN = ListSF.GetUserItem(bAINMLLIKOL);
			bool flag = false;
			if (iNODIOJPNJH != null && dKCHDHMLKHN != null)
			{
				flag = iNODIOJPNJH.IsAvailableWithMaterials(dKCHDHMLKHN);
			}
			BMDEBHIHIAJ.resultNumber = (flag ? 1 : 0);
			break;
		}
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "QuestCondition.EnchantmentFunction - unknown property ", KJFKPMCPIBH.property));
			break;
		}
	}

	private string GetPurchaseUnsuccessfulValue()
	{
		StringBuilder stringBuilder = new StringBuilder();
		ItemInfo dLKPBAJDHBO = questParameters.purchasedItem;
		if (dLKPBAJDHBO != null)
		{
			stringBuilder.Append(dLKPBAJDHBO.Name);
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

	private void EvaluateFightCurrencyCostFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		string text = ((KJFKPMCPIBH.arguments.Count <= 0) ? string.Empty : KJFKPMCPIBH.arguments[0].result);
		string text2 = ((KJFKPMCPIBH.arguments.Count <= 1) ? string.Empty : KJFKPMCPIBH.arguments[1].result);
		FightList jDIPBIHBGPF = ListSF.GetInstance().GetFightByIdString(text);
		if (jDIPBIHBGPF == null)
		{
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition::fightCurrencyCostFunction - cant fight fight: ", text));
		}
		else if (text2.Equals(string.Empty))
		{
			GameLog.Error("ERROR: QuestCondition::fightCurrencyCostFunction - currencyName is empty");
		}
		else
		{
			BMDEBHIHIAJ.resultNumber = jDIPBIHBGPF.GetCurrencyCost(text2);
		}
	}

	private void EvaluateShopAssertFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		string gOHIIMFFFJI = KJFKPMCPIBH.GetFirstArgument();
		string hBDLDIKHFEG = KJFKPMCPIBH.property;
		if (hBDLDIKHFEG.Equals("IsOpen"))
		{
			Roster nKGLHEGIKKP = ListSF.GetRoster();
			bool flag = nKGLHEGIKKP.HasShopLock(gOHIIMFFFJI);
			BMDEBHIHIAJ.resultNumber = (flag ? 1 : 0);
		}
		else
		{
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition::shopAssertFunction - unknown property: ", hBDLDIKHFEG));
		}
	}

	private void EvaluateSimOperatorFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		string text = KJFKPMCPIBH.GetFirstArgument();
		string hBDLDIKHFEG = KJFKPMCPIBH.property;
		if (hBDLDIKHFEG != null && hBDLDIKHFEG == "Code")
		{
			BMDEBHIHIAJ.resultSTR = string.Empty;
		}
		else
		{
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition::getSimOperator - unknown property: ", KJFKPMCPIBH.property));
		}
	}

	private void EvaluateRaidInfoFunction(QuestFunctions KJFKPMCPIBH, CompareResult BMDEBHIHIAJ)
	{
		switch (KJFKPMCPIBH.property)
		{
		case "TutorialStep":
			BMDEBHIHIAJ.resultSTR = GameUtils.RaidTutorialStepNames[ListSF.GetRoster().GetTutorials().GetRaidStep()];
			break;
		case "RestoreCurrencyValue":
			break;
		default:
			GameLog.Error(string.Format("{0},\"{1}\"", "ERROR: QuestCondition::getSimOperator - unknown property: ", KJFKPMCPIBH.property));
			break;
		}
	}
}
