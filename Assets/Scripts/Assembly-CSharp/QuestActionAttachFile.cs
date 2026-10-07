using System.Xml;

public class QuestActionAttachFile : QuestAction
{
	private string fileName = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		fileName = node.Attributes["File"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		if (fileName != string.Empty)
		{
			ListSF.GetInstance().LoadQuests(fileName);
		}
		FinishAction();
	}
}
