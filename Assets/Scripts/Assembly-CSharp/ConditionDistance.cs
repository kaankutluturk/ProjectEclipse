using System.Xml;
using UnityEngine;

public class ConditionDistance : ConditionAnimation
{
	private enum DistanceAxis
	{
		LENGTH_X = 0,
		LENGTH_Y = 1,
		LENGTH_FULL = 2
	}

	public const float nonlimit = 1000000f;

	private float _min;

	private float _max;

	private DistanceAxis _axis;

	private DistancePoint _from = new DistancePoint();

	private DistancePoint _to = new DistancePoint();

	public ConditionDistance(XmlNode node)
		: base(ConditionType.DISTANCE)
	{
		XmlAttribute xmlAttribute = node.Attributes["Axis"];
		_axis = ((xmlAttribute == null) ? DistanceAxis.LENGTH_FULL : ((!(xmlAttribute.Value == "X")) ? DistanceAxis.LENGTH_Y : DistanceAxis.LENGTH_X));
		_min = node.Attributes["Min"].ParseFloat(-1000000f);
		_max = node.Attributes["Max"].ParseFloat(1000000f);
		_from.Create(node["From"]);
		_to.Create(node["To"]);
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		float num = 0f;
		switch (_axis)
		{
		case DistanceAxis.LENGTH_X:
			num = _to.GetX(conditions) - _from.GetX(conditions);
			num *= (float)conditions.AnimationSign;
			break;
		case DistanceAxis.LENGTH_Y:
			num = _to.GetY(conditions) - _from.GetY(conditions);
			break;
		case DistanceAxis.LENGTH_FULL:
		{
			Vector3f eMAFACPEPDK = Vector3f.op_Implicit(_to.GetPosition(conditions));
			Vector3f eMAFACPEPDK2 = Vector3f.op_Implicit(_from.GetPosition(conditions));
			num = Mathf.Sqrt((eMAFACPEPDK.GetX() - eMAFACPEPDK2.GetX()) * (eMAFACPEPDK.GetX() - eMAFACPEPDK2.GetX()) + (eMAFACPEPDK.GetY() - eMAFACPEPDK2.GetY()) * (eMAFACPEPDK.GetY() - eMAFACPEPDK2.GetY()));
			break;
		}
		}
		bool flag = _min <= num && num <= _max;
		return (!IsNot) ? flag : (!flag);
	}

	public void UpdateNodes(ModelObject rootModel, bool isPlayer, ModelNode pivotNode, bool isChildPoint, ModelObject childModel = null)
	{
		_from.UpdateNode(rootModel, isPlayer, pivotNode, isChildPoint, childModel);
		_to.UpdateNode(rootModel, isPlayer, pivotNode, isChildPoint, childModel);
	}

	public void ResetNodes()
	{
		_from.ClearChildPoints();
		_to.ClearChildPoints();
	}
}
