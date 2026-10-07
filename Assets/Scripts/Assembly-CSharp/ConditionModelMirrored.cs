using System.Xml;

public class ConditionModelMirrored : ConditionAnimation
{
	public ConditionModelMirrored(XmlNode node)
		: base(ConditionType.MIRROR)
	{
	}

	public override bool IsEqual(Model model, InfoAnimation animationInfo)
	{
		bool flag = GetIsMirrored(model, animationInfo);
		return (!IsNot) ? flag : (!flag);
	}

	private bool GetIsMirrored(Model model, InfoAnimation animationInfo)
	{
		ModelType.ModelTargetType targetType = _targetModelType;
		if (targetType == ModelType.ModelTargetType.MODEL_THIS)
		{
			return ModelAnimation.CalcIsMirror(model.GetBodyObject(), animationInfo.GetMirrorNode().GetNodeName(), model.GetAnimationModule().GetSign(), animationInfo.GetFirstFrameNodes(), false);
		}
		GameLog.Error("ConditionModelMirrored: getMirror - wrong type: {0}", _targetModelType);
		return false;
	}
}
