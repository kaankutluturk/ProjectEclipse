public class DeviceInfo
{
	public SystemProperties.PathType GuiResolution;

	public SystemProperties.PathType LocationResolution;

	public SystemProperties.PathType DefaultResolution;

	public float InverseLocationScale;

	public bool IsTablet;

	public bool IsEditor;

	public bool IsWindowsEditor;

	public string Id;

	public string Os;

	public string OsName;

	public string Locale;

	public string UniqueId;

	public string CustomUniqueId;

	public string SocialPlayerId;

	public int DisplayWidth;

	public int DisplayHeight;

	public int CpuCount;

	public int TotalRam;

	public int FreeRam;

	public VersionContainer Version = new VersionContainer();

	public VersionContainer DataVersion = new VersionContainer();

	public string QualityCondition;

	public string GetLanguage()
	{
		return PreciseLocale.GetLanguage();
	}
}
