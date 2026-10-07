using System.Xml;

public class GameCurrency
{
	public enum CurrencyGroup
	{
		CURRENCY_GROUP_NONE = 0,
		CURRENCY_GROUP_FORGE = 1
	}

	public CurrencyGroup Group;

	public string Name;

	public string Icon;

	public GameCurrency(XmlNode node)
	{
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		Icon = node.Attributes["Icon"].GetStringOrDefault(string.Empty);
		string text = node.Attributes["Group"].GetStringOrDefault(string.Empty);
		if (text == "Forge")
		{
			Group = CurrencyGroup.CURRENCY_GROUP_FORGE;
		}
		else
		{
			Group = CurrencyGroup.CURRENCY_GROUP_NONE;
		}
	}

	public GameCurrency(string PIKIACPLHJE, string NFBKDDABPOM, CurrencyGroup APLILFFIMMM = CurrencyGroup.CURRENCY_GROUP_NONE)
	{
		Name = PIKIACPLHJE;
		Icon = NFBKDDABPOM;
		Group = APLILFFIMMM;
	}

	public void CopyFrom(GameCurrency MDDNHLBDJBN)
	{
		Name = MDDNHLBDJBN.Name;
		Icon = MDDNHLBDJBN.Icon;
		Group = MDDNHLBDJBN.Group;
	}
}
