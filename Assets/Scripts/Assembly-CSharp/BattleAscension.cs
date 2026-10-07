using UnityEngine;

public class BattleAscension : BattleReplayable
{
	public BattleAscension(string LFLGCDNKNJI, Vector2 MGMMDGFPBLP, string name, string ADONPNOBBDE, string LHCFHAIDNDP, string EMDJGBHIAIA, ushort CDCJKJNGPOE, ushort MCDAHGPLLDO, string LOKLDPLAPOL, string PEMOECLNECD, string LPJNEDFCBOI, string PINIIFIOECE, string OAPKHNPPGHP, string IHBMPGKIBAN)
		: base(LFLGCDNKNJI, MGMMDGFPBLP, name, ADONPNOBBDE, LHCFHAIDNDP, EMDJGBHIAIA, CDCJKJNGPOE, MCDAHGPLLDO, LOKLDPLAPOL, PEMOECLNECD, LPJNEDFCBOI, PINIIFIOECE, OAPKHNPPGHP, IHBMPGKIBAN)
	{
	}

	public void RefreshAllFightStatuses()
	{
		int num = 0;
		if (!_fightsParsed)
		{
			LoadAllFights();
		}
		int num2 = GetAscensionLevel();
		foreach (FightList item in _fights)
		{
			if (num + 1 < num2)
			{
				item.Status = ConditionStatus.StatusComplete;
			}
			else
			{
				item.Status = ConditionStatus.StatusOpen;
			}
			num++;
		}
	}

	public new virtual void RefreshFightStatus(FightList KGKDKENMAOA)
	{
		int num = 0;
		bool flag = false;
		int num2 = ((_rosterBattle == null) ? 1 : _rosterBattle.GetAscensionLevel());
		foreach (FightList item in _fights)
		{
			if (item == KGKDKENMAOA)
			{
				flag = true;
				break;
			}
			num++;
		}
		if (flag)
		{
			if (num + 1 < num2)
			{
				KGKDKENMAOA.Status = ConditionStatus.StatusComplete;
			}
			else
			{
				KGKDKENMAOA.Status = ConditionStatus.StatusOpen;
			}
		}
	}

	public new virtual void LoadAllFights()
	{
		base.LoadAllFights();
		RefreshAllFightStatuses();
	}

	public int GetAscensionLevel()
	{
		return (_rosterBattle == null) ? 1 : _rosterBattle.GetAscensionLevel();
	}

	public void SetAscensionLevel(int value)
	{
		if (_rosterBattle != null)
		{
			_rosterBattle.SetAscensionLevel(value);
		}
		ListSF.GetInstance().RequestSave();
	}

	public void AdvanceAscensionAfterFight(FightList KGKDKENMAOA)
	{
		int num = GetFightIndex(KGKDKENMAOA);
		if (num >= 0)
		{
			SetAscensionLevel(num + 2);
		}
	}

	public int GetFightIndex(FightList KGKDKENMAOA)
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
		if (flag)
		{
			return num;
		}
		return -1;
	}
}
