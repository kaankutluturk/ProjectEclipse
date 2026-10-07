using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

public class QuestStage : global::EventDispatcher<object>, IComparable<QuestStage>
{
	public enum QuestState
	{
		QUEST_UNCOMPLETE = 0,
		QUEST_ACTIONS = 1,
		QUEST_COMPLETE = 2
	}

	public enum QuestStageEvent
	{
		OnComplete = 0,
		OnCompleteQuest = 1
	}

	private List<QuestEvent> events = new List<QuestEvent>();

	private List<QuestCondition> conditions = new List<QuestCondition>();

	private List<QuestActionCheckPoint> checkPoints = new List<QuestActionCheckPoint>();

	private List<string> marks = new List<string>();

	private List<string> groups = new List<string>();

	private QuestActionsSequence actions = new QuestActionsSequence();

	private QuestActionCheckPoint firstCheckPoint;

	private QuestParameters lastParameters;

	private int placeId;

	public int priority;

	public int index;

	public int unresumable;

	public bool allowDoubles;

	internal Eclipse.Modding.ModQuestInvocationLedger EclipseLotteryInvocations;
	internal bool EclipseResumeActions;
	internal string EclipseActionsDefinition;
	internal QuestParameters EclipseQueuedParameters;
	internal bool EclipseQueuedResume;

