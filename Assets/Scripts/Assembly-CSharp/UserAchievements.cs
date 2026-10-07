using System.Collections.Generic;
using System.Xml;

public class UserAchievements
{
	private XmlNode countersNode;

	private XmlNode achievementsNode;

	private XmlNode repostAchievementsNode;

	private List<RosterAchievCounter> counters = new List<RosterAchievCounter>();

	private List<RosterAchievement> achievements = new List<RosterAchievement>();

	private List<RepostAchievement> repostAchievements = new List<RepostAchievement>();

	public List<RosterAchievCounter> Counters
	{
		get
		{
			return GetCounters();
		}
	}

	public List<RosterAchievement> Achievements
	{
		get
		{
			return GetAchievements();
		}
	}

	public List<RepostAchievement> RepostAchievements
	{
		get
		{
			return GetRepostAchievements();
		}
	}

	public int CompletedAchievementCount
	{
		get
		{
			return CountCompletedAchievements();
		}
	}

	public List<RosterAchievCounter> GetCounters()
	{
		return counters;
	}

	public List<RosterAchievement> GetAchievements()
	{
		return achievements;
	}

	public List<RepostAchievement> GetRepostAchievements()
	{
		return repostAchievements;
	}

	public int CountCompletedAchievements()
	{
		int num = 0;
		List<AchievCounter> achievCounters = GameUtils.AchievementDefinitions.Counters;
		for (int i = 0; i < achievCounters.Count; i++)
		{
			List<Achievement> counterAchievements = achievCounters[i].Achievements;
			for (int j = 0; j < counterAchievements.Count; j++)
			{
				if (counterAchievements[j].GetIsNew())
				{
					num++;
				}
			}
		}
		return num;
	}

	public void Parse(XmlNode node)
	{
		countersNode = node["Counters"];
		if (countersNode == null)
		{
			countersNode = node.AppendElement("Counters");
		}
		foreach (XmlNode childNode in countersNode.ChildNodes)
		{
			counters.Add(new RosterAchievCounter(childNode));
		}
		achievementsNode = node["Achievements"];
		if (achievementsNode == null)
		{
			achievementsNode = node.AppendElement("Achievements");
		}
		foreach (XmlNode childNode2 in achievementsNode.ChildNodes)
		{
			AddRosterAchievement(new RosterAchievement(childNode2));
		}
		repostAchievementsNode = node["RepostAchievements"];
		if (repostAchievementsNode == null)
		{
			repostAchievementsNode = node.AppendElement("RepostAchievements");
		}
		foreach (XmlNode childNode3 in repostAchievementsNode.ChildNodes)
		{
			repostAchievements.Add(new RepostAchievement(childNode3));
		}
	}

	public RosterAchievCounter FindCounter(string name)
	{
		for (int i = 0; i < counters.Count; i++)
		{
			if (counters[i].get_Name() == name)
			{
				return counters[i];
			}
		}
		return null;
	}

