using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class DistancePoint
{
	public enum Object
	{
		OBJECT_NULL = 0,
		OBJECT_NODES = 1,
		OBJECT_PIVOT = 2,
		OBJECT_WALL = 3,
		OBJECT_FLOOR = 4,
		OBJECT_COM = 5
	}

	public enum DistanceFrame
	{
		DISTANCE_FRAME_NULL = 0,
		DISTANCE_FRAME_CURRENT = 1,
		DISTANCE_FRAME_PREVIOUS = 2
	}

	public enum ImpulseDirection
	{
		IMPULSE_NONE = 0,
		IMPULSE_NOT_REVERSE = 1,
		IMPULSE_REVERSE = 2
	}

	public class PointNode
	{
		public ModelNode Node;

		public ModelNode PivotNode;
	}

	public class ChildPoint
	{
		public PointNode Point = new PointNode();

		public ModelObject Owner;

		public ChildPoint(ModelObject owner)
		{
			Owner = owner;
		}
	}

	public ModelType.ModelTargetType TargetModel;

	public Object ObjectType;

	public DistanceFrame Frame;

	public string Part;

	public bool IsBackWall;

	public float ShiftX;

	public float ShiftY;

	private List<ChildPoint> playerChildPoints = new List<ChildPoint>();

	private List<ChildPoint> opponentChildPoints = new List<ChildPoint>();

	private PointNode opponentPoint = new PointNode();

	private PointNode playerPoint = new PointNode();

    // Shared moves may be evaluated by several roots on the same native side.
    // Weak keys keep node/pivot bindings with the actual body without retaining
    // retired forms through a node's reference back to its owning ModelObject.
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<ModelObject, PointNode> _rootPoints =
        new System.Runtime.CompilerServices.ConditionalWeakTable<ModelObject, PointNode>();

	public DistancePoint()
	{
		IsBackWall = false;
		TargetModel = ModelType.ModelTargetType.MODEL_NULL;
		ObjectType = Object.OBJECT_NULL;
		Frame = DistanceFrame.DISTANCE_FRAME_NULL;
		ShiftX = (ShiftY = 0f);
	}

	public DistancePoint(XmlNode node)
	{
		IsBackWall = false;
		TargetModel = ModelType.ModelTargetType.MODEL_NULL;
		ObjectType = Object.OBJECT_NULL;
		Frame = DistanceFrame.DISTANCE_FRAME_NULL;
		ShiftX = (ShiftY = 0f);
		Create(node);
	}

	public virtual void Create(XmlNode node)
	{
		TargetModel = ModelType.ParseTargetType((node == null) ? "Null" : node.Attributes["Player"].GetStringOrDefault("Null"));
		XmlAttribute attribute = ((node == null) ? null : node.Attributes["Object"]);
		string objectName = attribute.GetStringOrDefault(string.Empty);
		ObjectType = (Object)MovesMaps.GetMappedIndex(MovesMaps.MapType.DISTANCE_OBJECT_TYPE, objectName);
		attribute = ((node == null) ? null : node.Attributes["Part"]);
		Part = attribute.GetStringOrDefault(string.Empty);
		if (ObjectType == Object.OBJECT_WALL)
		{
			IsBackWall = Part == "Back";
		}
		Frame = DistanceFrame.DISTANCE_FRAME_CURRENT;
		attribute = ((node == null) ? null : node.Attributes["Frame"]);
		if (attribute != null && attribute.GetStringOrDefault(string.Empty) == "Previous")
		{
			Frame = DistanceFrame.DISTANCE_FRAME_PREVIOUS;
		}
		ShiftX = ((node == null) ? 0f : node.Attributes["ShiftX"].ParseFloat());
		ShiftY = ((node == null) ? 0f : node.Attributes["ShiftY"].ParseFloat());
	}

	public void Create(string targetName, string objectName, string partName)
	{
		TargetModel = ModelType.ParseTargetType(targetName);
		ObjectType = (Object)MovesMaps.GetMappedIndex(MovesMaps.MapType.DISTANCE_OBJECT_TYPE, objectName);
		if (ObjectType == Object.OBJECT_WALL)
		{
			IsBackWall = partName == "Back";
		}
		Frame = DistanceFrame.DISTANCE_FRAME_CURRENT;
		string empty = string.Empty;
		if (empty == "Previous")
		{
			Frame = DistanceFrame.DISTANCE_FRAME_PREVIOUS;
		}
		ShiftX = 0f;
		ShiftY = 0f;
	}

	public virtual float GetX(ModelConditions conditions)
	{
		return GetPosition(conditions).x;
	}

	public virtual float GetY(ModelConditions conditions)
	{
		return GetPosition(conditions).y * -1f;
	}

	public virtual float GetZ(ModelConditions conditions)
	{
		return GetPosition(conditions).z;
	}

	public virtual Vector3 GetPosition(ModelConditions conditions)
	{
		ModelConditions.ModelPositions modelPositions = GetModelPositions(conditions);
		ModelNode modelNode = null;
		Vector3 result = default(Vector3);
		switch (ObjectType)
		{
		case Object.OBJECT_NODES:
		{
			modelNode = GetNode(conditions);
			if (modelNode != null)
			{
				result = Vector3f.op_Implicit(GetFramePosition(modelNode));
			}
			float x = result.x + ShiftX * (float)conditions.AnimationSign;
			float y = result.y - ShiftY;
			return new Vector3(x, y);
		}
		case Object.OBJECT_PIVOT:
			modelNode = GetPivotNode(conditions);
			if (modelNode != null)
			{
				result = Vector3f.op_Implicit(GetFramePosition(modelNode));
			}
			result.x += ShiftX * (float)conditions.AnimationSign;
			result.y -= ShiftY;
			return result;
		case Object.OBJECT_WALL:
		{
			Vector2 vector = GetWallPosition(conditions, modelPositions);
			vector.x += ShiftX * (float)conditions.SelfSign;
			vector.y -= ShiftY;
			return Vector3f.op_Implicit(new Vector3f(vector));
		}
		case Object.OBJECT_FLOOR:
			return Vector3f.op_Implicit(new Vector3f(ShiftX, 0f - ShiftY));
		case Object.OBJECT_COM:
			modelNode = modelPositions.Body.GetCenterOfMassNode();
			result = Vector3f.op_Implicit(GetFramePosition(modelNode));
			result.x += ShiftX * (float)conditions.SelfSign;
			result.y -= ShiftY;
			return result;
		default:
			GameLog.Write("ERROR: unknown object type: %i", ObjectType);
			return default(Vector3);
		}
	}

	public void UpdateNode(ModelObject rootModel, bool isPlayer, ModelNode pivotNode, bool isChildPoint, ModelObject childModel)
	{
        ModelNode resolved = null;
        if (ObjectType == Object.OBJECT_NODES)
        {
            // Binding visits both fighters' candidate moves before selection.
            // Equipment/child nodes can be absent on this body. Preserve the
            // native nullable lookup and replace any retired body's cached node.
            resolved = rootModel.GetNodeByName(Part);
        }
		PointNode pointNode = null;
		if (isChildPoint)
		{
			bool flag = false;
			List<ChildPoint> list = ((!isPlayer) ? opponentChildPoints : playerChildPoints);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Owner == childModel)
				{
					pointNode = list[i].Point;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				ChildPoint childPoint = new ChildPoint(childModel);
				list.Add(childPoint);
				pointNode = childPoint.Point;
			}
		}
		else
		{
			pointNode = ((!isPlayer) ? opponentPoint : playerPoint);
            if (rootModel != null)
            {
                var rootPoint = _rootPoints.GetValue(rootModel, _ => new PointNode());
                if (ObjectType == Object.OBJECT_NODES) rootPoint.Node = resolved;
                rootPoint.PivotNode = pivotNode;
            }
		}
		if (ObjectType == Object.OBJECT_NODES)
		{
			pointNode.Node = resolved;
		}
		pointNode.PivotNode = pivotNode;
	}

	protected Vector2 GetWallPosition(ModelConditions conditions, ModelConditions.ModelPositions positions)
	{
		int num = 0;
		switch (TargetModel)
		{
		case ModelType.ModelTargetType.MODEL_NULL:
		case ModelType.ModelTargetType.MODEL_THIS:
			num = conditions.AnimationSign;
			break;
		case ModelType.ModelTargetType.MODEL_OTHER:
		case ModelType.ModelTargetType.MODEL_OTHER_CHILD:
			num = conditions.OtherSign;
			break;
		case ModelType.ModelTargetType.MODEL_PARENT:
			num = conditions.ParentSign;
			break;
		default:
			GameLog.Error("ERROR: DistancePoint::getPosition - unknown model: {0}", TargetModel);
			break;
		}
		return (num > 0 != IsBackWall) ? positions.RightWall : positions.LeftWall;
	}

	protected ModelNode GetPivotNode(ModelConditions conditions)
	{
		ModelNode pivotNode = GetPointNode(conditions).PivotNode;
		if (pivotNode != null)
		{
			ModelNode lCDGOCIAIDK2 = pivotNode.GetPairNode();
			if (lCDGOCIAIDK2 != null)
			{
				int animationSign = conditions.AnimationSign;
				int pairSelector = conditions.PivotPairSelector;
				float num = pivotNode.GetStart().GetX() * (float)animationSign;
				float num2 = lCDGOCIAIDK2.GetStart().GetX() * (float)animationSign;
				bool flag = num > num2;
				if ((pairSelector == 1 && !flag) || (pairSelector == 2 && flag))
				{
					pivotNode = lCDGOCIAIDK2;
				}
			}
		}
		else
		{
			switch (TargetModel)
			{
			case ModelType.ModelTargetType.MODEL_NULL:
			case ModelType.ModelTargetType.MODEL_THIS:
				pivotNode = conditions.SelfNode;
				break;
			case ModelType.ModelTargetType.MODEL_OTHER:
			case ModelType.ModelTargetType.MODEL_OTHER_CHILD:
				pivotNode = conditions.OtherNode;
				break;
			case ModelType.ModelTargetType.MODEL_PARENT:
				pivotNode = conditions.ParentNode;
				break;
			default:
				GameLog.Error("DistancePoint: getNode - wrong type: {1}", TargetModel);
				break;
			}
		}
		return pivotNode;
	}

	private ModelNode GetNode(ModelConditions conditions)
	{
		return GetPointNode(conditions).Node;
	}

	protected ModelConditions.ModelPositions GetModelPositions(ModelConditions conditions)
	{
		switch (TargetModel)
		{
		case ModelType.ModelTargetType.MODEL_NULL:
		case ModelType.ModelTargetType.MODEL_THIS:
			return conditions.SelfPositions;
		case ModelType.ModelTargetType.MODEL_OTHER:
		case ModelType.ModelTargetType.MODEL_OTHER_CHILD:
			return conditions.OtherPositions;
		case ModelType.ModelTargetType.MODEL_PARENT:
			return conditions.ParentPositions;
		default:
			GameLog.Error("ERROR: DistancePoint.DistanceObject.getPosition - unknown model: {0}", TargetModel);
			return conditions.SelfPositions;
		}
	}

	protected Vector3f GetFramePosition(ModelNode node)
	{
		switch (Frame)
		{
		case DistanceFrame.DISTANCE_FRAME_CURRENT:
			return node.GetStart();
		case DistanceFrame.DISTANCE_FRAME_PREVIOUS:
			return node.GetEnd();
		default:
			GameLog.Error("DistancePoint: getPositionFrame - unknown frame: {0}", Frame);
			return node.GetStart();
		}
	}

	protected PointNode GetPointNode(ModelConditions conditions)
	{
		if (conditions.IsWeapon && TargetModel != ModelType.ModelTargetType.MODEL_OTHER &&
			TargetModel != ModelType.ModelTargetType.MODEL_OTHER_CHILD)
		{
			return GetChildPointNode(conditions);
		}
        ModelObject root = null;
        switch (TargetModel)
        {
            case ModelType.ModelTargetType.MODEL_NULL:
            case ModelType.ModelTargetType.MODEL_THIS:
                root = conditions.SelfPositions.Body;
                break;
            case ModelType.ModelTargetType.MODEL_OTHER:
            case ModelType.ModelTargetType.MODEL_OTHER_CHILD:
                root = conditions.OtherPositions.Body;
                break;
        }
        if (root != null)
        {
            if (_rootPoints.TryGetValue(root, out var point)) return point;
            // A live but unbound root must not read a different body's side slot.
            return new PointNode();
        }
        // Some archival callers supply only a side, without live model context.
        // Keep their original lookup while live combat uses identity above.
		switch (TargetModel)
		{
		case ModelType.ModelTargetType.MODEL_NULL:
		case ModelType.ModelTargetType.MODEL_THIS:
			return (!conditions.IsPlayer) ? opponentPoint : playerPoint;
		case ModelType.ModelTargetType.MODEL_OTHER:
		case ModelType.ModelTargetType.MODEL_OTHER_CHILD:
			return (!conditions.IsPlayer) ? playerPoint : opponentPoint;
		case ModelType.ModelTargetType.MODEL_PARENT:
			GameLog.Error("DistancePoint: getPointNode - taking parent from base model");
			break;
		}
		GameLog.Error("DistancePoint: getPointNode - wrong type: {0}", TargetModel);
		return null;
	}

	private PointNode GetChildPointNode(ModelConditions conditions)
	{
		ModelObject selfBody = conditions.SelfPositions.Body;
		List<ChildPoint> list = null;
		switch (TargetModel)
		{
		case ModelType.ModelTargetType.MODEL_NULL:
		case ModelType.ModelTargetType.MODEL_THIS:
			list = ((!conditions.IsPlayer) ? opponentChildPoints : playerChildPoints);
			break;
		case ModelType.ModelTargetType.MODEL_OTHER:
		case ModelType.ModelTargetType.MODEL_OTHER_CHILD:
			list = ((!conditions.IsPlayer) ? playerChildPoints : opponentChildPoints);
			break;
		case ModelType.ModelTargetType.MODEL_PARENT:
			return (!conditions.IsPlayer) ? opponentPoint : playerPoint;
		default:
			GameLog.Error("DistancePoint: getChildPointNode - wrong type: {0}", TargetModel);
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Owner == selfBody)
			{
				return list[i].Point;
			}
		}
		GameLog.Error("DistancePoint: getChildPointNode - no child found");
		return null;
	}

	public void ClearChildPoints()
	{
		playerChildPoints.Clear();
		opponentChildPoints.Clear();
	}
}
