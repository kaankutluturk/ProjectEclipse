using System.Xml;

public class Shock
{
	public float Threshold;

	public float FrameReduction;

	public float CriticalHitChanceBase;

	public string CriticalHitChanceAttribute;

	public float HeadHitChanceBase;

	public string HeadHitChanceAttribute;

	public int LooseningDelayFrames;

	public string WeaponName;

	public string SetAttributeName;

	public int SetAttributeValue;

	public Vector3f Impulse = new Vector3f();

	public void Parse(XmlNode node)
	{
		Threshold = node["Treshold"].Attributes["Value"].ParseFloat();
		FrameReduction = node["FrameReduction"].Attributes["Value"].ParseFloat();
		LooseningDelayFrames = node["LooseningDelay"].Attributes["Frames"].ParseInt();
		WeaponName = node["Weapon"].Attributes["Name"].GetStringOrDefault(string.Empty);
		SetAttributeName = node["SetAttribute"].Attributes["Name"].GetStringOrDefault(string.Empty);
		SetAttributeValue = node["SetAttribute"].Attributes["Value"].ParseInt();
		CriticalHitChanceBase = node["CriticalHitChance"].Attributes["Base"].ParseFloat();
		CriticalHitChanceAttribute = node["CriticalHitChance"].Attributes["Attribute"].GetStringOrDefault(string.Empty);
		HeadHitChanceBase = node["HeadHitChance"].Attributes["Base"].ParseFloat();
		HeadHitChanceAttribute = node["HeadHitChance"].Attributes["Attribute"].GetStringOrDefault(string.Empty);
		Impulse.SetX(node["Impulse"].Attributes["X"].ParseFloat());
		Impulse.SetY(node["Impulse"].Attributes["Y"].ParseFloat());
		Impulse.SetZ(node["Impulse"].Attributes["Z"].ParseFloat());
	}
}
