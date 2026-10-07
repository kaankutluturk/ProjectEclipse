using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class BattleReplayable : Battle
{
	protected List<Rule> _rules = new List<Rule>();

	protected bool _rulesAndWarriorsSet;

	public BattleReplayable(string typeName, Vector2 MGMMDGFPBLP, string name, string iconName, string previewIcon, string description, ushort rewardDigits, ushort prizeBaseDigits, string alias, string title, string location, string music, string rewardImage, string showResistance)
		: base(typeName, MGMMDGFPBLP, name, iconName, previewIcon, description, rewardDigits, prizeBaseDigits, alias, title, location, music, rewardImage, showResistance)
	{
		_rulesAndWarriorsSet = false;
	}

	public void Parse(XmlNode node)
	{
		XmlNode rulesNode = node["Rules"];
		ParseRules(rulesNode);
	}

	public int GetCompletedCycles()
	{
		return (_rosterBattle != null) ? _rosterBattle.GetReplayCount() : 0;
	}

	public bool TryStartNextReplay()
	{
		if (_rosterBattle == null || _rosterBattle.IsLocked())
		{
			return false;
		}
		List<FightList> fights = GetFights();
		if (fights.Count == 0)
		{
			return false;
		}
		// Wins are lifetime counters used by rewards and quests. Advance the
		// cycle instead of clearing them, and only after EVERY fight is done.
		// Deriving the cycle from saved wins also repairs already completed saves.
		int completedCycles = int.MaxValue;
		foreach (FightList fight in fights)
		{
			RosterFight rosterFight = fight.GetRosterFight();
			if (rosterFight == null || fight.ReplayCount <= 0)
			{
				return false;
			}
			completedCycles = System.Math.Min(completedCycles, rosterFight.GetWinCount() / fight.ReplayCount);
		}
		if (completedCycles <= GetCompletedCycles())
		{
			return false;
		}
		_rosterBattle.SetReplayCount(completedCycles);
		foreach (FightList fight in fights)
		{
			ListSF.UpdateFightStatus(fight);
		}
		return true;
	}

	public virtual void RefreshFightStatus(FightList fightList)
	{
		int num = GetCompletedCycles();
		RosterFight rosterFight = fightList.GetRosterFight();
		int requiredWins = fightList.ReplayCount;
		if (rosterFight != null)
		{
			if (rosterFight.GetWinCount() >= requiredWins * (num + 1))
			{
				fightList.Status = ConditionStatus.StatusComplete;
			}
			else
			{
				fightList.Status = ConditionStatus.StatusOpen;
			}
		}
	}

	protected void ParseRules(XmlNode node)
	{
		RuleParser.ParseRules(node, _rules);
	}
}