	public void ApplyPendingCounters()
	{
		GameUtils.AchievementCounters definitionCounters = GameUtils.ModeCounters;
		bool flag = false;
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Counter> item in definitionCounters.AllCounters)
		{
			Counter value = item.Value;
			if (value.CompleteValue > 0)
			{
				list.Add(value.Name);
				int num = 0;
				RosterAchievCounter rosterCounter = FindCounter(value.Name);
				if (rosterCounter != null)
				{
					num = rosterCounter.GetCounter() + value.CompleteValue;
					if (value.Type == "WinBattle")
					{
						num = ((num > 1) ? 1 : num);
					}
					rosterCounter.set_Counter(num);
					flag = true;
				}
				else
				{
					num = value.CompleteValue;
					CreateRosterAchievCounter(value.Name, num);
					flag = true;
				}
			}
			value.ResetCompleteValue();
		}
		List<global::Pair<Achievement, int>> pendingUnlocks = GameUtils.AchievementDefinitions.GetUnlockableAchievements(list);
		GameUtils.UnlockAchievements(pendingUnlocks);
		if (flag)
		{
			ListSF.GetRoster().RequestSave();
		}
	}

	public RosterAchievement FindAchievement(string name)
	{
		for (int i = 0; i < achievements.Count; i++)
		{
			if (achievements[i].get_Name() == name)
			{
				return achievements[i];
			}
		}
		return null;
	}

	public void SetAchievementRewardClaimed(RosterAchievement rosterAchievement, bool isClaimed, bool requestSave = true)
	{
		if (rosterAchievement.GetReward() != isClaimed)
		{
			rosterAchievement.set_Reward(isClaimed);
			if (requestSave)
			{
				ListSF.GetRoster().RequestSave();
			}
		}
	}

	public void UnlockAchievement(Achievement achievement, bool isClaimed = true, bool requestSave = true)
	{
		if (achievement == null)
		{
			return;
		}
		string achievementName = achievement.Name;
		for (int i = 0; i < achievements.Count; i++)
		{
			RosterAchievement existingAchievement = achievements[i];
			if (achievementName == existingAchievement.get_Name())
			{
				SetAchievementRewardClaimed(existingAchievement, isClaimed, requestSave);
				return;
			}
		}
		string achievementNodeName = "Achievement";
		XmlNode achievementNode = achievementsNode.AppendElement(achievementNodeName);
		RosterAchievement pMGCOHHMIIC2 = new RosterAchievement(achievementNode);
		pMGCOHHMIIC2.set_Name(achievementName);
		pMGCOHHMIIC2.set_Reward(isClaimed);
		AddRosterAchievement(pMGCOHHMIIC2, achievement);
		ArgsDict eventArgs = new ArgsDict();
		eventArgs["name"] = achievementName;
		StatisticsCollector.LogEvent(StatisticsEvent.EventType.Achievement, eventArgs);
		if (requestSave)
		{
			ListSF.GetRoster().RequestSave();
		}
	}

	public bool CreateRepostAchievement(string achievementName)
	{
		for (int i = 0; i < repostAchievements.Count; i++)
		{
			RepostAchievement repostAchievement = repostAchievements[i];
			if (repostAchievement.get_Name() == achievementName)
			{
				return false;
			}
		}
		repostAchievements.Add(new RepostAchievement(repostAchievementsNode, achievementName));
		return true;
	}

	public bool RemoveRepostAchievement(RepostAchievement repostAchievement)
	{
		int num = 0;
		foreach (XmlNode childNode in repostAchievementsNode.ChildNodes)
		{
			string text = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			if (text == repostAchievement.get_Name())
			{
				repostAchievementsNode.RemoveChild(childNode);
				repostAchievements.RemoveAt(num);
				return true;
			}
			num++;
		}
		return false;
	}

	public void AddRepostAchievements(List<string> achievementNames)
	{
		bool flag = false;
		for (int i = 0; i < achievementNames.Count; i++)
		{
			flag = CreateRepostAchievement(achievementNames[i]);
		}
		if (flag)
		{
			ListSF.GetRoster().RequestSave();
		}
	}

	public void RemoveRepostAchievements(List<RepostAchievement> repostsToRemove)
	{
		bool flag = false;
		for (int i = 0; i < repostsToRemove.Count; i++)
		{
			flag = RemoveRepostAchievement(repostsToRemove[i]);
		}
		if (flag)
		{
			ListSF.GetRoster().RequestSave();
		}
	}

    public int AdvanceExternalCounter(string name, int amount, int maximum)
    {
        if (!Eclipse.Modding.DefinitionId.TryParse(name, out var id) || id.Namespace.Value == "core" || id.Category != "counters" || amount < 0 || maximum < 1)
            throw new System.ArgumentException("Invalid external counter increment.");
        var existing = FindCounter(name);
        int previous = existing?.GetCounter() ?? 0;
        int next = System.Math.Max(previous, (int)System.Math.Min(maximum, (long)previous + amount));
        if(existing == null) CreateRosterAchievCounter(name,next); else existing.set_Counter(next);
        var definition = GameUtils.AchievementDefinitions.GetCounterByName(name);
        if(definition != null) foreach(var achievement in definition.Achievements)
            if(next >= achievement.CounterValue && FindAchievement(achievement.Name) == null)
                UnlockAchievement(achievement, true, false);
        ListSF.GetRoster().RequestSave();
        return next;
    }

	private void CreateRosterAchievCounter(string name, int value)
	{
		XmlNode counterNode = countersNode.AppendElement("Counter");
		RosterAchievCounter rosterCounter = new RosterAchievCounter(counterNode);
		rosterCounter.set_Name(name);
		rosterCounter.set_Counter(value);
		counters.Add(rosterCounter);
	}

	private void AddRosterAchievement(RosterAchievement rosterAchievement, Achievement achievement = null)
	{
		Achievement achievementDefinition = ((achievement == null) ? GameUtils.AchievementDefinitions.GetAchievementByName(rosterAchievement.get_Name()) : achievement);
		if (achievementDefinition != null)
		{
			achievementDefinition.IsUnlocked = true;
			achievementDefinition.RewardClaimed = rosterAchievement.GetReward();
			achievementDefinition.SetIsNew(!rosterAchievement.GetReward());
		}
		achievements.Add(rosterAchievement);
	}
}
