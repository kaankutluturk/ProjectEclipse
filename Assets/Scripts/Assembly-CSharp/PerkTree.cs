using System.Collections.Generic;
using System.Xml;

public class PerkTree
{
	public enum PerkItemType
	{
		TYPE_NONE = 0,
		TYPE_PERK = 1,
		TYPE_UPGRADE = 2
	}

	public class PerkItem
	{
		public PerkItemType Type;

		public string Name;

		public int Level;

		public PerkItem(PerkItemType _type, string _name, int _level)
		{
			Type = _type;
			Name = _name;
			Level = _level;
		}

		public bool IsAvailable()
		{
			bool flag = false;
			RosterPerk hOGDBKBFFDJ = ListSF.GetRoster().GetPerks().FindPerk(Name);
			bool flag2 = hOGDBKBFFDJ != null;
			switch (Type)
			{
			case PerkItemType.TYPE_PERK:
				return !flag2 || hOGDBKBFFDJ.GetLevel() >= Level;
			case PerkItemType.TYPE_UPGRADE:
				return flag2 && hOGDBKBFFDJ.GetLevel() <= Level;
			default:
				return false;
			}
		}
	}

	public class PerkBranch
	{
		public List<PerkItem> Items = new List<PerkItem>();

		public int Level;

		public PerkBranch(int _level)
		{
			Level = _level;
		}

		public List<PerkItem> GetAvailableItems(int CCBEHBMOPMC = 2)
		{
			List<PerkItem> list = new List<PerkItem>();
			for (int i = 0; i < Items.Count; i++)
			{
				PerkItem pJOFNPMOJJA = Items[i];
				if (pJOFNPMOJJA.IsAvailable())
				{
					list.Add(pJOFNPMOJJA);
				}
				if (list.Count >= CCBEHBMOPMC)
				{
					break;
				}
			}
			return list;
		}
	}

	private static PerkTree _instance;

	private List<PerkBranch> branches = new List<PerkBranch>();

	private List<ProfilePerkContainer> levelContainers = new List<ProfilePerkContainer>();

	private List<ProfilePerk> profilePerks = new List<ProfilePerk>();

	private List<PerkInfoItem> availablePerkInfos = new List<PerkInfoItem>();

	public static PerkTree GetInstance()
	{
		if (_instance == null)
		{
			_instance = new PerkTree();
		}
		return _instance;
	}

	public static bool Compare(PerkBranch MKICABFAHFA, PerkBranch JMLKHIPBCLI)
	{
		return MKICABFAHFA.Level < JMLKHIPBCLI.Level;
	}

	public void Clear()
	{
		branches.Clear();
		ClearProfileState();
	}

	public void ClearProfileState()
	{
		levelContainers.Clear();
		profilePerks.Clear();
		availablePerkInfos.Clear();
	}

	public void RebuildProfile()
	{
		ClearProfileState();
		// Profile initialization removes learned perks from this working list. Keep
		// the content catalog intact when replacing the title's preview profile.
		availablePerkInfos = new List<PerkInfoItem>(GameUtils.PerkItemList.GetProgressionPerks());
		List<PerkBranch> list = GetInstance().GetBranches();
		for (int i = 0; i < list.Count; i++)
		{
			PerkBranch gOOLLBPEFJM = list[i];
			int bLJGEOEHIGP = ((gOOLLBPEFJM.Items.Count == 1) ? 1 : 2);
			AddEmptyContainer(gOOLLBPEFJM.Level, bLJGEOEHIGP);
		}
		if (list.Count != 0)
		{
			RefreshBranch(list[0]);
		}
		UnlockFirstLevelPerks();
		List<PerkHistory.Perk> jOGBKOJCINM = ListSF.GetRoster().GetPerks().History.Perks;
		for (int j = 0; j < jOGBKOJCINM.Count; j++)
		{
			ApplyLearnedPerk(jOGBKOJCINM[j]);
		}
	}

