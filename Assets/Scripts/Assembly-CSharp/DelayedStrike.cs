public class DelayedStrike
{
	public ItemInfo Item;

	public SliderType SliderType;

	public bool IsStrikeResult;

	public DelayedStrike(SliderType _type, ItemInfo item = null, bool isStrikeResult = false)
	{
		Item = item;
		SliderType = _type;
		IsStrikeResult = isStrikeResult;
	}
}
