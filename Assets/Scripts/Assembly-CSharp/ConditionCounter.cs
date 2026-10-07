using System.Xml;

public class ConditionCounter
{
	public enum CounterConditionType
	{
		NONE = 0,
		BATTLE = 1,
		OPERATOR = 2
	}

	protected CounterConditionType _conditionType;

	protected bool _isNot;

	public ConditionCounter(CounterConditionType conditionType)
	{
		_conditionType = conditionType;
	}

	public virtual bool IsEqual(CounterConditions conditions)
	{
		GameLog.Error("ERROR: Unknown condition type checked: %i", _conditionType);
		return false;
	}

	public bool IsNotCompare(bool result)
	{
		return _isNot ? (!result) : result;
	}

	public virtual void Initialize()
	{
	}

	protected virtual void Parse(XmlNode node)
	{
		_isNot = node.Attributes["Not"].ParseBool();
	}
}
