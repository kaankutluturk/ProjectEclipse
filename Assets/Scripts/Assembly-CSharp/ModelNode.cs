public partial class ModelNode
{
	public enum NodeType
	{
		Node = 0,
		MacroNode = 1
	}

	private ModelNode _PairNode;

	protected Vector3f _Start = new Vector3f();

	protected Vector3f _End = new Vector3f();

	private string _Name;

	private NodeType _Type;

	private int _Id;

	private float _Weight;

	private float _Attenuation;

	private bool _IsNode;

	private bool _IsFixed;

	private bool _IsCloth;

	private bool _IsPhysics;

	// best guess for name
	private bool _defaultPhysics;

    // Evidence Points that this is a Fixed Boolean but OBCDNOHNEEM is directly set from the model loader as Fixed so I have no clue which one is which
	// so I temporarily named it from the conditional statement.
    private bool _IsFixedAndNotNode;

	private bool _Visible;

	// No Clue what this is...
	// best guess for name
	private bool _physicsActive;

	private bool _IsShock;

	private bool _Collisible;

	private bool _Weak;

	// no clue what this is either
	// best guess for name
	protected bool _skipMacroUpdate;

	private static Vector3f _TimeStepVector = new Vector3f();

	public ModelNode PairNode
	{
		get
		{
			return GetPairNode();
		}
		set
		{
			SetPairNode(value);
		}
	}

	public Vector3f StartPosition
	{
		get
		{
			return GetStart();
		}
		set
		{
			SetStart(value);
		}
	}

	public Vector3f EndPosition
	{
		get
		{
			return GetEnd();
		}
		set
		{
			SetEnd(value);
		}
	}

	public int NodeId
	{
		get
		{
			return GetID();
		}
		set
		{
			SetID(value);
		}
	}

	public float Weight
	{
		get
		{
			return GetWeight();
		}
		set
		{
			SetMass(value);
		}
	}

	public float AttenuationValue
	{
		get
		{
			return GetAttenuation();
		}
		set
		{
			SetAttenuation(value);
		}
	}

	public bool IsPlainNode
	{
		get
		{
			return IsNode();
		}
		set
		{
			SetIsNode(value);
		}
	}

	public bool IsFixedNode
	{
		get
		{
			return IsFixed();
		}
		set
		{
			SetFixed(value);
		}
	}

	public bool IsClothNode
	{
		get
		{
			return IsCloth();
		}
		set
		{
			SetCloth(value);
		}
	}

	public bool IsPhysicsNode
	{
		get
		{
			return IsPhysics();
		}
	}

	public bool IsFixedOrMacro
	{
		get
		{
			return IsFixedAndIsNotNode();
		}
	}

	public bool IsVisibleNode
	{
		get
		{
			return IsVisible();
		}
		set
		{
			SetVisible(value);
		}
	}

	public bool PhysicsActive
	{
		get
		{
			return IsPhysicsActive();
		}
		set
		{
			SetPhysicsActive(value);
		}
	}

	public bool IsShockNode
	{
		get
		{
			return IsShock();
		}
		set
		{
			SetIsShock(value);
		}
	}

	public bool IsCollisibleNode
	{
		get
		{
			return IsCollisible();
		}
		set
		{
			SetCollisible(value);
		}
	}

	public bool IsWeakNode
	{
		get
		{
			return IsWeak();
		}
		set
		{
			SetWeak(value);
		}
	}

	public bool SkipMacroUpdate
	{
		get
		{
			return GetSkipMacroUpdate();
		}
		set
		{
			SetSkipMacroUpdate(value);
		}
	}

	public ModelNode(string name)
		: this(name, new Vector3f())
	{
	}

	public ModelNode(string name, Vector3f initialPosition)
	{
		_Start.Set(initialPosition);
		_End.Set(initialPosition);
		_Name = name;
		_Id = 0;
		_Weight = 0f;
		_Attenuation = 0f;
		_PairNode = null;
		_IsFixed = true;
		_IsCloth = false;
		_Visible = false;
		_physicsActive = false;
		_defaultPhysics = false;
		_skipMacroUpdate = false;
		SetType(NodeType.Node);
	}

	public ModelNode(ModelNode source)
	{
		_Name = source._Name;
		_Id = 0;
		_Weight = 0f;
		_Attenuation = 0f;
		_PairNode = null;
		_IsFixed = true;
		_IsCloth = false;
		_Visible = false;
		_physicsActive = false;
		_defaultPhysics = false;
		_skipMacroUpdate = false;
		CopyFrom(source);
	}

	public ModelNode GetPairNode()
	{
		return _PairNode;
	}

	public void SetPairNode(ModelNode value)
	{
		_PairNode = value;
	}

	public Vector3f GetStart()
	{
		return _Start;
	}

	public void SetStart(Vector3f value)
	{
		_Start.Set(value);
	}

	public Vector3f GetEnd()
	{
		return _End;
	}

	public void SetEnd(Vector3f value)
	{
		_End.Set(value);
	}

	public string GetName()
	{
		return _Name;
	}

	public NodeType GetNodeType()
	{
		return _Type;
	}

	protected void SetType(NodeType value)
	{
		_Type = value;
		_IsNode = value == NodeType.Node;
		_IsPhysics = _IsCloth && _IsNode;
		_IsFixedAndNotNode = _IsFixed || !_IsNode;
	}

	public int GetID()
	{
		return _Id;
	}

	public void SetID(int value)
	{
		_Id = value;
	}

	public float GetWeight()
	{
		return _Weight;
	}

	public void SetMass(float value)
	{
		_Weight = value;
	}

	public float GetAttenuation()
	{
		return _Attenuation;
	}

	public void SetAttenuation(float value)
	{
		_Attenuation = value;
	}

	public bool IsNode()
	{
		return _IsNode;
	}

	public void SetIsNode(bool value)
	{
		_IsNode = value;
	}

	public bool IsFixed()
	{
		return _IsFixed;
	}

	public void SetFixed(bool value)
	{
		_IsFixed = value;
		_IsFixedAndNotNode = _IsFixed || !_IsNode;
	}

	public bool IsCloth()
	{
		return _IsCloth;
	}

	public void SetCloth(bool value)
	{
		_IsCloth = value;
		if (value)
		{
			_IsPhysics = _IsCloth && _IsNode;
		}
		_IsPhysics = _IsCloth && _IsNode;
		_defaultPhysics = _IsPhysics;
	}

	public bool IsPhysics()
	{
		return _IsPhysics;
	}

	public bool IsFixedAndIsNotNode()
	{
		return _IsFixedAndNotNode;
	}

	public bool IsVisible()
	{
		return _Visible;
	}

	public void SetVisible(bool value)
	{
		_Visible = true;
	}

	public bool IsPhysicsActive()
	{
		return _physicsActive;
	}

	public void SetPhysicsActive(bool value)
	{
		_physicsActive = value;
	}

	public bool IsShock()
	{
		return _IsShock;
	}

	public void SetIsShock(bool value)
	{
		_IsShock = value;
	}

	public bool IsCollisible()
	{
		return _Collisible;
	}

	public void SetCollisible(bool value)
	{
		_Collisible = value;
	}

	public bool IsWeak()
	{
		return _Weak;
	}

	public void SetWeak(bool value)
	{
		_Weak = value;
	}

	public bool GetSkipMacroUpdate()
	{
		return _skipMacroUpdate;
	}

	public void SetSkipMacroUpdate(bool value)
	{
		_skipMacroUpdate = value;
	}

	public void CopyFrom(ModelNode source)
	{
		_Start.Set(source._Start);
		_End.Set(source._End);
		_Type = source._Type;
		_Id = source._Id;
		_Weight = source._Weight;
		_Attenuation = source._Attenuation;
		_IsNode = source._IsNode;
		_IsFixed = source._IsFixed;
		_IsCloth = source._IsCloth;
		_IsPhysics = source._IsPhysics;
		_IsFixedAndNotNode = source._IsFixedAndNotNode;
		_Visible = source._Visible;
		_physicsActive = source._physicsActive;
	}

	public void RestoreDefaultPhysics()
	{
		if (_IsNode)
		{
			if (_IsPhysics != _defaultPhysics)
			{
				int num = 0;
				num++;
			}
			_IsPhysics = _defaultPhysics;
		}
	}

	public void DisablePhysics()
	{
		if (_IsNode)
		{
			_IsPhysics = false;
		}
	}

	public void SetEnd()
	{
		_End.Set(_Start);
	}

	public void ChangeSpeed(float speed)
	{
		Vector3f offset = Vector3f.op_Subtraction(_Start, _End);
		if (_IsPhysics)
		{
		}
		_End.Set(Vector3f.op_Subtraction(_Start, offset));
	}

	public void TimeStep(float gravity)
	{
		_TimeStepVector.Set(_Start);
		_TimeStepVector.Subtract(_End);
		if (_IsPhysics)
		{
			_TimeStepVector.Multiply(1f - _Attenuation);
		}
		_TimeStepVector.Add(_Start);
		Vector3f stepVector = _TimeStepVector;
		stepVector.SetY(stepVector.GetY() + gravity);
		_End.Set(_Start);
		_Start.Set(_TimeStepVector);
	}

	public override string ToString()
	{
		return string.Format("ModelNode(name = [{0}] c_pos= [{1}])", _Name, _Start);
	}
}
