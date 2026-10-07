using System.Collections.Generic;
using UnityEngine;

public class ModelPhysics
{
	public enum PhysicsEventType
	{
		onFalling = 0
	}

	private float _FrictionForce;

	private int _Iterative;

	private float wallLeftX;

	private float wallRightX;

	private ModelObject _ModelObject;

	private bool _IsPhysics;

	private List<string> _Names = new List<string>();

	private int _Frame;

	public bool PhysicsEnabled
	{
		get
		{
			return IsPhysics();
		}
	}

	public List<string> Names
	{
		get
		{
			return GetNames();
		}
	}

	public int CurrentFrame
	{
		get
		{
			return GetFrame();
		}
	}

	public float GravityStep
	{
		get
		{
			return GetGravityStep();
		}
	}

	public ModelPhysics(ModelObject modelObject)
	{
		wallLeftX = 0f;
		wallRightX = 0f;
		_ModelObject = modelObject;
		_IsPhysics = false;
		_Frame = 0;
		_Iterative = PhysicsController.GetIterativeProcess();
		_FrictionForce = PhysicsController.GetFrictionForce();
	}

	public bool IsPhysics()
	{
		return _IsPhysics;
	}

	public List<string> GetNames()
	{
		return _Names;
	}

	public int GetFrame()
	{
		return _Frame;
	}

	public void Render()
	{
		TimeStep();
		IterativeProcess();
		if (_IsPhysics)
		{
			_Frame++;
		}
	}

	public void SetWallShift(float leftX, float rightX)
	{
		wallLeftX = leftX;
		wallRightX = rightX;
	}

	public float GetGravityStep()
	{
		return PhysicsController.GetGravity() / (float)(GameUtils.GetSlowMode() * GameUtils.GetSlowMode());
	}

	public void Start(List<string> nodeNames)
	{
		_IsPhysics = true;
		_Frame = 0;
		_Names.Clear();
		if (nodeNames != null)
		{
			_Names.AddRange(nodeNames);
		}
		_ModelObject.RestoreDefaultPhysics();
	}

	public void Stop()
	{
		_IsPhysics = false;
		_Frame = 0;
	}

	public void IterativeProcess()
	{
		bool flag = _ModelObject.IsShock();
		List<ModelNode> list = _ModelObject.GetAllNodes();
		List<ModelEdge> list2 = _ModelObject.GetAllEdges();
		ModelNode node = null;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			node = list[i];
			bool isPhysicsActive = node.IsNode() && !node.IsFixedAndIsNotNode() && (_IsPhysics || node.IsPhysics() || (flag && node.IsShock()));
			node.SetPhysicsActive(isPhysicsActive);
		}
		for (int j = 0; j < _Iterative; j++)
		{
			ModelEdge edge = null;
			count = list2.Count;
			for (int k = 0; k < count; k++)
			{
				edge = list2[k];
				if (!flag || !edge.GetIsShock())
				{
					IterativeLine(edge);
				}
			}
		}
	}

	public void ChangeSpeed(float speed)
	{
		List<ModelNode> list = _ModelObject.GetAllNodes();
		foreach (ModelNode item in list)
		{
			if (!item.IsFixedAndIsNotNode() && (_IsPhysics || item.IsPhysics() || (_ModelObject.IsShock() && item.IsShock())))
			{
				item.ChangeSpeed(speed);
			}
		}
	}

	private void TimeStep()
	{
		List<ModelNode> list = _ModelObject.GetAllNodes();
		ModelNode node = null;
		for (int i = 0; i < list.Count; i++)
		{
			node = list[i];
			if (!node.IsFixedAndIsNotNode() && (_IsPhysics || node.IsPhysics() || (_ModelObject.IsShock() && node.IsShock())))
			{
				node.TimeStep(GetGravityStep());
			}
		}
	}

	private void IterativeLine(ModelEdge Edge)
	{
		ModelNode startNode = Edge.GetStartNode();
		ModelNode endNode = Edge.GetEndNode();
		if (startNode.IsPhysicsActive())
		{
			IterativeNode(startNode);
			if (endNode.IsPhysicsActive())
			{
				IterativeNode(endNode);
			}
			Edge.Iterative();
		}
		else if (endNode.IsPhysicsActive())
		{
			IterativeNode(endNode);
			Edge.Iterative();
		}
	}

	private void IterativeNode(ModelNode node)
	{
		Vector3f nodePosition = node.GetStart();
		if (nodePosition.GetY() >= 0f)
		{
			GetFrictionForce(node);
		}
		if (wallLeftX != wallRightX)
		{
			if (nodePosition.GetX() < wallLeftX)
			{
				nodePosition.SetX(wallLeftX);
			}
			else if (wallRightX < nodePosition.GetX())
			{
				nodePosition.SetX(wallRightX);
			}
		}
	}

	private void GetFrictionForce(ModelNode node)
	{
		if (node.IsCollisible() && wallLeftX != wallRightX)
		{
			Vector3f endPosition = node.GetEnd();
			Vector3f startPosition = node.GetStart();
			float num = startPosition.GetX() - endPosition.GetX();
			float num2 = startPosition.GetZ() - endPosition.GetZ();
			float num3 = num * num + num2 * num2;
			float num4 = startPosition.GetY() * _FrictionForce;
			startPosition.SetX(endPosition.GetX());
			startPosition.SetY(0f);
			startPosition.SetZ(endPosition.GetZ());
			if (num4 * num4 < num3)
			{
				num4 = 1f - num4 / Mathf.Sqrt(num3);
				startPosition.Add(num * num4, 0f, num2 * num4);
			}
		}
	}
}
