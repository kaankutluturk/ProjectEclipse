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

	public ModelParameters(ModelParameters NBMGOEMJJAF)
	{
		Perks = new List<PerkInfoItem>(NBMGOEMJJAF.Perks);
		WarriorPerks = new List<PerkInfoItem>(NBMGOEMJJAF.WarriorPerks);
		LearnedPerks = new List<PerkInfoItem>(NBMGOEMJJAF.LearnedPerks);
		FinalAttributes = new Attributes(NBMGOEMJJAF.FinalAttributes);
		BaseAttributes = new Attributes(NBMGOEMJJAF.BaseAttributes);
		SetLevel(NBMGOEMJJAF.GetLevel());
		FirstName = NBMGOEMJJAF.FirstName;
		Avatar = NBMGOEMJJAF.Avatar;
		Skeleton = NBMGOEMJJAF.Skeleton;
        EclipseBodyModel = NBMGOEMJJAF.EclipseBodyModel;
        EclipseCharacterId = NBMGOEMJJAF.EclipseCharacterId;
        EclipseSkinModels = (string[])NBMGOEMJJAF.EclipseSkinModels.Clone();
        EclipseRosterPlayer = NBMGOEMJJAF.EclipseRosterPlayer;
        EclipseVersusLook = NBMGOEMJJAF.EclipseVersusLook;
        _eclipseHiddenFigures.UnionWith(NBMGOEMJJAF._eclipseHiddenFigures);
        _eclipseAppearanceSkins = NBMGOEMJJAF._eclipseAppearanceSkins;
        _eclipseAppearanceVoice = NBMGOEMJJAF._eclipseAppearanceVoice;
		Weapon = NBMGOEMJJAF.Weapon;
		Armor = NBMGOEMJJAF.Armor;
		Helm = NBMGOEMJJAF.Helm;
		Ranged = NBMGOEMJJAF.Ranged;
		Magic = NBMGOEMJJAF.Magic;
		ModelDocuments.AddRange(NBMGOEMJJAF.ModelDocuments);
		ExcludedMoveNames.AddRange(NBMGOEMJJAF.ExcludedMoveNames);
		ExcludedPerkNames.AddRange(NBMGOEMJJAF.ExcludedPerkNames);
		IsPlayer = NBMGOEMJJAF.IsPlayer;
		AiControlled = NBMGOEMJJAF.AiControlled;
		UserControlled = NBMGOEMJJAF.UserControlled;
		IsWinner = NBMGOEMJJAF.IsWinner;
		RoundEnded = NBMGOEMJJAF.RoundEnded;
		IsDead = NBMGOEMJJAF.IsDead;
		RewardsEnabled = NBMGOEMJJAF.RewardsEnabled;
		AnimationEnabled = NBMGOEMJJAF.AnimationEnabled;
		EndRoundType = NBMGOEMJJAF.EndRoundType;
		OpponentCount = NBMGOEMJJAF.OpponentCount;
		SavedLife = NBMGOEMJJAF.SavedLife;
		MaxLife = NBMGOEMJJAF.MaxLife;
		RoundsWon = NBMGOEMJJAF.RoundsWon;
		RoundTotal = NBMGOEMJJAF.RoundTotal;
		Dan = NBMGOEMJJAF.Dan;
		WarriorPower = NBMGOEMJJAF.WarriorPower;
		ShieldTotal = NBMGOEMJJAF.ShieldTotal;
		HasShieldTotalOverride = NBMGOEMJJAF.HasShieldTotalOverride;
		RatingCorrection = NBMGOEMJJAF.RatingCorrection;
		LotteryLevel = NBMGOEMJJAF.LotteryLevel;
		Damage = NBMGOEMJJAF.Damage;
		Difficulty = NBMGOEMJJAF.Difficulty;
		OpponentIndex = NBMGOEMJJAF.OpponentIndex;
		BeginnerCheat = NBMGOEMJJAF.BeginnerCheat;
		UnknownFlag = NBMGOEMJJAF.BeginnerCheat;
		Voice = NBMGOEMJJAF.Voice;
		NoDoubles = NBMGOEMJJAF.NoDoubles;
		FightTactic = NBMGOEMJJAF.FightTactic;
		HasSourceNode = NBMGOEMJJAF.HasSourceNode;
		Node = NBMGOEMJJAF.Node;
		UnusedRoundFlag = NBMGOEMJJAF.UnusedRoundFlag;
		AutoTuneFactor = NBMGOEMJJAF.AutoTuneFactor;
		SpawnPosition = new Vector3f(NBMGOEMJJAF.SpawnPosition);
		RandomValue = NBMGOEMJJAF.RandomValue;
		Seal = NBMGOEMJJAF.Seal;
		IsUntouched = NBMGOEMJJAF.IsUntouched;
		MovesInitialized = NBMGOEMJJAF.MovesInitialized;
		SceneType = NBMGOEMJJAF.SceneType;
		playerRating = NBMGOEMJJAF.playerRating;
		enemyRating = NBMGOEMJJAF.enemyRating;
		playerRatingMagic = NBMGOEMJJAF.playerRatingMagic;
		enemyRatingMagic = NBMGOEMJJAF.enemyRatingMagic;
		playerRatingRanged = NBMGOEMJJAF.playerRatingRanged;
		enemyRatingRanged = NBMGOEMJJAF.enemyRatingRanged;
		_CurrentLife = NBMGOEMJJAF._CurrentLife;
		RecoverableLife = NBMGOEMJJAF.RecoverableLife;
		baseHealth = NBMGOEMJJAF.baseHealth;
		obscuredValueA = NBMGOEMJJAF.obscuredValueA;
		obscuredValueB = NBMGOEMJJAF.obscuredValueB;
		AttributeAlignments = new List<AttributesAlign>(NBMGOEMJJAF.AttributeAlignments);
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

	public void AddWarriorPerk(PerkInfoItem AEFFHJGMNFI)
	{
		WarriorPerks.AddIfNotExist(AEFFHJGMNFI);
	}

	public List<ItemInfo> GetEquippedItemsByType()
	{
		List<ItemInfo> list = new List<ItemInfo>();
		ItemInfo dJKEECEOCJB = GetItemByType("Skeleton");
		if (dJKEECEOCJB != null)
		{
			list.Add(dJKEECEOCJB);
		}
		dJKEECEOCJB = GetItemByType("Weapon");
		if (dJKEECEOCJB != null)
		{
			list.Add(dJKEECEOCJB);
		}
		dJKEECEOCJB = GetItemByType("Ranged");
		if (dJKEECEOCJB != null)
		{
			list.Add(dJKEECEOCJB);
		}
		dJKEECEOCJB = GetItemByType("Magic");
		if (dJKEECEOCJB != null)
		{
			list.Add(dJKEECEOCJB);
		}
		dJKEECEOCJB = GetItemByType("RaidConsumable");
		if (dJKEECEOCJB != null)
		{
			list.Add(dJKEECEOCJB);
		}
		dJKEECEOCJB = GetItemByType("Armor");
		if (dJKEECEOCJB != null)
		{
			list.Add(dJKEECEOCJB);
		}
		dJKEECEOCJB = GetItemByType("Helm");
		if (dJKEECEOCJB != null)
		{
			list.Add(dJKEECEOCJB);
		}
		dJKEECEOCJB = GetItemByType("Cheat");
		if (dJKEECEOCJB != null)
		{
			list.Add(dJKEECEOCJB);
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

	public ItemInfo GetItemByType(string LMNNBBKHMEI)
	{
		switch (LMNNBBKHMEI)
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

	public void SetItemByType(string LMNNBBKHMEI, ItemInfo item)
	{
		switch (LMNNBBKHMEI)
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

	private string GetItemNameByAttribute(string LMNNBBKHMEI)
	{
		switch (LMNNBBKHMEI)
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

	private ItemInfo GetItemByAttribute(string LMNNBBKHMEI)
	{
		switch (LMNNBBKHMEI)
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

	public int SumItemAttribute(string name, ref bool GMEMHMOHFGG)
	{
		List<ItemInfo> hELFDCAIJNE = GetEquippedItems();
		return SumItemAttribute(name, hELFDCAIJNE, ref GMEMHMOHFGG);
	}

	public int SumItemAttribute(string name, List<ItemInfo> HELFDCAIJNE, ref bool GMEMHMOHFGG)
	{
		GMEMHMOHFGG = false;
		int num = 0;
		for (int i = 0; i < HELFDCAIJNE.Count; i++)
		{
			int OEMALIFPGPO = 0;
			if (HELFDCAIJNE[i].ItemAttributes.Get(name, ref OEMALIFPGPO))
			{
				GMEMHMOHFGG = true;
				num += OEMALIFPGPO;
			}
		}
		return num;
	}

	private int SumPerkAttribute(string name, ref bool GMEMHMOHFGG)
	{
		int num = 0;
		for (int i = 0; i < Perks.Count; i++)
		{
			int OEMALIFPGPO = 0;
			if (Perks[i].AttributeValues.Get(name, ref OEMALIFPGPO))
			{
				GMEMHMOHFGG = true;
				num += OEMALIFPGPO;
			}
		}
		return num;
	}

	public void CalculateAttributes()
	{
		ClearAttributes();
		List<ItemInfo> hELFDCAIJNE = GetEquippedItems();
		List<WarriorAttribute> iBLHIAHECLK = GameUtils.WarriorAttributeList.AttributeList;
		for (int i = 0; i < iBLHIAHECLK.Count; i++)
		{
			string text = iBLHIAHECLK[i].get_Name();
			int OEMALIFPGPO = 0;
			if (BaseAttributes.Get(text, ref OEMALIFPGPO))
			{
				FinalAttributes.Set(text, OEMALIFPGPO, true);
				continue;
			}
			int num = 0;
			bool GMEMHMOHFGG = false;
			num += SumItemAttribute(text, hELFDCAIJNE, ref GMEMHMOHFGG);
			num += SumPerkAttribute(text, ref GMEMHMOHFGG);
			int OEMALIFPGPO2 = 0;
			if (GameUtils.StartingAttributes.Gains.Get(text, ref OEMALIFPGPO2))
			{
				num += OEMALIFPGPO2;
				GMEMHMOHFGG = true;
			}
			int OEMALIFPGPO3 = 0;
			if (GameUtils.LevelAttributeGains.Gains.Get(text, ref OEMALIFPGPO3))
			{
				num += (ObscuredInt)(level) * OEMALIFPGPO3;
				GMEMHMOHFGG = true;
			}
			if (GMEMHMOHFGG || !iBLHIAHECLK[i].UnusedFlag)
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
			bool bAINMLLIKOL = item.Type == "Weapon";
			foreach (PerkInfoItem item2 in item.InnatePerks)
			{
				item2.SetIsWeaponPerk(bAINMLLIKOL);
				list.Add(item2);
			}
			if (item.IgnoreInventoryEnchantments || !IsPlayer)
			{
				continue;
			}
			UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(item);
			if (dKCHDHMLKHN == null)
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
						item3.SetIsWeaponPerk(bAINMLLIKOL);
						list.Add(item3);
					}
				}
				else
				{
					item3.SetIsWeaponPerk(bAINMLLIKOL);
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

	public static void ParseRatingConfig(XmlNode AFHNINCKJEE)
	{
		impossibleRatio = XmlUtils.ParseFloat(AFHNINCKJEE.Attributes["ImpossibleRatio"]);
		easyRatio = XmlUtils.ParseFloat(AFHNINCKJEE.Attributes["EasyRatio"]);
		ratingEvaluations.Clear();
		perkAspectParameter = XmlUtils.ParseString(AFHNINCKJEE.Attributes["PerkAspectParameter"]);
		foreach (XmlNode childNode in AFHNINCKJEE.ChildNodes)
		{
			RatingEvaluation dCAFHLLHFJO = new RatingEvaluation();
			ratingEvaluations.Add(dCAFHLLHFJO);
			dCAFHLLHFJO.nodeName = childNode.Name;
			dCAFHLLHFJO.evaluationName = XmlUtils.ParseString(childNode.Attributes["Name"]);
			Evaluation.ParseAttributes(childNode, dCAFHLLHFJO.evaluations);
			Evaluation.ParseAverageQuantity(childNode, dCAFHLLHFJO);
			Evaluation.ParseAverageDamageAndRecharge(childNode, dCAFHLLHFJO);
			dCAFHLLHFJO.cancellingItem = XmlUtils.ParseString(childNode.Attributes["CancellingItem"]);
			Defense.Parse(childNode, dCAFHLLHFJO.defenses);
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
		int OEMALIFPGPO = 0;
		if (FinalAttributes.Get(name, ref OEMALIFPGPO))
		{
			return OEMALIFPGPO;
		}
		Debug.LogErrorFormat("Parameter \"{0}\" not found!", name);
		return float.MinValue;
	}

	private float GetParametersValue(List<string> NIKHAICFGNM)
	{
		float num = float.MinValue;
		foreach (string item in NIKHAICFGNM)
		{
			int OEMALIFPGPO = 0;
			if (FinalAttributes.Get(item, ref OEMALIFPGPO) && num < (float)OEMALIFPGPO)
			{
				num = OEMALIFPGPO;
			}
		}
		if (num == float.MinValue)
		{
			Debug.LogError("Parameters not found!");
		}
		return num;
	}

	private float GetMaxEvaluationValue(List<Evaluation> EKJDIGGGEBH, ModelParameters JCICKLIMBEF)
	{
		float num = float.MinValue;
		foreach (Evaluation item in EKJDIGGGEBH)
		{
			float num2 = JCICKLIMBEF.GetParametersValue(item.Name) + item.Shift;
			if (num < num2)
			{
				num = num2;
			}
		}
		return num;
	}

	public float CalculateDamageRating(ModelParameters AKBNKDBHCEO, List<global::Pair<string, float>> PNLMJFLBGMA)
	{
		float num = 0f;
		List<global::Pair<string, float>> list = new List<global::Pair<string, float>>();
		foreach (RatingEvaluation item in ratingEvaluations)
		{
			if (!(item.nodeName == "Damage"))
			{
				continue;
			}
			RatingEvaluation dCAFHLLHFJO = item;
			if (HasItem(dCAFHLLHFJO.cancellingItem))
			{
				continue;
			}
			float cDCIEOFCKNO = dCAFHLLHFJO.averageBaseDamage;
			list.Clear();
			CollectEvaluationAttributes(list, dCAFHLLHFJO.evaluations);
			AddShiftsToPairs(list, PNLMJFLBGMA);
			List<Defense> cKJBFNJEDHH = dCAFHLLHFJO.defenses;
			ApplyDefenseShiftsToOpponent(AKBNKDBHCEO, cKJBFNJEDHH, PNLMJFLBGMA);
			float num2 = 0f;
			foreach (Defense item2 in cKJBFNJEDHH)
			{
				if (item2.Evaluations.Count != 1)
				{
					Debug.LogError("Count of DefenseAttribute != 1");
				}
				if (HasItem(item2.CancellingItem))
				{
					continue;
				}
				float num3 = GameUtils.GetAttributesHitMultiplier(IsPlayer, this, AKBNKDBHCEO, list, item2.Evaluations[0].Name);
				num3 = Mathf.Min(1f, cDCIEOFCKNO * num3);
				List<PerkInfoItem> list2 = GetAllPerks();
				foreach (PerkInfoItem item3 in list2)
				{
					List<Rating> mLMLENHGNDJ = item3.Ratings;
					if (mLMLENHGNDJ.Count == 0)
					{
						continue;
					}
					foreach (Rating item4 in mLMLENHGNDJ)
					{
						if (!(item4.player != "Me") && (string.IsNullOrEmpty(item4.damageType) || item4.damageType == dCAFHLLHFJO.evaluationName) && (string.IsNullOrEmpty(item4.defenseType) || item4.defenseType == item2.DefenseName))
						{
							float num4 = 0f;
							if (!string.IsNullOrEmpty(perkAspectParameter))
							{
								string s = item3.GetSetValue(perkAspectParameter);
								num4 = float.Parse(s);
							}
							num3 *= 1f + (item4.Multiplier - 1f) * PerkInfoItem.AspectToMultiplier(num4 - AKBNKDBHCEO.GetParametersValue(item4.enemyAttribute));
						}
					}
				}
				List<PerkInfoItem> list3 = AKBNKDBHCEO.GetAllPerks();
				foreach (PerkInfoItem item5 in list3)
				{
					List<Rating> mLMLENHGNDJ2 = item5.Ratings;
					if (mLMLENHGNDJ2.Count == 0)
					{
						continue;
					}
					foreach (Rating item6 in mLMLENHGNDJ2)
					{
						if (!(item6.player != "Enemy") && (string.IsNullOrEmpty(item6.damageType) || item6.damageType == dCAFHLLHFJO.evaluationName) && (string.IsNullOrEmpty(item6.defenseType) || item6.defenseType == item2.DefenseName))
						{
							float num5 = 0f;
							if (!string.IsNullOrEmpty(perkAspectParameter))
							{
								string s2 = item5.GetSetValue(perkAspectParameter);
								num5 = float.Parse(s2);
							}
							num3 /= 1f + (item6.Multiplier - 1f) * PerkInfoItem.AspectToMultiplier(num5 - AKBNKDBHCEO.GetParametersValue(item6.enemyAttribute));
						}
					}
				}
				num2 += item2.Weight * num3;
			}
			if (dCAFHLLHFJO.magicRechargeRate > 0f)
			{
				float oFHGAJDLIDB = dCAFHLLHFJO.magicRechargeRate;
				float num6 = GameUtils.MagicConfig.GetPainRecharge();
				float num7 = GameUtils.MagicConfig.GetDamageRecharge();
				float num8 = GameUtils.MagicConfig.GetPainRecharge(this);
				float num9 = GameUtils.MagicConfig.GetDamageRecharge(this);
				num2 *= oFHGAJDLIDB * (num6 * num8 + num7 * num9);
			}
			num += num2;
		}
		return num;
	}

	private bool HasItem(string OHCGEEEKEJH)
	{
		List<ItemInfo> list = GetEquippedItems();
		foreach (ItemInfo item in list)
		{
			if (item.Name == OHCGEEEKEJH)
			{
				return true;
			}
		}
		return false;
	}

	private void AddShiftsToPairs(List<global::Pair<string, float>> FFJLHDENIEB, List<global::Pair<string, float>> PNLMJFLBGMA)
	{
		if (PNLMJFLBGMA == null)
		{
			return;
		}
		foreach (global::Pair<string, float> item in PNLMJFLBGMA)
		{
			string lLHEDBIEHAA = item.First;
			float nFNBFHCDEGG = item.Second;
			for (int i = 0; i < FFJLHDENIEB.Count; i++)
			{
				if (FFJLHDENIEB[i].First == lLHEDBIEHAA)
				{
					FFJLHDENIEB[i] = new global::Pair<string, float>(FFJLHDENIEB[i].First, FFJLHDENIEB[i].Second + nFNBFHCDEGG);
					break;
				}
			}
		}
	}

	private void ApplyDefenseShiftsToOpponent(ModelParameters AKBNKDBHCEO, List<Defense> PLPANBKKOEN, List<global::Pair<string, float>> PNLMJFLBGMA)
	{
		if (PNLMJFLBGMA == null)
		{
			return;
		}
		foreach (global::Pair<string, float> item in PNLMJFLBGMA)
		{
			string lLHEDBIEHAA = item.First;
			float nFNBFHCDEGG = item.Second;
			for (int i = 0; i < PLPANBKKOEN.Count; i++)
			{
				foreach (Evaluation item2 in PLPANBKKOEN[i].Evaluations)
				{
					if (item2.Name == lLHEDBIEHAA)
					{
						int OEMALIFPGPO = 0;
						AKBNKDBHCEO.FinalAttributes.Get(lLHEDBIEHAA, ref OEMALIFPGPO);
						AKBNKDBHCEO.FinalAttributes.Set(lLHEDBIEHAA, (int)((float)OEMALIFPGPO + nFNBFHCDEGG));
					}
				}
			}
		}
	}

	public void RemovePerksByNames(List<PerkInfoItem> JOGBKOJCINM, List<string> NIKHAICFGNM)
	{
		foreach (string item in NIKHAICFGNM)
		{
			for (int num = JOGBKOJCINM.Count - 1; num >= 0; num--)
			{
				if (IsPerkInNames(JOGBKOJCINM[num], item))
				{
					JOGBKOJCINM.RemoveAt(num);
				}
			}
		}
	}

	private bool IsPerkInNames(PerkInfoItem AEFFHJGMNFI, string name)
	{
		if (AEFFHJGMNFI == null)
		{
			return false;
		}
		return AEFFHJGMNFI.IsPerkByNames(name);
	}

	public bool UpdateLife(float DLEDDPFNPOH)
	{
		ChangeLife(DLEDDPFNPOH);
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

	private int CollectEvaluationAttributes(string name, List<string> OEMALIFPGPO)
	{
		int count = OEMALIFPGPO.Count;
		foreach (RatingEvaluation item in ratingEvaluations)
		{
			if (!(item.nodeName == name))
			{
				continue;
			}
			foreach (Evaluation item2 in item.evaluations)
			{
				OEMALIFPGPO.AddIfNotExist(item2.Name);
			}
		}
		return OEMALIFPGPO.Count - count;
	}

	private int CollectDefenseAttributes(string name, List<string> OEMALIFPGPO)
	{
		int count = OEMALIFPGPO.Count;
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
					OEMALIFPGPO.AddIfNotExist(item3.Name);
				}
			}
		}
		return OEMALIFPGPO.Count - count;
	}

	private int CollectEvaluationAttributes(List<global::Pair<string, float>> OEMALIFPGPO, List<Evaluation> JMMIKHLIKOE)
	{
		int count = OEMALIFPGPO.Count;
		foreach (Evaluation item in JMMIKHLIKOE)
		{
			OEMALIFPGPO.Add(new global::Pair<string, float>(item.Name, item.Shift));
		}
		return OEMALIFPGPO.Count - count;
	}

	private int CollectDamageAttributes(List<string> OEMALIFPGPO)
	{
		return CollectEvaluationAttributes("Damage", OEMALIFPGPO);
	}

	private int CollectMagicAttributes(List<string> OEMALIFPGPO)
	{
		return CollectEvaluationAttributes("Magic", OEMALIFPGPO);
	}

	private int CollectRangedAttributes(List<string> OEMALIFPGPO)
	{
		return CollectEvaluationAttributes("Ranged", OEMALIFPGPO);
	}

	private int CollectDamageDefenseAttributes(List<string> OEMALIFPGPO)
	{
		return CollectDefenseAttributes("Damage", OEMALIFPGPO);
	}

	private int CollectMagicDefenseAttributes(List<string> OEMALIFPGPO)
	{
		return CollectDefenseAttributes("Magic", OEMALIFPGPO);
	}

	private int CollectRangedDefenseAttributes(List<string> OEMALIFPGPO)
	{
		return CollectDefenseAttributes("Ranged", OEMALIFPGPO);
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

	public void AddLife(float AOGLLMEFEJB = 1f)
	{
		float num = (ObscuredFloat)(_CurrentLife);
		num += AOGLLMEFEJB;
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

	private void SetAttributes(ModelParameters IHEFAMAFBIA)
	{
		FinalAttributes = IHEFAMAFBIA.FinalAttributes;
	}

	public void CopyEquippedItemsTo(EquippedItemsStruct HELFDCAIJNE)
	{
		HELFDCAIJNE.Armor = Armor;
		HELFDCAIJNE.Helm = Helm;
		HELFDCAIJNE.Seal = Seal;
		HELFDCAIJNE.Skeleton = Skeleton;
		HELFDCAIJNE.Weapon = Weapon;
		HELFDCAIJNE.Magic = Magic;
		HELFDCAIJNE.Ranged = Ranged;
	}

	public void SetEquippedItemsFrom(EquippedItemsStruct HELFDCAIJNE)
	{
		Armor = HELFDCAIJNE.Armor;
		Helm = HELFDCAIJNE.Helm;
		Seal = HELFDCAIJNE.Seal;
		Skeleton = HELFDCAIJNE.Skeleton;
		Weapon = HELFDCAIJNE.Weapon;
		Magic = HELFDCAIJNE.Magic;
		Ranged = HELFDCAIJNE.Ranged;
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

	public void SetItemsFromRules(List<ItemRule> GEEJLFGCKNJ, bool FFBFPLODJME, int round = 0)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		foreach (ItemRule item in GEEJLFGCKNJ)
		{
			if (round > 0 && !item.AppliesToRound(round))
			{
				continue;
			}
			UserItem dKCHDHMLKHN = item.get_Item();
			if (dKCHDHMLKHN == null)
			{
				GameLog.Error(" ModelParameters::setItemsFromRules - UserItem not found ");
				continue;
			}
			ItemInfo dJKEECEOCJB = dKCHDHMLKHN.GetInfo();
			string text = dKCHDHMLKHN.get_Name();
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			UserItem dKCHDHMLKHN2 = nKGLHEGIKKP.GetInventory().FindItem(text);
			dJKEECEOCJB = null;
			if (dKCHDHMLKHN2 == null)
			{
				dJKEECEOCJB = ListSF.GetItems().GetItemByName(text);
				if (dJKEECEOCJB == null)
				{
					GameLog.Error(" Model::equipRulesItems - item not found \"{0}\"", text);
					continue;
				}
			}
			else
			{
				dJKEECEOCJB = dKCHDHMLKHN2.GetInfo();
			}
			if ((dJKEECEOCJB == null || !nKGLHEGIKKP.GetInventory().HasItem(dJKEECEOCJB)) && !item.GetIsEquipRule())
			{
				dJKEECEOCJB = null;
			}
			if (dJKEECEOCJB != null && (!FFBFPLODJME || !item.GetNoAttributeChange()))
			{
				ItemInfo dJKEECEOCJB2 = GetItemByType(dJKEECEOCJB.Type);
				ItemInfo dJKEECEOCJB3 = dJKEECEOCJB.Clone();
				dJKEECEOCJB3.IgnoreInventoryEnchantments = true;
				SetItemByType(dJKEECEOCJB.Type, dJKEECEOCJB3);
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

	public void AddAttributeShifts(List<global::Pair<string, float>> LHGAKDLAPJB)
	{
		int i = 0;
		for (int count = LHGAKDLAPJB.Count; i < count; i++)
		{
			global::Pair<string, float> cCKLNOPEKHO = LHGAKDLAPJB[i];
			string lLHEDBIEHAA = cCKLNOPEKHO.First;
			float nFNBFHCDEGG = cCKLNOPEKHO.Second;
			int OEMALIFPGPO = 0;
			if (FinalAttributes.Get(lLHEDBIEHAA, ref OEMALIFPGPO))
			{
				FinalAttributes.Set(lLHEDBIEHAA, (int)((float)OEMALIFPGPO + nFNBFHCDEGG));
			}
		}
	}
}
