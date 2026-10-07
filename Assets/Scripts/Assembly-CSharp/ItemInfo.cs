using System.Collections.Generic;
using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;
using SF2.Offline;

public class ItemInfo
{
	public enum SpendType
	{
		SPEND_TYPE_NONE = 0,
		SPEND_TYPE_ENERGY = 1
	}

	public enum DiscountSource
	{
		DISCOUNT_NONE = 0,
		DISCOUNT_QUEST = 1,
		DISCOUNT_CONFIG = 2
	}

	public const string TypeNoneItem = "NoneItem";

	public const string TypeSkeleton = "Skeleton";

	public const string TypeWeapon = "Weapon";

	public const string TypeArmor = "Armor";

	public const string TypeHelm = "Helm";

	public const string TypeRanged = "Ranged";

	public const string TypeMagic = "Magic";

	public const string TypeRealMoneyItem = "RealMoneyItem";

	public const string TypeEnergy = "Energy";

	public const string TypeDummy = "Dummy";

	public const string TypeDecorate = "Decorate";

	public const string TypeCheat = "Cheat";

	public const string TypeSeal = "Seal";

	public const string TypeFree = "Free";

	public const string TypeProfile = "Profile";

	public const string TypeRecipe = "Recipe";

	public const string TypeConsumable = "Consumable";

	public const string TypeRaidConsumable = "RaidConsumable";

	public const string TypeRaidItemPack = "RaidItemPack";

	public const string TypeGold = "Gold";

	public const string TypeBonus = "Bonus";

	public const string TypeUnlimitedEnergy = "UnlimitedEnergy";

	public const string TypeStarterPack = "StarterPack";

	public const string TypeTapJoy = "TapJoy";

	public const string TypeSponsorPay = "SponsorPay";

	public const string TypeMetaps = "Metaps";

	public const string TypeVideo = "Video";

	public const string TypeFacebookLike = "Facebook_Like";

	public const string TypePerkReset = "PerkReset";

	public const string TypeCurrency = "Currency";

	public const string TypeRaidCurrency = "RaidCurrency";

	public const string TypeRaidCharge = "RaidCharge";

	public const string TypeRaidPotion = "RaidPotion";

	public const string TypeRaidHorn = "RaidHorn";

	public string Name = string.Empty;

	public string FileName = string.Empty;

	// best guess for name
	public string ModelFileName = string.Empty;

	public string Type = string.Empty;

	// best guess for name
	public string SubType = string.Empty;

	private CombatSubtypeOverride _combatSubtypeOverride;
	private sealed class CombatSubtypeOverride : System.IDisposable
	{
		private ItemInfo _item;
		private readonly string _previous;
		public CombatSubtypeOverride(ItemInfo item) { _item = item; _previous = item.SubType; }
		public void Dispose()
		{
			ItemInfo item = _item;
			if (item == null) return;
			_item = null;
			if (!object.ReferenceEquals(item._combatSubtypeOverride, this)) return;
			item.SubType = _previous;
			item._combatSubtypeOverride = null;
		}
	}

	// Apply before profile/fight copies are built. Existing copies keep their snapshot.
	internal bool TryOverrideCombatSubtype(string subtype, out System.IDisposable lifetime)
	{
		lifetime = null;
		if ((Type != "Weapon" && Type != "Ranged" && Type != "Magic") ||
			_combatSubtypeOverride != null || string.IsNullOrEmpty(subtype) || subtype.Length > 128) return false;
		foreach (char c in subtype)
			if (!(c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == '_')) return false;
		var replacement = new CombatSubtypeOverride(this);
		SubType = subtype;
		_combatSubtypeOverride = replacement;
		lifetime = replacement;
		return true;
	}

	private InitialProfileOverride _initialProfileOverride;
	private sealed class InitialProfileOverride : System.IDisposable
	{
		private ItemInfo _item;
		private readonly bool _hadLevel;
		private readonly int _level;
		private readonly int _upgradeLevel;
		private readonly string _upgradeTemplate;
		private readonly string _legacyPaidItem;
		private readonly List<UpgradeData> _localUpgrades;
		private readonly Attributes _attributes;
		public InitialProfileOverride(ItemInfo item)
		{
			_item = item;
			_hadLevel = item.HasAuthoredLevel;
			_level = item.ItemLevel;
			_upgradeLevel = item.UpgradeLevel;
			_upgradeTemplate = item.UpgradeTemplateName;
			_legacyPaidItem = item.LegacyPaidItem;
			_localUpgrades = item.LocalUpgrades;
			_attributes = new Attributes(item.ItemAttributes);
		}
		public void Dispose()
		{
			ItemInfo item = _item;
			if (item == null) return;
			_item = null;
			if (!object.ReferenceEquals(item._initialProfileOverride, this)) return;
			item.HasAuthoredLevel = _hadLevel;
			item.ItemLevel = _level;
			item.UpgradeLevel = _upgradeLevel;
			item.UpgradeTemplateName = _upgradeTemplate;
			item.LegacyPaidItem = _legacyPaidItem;
			item.LocalUpgrades = _localUpgrades;
			item.ItemAttributes = new Attributes(_attributes);
			item._initialProfileOverride = null;
		}
	}

