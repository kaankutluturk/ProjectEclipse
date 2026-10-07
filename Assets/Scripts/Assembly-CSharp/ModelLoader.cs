using System.Collections.Generic;
using System.Xml;

public class ModelLoader
{
	public class CacheModelDocuments
	{
		private Dictionary<string, XmlDocument> documents = new Dictionary<string, XmlDocument>();

		public XmlDocument GetDocument(string EFGLOMANJHN, string PMFEIPCHENB)
		{
			XmlDocument value = null;
			if (documents.TryGetValue(PMFEIPCHENB, out value))
			{
				return value;
			}
			value = XmlUtils.OpenXMLDocument(EFGLOMANJHN, PMFEIPCHENB, XmlUtils.XmlSourceMode.ForcedResourced);
			documents.Add(PMFEIPCHENB, value);
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

	public static void Load(ModelObject ACENLMONNPA, List<string> CBHAEPCLDFG)
	{
		if (CBHAEPCLDFG.Count == 0)
		{
			return;
		}
		XmlDocument xmlDocument = null;

        // Opt into authored body/skin contracts without changing archival model
        // parsing or first-definition bindings for recovered equipment.
        var parameters = ACENLMONNPA.GetModel()?.Parameters;
        if (parameters != null && parameters.HasEclipseAuthoredModels)
        {
            var authored = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var model in parameters.EclipseAuthoredModels()) if (IsAuthoredModel(model)) authored.Add(AuthoredModelPath(model));
            if (authored.Count != 0)
            {
                var documents = new List<XmlDocument>(CBHAEPCLDFG.Count);
                foreach (string path in CBHAEPCLDFG)
                    documents.Add(path == "assets/models/.xml" ? null : DocumentCache.GetDocument(SF2Paths.GetModelsPath(), path));
                Eclipse.Modding.ModCharacterGeometry.Validate(CBHAEPCLDFG, documents, authored);
            }
        }
		string text = "assets/models/.xml";
		foreach (string item in CBHAEPCLDFG)
		{
			if (!(item == text))
			{
				xmlDocument = DocumentCache.GetDocument(SF2Paths.GetModelsPath(), item);
				if (xmlDocument != null)
				{
					Parse(ACENLMONNPA, xmlDocument, parameters != null && parameters.HidesEclipseFigures(item));
					continue;
				}
				GameLog.Error("File '{0}' not found", item);
			}
		}
		ACENLMONNPA.FindPivotNode();
		ACENLMONNPA.CalculateTotalWeight();
		PostProcessNodes(ACENLMONNPA.GetAllNodes());
		ACENLMONNPA.SetFileNames(CBHAEPCLDFG);
		ACENLMONNPA.BuildPairNodes();
		ACENLMONNPA.ResolveMacroNodeWeights();
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

	private static void Parse(ModelObject ACENLMONNPA, XmlDocument EELFNMOHGJL, bool hideFigures = false)
	{
		XmlNode eELFNMOHGJL = EELFNMOHGJL["Scene"];
		if (!ParseNodes(ACENLMONNPA, eELFNMOHGJL))
		{
			GameLog.Write("Nodes was not parsed");
		}
		if (!ParseEdges(ACENLMONNPA, eELFNMOHGJL))
		{
			GameLog.Write("Edges was not parsed");
		}
		// An Eclipse look keeps hidden equipment's nodes and edges but draws no figures.
		if (hideFigures) return;
		if (!ParseCapsules(ACENLMONNPA, eELFNMOHGJL))
		{
			GameLog.Write("Capsules was not parsed");
		}
		if (!ParseTriangles(ACENLMONNPA, eELFNMOHGJL))
		{
			GameLog.Write("Triangles was not parsed");
		}
	}

	private static bool ParseNodes(ModelObject ACENLMONNPA, XmlNode EELFNMOHGJL)
	{
		XmlNode xmlNode = EELFNMOHGJL["Nodes"];
		if (xmlNode == null)
		{
			return true;
		}
		List<global::Pair<string, float>> mFIEGKAMKNJ = new List<global::Pair<string, float>>();
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			ParseNode(ACENLMONNPA, childNode, mFIEGKAMKNJ);
		}
		ACENLMONNPA.AddCenterOfMassNodes(mFIEGKAMKNJ);
		if (ACENLMONNPA.GetNodeCount() == 0)
		{
			ACENLMONNPA.set_NodesCount(ACENLMONNPA.GetAllNodes().Count);
		}
		return true;
	}

