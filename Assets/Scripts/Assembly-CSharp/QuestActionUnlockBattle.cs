using System.Collections.Generic;
using System.Xml;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Map;

public class QuestActionUnlockBattle : QuestAction
{
	private bool _toggle;

	private bool isLocked;

	private bool isInstant;

	private bool isHidden;

	private string _name = string.Empty;

	private string replayCountExpression = string.Empty;

	private string _hidden = string.Empty;

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		string name = node.Name;
		_toggle = name == "ShowBattle";
		isLocked = node.Attributes["Locked"].ParseBool();
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		isInstant = node.Attributes["Instant"].ParseBool();
		_hidden = node.Attributes["Hidden"].GetStringOrDefault(string.Empty);
		replayCountExpression = node.Attributes["ReplayCount"].GetStringOrDefault(string.Empty);
	}

	public override void Execute(QuestParameters parameters)
	{
		base.Execute(parameters);
		ConditionExtension.CompareResult result = new ConditionExtension.CompareResult();
		QuestCondition condition = new QuestCondition();
		condition.SetParameters(parameters);
		condition.SetValue(_name, result);
		int replayCount = 0;
		if (!string.IsNullOrEmpty(replayCountExpression))
		{
			ConditionExtension.CompareResult replayCountResult = new ConditionExtension.CompareResult();
			condition.SetValue(replayCountExpression, replayCountResult);
			replayCount = (int)replayCountResult.resultNumber;
		}
		if (!string.IsNullOrEmpty(_hidden))
		{
			// 2.41.x intermission data uses this compact infix form.  The legacy
			// quest evaluator only understands ?Sub(...), so it used to parse as
			// false and exposed Eclipse battles while the mode was disabled.
			if (_hidden.Trim() == "1 - _$InEclipseMode")
			{
				Roster roster = ListSF.GetRoster();
				isHidden = roster == null || !roster.IsEclipseMode();
			}
			else
			{
				ConditionExtension.CompareResult hiddenResult = new ConditionExtension.CompareResult();
				condition.SetValue(_hidden, hiddenResult);
				isHidden = hiddenResult.resultNumber > 0.0;
			}
		}
		FightIDS fightIds = new FightIDS();
		fightIds.SetFightIDSByString(result.resultSTR);
		Roster playerRoster = ListSF.GetRoster();
		playerRoster.AddBattle(fightIds, true, _toggle, isLocked, isHidden, replayCount);
		ListSF.GetInstance().RequestSave();
		ListSF.RefreshConditionStatuses();
		Zone zone = ListSF.GetZoneByName(fightIds.GetZone());
		bool flag = HasVisibleBattle(zone);
		Battle battle = ((zone == null) ? null : zone.FindBattle(fightIds.GetBattle()));
		if (battle != null)
		{
			battle.IsMapVisible = _toggle;
		}
		bool flag2 = HasVisibleBattle(zone);
		MapScene current = Scene<MapScene>.get_Current();
		if (current != null)
		{
			if ((!flag && flag2) || (flag && !flag2))
			{
				current.ReloadZones();
			}
			current.ActiveBattleByFightIDS(fightIds, _toggle, false, isInstant);
		}
		FinishAction();
	}

	private bool HasVisibleBattle(Zone zone)
	{
		if (zone == null)
		{
			return false;
		}
		List<Battle> battles = zone.Battles;
		for (int i = 0; i < battles.Count; i++)
		{
			if (battles[i].IsMapVisible)
			{
				return true;
			}
		}
		return false;
	}
}
