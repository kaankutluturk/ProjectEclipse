using System.Xml;

public class ConditionModelMirrored : ConditionAnimation
{
	public ConditionModelMirrored(XmlNode node)
		: base(ConditionType.MIRROR)
	{
	}

	public override bool IsEqual(Model ACENLMONNPA, InfoAnimation DBOLBEOCEME)
	{
		bool flag = GetIsMirrored(ACENLMONNPA, DBOLBEOCEME);
		return (!IsNot) ? flag : (!flag);
	}

	private bool GetIsMirrored(Model ACENLMONNPA, InfoAnimation DBOLBEOCEME)
	{
		ModelType.ModelTargetType oOFFOILONLO = _targetModelType;
		if (oOFFOILONLO == ModelType.ModelTargetType.MODEL_THIS)
		{
			return ModelAnimation.CalcIsMirror(ACENLMONNPA.GetBodyObject(), DBOLBEOCEME.GetMirrorNode().GetNodeName(), ACENLMONNPA.GetAnimationModule().GetSign(), DBOLBEOCEME.GetFirstFrameNodes(), false);
		}
		GameLog.Error("ConditionModelMirrored: getMirror - wrong type: {0}", _targetModelType);
		return false;
	}
}
