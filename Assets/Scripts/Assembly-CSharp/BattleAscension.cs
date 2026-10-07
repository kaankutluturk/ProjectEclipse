using UnityEngine;

public class BattleAscension : BattleReplayable
{
	public BattleAscension(string typeName, Vector2 MGMMDGFPBLP, string name, string iconName, string previewIcon, string description, ushort rewardDigits, ushort prizeBaseDigits, string alias, string title, string location, string music, string rewardImage, string showResistance)
		: base(typeName, MGMMDGFPBLP, name, iconName, previewIcon, description, rewardDigits, prizeBaseDigits, alias, title, location, music, rewardImage, showResistance)
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

	public new virtual void RefreshFightStatus(FightList fight)
	{
		int num = 0;
		bool flag = false;
		int num2 = ((_rosterBattle == null) ? 1 : _rosterBattle.GetAscensionLevel());
		foreach (FightList item in _fights)
		{
			if (item == fight)
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
				fight.Status = ConditionStatus.StatusComplete;
			}
			else
			{
				fight.Status = ConditionStatus.StatusOpen;
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

	public void AdvanceAscensionAfterFight(FightList fight)
	{
		int num = GetFightIndex(fight);
		if (num >= 0)
		{
			SetAscensionLevel(num + 2);
		}
	}

	public int GetFightIndex(FightList fight)
	{
		int num = 0;
		bool flag = false;
		foreach (FightList item in _fights)
		{
			if (item == fight)
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
