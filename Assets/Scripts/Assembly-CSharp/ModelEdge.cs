using Nekki.SF2.Core.Fights.Renders.Model;
using UnityEngine;

public class ModelEdge : Segment3D
{
	public EquationLine LineEquation = new EquationLine();

	private string _Name;

	private string _BodyPart;

	private string defense;

	private EdgeType _Type;

	private EdgeSubType subType;

	private ModelNode StartNode;

	private ModelNode EndNode;

	private float length;

	private float collisionRadius;

	private float startMargin;

	private float endMargin;

	private int _Collisible;

	private bool hasBlood;

	private bool _IsShock;

	private Vector3f collisionStart = new Vector3f();

	private Vector3f collisionEnd = new Vector3f();

	public string BodyPart
	{
		get
		{
			return GetBodyPart();
		}
		set
		{
			SetBodyPart(value);
		}
	}

	public string Defense
	{
		get
		{
			return GetDefense();
		}
		set
		{
			SetDefense(value);
		}
	}

	public EdgeSubType SubType
	{
		get
		{
			return GetSubType();
		}
		set
		{
			SetSubType(value);
		}
	}

	public ModelNode FromNode
	{
		get
		{
			return GetStartNode();
		}
	}

	public ModelNode ToNode
	{
		get
		{
			return GetEndNode();
		}
	}

	public float EdgeLength
	{
		get
		{
			return GetLength();
		}
		set
		{
			set_Length(value);
		}
	}

	public float CollisionRadius
	{
		get
		{
			return GetCollisionRadius();
		}
		set
		{
			SetCollisionRadius(value);
		}
	}

	public float StartMargin
	{
		get
		{
			return GetStartMargin();
		}
		set
		{
			SetStartMargin(value);
		}
	}

	public float EndMargin
	{
		get
		{
			return GetEndMargin();
		}
		set
		{
			SetEndMargin(value);
		}
	}

	public int CollisibleLevel
	{
		get
		{
			return GetCollisible();
		}
		set
		{
			set_Collisible(value);
		}
	}

	public bool HasBlood
	{
		get
		{
			return GetHasBlood();
		}
		set
		{
			SetHasBlood(value);
		}
	}

	public bool ShockEdge
	{
		get
		{
			return GetIsShock();
		}
		set
		{
			set_IsShock(value);
		}
	}

	public Vector3f CollisionStart
	{
		get
		{
			return GetCollisionStart();
		}
	}

	public Vector3f CollisionEnd
	{
		get
		{
			return GetCollisionEnd();
		}
	}

	public new float NodeDistance
	{
		get
		{
			return GetNodeDistance();
		}
	}

	public Vector3f StartPosition
	{
		get
		{
			return GetStartPosition();
		}
	}

	public Vector3f EndPosition
	{
		get
		{
			return GetEndPosition();
		}
	}

	public ModelEdge(ModelNode startNode, ModelNode endNode)
	{
		AttachStartNode(startNode);
		AttachEndNode(endNode);
	}

	public string get_Name()
	{
		return _Name;
	}

	public void set_Name(string value)
	{
		_Name = value;
	}

	public string GetBodyPart()
	{
		return _BodyPart;
	}

	public void SetBodyPart(string value)
	{
		_BodyPart = value;
	}

	public string GetDefense()
	{
		return defense;
	}

	public void SetDefense(string value)
	{
		defense = value;
	}

	public EdgeType GetEdgeType()
	{
		return _Type;
	}

	public void SetType(EdgeType value)
	{
		_Type = value;
	}

	public EdgeSubType GetSubType()
	{
		return subType;
	}

	public void SetSubType(EdgeSubType value)
	{
		subType = value;
	}

	public ModelNode GetStartNode()
	{
		return StartNode;
	}

	public ModelNode GetEndNode()
	{
		return EndNode;
	}

	public float GetLength()
	{
		return length;
	}

	public void set_Length(float value)
	{
		length = value;
	}

	// best guess for name
	public float GetCollisionRadius()
	{
		return collisionRadius;
	}

	public void SetCollisionRadius(float value)
	{
		collisionRadius = value;
	}

	public void SetStartMargin(float value)
	{
		startMargin = value;
	}

	// best guess for name
	public float GetStartMargin()
	{
		return startMargin;
	}

	// best guess for name
	public float GetEndMargin()
	{
		return endMargin;
	}

	public void SetEndMargin(float value)
	{
		endMargin = value;
	}

	public int GetCollisible()
	{
		return _Collisible;
	}

