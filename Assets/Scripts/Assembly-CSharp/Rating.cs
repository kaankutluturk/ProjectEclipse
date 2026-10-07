using System;
using System.Xml;

public class Rating
{
	public string player;

	public string damageType;

	public string defenseType;

	public float Multiplier;

	public string enemyAttribute;

	public Rating()
	{
		player = string.Empty;
		damageType = string.Empty;
		defenseType = string.Empty;
		Multiplier = 0f;
		enemyAttribute = string.Empty;
	}

	public Rating(Rating source)
	{
		player = source.player;
		damageType = source.damageType;
		defenseType = source.defenseType;
		Multiplier = source.Multiplier;
		enemyAttribute = source.enemyAttribute;
	}

	public void Parse(XmlNode node, PerkSetAttributes setAttributes = null)
	{
		if (setAttributes != null)
		{
			foreach (XmlAttribute attribute in node.Attributes)
			{
				string text = XmlUtils.ParseString(attribute, string.Empty);
				if (text[0] == '_')
				{
					string attributeName = text.Substring(1, text.Length - 1);
					string value = setAttributes.GetValue(attributeName);
					attribute.Value = value;
				}
			}
		}
		player = XmlUtils.ParseString(node.Attributes["Player"], "Me");
		damageType = XmlUtils.ParseString(node.Attributes["Damage"]);
		defenseType = XmlUtils.ParseString(node.Attributes["Defense"]);
		try
		{
			Multiplier = XmlUtils.ParseFloat(node.Attributes["Multiplier"]);
		}
		catch (Exception)
		{
			int num = 0;
		}
		enemyAttribute = XmlUtils.ParseString(node.Attributes["EnemyAttribute"]);
	}
}
