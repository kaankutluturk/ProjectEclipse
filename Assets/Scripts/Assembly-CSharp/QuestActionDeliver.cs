using System.Xml;

public class QuestActionDeliver : QuestAction
{
	private string itemExpression;

	private string enchantmentExpression;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		itemExpression = node.Attributes["Item"].GetStringOrDefault(string.Empty);
		enchantmentExpression = node.Attributes["Enchantment"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(itemExpression, result);
		string text = result.ToString();
		if (!text.Equals("0"))
		{
			ItemBuyHelper.BuyImmediatelyDelivery(text);
		}
		else
		{
			string empty = string.Empty;
			condition.SetValue(enchantmentExpression, result);
			empty = result.ToString();
			GameUtils.TrackDelivery(empty);
		}
		FinishAction();
	}
}