	public void set_Collisible(int value)
	{
		_Collisible = value;
	}

	public bool GetHasBlood()
	{
		return hasBlood;
	}

	public void SetHasBlood(bool value)
	{
		hasBlood = value;
	}

	public bool GetIsShock()
	{
		return _IsShock;
	}

	public void set_IsShock(bool value)
	{
		_IsShock = value;
	}

	public Vector3f GetCollisionStart()
	{
		return collisionStart;
	}

	public Vector3f GetCollisionEnd()
	{
		return collisionEnd;
	}

	public EdgeRender CreateUI(Transform parent)
	{
		GameObject gameObject = new GameObject(_Name);
		EdgeRender edgeRender = gameObject.AddComponent<EdgeRender>();
		edgeRender.set_Edge(this);
		gameObject.transform.SetParent(parent, false);
		return edgeRender;
	}

	public void ReplaceStartNode(ModelNode node)
	{
		DetachStartNode();
		AttachStartNode(node);
	}

	public void ReplaceEndNode(ModelNode node)
	{
		DetachEndNode();
		AttachEndNode(node);
	}

	public void Iterative(Vector3f offset)
	{
		float num = StartNode.GetWeight();
		float num2 = EndNode.GetWeight();
		Vector3f startPoint = StartNode.GetStart();
		Vector3f eMAFACPEPDK2 = EndNode.GetStart();
		float num3 = length / Vector3f.Distance(startPoint, eMAFACPEPDK2);
		float num4 = (1f - num3) / (num + num2);
		float num5 = num * num4;
		float num6 = num2 * num4;
		offset.SetX(offset.GetX() * num3 + startPoint.GetX() * num5 + eMAFACPEPDK2.GetX() * num6);
		offset.SetY(offset.GetY() * num3 + startPoint.GetY() * num5 + eMAFACPEPDK2.GetY() * num6);
		offset.SetZ(offset.GetZ() * num3 + startPoint.GetZ() * num5 + eMAFACPEPDK2.GetZ() * num6);
	}

	public void Iterative()
	{
		if (StartNode.IsPhysicsActive() || EndNode.IsPhysicsActive())
		{
			float num = StartNode.GetWeight();
			float num2 = EndNode.GetWeight();
			Vector3f startPoint = StartNode.GetStart();
			Vector3f eMAFACPEPDK2 = EndNode.GetStart();
			float num3 = length / Vector3f.Distance(startPoint, eMAFACPEPDK2);
			float num4 = (1f - num3) / (num + num2);
			float num5 = num * num4;
			float num6 = num2 * num4;
			float offsetX = startPoint.GetX() * num5 + eMAFACPEPDK2.GetX() * num6;
			float offsetY = startPoint.GetY() * num5 + eMAFACPEPDK2.GetY() * num6;
			float offsetZ = startPoint.GetZ() * num5 + eMAFACPEPDK2.GetZ() * num6;
			if (StartNode.IsPhysicsActive())
			{
				startPoint.Multiply(num3);
				startPoint.Add(offsetX, offsetY, offsetZ);
			}
			if (EndNode.IsPhysicsActive())
			{
				eMAFACPEPDK2.Multiply(num3);
				eMAFACPEPDK2.Add(offsetX, offsetY, offsetZ);
			}
		}
	}

	public new float GetNodeDistance()
	{
		return Vector3f.Distance(StartNode.GetStart(), EndNode.GetStart());
	}

	public void UpdateCollisionGeometry()
	{
		UpdateCollisionPoints();
		Vector2f.BuildEquationLine(GetStartPosition(), GetEndPosition(), LineEquation);
	}

	public void UpdateCollisionPoints()
	{
		Vector3f startPosition = GetStartPosition();
		Vector3f endPosition = GetEndPosition();
		Vector3f.GetDivisionPoint3D(startPosition, endPosition, startMargin, collisionStart);
		Vector3f.GetDivisionPoint3D(startPosition, endPosition, 1f - endMargin, collisionEnd);
	}

	public Vector3f GetStartPosition()
	{
		return StartNode.GetStart();
	}

	public Vector3f GetEndPosition()
	{
		return EndNode.GetStart();
	}

	private void AttachStartNode(ModelNode node)
	{
		StartNode = node;
		SetStartReference(node.GetStart());
	}

	private void AttachEndNode(ModelNode node)
	{
		EndNode = node;
		SetEndReference(node.GetStart());
	}

	private void DetachStartNode()
	{
		StartNode = null;
	}

	private void DetachEndNode()
	{
		EndNode = null;
	}
}
