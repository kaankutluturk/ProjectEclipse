using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class BattleReplayable : Battle
{
	protected List<Rule> _rules = new List<Rule>();

	protected bool _rulesAndWarriorsSet;

	public BattleReplayable(string LFLGCDNKNJI, Vector2 MGMMDGFPBLP, string name, string ADONPNOBBDE, string LHCFHAIDNDP, string EMDJGBHIAIA, ushort CDCJKJNGPOE, ushort MCDAHGPLLDO, string LOKLDPLAPOL, string PEMOECLNECD, string LPJNEDFCBOI, string PINIIFIOECE, string OAPKHNPPGHP, string IHBMPGKIBAN)
		: base(LFLGCDNKNJI, MGMMDGFPBLP, name, ADONPNOBBDE, LHCFHAIDNDP, EMDJGBHIAIA, CDCJKJNGPOE, MCDAHGPLLDO, LOKLDPLAPOL, PEMOECLNECD, LPJNEDFCBOI, PINIIFIOECE, OAPKHNPPGHP, IHBMPGKIBAN)
	{
		_rulesAndWarriorsSet = false;
	}

	public void Parse(XmlNode node)
	{
		XmlNode hKPPBKPJOEO = node["Rules"];
		ParseRules(hKPPBKPJOEO);
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

	public virtual void RefreshFightStatus(FightList KGKDKENMAOA)
	{
		int num = GetCompletedCycles();
		RosterFight pIGKOIFBOME = KGKDKENMAOA.GetRosterFight();
		int eJGGHHEOGPG = KGKDKENMAOA.ReplayCount;
		if (pIGKOIFBOME != null)
		{
			if (pIGKOIFBOME.GetWinCount() >= eJGGHHEOGPG * (num + 1))
			{
				KGKDKENMAOA.Status = ConditionStatus.StatusComplete;
			}
			else
			{
				KGKDKENMAOA.Status = ConditionStatus.StatusOpen;
			}
		}
	}

	protected void ParseRules(XmlNode node)
	{
		RuleParser.ParseRules(node, _rules);
	}
}
