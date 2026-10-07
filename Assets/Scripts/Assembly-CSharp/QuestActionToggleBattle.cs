using System.Xml;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Map;

public class QuestActionToggleBattle : QuestAction
{
	private bool _toggle;

	private string _name;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		string text = node.Attributes["Toggle"].GetStringOrDefault(string.Empty);
		if (text.Equals("on"))
		{
			_toggle = true;
		}
		else if (text.Equals("off"))
		{
			_toggle = false;
		}
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(_name, result);
		FightIDS fightIds = new FightIDS();
		fightIds.SetFightIDSByString(result.resultSTR);
		Battle battle = ListSF.GetBattleById(fightIds);
		RosterBattle rosterBattle = ((battle == null) ? null : battle.GetRosterBattle());
		if (rosterBattle != null)
		{
			rosterBattle.SetHidden(!_toggle);
		}
		MapScene current = Scene<MapScene>.get_Current();
		if (current != null && battle != null)
		{
			current.UpdateBattleButtonHidden(battle);
		}
		ListSF.GetInstance().RequestSave();
		FinishAction();
	}
}
