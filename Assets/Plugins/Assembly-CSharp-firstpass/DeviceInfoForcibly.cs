public class DeviceInfoForcibly
{
	public string Tablet = string.Empty;

	public string Resolution = string.Empty;

	public string LocationResolution = string.Empty;

	public string QualityCondition = string.Empty;

	public bool Empty
	{
		get
		{
			return IsEmpty();
		}
	}

	public bool IsEmpty()
	{
		return string.IsNullOrEmpty(Tablet) && string.IsNullOrEmpty(Resolution) && string.IsNullOrEmpty(LocationResolution) && string.IsNullOrEmpty(QualityCondition);
	}
}
