using System.Collections.Generic;
using System.Xml;

public class Evaluation
{
	public string Name;

	public float Shift;

	public static int ParseAttributes(XmlNode node, List<Evaluation> evaluations)
	{
		return Parse(node, evaluations, "Attribute");
	}

	public static float ParseAverageQuantity(XmlNode node, RatingEvaluation rating)
	{
		rating.averageQuantity = XmlUtils.ParseFloat(node.Attributes["AverageQuantity"]);
		return rating.averageQuantity;
	}

	public static float ParseAverageDamageAndRecharge(XmlNode node, RatingEvaluation rating)
	{
		rating.averageBaseDamage = XmlUtils.ParseFloat(node.Attributes["AverageBaseDamage"]);
		rating.rechargeRate = XmlUtils.ParseFloat(node.Attributes["RechargeRate"]);
		rating.magicRechargeRate = XmlUtils.ParseFloat(node.Attributes["MagicRechargeRate"]);
		return rating.averageBaseDamage;
	}

	private void Parse(XmlNode node)
	{
		Name = XmlUtils.ParseString(node.Attributes["Name"]);
		Shift = XmlUtils.ParseFloat(node.Attributes["Shift"]);
	}

	private static int Parse(XmlNode node, List<Evaluation> evaluations, string childName)
	{
		int count = evaluations.Count;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == childName)
			{
				Evaluation evaluation = new Evaluation();
				evaluation.Parse(childNode);
				evaluations.Add(evaluation);
			}
		}
		return evaluations.Count - count;
	}
}
