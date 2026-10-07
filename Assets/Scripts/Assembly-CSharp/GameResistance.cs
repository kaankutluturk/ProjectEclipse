using System.Xml;

public class GameResistance
{
	public int MaxValue;

	public string Name;

	public string Icon;

	public string Color;

	public GameResistance(XmlNode node)
	{
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		MaxValue = node.Attributes["MaxValue"].ParseInt();
		Icon = node.Attributes["Icon"].GetStringOrDefault(string.Empty);
		Color = node.Attributes["Color"].GetStringOrDefault(string.Empty);
	}

	public string GetIcon()
	{
		return Icon;
	}
}
