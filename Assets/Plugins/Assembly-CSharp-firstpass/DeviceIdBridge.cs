public class DeviceIdBridge
{
	public static string DeviceId
	{
		get
		{
			return GetNativeDeviceId();
		}
	}

	public static string GetNativeDeviceId()
	{
		// The offline game uses SystemProperties' Unity identifier fallback.
		// Do not load the removed Nekki Android bridge or iOS native plugin.
		return null;
	}
}