	// The catalog item changes in place before new shop/fighter copies are built.
	// Its recovered NodeXML, price, shared upgrade tables and saved item identity are untouched.
	internal bool TryOverrideInitialProfile(int level, int upgradeLevel,
		System.Collections.Generic.IReadOnlyDictionary<string, int> stats, string upgradeTemplate, string legacyPaidItem,
		bool clearLocalUpgrades,
		out System.IDisposable lifetime)
	{
		lifetime = null;
		if ((Type != "Weapon" && Type != "Armor" && Type != "Helm" && Type != "Ranged" && Type != "Magic") ||
			_initialProfileOverride != null || level < 1 || level > 52 || upgradeLevel < 0 || upgradeLevel > 5200 || stats == null)
			return false;
		if (upgradeTemplate != null && upgradeTemplate != Type + "_Bonus" && upgradeTemplate != "Paid_" + Type + "_Bonus")
			return false;
		if (legacyPaidItem != null && legacyPaidItem != "None" && legacyPaidItem != "Paid" && legacyPaidItem != "SuperPaid")
			return false;
		var attributes = new Attributes();
		foreach (var stat in stats)
		{
			if (stat.Value < 0 || stat.Value > 1000000) return false;
			attributes.Set(stat.Key, stat.Value);
		}
		var replacement = new InitialProfileOverride(this);
		HasAuthoredLevel = true;
		ItemLevel = level;
		UpgradeLevel = upgradeLevel;
		if (upgradeTemplate != null) UpgradeTemplateName = upgradeTemplate;
		if (legacyPaidItem != null) LegacyPaidItem = legacyPaidItem;
		if (clearLocalUpgrades) LocalUpgrades = new List<UpgradeData>();
		ItemAttributes = attributes;
		_initialProfileOverride = replacement;
		lifetime = replacement;
		return true;
	}

	private ShopPriceOverride _shopPriceOverride;
	private sealed class ShopPriceOverride : System.IDisposable
	{
		private ItemInfo _item;
		private readonly ObscuredLong _coinPrice;
		private readonly ObscuredLong _gemPrice;
		public ShopPriceOverride(ItemInfo item)
		{
			_item = item;
			_coinPrice = item.CoinPrice;
			_gemPrice = item.GemPrice;
		}
		public void Dispose()
		{
			ItemInfo item = _item;
			if (item == null) return;
			_item = null;
			if (!object.ReferenceEquals(item._shopPriceOverride, this)) return;
			item.CoinPrice = _coinPrice;
			item.GemPrice = _gemPrice;
			item._shopPriceOverride = null;
		}
	}

	// Update the native catalog before shop copies are built. Both currency fields
	// are replaced so switching currency cannot leave the old price active.
	internal bool TryOverrideShopPrice(long coins, long gems, out System.IDisposable lifetime)
	{
		lifetime = null;
		if ((Type != "Weapon" && Type != "Armor" && Type != "Helm" && Type != "Ranged" && Type != "Magic") ||
			_shopPriceOverride != null || coins < 0 || gems < 0 || coins > int.MaxValue || gems > int.MaxValue ||
			(coins == 0 && gems == 0)) return false;
		var replacement = new ShopPriceOverride(this);
		CoinPrice = (ObscuredLong)coins;
		GemPrice = (ObscuredLong)gems;
		_shopPriceOverride = replacement;
		lifetime = replacement;
		return true;
	}

	private ItemPresentationOverride _presentationOverride;
	private sealed class ItemPresentationOverride : System.IDisposable
	{
		private ItemInfo _item;
		private readonly string _icon;
		private readonly string _model;
		public ItemPresentationOverride(ItemInfo item)
		{ _item = item; _icon = item.FileName; _model = item.ModelFileName; }
		public void Dispose()
		{
			ItemInfo item = _item;
			if (item == null) return;
			_item = null;
			if (!object.ReferenceEquals(item._presentationOverride, this)) return;
			item.FileName = _icon;
			item.ModelFileName = _model;
			item._presentationOverride = null;
		}
	}

	internal bool TryOverridePresentation(string icon, string model, out System.IDisposable lifetime)
	{
		lifetime = null;
		if ((Type != "Weapon" && Type != "Armor" && Type != "Helm" && Type != "Ranged" && Type != "Magic") ||
			_presentationOverride != null || (string.IsNullOrEmpty(icon) && string.IsNullOrEmpty(model))) return false;
		var replacement = new ItemPresentationOverride(this);
		if (!string.IsNullOrEmpty(icon)) FileName = icon;
		if (!string.IsNullOrEmpty(model)) ModelFileName = model;
		_presentationOverride = replacement;
		lifetime = replacement;
		return true;
	}

	// AI table grouping may differ from the subtype used by animations and conditions.
	internal string TacticSubtype { get; private set; } = string.Empty;
	internal string EffectiveTacticSubtype => string.IsNullOrEmpty(TacticSubtype) ? SubType : TacticSubtype;
	private TacticSubtypeOverride _tacticSubtypeOverride;

	private sealed class TacticSubtypeOverride : System.IDisposable
	{
		private ItemInfo _item;
		private readonly string _previous;
		public TacticSubtypeOverride(ItemInfo item) { _item = item; _previous = item.TacticSubtype; }
		public void Dispose()
		{
			ItemInfo item = _item;
			if (item == null) return;
			_item = null;
			if (!object.ReferenceEquals(item._tacticSubtypeOverride, this)) return;
			item.TacticSubtype = _previous;
			item._tacticSubtypeOverride = null;
		}
	}

	// Empty explicitly restores subtype fallback; null is not an override request.
	// Existing fight copies retain their snapshot until the next content lifecycle.
	internal bool TryOverrideTacticSubtype(string group, out System.IDisposable lifetime)
	{
		lifetime = null;
		if (Type != "Weapon" || _tacticSubtypeOverride != null || group == null || group.Length > 128) return false;
		foreach (char c in group)
			if (!(c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == '_')) return false;
		var replacement = new TacticSubtypeOverride(this);
		TacticSubtype = group;
		_tacticSubtypeOverride = replacement;
		lifetime = replacement;
		return true;
	}

	public string LegacyAlias = string.Empty;

	public string LegacyDescription = string.Empty;

	public string LegacyTitle = string.Empty;

	public string GroupId = string.Empty;

	private string marketId = string.Empty;

	private bool isConsumable;

	public string LocalizedPriceString = string.Empty;

	public string PriceAmountText = string.Empty;

	public string CurrencyCode = string.Empty;

	// best guess for name
	public string LegacyPaidItem = string.Empty;

	public string DescriptionAlias = string.Empty;

	public string ButtonTextAlias = string.Empty;

	public string IconName = string.Empty;

