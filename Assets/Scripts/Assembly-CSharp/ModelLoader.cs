using System.Collections.Generic;
using System.Xml;

public class ModelLoader
{
	public class CacheModelDocuments
	{
		private Dictionary<string, XmlDocument> documents = new Dictionary<string, XmlDocument>();

		public XmlDocument GetDocument(string path, string cacheKey)
		{
			XmlDocument value = null;
			if (documents.TryGetValue(cacheKey, out value))
			{
				return value;
			}
			value = XmlUtils.OpenXMLDocument(path, cacheKey, XmlUtils.XmlSourceMode.ForcedResourced);
			documents.Add(cacheKey, value);
			return value;
		}

		public void ClearCache()
		{
			documents.Clear();
		}
	}

	private const string ChildNodeAttributePrefix = "ChildNode";

	private const string WeightAttributePrefix = "LCC";

	public static CacheModelDocuments DocumentCache = new CacheModelDocuments();

	public static void ClearDocumentCache()
	{
		DocumentCache.ClearCache();
	}

	private static void PostProcessNodes(List<ModelNode> nodes)
	{
	}

	public static void Load(ModelObject modelObject, List<string> filePaths)
	{
		if (filePaths.Count == 0)
		{
			return;
		}
		XmlDocument xmlDocument = null;

        // Opt into authored body/skin contracts without changing archival model
        // parsing or first-definition bindings for recovered equipment.
        var parameters = modelObject.GetModel()?.Parameters;
        if (parameters != null && parameters.HasEclipseAuthoredModels)
        {
            var authored = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var model in parameters.EclipseAuthoredModels()) if (IsAuthoredModel(model)) authored.Add(AuthoredModelPath(model));
            if (authored.Count != 0)
            {
                var documents = new List<XmlDocument>(filePaths.Count);
                foreach (string path in filePaths)
                    documents.Add(path == "assets/models/.xml" ? null : DocumentCache.GetDocument(SF2Paths.GetModelsPath(), path));
                Eclipse.Modding.ModCharacterGeometry.Validate(filePaths, documents, authored);
            }
        }
		string text = "assets/models/.xml";
		foreach (string item in filePaths)
		{
			if (!(item == text))
			{
				xmlDocument = DocumentCache.GetDocument(SF2Paths.GetModelsPath(), item);
				if (xmlDocument != null)
				{
					Parse(modelObject, xmlDocument, parameters != null && parameters.HidesEclipseFigures(item));
					continue;
				}
				GameLog.Error("File '{0}' not found", item);
			}
		}
		modelObject.FindPivotNode();
		modelObject.CalculateTotalWeight();
		PostProcessNodes(modelObject.GetAllNodes());
		modelObject.SetFileNames(filePaths);
		modelObject.BuildPairNodes();
		modelObject.ResolveMacroNodeWeights();
	}

    private static string AuthoredModelPath(string path) => path.EndsWith(".xml", System.StringComparison.OrdinalIgnoreCase) ? path : path + ".xml";
    private static bool IsAuthoredModel(string path) => !string.IsNullOrEmpty(path) && path.IndexOf(':') > 0 && !path.StartsWith("core:", System.StringComparison.Ordinal);

    // Form preparation must not accept the legacy loader's log-and-continue
    // behavior for absent assets. Populate the same document cache before any
    // native model construction; optional empty-model sentinels remain omitted.
    internal static void RequireModelDocuments(List<string> paths)
    {
        if (paths == null) throw new System.ArgumentNullException(nameof(paths));
        int count = 0;
        foreach (string path in paths)
        {
            if (path == "assets/models/.xml") continue;
            if (string.IsNullOrWhiteSpace(path))
                throw new System.IO.InvalidDataException("Prepared form contains an empty model path.");
            XmlDocument document = DocumentCache.GetDocument(SF2Paths.GetModelsPath(), path);
            if (document == null)
                throw new System.IO.FileNotFoundException("Prepared form model is missing: " + path, path);
            if (document["Scene"] == null || document["Scene"]["Figures"] == null)
                throw new System.IO.InvalidDataException("Prepared form model requires Scene and Figures: " + path);
            count++;
        }
        if (count == 0)
            throw new System.IO.InvalidDataException("Prepared form requires at least one model document.");
    }

	private static void Parse(ModelObject modelObject, XmlDocument document, bool hideFigures = false)
	{
		XmlNode sceneNode = document["Scene"];
		if (!ParseNodes(modelObject, sceneNode))
		{
			GameLog.Write("Nodes was not parsed");
		}
		if (!ParseEdges(modelObject, sceneNode))
		{
			GameLog.Write("Edges was not parsed");
		}
		// An Eclipse look keeps hidden equipment's nodes and edges but draws no figures.
		if (hideFigures) return;
		if (!ParseCapsules(modelObject, sceneNode))
		{
			GameLog.Write("Capsules was not parsed");
		}
		if (!ParseTriangles(modelObject, sceneNode))
		{
			GameLog.Write("Triangles was not parsed");
		}
	}

	private static bool ParseNodes(ModelObject modelObject, XmlNode sceneNode)
	{
		XmlNode xmlNode = sceneNode["Nodes"];
		if (xmlNode == null)
		{
			return true;
		}
		List<global::Pair<string, float>> centerOfMassWeights = new List<global::Pair<string, float>>();
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			ParseNode(modelObject, childNode, centerOfMassWeights);
		}
		modelObject.AddCenterOfMassNodes(centerOfMassWeights);
		if (modelObject.GetNodeCount() == 0)
		{
			modelObject.set_NodesCount(modelObject.GetAllNodes().Count);
		}
		return true;
	}

	private static bool ParseEdges(ModelObject modelObject, XmlNode sceneNode)
	{
		XmlNode xmlNode = sceneNode["Edges"];
		if (xmlNode == null)
		{
			return true;
		}
		modelObject.GetAllEdges().Capacity = modelObject.GetAllEdges().Count + xmlNode.ChildNodes.Count;
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			int num = childNode.Attributes["Iterations"].ParseInt(1);
			string name = childNode.Name;
			for (int i = 0; i < num; i++)
			{
				string empty = string.Empty;
				empty += name;
				if (i > 0)
				{
					empty = empty + "CI" + i;
				}
				ParseEdge(modelObject, childNode, empty);
			}
		}
		return true;
	}

	private static bool ParseCapsules(ModelObject modelObject, XmlNode sceneNode)
	{
		XmlNode xmlNode = sceneNode["Figures"];
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			string value = childNode.Attributes["Type"].Value;
			if (value == "Capsule")
			{
				ParseCapsule(modelObject, childNode);
			}
		}
		return true;
	}

	private static bool ParseTriangles(ModelObject modelObject, XmlNode sceneNode)
	{
		XmlNode xmlNode = sceneNode["Figures"];
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			string value = childNode.Attributes["Type"].Value;
			if (value == "Triangle")
			{
				ParseTriangle(modelObject, childNode);
			}
		}
		return true;
	}

	private static void ParseNode(ModelObject modelObject, XmlNode node, List<global::Pair<string, float>> centerOfMassWeights)
	{
		ModelNode Node = null;
		Vector3f Position = new Vector3f(node.Attributes["X"].ParseFloat(), 0f - node.Attributes["Y"].ParseFloat(), node.Attributes["Z"].ParseFloat());
		string value = node.Attributes["Type"].Value;
		string name = node.Name;
		// Shipped models repeat node names: DE's mdl_armor_forest_spirit repeats 49, and dual
		// weapons such as mdl_weapon_hunger redefine the skeleton's off-hand Weapon-Node*_2
		// attachment nodes. The first definition owns the name; a later one is not created, so
		// that file's edges bind to the existing (arm-attached) node instead of a loose copy.
		if (modelObject.GetNodesByName().ContainsKey(name))
		{
			return;
		}
		bool flag = value == "CenterOfMass";
		if (value == "Node" || flag)
		{
			Node = new ModelNode(name, Position);
			Node.SetCloth(node.Attributes["Cloth"].ParseBool());
			Node.SetAttenuation(node.Attributes["Attenuation"].ParseFloat());
			if (flag)
			{
				centerOfMassWeights.Clear();
				ParseChildNodeWeights(centerOfMassWeights, node, false);
			}
			modelObject.GetPlainNodes().Add(Node);
		}
		else if (value == "MacroNode" || value == "SkinnedNode")
		{
			Position.SetX(Position.GetX() * -1f);
			ModelMacroNode macroNode = new ModelMacroNode(name, Position);
			Node = macroNode;
			if (value == "SkinnedNode") macroNode.LoadSkinBindings(modelObject, node);
			else ParseMacroNodeWeights(macroNode, node);
			modelObject.GetMacroNodes().Add(macroNode);
		}
		if (Node != null)
		{
			Node.SetID(modelObject.GetAllNodes().Count);
			Node.SetMass(node.Attributes["Mass"].ParseFloat());
			Node.SetFixed(node.Attributes["Fixed"].ParseBool());
			Node.SetVisible(node.Attributes["Visible"].ParseBool());
			Node.SetIsShock(node.Attributes["Shock"].ParseBool());
			Node.SetCollisible(node.Attributes["Collisible"].ParseBool());
			Node.SetWeak(node.Attributes["Weak"].ParseBool());
			modelObject.GetAllNodes().Add(Node);
			modelObject.GetNodesByName().Add(Node.GetName(), Node);
		}
	}

	private static void ParseEdge(ModelObject modelObject, XmlNode node, string edgeName)
	{
		ModelEdge edge = null;
		ModelNode startNode = modelObject.GetNodeByNameOrParent(node.Attributes["End1"].Value);
		ModelNode endNode = modelObject.GetNodeByNameOrParent(node.Attributes["End2"].Value);
		float edgeLength = node.Attributes["Length"].ParseFloat();
		float collisionRadius = node.Attributes["Radius"].ParseFloat();
		float startMargin = node.Attributes["Margin1"].ParseFloat();
		float endMargin = node.Attributes["Margin2"].ParseFloat();
		string text = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		string text2 = node.Attributes["SubType"].GetStringOrDefault(string.Empty);
		string bodyPart = node.Attributes["BodyPart"].GetStringOrDefault(string.Empty);
		string defense = node.Attributes["Defense"].GetStringOrDefault(string.Empty);
		int num = node.Attributes["Collisible"].ParseInt();
		bool hasBlood = node.Attributes["Blood"].ParseBool();
		bool isShock = node.Attributes["Shock"].ParseBool();
		edge = new ModelEdge(startNode, endNode);
		edge.set_Length(edgeLength);
		edge.set_Name(edgeName);
		edge.set_Collisible(num);
		edge.SetBodyPart(bodyPart);
		edge.SetDefense(defense);
		edge.SetHasBlood(hasBlood);
		edge.set_IsShock(isShock);
		edge.SetCollisionRadius(collisionRadius);
		edge.SetStartMargin(startMargin);
		edge.SetEndMargin(endMargin);
		if (text == "Edge")
		{
			edge.SetType(EdgeType.Edge);
			modelObject.GetStructuralEdges().Add(edge);
			if (num > 0)
			{
				modelObject.GetCollisionEdges().Add(edge);
			}
		}
		else
		{
			if (!(text == "Muscle"))
			{
				GameLog.Error("Wring type edge: {0}", text);
				return;
			}
			edge.SetType(EdgeType.Muscle);
			modelObject.GetMuscleEdges().Add(edge);
		}
		if (text2 == "None")
		{
			edge.SetSubType(EdgeSubType.None);
		}
		else if (text2 == "Blade")
		{
			edge.SetSubType(EdgeSubType.Blade);
		}
		modelObject.GetAllEdges().Add(edge);
	}

	private static void ParseCapsule(ModelObject modelObject, XmlNode node)
	{
		string value = node.Attributes["Edge"].Value;
		ModelEdge edge = modelObject.GetEdgeByName(value);
		if (edge != null)
		{
			Capsule capsule = new Capsule(edge);
			capsule.set_Name(node.Name);
			capsule.SetRadius1(node.Attributes["Radius1"].ParseFloat());
			capsule.SetRadius2(node.Attributes["Radius2"].ParseFloat());
			capsule.SetMargin1(node.Attributes["Margin1"].ParseFloat());
			capsule.SetMargin2(node.Attributes["Margin2"].ParseFloat());
			capsule.SetThickness(node.Attributes["Radius1"].ParseFloat() * 2f);
			capsule.CreateUI(modelObject.GetModel().GetGameObject().transform).Render();
			modelObject.GetCapsules().Add(capsule);
		}
	}

	private static void ParseTriangle(ModelObject modelObject, XmlNode node)
	{
		string value = node.Attributes["Node1"].Value;
		ModelNode firstNode = modelObject.GetNodeByName(value);
		if (firstNode == null)
		{
			return;
		}
		value = node.Attributes["Node2"].Value;
		ModelNode secondNode = modelObject.GetNodeByName(value);
		if (secondNode != null)
		{
			value = node.Attributes["Node3"].Value;
			ModelNode thirdNode = modelObject.GetNodeByName(value);
			if (thirdNode != null)
			{
				Triangle item = new Triangle(firstNode, secondNode, thirdNode, node.Name);
				modelObject.GetTriangles().Add(item);
				modelObject.GetModel()._MeshRender.get_Base().AddTriangle(firstNode, secondNode, thirdNode, node.Name);
			}
		}
	}

	private static void ParseMacroNodeWeights(ModelMacroNode macroNode, XmlNode node)
	{
		if (macroNode.GetNodeType() == ModelNode.NodeType.MacroNode)
		{
			ParseChildNodeWeights(macroNode.NamedWeights, node, true);
		}
	}

	private static void ParseChildNodeWeights(List<global::Pair<string, float>> weights, XmlNode node, bool hasWeights)
	{
		int num = node.Attributes["NodesCount"].ParseInt();
		if (0 >= num)
		{
			return;
		}
		string empty = string.Empty;
		string empty2 = string.Empty;
		weights.Capacity = num;
		for (int i = 0; i < num; i++)
		{
			empty2 = (i + 1).ToString();
			empty = "ChildNode";
			empty += empty2;
			string childNodeName = node.Attributes[empty].GetStringOrDefault(string.Empty);
			float weight = 0f;
			if (hasWeights)
			{
				empty = "LCC";
				empty += empty2;
				weight = node.Attributes[empty].ParseFloat();
			}
			weights.Add(new global::Pair<string, float>(childNodeName, weight));
		}
	}

	private static void LinkPairNodes(ModelObject modelObject)
	{
		List<ModelNode> list = modelObject.GetAllNodes();
		List<global::Pair<int, int>> list2 = modelObject.GetPairNodeIds();
		foreach (global::Pair<int, int> item in list2)
		{
			ModelNode firstNode = list[item.First];
			ModelNode pairNode = list[item.Second];
			firstNode.SetPairNode(pairNode);
			pairNode.SetPairNode(firstNode);
		}
	}
}
