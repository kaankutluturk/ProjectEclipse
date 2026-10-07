using System.Xml;

public class QuestActionRemovePack : QuestAction
{
	private string packName = string.Empty;

	public override void Parse(XmlNode EPKLCPOEELO)
	{
		base.Parse(EPKLCPOEELO);
		packName = EPKLCPOEELO.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		if (packName != string.Empty)
		{
			PacksController.GetInstance().DeletePack(packName);
			ListSF.GetInstance().OnPacksChanged();
		}
		FinishAction();
	}
}
