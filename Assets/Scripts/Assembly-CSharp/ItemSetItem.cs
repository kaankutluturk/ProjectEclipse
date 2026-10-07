using System.Xml;

public class ItemSetItem
{
	public string Name;

	public float Scale;

	public float Rotate;

	public float X;

	public float Y;

	public float IconsY;

	public ItemInfo Item;

	public ItemSetItem(XmlNode node)
	{
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		Scale = node.Attributes["Scale"].ParseFloat();
		Rotate = node.Attributes["Rotate"].ParseFloat();
		X = node.Attributes["X"].ParseFloat();
		Y = node.Attributes["Y"].ParseFloat();
		IconsY = node.Attributes["IconsY"].ParseFloat();
		Item = ListSF.GetItems().GetItemByName(Name);
	}
}
