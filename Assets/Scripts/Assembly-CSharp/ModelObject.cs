using System.Collections.Generic;

public class ModelObject
{
	private class ModelNodes
	{
		public ModelNode Pivot;

		public List<ModelNode> PlainNodes = new List<ModelNode>();

		public List<ModelMacroNode> MacroNodes = new List<ModelMacroNode>();

		public List<ModelNode> AllNodes = new List<ModelNode>();

		public Dictionary<string, ModelNode> NodesByName = new Dictionary<string, ModelNode>();

		public List<ModelNode> CenterOfMassNodes = new List<ModelNode>();
	}

	private class ModelEdges
	{
		public List<ModelEdge> StructuralEdges = new List<ModelEdge>();

		public List<ModelEdge> MuscleEdges = new List<ModelEdge>();

		public List<ModelEdge> CollisionEdges = new List<ModelEdge>();

		public List<ModelEdge> AllEdges = new List<ModelEdge>();
	}

	private class Figures
	{
		public List<Capsule> Capsules = new List<Capsule>();

		public List<Triangle> Triangles = new List<Triangle>();
	}

	private class AdditionalData
	{
		public List<global::Pair<int, int>> PairNodeIds = new List<global::Pair<int, int>>();

		public List<string> FileNames = new List<string>();
	}

	private ModelNodes nodeData = new ModelNodes();

	private ModelEdges edgeData = new ModelEdges();

	private Figures figureData = new Figures();

	private AdditionalData additionalData = new AdditionalData();

	private ModelNode centerOfMassNode;

	private float _ModelWeight;

	private int _NodesCount;

	private bool _IsShock;

	private Model _Model;

	private string _PivotName;

	public ModelNode CenterOfMassNode
	{
		get
		{
			return GetCenterOfMassNode();
		}
	}

	public float TotalWeight
	{
		get
		{
			return GetTotalWeight();
		}
	}

	public int NodeCount
	{
		get
		{
			return GetNodeCount();
		}
		set
		{
			set_NodesCount(value);
		}
	}

	public bool ShockActive
	{
		get
		{
			return IsShock();
		}
		set
		{
			SetShock(value);
		}
	}

	public Model OwnerModel
	{
		get
		{
			return GetModel();
		}
		set
		{
			SetModel(value);
		}
	}

	public Vector3f CenterOfMassPosition
	{
		get
		{
			return GetCenterOfMassPosition();
		}
	}

	public ModelNode PivotNode
	{
		get
		{
			return GetPivotNode();
		}
	}

	public List<ModelNode> PlainNodes
	{
		get
		{
			return GetPlainNodes();
		}
	}

	public List<ModelMacroNode> MacroNodes
	{
		get
		{
			return GetMacroNodes();
		}
	}

	public List<ModelNode> AllNodes
	{
		get
		{
			return GetAllNodes();
		}
	}

	public Dictionary<string, ModelNode> NodesByName
	{
		get
		{
			return GetNodesByName();
		}
	}

	private List<ModelNode> MassNodes
	{
		get
		{
			return GetMassNodes();
		}
	}

	public List<ModelEdge> CollisionEdges
	{
		get
		{
			return GetCollisionEdges();
		}
	}

	public List<ModelEdge> StructuralEdges
	{
		get
		{
			return GetStructuralEdges();
		}
	}

	public List<ModelEdge> MuscleEdges
	{
		get
		{
			return GetMuscleEdges();
		}
	}

	public List<ModelEdge> AllEdges
	{
		get
		{
			return GetAllEdges();
		}
	}

	public List<Capsule> Capsules
	{
		get
		{
			return GetCapsules();
		}
	}

	public List<Triangle> Triangles
	{
		get
		{
			return GetTriangles();
		}
	}

	public List<global::Pair<int, int>> PairNodeIds
	{
		get
		{
			return GetPairNodeIds();
		}
	}

	public Vector3f PreviousCenterOfMass
	{
		get
		{
			return GetPreviousCenterOfMass();
		}
	}

	public ModelObject()
	{
		centerOfMassNode = new ModelNode("_CenterOfMass_");
		_ModelWeight = 0f;
		_NodesCount = 0;
		_IsShock = false;
		_Model = null;
		nodeData.Pivot = null;
		_PivotName = GameUtils.PivotNodeName;
	}

	public ModelNode GetCenterOfMassNode()
	{
		return centerOfMassNode;
	}

