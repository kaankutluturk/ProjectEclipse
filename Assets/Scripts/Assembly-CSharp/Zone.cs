using System.Collections.Generic;

public class Zone
{
	private bool _isStart;

	private string _name;

	private string _fileName;

	public ConditionStatus Status;

	private int _index;

	public List<Battle> Battles = new List<Battle>();

	private uint rewardDigits;

	private uint prizeBaseDigits;

	public bool IsStart
	{
		get
		{
			return GetIsStart();
		}
	}

	public string FileName
	{
		get
		{
			return GetFileName();
		}
	}

	public int Index
	{
		get
		{
			return GetIndex();
		}
	}

	public uint RewardDigits
	{
		get
		{
			return GetRewardDigits();
		}
	}

	public uint PrizeBaseDigits
	{
		get
		{
			return GetPrizeBaseDigits();
		}
	}

	public Zone(string name, string PMFEIPCHENB, bool PENNHKHFEOM = false, ConditionStatus status = ConditionStatus.StatusOpen, int index = 0, uint CDCJKJNGPOE = 0u, uint MCDAHGPLLDO = 0u)
	{
		_name = name;
		_fileName = PMFEIPCHENB;
		_isStart = PENNHKHFEOM;
		Status = status;
		_index = index;
		rewardDigits = CDCJKJNGPOE;
		prizeBaseDigits = MCDAHGPLLDO;
	}

	public bool GetIsStart()
	{
		return _isStart;
	}

	public string get_Name()
	{
		return _name;
	}

	public string GetFileName()
	{
		return _fileName;
	}

	public int GetIndex()
	{
		return _index;
	}

	public uint GetRewardDigits()
	{
		return rewardDigits;
	}

	public uint GetPrizeBaseDigits()
	{
		return prizeBaseDigits;
	}

	public Battle FindBattle(string name)
	{
		foreach (Battle lGIIBNJFADum in Battles)
		{
			if (lGIIBNJFADum.get_Name() == name)
			{
				return lGIIBNJFADum;
			}
		}
		GameLog.Write("Error: battle with name=" + name + " not found");
		return null;
	}

	public List<Battle> FindBattlesByType(BattleType LFLGCDNKNJI)
	{
		List<Battle> list = new List<Battle>();
		foreach (Battle lGIIBNJFADum in Battles)
		{
			if (lGIIBNJFADum.get_Type() == LFLGCDNKNJI)
			{
				list.Add(lGIIBNJFADum);
			}
		}
		if (list.Count == 0)
		{
			GameLog.Write("Error: _battles with type={0} not found", LFLGCDNKNJI);
		}
		return list;
	}

	public void UpdateStatus()
	{
		int num = 0;
		bool flag = false;
		int i = 0;
		for (int count = Battles.Count; i < count; i++)
		{
			Battle cGJCGEBPCAF = Battles[i];
			List<FightList> list = cGJCGEBPCAF.GetLoadedFights();
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				num++;
				ConditionStatus pGBKNLAEANJ = list[j].Status;
				if (pGBKNLAEANJ == ConditionStatus.StatusOpen || pGBKNLAEANJ == ConditionStatus.StatusComplete)
				{
					Status = ConditionStatus.StatusOpen;
					flag = true;
					break;
				}
			}
		}
		if (num == 0 || !flag)
		{
			Status = ConditionStatus.StatusIncomplete;
		}
	}

	public void SetTime(long time)
	{
		int i = 0;
		for (int count = Battles.Count; i < count; i++)
		{
			Battles[i].SetTime(time);
		}
	}
}
