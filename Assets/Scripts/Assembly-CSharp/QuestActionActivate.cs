using System.Xml;

public class QuestActionActivate : QuestAction
{
	private string targetActionId = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		targetActionId = node.Attributes["ActionID"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
		questParameters.actionId = new RosterQuest.QuestVariable(targetActionId, targetActionId);
		ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_ACTIVATE);
		questParameters.actionId = null;
		FinishAction();
	}
}
