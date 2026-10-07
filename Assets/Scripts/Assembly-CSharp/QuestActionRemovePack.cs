using System.Xml;

public class QuestActionRemovePack : QuestAction
{
	private string packName = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		packName = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		if (packName != string.Empty)
		{
			PacksController.GetInstance().DeletePack(packName);
			ListSF.GetInstance().OnPacksChanged();
		}
		FinishAction();
	}
}
