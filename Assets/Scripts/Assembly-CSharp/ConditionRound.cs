using System.Xml;

public class ConditionRound : ConditionAnimation
{
	private string _roundName;

	public ConditionRound(XmlNode node)
		: base(ConditionType.ROUND)
	{
		_roundName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		bool flag = false;
		if ((conditions.RoundStage == 1 && _roundName == "StartStance") || (conditions.RoundStage == 2 && _roundName == "Fight") || (conditions.RoundStage == 3 && _roundName == "EndStance") || (conditions.RoundStage == 7 && _roundName == "TryOn"))
		{
			flag = true;
		}
		return (!IsNot) ? flag : (!flag);
	}
}
