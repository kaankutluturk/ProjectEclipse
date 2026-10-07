using System.Xml;
using UnityEngine;

public class DistancePointFollow : DistancePoint
{
	private bool follow;

	private bool isPositionComputed;

	private Vector3 _ComputedPosition;

	public DistancePointFollow()
	{
		isPositionComputed = false;
	}

	public DistancePointFollow(XmlNode node)
		: base(node)
	{
		isPositionComputed = false;
	}

	public override void Create(XmlNode node)
	{
		base.Create(node);
		follow = node.Attributes["Follow"].ParseBool();
	}

	public override float GetX(ModelConditions conditions)
	{
		UpdateComputedPosition(conditions);
		if (!conditions.HasOther)
		{
			return 0f;
		}
		return _ComputedPosition.x;
	}

	public override float GetY(ModelConditions conditions)
	{
		UpdateComputedPosition(conditions);
		if (!conditions.HasOther)
		{
			return 0f;
		}
		return -1f * _ComputedPosition.y;
	}

	public override Vector3 GetPosition(ModelConditions conditions)
	{
		UpdateComputedPosition(conditions);
		return _ComputedPosition;
	}

	private void UpdateComputedPosition(ModelConditions conditions)
	{
		if (follow || !isPositionComputed)
		{
			_ComputedPosition = base.GetPosition(conditions);
			isPositionComputed = true;
		}
	}
}
