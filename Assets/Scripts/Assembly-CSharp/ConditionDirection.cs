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

	public void UpdateNodes(ModelObject OECPEDPMKCD, bool EKBOGDKIHIH, ModelNode AECCPADGGPG, bool PHADJMAONJG, ModelObject MJCGOJBGFIE = null)
	{
		_direction.FromPoint.UpdateNode(OECPEDPMKCD, EKBOGDKIHIH, AECCPADGGPG, PHADJMAONJG, MJCGOJBGFIE);
		_direction.ToPoint.UpdateNode(OECPEDPMKCD, EKBOGDKIHIH, AECCPADGGPG, PHADJMAONJG, MJCGOJBGFIE);
	}

	public void ResetNodes()
	{
		_direction.FromPoint.ClearChildPoints();
		_direction.ToPoint.ClearChildPoints();
	}

	public override Model ResolveTargetModel(Model BPBMKGHEEBI, ModelType.ModelTargetType LFLGCDNKNJI)
	{
		return BPBMKGHEEBI;
	}

	public override void ApplyTargetModelType(ModelType.ModelTargetType LFLGCDNKNJI)
	{
	}
}
