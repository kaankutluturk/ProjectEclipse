public static class InputDeviceExtension
{
	public static bool CanOrderDelivery(ItemInfo item, UserItem userItem)
	{
		if (item == null)
		{
			return false;
		}
		if (userItem == null && item.DeliveryTime > 0)
		{
			return true;
		}
		return false;
	}

	public static bool IsDeliveryInProgress(ItemInfo item, UserItem userItem)
	{
		return userItem != null && userItem.GetDeliveryTimestamp() > 0;
	}

	public static bool CanUpgradeItem(ItemInfo item, UserItem userItem)
	{
		if (userItem == null)
		{
			return false;
		}
		return userItem.GetHasUpgrades() && !userItem.GetIsMaxUpgrade();
	}

	public static void GetOwnedAndEquippedState(ref bool isOwned, ref bool isEquipped, ItemInfo item)
	{
		if (item != null)
		{
			UserItem userItem = ListSF.GetRoster().GetInventory().FindItem(item);
			if (userItem != null)
			{
				isOwned = true;
				isEquipped = userItem.GetIsEquipped();
			}
		}
	}
}
