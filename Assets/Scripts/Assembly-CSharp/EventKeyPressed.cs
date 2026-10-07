public class EventKeyPressed : EventAnimation
{
	public EventKeyPressed()
		: base(EventAnimationType.EVENT_KEY_PRESSED)
	{
	}

	protected override bool Compare(EventAnimation other)
	{
		return true;
	}
}
