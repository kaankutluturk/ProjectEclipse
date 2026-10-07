using System.Xml;

public class QuestActionTakeCurrency : QuestAction
{
	private string _type = string.Empty;

	private string _name = string.Empty;

	private string _value = string.Empty;

	private QuestActionsSequence successSequence = new QuestActionsSequence();

	private QuestActionsSequence errorSequence = new QuestActionsSequence();

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_type = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_value = node.Attributes["Value"].GetStringOrDefault(string.Empty);
		XmlNode successNode = node["Success"];
		XmlNode ePKLCPOEELO2 = node["Error"];
		ParseSequenceWithUnlock(successNode, successSequence, OnActionComplete);
		ParseSequenceWithUnlock(ePKLCPOEELO2, errorSequence, OnActionComplete);
	}

	public override void Execute(QuestParameters parameters)
	{
		ResetSequences();
		base.Execute(parameters);
		string currencyType = string.Empty;
		string name = string.Empty;
		long value = 0L;
		GetValues(ref currencyType, ref name, ref value);
		bool flag = GetIsCurrencyExist(currencyType, name);
		bool flag2 = GetCurrencyCount(currencyType, name) >= value;
		if (flag && flag2)
		{
			AddCurrencyCount(currencyType, name, value);
			MenuController.RefreshMoney();
			successSequence.Run(parameters);
		}
		else
		{
			errorSequence.Run(parameters);
		}
	}

	private bool GetIsCurrencyExist(string currencyType, string name)
	{
		bool result = false;
		switch (currencyType)
		{
		case "Gold":
		case "Bonus":
			result = true;
			break;
		case "Currency":
			result = ListSF.GetRoster().GetIsCurrencyExist(name);
			break;
		default:
			if (currencyType != string.Empty)
			{
				result = ListSF.GetRoster().GetIsCurrencyExist(currencyType);
			}
			break;
		}
		return result;
	}

	private long GetCurrencyCount(string currencyType, string name)
	{
		long num = 0L;
		Roster roster = ListSF.GetRoster();
		switch (currencyType)
		{
		case "Gold":
			return roster.GetMoney();
		case "Bonus":
			return roster.GetBonus();
		case "Currency":
			return roster.GetCurrencyCount(name);
		default:
			return roster.GetCurrencyCount(currencyType);
		}
	}

	private void AddCurrencyCount(string currencyType, string name, long value)
	{
		Roster roster = ListSF.GetRoster();
		switch (currencyType)
		{
		case "Gold":
			roster.SetMoney(roster.GetMoney() - value);
			return;
		case "Bonus":
			roster.SetBonus(roster.GetBonus() - value, Roster.BalanceChangeType.CHANGE_QUEST);
			return;
		case "Currency":
			roster.AddCurrencyCount(name, (int)(-value));
			return;
		}
		if (currencyType != string.Empty)
		{
			roster.AddCurrencyCount(currencyType, (int)(-value));
		}
	}

	private void GetValues(ref string currencyType, ref string name, ref long value)
	{
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(Parameters);
		condition.SetValue(_type, result);
		currencyType = result.ToString();
		result.Clear();
		condition.SetValue(_name, result);
		name = result.ToString();
		result.Clear();
		condition.SetValue(_value, result);
		value = result.ToString().ToLong(0L);
	}

	private void OnActionComplete(object data)
	{
		FinishAction();
	}

	public override void ResetSequences()
	{
		successSequence.Reset();
		errorSequence.Reset();
	}
}
