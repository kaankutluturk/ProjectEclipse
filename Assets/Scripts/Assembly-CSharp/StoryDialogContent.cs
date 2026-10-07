using UnityEngine;

public class StoryDialogContent
{
	public enum ContentType
	{
		CONTENT_TYPE_REGULAR = 0,
		CONTENT_TYPE_PRICELINE = 1
	}

	public string Text = string.Empty;

	public string ButtonText = string.Empty;

	public string FontName = string.Empty;

	public string ItemName = string.Empty;

	public string EnchantmentName = string.Empty;

	public int Id;

	public long Timer;

	public bool CheckTimer;

	public UserItem OwnedItem;

	public RecipeItemInfo Recipe;

	public TextTimer ItemTimer;

	public ContentType Type;

	public Color FontColor = Constants.DialogTextColor;

	public StoryDialogContent(string _text = "", string buttonText = "", string itemName = "", string enchantmentName = "", int timer = 0, int _id = -1, UserItem ownedItem = null, TextTimer itemTimer = null, ContentType _type = ContentType.CONTENT_TYPE_REGULAR, RecipeItemInfo recipe = null)
	{
		Text = _text;
		ButtonText = buttonText;
		ItemName = itemName;
		EnchantmentName = enchantmentName;
		Id = _id;
		Timer = timer;
		OwnedItem = ownedItem;
		Recipe = recipe;
		ItemTimer = itemTimer;
		Type = _type;
	}

	public StoryDialogContent(StoryDialogContent source)
	{
		Text = source.Text;
		ButtonText = source.ButtonText;
		ItemName = source.ItemName;
		EnchantmentName = source.EnchantmentName;
		Id = source.Id;
		Timer = source.Timer;
		OwnedItem = source.OwnedItem;
		Recipe = source.Recipe;
		ItemTimer = source.ItemTimer;
		Type = source.Type;
	}

	public bool RefreshItemTimer()
	{
		if (ItemName != string.Empty || EnchantmentName != string.Empty)
		{
			UserItem userItem = ListSF.GetUserItem(ItemName);
			if (userItem == null)
			{
				Timer = 0L;
			}
			else
			{
				Timer = GameUtils.GetLeftTime(userItem.GetDeliveryTimestamp());
			}
			CheckTimer = true;
			OwnedItem = userItem;
			if (Timer == 0)
			{
				return false;
			}
		}
		return true;
	}
}
