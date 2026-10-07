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

	public StoryDialogContent(string _text = "", string ADDKGJGCBMB = "", string KLIDPJCCAME = "", string MJEBMLFLLHO = "", int KMFDBBKMLOO = 0, int _id = -1, UserItem NKBIOFJMONB = null, TextTimer OIHKOMFCFME = null, ContentType _type = ContentType.CONTENT_TYPE_REGULAR, RecipeItemInfo DMDLCMBKEHA = null)
	{
		Text = _text;
		ButtonText = ADDKGJGCBMB;
		ItemName = KLIDPJCCAME;
		EnchantmentName = MJEBMLFLLHO;
		Id = _id;
		Timer = KMFDBBKMLOO;
		OwnedItem = NKBIOFJMONB;
		Recipe = DMDLCMBKEHA;
		ItemTimer = OIHKOMFCFME;
		Type = _type;
	}

	public StoryDialogContent(StoryDialogContent NOLFMPDGCOC)
	{
		Text = NOLFMPDGCOC.Text;
		ButtonText = NOLFMPDGCOC.ButtonText;
		ItemName = NOLFMPDGCOC.ItemName;
		EnchantmentName = NOLFMPDGCOC.EnchantmentName;
		Id = NOLFMPDGCOC.Id;
		Timer = NOLFMPDGCOC.Timer;
		OwnedItem = NOLFMPDGCOC.OwnedItem;
		Recipe = NOLFMPDGCOC.Recipe;
		ItemTimer = NOLFMPDGCOC.ItemTimer;
		Type = NOLFMPDGCOC.Type;
	}

	public bool RefreshItemTimer()
	{
		if (ItemName != string.Empty || EnchantmentName != string.Empty)
		{
			UserItem dKCHDHMLKHN = ListSF.GetUserItem(ItemName);
			if (dKCHDHMLKHN == null)
			{
				Timer = 0L;
			}
			else
			{
				Timer = GameUtils.GetLeftTime(dKCHDHMLKHN.GetDeliveryTimestamp());
			}
			CheckTimer = true;
			OwnedItem = dKCHDHMLKHN;
			if (Timer == 0)
			{
				return false;
			}
		}
		return true;
	}
}
