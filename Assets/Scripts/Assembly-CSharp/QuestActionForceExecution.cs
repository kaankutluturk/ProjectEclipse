using System.Xml;

public class QuestActionForceExecution : QuestAction
{
	private string Name;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ListSF.GetInstance().AddQuestToStek(Name, true);
		FinishAction();
	}
}
