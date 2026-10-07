using System.Collections.Generic;

public partial class ModelMacroNode : ModelNode
{
	// best guess for name
	public List<global::Pair<string, float>> NamedWeights = new List<global::Pair<string, float>>();

	// best guess for name
	private List<global::Pair<ModelNode, float>> _nodeWeights = new List<global::Pair<ModelNode, float>>();

	public List<global::Pair<ModelNode, float>> NodeWeights
	{
		get
		{
			return GetNodeWeights();
		}
	}

	public ModelMacroNode(string name, Vector3f OBLEMIHLFII)
		: base(name, OBLEMIHLFII)
	{
		SetType(NodeType.MacroNode);
	}

	public ModelMacroNode(ModelMacroNode source)
		: base(source)
	{
		_nodeWeights = new List<global::Pair<ModelNode, float>>(source._nodeWeights);
		SetType(NodeType.MacroNode);
	}

	public List<global::Pair<ModelNode, float>> GetNodeWeights()
	{
		return _nodeWeights;
	}

	public void AddNodeWeight(ModelNode node, float weight)
	{
		_nodeWeights.Add(new global::Pair<ModelNode, float>(node, weight));
	}

	public void UpdateFromWeights()
	{
		if (_skipMacroUpdate)
		{
			_skipMacroUpdate = false;
			return;
		}
		_End.Set(_Start);
		_Start.Reset();
		if (UpdateSkinBindings()) return;
		global::Pair<ModelNode, float> nodeWeight = null;
		int count = _nodeWeights.Count;
		for (int i = 0; i < count; i++)
		{
			nodeWeight = _nodeWeights[i];
			_Start.AddScaledXY(nodeWeight.First.GetStart(), nodeWeight.Second);
		}
	}
}
