using System.Xml;

public static class ProfileGUI
{
	public static int AnimationSpeed = 1;

	public static MinMaxValue PerkOpacity = new MinMaxValue();

	public static MinMaxValue SelectOpacity = new MinMaxValue();

	public static float SpeedScrollAchievements = 0f;

	public static void Parse(XmlNode node)
	{
		AnimationSpeed = node["AnimationSpeed"].FirstAttribute().ParseInt(1);
		PerkOpacity.Parse(node["PerkOpacity"], 1f, 1f);
		SelectOpacity.Parse(node["SelectOpacity"], 1f, 1f);
		SpeedScrollAchievements = node["SpeedScrollAchievements"].FirstAttribute().ParseFloat() / 60f;
	}
}
