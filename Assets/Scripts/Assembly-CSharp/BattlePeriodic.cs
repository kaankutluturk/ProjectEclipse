using System.Collections.Generic;
using Nekki.SF2.GUI;
using Nekki.SF2.GUI.Map;
using Nekki.Utils;
using UnityEngine;

public class BattlePeriodic : Battle
{
	protected static List<BattlePeriodic> _battles = new List<BattlePeriodic>();

	private static long _elapsedTime = 0L;

	private static long _repeatTime = 0L;

	protected static long _startTime;

	public static long Time
	{
		get
		{
			return GetTime();
		}
	}

	public static long RepeatTime
	{
		get
		{
			return GetRepeatTime();
		}
	}

	public BattlePeriodic(string typeName, Vector2 MGMMDGFPBLP, string name, string iconName, string previewIcon, string description, ushort rewardDigits, ushort prizeBaseDigits, string alias, string title, string location, string music, string rewardImage, string showResistance)
		: base(typeName, MGMMDGFPBLP, name, iconName, previewIcon, description, rewardDigits, prizeBaseDigits, alias, title, location, music, rewardImage, showResistance)
	{
		_battles.Add(this);
	}

	public static long GetTime()
	{
		return _elapsedTime;
	}

	public override void SetTime(long value)
	{
		_elapsedTime = ((_startTime > 0) ? (value - _startTime) : (-1));
		foreach (FightList item in _fights)
		{
			item.SetTime(value);
		}
	}

	public static long GetRepeatTime()
	{
		return _repeatTime;
	}

	protected void ApplyFightTime(long time)
	{
		FightList fightList = GetFirstOpenFight();
		if (fightList == null)
		{
			Reset();
			if (_fights.Count > 0)
			{
				fightList = _fights[0];
			}
		}
		if (fightList != null)
		{
			Roster roster = ListSF.GetRoster();
			RosterFight rosterFight = fightList.GetRosterFight();
			if (rosterFight == null)
			{
				rosterFight = roster.CreateFight(fightList.FightId);
				fightList.SetRosterFight(rosterFight);
			}
			rosterFight.SetCompletionTimestamp(time);
			// The map's availability check reads elapsed runtime state, while the
			// line above only persists the completion timestamp. Keep both in sync
			// so a finished duel locks and displays its timer immediately.
			rosterFight.UpdateElapsedSinceCompletion(time);
			ListSF.GetInstance().RequestSave();
		}
	}

	protected void ResetSingle(bool useLatestTimestamp = true)
	{
		long num = 0L;
		foreach (FightList item in _fights)
		{
			item.Status = ConditionStatus.StatusOpen;
			RosterFight rosterFight = item.GetRosterFight();
			if (rosterFight != null && useLatestTimestamp && rosterFight.GetCompletionTimestamp() > num)
			{
				num = rosterFight.GetCompletionTimestamp();
			}
		}
		foreach (FightList item2 in _fights)
		{
			RosterFight pIGKOIFBOME2 = item2.GetRosterFight();
			if (pIGKOIFBOME2 != null)
			{
				pIGKOIFBOME2.SetCompletionTimestamp(num);
				pIGKOIFBOME2.SetRandomizeTimestamp(0L);
			}
		}
		ListSF.GetInstance().RequestSave();
	}

	protected void SetRepeatTime(long time)
	{
		_repeatTime = time;
		foreach (FightList item in _fights)
		{
			item.RepeatTime = time;
		}
	}

	public override void AddFight(FightList fightList, int index)
	{
		base.AddFight(fightList, index);
		SetTime(GlobalTimer.get_GetTime());
	}

	public override void UpdateByTime(long time)
	{
		ApplyFightTime(time);
		foreach (BattlePeriodic item in _battles)
		{
			if (item != this)
			{
				item.ApplyFightTime(time);
			}
		}
		_startTime = time;
		ListSF.GetRoster().SetPeriodicPlayTime(time);
	}

	public override void UpdateRosterFight(FightList fightList, bool createIfMissing)
	{
		int num = 0;
		bool flag = false;
		foreach (FightList item in _fights)
		{
			if (item == fightList)
			{
				flag = true;
				break;
			}
			num++;
		}
		if (!flag)
		{
			GameLog.Error("BattleDaily::setRosterFight ERROR - no fightList found in _fights");
		}
		else
		{
			fightList.SetRosterFight(ListSF.LoadRosterFight(fightList, createIfMissing));
		}
	}

	public static void Reset(bool useLatestTimestamp = true)
	{
		foreach (BattlePeriodic item in _battles)
		{
			item.ResetSingle(useLatestTimestamp);
			item.SetTime(GlobalTimer.get_GetTime());
		}
		ListSF.GetRoster().SetPeriodicPlayTime(0L);
		MapScene current2 = Scene<MapScene>.get_Current();
		if (current2 != null)
		{
			current2.UpdateInfoBattle();
		}
	}

	public static void InitBattles(int battleIndex, long startTime)
	{
		_startTime = startTime;
		bool flag = false;
		foreach (BattlePeriodic item in _battles)
		{
			item.ApplyFightTime(startTime);
			if (GameUtils.DailyDebugMode)
			{
				item.SetRepeatTime(GameUtils.DailyDebugTime);
			}
			if (!flag && item.GetFightCount() > 0)
			{
				_repeatTime = item.GetFightByIndex(0).RepeatTime;
				flag = true;
			}
		}
		if (!flag)
		{
			GameLog.Error("BattlePeriodic::initBattles WARNING - no duel fights found, repeatTime has not been set!");
		}
	}

	public static void Clear()
	{
		_battles.Clear();
		_elapsedTime = 0L;
	}
}