	public void ApplyLearnedPerk(PerkHistory.Perk AEFFHJGMNFI)
	{
		if (AEFFHJGMNFI != null)
		{
			RemovePerkInfoItemIfExist(AEFFHJGMNFI.Name);
			PerkBranch gOOLLBPEFJM = FindNextBranchAfterLevel(AEFFHJGMNFI.Level);
			if (gOOLLBPEFJM != null)
			{
				RefreshBranch(gOOLLBPEFJM);
			}
			UpdateStatesForLearnedPerk(AEFFHJGMNFI);
		}
	}

	public void Parse(XmlNode node)
	{
		Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			ParseBranch(childNode);
		}
		branches.Sort((PerkBranch LHBNIMGFKIB, PerkBranch AAOIAEJJINO) => LHBNIMGFKIB.Level.CompareTo(AAOIAEJJINO.Level));
	}

	public int GetUnlockedBranchCount()
	{
		int num = 0;
		int num2 = ListSF.GetRoster().GetLevel();
		for (int i = 0; i < branches.Count; i++)
		{
			PerkBranch gOOLLBPEFJM = branches[i];
			if (gOOLLBPEFJM.Level > num2)
			{
				break;
			}
			num++;
		}
		return num;
	}

	public List<PerkBranch> GetBranches()
	{
		return branches;
	}

	public PerkBranch ReplaceExternalBranch(int level, IReadOnlyList<PerkItem> items)
	{
		if (items == null || items.Count == 0)
			throw new System.ArgumentException("External perk-tree branch requires at least one item.", "items");
		PerkBranch previous = FindNextBranchAfterLevel(level);
		PerkBranch replacement = new PerkBranch(level);
		for (int i = 0; i < items.Count; i++)
		{
			PerkItem item = items[i];
			if (item == null || item.Type == PerkItemType.TYPE_NONE || string.IsNullOrEmpty(item.Name))
				throw new System.ArgumentException("External perk-tree branch contains an invalid item.", "items");
			replacement.Items.Add(new PerkItem(item.Type, item.Name, level));
		}
		if (previous != null) branches.Remove(previous);
		branches.Add(replacement);
		branches.Sort((left, right) => left.Level.CompareTo(right.Level));
		return previous;
	}

	public void RestoreExternalBranch(int level, PerkBranch branch)
	{
		PerkBranch current = FindNextBranchAfterLevel(level);
		if (current != null) branches.Remove(current);
		if (branch != null) branches.Add(branch);
		branches.Sort((left, right) => left.Level.CompareTo(right.Level));
	}

	public List<ProfilePerk> GetProfilePerks()
	{
		return profilePerks;
	}

	public List<ProfilePerk> GetProfilePerksAtLevel(int GNLOCMLBNHF)
	{
		List<ProfilePerk> list = new List<ProfilePerk>();
		for (int i = 0; i < profilePerks.Count; i++)
		{
			ProfilePerk pLKCIINIFMJ = profilePerks[i];
			if (pLKCIINIFMJ.GetLevel() == GNLOCMLBNHF)
			{
				list.Add(pLKCIINIFMJ);
			}
			else if (pLKCIINIFMJ.GetLevel() > GNLOCMLBNHF)
			{
				break;
			}
		}
		return list;
	}

	public ProfilePerk FindProfilePerk(string name)
	{
		for (int i = 0; i < profilePerks.Count; i++)
		{
			ProfilePerk pLKCIINIFMJ = profilePerks[i];
			if (pLKCIINIFMJ.GetPerkName() == name)
			{
				return pLKCIINIFMJ;
			}
		}
		return null;
	}

	public List<ProfilePerkContainer> GetLevelContainers()
	{
		return levelContainers;
	}

	public ProfilePerkContainer GetContainerAtLevel(int GNLOCMLBNHF)
	{
		for (int i = 0; i < levelContainers.Count; i++)
		{
			ProfilePerkContainer fHPJJGPJLHD = levelContainers[i];
			if (fHPJJGPJLHD.Level == GNLOCMLBNHF)
			{
				return fHPJJGPJLHD;
			}
		}
		return null;
	}

	public ProfilePerkContainer GetNextContainerAfterLevel(int GNLOCMLBNHF)
	{
		for (int i = 0; i < levelContainers.Count; i++)
		{
			ProfilePerkContainer fHPJJGPJLHD = levelContainers[i];
			if (fHPJJGPJLHD.Level > GNLOCMLBNHF)
			{
				return fHPJJGPJLHD;
			}
		}
		return null;
	}

	public PerkBranch FindNextBranchAfterLevel(int GNLOCMLBNHF)
	{
		for (int i = 0; i < branches.Count; i++)
		{
			PerkBranch gOOLLBPEFJM = branches[i];
			if (gOOLLBPEFJM.Level > GNLOCMLBNHF)
			{
				return gOOLLBPEFJM;
			}
		}
		return null;
	}

	private void ParseBranch(XmlNode node)
	{
		int iBMNBEGDMBJ = node.Attributes["Value"].ParseInt();
		PerkBranch gOOLLBPEFJM = new PerkBranch(iBMNBEGDMBJ);
		foreach (XmlNode childNode in node.ChildNodes)
		{
			PerkItemType aMKFIJMKLIB = ParseItemType(childNode.Name);
			string pHJCCJNOCGJ = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			PerkItem item = new PerkItem(aMKFIJMKLIB, pHJCCJNOCGJ, iBMNBEGDMBJ);
			gOOLLBPEFJM.Items.Add(item);
		}
		branches.Add(gOOLLBPEFJM);
	}

	private void RemoveProfilePerksWithLevel(int GNLOCMLBNHF)
	{
		int num = 0;
		while (num != profilePerks.Count)
		{
			if (profilePerks[num].GetLevel() == GNLOCMLBNHF)
			{
				profilePerks.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
		ProfilePerkContainer fHPJJGPJLHD = GetContainerAtLevel(GNLOCMLBNHF);
		if (fHPJJGPJLHD != null)
		{
			List<ProfilePerk> jOGBKOJCINM = fHPJJGPJLHD.Perks;
			jOGBKOJCINM.Clear();
		}
	}

	private void PopulateBranchPerks(PerkBranch JBEIKKDKINI)
	{
		List<PerkItem> list = JBEIKKDKINI.GetAvailableItems();
		ProfilePerkContainer fHPJJGPJLHD = GetContainerAtLevel(JBEIKKDKINI.Level);
		for (int i = 0; i < list.Count; i++)
		{
			PerkItem pJOFNPMOJJA = list[i];
			PerkInfoItem aCONCDFDNJH = FindPerkInfo(pJOFNPMOJJA.Name);
			if (aCONCDFDNJH != null)
			{
				ProfilePerk item = new ProfilePerk(aCONCDFDNJH, pJOFNPMOJJA.Level, ProfilePerk.ProfilePerkState.PERK_LOCK, ToProfilePerkType(pJOFNPMOJJA.Type));
				profilePerks.Add(item);
				if (fHPJJGPJLHD != null)
				{
					fHPJJGPJLHD.Perks.Add(item);
				}
			}
		}
	}

	private void AddEmptyContainer(int GNLOCMLBNHF, int count = 2)
	{
		ProfilePerkContainer fHPJJGPJLHD = new ProfilePerkContainer(GNLOCMLBNHF);
		levelContainers.Add(fHPJJGPJLHD);
		for (int i = 0; i < count; i++)
		{
			ProfilePerk item = new ProfilePerk(null, GNLOCMLBNHF, ProfilePerk.ProfilePerkState.PERK_LOCK);
			profilePerks.Add(item);
			fHPJJGPJLHD.Perks.Add(item);
		}
	}

	private void RefreshBranch(PerkBranch JBEIKKDKINI)
	{
		RemoveProfilePerksWithLevel(JBEIKKDKINI.Level);
		PopulateBranchPerks(JBEIKKDKINI);
		profilePerks.Sort((ProfilePerk LHBNIMGFKIB, ProfilePerk AAOIAEJJINO) => LHBNIMGFKIB.GetLevel().CompareTo(AAOIAEJJINO.GetLevel()));
	}

	private void UpdateStatesForLearnedPerk(PerkHistory.Perk AEFFHJGMNFI)
	{
		List<ProfilePerk> list = GetProfilePerksAtLevel(AEFFHJGMNFI.Level);
		for (int i = 0; i < list.Count; i++)
		{
			ProfilePerk pLKCIINIFMJ = list[i];
			ProfilePerk.ProfilePerkState bAINMLLIKOL = ((!(pLKCIINIFMJ.GetPerkName() == AEFFHJGMNFI.Name)) ? ProfilePerk.ProfilePerkState.PERK_UNAVAILABLE : ProfilePerk.ProfilePerkState.PERK_SELECTED);
			pLKCIINIFMJ.set_State(bAINMLLIKOL);
		}
		List<ProfilePerk> list2 = GetNextLevelProfilePerks(AEFFHJGMNFI.Level);
		for (int j = 0; j < list2.Count; j++)
		{
			list2[j].set_State(ProfilePerk.ProfilePerkState.PERK_AVAILABLE);
		}
	}

	private void UnlockFirstLevelPerks()
	{
		if (profilePerks.Count > 0)
		{
			int num = profilePerks[0].GetLevel();
			int count = profilePerks.Count;
			for (int i = 0; i < count && num == profilePerks[i].GetLevel(); i++)
			{
				profilePerks[i].set_State(ProfilePerk.ProfilePerkState.PERK_AVAILABLE);
			}
		}
	}

	private PerkInfoItem FindPerkInfo(string name)
	{
		PerkInfoItem aCONCDFDNJH = null;
		foreach (PerkInfoItem item in availablePerkInfos)
		{
			if (item.Name == name)
			{
				aCONCDFDNJH = item;
				break;
			}
		}
		if (aCONCDFDNJH == null)
		{
			aCONCDFDNJH = GameUtils.PerkItemList.FindBasePerk(name);
		}
		return aCONCDFDNJH;
	}

	private void RemovePerkInfoItemIfExist(string name)
	{
		for (int i = 0; i < availablePerkInfos.Count; i++)
		{
			if (availablePerkInfos[i].Name == name)
			{
				availablePerkInfos.RemoveAt(i);
				break;
			}
		}
	}

	private PerkItemType ParseItemType(string CNKBLODAFDO)
	{
		PerkItemType result = PerkItemType.TYPE_NONE;
		if (CNKBLODAFDO == "Perk")
		{
			result = PerkItemType.TYPE_PERK;
		}
		else if (CNKBLODAFDO == "Upgrade")
		{
			result = PerkItemType.TYPE_UPGRADE;
		}
		return result;
	}

	private ProfilePerk.ProfilePerkType ToProfilePerkType(PerkItemType LFLGCDNKNJI)
	{
		switch (LFLGCDNKNJI)
		{
		case PerkItemType.TYPE_PERK:
			return ProfilePerk.ProfilePerkType.TYPE_PERK;
		case PerkItemType.TYPE_UPGRADE:
			return ProfilePerk.ProfilePerkType.TYPE_UPGRADE;
		default:
			return ProfilePerk.ProfilePerkType.TYPE_NONE;
		}
	}

	private List<ProfilePerk> GetNextLevelProfilePerks(int GNLOCMLBNHF)
	{
		List<ProfilePerk> list = new List<ProfilePerk>();
		int num = GNLOCMLBNHF;
		for (int i = 0; i < profilePerks.Count; i++)
		{
			ProfilePerk pLKCIINIFMJ = profilePerks[i];
			int num2 = pLKCIINIFMJ.GetLevel();
			if (num2 > num && num > GNLOCMLBNHF)
			{
				break;
			}
			if (num2 > GNLOCMLBNHF)
			{
				num = num2;
				list.Add(pLKCIINIFMJ);
			}
		}
		return list;
	}
}
