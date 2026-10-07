using System.Collections.Generic;
using System.Xml;

public class StyleLevels
{
	public List<Style> Styles = new List<Style>();

	public float StylePerHit;

	public float DecreaseSpeed;

	public float Penalty;

	public Style GetStyle(int index)
	{
		if (Styles.Count > index)
		{
			return Styles[index];
		}
		return null;
	}

	public float GetStyleMultiplier(int index)
	{
		Style mHOJFHKHIIL = GetStyle(index);
		return (mHOJFHKHIIL == null) ? 0f : mHOJFHKHIIL.StyleMultiplier;
	}

	public void Parse(XmlNode node)
	{
		Styles.Clear();
		XmlAttribute xmlAttribute = node.Attributes["StylePerHit"];
		if (xmlAttribute != null)
		{
			StylePerHit = xmlAttribute.ParseFloat();
		}
		else
		{
			GameLog.Error("Error: InternalSettings->StyleLevels: Attribute StylePerHit is absent!");
		}
		XmlAttribute xmlAttribute2 = node.Attributes["DecreaseSpeed"];
		if (xmlAttribute2 != null)
		{
			DecreaseSpeed = xmlAttribute2.ParseFloat();
		}
		else
		{
			GameLog.Error("Error: InternalSettings->StyleLevels: Attribute DecreaseSpeed is absent!");
		}
		XmlAttribute xmlAttribute3 = node.Attributes["Penalty"];
		if (xmlAttribute3 != null)
		{
			Penalty = xmlAttribute3.ParseFloat();
		}
		else
		{
			GameLog.Error("Error: InternalSettings->StyleLevels: Attribute Penalty is absent!");
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			Style mHOJFHKHIIL = new Style();
			mHOJFHKHIIL.Name = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			mHOJFHKHIIL.StyleMultiplier = childNode.Attributes["StyleMultiplier"].ParseFloat();
			mHOJFHKHIIL.TextImage = childNode.Attributes["TextImage"].GetStringOrDefault(string.Empty);
			mHOJFHKHIIL.BarImage = childNode.Attributes["BarImage"].GetStringOrDefault(string.Empty);
			Styles.Add(mHOJFHKHIIL);
		}
		if (Styles.Count == 0)
		{
			GameLog.Error("Error: InternalSettings->StyleLevels: Styles is absent!");
		}
	}
}
