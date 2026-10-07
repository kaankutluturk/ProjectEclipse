using System.Xml;

public class ConditionEvent : ConditionAnimation
{
	private EventAnimation _event;

	public ConditionEvent(XmlNode node)
		: base(ConditionType.EVENT)
	{
		_event = EventParser.Create(node);
		_event.Init(node);
	}

	public override bool IsEqual(ModelConditions conditions)
	{
		bool flag = _event.IsEqual(conditions.CurrentEvent);
		return (!IsNot) ? flag : (!flag);
	}
}
