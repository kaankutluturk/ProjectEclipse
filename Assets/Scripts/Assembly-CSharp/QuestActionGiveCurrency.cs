using System.Xml;

public class QuestActionGiveCurrency : QuestAction
{
	private string Type;

	private string Value;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		Type = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		Value = node.Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		Roster roster = ListSF.GetRoster();
		string currencyType = string.Empty;
		long value = 0L;
		GetValues(ref currencyType, ref value);
		if (currencyType == "Gold")
		{
			roster.SetMoney(roster.GetMoney() + value);
		}
		else if (currencyType == "Bonus")
		{
			roster.SetBonus(roster.GetBonus() + value, Roster.BalanceChangeType.CHANGE_QUEST);
		}
		else if (currencyType != string.Empty)
		{
			roster.AddCurrencyCount(currencyType, (int)value);
		}
		MenuController.RecreateMoney();
		FinishAction();
	}

	private void GetValues(ref string currencyType, ref long value)
	{
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(Parameters);
		condition.SetValue(Type, result);
		currencyType = result.ToString();
		result.Clear();
		condition.SetValue(Value, result);
		value = (long)result.resultNumber;
	}
}
