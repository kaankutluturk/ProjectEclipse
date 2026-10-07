public static class InputDeviceExtension
{
	public static bool CanOrderDelivery(ItemInfo item, UserItem NDMCFNGEPOA)
	{
		if (item == null)
		{
			return false;
		}
		if (NDMCFNGEPOA == null && item.DeliveryTime > 0)
		{
			return true;
		}
		return false;
	}

	public static bool IsDeliveryInProgress(ItemInfo item, UserItem NDMCFNGEPOA)
	{
		return NDMCFNGEPOA != null && NDMCFNGEPOA.GetDeliveryTimestamp() > 0;
	}

	public static bool CanUpgradeItem(ItemInfo item, UserItem NDMCFNGEPOA)
	{
		if (NDMCFNGEPOA == null)
		{
			return false;
		}
		return NDMCFNGEPOA.GetHasUpgrades() && !NDMCFNGEPOA.GetIsMaxUpgrade();
	}

	public static void GetOwnedAndEquippedState(ref bool PNKJLPDJOJF, ref bool CBDBANOPFDM, ItemInfo item)
	{
		if (item != null)
		{
			UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem(item);
			if (dKCHDHMLKHN != null)
			{
				PNKJLPDJOJF = true;
				CBDBANOPFDM = dKCHDHMLKHN.GetIsEquipped();
			}
		}
	}
}
