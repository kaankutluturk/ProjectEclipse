using System.Collections.Generic;
using System.Xml;

public class ItemSet
{
	public string Name;

	public string Title;

	public string Text;

	public string Brief;

	public List<ItemSetItem> Items;

	// Eclipse: the combo enchantment this set grants (list.xml DefaultComboPerk).
	public string DefaultComboPerk;

	public ItemSet(XmlNode node)
	{
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		Title = node.Attributes["Title"].GetStringOrDefault(string.Empty);
		Text = node.Attributes["Text"].GetStringOrDefault(string.Empty);
		Brief = node.Attributes["Brief"].GetStringOrDefault(string.Empty);
		DefaultComboPerk = node.Attributes["DefaultComboPerk"].GetStringOrDefault(string.Empty);
		Items = new List<ItemSetItem>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			ItemSetItem item = new ItemSetItem(childNode);
			Items.Add(item);
		}
	}

	public ItemSetItem GetItemByName(string itemName)
	{
		foreach (ItemSetItem item in Items)
		{
			if (item.Name.Equals(itemName))
			{
				return item;
			}
		}
		return null;
	}

	public bool IsComplete()
	{
		foreach (ItemSetItem item in Items)
		{
			ItemInfo itemInfo = item.Item;
			if (itemInfo == null)
			{
				return false;
			}
			UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(itemInfo);
			if (userItem == null)
			{
				return false;
			}
		}
		return true;
	}
}
