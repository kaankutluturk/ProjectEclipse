using System.Xml;

public class QuestActionSessionSettings : QuestAction
{
	private string settingName = string.Empty;

	private string settingValue = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		settingName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		settingValue = node.Attributes["Value"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		Roster roster = ListSF.GetRoster();
		if (roster != null)
		{
			roster.SessionSettings(settingName, settingValue);
		}
		ListSF.GetInstance().RequestSave();
		FinishAction();
	}
}
