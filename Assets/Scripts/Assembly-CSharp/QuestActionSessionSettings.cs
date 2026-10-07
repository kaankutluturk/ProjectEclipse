using System.Xml;

public class QuestActionSessionSettings : QuestAction
{
	private string settingName = string.Empty;

	private string settingValue = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		settingName = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
		settingValue = EPKLCPOEELO.Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP != null)
		{
			nKGLHEGIKKP.SessionSettings(settingName, settingValue);
		}
		ListSF.GetInstance().RequestSave();
		FinishAction();
	}
}
