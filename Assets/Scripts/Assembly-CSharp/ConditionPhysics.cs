using System.Xml;

public class ConditionPhysics : ConditionAnimation
{
	private float _min;

	private float _max;

	public ConditionPhysics(XmlNode node)
		: base(ConditionType.PHYSICS_FRAME)
	{
		_min = node.Attributes["Min"].ParseFloat(-1f);
		_max = node.Attributes["Max"].ParseFloat(-1f);
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		bool flag = false;
		if ((_min == -1f || (float)conditions.CurrentFrame >= _min) && (_max == -1f || (float)conditions.CurrentFrame <= _max))
		{
			flag = true;
		}
		return (!IsNot) ? flag : (!flag);
	}
}
