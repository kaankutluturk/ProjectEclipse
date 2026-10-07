using System.Collections.Generic;
using System.Xml;

public class ConditionCurrentAnimation : ConditionAnimation
{
	private string _Name;

	private bool _isNoAnimation;

	private bool _physics;

	public ConditionCurrentAnimation(XmlNode node)
		: base(ConditionType.CURRENT_ANIMATION)
	{
		if (node.Attributes["Name"] != null)
		{
			_Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
			_isNoAnimation = _Name == "$NoAnimation$";
		}
		_physics = node.Attributes["Physics"].ParseBool();
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		bool flag = false;
		if (!string.IsNullOrEmpty(_Name))
		{
			List<string> list = GetAnimationNames(conditions);
			if (!("$Move" == _Name))
			{
				flag = ((!_isNoAnimation) ? IsNames(_Name, list) : (list.Count == 0));
			}
			else if (0 < list.Count)
			{
				flag = IsNames(list[0], conditions.CandidateMoveNames);
			}
		}
		else
		{
			flag = GetIsPhysicsAnimation(conditions);
			flag = flag == _physics;
		}
		return (!IsNot) ? flag : (!flag);
	}

	private static bool IsNames(List<string> NIKHAICFGNM, List<string> MGNOPLPBOHC)
	{
		foreach (string item in NIKHAICFGNM)
		{
			foreach (string item2 in MGNOPLPBOHC)
			{
				if (item == item2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsNames(string name, List<string> MGNOPLPBOHC)
	{
		foreach (string item in MGNOPLPBOHC)
		{
			if (name == item)
			{
				return true;
			}
		}
		return false;
	}

	private List<string> GetAnimationNames(ModelConditions conditions)
	{
		switch (_targetModelType)
		{
		case ModelType.ModelTargetType.MODEL_THIS:
			return conditions.SelfAnimationNames;
		case ModelType.ModelTargetType.MODEL_OTHER:
			return conditions.OtherAnimationNames;
		case ModelType.ModelTargetType.MODEL_PARENT:
			return conditions.ParentAnimationNames;
		case ModelType.ModelTargetType.MODEL_CHILD:
			return conditions.ChildAnimationNames;
		default:
			GameLog.Error("ConditionCurrentAnimation: getAnimationNames - wrong type: {0}", _targetModelType);
			return conditions.SelfAnimationNames;
		}
	}

	private bool GetIsPhysicsAnimation(ModelConditions conditions)
	{
		switch (_targetModelType)
		{
		case ModelType.ModelTargetType.MODEL_THIS:
			return conditions.SelfIsPhysics;
		case ModelType.ModelTargetType.MODEL_OTHER:
			return conditions.OtherIsPhysics;
		case ModelType.ModelTargetType.MODEL_PARENT:
			return conditions.ParentIsPhysics;
		default:
			GameLog.Error("ConditionCurrentAnimation: getAnimationNames - wrong type: %i", _targetModelType);
			return conditions.SelfIsPhysics;
		}
	}
}
