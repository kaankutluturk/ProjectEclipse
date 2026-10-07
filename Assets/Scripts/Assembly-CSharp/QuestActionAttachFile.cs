using System.Xml;

public class QuestActionAttachFile : QuestAction
{
	private string fileName = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		fileName = EPKLCPOEELO.Attributes["File"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		if (fileName != string.Empty)
		{
			ListSF.GetInstance().LoadQuests(fileName);
		}
		FinishAction();
	}
}
