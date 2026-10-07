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
			ShopOverride jHJPEFFBMFM = new ShopOverride();
			jHJPEFFBMFM.Type = childNode.Attributes["Type"].GetStringOrDefault();
			jHJPEFFBMFM.Screen = childNode.Attributes["Screen"].GetStringOrDefault();
			jHJPEFFBMFM.ItemName = childNode.Attributes["Name"].GetStringOrDefault();
			_Overrides.Add(jHJPEFFBMFM);
		}
	}

	public ShopOverride GetOverrideByScreen(string JPDNPODKKJP)
	{
		return _Overrides.Find((ShopOverride DHDMNHCIPEH) => DHDMNHCIPEH.Screen.Equals(JPDNPODKKJP));
	}
}
