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

	public ConditionCounter(CounterConditionType LFLGCDNKNJI)
	{
		_conditionType = LFLGCDNKNJI;
	}

	public virtual bool IsEqual(CounterConditions conditions)
	{
		GameLog.Error("ERROR: Unknown condition type checked: %i", _conditionType);
		return false;
	}

	public bool IsNotCompare(bool DCJLKCFKCOM)
	{
		return _isNot ? (!DCJLKCFKCOM) : DCJLKCFKCOM;
	}

	public virtual void Initialize()
	{
	}

	protected virtual void Parse(XmlNode BGPKIKNPIKP)
	{
		_isNot = BGPKIKNPIKP.Attributes["Not"].ParseBool();
	}
}
