using System.Xml;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Map;

public class QuestActionMapFocus : QuestAction
{
	private string _BattleName = string.Empty;

	private float _Duration;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		_BattleName = node.Attributes["Battle"].GetStringOrDefault(string.Empty);
		_Duration = node.Attributes["Frames"].ParseFloat() / 60f;
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(Parameters);
		condition.SetValue(_BattleName, result);
		ListSF.GetRoster().SetMapFocus(result.ToString());
		FightIDS fightIds = ListSF.GetRoster().GetMapFocus();
		MapScene current = Scene<MapScene>.get_Current();
		if (current != null)
		{
			FightList fight = ListSF.GetFightById(fightIds);
			if (fight != null)
			{
				current.SelectFight(fight, _Duration);
			}
			else
			{
				Battle battle = ListSF.GetBattleById(fightIds);
				current.SelectBattle(battle, _Duration);
			}
		}
		FinishAction();
	}
}
