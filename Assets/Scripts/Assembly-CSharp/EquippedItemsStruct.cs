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

	public bool Compare(EquippedItemsStruct FNGODBOFAJD)
	{
		return FNGODBOFAJD.Armor == Armor && FNGODBOFAJD.Helm == Helm && FNGODBOFAJD.Skeleton == Skeleton && FNGODBOFAJD.Weapon == Weapon && FNGODBOFAJD.Magic == Magic && FNGODBOFAJD.RaidCharge == RaidCharge && FNGODBOFAJD.Ranged == Ranged;
	}
}
