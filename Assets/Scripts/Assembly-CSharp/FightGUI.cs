using System.Xml;

public static class FightGUI
{
	private static float lifeBarMin;

	public static float LifeBarMin
	{
		get
		{
			return GetLifeBarMin();
		}
	}

	public static float GetLifeBarMin()
	{
		return lifeBarMin;
	}

	public static void Parse(XmlNode node)
	{
		PerkGUI.Parse(node["PerkIcons"]);
		lifeBarMin = node["LifeBarMin"].FirstAttribute().ParseFloat();
	}
}
