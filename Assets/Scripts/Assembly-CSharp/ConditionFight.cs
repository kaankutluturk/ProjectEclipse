public class ConditionFight
{
	public ConditionType Type;

	public ConditionSubType SubType;

	public int Count;

	public ConditionCompare Compare;

	private FightIDS _fightIDS;

	public FightIDS FightIds
	{
		get
		{
			return GetFightIds();
		}
	}

	public FightIDS GetFightIds()
	{
		return _fightIDS;
	}

	public void SetFightIds(string DIAIIPCBMFL)
	{
		_fightIDS.SetFightIDSByString(DIAIIPCBMFL);
	}
}
