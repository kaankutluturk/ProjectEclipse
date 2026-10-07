using System.Collections.Generic;
using System.Xml;

public class BarScales
{
	public List<BarScale> Scales = new List<BarScale>();

	public void Parse(XmlNode node)
	{
		Scales.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			BarScale scale = new BarScale();
			scale.Name = childNode.Attributes["Name"].GetStringOrDefault();
			XmlNode limitsNode = childNode["AttributeLimits"];
			scale.ParseLimits(limitsNode, scale.AttributeLimits);
			XmlNode oEOOHNMCBOC2 = childNode["ItemLimits"];
			scale.ParseLimits(oEOOHNMCBOC2, scale.ItemLimits);
			XmlAttribute powerAttribute = childNode.Attributes["Power"];
			scale.Power = powerAttribute.ParseFloat(-1f);
			XmlAttribute cJBEMNNNHDM2 = childNode.Attributes["Min"];
			scale.MinPower = cJBEMNNNHDM2.ParseFloat(-1f);
			XmlAttribute cJBEMNNNHDM3 = childNode.Attributes["Type"];
			scale.Type = cJBEMNNNHDM3.GetStringOrDefault();
			Scales.Add(scale);
		}
	}

	public BarScale GetScaleByName(string name)
	{
		return Scales.Find((BarScale scale) => scale.Name.Equals(name));
	}
}
