using System.Xml;

public class ConditionPlayer : ConditionAnimation
{
	private bool _isPlayerOne;

	public ConditionPlayer(XmlNode node)
		: base(ConditionType.PLAYER)
	{
		_isPlayerOne = node.Attributes["Number"].ParseInt(1) == 1;
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		bool flag = _isPlayerOne == conditions.IsPlayer;
		return (!IsNot) ? flag : (!flag);
	}
}
