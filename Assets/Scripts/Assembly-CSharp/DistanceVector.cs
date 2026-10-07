using System.Xml;
using UnityEngine;

public class DistanceVector
{
	private bool _Exists;

	private DistancePointFollow fromPoint = new DistancePointFollow();

	private DistancePointFollow toPoint = new DistancePointFollow();

	private Vector2f cachedDistance;

	public DistanceVector()
	{
		_Exists = false;
	}

	public DistanceVector(XmlNode node)
	{
		Parse(node);
	}

	public void Parse(XmlNode node)
	{
		fromPoint.Create(node["From"]);
		toPoint.Create(node["To"]);
		_Exists = true;
	}

	public Vector2f GetVector(ModelConditions conditions)
	{
		if (_Exists)
		{
			Vector3 vector = fromPoint.GetPosition(conditions);
			Vector3 vector2 = toPoint.GetPosition(conditions);
			cachedDistance = new Vector2f(vector2.x - vector.x, vector2.y - vector.y);
		}
		else
		{
			cachedDistance = new Vector2f();
		}
		return cachedDistance;
	}

	public void UpdateNodes(ModelObject rootModel, bool isPlayer, ModelNode pivotNode, bool isChildPoint, ModelObject childModel = null)
	{
		fromPoint.UpdateNode(rootModel, isPlayer, pivotNode, isChildPoint, childModel);
		toPoint.UpdateNode(rootModel, isPlayer, pivotNode, isChildPoint, childModel);
	}

	public void ClearChildPoints()
	{
		fromPoint.ClearChildPoints();
		toPoint.ClearChildPoints();
	}
}
