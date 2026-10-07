public class PerkData
{
	public PerkInfoItem PerkInfo;

	public bool Enabled = true;

	public PerkData(PerkInfoItem perkInfo, bool enabled = true)
	{
		PerkInfo = perkInfo;
		Enabled = enabled;
	}
}
