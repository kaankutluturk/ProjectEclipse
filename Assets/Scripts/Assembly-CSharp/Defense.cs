using System.Collections.Generic;
using System.Xml;

public class Defense
{
	public string DefenseName;

	public float Weight;

	public string CancellingItem;

	public List<Evaluation> Evaluations = new List<Evaluation>();

	public static int Parse(XmlNode BLLNKKNDNII, List<Defense> PNKJPOHEOJB)
	{
		int count = PNKJPOHEOJB.Count;
		foreach (XmlNode childNode in BLLNKKNDNII.ChildNodes)
		{
			if (childNode.Name == "Defense")
			{
				Defense dFKIEMBKKGA = new Defense();
				dFKIEMBKKGA.Parse(childNode);
				PNKJPOHEOJB.Add(dFKIEMBKKGA);
			}
		}
		return PNKJPOHEOJB.Count - count;
	}

	public void Parse(XmlNode MEEAKLDGLDF)
	{
		DefenseName = XmlUtils.ParseString(MEEAKLDGLDF.Attributes["Name"]);
		Weight = XmlUtils.ParseFloat(MEEAKLDGLDF.Attributes["Weight"]);
		CancellingItem = XmlUtils.ParseString(MEEAKLDGLDF.Attributes["CancellingItem"]);
		Evaluation.ParseAttributes(MEEAKLDGLDF, Evaluations);
	}
}
