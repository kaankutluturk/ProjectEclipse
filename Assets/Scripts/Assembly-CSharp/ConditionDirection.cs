using System.Xml;

public class ConditionDirection : ConditionAnimation
{
	public InfoAnimation.MoveInside.Direction _direction;

	public ConditionDirection(XmlNode node)
		: base(ConditionType.DIRECTION)
	{
		_direction = MovesParser.ParseDirection(node);
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		int num = _direction.GetDirectionSign(conditions);
		int num2 = 1;
		switch (_targetModelType)
		{
		case ModelType.ModelTargetType.MODEL_THIS:
			num2 = conditions.SelfSign;
			break;
		case ModelType.ModelTargetType.MODEL_OTHER:
			num2 = conditions.OtherSign;
			break;
		case ModelType.ModelTargetType.MODEL_PARENT:
			num2 = conditions.ParentSign;
			break;
		default:
			GameLog.Error("ConditionDirection::isEqual ERROR - unsupported model type: {0}", _targetModelType);
			break;
		}
		bool flag = num == num2;
		return (!IsNot) ? flag : (!flag);
	}

	public void UpdateNodes(ModelObject rootModel, bool isPlayer, ModelNode pivotNode, bool isChildPoint, ModelObject childModel = null)
	{
		_direction.FromPoint.UpdateNode(rootModel, isPlayer, pivotNode, isChildPoint, childModel);
		_direction.ToPoint.UpdateNode(rootModel, isPlayer, pivotNode, isChildPoint, childModel);
	}

	public void ResetNodes()
	{
		_direction.FromPoint.ClearChildPoints();
		_direction.ToPoint.ClearChildPoints();
	}

	public override Model ResolveTargetModel(Model model, ModelType.ModelTargetType targetType)
	{
		return model;
	}

	public override void ApplyTargetModelType(ModelType.ModelTargetType targetType)
	{
	}
}
