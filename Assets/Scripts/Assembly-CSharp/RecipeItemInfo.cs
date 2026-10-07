using System;
using System.Xml;
using CodeStage.AntiCheat.ObscuredTypes;
using Nekki.Utils;

public class RecipeItemInfo : ItemInfo
{
	private Recipe recipeData;
	private RecipePrice recipePriceInfo;
	private UserItem userItemData;
	private uint itemLevelRaw;
	private uint playerLevelRaw;
	private long _RecipeDeliveryTime;

	public Recipe SourceRecipe => GetRecipe();
	public RecipePrice RecipePriceInfo => GetPrice();
	public UserItem TargetUserItem => GetUserItem();
	public long DeliveryEndTime => GetDeliveryEndTime();

	public int ItemLevel => (int)itemLevelRaw;
	public int PlayerLevel => (int)playerLevelRaw;
	public long RecipeDeliveryTime => _RecipeDeliveryTime;
	public long TimeLeft => Math.Max(0L, GetDeliveryEndTime() - CurrentTimeSeconds());
	public bool IsStillInOrder => _RecipeDeliveryTime > 0L && TimeLeft > 0L;
	public bool IsReadyForDelivery(long time) => _RecipeDeliveryTime > 0L &&
		(Eclipse.Modding.ModPolicies.CompletePending("forge") || _RecipeDeliveryTime <= time);
	public string ItemAndRecipeInfo => Name;

	public RecipeItemInfo(Recipe recipe, UserItem userItem, RecipePrice price)
	{
		recipeData = recipe;
		userItemData = userItem;
		recipePriceInfo = price;
		Type = "Recipe";
		int deliverySeconds = Eclipse.Modding.ModPolicies.DeliverySeconds("forge", price?.DeliveryTime ?? 0);
		if (deliverySeconds > 0)
		{
			_RecipeDeliveryTime = CurrentTimeSeconds() + deliverySeconds;
			DeliveryGemPrice = price?.BonusDeliveryPrice ?? (ObscuredLong)0L;
		}
		else
		{
			_RecipeDeliveryTime = 0L;
			DeliveryGemPrice = (ObscuredLong)0L;
		}
		if (userItem != null)
		{
			ItemInfo info = userItem.GetDisplayInfo(false) ?? userItem.GetInfo();
			if (info != null) itemLevelRaw = (uint)Math.Max(0, info.ItemLevel);
		}
		Roster roster = ListSF.GetRoster();
		playerLevelRaw = (uint)Math.Max(0, roster == null ? 0 : roster.GetLevel());
		Name = (userItem == null ? string.Empty : userItem.get_Name()) + "|" +
			(recipe == null ? string.Empty : recipe.Name);
	}

	public RecipeItemInfo(XmlNode node, UserItem userItem)
	{
		userItemData = userItem;
		string recipeName = node?.Attributes?["Name"].GetStringOrDefault(string.Empty) ?? string.Empty;
		recipeData = ForgeManager.GetInstance().GetRecipeByName(recipeName);
		itemLevelRaw = node?.Attributes?["ItemLevel"].ParseUint() ?? 0u;
		_RecipeDeliveryTime = node?.Attributes?["DeliveryTime"].ParseLong(0L) ?? 0L;
		playerLevelRaw = node?.Attributes?["PlayerLevel"].ParseUint() ?? 0u;
		recipePriceInfo = recipeData?.GetPriceByItemLevel(userItem, (int)itemLevelRaw);
		Type = "Recipe";
		if (recipePriceInfo != null) DeliveryGemPrice = recipePriceInfo.BonusDeliveryPrice;
		Name = (userItem == null ? string.Empty : userItem.get_Name()) + "|" + recipeName;
	}

	private static long CurrentTimeSeconds()
	{
		long currentTime = ListSF.GetCurrentTime();
		return currentTime > 0L ? currentTime : GlobalTimer.get_GetTime();
	}

	public Recipe GetRecipe() => recipeData;
	public RecipePrice GetPrice() => recipePriceInfo;
	public UserItem GetUserItem() => userItemData;
	public long GetDeliveryEndTime() => Eclipse.Modding.ModPolicies.CompletePending("forge") ? 0L : _RecipeDeliveryTime;
}
