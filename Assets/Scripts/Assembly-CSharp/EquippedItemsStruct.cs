public class EquippedItemsStruct
{
	public ItemInfo Skeleton;

	public ItemInfo Weapon;

	public ItemInfo Armor;

	public ItemInfo Helm;

	public ItemInfo Ranged;

	public ItemInfo Magic;

	public ItemInfo RaidCharge;

	public ItemInfo Seal;

	public bool Compare(EquippedItemsStruct other)
	{
		return other.Armor == Armor && other.Helm == Helm && other.Skeleton == Skeleton && other.Weapon == Weapon && other.Magic == Magic && other.RaidCharge == RaidCharge && other.Ranged == Ranged;
	}
}
