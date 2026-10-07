public static class StageType
{
	public enum Stage
	{
		STAGE_NONE = 0,
		STAGE_START_STANCE = 1,
		STAGE_FIGHT = 2,
		STAGE_END_STANCE = 3,
		STAGE_SHOP_START = 4,
		STAGE_SHOP_PURCHASE = 5,
		STAGE_PEACEFUL_RESTORE = 6,
		STAGE_SHOP_TRY_ON = 7
	}

	public static Stage GetStageByName(string name)
	{
		switch (name)
		{
		case "StartStance":
			return Stage.STAGE_START_STANCE;
		case "Fight":
			return Stage.STAGE_FIGHT;
		case "EndStance":
			return Stage.STAGE_END_STANCE;
		case "PeacefulStart":
			return Stage.STAGE_SHOP_START;
		case "ShopPurchase":
			return Stage.STAGE_SHOP_PURCHASE;
		case "PeacefulRestore":
			return Stage.STAGE_PEACEFUL_RESTORE;
		case "TryOn":
			return Stage.STAGE_SHOP_TRY_ON;
		default:
			GameLog.Error("StageType::getStageByName - unknown stage: %s", name);
			return Stage.STAGE_NONE;
		}
	}
}
