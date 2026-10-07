using System.Xml;

public class ChangeFightRule : Rule
{
	private int _rounds;

	private int _roundTime;

	public ChangeFightRule(XmlNode node)
		: base(RuleType.RuleChangeFight, node)
	{
		Parse(node);
	}

	public int GetRounds()
	{
		return _rounds;
	}

	public int GetRoundTime()
	{
		return _roundTime;
	}

	protected override void Parse(XmlNode node)
	{
		_rounds = node.Attributes["Rounds"].ParseInt(-1);
		_roundTime = node.Attributes["RoundTime"].ParseInt(-1);
	}
}
