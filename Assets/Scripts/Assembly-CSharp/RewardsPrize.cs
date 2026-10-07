using System.Collections.Generic;
using System.Xml;

public class RewardsPrize
{
	private string NameAttr = "Value";

	public float PerfectFactor;

	public float FirstStrikeFactor;

	public float ComboCountFactor;

	public float HeadShotFactor;

	public float DefaultPrizeBaseFactor;

	public float ShockFactor;

	public List<float> Styles = new List<float>();

	public void Parse(XmlNode node)
	{
		DefaultPrizeBaseFactor = node["DefaultPrizeBaseFactor"].Attributes[NameAttr].ParseFloat();
		PerfectFactor = node["Perfect"].Attributes[NameAttr].ParseFloat();
		FirstStrikeFactor = node["FirstStrike"].Attributes[NameAttr].ParseFloat();
		ComboCountFactor = node["ComboCount"].Attributes[NameAttr].ParseFloat();
		HeadShotFactor = node["HeadShot"].Attributes[NameAttr].ParseFloat();
		ShockFactor = node["Shock"].Attributes[NameAttr].ParseFloat();
		ParseStyles(node);
	}

	private void ParseStyles(XmlNode node)
	{
		Styles.Clear();
		XmlNode xmlNode = node["Styles"];
		Styles.Add(xmlNode["Turtle"].Attributes[NameAttr].ParseFloat());
		Styles.Add(xmlNode["Hard"].Attributes[NameAttr].ParseFloat());
		Styles.Add(xmlNode["Brutal"].Attributes[NameAttr].ParseFloat());
		Styles.Add(xmlNode["Agressive"].Attributes[NameAttr].ParseFloat());
		Styles.Add(xmlNode["Crazy"].Attributes[NameAttr].ParseFloat());
		Styles.Add(xmlNode["Fantastic"].Attributes[NameAttr].ParseFloat());
	}
}
