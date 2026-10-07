using System.Xml;

public class RosterFight
{
	protected XmlNode _node;

	public FightList LinkedFightList;

	protected int lossCount;

	protected int winCount;

	protected long completionTimestamp;

	protected int _level;

	private int _id;

	private int eclipseWinCount;

	private int eclipseLossCount;

	private int consecutiveLosses;

	private int storyCount;

	private int randomGroupSeed;

	private int _randomRuleSeed;

	public bool HasRandomSeeds;

	private long randomizeTimestamp;

	private long elapsedSinceCompletion;

	private long elapsedSinceRandomize;

	private string _fightIDS = string.Empty;

	public XmlNode Node
	{
		get
		{
			return GetNode();
		}
	}

	public int LossCount
	{
		get
		{
			return GetLossCount();
		}
		set
		{
			SetLossCount(value);
		}
	}

	public int WinCount
	{
		get
		{
			return GetWinCount();
		}
		set
		{
			SetWinCount(value);
		}
	}

	public long CompletionTimestamp
	{
		get
		{
			return GetCompletionTimestamp();
		}
		set
		{
			SetCompletionTimestamp(value);
		}
	}

	public int Level
	{
		get
		{
			return GetLevel();
		}
		set
		{
			SetLevel(value);
		}
	}

	public int Id
	{
		set
		{
			SetId(value);
		}
	}

	public int EclipseWinCount
	{
		get
		{
			return GetEclipseWinCount();
		}
		set
		{
			SetEclipseWinCount(value);
		}
	}

	public int EclipseLossCount
	{
		get
		{
			return GetEclipseLossCount();
		}
		set
		{
			SetEclipseLossCount(value);
		}
	}

	public int ConsecutiveLosses
	{
		get
		{
			return GetConsecutiveLosses();
		}
	}

	public int StoryCount
	{
		get
		{
			return GetStoryCount();
		}
		set
		{
			SetStoryCount(value);
		}
	}

	public int RandomGroupSeed
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

	public int RandomRuleSeed
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

	public long RandomizeTimestamp
	{
		get
		{
			return GetRandomizeTimestamp();
		}
		set
		{
			SetRandomizeTimestamp(value);
		}
	}

	public long Time
	{
		get
		{
			return GetElapsedSinceCompletion();
		}
		set
		{
			UpdateElapsedSinceCompletion(value);
		}
	}

	public long RandomizeElapsedTime
	{
		set
		{
			UpdateElapsedSinceRandomize(value);
		}
	}

	public string FightIdString
	{
		get
		{
			return GetFightIdString();
		}
		set
		{
			set_FightIDS(value);
		}
	}

	public RosterFight(XmlNode node)
	{
		elapsedSinceCompletion = -1L;
		elapsedSinceRandomize = 0L;
		LinkedFightList = null;
		_node = node;
		if (_node.Attributes["ID"].Empty())
		{
			_node.AppendAttribute("ID").Value = "-1";
		}
		if (_node.Attributes["IDS"].Empty())
		{
			_node.AppendAttribute("IDS").Value = "-1|-1|-1";
		}
		if (_node.Attributes["CompletedCount"].Empty())
		{
			_node.AppendAttribute("CompletedCount").Value = "0";
		}
		if (_node.Attributes["LossCount"].Empty())
		{
			_node.AppendAttribute("LossCount").Value = "0";
		}
		if (_node.Attributes["EclipseCompletedCount"].Empty())
		{
			_node.AppendAttribute("EclipseCompletedCount").Value = "0";
		}
		if (_node.Attributes["EclipseLossCount"].Empty())
		{
			_node.AppendAttribute("EclipseLossCount").Value = "0";
		}
		if (_node.Attributes["StoryCount"].Empty())
		{
			_node.AppendAttribute("StoryCount").Value = "0";
		}
		if (_node.Attributes["CompletedTime"].Empty())
		{
			_node.AppendAttribute("CompletedTime").Value = "0";
		}
		if (_node.Attributes["TimeLeft"].Empty())
		{
			_node.AppendAttribute("TimeLeft").Value = "0";
		}
		if (_node.Attributes["RandomizeTimeLeft"].Empty())
		{
			_node.AppendAttribute("RandomizeTimeLeft").Value = "0";
		}
		if (_node.Attributes["Level"].Empty())
		{
			_node.AppendAttribute("Level").Value = "0";
		}
		_fightIDS = _node.Attributes["IDS"].GetStringOrDefault(string.Empty);
		_id = _node.Attributes["ID"].ParseInt();
		winCount = _node.Attributes["CompletedCount"].ParseInt();
		lossCount = _node.Attributes["LossCount"].ParseInt();
		eclipseWinCount = _node.Attributes["EclipseCompletedCount"].ParseInt();
		eclipseLossCount = _node.Attributes["EclipseLossCount"].ParseInt();
		completionTimestamp = _node.Attributes["TimeLeft"].ParseLong(0L);
		randomizeTimestamp = _node.Attributes["RandomizeTimeLeft"].ParseLong(0L);
		storyCount = _node.Attributes["StoryCount"].ParseInt();
		_level = _node.Attributes["Level"].ParseInt();
		randomGroupSeed = _node.Attributes["RandomGroupSeed"].ParseInt();
		_randomRuleSeed = _node.Attributes["RandomRuleSeed"].ParseInt();
		HasRandomSeeds = randomGroupSeed != 0 || _randomRuleSeed != 0;
		consecutiveLosses = 0;
	}

	public XmlNode GetNode()
	{
		return _node;
	}

	// best guess for name
	public int GetLossCount()
	{
		return lossCount;
	}

