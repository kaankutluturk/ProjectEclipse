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

	public BattlePeriodic(string LFLGCDNKNJI, Vector2 MGMMDGFPBLP, string name, string ADONPNOBBDE, string LHCFHAIDNDP, string EMDJGBHIAIA, ushort CDCJKJNGPOE, ushort MCDAHGPLLDO, string LOKLDPLAPOL, string PEMOECLNECD, string LPJNEDFCBOI, string PINIIFIOECE, string OAPKHNPPGHP, string IHBMPGKIBAN)
		: base(LFLGCDNKNJI, MGMMDGFPBLP, name, ADONPNOBBDE, LHCFHAIDNDP, EMDJGBHIAIA, CDCJKJNGPOE, MCDAHGPLLDO, LOKLDPLAPOL, PEMOECLNECD, LPJNEDFCBOI, PINIIFIOECE, OAPKHNPPGHP, IHBMPGKIBAN)
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
		FightList jDIPBIHBGPF = GetFirstOpenFight();
		if (jDIPBIHBGPF == null)
		{
			Reset();
			if (_fights.Count > 0)
			{
				jDIPBIHBGPF = _fights[0];
			}
		}
		if (jDIPBIHBGPF != null)
		{
			Roster nKGLHEGIKKP = ListSF.GetRoster();
			RosterFight pIGKOIFBOME = jDIPBIHBGPF.GetRosterFight();
			if (pIGKOIFBOME == null)
			{
				pIGKOIFBOME = nKGLHEGIKKP.CreateFight(jDIPBIHBGPF.FightId);
				jDIPBIHBGPF.SetRosterFight(pIGKOIFBOME);
			}
			pIGKOIFBOME.SetCompletionTimestamp(time);
			// The map's availability check reads elapsed runtime state, while the
			// line above only persists the completion timestamp. Keep both in sync
			// so a finished duel locks and displays its timer immediately.
			pIGKOIFBOME.UpdateElapsedSinceCompletion(time);
			ListSF.GetInstance().RequestSave();
		}
	}

	protected void ResetSingle(bool IKINMKHLDIB = true)
	{
		long num = 0L;
		foreach (FightList item in _fights)
		{
			item.Status = ConditionStatus.StatusOpen;
			RosterFight pIGKOIFBOME = item.GetRosterFight();
			if (pIGKOIFBOME != null && IKINMKHLDIB && pIGKOIFBOME.GetCompletionTimestamp() > num)
			{
				num = pIGKOIFBOME.GetCompletionTimestamp();
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

	public override void AddFight(FightList KGKDKENMAOA, int index)
	{
		base.AddFight(KGKDKENMAOA, index);
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

	public override void UpdateRosterFight(FightList KGKDKENMAOA, bool FFIBGBMOMPD)
	{
		int num = 0;
		bool flag = false;
		foreach (FightList item in _fights)
		{
			if (item == KGKDKENMAOA)
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
			KGKDKENMAOA.SetRosterFight(ListSF.LoadRosterFight(KGKDKENMAOA, FFIBGBMOMPD));
		}
	}

	public static void Reset(bool IKINMKHLDIB = true)
	{
		foreach (BattlePeriodic item in _battles)
		{
			item.ResetSingle(IKINMKHLDIB);
			item.SetTime(GlobalTimer.get_GetTime());
		}
		ListSF.GetRoster().SetPeriodicPlayTime(0L);
		MapScene current2 = Scene<MapScene>.get_Current();
		if (current2 != null)
		{
			current2.UpdateInfoBattle();
		}
	}

	public static void InitBattles(int BLGLACLODID, long ICBOBIILOFE)
	{
		_startTime = ICBOBIILOFE;
		bool flag = false;
		foreach (BattlePeriodic item in _battles)
		{
			item.ApplyFightTime(ICBOBIILOFE);
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
