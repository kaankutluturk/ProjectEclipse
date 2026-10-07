using System.Collections.Generic;
using System.Xml;

public class ModifiedAlignFormula
{
	public class DamageAttribute
	{
		public string Name;

		public float DamageMultiplier;

		public float NetDamage;

		public float MinAttributeDifference;

		public string ApplyTo;

		public DamageAttribute(XmlNode node)
		{
			Name = XmlUtils.ParseString(node.Attributes["Name"]);
			DamageMultiplier = XmlUtils.ParseFloat(node.Attributes["DamageMultiplier"], 2f);
			NetDamage = XmlUtils.ParseFloat(node.Attributes["NetDamage"], 0.1f);
			MinAttributeDifference = XmlUtils.ParseFloat(node.Attributes["MinAttributeDifference"]);
			ApplyTo = XmlUtils.ParseString(node.Attributes["ApplyTo"], "Player");
			if (ApplyTo != "Player" && ApplyTo != "Enemy")
			{
				ApplyTo = "Player";
			}
		}
	}

	private List<DamageAttribute> damageAttributes = new List<DamageAttribute>();

	public void Parse(XmlNode node)
	{
		damageAttributes.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			DamageAttribute item = new DamageAttribute(childNode);
			damageAttributes.Add(item);
		}
	}

	public DamageAttribute FindDamageAttribute(string name)
	{
		for (int i = 0; i < damageAttributes.Count; i++)
		{
			if (damageAttributes[i].Name == name)
			{
				return damageAttributes[i];
			}
		}
		return null;
	}
}
