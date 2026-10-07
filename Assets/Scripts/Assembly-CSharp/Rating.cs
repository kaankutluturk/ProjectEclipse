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

	public Rating(Rating NOLFMPDGCOC)
	{
		player = NOLFMPDGCOC.player;
		damageType = NOLFMPDGCOC.damageType;
		defenseType = NOLFMPDGCOC.defenseType;
		Multiplier = NOLFMPDGCOC.Multiplier;
		enemyAttribute = NOLFMPDGCOC.enemyAttribute;
	}

	public void Parse(XmlNode node, PerkSetAttributes CJILONFAJIK = null)
	{
		if (CJILONFAJIK != null)
		{
			foreach (XmlAttribute attribute in node.Attributes)
			{
				string text = XmlUtils.ParseString(attribute, string.Empty);
				if (text[0] == '_')
				{
					string gOHIIMFFFJI = text.Substring(1, text.Length - 1);
					string value = CJILONFAJIK.GetValue(gOHIIMFFFJI);
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
