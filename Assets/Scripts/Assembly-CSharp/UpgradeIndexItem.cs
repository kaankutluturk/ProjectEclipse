public class UpgradeIndexItem
{
	public enum UpgradeIndexType
	{
		UPGRADE_INDEX_NONE = 0,
		UPGRADE_INDEX_MILESTONE = 1,
		UPGRADE_INDEX_NORMAL = 2
	}

	public int Index;

	public UpgradeIndexType Type = UpgradeIndexType.UPGRADE_INDEX_NORMAL;
}