	// best guess for name
	public string UpgradeTemplateName = string.Empty;

	public bool SpendAfterUse;

	// best guess for name
	public bool HasAuthoredLevel;

	private bool isEnabledByDefault;

	public int Index;

	public int ItemId;

	// best guess for name
	public int ItemLevel;

	public int HiddenFlag;

	public bool IsShopVisible;

	private bool defaultShopVisible;

	public int Milestone;

	public int SilentReceive;

	// best guess for name
	public int UpgradeLevel;

	public long DeliveryTime;

	// best guess for name
	public ObscuredLong CoinPrice = (ObscuredLong)(0L);

	// best guess for name
	public ObscuredLong GemPrice = (ObscuredLong)(0L);

	public ObscuredLong DeliveryGemPrice = (ObscuredLong)(0L);

	public ObscuredLong DeliveryCoinPrice = (ObscuredLong)(0L);

	public bool IsPaid;

	public ObscuredLong ReceiveGold = (ObscuredLong)(0L);

	public ObscuredLong ReceiveBonus = (ObscuredLong)(0L);

	public ItemInfo ParentItem;

	public XmlNode NodeXML;

	// best guess for name
	public bool IgnoreInventoryEnchantments;

	private bool hasDeliveryDescription;

	// best guess for name
	public Attributes ItemAttributes = new Attributes();

	// best guess for name
	public List<PerkInfoItem> InnatePerks = new List<PerkInfoItem>();
	private InnatePerkOverride _innatePerkOverride;

	private sealed class InnatePerkOverride : System.IDisposable
	{
		private ItemInfo _item;
		private readonly List<PerkInfoItem> _previous;
		public InnatePerkOverride(ItemInfo item) { _item = item; _previous = item.InnatePerks; }
		public void Dispose()
		{
			ItemInfo item = _item;
			if (item == null) return;
			_item = null;
			if (!object.ReferenceEquals(item._innatePerkOverride, this)) return;
			item.InnatePerks = _previous;
			item._innatePerkOverride = null;
		}
	}

	internal bool TryOverrideInnatePerks(XmlNode perks, out System.IDisposable lifetime)
	{
		lifetime = null;
		if (_innatePerkOverride != null || perks == null || perks.Name != "Perks") return false;
		var resolved = new List<PerkInfoItem>();
		var names = new HashSet<string>(System.StringComparer.Ordinal);
		foreach (XmlNode child in perks.ChildNodes)
		{
			if (child.NodeType == XmlNodeType.Comment || child.NodeType == XmlNodeType.Whitespace) continue;
			if (child.NodeType != XmlNodeType.Element || child.Name != "Perk" || resolved.Count >= 64) return false;
			string name = child.Attributes?["Name"]?.Value;
			if (string.IsNullOrEmpty(name) || !names.Add(name)) return false;
			PerkInfoItem definition = GameUtils.PerkItemList.FindBasePerk(name);
			if (definition == null) return false;
			// Combat marks equipment perks as weapon/non-weapon. Never share that mutable marker with the registry or another item.
			resolved.Add(definition.Clone(child["Set"], child["RatingEvaluation"]));
		}
		var replacement = new InnatePerkOverride(this);
		InnatePerks = resolved;
		_innatePerkOverride = replacement;
		lifetime = replacement;
		return true;
	}

	// best guess for name
	public List<UpgradeData> LocalUpgrades = new List<UpgradeData>();

	// best guess for name
	public List<PerkInfoItem> DefaultEnchantmentPreviews = new List<PerkInfoItem>();

	// best guess for name
	public List<PerkInfoItem> ParsedPerks = new List<PerkInfoItem>();

	// best guess for name
	public List<PerkStruct> DefaultEnchantments = new List<PerkStruct>();

	private DefaultEnchantmentOverride _defaultEnchantmentOverride;

	private sealed class DefaultEnchantmentOverride : System.IDisposable
	{
		private ItemInfo _item;
		private readonly List<PerkInfoItem> _previousPreview;
		private readonly List<PerkStruct> _previousGrants;
		public DefaultEnchantmentOverride(ItemInfo item)
		{ _item = item; _previousPreview = item.DefaultEnchantmentPreviews; _previousGrants = item.DefaultEnchantments; }
		public void Dispose()
		{
			ItemInfo item = _item;
			if (item == null) return;
			_item = null;
			if (!object.ReferenceEquals(item._defaultEnchantmentOverride, this)) return;
			item.DefaultEnchantmentPreviews = _previousPreview;
			item.DefaultEnchantments = _previousGrants;
			item._defaultEnchantmentOverride = null;
		}
	}

	// Update shop preview and acquisition defaults together. Existing UserItem save nodes are untouched.
	internal bool TryOverrideDefaultEnchantments(XmlNode enchantments, out System.IDisposable lifetime)
	{
		lifetime = null;
		if (_defaultEnchantmentOverride != null || enchantments == null || enchantments.Name != "Enchantments") return false;
		var previews = new List<PerkInfoItem>();
		var grants = new List<PerkStruct>();
		var names = new HashSet<string>(System.StringComparer.Ordinal);
		foreach (XmlNode child in enchantments.ChildNodes)
		{
			if (child.NodeType == XmlNodeType.Comment || child.NodeType == XmlNodeType.Whitespace) continue;
			if (child.NodeType != XmlNodeType.Element || child.Name != "Perk" || grants.Count >= 64) return false;
			string name = child.Attributes?["Name"]?.Value;
			if (string.IsNullOrEmpty(name) || !names.Add(name)) return false;
			PerkInfoItem preview = ParsePerk(child);
			if (preview == null) return false;
			previews.Add(preview);
			grants.Add(new PerkStruct(child));
		}
		var replacement = new DefaultEnchantmentOverride(this);
		DefaultEnchantmentPreviews = previews;
		DefaultEnchantments = grants;
		_defaultEnchantmentOverride = replacement;
		lifetime = replacement;
		return true;
	}

	private bool isNew;

	public string CurrencyName = string.Empty;

