using System.Collections.Generic;
using System.Xml;

public class ItemSets
{
	private List<ItemSet> sets = new List<ItemSet>();

	public IReadOnlyList<ItemSet> Sets => sets.AsReadOnly();

	public void Parse(XmlNode node)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			ItemSet item = new ItemSet(childNode);
			sets.Add(item);
		}
	}

	public ItemSet FindSetByItemName(string OHCGEEEKEJH)
	{
		foreach (ItemSet item in sets)
		{
			ItemSetItem bADHNGONFNC = item.GetItemByName(OHCGEEEKEJH);
			if (bADHNGONFNC != null)
			{
				return item;
			}
		}
		return null;
	}

	public ItemSet GetSetByName(string JFGJBCGEGCN)
	{
		foreach (ItemSet item in sets)
		{
			if (item.Name.Equals(JFGJBCGEGCN))
			{
				return item;
			}
		}
		return null;
	}

	public ItemSet AddExternalSet(XmlNode node)
	{
		if (node == null) throw new System.ArgumentNullException("node");
		string name = node.Attributes?["Name"]?.Value ?? string.Empty;
		if (string.IsNullOrEmpty(name)) throw new System.InvalidOperationException("External item set requires a Name attribute.");
		if (GetSetByName(name) != null) throw new System.InvalidOperationException("Item set already exists: " + name);
		ItemSet itemSet = new ItemSet(node);
		if (itemSet.Items == null || itemSet.Items.Count == 0)
			throw new System.InvalidOperationException("External item set requires at least one member: " + name);
		sets.Add(itemSet);
		return itemSet;
	}

	public bool RemoveExternalSet(string name)
	{
		if (string.IsNullOrEmpty(name)) return false;
		ItemSet itemSet = GetSetByName(name);
		return itemSet != null && sets.Remove(itemSet);
	}
}
