using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class PerkTrigger
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string name;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private PerkInfoItem perk;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkEvent> events;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkCondition> conditions;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private List<PerkAction> actions;

	public PerkInfoItem PerkInfo
	{
		get
		{
			return GetPerk();
		}
		set
		{
			SetPerk(value);
		}
	}

	public List<PerkEvent> Events
	{
		get
		{
			return GetEvents();
		}
		protected set
		{
			SetEvents(value);
		}
	}

	public List<PerkCondition> Conditions
	{
		get
		{
			return GetConditions();
		}
		protected set
		{
			SetConditions(value);
		}
	}

	public List<PerkAction> Actions
	{
		get
		{
			return GetActions();
		}
		protected set
		{
			SetActions(value);
		}
	}

	public string get_Name()
	{
		return name;
	}

	protected void set_Name(string value)
	{
		name = value;
	}

	public PerkInfoItem GetPerk()
	{
		return perk;
	}

	public void SetPerk(PerkInfoItem value)
	{
		perk = value;
	}

	public List<PerkEvent> GetEvents()
	{
		return events;
	}

	protected void SetEvents(List<PerkEvent> value)
	{
		events = value;
	}

	public List<PerkCondition> GetConditions()
	{
		return conditions;
	}

	protected void SetConditions(List<PerkCondition> value)
	{
		conditions = value;
	}

	public List<PerkAction> GetActions()
	{
		return actions;
	}

	protected void SetActions(List<PerkAction> value)
	{
		actions = value;
	}

	public void Parse(XmlNode node)
	{
		set_Name(node.Attributes["Name"].GetStringOrDefault(string.Empty));
		XmlNode eventsNode = node["Events"];
		XmlNode conditionsNode = node["Conditions"];
		XmlNode actionsNode = node["Actions"];
		SetEvents(PerkEvent.Create(eventsNode, GetPerk()));
		SetConditions(PerkCondition.Create(conditionsNode, GetPerk()));
		SetActions(PerkAction.Create(actionsNode, GetPerk(), this));
	}

	public bool MatchesEvent(PerkEvent.EventStruct eventData)
	{
		PerkEvent triggerEvent = null;
		for (int i = 0; i < GetEvents().Count; i++)
		{
			triggerEvent = GetEvents()[i];
			bool flag = triggerEvent.IsEqual(eventData);
			if ((!triggerEvent.IsNot) ? flag : (!flag))
			{
				return true;
			}
		}
		return false;
	}

	public bool AreConditionsMet(Model model, List<string> activeActionNames)
	{
		PerkCondition condition = null;
		for (int i = 0; i < GetConditions().Count; i++)
		{
			condition = GetConditions()[i];
			bool flag = condition.IsEqual(model, activeActionNames);
			if (!((!condition.IsNot) ? flag : (!flag)))
			{
				return false;
			}
		}
		return true;
	}
}
