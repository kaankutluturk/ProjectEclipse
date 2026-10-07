public class Triangle
{
	private string _name;

	// best guess for name
	private ModelNode[] _nodes = new ModelNode[3];

	// Snapshot codecs preserve the array identity as well as its node references.
	internal ModelNode[] Nodes { get => _nodes; set => _nodes = value; }

	public ModelNode FirstNode
	{
		get
		{
			return GetFirstNode();
		}
		set
		{
			SetFirstNode(value);
		}
	}

	public ModelNode SecondNode
	{
		get
		{
			return GetSecondNode();
		}
		set
		{
			SetSecondNode(value);
		}
	}

	public ModelNode ThirdNode
	{
		get
		{
			return GetThirdNode();
		}
		set
		{
			SetThirdNode(value);
		}
	}

	public Triangle()
	{
		_nodes[0] = new ModelNode("tmp");
		_nodes[1] = new ModelNode("tmp");
		_nodes[2] = new ModelNode("tmp");
	}

	public Triangle(ModelNode firstNode, ModelNode secondNode, ModelNode thirdNode, string name)
	{
		_nodes[0] = firstNode;
		_nodes[1] = secondNode;
		_nodes[2] = thirdNode;
		_name = name;
	}

	public Triangle(Triangle source)
	{
		_nodes[0] = source._nodes[0];
		_nodes[1] = source._nodes[1];
		_nodes[2] = source._nodes[2];
		_name = source._name;
	}

	public string get_Name()
	{
		return _name;
	}

	public void set_Name(string value)
	{
		_name = value;
	}

	public ModelNode GetFirstNode()
	{
		return _nodes[0];
	}

	public void SetFirstNode(ModelNode value)
	{
		_nodes[0] = value;
	}

	public ModelNode GetSecondNode()
	{
		return _nodes[1];
	}

	public void SetSecondNode(ModelNode value)
	{
		_nodes[1] = value;
	}

	public ModelNode GetThirdNode()
	{
		return _nodes[2];
	}

	public void SetThirdNode(ModelNode value)
	{
		_nodes[2] = value;
	}

	public void CopyFrom(Triangle source)
	{
		_nodes[0].CopyFrom(source.GetFirstNode());
		_nodes[1].CopyFrom(source.GetSecondNode());
		_nodes[2].CopyFrom(source.GetThirdNode());
	}

	public void Reset()
	{
	}

	private void InitializeNodes()
	{
	}
}
