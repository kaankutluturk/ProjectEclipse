using System.Collections.Generic;
using System.Xml;

public class QuestActionClearStack : QuestAction
{
	private List<string> questNames = new List<string>();

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		string text = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		string[] collection = text.Split('|');
		questNames.AddRange(collection);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ListSF.GetInstance().ClearQuestsStack(questNames);
		FinishAction();
	}
}
