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
			BarScale bABKPEHINKF = new BarScale();
			bABKPEHINKF.Name = childNode.Attributes["Name"].GetStringOrDefault();
			XmlNode oEOOHNMCBOC = childNode["AttributeLimits"];
			bABKPEHINKF.ParseLimits(oEOOHNMCBOC, bABKPEHINKF.AttributeLimits);
			XmlNode oEOOHNMCBOC2 = childNode["ItemLimits"];
			bABKPEHINKF.ParseLimits(oEOOHNMCBOC2, bABKPEHINKF.ItemLimits);
			XmlAttribute cJBEMNNNHDM = childNode.Attributes["Power"];
			bABKPEHINKF.Power = cJBEMNNNHDM.ParseFloat(-1f);
			XmlAttribute cJBEMNNNHDM2 = childNode.Attributes["Min"];
			bABKPEHINKF.MinPower = cJBEMNNNHDM2.ParseFloat(-1f);
			XmlAttribute cJBEMNNNHDM3 = childNode.Attributes["Type"];
			bABKPEHINKF.Type = cJBEMNNNHDM3.GetStringOrDefault();
			Scales.Add(bABKPEHINKF);
		}
	}

	public BarScale GetScaleByName(string name)
	{
		return Scales.Find((BarScale DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(name));
	}
}
