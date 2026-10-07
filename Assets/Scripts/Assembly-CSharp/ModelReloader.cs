using System.Collections.Generic;
using System.Xml;

public static class ModelReloader
{
	public static void Reload(ModelObject modelObject, List<string> fileNames)
	{
		if (fileNames.Count > 0)
		{
			List<XmlNode> nodeSections = new List<XmlNode>();
			if (Concat(modelObject, nodeSections, fileNames))
			{
				Parse(modelObject, nodeSections);
			}
		}
	}

	private static bool Concat(ModelObject modelObject, List<XmlNode> nodes, List<string> fileNames)
	{
		int i = 0;
		for (int count = fileNames.Count; i < count; i++)
		{
			XmlDocument xmlDocument = ModelLoader.DocumentCache.GetDocument(SF2Paths.GetModelsPath(), fileNames[i]);
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

	private static void Parse(ModelObject modelObject, List<XmlNode> nodes)
	{
		if (!ReloadNodes(modelObject, nodes))
		{
			GameLog.Error("Nodes was not parsed");
		}
	}

	private static bool ReloadNodes(ModelObject modelObject, List<XmlNode> nodeSections)
	{
		for (int i = 0; i < nodeSections.Count; i++)
		{
			foreach (XmlNode childNode in nodeSections[i].ChildNodes)
			{
				ModelNode modelNode = modelObject.GetNodeByName(childNode.Name);
				if (modelNode != null)
				{
					ReloadNode(modelObject, modelNode, childNode);
				}
			}
		}
		modelObject.ResolveMacroNodeWeights();
		return true;
	}

	private static void ReloadNode(ModelObject modelObject, ModelNode modelNode, XmlNode xmlNode)
	{
		if (modelNode.GetName() != xmlNode.Name)
		{
			GameLog.Error("Model reload: {0} -- {1}", modelNode.GetName(), xmlNode.Name);
		}
		float x = xmlNode.Attributes["X"].ParseFloat();
		float y = 0f - xmlNode.Attributes["Y"].ParseFloat();
		float z = xmlNode.Attributes["Z"].ParseFloat();
		Vector3f nodePosition = new Vector3f(x, y, z);
		string text = xmlNode.Attributes["Type"].GetStringOrDefault();
		modelNode.SetStart(nodePosition);
		modelNode.SetEnd(nodePosition);
		if (text == "Node")
		{
			modelNode.SetCloth(xmlNode.Attributes["Cloth"].ParseBool());
			modelNode.SetAttenuation(xmlNode.Attributes["Attenuation"].ParseFloat());
		}
		modelNode.SetMass(xmlNode.Attributes["Mass"].ParseFloat());
		modelNode.SetFixed(xmlNode.Attributes["Fixed"].ParseBool());
		modelNode.SetVisible(xmlNode.Attributes["Visible"].ParseBool());
	}
}
