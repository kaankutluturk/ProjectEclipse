using System.Diagnostics;
using System.Xml;

public class RosterTimer
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string name;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private long endTimeSeconds;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private XmlNode node;

	public long EndTime
	{
		get
		{
			return GetEndTimeSeconds();
		}
		set
		{
			set_EndTimeSeconds(value);
		}
	}

	public RosterTimer(string name, long endTime)
	{
		set_Name(name);
		set_EndTimeSeconds(endTime);
		Roster roster = ListSF.GetRoster();
		RosterTimerContainer timerContainer = roster.GetTimerContainer();
		XmlNode timersNode = timerContainer.GetNode();
		set_Node(timersNode.AppendElement("Timer"));
		GetNode().AppendAttribute("Name").Value = get_Name();
		GetNode().AppendAttribute("EndTime").Value = ((ulong)GetEndTimeSeconds()/*cast due to constrained. prefix*/).ToString();
	}

	public RosterTimer(XmlNode node)
	{
		set_Node(node);
		if (GetNode().Attributes["Name"].Empty())
		{
			GetNode().AppendAttribute("Name").Value = string.Empty;
		}
		if (GetNode().Attributes["EndTime"].Empty())
		{
			GetNode().AppendAttribute("EndTime").Value = "0";
		}
		set_Name(GetNode().Attributes["Name"].GetStringOrDefault(string.Empty));
		set_EndTimeSeconds(GetNode().Attributes["EndTime"].ParseLong(0L));
	}

	public string get_Name()
	{
		return name;
	}

	private void set_Name(string value)
	{
		name = value;
	}

	public long GetEndTimeSeconds()
	{
		return endTimeSeconds;
	}

	public void set_EndTimeSeconds(long value)
	{
		endTimeSeconds = value;
	}

	public XmlNode GetNode()
	{
		return node;
	}

	private void set_Node(XmlNode value)
	{
		node = value;
	}
}
