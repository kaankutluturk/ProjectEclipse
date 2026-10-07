using System.Collections.Generic;
using System.Xml;

public class CurrencyBaseValues
{
	public class CurrencyBaseValue
	{
		public string currencyName;

		private List<CharProgLevel> levels;

		public CurrencyBaseValue(XmlNode node)
		{
			currencyName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
			levels = new List<CharProgLevel>();
			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (childNode.Name == "Level")
				{
					CharProgLevel item = new CharProgLevel(childNode);
					levels.Add(item);
				}
			}
		}

		public float GetBaseValue(int level)
		{
			foreach (CharProgLevel item in levels)
			{
				if (level >= item.Min && level <= item.Max)
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
				int playerLevel = ListSF.GetRoster().GetLevel();
				return item.GetBaseValue(playerLevel);
			}
		}
		return 0f;
	}
}
