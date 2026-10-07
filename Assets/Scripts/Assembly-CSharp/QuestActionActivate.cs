using System.Xml;

public class QuestActionActivate : QuestAction
{
	private string targetActionId = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		targetActionId = EPKLCPOEELO.Attributes["ActionID"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		hHKLFIIBIFF.actionId = new RosterQuest.QuestVariable(targetActionId, targetActionId);
		ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_ACTIVATE);
		hHKLFIIBIFF.actionId = null;
		FinishAction();
	}
}
