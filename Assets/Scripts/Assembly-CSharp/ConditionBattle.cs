using System.Xml;

public class ConditionBattle : ConditionCounter
{
	private string _type;

	private BattleType _battleType;

	public ConditionBattle(XmlNode node)
		: base(CounterConditionType.BATTLE)
	{
		Parse(node);
	}

	public override bool IsEqual(CounterConditions conditions)
	{
		bool dCJLKCFKCOM = conditions.BattleType == _battleType;
		return IsNotCompare(dCJLKCFKCOM);
	}

	public override void Initialize()
	{
		_battleType = ListSF.GetInstance().GetBattleTypeByName(_type);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		_type = node.Attributes["Type"].GetStringOrDefault(string.Empty);
	}
}
