using System.Xml;

public class AntichitingModule : LoadingModule
{
	public override void ProcessStep()
	{
		if (!isFinished)
		{
			GameSettings.LoadAssemblySettings();
			SystemProperties.InitDeviceInfo();
			XmlDocument jFJPKEONJIJ = XmlUtils.OpenXMLDocument(SF2Paths.GetGameDataPath(), "devices.xml");
			SystemProperties.LoadDevicesConfig(jFJPKEONJIJ);
			GameCenterController.Init();
			isFinished = true;
		}
	}
}