	public ObscuredInt CurrencyValue = (ObscuredInt)(0);

	private int addPercent;

	private int priceDigits;

	private bool unusedFlag;

	public ObscuredInt LotteryPrice = (ObscuredInt)(0);

	public bool IsUpgradePurchase;

	public long MissingCoins;

	public long MissingGems;

	public string MarketId
	{
		get
		{
			return GetMarketId();
		}
		set
		{
			set_MarketID(value);
		}
	}

	public bool IsConsumable
	{
		get
		{
			return GetIsConsumable();
		}
	}

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

	public ItemInfo(XmlNode node)
	{
		if (node != null)
		{
			Init();
			ParseNode(node);
		}
	}

	protected ItemInfo()
	{
		Init();
	}

	protected ItemInfo(ItemInfo item)
	{
		Name = item.Name;
		FileName = item.FileName;
		ModelFileName = item.ModelFileName;
		Type = item.Type;
		SubType = item.SubType;
		TacticSubtype = item.TacticSubtype;
		LegacyAlias = item.LegacyAlias;
		LegacyDescription = item.LegacyDescription;
		LegacyTitle = item.LegacyTitle;
		GroupId = item.GroupId;
		marketId = item.marketId;
		isConsumable = item.isConsumable;
		LocalizedPriceString = item.LocalizedPriceString;
		PriceAmountText = item.PriceAmountText;
		CurrencyCode = item.CurrencyCode;
		LegacyPaidItem = item.LegacyPaidItem;
		DescriptionAlias = item.DescriptionAlias;
		ButtonTextAlias = item.ButtonTextAlias;
		IconName = item.IconName;
		SpendAfterUse = item.SpendAfterUse;
		HasAuthoredLevel = item.HasAuthoredLevel;
		UpgradeTemplateName = item.UpgradeTemplateName;
		isEnabledByDefault = item.isEnabledByDefault;
		Index = item.Index;
		ItemId = item.ItemId;
		ItemLevel = item.ItemLevel;
		HiddenFlag = item.HiddenFlag;
		IsShopVisible = item.IsShopVisible;
		defaultShopVisible = item.defaultShopVisible;
		Milestone = item.Milestone;
		SilentReceive = item.SilentReceive;
		UpgradeLevel = item.UpgradeLevel;
		DeliveryTime = item.DeliveryTime;
		CoinPrice = item.CoinPrice;
		GemPrice = item.GemPrice;
		DeliveryGemPrice = item.DeliveryGemPrice;
		DeliveryCoinPrice = item.DeliveryCoinPrice;
		IsPaid = item.IsPaid;
		ReceiveGold = item.ReceiveGold;
		ReceiveBonus = item.ReceiveBonus;
		ParentItem = item.ParentItem;
		IgnoreInventoryEnchantments = item.IgnoreInventoryEnchantments;
		isNew = item.isNew;
		ItemAttributes = new Attributes(item.ItemAttributes);
		InnatePerks = new List<PerkInfoItem>(item.InnatePerks);
		LocalUpgrades = new List<UpgradeData>(item.LocalUpgrades);
		DefaultEnchantmentPreviews = new List<PerkInfoItem>(item.DefaultEnchantmentPreviews);
		ParsedPerks = new List<PerkInfoItem>(item.ParsedPerks);
		DefaultEnchantments = new List<PerkStruct>(item.DefaultEnchantments);
	}

	public string GetMarketId()
	{
		return marketId;
	}

	public void set_MarketID(string value)
	{
		marketId = value;
	}

	public bool GetIsConsumable()
	{
		return isConsumable;
	}

	public bool GetIsNew()
	{
		return isNew;
	}

	public void SetIsNew(bool value)
	{
		if (!value || SilentReceive == 0)
		{
			isNew = value;
		}
	}

	public void RandomizeObscuredVars()
	{
		CurrencyValue.RandomizeCryptoKey();
		LotteryPrice.RandomizeCryptoKey();
		CoinPrice.RandomizeCryptoKey();
		GemPrice.RandomizeCryptoKey();
		DeliveryGemPrice.RandomizeCryptoKey();
		DeliveryCoinPrice.RandomizeCryptoKey();
		ReceiveGold.RandomizeCryptoKey();
		ReceiveBonus.RandomizeCryptoKey();
		LocalUpgrades.ForEach((UpgradeData upgrade) =>
		{
			upgrade.RandomizeObscuredVars();
		});
	}

	private long GetPrice()
	{
		return (ObscuredLong)((!HasGemPrice()) ? CoinPrice : GemPrice);
	}

	private long GetEffectivePrice()
	{
		return (!HasGemPrice()) ? GetCoinPrice() : GetGemPrice();
	}

	private long GetScaledCoinPrice()
	{
		return (ObscuredLong)(CoinPrice) * (long)GameUtils.SellPriceFactor;
	}

	public long GetCoinPrice()
	{
		return (ObscuredLong)(CoinPrice);
	}

	public long GetGemPrice()
	{
		return (ObscuredLong)(GemPrice);
	}

	private int GetLotteryPrice()
	{
		return (int)((float)(ObscuredInt)(LotteryPrice) * GetLotteryPriceMultiplier());
	}

	public bool HasGemPrice()
	{
		return 0 < (ObscuredLong)(GemPrice);
	}

	public bool HasCoinPrice()
	{
		return 0 < (ObscuredLong)(CoinPrice);
	}

	public bool HasLotteryPrice()
	{
		return 0 < (ObscuredInt)(LotteryPrice);
	}

	public void RestoreShopVisibility()
	{
		IsShopVisible = defaultShopVisible;
	}

	public bool IsHidden()
	{
		return HiddenFlag > 0;
	}

	public bool IsUpgradeVariant()
	{
		return (ParentItem != null) ? true : false;
	}

	public virtual ItemInfo Clone()
	{
		ItemInfo clone = new ItemInfo(this);
		clone.DefaultEnchantments.Clear();
		return clone;
	}

