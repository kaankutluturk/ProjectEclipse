public class NewsButton
{
	public string LabelAliasName = string.Empty;

	public LabelButton.ButtonColor Color = LabelButton.ButtonColor.BUTTON_WHITE;

	public string Url = string.Empty;

	public string RedirectShop = string.Empty;

	public bool GoShop;

	public bool BuyItem;

	public NewsButton()
	{
	}

	public NewsButton(NewsButton AOMLCBHAJJH)
	{
		LabelAliasName = AOMLCBHAJJH.LabelAliasName;
		Color = AOMLCBHAJJH.Color;
		Url = AOMLCBHAJJH.Url;
		RedirectShop = AOMLCBHAJJH.RedirectShop;
		GoShop = AOMLCBHAJJH.GoShop;
		BuyItem = AOMLCBHAJJH.BuyItem;
	}
}
