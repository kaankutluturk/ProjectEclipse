using System.Xml;

public class RandomAquiredItemRule : ItemRule
{
	private string itemType;

	public RandomAquiredItemRule(XmlNode node)
		: base(node, false)
	{
		_type = RuleType.RuleRandomAquiredItem;
		itemType = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		RefreshItems();
	}

	public void RefreshItems()
	{
		item = ListSF.GetRoster().GetInventory().GetRandomItemOfType(itemType);
	}

	protected virtual void OnItemsRefreshed()
	{
	}
}
