using System.Xml;

public class QuestActionToggleGroup : QuestAction
{
	private string _toggle;

	private string _name;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_toggle = node.Attributes["Toggle"].GetStringOrDefault("on");
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		Roster roster = ListSF.GetRoster();
		roster.SetAbGroupToggle(_name, _toggle.Equals("on"));
		ListSF.GetInstance().OnAuthenticate(true);
		FinishAction();
	}
}
