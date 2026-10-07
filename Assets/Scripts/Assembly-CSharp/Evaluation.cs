using System.Collections.Generic;
using System.Xml;

public class Evaluation
{
	public string Name;

	public float Shift;

	public static int ParseAttributes(XmlNode BLLNKKNDNII, List<Evaluation> PNKJPOHEOJB)
	{
		return Parse(BLLNKKNDNII, PNKJPOHEOJB, "Attribute");
	}

	public static float ParseAverageQuantity(XmlNode BLLNKKNDNII, RatingEvaluation PNKJPOHEOJB)
	{
		PNKJPOHEOJB.averageQuantity = XmlUtils.ParseFloat(BLLNKKNDNII.Attributes["AverageQuantity"]);
		return PNKJPOHEOJB.averageQuantity;
	}

	public static float ParseAverageDamageAndRecharge(XmlNode BLLNKKNDNII, RatingEvaluation PNKJPOHEOJB)
	{
		PNKJPOHEOJB.averageBaseDamage = XmlUtils.ParseFloat(BLLNKKNDNII.Attributes["AverageBaseDamage"]);
		PNKJPOHEOJB.rechargeRate = XmlUtils.ParseFloat(BLLNKKNDNII.Attributes["RechargeRate"]);
		PNKJPOHEOJB.magicRechargeRate = XmlUtils.ParseFloat(BLLNKKNDNII.Attributes["MagicRechargeRate"]);
		return PNKJPOHEOJB.averageBaseDamage;
	}

	private void Parse(XmlNode MEEAKLDGLDF)
	{
		Name = XmlUtils.ParseString(MEEAKLDGLDF.Attributes["Name"]);
		Shift = XmlUtils.ParseFloat(MEEAKLDGLDF.Attributes["Shift"]);
	}

	private static int Parse(XmlNode BLLNKKNDNII, List<Evaluation> PNKJPOHEOJB, string JLEKBBJBLOE)
	{
		int count = PNKJPOHEOJB.Count;
		foreach (XmlNode childNode in BLLNKKNDNII.ChildNodes)
		{
			if (childNode.Name == JLEKBBJBLOE)
			{
				Evaluation bAEELPHJKBD = new Evaluation();
				bAEELPHJKBD.Parse(childNode);
				PNKJPOHEOJB.Add(bAEELPHJKBD);
			}
		}
		return PNKJPOHEOJB.Count - count;
	}
}
