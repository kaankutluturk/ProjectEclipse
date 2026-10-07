public class PerkEventAreaEnter : PerkEvent
{
	public PerkEventAreaEnter()
	{
	}

	public PerkEventAreaEnter(PerkEventAreaEnter source)
		: base(source)
	{
	}

	public override bool IsEqual(EventStruct eventData)
	{
		if (!base.IsEqual(eventData) || eventData == null)
		{
			return false;
		}
		return true;
	}
}
