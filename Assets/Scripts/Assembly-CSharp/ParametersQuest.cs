using System.Xml;

public class ParametersQuest
{
	private string _fightName = string.Empty;

	private string _fightResultName = string.Empty;

	private string _raidResultName = string.Empty;

	private int _levelUp;

	private int _power;

	private int _screenIndex;

	private int _checkPointIndex;

	private float _fightAvgFPS;

	public XmlNode Node;

	public string FightName
	{
		get
		{
			return GetFightName();
		}
		set
		{
			SetFightName(value);
		}
	}

	public string FightResultName
	{
		get
		{
			return GetFightResultName();
		}
		set
		{
			SetFightResultName(value);
		}
	}

	public string RaidResultName
	{
		get
		{
			return GetRaidResultName();
		}
		set
		{
			SetRaidResultName(value);
		}
	}

	public int LevelUp
	{
		get
		{
			return GetLevelUp();
		}
		set
		{
			SetLevelUp(value);
		}
	}

	public int Power
	{
		get
		{
			return GetPower();
		}
		set
		{
			SetPower(value);
		}
	}

	public int ScreenIndex
	{
		get
		{
			return GetScreenIndex();
		}
		set
		{
			SetScreenIndex(value);
		}
	}

	public int CheckPointIndex
	{
		get
		{
			return GetCheckPointIndex();
		}
		set
		{
			SetCheckPointIndex(value);
		}
	}

	public float AverageFightFps
	{
		get
		{
			return GetFightAvgFps();
		}
		set
		{
			set_FightAvgFPS(value);
		}
	}

	public ParametersQuest(XmlNode PKHDLOGJKAD)
	{
		Node = PKHDLOGJKAD;
		if (Node.Attributes["ScreenIndex"] == null)
		{
			Node.AppendAttribute("ScreenIndex").Value = "0";
		}
		if (Node.Attributes["ChekPointIndex"] == null)
		{
			Node.AppendAttribute("ChekPointIndex").Value = "0";
		}
		if (Node["FightResult"] == null)
		{
			Node.AppendElement("FightResult").AppendAttribute("Name");
		}
		if (Node["RaidResult"] == null)
		{
			Node.AppendElement("RaidResult").AppendAttribute("Name");
		}
		if (Node["Fight"] == null)
		{
			Node.AppendElement("Fight").AppendAttribute("Name");
		}
		if (Node["LevelUp"] == null)
		{
			Node.AppendElement("LevelUp").AppendAttribute("Value").Value = "0";
		}
		if (Node["PowerAmount"] == null)
		{
			Node.AppendElement("PowerAmount").AppendAttribute("Value").Value = "0";
		}
		if (Node["FightAvgFPS"] == null)
		{
			Node.AppendElement("FightAvgFPS").AppendAttribute("Value").Value = "0";
		}
		_screenIndex = Node.Attributes["ScreenIndex"].ParseInt();
		_checkPointIndex = Node.Attributes["ChekPointIndex"].ParseInt();
		_fightResultName = Node["FightResult"].Attributes["Name"].GetStringOrDefault(string.Empty);
		_raidResultName = Node["RaidResult"].Attributes["Name"].GetStringOrDefault(string.Empty);
		_fightName = Node["Fight"].Attributes["Name"].GetStringOrDefault(string.Empty);
		_levelUp = Node["LevelUp"].Attributes["Value"].ParseInt();
		_power = Node["PowerAmount"].Attributes["Value"].ParseInt();
		_fightAvgFPS = Node["FightAvgFPS"].Attributes["Value"].ParseFloat();
	}

	public string GetFightName()
	{
		return _fightName;
	}

	public void SetFightName(string value)
	{
		_fightName = value;
		Node["Fight"].Attributes["Name"].Value = ((_fightName == null) ? string.Empty : _fightName);
	}

	public string GetFightResultName()
	{
		return _fightResultName;
	}

	public void SetFightResultName(string value)
	{
		_fightResultName = value;
		Node["FightResult"].Attributes["Name"].Value = ((_fightResultName == null) ? string.Empty : _fightResultName);
	}

	public string GetRaidResultName()
	{
		return _raidResultName;
	}

	public void SetRaidResultName(string value)
	{
		_raidResultName = value;
		Node["RaidResult"].Attributes["Name"].Value = ((_raidResultName == null) ? string.Empty : _raidResultName);
	}

	public int GetLevelUp()
	{
		return _levelUp;
	}

	public void SetLevelUp(int value)
	{
		_levelUp = value;
		Node["LevelUp"].Attributes["Value"].Value = _levelUp.ToString();
	}

	public int GetPower()
	{
		return _power;
	}

	public void SetPower(int value)
	{
		_power = value;
		Node["PowerAmount"].Attributes["Value"].Value = _levelUp.ToString();
	}

	public int GetScreenIndex()
	{
		return _screenIndex;
	}

	public void SetScreenIndex(int value)
	{
		_screenIndex = value;
		Node.Attributes["ScreenIndex"].Value = _screenIndex.ToString();
	}

	public int GetCheckPointIndex()
	{
		return _checkPointIndex;
	}

	public void SetCheckPointIndex(int value)
	{
		_checkPointIndex = value;
		Node.Attributes["ChekPointIndex"].Value = _checkPointIndex.ToString();
	}

	public float GetFightAvgFps()
	{
		return _fightAvgFPS;
	}

	public void set_FightAvgFPS(float value)
	{
		_fightAvgFPS = value;
		Node["FightAvgFPS"].Attributes["Value"].Value = _fightAvgFPS.ToString(System.Globalization.CultureInfo.InvariantCulture);
	}
}
