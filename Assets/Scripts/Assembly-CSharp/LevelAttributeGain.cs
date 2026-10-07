using System.Xml;

public class LevelAttributeGain
{
	public Attributes Gains = new Attributes();

	public void Parse(XmlNode node)
	{
		Gains.Clear();
		foreach (XmlAttribute attribute in node.Attributes)
		{
			string name = attribute.Name;
			int bAINMLLIKOL = XmlUtils.ParseInt(attribute);
			Gains.Set(name, bAINMLLIKOL);
		}
	}
}