	// Source provenance is separate from FileName, which is a saved loader contract.
	public string EclipseSourceFile { get; private set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string name;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string fileName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private RosterQuest rosterQuest;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private QuestState state;

	public string FileName
	{
		get
		{
			return GetFileName();
		}
		private set
		{
			SetFileName(value);
		}
	}

	public RosterQuest LinkedRosterQuest
	{
		get
		{
			return GetRosterQuest();
		}
		private set
		{
			SetRosterQuest(value);
		}
	}

	public QuestState StageStatus
	{
		get
		{
			return GetState();
		}
		private set
		{
			SetState(value);
		}
	}

	public QuestStage(XmlNode node, string PMFEIPCHENB)
	{
		SetFileName(PMFEIPCHENB);
		EclipseSourceFile = (node.Attributes?["EclipseSourceFile"]?.Value ?? PMFEIPCHENB).Replace('\\', '/');
		set_Name(XmlUtils.ParseString(node.Attributes["Name"], string.Empty));
		groups.Add(get_Name());
		string text = XmlUtils.ParseString(node.Attributes["Group"], string.Empty);
		groups.AddRange(text.Split('|'));
		SetState(QuestState.QUEST_UNCOMPLETE);
		priority = XmlUtils.ParseInt(node.Attributes["Priority"]);
		unresumable = XmlUtils.ParseInt(node.Attributes["Unresumable"]);
		allowDoubles = XmlUtils.ParseBool(node.Attributes["AllowDoubles"]);
		EclipseActionsDefinition = node["Actions"]?.OuterXml ?? string.Empty;
		ParseEvents(node["Events"], events, this);
		ParseConditions(node["Conditions"], conditions, this);
		ParseActions(node["Actions"], actions, this);
		ParseMarks(node["Marks"], marks, this);
	}

	public string get_Name()
	{
		return name;
	}

	private void set_Name(string value)
	{
		name = value;
	}

	public string GetFileName()
	{
		return fileName;
	}

	private void SetFileName(string value)
	{
		fileName = value;
	}

	public RosterQuest GetRosterQuest()
	{
		return rosterQuest;
	}

	private void SetRosterQuest(RosterQuest value)
	{
		rosterQuest = value;
	}

	public QuestState GetState()
	{
		return state;
	}

	private void SetState(QuestState value)
	{
		state = value;
	}

	private void ParseMarks(XmlNode CDFJOIJHJDA, List<string> GENLBPMKENI, QuestStage PJEAMPLHPOH)
	{
		if (CDFJOIJHJDA == null)
		{
			return;
		}
		XmlNodeList childNodes = CDFJOIJHJDA.ChildNodes;
		foreach (XmlNode item2 in childNodes)
		{
			string item = XmlUtils.ParseString(item2.Attributes["Name"]);
			GENLBPMKENI.Add(item);
		}
	}

	private void ParseEvents(XmlNode MKFADLKDEJM, List<QuestEvent> GENLBPMKENI, QuestStage PJEAMPLHPOH = null)
	{
		if (MKFADLKDEJM == null)
		{
			return;
		}
		XmlNodeList childNodes = MKFADLKDEJM.ChildNodes;
		foreach (XmlNode item in childNodes)
		{
			QuestEvent hKFNABCMDCB = new QuestEvent();
			hKFNABCMDCB.Parse(item);
			GENLBPMKENI.Add(hKFNABCMDCB);
		}
	}

	private void ParseConditions(XmlNode IPDGDBMMHEP, List<QuestCondition> GENLBPMKENI, QuestStage PJEAMPLHPOH = null)
	{
		if (IPDGDBMMHEP == null)
		{
			return;
		}
		XmlNodeList childNodes = IPDGDBMMHEP.ChildNodes;
		foreach (XmlNode item in childNodes)
		{
			QuestCondition kKDGLNECFHA = new QuestCondition();
			kKDGLNECFHA.Parse(item);
			if (kKDGLNECFHA.comparison == QuestCondition.ComparisonType.QUEST_CONDITION_OPERATOR)
			{
				ParseConditions(item, kKDGLNECFHA.conditions);
			}
			GENLBPMKENI.Add(kKDGLNECFHA);
		}
	}

	private void ParseActions(XmlNode EPKLCPOEELO, QuestActionsSequence GENLBPMKENI, QuestStage PJEAMPLHPOH = null)
	{
		if (EPKLCPOEELO == null)
		{
			return;
		}
		string bAINMLLIKOL = XmlUtils.ParseString(EPKLCPOEELO.Attributes["Place"], "Map");
		placeId = ParsePlace(bAINMLLIKOL);
		int num = 0;
		foreach (XmlNode childNode in EPKLCPOEELO.ChildNodes)
		{
			if (num == 0)
			{
				firstCheckPoint = new QuestActionCheckPoint();
				firstCheckPoint.QuestName = get_Name();
				firstCheckPoint.QuestFileName = GetFileName();
				firstCheckPoint.Index = 0;
				firstCheckPoint.StageIndex = placeId;
				firstCheckPoint.Parse(childNode);
				firstCheckPoint.SetStage(this);
				firstCheckPoint.AddEventListener(2, OnCheckPointRosterQuestChanged);
				checkPoints.Add(firstCheckPoint);
			}
			ParseAction(childNode.Name, childNode, GENLBPMKENI, num);
			num++;
		}
		SetRosterQuest(ListSF.GetRoster().FindQuest(get_Name()));
		GENLBPMKENI.AddEventListener(1, OnActionComplete);
	}

	private void OnActionComplete(object data)
	{
		FinishQuest();
	}

	private void OnCheckPointRosterQuestChanged(object data)
	{
		if (data != null)
		{
			SetRosterQuest((RosterQuest)data);
		}
	}

	private void ParseAction(string LJICOHPCPKO, XmlNode node, QuestActionsSequence GENLBPMKENI, int index)
	{
		QuestAction mBAAKHELFKL = QuestAction.GetClassActionByName(LJICOHPCPKO);
		mBAAKHELFKL.QuestName = get_Name();
		mBAAKHELFKL.QuestFileName = GetFileName();
		mBAAKHELFKL.SetStage(this);
		mBAAKHELFKL.Index = index;
		mBAAKHELFKL.StageIndex = placeId;
		mBAAKHELFKL.Parse(node);
		GENLBPMKENI.AddAction(mBAAKHELFKL);
	}

	public static int ParsePlace(string value)
	{
		if (value.Equals("Fight"))
		{
			return 5;
		}
		if (value.Equals("Dojo"))
		{
			return 2;
		}
		if (value.Equals("Map"))
		{
			return 4;
		}
		return -1;
	}

	public void FinishQuest()
	{
		if (MarkComplete())
		{
			if (GameUtils.LogSettings.LogQuests)
			{
				string text = "Quest ";
				text += get_Name();
				text += " completed";
				GameLog.Info(text);
			}
			CallEvent(0, this);
		}
		CallEvent(1, this);
	}

	public QuestEvent FindEvent(string MCGHIOHACBJ)
	{
		QuestEvent.QuestEventType mCGHIOHACBJ = QuestEvent.ParseEventType(MCGHIOHACBJ);
		return FindEvent(mCGHIOHACBJ);
	}

	public QuestEvent FindEvent(QuestEvent.QuestEventType MCGHIOHACBJ)
	{
		foreach (QuestEvent item in events)
		{
			if (item.IsEvent(MCGHIOHACBJ))
			{
				return item;
			}
		}
		return null;
	}

	public bool IsEvent(string MCGHIOHACBJ)
	{
		QuestEvent.QuestEventType mCGHIOHACBJ = QuestEvent.ParseEventType(MCGHIOHACBJ);
		return IsEvent(mCGHIOHACBJ);
	}

	public bool IsEvent(QuestEvent.QuestEventType MCGHIOHACBJ)
	{
		QuestEvent hKFNABCMDCB = FindEvent(MCGHIOHACBJ);
		return hKFNABCMDCB != null;
	}

	public bool IsGroup(List<string> FBDKJJBICOK)
	{
		foreach (string item in FBDKJJBICOK)
		{
			foreach (string item2 in this.groups)
			{
				if (item.Equals(item2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Compare(QuestParameters GFIHPBCEEOB)
	{
        string sourcePath = (FileName ?? "").Replace('\\', '/');
        if (sourcePath.IndexOf("/battle_pass/", StringComparison.OrdinalIgnoreCase) >= 0 && !Eclipse.Modding.ModPolicies.FeatureEnabled("battle_pass")) return false;
        foreach (string group in groups)
        {
            string feature = group == "Advertising" ? "ads" : group == "BattlePass" ? "battle_pass" :
                group == "Offers" ? "paid_offers" : group == "RewardedVideo" ? "rewarded_video" :
                group == "OnlineServices" ? "online_services" : group == "Payments" ? "payments" : null;
            if (feature != null && !Eclipse.Modding.ModPolicies.FeatureEnabled(feature)) return false;
        }
		foreach (QuestCondition item in conditions)
		{
			if (!item.Compare(GFIHPBCEEOB, GetRosterQuest()))
			{
				return false;
			}
		}
		return true;
	}

	public void QueueForRun(QuestParameters GFIHPBCEEOB)
	{
		EclipseQueuedParameters = GFIHPBCEEOB.SnapshotForQueue();
		EclipseQueuedResume = false;
		if (firstCheckPoint != null)
		{
			firstCheckPoint.SaveCheckPoint(EclipseQueuedParameters);
		}
	}

	public void StartActions(QuestParameters GFIHPBCEEOB, bool MKBPLLIHMPE)
	{
		EclipseQueuedParameters = null;
		EclipseQueuedResume = false;
		EclipseLotteryInvocations = null;
		EclipseResumeActions = MKBPLLIHMPE;
		if (LogRules.GetInstance().GetLogQuests())
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Quest ");
			stringBuilder.Append(get_Name());
			stringBuilder.Append(" started");
			GameLog.Info(stringBuilder.ToString());
		}
		SetState(QuestState.QUEST_ACTIONS);
		actions.currentIndex = ((MKBPLLIHMPE && GetRosterQuest() != null) ? GetRosterQuest().GetCheckpointIndex() : 0);
		actions.Run(GFIHPBCEEOB);
	}

	public string StateToString(QuestState value)
	{
		switch (value)
		{
		case QuestState.QUEST_UNCOMPLETE:
			return "UNCOMPLETE";
		case QuestState.QUEST_ACTIONS:
			return "ACTIONS";
		case QuestState.QUEST_COMPLETE:
			return "COMPLETE";
		default:
			return string.Empty;
		}
	}

	public QuestState StateFromString(string value)
	{
		switch (value)
		{
		case "UNCOMPLETE":
			return QuestState.QUEST_UNCOMPLETE;
		case "ACTIONS":
			return QuestState.QUEST_ACTIONS;
		case "COMPLETE":
			return QuestState.QUEST_COMPLETE;
		default:
			return QuestState.QUEST_UNCOMPLETE;
		}
	}

	public bool MarkComplete()
	{
		bool saveLotteryRun = Eclipse.Modding.ModRuntime.CompleteQuestLotteryRun(this);
		SetState(QuestState.QUEST_COMPLETE);
		if (GetRosterQuest() != null)
		{
			GetRosterQuest().ClearParameters();
			ListSF.GetInstance().RequestSave();
			if (saveLotteryRun) ListSF.GetInstance().OnAuthenticate(true);
			return true;
		}
		if (saveLotteryRun) ListSF.GetInstance().OnAuthenticate(true);
		return false;
	}

	public bool IsUnresumable()
	{
		return unresumable > 0;
	}

	public QuestParameters GetSavedParameters()
	{
		if (GetRosterQuest() != null)
		{
			return RestoreParameters(GetRosterQuest().get_Parameters());
		}
		return new QuestParameters();
	}

	public QuestParameters RestoreParameters(ParametersQuest KKNOCIPBIIK)
	{
		QuestParameters hHKLFIIBIFF = new QuestParameters();
		if (KKNOCIPBIIK != null)
		{
			FightList jDIPBIHBGPF = ListSF.GetInstance().GetFightByIdString(KKNOCIPBIIK.GetFightName());
			hHKLFIIBIFF.fightIds = ((jDIPBIHBGPF == null) ? FightIDS.Empty() : jDIPBIHBGPF.FightId);
			hHKLFIIBIFF.fightResult = KKNOCIPBIIK.GetFightResultName();
			hHKLFIIBIFF.raidResult = KKNOCIPBIIK.GetRaidResultName();
			hHKLFIIBIFF.levelUp = KKNOCIPBIIK.GetLevelUp();
			hHKLFIIBIFF.energyChange = KKNOCIPBIIK.GetPower();
			hHKLFIIBIFF.fightAvgFps = KKNOCIPBIIK.GetFightAvgFps();
			Eclipse.Modding.ModRuntime.RestoreQuestLotteryContext(KKNOCIPBIIK, hHKLFIIBIFF);
		}
		return hHKLFIIBIFF;
	}

	public int CompareTo(QuestStage NOLFMPDGCOC)
	{
		if (NOLFMPDGCOC == null)
		{
			return 1;
		}
		// Pending quests precede running actions; equal states use descending priority.
		bool running = GetState() == QuestState.QUEST_ACTIONS;
		bool otherRunning = NOLFMPDGCOC.GetState() == QuestState.QUEST_ACTIONS;
		if (running != otherRunning)
		{
			return running ? 1 : -1;
		}
		return NOLFMPDGCOC.priority.CompareTo(priority);
	}
}
