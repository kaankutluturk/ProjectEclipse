using System.Collections.Generic;
using System.Xml;

public class QuestActionClearStack : QuestAction
{
	private List<string> questNames = new List<string>();

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		string text = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
		string[] collection = text.Split('|');
		questNames.AddRange(collection);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		ListSF.GetInstance().ClearQuestsStack(questNames);
		FinishAction();
	}
}