	private static bool ParseEdges(ModelObject ACENLMONNPA, XmlNode EELFNMOHGJL)
	{
		XmlNode xmlNode = EELFNMOHGJL["Edges"];
		if (xmlNode == null)
		{
			return true;
		}
		ACENLMONNPA.GetAllEdges().Capacity = ACENLMONNPA.GetAllEdges().Count + xmlNode.ChildNodes.Count;
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
				ParseEdge(ACENLMONNPA, childNode, empty);
			}
		}
		return true;
	}

	private static bool ParseCapsules(ModelObject ACENLMONNPA, XmlNode EELFNMOHGJL)
	{
		XmlNode xmlNode = EELFNMOHGJL["Figures"];
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			string value = childNode.Attributes["Type"].Value;
			if (value == "Capsule")
			{
				ParseCapsule(ACENLMONNPA, childNode);
			}
		}
		return true;
	}

	private static bool ParseTriangles(ModelObject ACENLMONNPA, XmlNode EELFNMOHGJL)
	{
		XmlNode xmlNode = EELFNMOHGJL["Figures"];
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			string value = childNode.Attributes["Type"].Value;
			if (value == "Triangle")
			{
				ParseTriangle(ACENLMONNPA, childNode);
			}
		}
		return true;
	}

	private static void ParseNode(ModelObject ACENLMONNPA, XmlNode node, List<global::Pair<string, float>> MFIEGKAMKNJ)
	{
		ModelNode Node = null;
		Vector3f Position = new Vector3f(node.Attributes["X"].ParseFloat(), 0f - node.Attributes["Y"].ParseFloat(), node.Attributes["Z"].ParseFloat());
		string value = node.Attributes["Type"].Value;
		string name = node.Name;
		// Shipped models repeat node names: DE's mdl_armor_forest_spirit repeats 49, and dual
		// weapons such as mdl_weapon_hunger redefine the skeleton's off-hand Weapon-Node*_2
		// attachment nodes. The first definition owns the name; a later one is not created, so
		// that file's edges bind to the existing (arm-attached) node instead of a loose copy.
		if (ACENLMONNPA.GetNodesByName().ContainsKey(name))
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
				MFIEGKAMKNJ.Clear();
				ParseChildNodeWeights(MFIEGKAMKNJ, node, false);
			}
			ACENLMONNPA.GetPlainNodes().Add(Node);
		}
		else if (value == "MacroNode" || value == "SkinnedNode")
		{
			Position.SetX(Position.GetX() * -1f);
			ModelMacroNode gDNAJOODAGP = new ModelMacroNode(name, Position);
			Node = gDNAJOODAGP;
			if (value == "SkinnedNode") gDNAJOODAGP.LoadSkinBindings(ACENLMONNPA, node);
			else ParseMacroNodeWeights(gDNAJOODAGP, node);
			ACENLMONNPA.GetMacroNodes().Add(gDNAJOODAGP);
		}
		if (Node != null)
		{
			Node.SetID(ACENLMONNPA.GetAllNodes().Count);
			Node.SetMass(node.Attributes["Mass"].ParseFloat());
			Node.SetFixed(node.Attributes["Fixed"].ParseBool());
			Node.SetVisible(node.Attributes["Visible"].ParseBool());
			Node.SetIsShock(node.Attributes["Shock"].ParseBool());
			Node.SetCollisible(node.Attributes["Collisible"].ParseBool());
			Node.SetWeak(node.Attributes["Weak"].ParseBool());
			ACENLMONNPA.GetAllNodes().Add(Node);
			ACENLMONNPA.GetNodesByName().Add(Node.GetName(), Node);
		}
	}

	private static void ParseEdge(ModelObject ACENLMONNPA, XmlNode node, string IMGCANJHPND)
	{
		ModelEdge nAKBKCDKEHF = null;
		ModelNode iLENLCMAMBH = ACENLMONNPA.GetNodeByNameOrParent(node.Attributes["End1"].Value);
		ModelNode bFDAHEHCAGK = ACENLMONNPA.GetNodeByNameOrParent(node.Attributes["End2"].Value);
		float bAINMLLIKOL = node.Attributes["Length"].ParseFloat();
		float bAINMLLIKOL2 = node.Attributes["Radius"].ParseFloat();
		float bAINMLLIKOL3 = node.Attributes["Margin1"].ParseFloat();
		float bAINMLLIKOL4 = node.Attributes["Margin2"].ParseFloat();
		string text = node.Attributes["Type"].GetStringOrDefault(string.Empty);
		string text2 = node.Attributes["SubType"].GetStringOrDefault(string.Empty);
		string bAINMLLIKOL5 = node.Attributes["BodyPart"].GetStringOrDefault(string.Empty);
		string bAINMLLIKOL6 = node.Attributes["Defense"].GetStringOrDefault(string.Empty);
		int num = node.Attributes["Collisible"].ParseInt();
		bool bAINMLLIKOL7 = node.Attributes["Blood"].ParseBool();
		bool bAINMLLIKOL8 = node.Attributes["Shock"].ParseBool();
		nAKBKCDKEHF = new ModelEdge(iLENLCMAMBH, bFDAHEHCAGK);
		nAKBKCDKEHF.set_Length(bAINMLLIKOL);
		nAKBKCDKEHF.set_Name(IMGCANJHPND);
		nAKBKCDKEHF.set_Collisible(num);
		nAKBKCDKEHF.SetBodyPart(bAINMLLIKOL5);
		nAKBKCDKEHF.SetDefense(bAINMLLIKOL6);
		nAKBKCDKEHF.SetHasBlood(bAINMLLIKOL7);
		nAKBKCDKEHF.set_IsShock(bAINMLLIKOL8);
		nAKBKCDKEHF.SetCollisionRadius(bAINMLLIKOL2);
		nAKBKCDKEHF.SetStartMargin(bAINMLLIKOL3);
		nAKBKCDKEHF.SetEndMargin(bAINMLLIKOL4);
		if (text == "Edge")
		{
			nAKBKCDKEHF.SetType(EdgeType.Edge);
			ACENLMONNPA.GetStructuralEdges().Add(nAKBKCDKEHF);
			if (num > 0)
			{
				ACENLMONNPA.GetCollisionEdges().Add(nAKBKCDKEHF);
			}
		}
		else
		{
			if (!(text == "Muscle"))
			{
				GameLog.Error("Wring type edge: {0}", text);
				return;
			}
			nAKBKCDKEHF.SetType(EdgeType.Muscle);
			ACENLMONNPA.GetMuscleEdges().Add(nAKBKCDKEHF);
		}
		if (text2 == "None")
		{
			nAKBKCDKEHF.SetSubType(EdgeSubType.None);
		}
		else if (text2 == "Blade")
		{
			nAKBKCDKEHF.SetSubType(EdgeSubType.Blade);
		}
		ACENLMONNPA.GetAllEdges().Add(nAKBKCDKEHF);
	}

	private static void ParseCapsule(ModelObject ACENLMONNPA, XmlNode node)
	{
		string value = node.Attributes["Edge"].Value;
		ModelEdge nAKBKCDKEHF = ACENLMONNPA.GetEdgeByName(value);
		if (nAKBKCDKEHF != null)
		{
			Capsule cOGLBFKLNFC = new Capsule(nAKBKCDKEHF);
			cOGLBFKLNFC.set_Name(node.Name);
			cOGLBFKLNFC.SetRadius1(node.Attributes["Radius1"].ParseFloat());
			cOGLBFKLNFC.SetRadius2(node.Attributes["Radius2"].ParseFloat());
			cOGLBFKLNFC.SetMargin1(node.Attributes["Margin1"].ParseFloat());
			cOGLBFKLNFC.SetMargin2(node.Attributes["Margin2"].ParseFloat());
			cOGLBFKLNFC.SetThickness(node.Attributes["Radius1"].ParseFloat() * 2f);
			cOGLBFKLNFC.CreateUI(ACENLMONNPA.GetModel().GetGameObject().transform).Render();
			ACENLMONNPA.GetCapsules().Add(cOGLBFKLNFC);
		}
	}

	private static void ParseTriangle(ModelObject ACENLMONNPA, XmlNode node)
	{
		string value = node.Attributes["Node1"].Value;
		ModelNode lCDGOCIAIDK = ACENLMONNPA.GetNodeByName(value);
		if (lCDGOCIAIDK == null)
		{
			return;
		}
		value = node.Attributes["Node2"].Value;
		ModelNode lCDGOCIAIDK2 = ACENLMONNPA.GetNodeByName(value);
		if (lCDGOCIAIDK2 != null)
		{
			value = node.Attributes["Node3"].Value;
			ModelNode lCDGOCIAIDK3 = ACENLMONNPA.GetNodeByName(value);
			if (lCDGOCIAIDK3 != null)
			{
				Triangle item = new Triangle(lCDGOCIAIDK, lCDGOCIAIDK2, lCDGOCIAIDK3, node.Name);
				ACENLMONNPA.GetTriangles().Add(item);
				ACENLMONNPA.GetModel()._MeshRender.get_Base().AddTriangle(lCDGOCIAIDK, lCDGOCIAIDK2, lCDGOCIAIDK3, node.Name);
			}
		}
	}

	private static void ParseMacroNodeWeights(ModelMacroNode AHJOLBKABMC, XmlNode node)
	{
		if (AHJOLBKABMC.GetNodeType() == ModelNode.NodeType.MacroNode)
		{
			ParseChildNodeWeights(AHJOLBKABMC.NamedWeights, node, true);
		}
	}

	private static void ParseChildNodeWeights(List<global::Pair<string, float>> NBAGKJAPCFD, XmlNode node, bool OGFKPCPEDAK)
	{
		int num = node.Attributes["NodesCount"].ParseInt();
		if (0 >= num)
		{
			return;
		}
		string empty = string.Empty;
		string empty2 = string.Empty;
		NBAGKJAPCFD.Capacity = num;
		for (int i = 0; i < num; i++)
		{
			empty2 = (i + 1).ToString();
			empty = "ChildNode";
			empty += empty2;
			string gBCLEDJAOBM = node.Attributes[empty].GetStringOrDefault(string.Empty);
			float pOFHDGJAFMP = 0f;
			if (OGFKPCPEDAK)
			{
				empty = "LCC";
				empty += empty2;
				pOFHDGJAFMP = node.Attributes[empty].ParseFloat();
			}
			NBAGKJAPCFD.Add(new global::Pair<string, float>(gBCLEDJAOBM, pOFHDGJAFMP));
		}
	}

	private static void LinkPairNodes(ModelObject ACENLMONNPA)
	{
		List<ModelNode> list = ACENLMONNPA.GetAllNodes();
		List<global::Pair<int, int>> list2 = ACENLMONNPA.GetPairNodeIds();
		foreach (global::Pair<int, int> item in list2)
		{
			ModelNode lCDGOCIAIDK = list[item.First];
			ModelNode lCDGOCIAIDK2 = list[item.Second];
			lCDGOCIAIDK.SetPairNode(lCDGOCIAIDK2);
			lCDGOCIAIDK2.SetPairNode(lCDGOCIAIDK);
		}
	}
}
