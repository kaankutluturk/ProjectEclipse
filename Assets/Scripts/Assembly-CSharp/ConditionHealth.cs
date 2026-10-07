using System.Xml;

public class ConditionHealth : ConditionAnimation
{
	public float Min;

	public float Max;

	public ConditionHealth(XmlNode node)
		: base(ConditionType.HEALTH)
	{
		Min = node.Attributes["Min"].ParseFloat();
		Max = node.Attributes["Max"].ParseFloat();
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		float num = conditions.CurrentHealth / conditions.MaxHealth;
		bool flag = Min <= num && num <= Max;
		return (!IsNot) ? flag : (!flag);
	}
}
