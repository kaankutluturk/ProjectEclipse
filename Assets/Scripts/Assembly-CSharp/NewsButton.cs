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

	public NewsButton(NewsButton source)
	{
		LabelAliasName = source.LabelAliasName;
		Color = source.Color;
		Url = source.Url;
		RedirectShop = source.RedirectShop;
		GoShop = source.GoShop;
		BuyItem = source.BuyItem;
	}
}
