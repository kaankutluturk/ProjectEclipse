using System.Collections.Generic;
using System.Xml;

public class QuestActionDiscount : QuestAction
{
	private string itemExpression = string.Empty;

	private string percentExpression = "0";

	private string toggleExpression = "0";

	private string periodExpression = string.Empty;

	private string newAmountExpression = string.Empty;

	private string newPriceExpression = string.Empty;

	private string saleExpression = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		itemExpression = node.Attributes["Item"].GetStringOrDefault(string.Empty);
		percentExpression = node.Attributes["Percent"].GetStringOrDefault(string.Empty);
		toggleExpression = node.Attributes["Toggle"].GetStringOrDefault(string.Empty);
		newAmountExpression = node.Attributes["NewAmount"].GetStringOrDefault("0");
		newPriceExpression = node.Attributes["NewPrice"].GetStringOrDefault(string.Empty);
		periodExpression = node.Attributes["Period"].GetStringOrDefault("0");
		saleExpression = node.Attributes["Sale"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		FinishAction();
	}

	private void EvaluateParameters(QuestParameters parameters, ItemInfo item, ref int upgradeLevel, ref float discountPercent, ref bool isToggled, ref long newAmount, ref long period, ref string newPrice, ref bool isSale)
	{
		string empty = string.Empty;
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(itemExpression, result);
		empty = result.ToString();
		string text = string.Empty;
		string text2 = string.Empty;
		List<string> list = new List<string>(empty.Split('|'));
		int count = list.Count;
		if (count > 0)
		{
			text = list[0];
		}
		if (count >= 2)
		{
			text2 = list[1];
		}
		if (text2 != string.Empty)
		{
			condition.SetValue(text2, result);
			upgradeLevel = (int)result.resultNumber;
		}
		else
		{
			upgradeLevel = -1;
		}
		result.Clear();
		condition.SetValue(percentExpression, result);
		discountPercent = (float)result.resultNumber;
		result.Clear();
		condition.SetValue(toggleExpression, result);
		isToggled = result.resultNumber > 0.0;
		result.Clear();
		condition.SetValue(newAmountExpression, result);
		newAmount = (long)result.resultNumber;
		result.Clear();
		condition.SetValue(periodExpression, result);
		period = (long)result.resultNumber;
		result.Clear();
		condition.SetValue(newPriceExpression, result);
		if (newPriceExpression != string.Empty && result.IsNumber())
		{
			newPrice = result.resultNumber.ToString();
		}
		else
		{
			newPrice = result.resultSTR;
		}
		result.Clear();
		condition.SetValue(saleExpression, result);
		isSale = result.resultNumber > 0.0;
		item = ListSF.GetItems().GetItemByName(text);
		if (item == null)
		{
			GameLog.Error("QuestActionDiscount - cant find item \"%s\" from name \"%s\"", text, itemExpression);
		}
	}
}