	public float GetTotalWeight()
	{
		return _ModelWeight;
	}

	public int GetNodeCount()
	{
		return _NodesCount;
	}

	public void set_NodesCount(int value)
	{
		_NodesCount = value;
	}

	public bool IsShock()
	{
		return _IsShock;
	}

	public void SetShock(bool value)
	{
		_IsShock = value;
	}

	public Model GetModel()
	{
		return _Model;
	}

	public void SetModel(Model value)
	{
		_Model = value;
	}

	public Vector3f GetCenterOfMassPosition()
	{
		return GetCenterOfMassNode().GetStart();
	}

	public ModelNode GetPivotNode()
	{
		return nodeData.Pivot;
	}

	public List<ModelNode> GetPlainNodes()
	{
		return nodeData.PlainNodes;
	}

	public List<ModelMacroNode> GetMacroNodes()
	{
		return nodeData.MacroNodes;
	}

	public List<ModelNode> GetAllNodes()
	{
		return nodeData.AllNodes;
	}

	public Dictionary<string, ModelNode> GetNodesByName()
	{
		return nodeData.NodesByName;
	}

	private List<ModelNode> GetMassNodes()
	{
		return (nodeData.CenterOfMassNodes.Count == 0) ? nodeData.AllNodes : nodeData.CenterOfMassNodes;
	}

	// best guess for name
	public List<ModelEdge> GetCollisionEdges()
	{
		return edgeData.CollisionEdges;
	}

	public List<ModelEdge> GetStructuralEdges()
	{
		return edgeData.StructuralEdges;
	}

	public List<ModelEdge> GetMuscleEdges()
	{
		return edgeData.MuscleEdges;
	}

	public List<ModelEdge> GetAllEdges()
	{
		return edgeData.AllEdges;
	}

	public List<Capsule> GetCapsules()
	{
		return figureData.Capsules;
	}

	public List<Triangle> GetTriangles()
	{
		return figureData.Triangles;
	}

	public List<global::Pair<int, int>> GetPairNodeIds()
	{
		return additionalData.PairNodeIds;
	}

	public static Vector3f GetNodesMidpoint(ModelNode LHBNIMGFKIB, ModelNode AAOIAEJJINO)
	{
		return Vector3f.Middle(LHBNIMGFKIB.GetStart(), AAOIAEJJINO.GetStart());
	}

	public int GetNodeIDByPairName(int index)
	{
		List<global::Pair<int, int>> list = GetPairNodeIds();
		foreach (global::Pair<int, int> item in list)
		{
			if (index == item.First)
			{
				return item.Second;
			}
			if (index == item.Second)
			{
				return item.First;
			}
		}
		return -1;
	}

	public ModelNode FindNodeAtPoint(Vector2f MGMMDGFPBLP, float PKFOEAEPOAF)
	{
		ModelNode result = null;
		PKFOEAEPOAF *= PKFOEAEPOAF;
		List<ModelNode> list = GetAllNodes();
		foreach (ModelNode item in list)
		{
			Vector3f lHBNIMGFKIB = item.GetStart();
			if (Vector2f.DistanceSquared2D(lHBNIMGFKIB, MGMMDGFPBLP) < PKFOEAEPOAF)
			{
				result = item;
				break;
			}
		}
		return result;
	}

