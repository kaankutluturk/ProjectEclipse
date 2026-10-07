public class FightIDS
{
	private string zone;

	private string battle;

	private string fightIdRaw;

	private string fullId;

	public string Zone
	{
		get
		{
			return GetZone();
		}
	}

	public string Battle
	{
		get
		{
			return GetBattle();
		}
	}

	public string FightIdentifier
	{
		get
		{
			return GetFight();
		}
	}

	public FightIDS()
	{
		Clear();
	}

	public FightIDS(string DIAIIPCBMFL)
	{
		SetFightIDSByString(DIAIIPCBMFL);
	}

	public FightIDS(FightIDS MMEJHKCKFDD)
	{
		SetFightIDSByString(MMEJHKCKFDD.ToString());
	}

	public FightIDS(string HLJKOKMKMLM, string DPOOIONCEOA, string fight)
	{
		SetFightIDSByZBF(HLJKOKMKMLM, DPOOIONCEOA, fight);
	}

	public string GetZone()
	{
		return zone;
	}

	public string GetBattle()
	{
		return battle;
	}

	public string GetFight()
	{
		return fightIdRaw;
	}

	public override string ToString()
	{
		return fullId;
	}

	public string GetZoneBattle()
	{
		return zone + '|' + battle;
	}

	public void SetFightIDSByString(string value)
	{
		if (value != null)
		{
			string[] array = value.Split('|');
			int num = array.Length;
			zone = ((num <= 0) ? string.Empty : array[0]);
			battle = ((num <= 1) ? string.Empty : array[1]);
			fightIdRaw = ((num <= 2) ? string.Empty : array[2]);
			UpdateFullId();
		}
	}

	public void SetFightIDSByZBF(string HLJKOKMKMLM, string DPOOIONCEOA, string fight)
	{
		zone = ((HLJKOKMKMLM == null) ? string.Empty : HLJKOKMKMLM);
		battle = ((DPOOIONCEOA == null) ? string.Empty : DPOOIONCEOA);
		fightIdRaw = ((fight == null) ? string.Empty : fight);
		UpdateFullId();
	}

	public bool Equals(string DIAIIPCBMFL)
	{
		return ToString() == DIAIIPCBMFL;
	}

	public bool Equals(FightIDS DIAIIPCBMFL)
	{
		return Equals(DIAIIPCBMFL.ToString());
	}

	public bool Equals(string HLJKOKMKMLM, string DPOOIONCEOA, string fight)
	{
		return HLJKOKMKMLM.Equals(zone) && DPOOIONCEOA.Equals(battle) && fight.Equals(fightIdRaw);
	}

	public bool EqualsZoneBattle(string DIAIIPCBMFL)
	{
		string[] array = DIAIIPCBMFL.Split('|');
		int num = array.Length;
		string hLJKOKMKMLM = ((num <= 0) ? string.Empty : array[0]);
		string dPOOIONCEOA = ((num <= 1) ? string.Empty : array[1]);
		return EqualsZoneBattle(hLJKOKMKMLM, dPOOIONCEOA);
	}

	public bool EqualsZoneBattle(string HLJKOKMKMLM, string DPOOIONCEOA)
	{
		return HLJKOKMKMLM.Equals(zone) && DPOOIONCEOA.Equals(battle);
	}

	public void Clear()
	{
		zone = string.Empty;
		battle = string.Empty;
		fightIdRaw = string.Empty;
		fullId = string.Empty;
	}

	public bool IsEmpty()
	{
		return zone.Equals(string.Empty) && battle.Equals(string.Empty);
	}

	public static FightIDS Empty()
	{
		FightIDS mOCEDDJOAEB = new FightIDS();
		mOCEDDJOAEB.Clear();
		return mOCEDDJOAEB;
	}

	private void UpdateFullId()
	{
		fullId = zone + "|" + battle + "|" + fightIdRaw;
	}
}
