using System.Collections.Generic;
using System.Xml;

public static class ModelReloader
{
	public static void Reload(ModelObject ACENLMONNPA, List<string> CBHAEPCLDFG)
	{
		if (CBHAEPCLDFG.Count > 0)
		{
			List<XmlNode> iLCCDINCICK = new List<XmlNode>();
			if (Concat(ACENLMONNPA, iLCCDINCICK, CBHAEPCLDFG))
			{
				Parse(ACENLMONNPA, iLCCDINCICK);
			}
		}
	}

	private static bool Concat(ModelObject GIAMLEDNFJD, List<XmlNode> nodes, List<string> CBHAEPCLDFG)
	{
		int i = 0;
		for (int count = CBHAEPCLDFG.Count; i < count; i++)
		{
			XmlDocument xmlDocument = ModelLoader.DocumentCache.GetDocument(SF2Paths.GetModelsPath(), CBHAEPCLDFG[i]);
			XmlNode xmlNode = ((xmlDocument == null) ? null : xmlDocument["Scene"]);
			XmlNode xmlNode2 = ((xmlNode == null) ? null : xmlNode["Nodes"]);
			if (xmlNode2 == null)
			{
				// Overlay model files are allowed to contain only figures, edges or
				// materials. They contribute no reset positions and must not add a null
				// Nodes entry to the reload pass (mdl_punching_bag is one such file).
				continue;
			}
			nodes.Add(xmlNode2);
		}
		return nodes.Count > 0;
	}

	private static void Parse(ModelObject ACENLMONNPA, List<XmlNode> nodes)
	{
		if (!ReloadNodes(ACENLMONNPA, nodes))
		{
			GameLog.Error("Nodes was not parsed");
		}
	}

	private static bool ReloadNodes(ModelObject ACENLMONNPA, List<XmlNode> BMGDKMNOLLL)
	{
		for (int i = 0; i < BMGDKMNOLLL.Count; i++)
		{
			foreach (XmlNode childNode in BMGDKMNOLLL[i].ChildNodes)
			{
				ModelNode modelNode = ACENLMONNPA.GetNodeByName(childNode.Name);
				if (modelNode != null)
				{
					ReloadNode(ACENLMONNPA, modelNode, childNode);
				}
			}
		}
		ACENLMONNPA.ResolveMacroNodeWeights();
		return true;
	}

	private static void ReloadNode(ModelObject ACENLMONNPA, ModelNode NPDJNAMFIKD, XmlNode EABJIAHGLEO)
	{
		if (NPDJNAMFIKD.GetName() != EABJIAHGLEO.Name)
		{
			GameLog.Error("Model reload: {0} -- {1}", NPDJNAMFIKD.GetName(), EABJIAHGLEO.Name);
		}
		float lHNJJFDIJKK = EABJIAHGLEO.Attributes["X"].ParseFloat();
		float fFFHIOALHGM = 0f - EABJIAHGLEO.Attributes["Y"].ParseFloat();
		float pDCENMEKIAP = EABJIAHGLEO.Attributes["Z"].ParseFloat();
		Vector3f bAINMLLIKOL = new Vector3f(lHNJJFDIJKK, fFFHIOALHGM, pDCENMEKIAP);
		string text = EABJIAHGLEO.Attributes["Type"].GetStringOrDefault();
		NPDJNAMFIKD.SetStart(bAINMLLIKOL);
		NPDJNAMFIKD.SetEnd(bAINMLLIKOL);
		if (text == "Node")
		{
			NPDJNAMFIKD.SetCloth(EABJIAHGLEO.Attributes["Cloth"].ParseBool());
			NPDJNAMFIKD.SetAttenuation(EABJIAHGLEO.Attributes["Attenuation"].ParseFloat());
		}
		NPDJNAMFIKD.SetMass(EABJIAHGLEO.Attributes["Mass"].ParseFloat());
		NPDJNAMFIKD.SetFixed(EABJIAHGLEO.Attributes["Fixed"].ParseBool());
		NPDJNAMFIKD.SetVisible(EABJIAHGLEO.Attributes["Visible"].ParseBool());
	}
}
