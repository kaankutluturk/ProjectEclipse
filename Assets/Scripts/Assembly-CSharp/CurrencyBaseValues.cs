using System.Collections.Generic;
using System.Xml;

public class CurrencyBaseValues
{
	public class CurrencyBaseValue
	{
		public string currencyName;

		private List<CharProgLevel> levels;

		public CurrencyBaseValue(XmlNode EBLIGDMALEA)
		{
			currencyName = EBLIGDMALEA.Attributes["Name"].GetStringOrDefault(string.Empty);
			levels = new List<CharProgLevel>();
			foreach (XmlNode childNode in EBLIGDMALEA.ChildNodes)
			{
				if (childNode.Name == "Level")
				{
					CharProgLevel item = new CharProgLevel(childNode);
					levels.Add(item);
				}
			}
		}

		public float GetBaseValue(int OMHDLKNHNMJ)
		{
			foreach (CharProgLevel item in levels)
			{
				if (OMHDLKNHNMJ >= item.Min && OMHDLKNHNMJ <= item.Max)
				{
					return item.value;
				}
			}
			return 0f;
		}
	}

	public List<CurrencyBaseValue> CurrencyValues = new List<CurrencyBaseValue>();

	public void Parse(XmlNode node)
	{
		CurrencyValues.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "Currency")
			{
				CurrencyBaseValue item = new CurrencyBaseValue(childNode);
				CurrencyValues.Add(item);
			}
		}
	}

	public float GetBaseValue(string currencyName)
	{
		foreach (CurrencyBaseValue item in CurrencyValues)
		{
			if (item.currencyName == currencyName)
			{
				int oMHDLKNHNMJ = ListSF.GetRoster().GetLevel();
				return item.GetBaseValue(oMHDLKNHNMJ);
			}
		}
		return 0f;
	}
}
