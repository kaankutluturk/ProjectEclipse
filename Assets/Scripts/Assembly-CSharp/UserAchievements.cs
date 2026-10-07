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
		List<AchievCounter> mDNKEAFGAOB = GameUtils.AchievementDefinitions.Counters;
		for (int i = 0; i < mDNKEAFGAOB.Count; i++)
		{
			List<Achievement> fOICCCGPCMJ = mDNKEAFGAOB[i].Achievements;
			for (int j = 0; j < fOICCCGPCMJ.Count; j++)
			{
				if (fOICCCGPCMJ[j].GetIsNew())
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
		GameUtils.AchievementCounters oJNHPHEPFLI = GameUtils.ModeCounters;
		bool flag = false;
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Counter> item in oJNHPHEPFLI.AllCounters)
		{
			Counter value = item.Value;
			if (value.CompleteValue > 0)
			{
				list.Add(value.Name);
				int num = 0;
				RosterAchievCounter cKJBHGKBPPM = FindCounter(value.Name);
				if (cKJBHGKBPPM != null)
				{
					num = cKJBHGKBPPM.GetCounter() + value.CompleteValue;
					if (value.Type == "WinBattle")
					{
						num = ((num > 1) ? 1 : num);
					}
					cKJBHGKBPPM.set_Counter(num);
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
		List<global::Pair<Achievement, int>> cIMGCGDDKCE = GameUtils.AchievementDefinitions.GetUnlockableAchievements(list);
		GameUtils.UnlockAchievements(cIMGCGDDKCE);
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

	public void SetAchievementRewardClaimed(RosterAchievement PGAGNLJABIE, bool POHFOGPKMMK, bool NLCCJEHMAOF = true)
	{
		if (PGAGNLJABIE.GetReward() != POHFOGPKMMK)
		{
			PGAGNLJABIE.set_Reward(POHFOGPKMMK);
			if (NLCCJEHMAOF)
			{
				ListSF.GetRoster().RequestSave();
			}
		}
	}

	public void UnlockAchievement(Achievement NCCHENOEPNF, bool POHFOGPKMMK = true, bool NLCCJEHMAOF = true)
	{
		if (NCCHENOEPNF == null)
		{
			return;
		}
		string mENAJEAJJBE = NCCHENOEPNF.Name;
		for (int i = 0; i < achievements.Count; i++)
		{
			RosterAchievement pMGCOHHMIIC = achievements[i];
			if (mENAJEAJJBE == pMGCOHHMIIC.get_Name())
			{
				SetAchievementRewardClaimed(pMGCOHHMIIC, POHFOGPKMMK, NLCCJEHMAOF);
				return;
			}
		}
		string jLEKBBJBLOE = "Achievement";
		XmlNode hKPPBKPJOEO = achievementsNode.AppendElement(jLEKBBJBLOE);
		RosterAchievement pMGCOHHMIIC2 = new RosterAchievement(hKPPBKPJOEO);
		pMGCOHHMIIC2.set_Name(mENAJEAJJBE);
		pMGCOHHMIIC2.set_Reward(POHFOGPKMMK);
		AddRosterAchievement(pMGCOHHMIIC2, NCCHENOEPNF);
		ArgsDict kEMMIFBFDPK = new ArgsDict();
		kEMMIFBFDPK["name"] = mENAJEAJJBE;
		StatisticsCollector.LogEvent(StatisticsEvent.EventType.Achievement, kEMMIFBFDPK);
		if (NLCCJEHMAOF)
		{
			ListSF.GetRoster().RequestSave();
		}
	}

	public bool CreateRepostAchievement(string OGPJPGMBIHJ)
	{
		for (int i = 0; i < repostAchievements.Count; i++)
		{
			RepostAchievement aFOGJMECGBG = repostAchievements[i];
			if (aFOGJMECGBG.get_Name() == OGPJPGMBIHJ)
			{
				return false;
			}
		}
		repostAchievements.Add(new RepostAchievement(repostAchievementsNode, OGPJPGMBIHJ));
		return true;
	}

	public bool RemoveRepostAchievement(RepostAchievement NCCHENOEPNF)
	{
		int num = 0;
		foreach (XmlNode childNode in repostAchievementsNode.ChildNodes)
		{
			string text = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			if (text == NCCHENOEPNF.get_Name())
			{
				repostAchievementsNode.RemoveChild(childNode);
				repostAchievements.RemoveAt(num);
				return true;
			}
			num++;
		}
		return false;
	}

	public void AddRepostAchievements(List<string> DODEADGDJCM)
	{
		bool flag = false;
		for (int i = 0; i < DODEADGDJCM.Count; i++)
		{
			flag = CreateRepostAchievement(DODEADGDJCM[i]);
		}
		if (flag)
		{
			ListSF.GetRoster().RequestSave();
		}
	}

	public void RemoveRepostAchievements(List<RepostAchievement> MGNCKHDDHLE)
	{
		bool flag = false;
		for (int i = 0; i < MGNCKHDDHLE.Count; i++)
		{
			flag = RemoveRepostAchievement(MGNCKHDDHLE[i]);
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
		XmlNode hKPPBKPJOEO = countersNode.AppendElement("Counter");
		RosterAchievCounter cKJBHGKBPPM = new RosterAchievCounter(hKPPBKPJOEO);
		cKJBHGKBPPM.set_Name(name);
		cKJBHGKBPPM.set_Counter(value);
		counters.Add(cKJBHGKBPPM);
	}

	private void AddRosterAchievement(RosterAchievement BCIJIDMGJLC, Achievement NCCHENOEPNF = null)
	{
		Achievement jNPIOKEKMII = ((NCCHENOEPNF == null) ? GameUtils.AchievementDefinitions.GetAchievementByName(BCIJIDMGJLC.get_Name()) : NCCHENOEPNF);
		if (jNPIOKEKMII != null)
		{
			jNPIOKEKMII.IsUnlocked = true;
			jNPIOKEKMII.RewardClaimed = BCIJIDMGJLC.GetReward();
			jNPIOKEKMII.SetIsNew(!BCIJIDMGJLC.GetReward());
		}
		achievements.Add(BCIJIDMGJLC);
	}
}