	private void ReadCombatClassification(XmlNode node)
	{
		if (!node.Attributes["Type"].Empty())
		{
			Type = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		}
		if (!node.Attributes["SubType"].Empty())
		{
			SubType = node.Attributes["SubType"].GetStringOrDefault(string.Empty);
		}
		if (!node.Attributes["TacticSubtype"].Empty())
		{
			TacticSubtype = node.Attributes["TacticSubtype"].GetStringOrDefault(string.Empty);
		}
	}

	private void ParseNode(XmlNode node)
	{
		if (!node.Attributes["Name"].Empty())
		{
			Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		}
		if (!node.Attributes["PackLabel"].Empty())
		{
			GroupId = node.Attributes["PackLabel"].GetStringOrDefault(string.Empty);
		}
		if (!node.Attributes["GroupID"].Empty())
		{
			GroupId = node.Attributes["GroupID"].GetStringOrDefault(string.Empty);
		}
		if (!node.Attributes["Image"].Empty())
		{
			FileName = node.Attributes["Image"].GetStringOrDefault(string.Empty);
		}
		if (!node.Attributes["Model"].Empty())
		{
			ModelFileName = node.Attributes["Model"].GetStringOrDefault(string.Empty);
		}
		ReadCombatClassification(node);
		if (!node.Attributes["Text"].Empty())
		{
			DescriptionAlias = node.Attributes["Text"].GetStringOrDefault(string.Empty);
		}
		if (!node.Attributes["TextButton"].Empty())
		{
			ButtonTextAlias = node.Attributes["TextButton"].GetStringOrDefault(string.Empty);
		}
		if (!node.Attributes["Price"].Empty())
		{
			CoinPrice = (ObscuredLong)(node.Attributes["Price"].ParseLong(0L));
		}
		if (!node.Attributes["PriceDigits"].Empty())
		{
			priceDigits = node.Attributes["PriceDigits"].ParseInt();
		}
		if (!node.Attributes["BonusPrice"].Empty())
		{
			GemPrice = (ObscuredLong)(node.Attributes["BonusPrice"].ParseLong(0L));
		}
		if (!node.Attributes["LotteryPrice"].Empty())
		{
			LotteryPrice = (ObscuredInt)(node.Attributes["LotteryPrice"].ParseInt());
		}
		if (!node.Attributes["SilentRecieve"].Empty())
		{
			SilentReceive = node.Attributes["SilentRecieve"].ParseInt();
		}
		if (string.IsNullOrEmpty(marketId) && (SystemProperties.IsEditorPlatform() || SystemProperties.IsIosPlatform()) && !node.Attributes["IphoneID"].Empty())
		{
			marketId = node.Attributes["IphoneID"].GetStringOrDefault(string.Empty);
		}
		if (string.IsNullOrEmpty(marketId) && !AssemblyController.GetMarket().GetIsChinaMarket() && SystemProperties.IsAndroidPlatform() && !node.Attributes["AndroidID"].Empty())
		{
			marketId = node.Attributes["AndroidID"].GetStringOrDefault(string.Empty);
		}
		if (string.IsNullOrEmpty(marketId) && AssemblyController.GetMarket().GetIsChinaMarket() && SystemProperties.IsAndroidPlatform() && !node.Attributes["ChineseID"].Empty())
		{
			marketId = node.Attributes["ChineseID"].GetStringOrDefault(string.Empty);
		}
		if (string.IsNullOrEmpty(marketId) && SystemProperties.IsMetroArmPlatform() && !node.Attributes["WinPhoneID"].Empty())
		{
			marketId = node.Attributes["WinPhoneID"].GetStringOrDefault(string.Empty);
		}
		if (!node.Attributes["ConsumableProduct"].Empty())
		{
			isConsumable = node.Attributes["ConsumableProduct"].ParseBool();
		}
		if (!AssemblyController.GetMarket().GetIsChinaMarket() && !node.Attributes["RealPrice"].Empty())
		{
			LocalizedPriceString = node.Attributes["RealPrice"].GetStringOrDefault(string.Empty);
			PriceAmountText = LocalizedPriceString.Substring(1);
			CurrencyCode = "USD";
		}
		if (AssemblyController.GetMarket().GetIsChinaMarket() && !node.Attributes["RealPriceChina"].Empty())
		{
			LocalizedPriceString = node.Attributes["RealPriceChina"].GetStringOrDefault(string.Empty);
			PriceAmountText = LocalizedPriceString.Substring(2);
			CurrencyCode = "CNY";
		}
		if (!node.Attributes["isPaid"].Empty())
		{
			IsPaid = node.Attributes["isPaid"].ParseBool();
		}
		if (!node.Attributes["RecieveGold"].Empty())
		{
			ReceiveGold = (ObscuredLong)(node.Attributes["RecieveGold"].ParseLong(0L));
		}
		if (!node.Attributes["RecieveBonus"].Empty())
		{
			ReceiveBonus = (ObscuredLong)(node.Attributes["RecieveBonus"].ParseLong(0L));
		}
		if (!node.Attributes["CurrencyName"].Empty())
		{
			CurrencyName = node.Attributes["CurrencyName"].GetStringOrDefault(string.Empty);
		}
		if (!node.Attributes["CurrencyValue"].Empty())
		{
			CurrencyValue = (ObscuredInt)(node.Attributes["CurrencyValue"].ParseInt());
		}
		if (!node.Attributes["ShopHide"].Empty())
		{
			bool flag = node.Attributes["ShopHide"].ParseBool();
			IsShopVisible = !flag;
			defaultShopVisible = IsShopVisible;
		}
		if (!node.Attributes["Hidden"].Empty())
		{
			HiddenFlag = node.Attributes["Hidden"].ParseInt();
		}
		HasAuthoredLevel = !node.Attributes["Level"].Empty();
		if (!node.Attributes["Level"].Empty())
		{
			ItemLevel = node.Attributes["Level"].ParseInt();
		}
		if (!node.Attributes["UpgradeLevel"].Empty())
		{
			UpgradeLevel = node.Attributes["UpgradeLevel"].ParseInt();
		}
		if (!node.Attributes["SpendAfterUse"].Empty())
		{
			SpendAfterUse = node.Attributes["SpendAfterUse"].ParseBool();
		}
		if (!node.Attributes["DeliveryTime"].Empty())
		{
			DeliveryTime = node.Attributes["DeliveryTime"].ParseLong(0L);
		}
		if (!node.Attributes["DeliveryDescription"].Empty())
		{
			hasDeliveryDescription = node.Attributes["DeliveryDescription"].ParseBool();
		}
		if (!node.Attributes["BonusDeliveryPrice"].Empty())
		{
			DeliveryGemPrice = (ObscuredLong)(node.Attributes["BonusDeliveryPrice"].ParseLong(0L));
		}
		if (!node.Attributes["Milestone"].Empty())
		{
			Milestone = node.Attributes["Milestone"].ParseInt();
		}
		XmlNode xmlNode = node["Perks"];
		if (xmlNode != null)
		{
			ParseInnatePerks(xmlNode);
		}
		XmlNode xmlNode2 = node["Enchantments"];
		if (xmlNode2 != null)
		{
			ParseEnchantmentPreviews(xmlNode2);
			ParseDefaultEnchantments(xmlNode2);
		}
		if (!node.Attributes["AddPercent"].Empty())
		{
			addPercent = node.Attributes["AddPercent"].ParseInt();
		}
		if (!node.Attributes["Icon"].Empty())
		{
			IconName = node.Attributes["Icon"].GetStringOrDefault(string.Empty);
		}
		if (!node.Attributes["PaidItem"].Empty())
		{
			LegacyPaidItem = node.Attributes["PaidItem"].GetStringOrDefault(string.Empty);
		}
		List<WarriorAttribute> warriorAttributes = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item in warriorAttributes)
		{
			XmlAttribute attributeNode = node.Attributes[item.get_Name()];
			if (!attributeNode.Empty())
			{
				ItemAttributes.Set(item.get_Name(), attributeNode.ParseInt());
			}
		}
		XmlNode xmlNode3 = node["Upgrades"];
		if (xmlNode3 != null)
		{
			UpgradeTemplateName = xmlNode3.Attributes["Template"].GetStringOrDefault(string.Empty);
		}
	}

	private float GetLotteryPriceMultiplier()
	{
		return 1f;
	}

	public void MergeWithItem(ItemInfo item)
	{
		if (!string.IsNullOrEmpty(item.Type))
		{
			Type = item.Type;
		}
		if (!string.IsNullOrEmpty(item.SubType))
		{
			SubType = item.SubType;
		}
		if (!string.IsNullOrEmpty(item.TacticSubtype))
		{
			TacticSubtype = item.TacticSubtype;
		}
	}

	public void ParseInnatePerks(XmlNode node)
	{
		InnatePerks.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			PerkInfoItem perk = ParsePerk(childNode);
			if (perk != null)
			{
				InnatePerks.Add(perk);
			}
		}
	}

	public static PerkInfoItem ParsePerk(XmlNode node)
	{
		string perkName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		PerkInfoItem perk = GameUtils.PerkItemList.FindBasePerk(perkName);
		if (perk != null)
		{
			if (node["Set"] != null || node["RatingEvaluation"] != null)
			{
				perk = perk.Clone(node["Set"], node["RatingEvaluation"]);
				string text = node.Attributes["Description"].GetStringOrDefault(string.Empty);
				if (text != null && !text.Equals(string.Empty))
				{
					perk.DescriptionKey = text;
				}
			}
			return perk;
		}
		return null;
	}

	public void ParseEnchantmentPreviews(XmlNode node)
	{
		DefaultEnchantmentPreviews.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			PerkInfoItem perk = ParsePerk(childNode);
			if (perk != null)
			{
				DefaultEnchantmentPreviews.Add(perk);
			}
		}
	}

	public void SetParsedPerks(XmlNode node)
	{
		RemoveParsedPerks();
		if (node == null)
		{
			return;
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			PerkInfoItem perk = ParsePerk(childNode);
			if (perk != null)
			{
				ParsedPerks.Add(perk);
				InnatePerks.Add(perk);
			}
		}
	}

	public void ParseDefaultEnchantments(XmlNode node)
	{
		ClearDefaultEnchantments();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			PerkStruct item = new PerkStruct(childNode);
			DefaultEnchantments.Add(item);
		}
	}

	public void ClearDefaultEnchantments()
	{
		DefaultEnchantments.Clear();
	}

	private void DenominateReservedPrices(int digits)
	{
	}

	private void DenominateReservedValues(int digits)
	{
	}

	public void AddLocalUpgrade(UpgradeData upgrade)
	{
		UpgradeData item = CompleteUpgradeData(upgrade);
		LocalUpgrades.Add(item);
	}

	public int GetMaxLocalUpgradeLevel()
	{
		int maxLevel = int.MinValue;
		LocalUpgrades.ForEach((UpgradeData upgrade) =>
		{
			if (upgrade.Values.UpgradeLevel > maxLevel)
			{
				maxLevel = upgrade.Values.UpgradeLevel;
			}
		});
		return maxLevel;
	}

	public List<UpgradeData> GetUpgrades(bool onlyAboveCurrent = false, int maxItemLevel = int.MaxValue)
	{
		List<UpgradeData> list = new List<UpgradeData>();
		List<UpgradeData> list2 = new List<UpgradeData>();
		int num = GetMaxLocalUpgradeLevel();
		list2.AddRange(LocalUpgrades);
		UpgradeDataContainer upgradeContainer = ListSF.GetItems().GetUpgradeDataContainerByName(UpgradeTemplateName);
		if (upgradeContainer != null)
		{
			foreach (UpgradeData item in upgradeContainer.Upgrades)
			{
				if (item.Values.UpgradeLevel > num)
				{
					list2.Add(item);
				}
			}
		}
		list2.Sort();
		foreach (UpgradeData item2 in list2)
		{
			if ((!onlyAboveCurrent || item2.Values.UpgradeLevel > UpgradeLevel) && item2.Values.Level <= maxItemLevel)
			{
				list.Add(item2);
			}
		}
		return list;
	}

	public void ApplyUpgrade(UpgradeData upgrade)
	{
		List<WarriorAttribute> warriorAttributes = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item in warriorAttributes)
		{
			int attributeValue = 0;
			if (upgrade.Values.Attributes.Get(item.get_Name(), ref attributeValue))
			{
				ItemAttributes.Set(item.get_Name(), attributeValue);
			}
		}
		if (upgrade.HasValues.HasBonusDeliveryPrice)
		{
			DeliveryGemPrice = upgrade.Values.BonusDeliveryPrice;
		}
		if (upgrade.HasValues.HasBonusPrice)
		{
			GemPrice = upgrade.Values.BonusPrice;
		}
		if (upgrade.HasValues.HasDeliveryTime)
		{
			DeliveryTime = upgrade.Values.DeliveryTime;
		}
		if (upgrade.HasValues.Level)
		{
			ItemLevel = upgrade.Values.Level;
		}
		if (upgrade.HasValues.HasMilestone)
		{
			Milestone = upgrade.Values.Milestone;
		}
		if (upgrade.HasValues.HasPrice)
		{
			CoinPrice = upgrade.Values.Price;
		}
		if (upgrade.HasValues.HasUpgradeLevel)
		{
			UpgradeLevel = upgrade.Values.UpgradeLevel;
		}
	}

	public ItemInfo GetUpgradeItemByUpgradeLevel(int upgradeLevel)
	{
		List<UpgradeData> list = GetUpgrades();
		foreach (UpgradeData item in list)
		{
			if (item.Values.UpgradeLevel == upgradeLevel)
			{
				return CreateUpgradedItem(item);
			}
		}
		return null;
	}

	public ItemInfo GetUpgradeItemAtOrAboveUpgradeLevel(int upgradeLevel)
	{
		List<UpgradeData> list = GetUpgrades();
		foreach (UpgradeData item in list)
		{
			if (item.Values.UpgradeLevel >= upgradeLevel)
			{
				return CreateUpgradedItem(item);
			}
		}
		return null;
	}

	public ItemInfo GetUpdateItemByLevel(int level, bool usePreviousUpgrade = true)
	{
		UpgradeData exactMatch = null;
		UpgradeData previousUpgrade = null;
		UpgradeData currentUpgrade = null;
		bool flag = false;
		List<UpgradeData> list = GetUpgrades();
		foreach (UpgradeData item in list)
		{
			int itemLevel = item.Values.Level;
			if (!flag && itemLevel == level)
			{
				exactMatch = item;
				flag = true;
			}
			if (itemLevel > level)
			{
				previousUpgrade = currentUpgrade;
				break;
			}
			currentUpgrade = item;
		}
		if (exactMatch == null && previousUpgrade == null)
		{
			return null;
		}
		UpgradeData selectedUpgrade = ((!usePreviousUpgrade) ? exactMatch : previousUpgrade);
		return CreateUpgradedItem(selectedUpgrade);
	}

	public ItemInfo GetUpgradeItemByIndex(int index)
	{
		List<UpgradeData> list = GetUpgrades();
		if (0 <= index && index < list.Count)
		{
			return CreateUpgradedItem(list[index]);
		}
		GameLog.Error("ItemInfo.getUpdateItemByIndex wrong index: {0}", index);
		return null;
	}

	public UpgradeIndexItem GetUpgradeIndexItem(int maxItemLevel, int upgradeLevel)
	{
		UpgradeIndexItem indexItem = new UpgradeIndexItem();
		int num = 0;
		if (ParentItem != null)
		{
			List<UpgradeData> list = ParentItem.GetUpgrades();
			foreach (UpgradeData item in list)
			{
				UpgradeData.UpgradeValues values = item.Values;
				if (values.Level == ItemLevel && values.UpgradeLevel < UpgradeLevel && values.UpgradeLevel > ParentItem.UpgradeLevel)
				{
					num++;
				}
			}
			if (ParentItem.ItemLevel == ItemLevel)
			{
				num++;
			}
		}
		if (num == 0)
		{
			indexItem.Type = UpgradeIndexItem.UpgradeIndexType.UPGRADE_INDEX_MILESTONE;
			indexItem.Index = ItemLevel;
		}
		else
		{
			indexItem.Index = num;
		}
		return indexItem;
	}

	public void FindNextUpgradeItems(int maxItemLevel, int upgradeLevel, ref ItemInfo currentItem, ref ItemInfo nextItem)
	{
		List<UpgradeData> list = GetUpgrades();
		List<UpgradeData> list2 = new List<UpgradeData>();
		UpgradeData currentUpgrade = null;
		UpgradeData nextMilestoneUpgrade = null;
		UpgradeData nextRegularUpgrade = null;
		float num = GameUtils.OutdateLevelTable.GetValue(Type);
		int num2 = upgradeLevel / 100;
		foreach (UpgradeData item in list)
		{
			int candidateLevel = item.Values.UpgradeLevel;
			if (candidateLevel == upgradeLevel)
			{
				currentUpgrade = item;
			}
			if (item.Values.Level <= maxItemLevel && candidateLevel > upgradeLevel)
			{
				if (item.Values.Milestone > 0 && (float)item.Values.Level >= (float)num2 + num && (nextMilestoneUpgrade == null || nextMilestoneUpgrade.Values.UpgradeLevel < candidateLevel))
				{
					nextMilestoneUpgrade = item;
				}
				if (item.Values.Milestone <= 0 && (nextRegularUpgrade == null || nextRegularUpgrade.Values.UpgradeLevel > candidateLevel))
				{
					nextRegularUpgrade = item;
				}
			}
		}
		if (currentUpgrade != null)
		{
			currentItem = CreateUpgradedItem(currentUpgrade);
		}
		else
		{
			currentItem = null;
		}
		if (nextMilestoneUpgrade != null)
		{
			nextItem = CreateUpgradedItem(nextMilestoneUpgrade);
		}
		else if (nextRegularUpgrade != null)
		{
			nextItem = CreateUpgradedItem(nextRegularUpgrade);
		}
		else
		{
			nextItem = null;
		}
	}

	public static void DenominateItems(int digits = 0)
	{
		List<ItemInfo> list = ListSF.GetItems().GetAllItems();
		foreach (ItemInfo item in list)
		{
			item.CoinPrice = (ObscuredLong)(GameUtils.GetDenominatedValue((ObscuredLong)(item.CoinPrice), digits));
			item.DenominateReservedPrices(digits);
			item.DenominateReservedValues(digits);
			List<UpgradeData> localUpgrades = item.LocalUpgrades;
			foreach (UpgradeData item2 in localUpgrades)
			{
				item2.Values.Price = (ObscuredLong)(GameUtils.GetDenominatedValue((ObscuredLong)(item2.Values.Price), digits));
			}
			if (item.Type.Equals("RealMoneyItem"))
			{
				item.ReceiveGold = (ObscuredLong)(GameUtils.GetDenominatedValue((ObscuredLong)(item.ReceiveGold), digits));
			}
		}
		foreach (UpgradeDataContainer item3 in ListSF.GetItems().GetUpgradeContainers())
		{
			foreach (UpgradeData item4 in item3.Upgrades)
			{
				item4.Values.Price = (ObscuredLong)(GameUtils.GetDenominatedValue((ObscuredLong)(item4.Values.Price), digits));
			}
		}
		ListSF.GetRoster().GetInventory().RefreshUpgradeStates();
	}

	public ItemInfo CreateUpgradedItem(UpgradeData upgrade)
	{
		if (upgrade == null)
		{
			return null;
		}
		ItemInfo upgradedItem = Clone();
		upgradedItem.ParentItem = this;
		upgradedItem.ApplyUpgrade(upgrade);
		return upgradedItem;
	}

	private void RemoveParsedPerks()
	{
		ParsedPerks.ForEach((PerkInfoItem perk) =>
		{
			RemoveInnatePerk(perk);
		});
		ParsedPerks.Clear();
	}

	private void RemoveInnatePerk(PerkInfoItem perk)
	{
		InnatePerks.Remove(perk);
	}

	private void Init()
	{
		CoinPrice = (ObscuredLong)(0L);
		GemPrice = (ObscuredLong)(0L);
		LotteryPrice = (ObscuredInt)(0);
		ReceiveGold = (ObscuredLong)(0L);
		ReceiveBonus = (ObscuredLong)(0L);
		CurrencyName = string.Empty;
		CurrencyValue = (ObscuredInt)(0);
		IsShopVisible = true;
		HiddenFlag = 0;
		ItemLevel = 0;
		UpgradeLevel = 0;
		IsUpgradePurchase = false;
		SpendAfterUse = false;
		MissingCoins = 0L;
		MissingGems = 0L;
		IsPaid = false;
		DeliveryTime = 0L;
		hasDeliveryDescription = false;
		DeliveryGemPrice = (ObscuredLong)(0L);
		DeliveryCoinPrice = (ObscuredLong)(0L);
		Milestone = 0;
		SilentReceive = 0;
		ParentItem = null;
		addPercent = 0;
		priceDigits = 0;
		unusedFlag = false;
		IconName = string.Empty;
		IgnoreInventoryEnchantments = false;
		LegacyPaidItem = "None";
		isEnabledByDefault = true;
	}

	private UpgradeData CompleteUpgradeData(UpgradeData upgrade)
	{
		UpgradeData completedUpgrade = new UpgradeData(upgrade);
		List<WarriorAttribute> warriorAttributes = GameUtils.WarriorAttributeList.AttributeList;
		foreach (WarriorAttribute item in warriorAttributes)
		{
			int attributeValue = 0;
			if (ItemAttributes.Get(item.get_Name(), ref attributeValue) && !completedUpgrade.Values.Attributes.Get(item.get_Name(), ref attributeValue))
			{
				ItemAttributes.Get(item.get_Name(), ref attributeValue);
				completedUpgrade.Values.Attributes.Set(item.get_Name(), attributeValue);
			}
		}
		if (!completedUpgrade.HasValues.HasBonusDeliveryPrice)
		{
			completedUpgrade.Values.BonusDeliveryPrice = DeliveryGemPrice;
		}
		if (!completedUpgrade.HasValues.HasBonusPrice)
		{
			completedUpgrade.Values.BonusPrice = GemPrice;
		}
		if (!completedUpgrade.HasValues.HasDeliveryTime)
		{
			completedUpgrade.Values.DeliveryTime = DeliveryTime;
		}
		if (!completedUpgrade.HasValues.Level)
		{
			completedUpgrade.Values.Level = ItemLevel;
		}
		if (!completedUpgrade.HasValues.HasMilestone)
		{
			completedUpgrade.Values.Milestone = Milestone;
		}
		if (!completedUpgrade.HasValues.HasPrice)
		{
			completedUpgrade.Values.Price = CoinPrice;
		}
		if (!completedUpgrade.HasValues.HasUpgradeLevel)
		{
			completedUpgrade.Values.UpgradeLevel = UpgradeLevel;
		}
		return completedUpgrade;
	}

	public void ApplyProductMetadata(ProductMetadata metadata)
	{
		LocalizedPriceString = metadata.localizedPriceString;
		PriceAmountText = metadata.localizedPrice.ToString();
		CurrencyCode = metadata.isoCurrencyCode;
	}

	public void SortLocalUpgrades()
	{
		LocalUpgrades.Sort();
		int index = 0;
		LocalUpgrades.ForEach((UpgradeData upgrade) =>
		{
			upgrade.UpgradeIndex = index;
			index++;
		});
	}
}