	public ModelNode GetNodeByName(string name)
	{
		if (nodeData.Pivot != null && name == _PivotName)
		{
			return nodeData.Pivot;
		}
		ModelNode value = null;
		if (nodeData.NodesByName.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public ModelNode GetNodeByNameOrParent(string name)
	{
		if (nodeData.Pivot != null && name == _PivotName)
		{
			return nodeData.Pivot;
		}
		ModelNode lCDGOCIAIDK = GetNodeByName(name);
		if (lCDGOCIAIDK != null)
		{
			return lCDGOCIAIDK;
		}
		if (GetModel() != null && GetModel().GetParentModel() != null)
		{
			lCDGOCIAIDK = GetModel().GetParentModel().GetBodyObject().GetNodeByName(name);
		}
		return lCDGOCIAIDK;
	}

	// best guess for name
	public ModelNode FindNodeOrParent(string name)
	{
		return GetNodeByNameOrParent(name);
	}

	public int GetNodeIDByName(string name)
	{
		for (int i = 0; i < nodeData.AllNodes.Count; i++)
		{
			if (name == nodeData.AllNodes[i].GetName())
			{
				return i;
			}
		}
		return -1;
	}

	public ModelEdge GetEdgeByName(string name)
	{
		foreach (ModelEdge item in edgeData.AllEdges)
		{
			if (name == item.get_Name())
			{
				return item;
			}
		}
		return null;
	}

	public void CalculateTotalWeight()
	{
		_ModelWeight = 0f;
		List<ModelNode> list = GetMassNodes();
		foreach (ModelNode item in list)
		{
			_ModelWeight += item.GetWeight();
		}
	}

	public void UpdateCenterOfMass()
	{
		Vector3f eMAFACPEPDK = new Vector3f();
		Vector3f eMAFACPEPDK2 = new Vector3f();
		Vector3f bAINMLLIKOL = new Vector3f(centerOfMassNode.GetStart());
		List<ModelNode> list = GetMassNodes();
		centerOfMassNode.GetStart().Reset();
		foreach (ModelNode item in list)
		{
			eMAFACPEPDK2.Set(item.GetStart());
			eMAFACPEPDK2.Multiply(item.GetWeight());
			eMAFACPEPDK.Add(eMAFACPEPDK2);
		}
		eMAFACPEPDK.Multiply(1f / _ModelWeight);
		centerOfMassNode.SetStart(eMAFACPEPDK);
		centerOfMassNode.SetEnd(bAINMLLIKOL);
	}

	public Vector3f GetPreviousCenterOfMass()
	{
		return centerOfMassNode.GetEnd();
	}

	public void SetModelPosition(Vector3f MGMMDGFPBLP, ModelNode NFADOLIKJEA = null)
	{
		if (NFADOLIKJEA == null)
		{
			NFADOLIKJEA = GetNodeByName(_PivotName);
		}
		if (NFADOLIKJEA == null || 0 >= nodeData.AllNodes.Count)
		{
			return;
		}
		Vector3f bEHOPOPCJGB = new Vector3f(Vector3f.op_Subtraction(MGMMDGFPBLP, NFADOLIKJEA.GetStart()));
		Vector3f bEHOPOPCJGB2 = new Vector3f(Vector3f.op_Subtraction(MGMMDGFPBLP, NFADOLIKJEA.GetEnd()));
		foreach (ModelNode item in nodeData.AllNodes)
		{
			item.GetStart().Add(bEHOPOPCJGB);
			item.GetEnd().Add(bEHOPOPCJGB2);
		}
	}

	public void AlignToFrame(List<Vector3f> KPLANIHPMED)
	{
		int index = GetNodeIDByName(_PivotName);
		Vector3f nBMEGFBPGFE = GetPivotNode().GetStart();
		Vector3f eMAFACPEPDK = Vector3f.op_Subtraction(nBMEGFBPGFE, KPLANIHPMED[index]);
		eMAFACPEPDK.SetY(0f);
		int i = 0;
		for (int count = KPLANIHPMED.Count; i < count; i++)
		{
			nodeData.AllNodes[i].SetStart(Vector3f.op_Addition(KPLANIHPMED[i], eMAFACPEPDK));
			nodeData.AllNodes[i].SetEnd(Vector3f.op_Addition(KPLANIHPMED[i], eMAFACPEPDK));
		}
	}

	public void AlignToFrame(List<Vector3f> KPLANIHPMED, ModelNode AECCPADGGPG)
	{
		int index = AECCPADGGPG.GetID();
		Vector3f nBMEGFBPGFE = AECCPADGGPG.GetStart();
		Vector3f eMAFACPEPDK = Vector3f.op_Subtraction(nBMEGFBPGFE, KPLANIHPMED[index]);
		eMAFACPEPDK.SetY(0f);
		int i = 0;
		for (int count = KPLANIHPMED.Count; i < count; i++)
		{
			nodeData.AllNodes[i].SetStart(Vector3f.op_Addition(KPLANIHPMED[i], eMAFACPEPDK));
			nodeData.AllNodes[i].SetEnd(Vector3f.op_Addition(KPLANIHPMED[i], eMAFACPEPDK));
		}
	}

	public void TranslateAllNodes(Vector3f OPNPKNEOALJ)
	{
		foreach (ModelNode item in nodeData.AllNodes)
		{
			item.GetStart().Add(OPNPKNEOALJ);
			item.GetEnd().Add(OPNPKNEOALJ);
		}
		UpdateCenterOfMass();
	}

	public void FindPivotNode()
	{
		ModelNode lCDGOCIAIDK = GetNodeByName(_PivotName);
		if (lCDGOCIAIDK != null)
		{
			nodeData.Pivot = lCDGOCIAIDK;
		}
	}

	public void SetFileNames(List<string> CBHAEPCLDFG)
	{
		additionalData.FileNames.AddRange(CBHAEPCLDFG);
	}

	public void ResolveMacroNodeWeights()
	{
		int count = nodeData.MacroNodes.Count;
		for (int i = 0; i < count; i++)
		{
			ModelMacroNode gDNAJOODAGP = nodeData.MacroNodes[i];
			List<global::Pair<string, float>> lMPPCKACMNB = gDNAJOODAGP.NamedWeights;
			if (lMPPCKACMNB == null)
			{
				continue;
			}
			foreach (global::Pair<string, float> item in lMPPCKACMNB)
			{
				ModelNode lCDGOCIAIDK = GetNodeByName(item.First);
				if (lCDGOCIAIDK != null)
				{
					gDNAJOODAGP.AddNodeWeight(lCDGOCIAIDK, item.Second);
					continue;
				}
				GameLog.Error("Nodes '{0}' for macronode '{1}' was not found", item.First, gDNAJOODAGP.GetName());
			}
			gDNAJOODAGP.NamedWeights = null;
		}
	}

	public void UpdateMacroNodes()
	{
		int count = nodeData.MacroNodes.Count;
		for (int i = 0; i < count; i++)
		{
			nodeData.MacroNodes[i].UpdateFromWeights();
		}
	}

	public void Clear()
	{
		edgeData.StructuralEdges.Clear();
		edgeData.MuscleEdges.Clear();
		edgeData.CollisionEdges.Clear();
		edgeData.AllEdges.Clear();
		figureData.Capsules.Clear();
		figureData.Triangles.Clear();
		additionalData.PairNodeIds.Clear();
		nodeData.PlainNodes.Clear();
		nodeData.MacroNodes.Clear();
		if (nodeData.Pivot != null)
		{
			nodeData.Pivot = null;
		}
		nodeData.AllNodes.Clear();
		_Model = null;
	}

	public void Reset()
	{
		SetShock(false);
		ModelReloader.Reload(this, additionalData.FileNames);
	}

	public void ResetNodeVelocities()
	{
		foreach (ModelNode item in nodeData.AllNodes)
		{
			item.SetEnd();
		}
	}

	public void BuildPairNodes()
	{
		LinkPairNodes(GetAllNodes(), additionalData.PairNodeIds);
	}

	public void RestoreDefaultPhysics()
	{
		foreach (ModelNode item in nodeData.AllNodes)
		{
			item.RestoreDefaultPhysics();
		}
	}

	public void AddCenterOfMassNodes(List<global::Pair<string, float>> MFIEGKAMKNJ)
	{
		foreach (global::Pair<string, float> item in MFIEGKAMKNJ)
		{
			ModelNode lCDGOCIAIDK = GetNodeByName(item.First);
			if (lCDGOCIAIDK == null)
			{
				GameLog.Error("ModelObject::addComNodes - no node with name: {0}", item.First);
			}
			nodeData.CenterOfMassNodes.Add(lCDGOCIAIDK);
		}
	}

	private void LinkPairNodes(List<ModelNode> nodes, List<global::Pair<int, int>> OEMALIFPGPO)
	{
		OEMALIFPGPO.Clear();
		foreach (ModelNode item in nodes)
		{
			string text = item.GetName();
			string text2 = text.Substring(0, text.Length - 2);
			string text3 = text.Substring(text.Length - 2, 2);
			if (!(text3 == "_1"))
			{
				continue;
			}
			string text4 = text2 + "_2";
			foreach (ModelNode item2 in nodes)
			{
				string text5 = item2.GetName();
				if (text5 == text4)
				{
					int gBCLEDJAOBM = item.GetID();
					int pOFHDGJAFMP = item2.GetID();
					OEMALIFPGPO.Add(new global::Pair<int, int>(gBCLEDJAOBM, pOFHDGJAFMP));
					item.SetPairNode(item2);
					item2.SetPairNode(item);
					break;
				}
			}
		}
	}
}
