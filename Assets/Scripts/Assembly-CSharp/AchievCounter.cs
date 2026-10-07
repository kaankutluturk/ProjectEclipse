using System.Collections.Generic;
using System.Xml;

public class AchievCounter
{
	public string Name;

	public List<Achievement> Achievements = new List<Achievement>();

	public AchievCounter(XmlNode node)
	{
		Name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		foreach (XmlNode childNode in node.ChildNodes)
		{
			Achievement item = new Achievement(childNode);
			Achievements.Add(item);
		}
		Achievements.Sort((Achievement LHBNIMGFKIB, Achievement AAOIAEJJINO) => LHBNIMGFKIB.CounterValue.CompareTo(AAOIAEJJINO.CounterValue));
	}
}
