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

	public QuestStage(XmlNode node, string fileName)
	{
		SetFileName(fileName);
		EclipseSourceFile = (node.Attributes?["EclipseSourceFile"]?.Value ?? fileName).Replace('\\', '/');
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

	private void ParseMarks(XmlNode marksNode, List<string> marks, QuestStage ownerStage)
	{
		if (marksNode == null)
		{
			return;
		}
		XmlNodeList childNodes = marksNode.ChildNodes;
		foreach (XmlNode item2 in childNodes)
		{
			string item = XmlUtils.ParseString(item2.Attributes["Name"]);
			marks.Add(item);
		}
	}

	private void ParseEvents(XmlNode eventsNode, List<QuestEvent> events, QuestStage ownerStage = null)
	{
		if (eventsNode == null)
		{
			return;
		}
		XmlNodeList childNodes = eventsNode.ChildNodes;
		foreach (XmlNode item in childNodes)
		{
			QuestEvent questEvent = new QuestEvent();
			questEvent.Parse(item);
			events.Add(questEvent);
		}
	}

	private void ParseConditions(XmlNode conditionsNode, List<QuestCondition> conditions, QuestStage ownerStage = null)
	{
		if (conditionsNode == null)
		{
			return;
		}
		XmlNodeList childNodes = conditionsNode.ChildNodes;
		foreach (XmlNode item in childNodes)
		{
			QuestCondition condition = new QuestCondition();
			condition.Parse(item);
			if (condition.comparison == QuestCondition.ComparisonType.QUEST_CONDITION_OPERATOR)
			{
				ParseConditions(item, condition.conditions);
			}
			conditions.Add(condition);
		}
	}

	private void ParseActions(XmlNode actionsNode, QuestActionsSequence sequence, QuestStage ownerStage = null)
	{
		if (actionsNode == null)
		{
			return;
		}
		string placeName = XmlUtils.ParseString(actionsNode.Attributes["Place"], "Map");
		placeId = ParsePlace(placeName);
		int num = 0;
		foreach (XmlNode childNode in actionsNode.ChildNodes)
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
			ParseAction(childNode.Name, childNode, sequence, num);
			num++;
		}
		SetRosterQuest(ListSF.GetRoster().FindQuest(get_Name()));
		sequence.AddEventListener(1, OnActionComplete);
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

	private void ParseAction(string actionName, XmlNode node, QuestActionsSequence sequence, int index)
	{
		QuestAction action = QuestAction.GetClassActionByName(actionName);
		action.QuestName = get_Name();
		action.QuestFileName = GetFileName();
		action.SetStage(this);
		action.Index = index;
		action.StageIndex = placeId;
		action.Parse(node);
		sequence.AddAction(action);
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

	public QuestEvent FindEvent(string eventName)
	{
		QuestEvent.QuestEventType eventType = QuestEvent.ParseEventType(eventName);
		return FindEvent(eventType);
	}

	public QuestEvent FindEvent(QuestEvent.QuestEventType eventType)
	{
		foreach (QuestEvent item in events)
		{
			if (item.IsEvent(eventType))
			{
				return item;
			}
		}
		return null;
	}

	public bool IsEvent(string eventName)
	{
		QuestEvent.QuestEventType eventType = QuestEvent.ParseEventType(eventName);
		return IsEvent(eventType);
	}

	public bool IsEvent(QuestEvent.QuestEventType eventType)
	{
		QuestEvent questEvent = FindEvent(eventType);
		return questEvent != null;
	}

	public bool IsGroup(List<string> groupNames)
	{
		foreach (string item in groupNames)
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

	public bool Compare(QuestParameters parameters)
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
			if (!item.Compare(parameters, GetRosterQuest()))
			{
				return false;
			}
		}
		return true;
	}

	public void QueueForRun(QuestParameters parameters)
	{
		EclipseQueuedParameters = parameters.SnapshotForQueue();
		EclipseQueuedResume = false;
		if (firstCheckPoint != null)
		{
			firstCheckPoint.SaveCheckPoint(EclipseQueuedParameters);
		}
	}

	public void StartActions(QuestParameters parameters, bool resume)
	{
		EclipseQueuedParameters = null;
		EclipseQueuedResume = false;
		EclipseLotteryInvocations = null;
		EclipseResumeActions = resume;
		if (LogRules.GetInstance().GetLogQuests())
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Quest ");
			stringBuilder.Append(get_Name());
			stringBuilder.Append(" started");
			GameLog.Info(stringBuilder.ToString());
		}
		SetState(QuestState.QUEST_ACTIONS);
		actions.currentIndex = ((resume && GetRosterQuest() != null) ? GetRosterQuest().GetCheckpointIndex() : 0);
		actions.Run(parameters);
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

	public QuestParameters RestoreParameters(ParametersQuest savedParameters)
	{
		QuestParameters restoredParameters = new QuestParameters();
		if (savedParameters != null)
		{
			FightList fight = ListSF.GetInstance().GetFightByIdString(savedParameters.GetFightName());
			restoredParameters.fightIds = ((fight == null) ? FightIDS.Empty() : fight.FightId);
			restoredParameters.fightResult = savedParameters.GetFightResultName();
			restoredParameters.raidResult = savedParameters.GetRaidResultName();
			restoredParameters.levelUp = savedParameters.GetLevelUp();
			restoredParameters.energyChange = savedParameters.GetPower();
			restoredParameters.fightAvgFps = savedParameters.GetFightAvgFps();
			Eclipse.Modding.ModRuntime.RestoreQuestLotteryContext(savedParameters, restoredParameters);
		}
		return restoredParameters;
	}

	public int CompareTo(QuestStage otherStage)
	{
		if (otherStage == null)
		{
			return 1;
		}
		// Pending quests precede running actions; equal states use descending priority.
		bool running = GetState() == QuestState.QUEST_ACTIONS;
		bool otherRunning = otherStage.GetState() == QuestState.QUEST_ACTIONS;
		if (running != otherRunning)
		{
			return running ? 1 : -1;
		}
		return otherStage.priority.CompareTo(priority);
	}
}
