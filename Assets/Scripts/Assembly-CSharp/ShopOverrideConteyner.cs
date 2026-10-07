using System.Collections.Generic;
using System.Xml;

public class ShopOverrideConteyner
{
	private List<ShopOverride> _Overrides = new List<ShopOverride>();

	public void Parse(XmlNode node)
	{
		_Overrides.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			ShopOverride shopOverride = new ShopOverride();
			shopOverride.Type = childNode.Attributes["Type"].GetStringOrDefault();
			shopOverride.Screen = childNode.Attributes["Screen"].GetStringOrDefault();
			shopOverride.ItemName = childNode.Attributes["Name"].GetStringOrDefault();
			_Overrides.Add(shopOverride);
		}
	}

	public ShopOverride GetOverrideByScreen(string screenName)
	{
		return _Overrides.Find((ShopOverride shopOverride) => shopOverride.Screen.Equals(screenName));
	}
}
