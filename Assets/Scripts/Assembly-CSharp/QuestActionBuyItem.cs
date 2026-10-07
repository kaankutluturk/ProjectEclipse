using System.Xml;

public class QuestActionBuyItem : QuestAction
{
	private string _name = string.Empty;

	private string currency = string.Empty;

	private ItemAction _itemAction;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		_name = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
		currency = EPKLCPOEELO.Attributes["Currency"].GetStringOrDefault(string.Empty);
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

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		kKDGLNECFHA.SetValue(_name, lNIDLHOIHIM);
		string gOHIIMFFFJI = lNIDLHOIHIM.ToString();
		ItemInfo dJKEECEOCJB = ListSF.GetItems().GetItemByName(gOHIIMFFFJI);
		if (dJKEECEOCJB != null)
		{
			GameUtils.ApplyItemAction(dJKEECEOCJB, _itemAction);
		}
		FinishAction();
	}
}
