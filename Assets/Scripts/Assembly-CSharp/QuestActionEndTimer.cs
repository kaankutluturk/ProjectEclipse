using System.Xml;

public class QuestActionEndTimer : QuestAction
{
	private string timerName;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		timerName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		Roster roster = ListSF.GetRoster();
		RosterTimerContainer timerContainer = roster.GetTimerContainer();
		timerContainer.RemoveTimer(timerName);
		FinishAction();
	}
}
