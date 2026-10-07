using System.Diagnostics;
using System.Xml;

public class LogRules
{
	private static LogRules instance;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool logEnabled;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool logQuests;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool logQuestActions;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool logAnimations;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool logHits;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool logHitDamage;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool logHitStyle;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool logTactics;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool logPerks;

	public static LogRules Instance
	{
		get
		{
			return GetInstance();
		}
	}

	public bool LogEnabled
	{
		get
		{
			return GetLogEnabled();
		}
		set
		{
			SetLogEnabled(value);
		}
	}

	public bool LogQuests
	{
		get
		{
			return GetLogQuests();
		}
		set
		{
			SetLogQuests(value);
		}
	}

	public bool LogQuestActions
	{
		get
		{
			return GetLogQuestActions();
		}
		set
		{
			SetLogQuestActions(value);
		}
	}

	public bool LogAnimations
	{
		get
		{
			return GetLogAnimations();
		}
		set
		{
			SetLogAnimations(value);
		}
	}

	public bool LogHits
	{
		get
		{
			return GetLogHits();
		}
		set
		{
			SetLogHits(value);
		}
	}

	public bool LogHitDamage
	{
		get
		{
			return GetLogHitDamage();
		}
		set
		{
			SetLogHitDamage(value);
		}
	}

	public bool LogHitStyle
	{
		get
		{
			return GetLogHitStyle();
		}
		set
		{
			SetLogHitStyle(value);
		}
	}

	public bool LogTactics
	{
		get
		{
			return GetLogTactics();
		}
		set
		{
			SetLogTactics(value);
		}
	}

	public bool LogPerks
	{
		get
		{
			return GetLogPerks();
		}
		set
		{
			SetLogPerks(value);
		}
	}

	private LogRules()
	{
		SetLogEnabled(false);
		SetLogQuests(false);
		SetLogQuestActions(false);
		SetLogAnimations(false);
		SetLogHits(false);
		SetLogHitDamage(false);
		SetLogHitStyle(false);
		SetLogTactics(false);
		SetLogPerks(false);
	}

	public static LogRules GetInstance()
	{
		if (instance == null)
		{
			instance = new LogRules();
		}
		return instance;
	}

	public bool GetLogEnabled()
	{
		return logEnabled;
	}

	public void SetLogEnabled(bool value)
	{
		logEnabled = value;
	}

	public bool GetLogQuests()
	{
		return logQuests;
	}

	public void SetLogQuests(bool value)
	{
		logQuests = value;
	}

	public bool GetLogQuestActions()
	{
		return logQuestActions;
	}

	public void SetLogQuestActions(bool value)
	{
		logQuestActions = value;
	}

	public bool GetLogAnimations()
	{
		return logAnimations;
	}

	public void SetLogAnimations(bool value)
	{
		logAnimations = value;
	}

	public bool GetLogHits()
	{
		return logHits;
	}

	public void SetLogHits(bool value)
	{
		logHits = value;
	}

	public bool GetLogHitDamage()
	{
		return logHitDamage;
	}

	public void SetLogHitDamage(bool value)
	{
		logHitDamage = value;
	}

	public bool GetLogHitStyle()
	{
		return logHitStyle;
	}

	public void SetLogHitStyle(bool value)
	{
		logHitStyle = value;
	}

	public bool GetLogTactics()
	{
		return logTactics;
	}

	public void SetLogTactics(bool value)
	{
		logTactics = value;
	}

	public bool GetLogPerks()
	{
		return logPerks;
	}

	public void SetLogPerks(bool value)
	{
		logPerks = value;
	}

	private bool ParseFlag(XmlNode node, bool AGADEMLBJGJ = false)
	{
		return (node == null) ? AGADEMLBJGJ : XmlUtils.ParseBool(node.Attributes[0], AGADEMLBJGJ);
	}

	public void Parse(XmlNode node)
	{
		SetLogEnabled(ParseFlag(node));
		if (GetLogEnabled())
		{
			XmlNode xmlNode = node["Quests"];
			SetLogQuests(ParseFlag(xmlNode));
			if (GetLogQuests())
			{
				XmlNode hKPPBKPJOEO = xmlNode["Actions"];
				SetLogQuestActions(ParseFlag(hKPPBKPJOEO));
			}
			SetLogAnimations(ParseFlag(node["Animations"]));
			SetLogTactics(ParseFlag(node["Tactics"]));
			SetLogPerks(ParseFlag(node["Perks"]));
			XmlNode xmlNode2 = node["Hits"];
			SetLogHits(ParseFlag(xmlNode2));
			if (GetLogHits())
			{
				SetLogHitDamage(ParseFlag(xmlNode2["Damage"]));
				SetLogHitStyle(ParseFlag(xmlNode2["Style"]));
			}
		}
	}
}
