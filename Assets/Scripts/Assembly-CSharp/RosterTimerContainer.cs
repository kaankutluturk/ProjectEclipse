using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class RosterTimerContainer
{
	public enum TimerEventType
	{
		TIMER_END = 0
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private XmlNode node;

	private List<RosterTimer> timers = new List<RosterTimer>();

	public RosterTimerContainer(XmlNode node)
	{
		set_Node(node);
		foreach (XmlNode childNode in GetNode().ChildNodes)
		{
			AddTimer(childNode);
		}
	}

	public XmlNode GetNode()
	{
		return node;
	}

	protected void set_Node(XmlNode value)
	{
		node = value;
	}

	public List<RosterTimer> GetTimers()
	{
		return timers;
	}

	public void AddTimer(string name, long endTime)
	{
		RosterTimer timer = new RosterTimer(name, endTime);
		AddTimer(timer);
	}

	public void AddTimer(XmlNode node)
	{
		RosterTimer timer = new RosterTimer(node);
		AddTimer(timer);
	}

	public void AddTimer(RosterTimer timer)
	{
		RosterTimer existingTimer = FindTimer(timer.get_Name());
		if (existingTimer == null)
		{
			timers.Add(timer);
			return;
		}
		RemoveTimer(existingTimer);
		AddTimer(timer);
	}

	public RosterTimer FindTimer(string name)
	{
		foreach (RosterTimer item in timers)
		{
			if (item.get_Name().Equals(name))
			{
				return item;
			}
		}
		return null;
	}

	public void RemoveTimer(string name)
	{
		RosterTimer timer = FindTimer(name);
		RemoveTimer(timer);
	}

	public void RemoveTimer(RosterTimer timer)
	{
		if (timer == null)
		{
			return;
		}
		for (int i = 0; i < timers.Count; i++)
		{
			RosterTimer candidate = timers[i];
			if (candidate == timer)
			{
				GetNode().RemoveChild(candidate.GetNode());
				timers.Remove(candidate);
				break;
			}
		}
	}

	public void ResetNode()
	{
		XmlNode parentNode = GetNode().ParentNode;
		string name = GetNode().Name;
		parentNode.RemoveChild(GetNode());
		set_Node(parentNode.AppendElement(name));
	}

	public void CheckTimers(long currentTime)
	{
		List<RosterTimer> list = new List<RosterTimer>();
		foreach (RosterTimer item in timers)
		{
			if (item.GetEndTimeSeconds() <= currentTime)
			{
				list.Add(item);
				FireTimerEnd(item);
			}
		}
		foreach (RosterTimer item2 in list)
		{
			RemoveTimer(item2);
		}
		list.Clear();
	}

	public void FireTimerEnd(RosterTimer timer)
	{
		FireTimerEnd(timer.get_Name());
	}

	public void FireTimerEnd(string name)
	{
		QuestParameters questParameters = ListSF.GetInstance().GetQuestParameters();
		questParameters.timerName = name;
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_TIMER_END))
		{
			ListSF.GetInstance().RunQuestActions();
		}
	}

	public void CancelTimer(string timerName)
	{
		RemoveTimer(timerName);
	}
}
