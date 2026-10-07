using System.Collections.Generic;
using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public partial class ModelParameters
{
	public enum EstimationType
	{
		EstimationTypeNone = 0,
		EstimationTypeShowDifficulty = 1,
		EstimationTypeAttributesAlign = 2
	}

	public enum DifficultyFilter
	{
		DFHard = 0,
		DFNormal = 1,
		DFBoth = 2
	}

	private static HashSet<GroupModel> groupModels = new HashSet<GroupModel>();

	private static List<RatingEvaluation> ratingEvaluations = new List<RatingEvaluation>();

	private static float impossibleRatio;

	private static float easyRatio;

	private static string perkAspectParameter;

	public List<GroupModel> GroupModels = new List<GroupModel>();

	public SceneTypes SceneType;

	// best guess for name
	public List<AttributesAlign> AttributeAlignments = new List<AttributesAlign>();

	// best guess for name
	public List<string> ModelDocuments = new List<string>();

	public List<ItemInfo> DecorateItems = new List<ItemInfo>();

	public List<ItemInfo> ConditionItems = new List<ItemInfo>();

	public List<int> MoveIds = new List<int>();

	public List<string> ExcludedMoveNames = new List<string>();

	public List<string> ExcludedPerkNames = new List<string>();

	public int OpponentCount;

	public float AutoTuneFactor;

	public bool NoDoubles;

	public bool IsPlayer;

	// best guess for name
	public bool AiControlled;

	// best guess for name
	public bool UserControlled;

	public bool UnknownFlag;

	public List<int> ControlKeys = new List<int>();

	public Vector3f SpawnPosition;

	public string GroupName;

	public int OpponentIndex;

	public int RandomValue;

	// best guess for name
	public string Voice;

	public XmlNode Node;

	public bool HasSourceNode;

	// best guess for name
	public ItemInfo Skeleton;
    public string EclipseBodyModel;
    public string EclipseCharacterId;
    public string[] EclipseSkinModels = System.Array.Empty<string>();

	// best guess for name
	public ItemInfo Weapon;

	// best guess for name
	public ItemInfo Armor;

	// best guess for name
	public ItemInfo Helm;

	// best guess for name
	public ItemInfo Ranged;

	// best guess for name
	public ItemInfo Magic;

	public ItemInfo Seal;

	public float SavedLife;

	// best guess for name
	public float MaxLife;

	// PvP grey health, in the same single-bar units as combat damage.
	public float RecoverableLife;

	public EndRoundType EndRoundType;

	public bool IsWinner;

	public bool RoundEnded;

	public bool IsDead;

	public bool RewardsEnabled;

	public bool AnimationEnabled;

	public bool BeginnerCheat;

	public bool IsUntouched;

	public bool MovesInitialized;

	public bool UnusedRoundFlag;

	// best guess for name
	public int RoundsWon;

	public int RoundTotal;

	public int Dan;

	public int WarriorPower;

	// Total one-bar health pools authored by the raid template/warrior.
	public int ShieldTotal;

	public bool HasShieldTotalOverride;

	// Life stays normalized for existing perks, rules and round comparisons.
	// Each incoming combat delta is one bar's worth, not the whole raid pool.
	public int HealthBarCount { get { return System.Math.Max(1, ShieldTotal); } }

	// Combat damage is measured in single-bar units; stored life is a fraction
	// of the whole pool. Only convert at damage comparisons/application, not in
	// the normalized health getters used by perks, AI and the HUD.
	public float RemainingHealthInDamageUnits
	{
		get { return (float)_CurrentLife * HealthBarCount; }
	}

	public float ResolveStrikeDamage(float damage, out bool overkill)
	{
		float remaining = RemainingHealthInDamageUnits;
		overkill = remaining < damage;
		// Retain the original final-hit margin, but in the same units as damage.
		return overkill ? remaining + 0.01f : damage;
	}

	public int RemainingHealthBars
	{
		get { return (int)System.Math.Ceiling(HealthBarsLeft); }
	}

	private double HealthBarsLeft
	{
		get
		{
			double bars = System.Math.Max(0d, System.Math.Min(1d, GetLifeRatio())) * HealthBarCount;
			double rounded = System.Math.Round(bars);
			// Repeated float damage must not leave a phantom, almost-empty bar.
			return rounded > 0d && System.Math.Abs(bars - rounded) < 0.0001d ? rounded : bars;
		}
	}

	public float CurrentHealthBarFraction
	{
		get
		{
			double bars = HealthBarsLeft;
			return bars <= 0d ? 0f : (float)(bars - System.Math.Ceiling(bars) + 1d);
		}
	}

	public int RatingCorrection;

	public uint LotteryLevel;

	public float Damage;

	public float Difficulty;

	public string FirstName;

	public string LastName;

	public string Avatar;

	// best guess for name
	public string DisplayName;

	// best guess for name
	public List<PerkInfoItem> Perks = new List<PerkInfoItem>();

	// best guess for name
	public List<PerkInfoItem> WarriorPerks = new List<PerkInfoItem>();

	// best guess for name
	public List<PerkInfoItem> LearnedPerks = new List<PerkInfoItem>();

	public Attributes BaseAttributes;

	public Attributes FinalAttributes;

	private float playerRating;

	private float enemyRating;

	private float playerRatingMagic;

	private float enemyRatingMagic;

	private float playerRatingRanged;

	private float enemyRatingRanged;

	public Tactic FightTactic;

	private ObscuredInt level;

	private ObscuredFloat _CurrentLife;

	private ObscuredInt baseHealth;

	private ObscuredInt obscuredValueA;

	private ObscuredInt obscuredValueB;

	private bool isImmortalityEnabled;

	public ObscuredInt Level
	{
		get
		{
			return GetLevel();
		}
		set
		{
			SetLevel(value);
		}
	}

	public ObscuredFloat CurrentLife
	{
		get
		{
			return GetCurrentLife();
		}
	}

	public ObscuredInt BaseHealth
	{
		get
		{
			return GetBaseHealth();
		}
	}

	public List<PerkInfoItem> AllPerks
	{
		get
		{
			return GetAllPerks();
		}
	}

	public bool IsGroup
	{
		get
		{
			return GetIsGroup();
		}
	}

	public bool ImmortalityEnabled
	{
		get
		{
			return GetIsImmortalityEnabled();
		}
		set
		{
			set_IsImmortalityEnabled(value);
		}
	}

	public float LifeRatio
	{
		get
		{
			return GetLifeRatio();
		}
	}

	public bool LifeDepleted
	{
		get
		{
			return GetLifeDepleted();
		}
	}

	private bool HasWonAllRounds
	{
		get
		{
			return GetHasWonAllRounds();
		}
	}

	private float ImpossibleRatio
	{
		get
		{
			return GetImpossibleRatio();
		}
	}

	private float EasyRatio
	{
		get
		{
			return GetEasyRatio();
		}
	}

	public float PlayerRating
	{
		get
		{
			return GetPlayerRating();
		}
		set
		{
			SetPlayerRating(value);
		}
	}

	public float EnemyRating
	{
		get
		{
			return GetEnemyRating();
		}
		set
		{
			SetEnemyRating(value);
		}
	}

	public float PlayerRatingRanged
	{
		get
		{
			return GetPlayerRatingRanged();
		}
		set
		{
			SetPlayerRatingRanged(value);
		}
	}

	public float EnemyRatingRanged
	{
		get
		{
			return GetEnemyRatingRanged();
		}
		set
		{
			SetEnemyRatingRanged(value);
		}
	}

	public float PlayerRatingMagic
	{
		get
		{
			return GetPlayerRatingMagic();
		}
		set
		{
			SetPlayerRatingMagic(value);
		}
	}

	public float EnemyRatingMagic
	{
		get
		{
			return GetEnemyRatingMagic();
		}
		set
		{
			SetEnemyRatingMagic(value);
		}
	}

	public ModelParameters()
	{
		Perks = new List<PerkInfoItem>();
		FinalAttributes = new Attributes();
		BaseAttributes = new Attributes();
		Avatar = string.Empty;
		Skeleton = null;
		Weapon = null;
		Armor = null;
		Helm = null;
		Ranged = null;
		Magic = null;
		IsPlayer = false;
		AiControlled = true;
		UserControlled = false;
		IsWinner = false;
		RoundEnded = false;
		IsDead = false;
		RewardsEnabled = true;
		AnimationEnabled = true;
		EndRoundType = EndRoundType.EndRoundTypeNone;
		OpponentCount = 1;
		SavedLife = 0f;
		MaxLife = 0f;
		RoundsWon = 0;
		RoundTotal = 0;
		Dan = 0;
		WarriorPower = 0;
		RatingCorrection = 0;
		LotteryLevel = 0u;
		Damage = 0f;
		Difficulty = 0f;
		OpponentIndex = 0;
		BeginnerCheat = false;
		UnknownFlag = false;
		Voice = string.Empty;
		NoDoubles = false;
		FightTactic = null;
		HasSourceNode = false;
		UnusedRoundFlag = false;
		AutoTuneFactor = 0f;
		SpawnPosition = Vector3f.op_Implicit(new Vector3(-100f, -100f, -100f));
		RandomValue = 0;
		Seal = null;
		IsUntouched = false;
		MovesInitialized = false;
		SceneType = SceneTypes.SceneNone;
		playerRating = -1f;
		enemyRating = -1f;
		playerRatingMagic = -1f;
		enemyRatingMagic = -1f;
		playerRatingRanged = -1f;
		enemyRatingRanged = -1f;
		_CurrentLife = (ObscuredFloat)(0f);
		baseHealth = (ObscuredInt)(-1);
		obscuredValueA = (ObscuredInt)(-1);
		obscuredValueB = (ObscuredInt)(-1);
	}

	public ModelParameters(ModelParameters source)
	{
		Perks = new List<PerkInfoItem>(source.Perks);
		WarriorPerks = new List<PerkInfoItem>(source.WarriorPerks);
		LearnedPerks = new List<PerkInfoItem>(source.LearnedPerks);
		FinalAttributes = new Attributes(source.FinalAttributes);
		BaseAttributes = new Attributes(source.BaseAttributes);
		SetLevel(source.GetLevel());
		FirstName = source.FirstName;
		Avatar = source.Avatar;
		Skeleton = source.Skeleton;
        EclipseBodyModel = source.EclipseBodyModel;
        EclipseCharacterId = source.EclipseCharacterId;
        EclipseSkinModels = (string[])source.EclipseSkinModels.Clone();
        EclipseRosterPlayer = source.EclipseRosterPlayer;
        EclipseVersusLook = source.EclipseVersusLook;
        _eclipseHiddenFigures.UnionWith(source._eclipseHiddenFigures);
        _eclipseAppearanceSkins = source._eclipseAppearanceSkins;
        _eclipseAppearanceVoice = source._eclipseAppearanceVoice;
		Weapon = source.Weapon;
		Armor = source.Armor;
		Helm = source.Helm;
		Ranged = source.Ranged;
		Magic = source.Magic;
		ModelDocuments.AddRange(source.ModelDocuments);
		ExcludedMoveNames.AddRange(source.ExcludedMoveNames);
		ExcludedPerkNames.AddRange(source.ExcludedPerkNames);
		IsPlayer = source.IsPlayer;
		AiControlled = source.AiControlled;
		UserControlled = source.UserControlled;
		IsWinner = source.IsWinner;
		RoundEnded = source.RoundEnded;
		IsDead = source.IsDead;
		RewardsEnabled = source.RewardsEnabled;
		AnimationEnabled = source.AnimationEnabled;
		EndRoundType = source.EndRoundType;
		OpponentCount = source.OpponentCount;
		SavedLife = source.SavedLife;
		MaxLife = source.MaxLife;
		RoundsWon = source.RoundsWon;
		RoundTotal = source.RoundTotal;
		Dan = source.Dan;
		WarriorPower = source.WarriorPower;
		ShieldTotal = source.ShieldTotal;
		HasShieldTotalOverride = source.HasShieldTotalOverride;
		RatingCorrection = source.RatingCorrection;
		LotteryLevel = source.LotteryLevel;
		Damage = source.Damage;
		Difficulty = source.Difficulty;
		OpponentIndex = source.OpponentIndex;
		BeginnerCheat = source.BeginnerCheat;
		UnknownFlag = source.BeginnerCheat;
		Voice = source.Voice;
		NoDoubles = source.NoDoubles;
		FightTactic = source.FightTactic;
		HasSourceNode = source.HasSourceNode;
		Node = source.Node;
		UnusedRoundFlag = source.UnusedRoundFlag;
		AutoTuneFactor = source.AutoTuneFactor;
		SpawnPosition = new Vector3f(source.SpawnPosition);
		RandomValue = source.RandomValue;
		Seal = source.Seal;
		IsUntouched = source.IsUntouched;
		MovesInitialized = source.MovesInitialized;
		SceneType = source.SceneType;
		playerRating = source.playerRating;
		enemyRating = source.enemyRating;
		playerRatingMagic = source.playerRatingMagic;
		enemyRatingMagic = source.enemyRatingMagic;
		playerRatingRanged = source.playerRatingRanged;
		enemyRatingRanged = source.enemyRatingRanged;
		_CurrentLife = source._CurrentLife;
		RecoverableLife = source.RecoverableLife;
		baseHealth = source.baseHealth;
		obscuredValueA = source.obscuredValueA;
		obscuredValueB = source.obscuredValueB;
		AttributeAlignments = new List<AttributesAlign>(source.AttributeAlignments);
	}

	public ObscuredInt GetLevel()
	{
		return level;
	}

	public void SetLevel(ObscuredInt value)
	{
		level = value;
	}

	public ObscuredFloat GetCurrentLife()
	{
		return _CurrentLife;
	}

	public ObscuredInt GetBaseHealth()
	{
		return baseHealth;
	}

	public void RandomizeObscuredVars()
	{
		level.RandomizeCryptoKey();
		_CurrentLife.RandomizeCryptoKey();
		baseHealth.RandomizeCryptoKey();
		obscuredValueA.RandomizeCryptoKey();
		obscuredValueB.RandomizeCryptoKey();
	}

	public void AddConditionItem(ItemInfo item)
	{
		ConditionItems.AddIfNotExist(item);
	}

	public void AddWarriorPerk(PerkInfoItem perk)
	{
		WarriorPerks.AddIfNotExist(perk);
	}

	public List<ItemInfo> GetEquippedItemsByType()
	{
		List<ItemInfo> list = new List<ItemInfo>();
		ItemInfo equippedItem = GetItemByType("Skeleton");
		if (equippedItem != null)
		{
			list.Add(equippedItem);
		}
		equippedItem = GetItemByType("Weapon");
		if (equippedItem != null)
		{
			list.Add(equippedItem);
		}
		equippedItem = GetItemByType("Ranged");
		if (equippedItem != null)
		{
			list.Add(equippedItem);
		}
		equippedItem = GetItemByType("Magic");
		if (equippedItem != null)
		{
			list.Add(equippedItem);
		}
		equippedItem = GetItemByType("RaidConsumable");
		if (equippedItem != null)
		{
			list.Add(equippedItem);
		}
		equippedItem = GetItemByType("Armor");
		if (equippedItem != null)
		{
			list.Add(equippedItem);
		}
		equippedItem = GetItemByType("Helm");
		if (equippedItem != null)
		{
			list.Add(equippedItem);
		}
		equippedItem = GetItemByType("Cheat");
		if (equippedItem != null)
		{
			list.Add(equippedItem);
		}
		return list;
	}

	public List<ItemInfo> GetEquippedItems()
	{
		List<ItemInfo> list = new List<ItemInfo>();
		if (Skeleton != null)
		{
			list.Add(Skeleton);
		}
		if (Weapon != null)
		{
			list.Add(Weapon);
		}
		if (Ranged != null)
		{
			list.Add(Ranged);
		}
		if (Magic != null)
		{
			list.Add(Magic);
		}
		if (Armor != null)
		{
			list.Add(Armor);
		}
		if (Helm != null)
		{
			list.Add(Helm);
		}
		return list;
	}

	public ItemInfo GetItemByType(string itemType)
	{
		switch (itemType)
		{
		case "Skeleton":
			return Skeleton;
		case "Weapon":
			return Weapon;
		case "Ranged":
			return Ranged;
		case "Magic":
			return Magic;
		case "Armor":
			return Armor;
		case "Helm":
			return Helm;
		default:
			return null;
		}
	}

	public void SetItemByType(string itemType, ItemInfo item)
	{
		switch (itemType)
		{
		case "Skeleton":
			Skeleton = item;
			break;
		case "Weapon":
			Weapon = item;
			break;
		case "Ranged":
			Ranged = item;
			break;
		case "Magic":
			Magic = item;
			break;
		case "Armor":
			Armor = item;
			break;
		case "Helm":
			Helm = item;
			break;
		}
	}

	private string GetItemNameByAttribute(string attributeName)
	{
		switch (attributeName)
		{
		case "HeadDefense":
			return Helm.Name;
		case "BodyDefense":
			return Armor.Name;
		case "UnarmedDamage":
			return Armor.Name;
		case "WeaponDamage":
			return Weapon.Name;
		case "RangedDamage":
			return Ranged.Name;
		case "MagicDamage":
			return Magic.Name;
		default:
			return null;
		}
	}

	private ItemInfo GetItemByAttribute(string attributeName)
	{
		switch (attributeName)
		{
		case "HeadDefense":
			return Helm;
		case "BodyDefense":
			return Armor;
		case "UnarmedDamage":
			return Armor;
		case "WeaponDamage":
			return Weapon;
		case "RangedDamage":
			return Ranged;
		case "MagicDamage":
			return Magic;
		default:
			return null;
		}
	}

	public void BuildModelDocuments()
	{
		ModelDocuments.Clear();
        // A chosen player look replaces armor/helmet geometry only; their stats stay.
        bool eclipseLook = ResolveEclipseAppearance();
        if (!string.IsNullOrEmpty(EclipseBodyModel))
            ModelDocuments.Add(EclipseBodyModel.EndsWith(".xml",System.StringComparison.OrdinalIgnoreCase) ? EclipseBodyModel : ToXmlFileName(EclipseBodyModel));
		else if (Skeleton != null && !string.IsNullOrEmpty(Skeleton.ModelFileName))
		{
			ModelDocuments.Add(ToXmlFileName(Skeleton.ModelFileName));
		}
		if (Weapon != null && !string.IsNullOrEmpty(Weapon.ModelFileName))
		{
			ModelDocuments.Add(ToXmlFileName(Weapon.ModelFileName));
		}
		// Under a look, armor and helmet still load (same nodes, edges and physics) but
		// their figures are not drawn unless the player shows armor.
		bool eclipseHideArmor = eclipseLook && !Eclipse.Modding.PlayerAppearance.ShowArmor;
		if (Armor != null && !string.IsNullOrEmpty(Armor.ModelFileName))
		{
			string path = ToXmlFileName(Armor.ModelFileName);
			ModelDocuments.Add(eclipseHideArmor ? HideEclipseFigures(path) : path);
		}
		if (Helm != null && !string.IsNullOrEmpty(Helm.ModelFileName))
		{
			string path = ToXmlFileName(Helm.ModelFileName);
			ModelDocuments.Add(eclipseHideArmor ? HideEclipseFigures(path) : path);
		}
		for (int i = 0; i < DecorateItems.Count; i++)
		{
			ModelDocuments.Add(ToXmlFileName(DecorateItems[i].ModelFileName));
		}
        foreach (var skin in EclipseSkinModels)
            ModelDocuments.Add(skin.EndsWith(".xml",System.StringComparison.OrdinalIgnoreCase) ? skin : ToXmlFileName(skin));
        foreach (var skin in _eclipseAppearanceSkins)
            ModelDocuments.Add(EclipseModelPath(skin));
	}

	public int SumItemAttribute(string name, ref bool hasValue)
	{
		List<ItemInfo> equippedItems = GetEquippedItems();
		return SumItemAttribute(name, equippedItems, ref hasValue);
	}

	public int SumItemAttribute(string name, List<ItemInfo> items, ref bool hasValue)
	{
		hasValue = false;
		int num = 0;
		for (int i = 0; i < items.Count; i++)
		{
			int attributeValue = 0;
			if (items[i].ItemAttributes.Get(name, ref attributeValue))
			{
				hasValue = true;
				num += attributeValue;
			}
		}
		return num;
	}

	private int SumPerkAttribute(string name, ref bool hasValue)
	{
		int num = 0;
		for (int i = 0; i < Perks.Count; i++)
		{
			int attributeValue = 0;
			if (Perks[i].AttributeValues.Get(name, ref attributeValue))
			{
				hasValue = true;
				num += attributeValue;
			}
		}
		return num;
	}

	public void CalculateAttributes()
	{
		ClearAttributes();
		List<ItemInfo> equippedItems = GetEquippedItems();
		List<WarriorAttribute> warriorAttributes = GameUtils.WarriorAttributeList.AttributeList;
		for (int i = 0; i < warriorAttributes.Count; i++)
		{
			string text = warriorAttributes[i].get_Name();
			int attributeValue = 0;
			if (BaseAttributes.Get(text, ref attributeValue))
			{
				FinalAttributes.Set(text, attributeValue, true);
				continue;
			}
			int num = 0;
			bool hasValue = false;
			num += SumItemAttribute(text, equippedItems, ref hasValue);
			num += SumPerkAttribute(text, ref hasValue);
			int OEMALIFPGPO2 = 0;
			if (GameUtils.StartingAttributes.Gains.Get(text, ref OEMALIFPGPO2))
			{
				num += OEMALIFPGPO2;
				hasValue = true;
			}
			int OEMALIFPGPO3 = 0;
			if (GameUtils.LevelAttributeGains.Gains.Get(text, ref OEMALIFPGPO3))
			{
				num += (ObscuredInt)(level) * OEMALIFPGPO3;
				hasValue = true;
			}
			if (hasValue || !warriorAttributes[i].UnusedFlag)
			{
				FinalAttributes.Set(text, num, true);
			}
		}
	}

	public List<PerkInfoItem> GetAllPerks()
	{
		List<PerkInfoItem> list = new List<PerkInfoItem>();
		list.AddRange(Perks);
		list.AddRange(WarriorPerks);
		list.AddRange(LearnedPerks);
		List<ItemInfo> list2 = GetEquippedItems();
		foreach (ItemInfo item in list2)
		{
			bool isWeaponPerk = item.Type == "Weapon";
			foreach (PerkInfoItem item2 in item.InnatePerks)
			{
				item2.SetIsWeaponPerk(isWeaponPerk);
				list.Add(item2);
			}
			if (item.IgnoreInventoryEnchantments || !IsPlayer)
			{
				continue;
			}
			UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(item);
			if (userItem == null)
			{
				continue;
			}
			List<PerkInfoItem> list3 = ListSF.GetItemEnchantmentsForSide(item, IsPlayer);
			foreach (PerkInfoItem item3 in list3)
			{
				if (item3.Kind == PerkInfoItem.PerkKind.COMBO)
				{
					if (item.Type == "Weapon" && GameUtils.IsPerkCompatibleWithEquipment(item3))
					{
						item3.SetIsWeaponPerk(isWeaponPerk);
						list.Add(item3);
					}
				}
				else
				{
					item3.SetIsWeaponPerk(isWeaponPerk);
					list.Add(item3);
				}
			}
		}
		RemovePerksByNames(list, ExcludedPerkNames);
		return list;
	}

	public void RefreshPerks()
	{
		Perks.Clear();
		Perks.AddRange(GetAllPerks());
		RemovePerksByNames(Perks, ExcludedPerkNames);
	}

	private string ToXmlFileName(string name)
	{
		return string.Format("{0}.xml", name);
	}

	public override string ToString()
	{
		return string.Format("User ID='{0}' SilhouetteItemID='{1}' WeaponID='{2}' Dan='{3}' Damage='{4}' Difficulty='{5}' FirstName='{6}' LastName='{7}'  Level='{8}' LotteryLevel='{9}' ", 0, Armor.ItemId, (Weapon != null) ? Weapon.ItemId : 0, Dan, Damage, Difficulty, FirstName, LastName, level, LotteryLevel);
	}

	public static void ParseRatingConfig(XmlNode configNode)
	{
		impossibleRatio = XmlUtils.ParseFloat(configNode.Attributes["ImpossibleRatio"]);
		easyRatio = XmlUtils.ParseFloat(configNode.Attributes["EasyRatio"]);
		ratingEvaluations.Clear();
		perkAspectParameter = XmlUtils.ParseString(configNode.Attributes["PerkAspectParameter"]);
		foreach (XmlNode childNode in configNode.ChildNodes)
		{
			RatingEvaluation evaluation = new RatingEvaluation();
			ratingEvaluations.Add(evaluation);
			evaluation.nodeName = childNode.Name;
			evaluation.evaluationName = XmlUtils.ParseString(childNode.Attributes["Name"]);
			Evaluation.ParseAttributes(childNode, evaluation.evaluations);
			Evaluation.ParseAverageQuantity(childNode, evaluation);
			Evaluation.ParseAverageDamageAndRecharge(childNode, evaluation);
			evaluation.cancellingItem = XmlUtils.ParseString(childNode.Attributes["CancellingItem"]);
			Defense.Parse(childNode, evaluation.defenses);
		}
	}

	private void EvaluateAttributeAlignment(float ratio)
	{
		float num = (0f - GameUtils.DamageDoublingRange) * (Mathf.Log10(ratio) / Mathf.Log10(2f)) / 2f;
		float num2 = float.MinValue;
		foreach (AttributesAlign item in AttributeAlignments)
		{
			float num3 = num * item.Factor + item.Shift;
			if (num2 < num3)
			{
				num2 = num3;
			}
		}
	}

	private float GetParametersValue(string name)
	{
		int attributeValue = 0;
		if (FinalAttributes.Get(name, ref attributeValue))
		{
			return attributeValue;
		}
		Debug.LogErrorFormat("Parameter \"{0}\" not found!", name);
		return float.MinValue;
	}

	private float GetParametersValue(List<string> attributeNames)
	{
		float num = float.MinValue;
		foreach (string item in attributeNames)
		{
			int attributeValue = 0;
			if (FinalAttributes.Get(item, ref attributeValue) && num < (float)attributeValue)
			{
				num = attributeValue;
			}
		}
		if (num == float.MinValue)
		{
			Debug.LogError("Parameters not found!");
		}
		return num;
	}

	private float GetMaxEvaluationValue(List<Evaluation> evaluations, ModelParameters parameters)
	{
		float num = float.MinValue;
		foreach (Evaluation item in evaluations)
		{
			float num2 = parameters.GetParametersValue(item.Name) + item.Shift;
			if (num < num2)
			{
				num = num2;
			}
		}
		return num;
	}

	public float CalculateDamageRating(ModelParameters opponentParameters, List<global::Pair<string, float>> shifts)
	{
		float num = 0f;
		List<global::Pair<string, float>> list = new List<global::Pair<string, float>>();
		foreach (RatingEvaluation item in ratingEvaluations)
		{
			if (!(item.nodeName == "Damage"))
			{
				continue;
			}
			RatingEvaluation ratingEvaluation = item;
			if (HasItem(ratingEvaluation.cancellingItem))
			{
				continue;
			}
			float averageBaseDamage = ratingEvaluation.averageBaseDamage;
			list.Clear();
			CollectEvaluationAttributes(list, ratingEvaluation.evaluations);
			AddShiftsToPairs(list, shifts);
			List<Defense> defenses = ratingEvaluation.defenses;
			ApplyDefenseShiftsToOpponent(opponentParameters, defenses, shifts);
			float num2 = 0f;
			foreach (Defense item2 in defenses)
			{
				if (item2.Evaluations.Count != 1)
				{
					Debug.LogError("Count of DefenseAttribute != 1");
				}
				if (HasItem(item2.CancellingItem))
				{
					continue;
				}
				float num3 = GameUtils.GetAttributesHitMultiplier(IsPlayer, this, opponentParameters, list, item2.Evaluations[0].Name);
				num3 = Mathf.Min(1f, averageBaseDamage * num3);
				List<PerkInfoItem> list2 = GetAllPerks();
				foreach (PerkInfoItem item3 in list2)
				{
					List<Rating> ratings = item3.Ratings;
					if (ratings.Count == 0)
					{
						continue;
					}
					foreach (Rating item4 in ratings)
					{
						if (!(item4.player != "Me") && (string.IsNullOrEmpty(item4.damageType) || item4.damageType == ratingEvaluation.evaluationName) && (string.IsNullOrEmpty(item4.defenseType) || item4.defenseType == item2.DefenseName))
						{
							float num4 = 0f;
							if (!string.IsNullOrEmpty(perkAspectParameter))
							{
								string s = item3.GetSetValue(perkAspectParameter);
								num4 = float.Parse(s);
							}
							num3 *= 1f + (item4.Multiplier - 1f) * PerkInfoItem.AspectToMultiplier(num4 - opponentParameters.GetParametersValue(item4.enemyAttribute));
						}
					}
				}
				List<PerkInfoItem> list3 = opponentParameters.GetAllPerks();
				foreach (PerkInfoItem item5 in list3)
				{
					List<Rating> mLMLENHGNDJ2 = item5.Ratings;
					if (mLMLENHGNDJ2.Count == 0)
					{
						continue;
					}
					foreach (Rating item6 in mLMLENHGNDJ2)
					{
						if (!(item6.player != "Enemy") && (string.IsNullOrEmpty(item6.damageType) || item6.damageType == ratingEvaluation.evaluationName) && (string.IsNullOrEmpty(item6.defenseType) || item6.defenseType == item2.DefenseName))
						{
							float num5 = 0f;
							if (!string.IsNullOrEmpty(perkAspectParameter))
							{
								string s2 = item5.GetSetValue(perkAspectParameter);
								num5 = float.Parse(s2);
							}
							num3 /= 1f + (item6.Multiplier - 1f) * PerkInfoItem.AspectToMultiplier(num5 - opponentParameters.GetParametersValue(item6.enemyAttribute));
						}
					}
				}
				num2 += item2.Weight * num3;
			}
			if (ratingEvaluation.magicRechargeRate > 0f)
			{
				float magicRechargeRate = ratingEvaluation.magicRechargeRate;
				float num6 = GameUtils.MagicConfig.GetPainRecharge();
				float num7 = GameUtils.MagicConfig.GetDamageRecharge();
				float num8 = GameUtils.MagicConfig.GetPainRecharge(this);
				float num9 = GameUtils.MagicConfig.GetDamageRecharge(this);
				num2 *= magicRechargeRate * (num6 * num8 + num7 * num9);
			}
			num += num2;
		}
		return num;
	}

	private bool HasItem(string itemName)
	{
		List<ItemInfo> list = GetEquippedItems();
		foreach (ItemInfo item in list)
		{
			if (item.Name == itemName)
			{
				return true;
			}
		}
		return false;
	}

	private void AddShiftsToPairs(List<global::Pair<string, float>> targetShifts, List<global::Pair<string, float>> sourceShifts)
	{
		if (sourceShifts == null)
		{
			return;
		}
		foreach (global::Pair<string, float> item in sourceShifts)
		{
			string shiftName = item.First;
			float shiftValue = item.Second;
			for (int i = 0; i < targetShifts.Count; i++)
			{
				if (targetShifts[i].First == shiftName)
				{
					targetShifts[i] = new global::Pair<string, float>(targetShifts[i].First, targetShifts[i].Second + shiftValue);
					break;
				}
			}
		}
	}

	private void ApplyDefenseShiftsToOpponent(ModelParameters opponentParameters, List<Defense> defenses, List<global::Pair<string, float>> shifts)
	{
		if (shifts == null)
		{
			return;
		}
		foreach (global::Pair<string, float> item in shifts)
		{
			string attributeName = item.First;
			float shiftValue = item.Second;
			for (int i = 0; i < defenses.Count; i++)
			{
				foreach (Evaluation item2 in defenses[i].Evaluations)
				{
					if (item2.Name == attributeName)
					{
						int attributeValue = 0;
						opponentParameters.FinalAttributes.Get(attributeName, ref attributeValue);
						opponentParameters.FinalAttributes.Set(attributeName, (int)((float)attributeValue + shiftValue));
					}
				}
			}
		}
	}

	public void RemovePerksByNames(List<PerkInfoItem> perks, List<string> perkNames)
	{
		foreach (string item in perkNames)
		{
			for (int num = perks.Count - 1; num >= 0; num--)
			{
				if (IsPerkInNames(perks[num], item))
				{
					perks.RemoveAt(num);
				}
			}
		}
	}

	private bool IsPerkInNames(PerkInfoItem perk, string name)
	{
		if (perk == null)
		{
			return false;
		}
		return perk.IsPerkByNames(name);
	}

	public bool UpdateLife(float lifeDelta)
	{
		ChangeLife(lifeDelta);
		if (GetLifeDepleted())
		{
			IsDead = true;
		}
		return IsDead;
	}

	public bool GetIsGroup()
	{
		return GroupModels.Count > 0;
	}

	public bool GetIsImmortalityEnabled()
	{
		return isImmortalityEnabled;
	}

	public void set_IsImmortalityEnabled(bool value)
	{
		isImmortalityEnabled = value;
	}

	// best guess for name
	public void SetCurrentLife(float value)
	{
		if (GetIsImmortalityEnabled())
		{
			_CurrentLife = (ObscuredFloat)(MaxLife);
		}
		else if (value < 0f)
		{
			_CurrentLife = (ObscuredFloat)(0f);
		}
		else if (value > MaxLife)
		{
			_CurrentLife = (ObscuredFloat)(MaxLife);
		}
		else
		{
			_CurrentLife = (ObscuredFloat)(value);
		}
		RecoverableLife = (float)_CurrentLife <= 0f ? 0f : Mathf.Clamp(RecoverableLife, 0f, Mathf.Max(0f, MaxLife - (float)_CurrentLife));
	}

	public void ChangeLife(float value)
	{
		float next = (ObscuredFloat)(_CurrentLife) + value / HealthBarCount;
		// Float subtraction across many shields can leave a microscopic last
		// bar and prevent death even after the exact pool has been depleted.
		if (HealthBarCount > 1 && value < 0f && next * HealthBarCount < 0.0001f)
			next = 0f;
		SetCurrentLife(next);
	}

	private int CollectEvaluationAttributes(string name, List<string> attributeNames)
	{
		int count = attributeNames.Count;
		foreach (RatingEvaluation item in ratingEvaluations)
		{
			if (!(item.nodeName == name))
			{
				continue;
			}
			foreach (Evaluation item2 in item.evaluations)
			{
				attributeNames.AddIfNotExist(item2.Name);
			}
		}
		return attributeNames.Count - count;
	}

	private int CollectDefenseAttributes(string name, List<string> attributeNames)
	{
		int count = attributeNames.Count;
		foreach (RatingEvaluation item in ratingEvaluations)
		{
			if (!(item.nodeName == name))
			{
				continue;
			}
			foreach (Defense item2 in item.defenses)
			{
				foreach (Evaluation item3 in item2.Evaluations)
				{
					attributeNames.AddIfNotExist(item3.Name);
				}
			}
		}
		return attributeNames.Count - count;
	}

	private int CollectEvaluationAttributes(List<global::Pair<string, float>> shifts, List<Evaluation> evaluations)
	{
		int count = shifts.Count;
		foreach (Evaluation item in evaluations)
		{
			shifts.Add(new global::Pair<string, float>(item.Name, item.Shift));
		}
		return shifts.Count - count;
	}

	private int CollectDamageAttributes(List<string> attributeNames)
	{
		return CollectEvaluationAttributes("Damage", attributeNames);
	}

	private int CollectMagicAttributes(List<string> attributeNames)
	{
		return CollectEvaluationAttributes("Magic", attributeNames);
	}

	private int CollectRangedAttributes(List<string> attributeNames)
	{
		return CollectEvaluationAttributes("Ranged", attributeNames);
	}

	private int CollectDamageDefenseAttributes(List<string> attributeNames)
	{
		return CollectDefenseAttributes("Damage", attributeNames);
	}

	private int CollectMagicDefenseAttributes(List<string> attributeNames)
	{
		return CollectDefenseAttributes("Magic", attributeNames);
	}

	private int CollectRangedDefenseAttributes(List<string> attributeNames)
	{
		return CollectDefenseAttributes("Ranged", attributeNames);
	}

	public void SaveCurrentLife()
	{
		SavedLife = (ObscuredFloat)(_CurrentLife);
	}

	public float GetLifeRatio()
	{
		return MaxLife > 0f ? (ObscuredFloat)(_CurrentLife) / MaxLife : 0f;
	}

	public bool GetLifeDepleted()
	{
		return (ObscuredFloat)(_CurrentLife) <= 0f;
	}

	public void AddLife(float lifeAmount = 1f)
	{
		float num = (ObscuredFloat)(_CurrentLife);
		num += lifeAmount;
		num = Mathf.Min(num, MaxLife);
		num = Mathf.Max(0f, num);
		SetCurrentLife(num);
	}

	public void RestoreFullLife()
	{
		SetCurrentLife(MaxLife);
	}

	private bool GetHasWonAllRounds()
	{
		return RoundsWon >= RoundTotal;
	}

	public ModelParameters Clone()
	{
		return new ModelParameters(this);
	}

	private void ClearAttributes()
	{
		FinalAttributes.Clear();
	}

	private void SetAttributes(ModelParameters source)
	{
		FinalAttributes = source.FinalAttributes;
	}

	public void CopyEquippedItemsTo(EquippedItemsStruct equippedItems)
	{
		equippedItems.Armor = Armor;
		equippedItems.Helm = Helm;
		equippedItems.Seal = Seal;
		equippedItems.Skeleton = Skeleton;
		equippedItems.Weapon = Weapon;
		equippedItems.Magic = Magic;
		equippedItems.Ranged = Ranged;
	}

	public void SetEquippedItemsFrom(EquippedItemsStruct equippedItems)
	{
		Armor = equippedItems.Armor;
		Helm = equippedItems.Helm;
		Seal = equippedItems.Seal;
		Skeleton = equippedItems.Skeleton;
		Weapon = equippedItems.Weapon;
		Magic = equippedItems.Magic;
		Ranged = equippedItems.Ranged;
	}

	private float GetImpossibleRatio()
	{
		return impossibleRatio;
	}

	private float GetEasyRatio()
	{
		return easyRatio;
	}

	private void ClearRatingEvaluations()
	{
		ratingEvaluations.Clear();
	}

	private void LogAttributes()
	{
		float num = GetParametersValue("WeaponDamage");
		float num2 = GetParametersValue("UnarmedDamage");
		float num3 = GetParametersValue("BodyDefense");
		float num4 = GetParametersValue("HeadDefense");
		float num5 = GetParametersValue("RangedDamage");
		float num6 = GetParametersValue("MagicDamage");
		float num7 = GetParametersValue("RangedQuantity");
		GameLog.Write("- WeaponDamage:   {0}", num);
		GameLog.Write("- UnarmedDamage:  {0}", num2);
		GameLog.Write("- BodyDefense:    {0}", num3);
		GameLog.Write("- HeadDefense:    {0}", num4);
		GameLog.Write("- RangedDamage:   {0}", num5);
		GameLog.Write("- MagicDamage:    {0}", num6);
		GameLog.Write("- RangedQuantity: {0}", num7);
	}

	public void SetItemsFromRules(List<ItemRule> rules, bool respectNoAttributeChange, int round = 0)
	{
		Roster roster = ListSF.GetRoster();
		foreach (ItemRule item in rules)
		{
			if (round > 0 && !item.AppliesToRound(round))
			{
				continue;
			}
			UserItem userItem = item.get_Item();
			if (userItem == null)
			{
				GameLog.Error(" ModelParameters::setItemsFromRules - UserItem not found ");
				continue;
			}
			ItemInfo itemInfo = userItem.GetInfo();
			string text = userItem.get_Name();
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			UserItem dKCHDHMLKHN2 = roster.GetInventory().FindItem(text);
			itemInfo = null;
			if (dKCHDHMLKHN2 == null)
			{
				itemInfo = ListSF.GetItems().GetItemByName(text);
				if (itemInfo == null)
				{
					GameLog.Error(" Model::equipRulesItems - item not found \"{0}\"", text);
					continue;
				}
			}
			else
			{
				itemInfo = dKCHDHMLKHN2.GetInfo();
			}
			if ((itemInfo == null || !roster.GetInventory().HasItem(itemInfo)) && !item.GetIsEquipRule())
			{
				itemInfo = null;
			}
			if (itemInfo != null && (!respectNoAttributeChange || !item.GetNoAttributeChange()))
			{
				ItemInfo dJKEECEOCJB2 = GetItemByType(itemInfo.Type);
				ItemInfo dJKEECEOCJB3 = itemInfo.Clone();
				dJKEECEOCJB3.IgnoreInventoryEnchantments = true;
				SetItemByType(itemInfo.Type, dJKEECEOCJB3);
			}
		}
	}

	public float GetPlayerRating()
	{
		return playerRating;
	}

	public void SetPlayerRating(float value)
	{
		playerRating = value;
	}

	public float GetEnemyRating()
	{
		return enemyRating;
	}

	public void SetEnemyRating(float value)
	{
		enemyRating = value;
	}

	public float GetPlayerRatingRanged()
	{
		return playerRatingRanged;
	}

	public void SetPlayerRatingRanged(float value)
	{
		playerRatingRanged = value;
	}

	public float GetEnemyRatingRanged()
	{
		return enemyRatingRanged;
	}

	public void SetEnemyRatingRanged(float value)
	{
		enemyRatingRanged = value;
	}

	public void SetPlayerRatingMagic(float value)
	{
		playerRatingMagic = value;
	}

	public float GetPlayerRatingMagic()
	{
		return playerRatingMagic;
	}

	public void SetEnemyRatingMagic(float value)
	{
		enemyRatingMagic = value;
	}

	public float GetEnemyRatingMagic()
	{
		return enemyRatingMagic;
	}

	public void DisableActivePerks()
	{
		foreach (PerkInfoItem item in Perks)
		{
			if (item.GetIsWeaponPerk())
			{
				item.SetIsEnabled(false);
			}
		}
	}

	public void EnableAllPerks()
	{
		foreach (PerkInfoItem item in Perks)
		{
			if (item != null)
			{
				item.SetIsEnabled(true);
			}
		}
	}

	public void AddAttributeShifts(List<global::Pair<string, float>> shifts)
	{
		int i = 0;
		for (int count = shifts.Count; i < count; i++)
		{
			global::Pair<string, float> shift = shifts[i];
			string attributeName = shift.First;
			float shiftValue = shift.Second;
			int attributeValue = 0;
			if (FinalAttributes.Get(attributeName, ref attributeValue))
			{
				FinalAttributes.Set(attributeName, (int)((float)attributeValue + shiftValue));
			}
		}
	}
}
