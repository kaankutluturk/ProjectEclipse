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
		XmlNode hKPPBKPJOEO = node["Events"];
		XmlNode hKPPBKPJOEO2 = node["Conditions"];
		XmlNode hKPPBKPJOEO3 = node["Actions"];
		SetEvents(PerkEvent.Create(hKPPBKPJOEO, GetPerk()));
		SetConditions(PerkCondition.Create(hKPPBKPJOEO2, GetPerk()));
		SetActions(PerkAction.Create(hKPPBKPJOEO3, GetPerk(), this));
	}

	public bool MatchesEvent(PerkEvent.EventStruct EJMEALJNNIL)
	{
		PerkEvent gBMAKFJNAPG = null;
		for (int i = 0; i < GetEvents().Count; i++)
		{
			gBMAKFJNAPG = GetEvents()[i];
			bool flag = gBMAKFJNAPG.IsEqual(EJMEALJNNIL);
			if ((!gBMAKFJNAPG.IsNot) ? flag : (!flag))
			{
				return true;
			}
		}
		return false;
	}

	public bool AreConditionsMet(Model ACENLMONNPA, List<string> NIKHAICFGNM)
	{
		PerkCondition iDJILNODHAD = null;
		for (int i = 0; i < GetConditions().Count; i++)
		{
			iDJILNODHAD = GetConditions()[i];
			bool flag = iDJILNODHAD.IsEqual(ACENLMONNPA, NIKHAICFGNM);
			if (!((!iDJILNODHAD.IsNot) ? flag : (!flag)))
			{
				return false;
			}
		}
		return true;
	}
}
