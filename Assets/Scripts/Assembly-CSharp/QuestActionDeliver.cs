using System.Xml;

public class QuestActionDeliver : QuestAction
{
	private string itemExpression;

	private string enchantmentExpression;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		itemExpression = EPKLCPOEELO.Attributes["Item"].GetStringOrDefault(string.Empty);
		enchantmentExpression = EPKLCPOEELO.Attributes["Enchantment"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ConditionExtension.CompareResult lNIDLHOIHIM = new ConditionExtension.CompareResult();
		QuestCondition kKDGLNECFHA = new QuestCondition();
		kKDGLNECFHA.SetParameters(GFIHPBCEEOB);
		kKDGLNECFHA.SetValue(itemExpression, lNIDLHOIHIM);
		string text = lNIDLHOIHIM.ToString();
		if (!text.Equals("0"))
		{
			ItemBuyHelper.BuyImmediatelyDelivery(text);
		}
		else
		{
			string empty = string.Empty;
			kKDGLNECFHA.SetValue(enchantmentExpression, lNIDLHOIHIM);
			empty = lNIDLHOIHIM.ToString();
			GameUtils.TrackDelivery(empty);
		}
		FinishAction();
	}
}
