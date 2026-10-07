using System.Collections.Generic;

public class UpgradeDataContainer
{
	public string Type = string.Empty;

	public List<UpgradeData> Upgrades = new List<UpgradeData>();

	public void RandomizeObscuredVars()
	{
		Upgrades.ForEach((UpgradeData upgradeData) =>
		{
			upgradeData.RandomizeObscuredVars();
		});
	}
}
