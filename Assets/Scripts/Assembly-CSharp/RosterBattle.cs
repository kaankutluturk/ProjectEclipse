using System;
using System.Xml;

public class RosterBattle
{
	private XmlNode _node;

	public Battle LinkedBattle;

	private bool locked;

	private bool hidden;

	public bool RuntimeFlagA;

	public bool RuntimeFlagB;

	private FightIDS battleId = new FightIDS();

	private long _randomRuleSeed;

	private long randomGroupSeed;

	private int replayCount;

	private bool hasRandomRuleSeed;

	private bool hasRandomGroupSeed;

	private bool hasAscensionLevel;

	private int ascensionLevel = -1;

	public bool Locked
	{
		get
		{
			return IsLocked();
		}
		set
		{
			SetLocked(value);
		}
	}

	public bool Hidden
	{
		get
		{
			return IsHidden();
		}
		set
		{
			SetHidden(value);
		}
	}

	public FightIDS BattleId
	{
		get
		{
			return GetBattleId();
		}
		set
		{
			SetBattleId(value);
		}
	}

	public long RandomRuleSeed
	{
		get
		{
			return GetRandomRuleSeed();
		}
		set
		{
			SetRandomRuleSeed(value);
		}
	}

	public long RandomGroupSeed
	{
		get
		{
			return GetRandomGroupSeed();
		}
		set
		{
			SetRandomGroupSeed(value);
		}
	}

	public int ReplayCount
	{
		get
		{
			return GetReplayCount();
		}
		set
		{
			SetReplayCount(value);
		}
	}

	public bool HasRandomRuleSeed
	{
		get
		{
			return IsRandomRuleSeedSet();
		}
	}

	public bool HasRandomGroupSeed
	{
		get
		{
			return IsRandomGroupSeedSet();
		}
	}

	public bool HasAscensionLevel
	{
		get
		{
			return IsAscensionLevelSet();
		}
	}

	public int AscensionLevel
	{
		get
		{
			return GetAscensionLevel();
		}
		set
		{
			SetAscensionLevel(value);
		}
	}

	public RosterBattle(XmlNode node)
	{
		_node = node;
		LinkedBattle = null;
		RuntimeFlagA = false;
		RuntimeFlagB = false;
		randomGroupSeed = 0L;
		_randomRuleSeed = 0L;
		replayCount = 0;
		hasRandomGroupSeed = false;
		hasRandomRuleSeed = false;
		hasAscensionLevel = false;
		ascensionLevel = 1;
		if (_node.Attributes["Name"].Empty())
		{
			_node.AppendAttribute("Name").Value = string.Empty;
		}
		if (_node.Attributes["Locked"].Empty())
		{
			_node.AppendAttribute("Locked").Value = "0";
		}
		locked = _node.Attributes["Locked"].ParseBool();
		hidden = _node.Attributes["Hidden"].ParseBool();
		battleId = new FightIDS();
		battleId.SetFightIDSByString(_node.Attributes["Name"].GetStringOrDefault(string.Empty));
		hasRandomGroupSeed = !node.Attributes["RandomGroupSeed"].Empty();
		hasRandomRuleSeed = !node.Attributes["RandomRuleSeed"].Empty();
		if (hasRandomGroupSeed)
		{
			randomGroupSeed = node.Attributes["RandomGroupSeed"].ParseInt();
		}
		if (hasRandomRuleSeed)
		{
			_randomRuleSeed = node.Attributes["RandomRuleSeed"].ParseInt();
		}
		replayCount = node.Attributes["ReplayCount"].ParseInt();
		hasAscensionLevel = !node.Attributes["Fight"].Empty();
		if (hasAscensionLevel)
		{
			ascensionLevel = node.Attributes["Fight"].ParseInt();
		}
	}

	// best guess for name
	public bool IsLocked()
	{
		return locked;
	}

	// best guess for name
	public void SetLocked(bool value)
	{
		locked = value;
		_node.Attributes["Locked"].Value = Convert.ToInt32(locked).ToString();
	}

	public bool IsHidden()
	{
		return hidden;
	}

	public void SetHidden(bool value)
	{
		hidden = value;
		if (_node.Attributes["Hidden"] == null)
		{
			_node.AppendAttribute("Hidden");
		}
		_node.Attributes["Hidden"].Value = Convert.ToInt32(hidden).ToString();
	}

	// best guess for name
	public FightIDS GetBattleId()
	{
		return battleId;
	}

	public void SetBattleId(FightIDS value)
	{
		battleId = value;
		_node.Attributes["Name"].Value = battleId.ToString();
	}

	public long GetRandomRuleSeed()
	{
		return _randomRuleSeed;
	}

	public void SetRandomRuleSeed(long value)
	{
		hasRandomRuleSeed = true;
		_randomRuleSeed = value;
		if (_node.Attributes["RandomRuleSeed"] == null)
		{
			_node.AppendAttribute("RandomRuleSeed");
		}
		_node.Attributes["RandomRuleSeed"].Value = _randomRuleSeed.ToString();
	}

	public long GetRandomGroupSeed()
	{
		return randomGroupSeed;
	}

	public void SetRandomGroupSeed(long value)
	{
		hasRandomGroupSeed = true;
		randomGroupSeed = value;
		if (_node.Attributes["RandomGroupSeed"] == null)
		{
			_node.AppendAttribute("RandomGroupSeed");
		}
		_node.Attributes["RandomGroupSeed"].Value = randomGroupSeed.ToString();
	}

	public int GetReplayCount()
	{
		return replayCount;
	}

	public void SetReplayCount(int value)
	{
		replayCount = value;
		if (_node.Attributes["ReplayCount"].Empty())
		{
			_node.AppendAttribute("ReplayCount");
		}
		_node.Attributes["ReplayCount"].Value = replayCount.ToString();
	}

	public bool IsRandomRuleSeedSet()
	{
		return hasRandomRuleSeed;
	}

	public bool IsRandomGroupSeedSet()
	{
		return hasRandomGroupSeed;
	}

	public bool IsAscensionLevelSet()
	{
		return hasAscensionLevel;
	}

	public int GetAscensionLevel()
	{
		return ascensionLevel;
	}

	public void SetAscensionLevel(int value)
	{
		hasAscensionLevel = true;
		ascensionLevel = value;
		if (_node.Attributes["Fight"].Empty())
		{
			_node.AppendAttribute("Fight");
		}
		_node.Attributes["Fight"].Value = ascensionLevel.ToString();
	}
}
