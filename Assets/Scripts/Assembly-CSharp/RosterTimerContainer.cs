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

	public void AddTimer(string name, long MCEDKIPLOMO)
	{
		RosterTimer nFFICPMLCFD = new RosterTimer(name, MCEDKIPLOMO);
		AddTimer(nFFICPMLCFD);
	}

	public void AddTimer(XmlNode node)
	{
		RosterTimer nFFICPMLCFD = new RosterTimer(node);
		AddTimer(nFFICPMLCFD);
	}

	public void AddTimer(RosterTimer NFFICPMLCFD)
	{
		RosterTimer fPNMILOHPMB = FindTimer(NFFICPMLCFD.get_Name());
		if (fPNMILOHPMB == null)
		{
			timers.Add(NFFICPMLCFD);
			return;
		}
		RemoveTimer(fPNMILOHPMB);
		AddTimer(NFFICPMLCFD);
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
		RosterTimer kIKOMNOGKDK = FindTimer(name);
		RemoveTimer(kIKOMNOGKDK);
	}

	public void RemoveTimer(RosterTimer KIKOMNOGKDK)
	{
		if (KIKOMNOGKDK == null)
		{
			return;
		}
		for (int i = 0; i < timers.Count; i++)
		{
			RosterTimer fPNMILOHPMB = timers[i];
			if (fPNMILOHPMB == KIKOMNOGKDK)
			{
				GetNode().RemoveChild(fPNMILOHPMB.GetNode());
				timers.Remove(fPNMILOHPMB);
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

	public void CheckTimers(long LBIGLJLMIDG)
	{
		List<RosterTimer> list = new List<RosterTimer>();
		foreach (RosterTimer item in timers)
		{
			if (item.GetEndTimeSeconds() <= LBIGLJLMIDG)
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
		QuestParameters hHKLFIIBIFF = ListSF.GetInstance().GetQuestParameters();
		hHKLFIIBIFF.timerName = name;
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_TIMER_END))
		{
			ListSF.GetInstance().RunQuestActions();
		}
	}

	public void CancelTimer(string EBGIGEGKIBD)
	{
		RemoveTimer(EBGIGEGKIBD);
	}
}
