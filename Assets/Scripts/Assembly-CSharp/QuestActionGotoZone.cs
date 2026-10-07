using System.Xml;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Map;

public class QuestActionGotoZone : QuestAction
{
	private string _name = string.Empty;

	private int _frames;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_frames = node.Attributes["Frames"].ParseInt();
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		MapScene current = Scene<MapScene>.get_Current();
		if (current != null)
		{
			current.GotoZoneByName(_name, _frames);
		}
		FinishAction();
	}
}
