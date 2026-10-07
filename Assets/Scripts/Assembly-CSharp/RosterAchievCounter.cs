using System.Xml;

public class RosterAchievCounter
{
	private XmlNode _node;

	private string _name = string.Empty;

	private int _counter;

	public int CurrentValue
	{
		get
		{
			return GetCounter();
		}
		set
		{
			set_Counter(value);
		}
	}

	public RosterAchievCounter(XmlNode node)
	{
		_node = node;
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_counter = node.Attributes["CurrentValue"].ParseInt();
	}

	public string get_Name()
	{
		return _name;
	}

	public void set_Name(string value)
	{
		_name = value;
		if (_node.Attributes["Name"] == null)
		{
			_node.AppendAttribute("Name");
		}
		_node.Attributes["Name"].Value = _name;
	}

	public int GetCounter()
	{
		return _counter;
	}

	public void set_Counter(int value)
	{
		_counter = value;
		if (_node.Attributes["CurrentValue"] == null)
		{
			_node.AppendAttribute("CurrentValue");
		}
		_node.Attributes["CurrentValue"].Value = _counter.ToString();
	}
}
