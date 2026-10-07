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

	public NewsItem(NewsItem source)
	{
		Name = source.Name;
		Url = source.Url;
		ImageUrl = source.ImageUrl;
		LocalImagePath = source.LocalImagePath;
		Title = source.Title;
		RedirectShop = source.RedirectShop;
		SpenderTypeId = source.SpenderTypeId;
		IsActive = source.IsActive;
		WasShown = source.WasShown;
		IsImageReady = source.IsImageReady;
		GoShop = source.GoShop;
		Id = source.Id;
		EndDate = source.EndDate;
		foreach (NewsButton item in source.Buttons)
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
