using System.Collections.Generic;

public class News
{
	private List<NewsItem> _items = new List<NewsItem>();

	public List<NewsItem> Items
	{
		get
		{
			return GetItems();
		}
	}

	public List<NewsItem> GetItems()
	{
		return _items;
	}

	public void Reset()
	{
		_items.Clear();
	}

	public void AddOrReplaceItem(string name, string url, string imageUrl, int id, bool isActive, long endDate, List<NewsButton> buttons, string title = "", bool goShop = false, string redirectShop = "", string spenderTypeId = "")
	{
		NewsItem existingItem = _items.Find((NewsItem entry) => entry.Id == id);
		if (existingItem != null)
		{
			_items.Remove(existingItem);
		}
		existingItem = new NewsItem();
		existingItem.Title = title;
		existingItem.Name = name;
		existingItem.Url = url;
		existingItem.ImageUrl = imageUrl;
		existingItem.Id = id;
		existingItem.IsActive = isActive;
		existingItem.EndDate = endDate;
		existingItem.WasShown = false;
		existingItem.IsImageReady = false;
		existingItem.GoShop = goShop;
		existingItem.RedirectShop = redirectShop;
		existingItem.Buttons = buttons;
		existingItem.SpenderTypeId = spenderTypeId;
		GetItems().Add(existingItem);
	}
}