	public void SetLossCount(int value)
	{
		lossCount = value;
		_node.Attributes["LossCount"].Value = lossCount.ToString();
	}

	// best guess for name
	public int GetWinCount()
	{
		return winCount;
	}

	public void SetWinCount(int value)
	{
		winCount = value;
		_node.Attributes["CompletedCount"].Value = winCount.ToString();
	}

	public long GetCompletionTimestamp()
	{
		return completionTimestamp;
	}

	public void SetCompletionTimestamp(long value)
	{
		completionTimestamp = value;
		_node.Attributes["TimeLeft"].Value = completionTimestamp.ToString();
	}

	public int GetLevel()
	{
		return _level;
	}

	public void SetLevel(int value)
	{
		_level = value;
		_node.Attributes["Level"].Value = _level.ToString();
	}

	public void SetId(int value)
	{
		_id = value;
		_node.Attributes["ID"].Value = _id.ToString();
	}

	public int GetEclipseWinCount()
	{
		return eclipseWinCount;
	}

	public void SetEclipseWinCount(int value)
	{
		eclipseWinCount = value;
		_node.Attributes["EclipseCompletedCount"].Value = eclipseWinCount.ToString();
	}

	public void IncrementEclipseWinCount()
	{
		eclipseWinCount++;
		_node.Attributes["EclipseCompletedCount"].Value = eclipseWinCount.ToString();
	}

	public int GetEclipseLossCount()
	{
		return eclipseLossCount;
	}

	public void SetEclipseLossCount(int value)
	{
		eclipseLossCount = value;
		_node.Attributes["EclipseLossCount"].Value = eclipseLossCount.ToString();
	}

	public void IncrementEclipseLossCount()
	{
		eclipseLossCount++;
		_node.Attributes["EclipseLossCount"].Value = eclipseLossCount.ToString();
	}

	public int GetConsecutiveLosses()
	{
		return consecutiveLosses;
	}

	public int GetStoryCount()
	{
		return storyCount;
	}

	public void SetStoryCount(int value)
	{
		storyCount = value;
		_node.Attributes["StoryCount"].Value = storyCount.ToString();
	}

	private void IncrementStoryCount()
	{
		storyCount++;
		_node.Attributes["StoryCount"].Value = storyCount.ToString();
	}

	public int GetRandomGroupSeed()
	{
		return randomGroupSeed;
	}

	public void SetRandomGroupSeed(int value)
	{
		randomGroupSeed = value;
		if (_node.Attributes["RandomGroupSeed"].Empty())
		{
			_node.AppendAttribute("RandomGroupSeed").Value = randomGroupSeed.ToString();
		}
		else
		{
			_node.Attributes["RandomGroupSeed"].Value = randomGroupSeed.ToString();
		}
	}

	public int GetRandomRuleSeed()
	{
		return _randomRuleSeed;
	}

	public void SetRandomRuleSeed(int value)
	{
		_randomRuleSeed = value;
		if (_node.Attributes["RandomRuleSeed"].Empty())
		{
			_node.AppendAttribute("RandomRuleSeed").Value = _randomRuleSeed.ToString();
		}
		else
		{
			_node.Attributes["RandomRuleSeed"].Value = _randomRuleSeed.ToString();
		}
		if (LinkedFightList != null)
		{
			LinkedFightList.ResetRandomRules();
		}
	}

	public long GetRandomizeTimestamp()
	{
		return randomizeTimestamp;
	}

	public void SetRandomizeTimestamp(long value)
	{
		randomizeTimestamp = value;
		_node.Attributes["RandomizeTimeLeft"].Value = randomizeTimestamp.ToString();
	}

	public long GetElapsedSinceCompletion()
	{
		return elapsedSinceCompletion;
	}

	public void UpdateElapsedSinceCompletion(long value)
	{
		if (completionTimestamp <= 0)
		{
			elapsedSinceCompletion = -1L;
		}
		else
		{
			elapsedSinceCompletion = value - completionTimestamp;
		}
	}

	public void UpdateElapsedSinceRandomize(long value)
	{
		if (randomizeTimestamp <= 0)
		{
			elapsedSinceRandomize = -1L;
		}
		else
		{
			elapsedSinceRandomize = value - randomizeTimestamp;
		}
	}

	public string GetFightIdString()
	{
		return _fightIDS;
	}

	public void set_FightIDS(string value)
	{
		_fightIDS = value;
		_node.Attributes["IDS"].Value = _fightIDS.ToString();
	}

	public string GetBattleName()
	{
		FightIDS fightIds = new FightIDS();
		fightIds.SetFightIDSByString(_fightIDS);
		return fightIds.GetBattle();
	}

	public void RecordWin()
	{
		consecutiveLosses = 0;
		winCount++;
		_node.Attributes["CompletedCount"].Value = winCount.ToString();
	}

	public void RecordLoss()
	{
		consecutiveLosses++;
		lossCount++;
		_node.Attributes["LossCount"].Value = lossCount.ToString();
	}

	public bool IsRepeatAvailable(long value)
	{
		return elapsedSinceCompletion < 0 || elapsedSinceCompletion >= value;
	}

	public void RandomizeSeeds()
	{
		SetRandomGroupSeed(NekkiMath.randomInt(int.MaxValue));
		SetRandomRuleSeed(NekkiMath.randomInt(int.MaxValue));
		HasRandomSeeds = true;
		SetRandomizeTimestamp(ListSF.GetCurrentTime());
		ListSF.GetInstance().RequestSave();
	}

	public bool RerandomizeIfElapsed(long interval)
	{
		if (elapsedSinceRandomize >= interval || elapsedSinceRandomize == -1)
		{
			RandomizeSeeds();
			return true;
		}
		return false;
	}
}
