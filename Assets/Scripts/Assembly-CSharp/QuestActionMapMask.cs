using System.Xml;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Map;
using UnityEngine;

public class QuestActionMapMask : QuestAction
{
	private Color maskColor;

	private bool unusedFlag;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		string colorText = node.Attributes["Color"].GetStringOrDefault(string.Empty);
		maskColor = ColorUtils.ParseHexColor(colorText);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		if (Module.GetInstance().GetCurrentScreenType() == ScreenType.ModuleMap)
		{
			MapScene current = Scene<MapScene>.get_Current();
			if (current != null)
			{
				// Eclipse: ease the toggle instead of snapping the whole map to the new tint.
				current.FadeStoryZonesBackgroundMask(maskColor, 0.8f);
			}
		}
		ListSF.GetRoster().set_MapMaskColor(maskColor);
		FinishAction();
	}
}
