using System.Xml;

public class QuestActionEclipseMode : QuestAction
{
	private bool _enabled;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		string value = node.Attributes["Toggle"].GetStringOrDefault("Off");
		_enabled = value.Equals("On", System.StringComparison.OrdinalIgnoreCase) ||
			value.Equals("True", System.StringComparison.OrdinalIgnoreCase) || value == "1";
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		Roster roster = ListSF.GetRoster();
		if (roster != null)
		{
			roster.SetEclipseMode(_enabled);
			ListSF.GetInstance().RequestSave();
		}
		FinishAction();
	}
}
