using System.Xml;

public class QuestActionBuyItem : QuestAction
{
	private string _name = string.Empty;

	private string currency = string.Empty;

	private ItemAction _itemAction;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		currency = node.Attributes["Currency"].GetStringOrDefault(string.Empty);
		if (currency == "Coins")
		{
			_itemAction = ItemAction.Item_Buy_Gold;
		}
		else if (currency == "Ruby")
		{
			_itemAction = ItemAction.Item_Buy_Ruby;
		}
		else if (currency == "Real")
		{
			_itemAction = ItemAction.Item_Buy_Real;
		}
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(_name, result);
		string itemName = result.ToString();
		ItemInfo itemInfo = ListSF.GetItems().GetItemByName(itemName);
		if (itemInfo != null)
		{
			GameUtils.ApplyItemAction(itemInfo, _itemAction);
		}
		FinishAction();
	}
}
