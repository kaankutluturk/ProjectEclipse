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

	public FightIDS(string idText)
	{
		SetFightIDSByString(idText);
	}

	public FightIDS(FightIDS other)
	{
		SetFightIDSByString(other.ToString());
	}

	public FightIDS(string zoneId, string battleId, string fight)
	{
		SetFightIDSByZBF(zoneId, battleId, fight);
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

	public void SetFightIDSByZBF(string zoneId, string battleId, string fight)
	{
		zone = ((zoneId == null) ? string.Empty : zoneId);
		battle = ((battleId == null) ? string.Empty : battleId);
		fightIdRaw = ((fight == null) ? string.Empty : fight);
		UpdateFullId();
	}

	public bool Equals(string idText)
	{
		return ToString() == idText;
	}

	public bool Equals(FightIDS other)
	{
		return Equals(other.ToString());
	}

	public bool Equals(string zoneId, string battleId, string fight)
	{
		return zoneId.Equals(zone) && battleId.Equals(battle) && fight.Equals(fightIdRaw);
	}

	public bool EqualsZoneBattle(string zoneBattleText)
	{
		string[] array = zoneBattleText.Split('|');
		int num = array.Length;
		string zoneId = ((num <= 0) ? string.Empty : array[0]);
		string battleId = ((num <= 1) ? string.Empty : array[1]);
		return EqualsZoneBattle(zoneId, battleId);
	}

	public bool EqualsZoneBattle(string zoneId, string battleId)
	{
		return zoneId.Equals(zone) && battleId.Equals(battle);
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
		FightIDS empty = new FightIDS();
		empty.Clear();
		return empty;
	}

	private void UpdateFullId()
	{
		fullId = zone + "|" + battle + "|" + fightIdRaw;
	}
}
