using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Map;

public class QuestActionSwitchToRaidsMap : QuestAction
{
	public override void Execute(QuestParameters GFIHPBCEEOB)
	{
		base.Execute(GFIHPBCEEOB);
		MapScene current = Scene<MapScene>.get_Current();
		if (current != null)
		{
			current.SwitchToRaidMap();
		}
		FinishAction();
	}
}
