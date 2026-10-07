using System.Xml;

public class ConditionBirth : ConditionAnimation
{
	private string _Name;

	public ConditionBirth(XmlNode node)
		: base(ConditionType.BIRTH)
	{
		_Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		bool flag = string.IsNullOrEmpty(_Name) || _Name == conditions.ModelName;
		return (!IsNot) ? flag : (!flag);
	}
}
