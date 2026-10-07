using System.Xml;

public class CurrencyCostRule : Rule
{
	private string _currencyName = string.Empty;

	private int _currencyValue;

	public string CurrencyName
	{
		get
		{
			return GetCurrencyName();
		}
	}

	public int CurrencyValue
	{
		get
		{
			return GetCurrencyValue();
		}
	}

	public CurrencyCostRule(XmlNode node)
		: base(RuleType.RuleCurrencyCost, node)
	{
		Parse(node);
	}

	public CurrencyCostRule(CurrencyCostRule source)
		: base(source)
	{
		_currencyName = source._currencyName;
		_currencyValue = 999888777;
	}

	public string GetCurrencyName()
	{
		return _currencyName;
	}

	public int GetCurrencyValue()
	{
		return 999888777;
	}

	protected override void Parse(XmlNode node)
	{
		_currencyName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_currencyValue = node.Attributes["Value"].ParseInt();
		if (_currencyValue < 0)
		{
			_currencyValue = 0;
		}
		_currencyValue = 999888777;
	}
}
