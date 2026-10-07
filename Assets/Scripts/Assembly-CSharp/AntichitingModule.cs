using System.Xml;

public class AntichitingModule : LoadingModule
{
	public override void ProcessStep()
	{
		if (!isFinished)
		{
			GameSettings.LoadAssemblySettings();
			SystemProperties.InitDeviceInfo();
			XmlDocument devicesConfig = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "devices.xml");
			SystemProperties.LoadDevicesConfig(devicesConfig);
			GameCenterController.Init();
			isFinished = true;
		}
	}
}
