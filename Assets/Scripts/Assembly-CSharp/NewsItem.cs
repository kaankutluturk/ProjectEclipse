using System.Collections.Generic;

public class NewsItem
{
	public string Name = string.Empty;

	public string Url = string.Empty;

	public string ImageUrl = string.Empty;

	public string LocalImagePath = string.Empty;

	public string Title = string.Empty;

	public string RedirectShop = string.Empty;

	public string SpenderTypeId = string.Empty;

	public bool IsActive;

	public bool WasShown;

	public bool IsImageReady;

	public bool GoShop;

	public int Id;

	public long EndDate;

	public List<NewsButton> Buttons = new List<NewsButton>();

	public NewsItem()
	{
	}

	public NewsItem(NewsItem AOMLCBHAJJH)
	{
		Name = AOMLCBHAJJH.Name;
		Url = AOMLCBHAJJH.Url;
		ImageUrl = AOMLCBHAJJH.ImageUrl;
		LocalImagePath = AOMLCBHAJJH.LocalImagePath;
		Title = AOMLCBHAJJH.Title;
		RedirectShop = AOMLCBHAJJH.RedirectShop;
		SpenderTypeId = AOMLCBHAJJH.SpenderTypeId;
		IsActive = AOMLCBHAJJH.IsActive;
		WasShown = AOMLCBHAJJH.WasShown;
		IsImageReady = AOMLCBHAJJH.IsImageReady;
		GoShop = AOMLCBHAJJH.GoShop;
		Id = AOMLCBHAJJH.Id;
		EndDate = AOMLCBHAJJH.EndDate;
		foreach (NewsButton item in AOMLCBHAJJH.Buttons)
		{
			Buttons.Add(new NewsButton(item));
		}
	}

	public bool HasRedirectTarget(string value)
	{
		bool result = false;
		if (RedirectShop == value)
		{
			result = true;
		}
		else
		{
			foreach (NewsButton item in Buttons)
			{
				if (item.RedirectShop == value)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}
}
