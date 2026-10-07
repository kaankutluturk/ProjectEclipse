using System.Collections.Generic;
using System.Xml;

public class LevelThresholds
{
	public List<global::Pair<int, uint>> Thresholds = new List<global::Pair<int, uint>>();

	public void Parse(XmlNode EBLIGDMALEA)
	{
		Thresholds.Clear();
		foreach (XmlNode childNode in EBLIGDMALEA.ChildNodes)
		{
			int gBCLEDJAOBM = childNode.Attributes["Level"].ParseInt();
			uint pOFHDGJAFMP = childNode.Attributes["Exp"].ParseUint();
			Thresholds.Add(new global::Pair<int, uint>(gBCLEDJAOBM, pOFHDGJAFMP));
		}
	}
}
