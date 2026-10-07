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
			RosterPerk rosterPerk = ListSF.GetRoster().GetPerks().FindPerk(Name);
			bool flag2 = rosterPerk != null;
			switch (Type)
			{
			case PerkItemType.TYPE_PERK:
				return !flag2 || rosterPerk.GetLevel() >= Level;
			case PerkItemType.TYPE_UPGRADE:
				return flag2 && rosterPerk.GetLevel() <= Level;
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

		public List<PerkItem> GetAvailableItems(int maxCount = 2)
		{
			List<PerkItem> list = new List<PerkItem>();
			for (int i = 0; i < Items.Count; i++)
			{
				PerkItem perkItem = Items[i];
				if (perkItem.IsAvailable())
				{
					list.Add(perkItem);
				}
				if (list.Count >= maxCount)
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

	public static bool Compare(PerkBranch left, PerkBranch right)
	{
		return left.Level < right.Level;
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
			PerkBranch branch = list[i];
			int slotCount = ((branch.Items.Count == 1) ? 1 : 2);
			AddEmptyContainer(branch.Level, slotCount);
		}
		if (list.Count != 0)
		{
			RefreshBranch(list[0]);
		}
		UnlockFirstLevelPerks();
		List<PerkHistory.Perk> learnedPerks = ListSF.GetRoster().GetPerks().History.Perks;
		for (int j = 0; j < learnedPerks.Count; j++)
		{
			ApplyLearnedPerk(learnedPerks[j]);
		}
	}

	public void ApplyLearnedPerk(PerkHistory.Perk learnedPerk)
	{
		if (learnedPerk != null)
		{
			RemovePerkInfoItemIfExist(learnedPerk.Name);
			PerkBranch branch = FindNextBranchAfterLevel(learnedPerk.Level);
			if (branch != null)
			{
				RefreshBranch(branch);
			}
			UpdateStatesForLearnedPerk(learnedPerk);
		}
	}

	public void Parse(XmlNode node)
	{
		Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			ParseBranch(childNode);
		}
		branches.Sort((PerkBranch left, PerkBranch right) => left.Level.CompareTo(right.Level));
	}

	public int GetUnlockedBranchCount()
	{
		int num = 0;
		int num2 = ListSF.GetRoster().GetLevel();
		for (int i = 0; i < branches.Count; i++)
		{
			PerkBranch branch = branches[i];
			if (branch.Level > num2)
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

	public List<ProfilePerk> GetProfilePerksAtLevel(int level)
	{
		List<ProfilePerk> list = new List<ProfilePerk>();
		for (int i = 0; i < profilePerks.Count; i++)
		{
			ProfilePerk profilePerk = profilePerks[i];
			if (profilePerk.GetLevel() == level)
			{
				list.Add(profilePerk);
			}
			else if (profilePerk.GetLevel() > level)
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
			ProfilePerk profilePerk = profilePerks[i];
			if (profilePerk.GetPerkName() == name)
			{
				return profilePerk;
			}
		}
		return null;
	}

	public List<ProfilePerkContainer> GetLevelContainers()
	{
		return levelContainers;
	}

	public ProfilePerkContainer GetContainerAtLevel(int level)
	{
		for (int i = 0; i < levelContainers.Count; i++)
		{
			ProfilePerkContainer container = levelContainers[i];
			if (container.Level == level)
			{
				return container;
			}
		}
		return null;
	}

	public ProfilePerkContainer GetNextContainerAfterLevel(int level)
	{
		for (int i = 0; i < levelContainers.Count; i++)
		{
			ProfilePerkContainer container = levelContainers[i];
			if (container.Level > level)
			{
				return container;
			}
		}
		return null;
	}

	public PerkBranch FindNextBranchAfterLevel(int level)
	{
		for (int i = 0; i < branches.Count; i++)
		{
			PerkBranch branch = branches[i];
			if (branch.Level > level)
			{
				return branch;
			}
		}
		return null;
	}

	private void ParseBranch(XmlNode node)
	{
		int level = node.Attributes["Value"].ParseInt();
		PerkBranch branch = new PerkBranch(level);
		foreach (XmlNode childNode in node.ChildNodes)
		{
			PerkItemType itemType = ParseItemType(childNode.Name);
			string perkName = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			PerkItem item = new PerkItem(itemType, perkName, level);
			branch.Items.Add(item);
		}
		branches.Add(branch);
	}

	private void RemoveProfilePerksWithLevel(int level)
	{
		int num = 0;
		while (num != profilePerks.Count)
		{
			if (profilePerks[num].GetLevel() == level)
			{
				profilePerks.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
		ProfilePerkContainer container = GetContainerAtLevel(level);
		if (container != null)
		{
			List<ProfilePerk> containerPerks = container.Perks;
			containerPerks.Clear();
		}
	}

	private void PopulateBranchPerks(PerkBranch branch)
	{
		List<PerkItem> list = branch.GetAvailableItems();
		ProfilePerkContainer container = GetContainerAtLevel(branch.Level);
		for (int i = 0; i < list.Count; i++)
		{
			PerkItem branchItem = list[i];
			PerkInfoItem perkInfo = FindPerkInfo(branchItem.Name);
			if (perkInfo != null)
			{
				ProfilePerk item = new ProfilePerk(perkInfo, branchItem.Level, ProfilePerk.ProfilePerkState.PERK_LOCK, ToProfilePerkType(branchItem.Type));
				profilePerks.Add(item);
				if (container != null)
				{
					container.Perks.Add(item);
				}
			}
		}
	}

	private void AddEmptyContainer(int level, int count = 2)
	{
		ProfilePerkContainer container = new ProfilePerkContainer(level);
		levelContainers.Add(container);
		for (int i = 0; i < count; i++)
		{
			ProfilePerk item = new ProfilePerk(null, level, ProfilePerk.ProfilePerkState.PERK_LOCK);
			profilePerks.Add(item);
			container.Perks.Add(item);
		}
	}

	private void RefreshBranch(PerkBranch branch)
	{
		RemoveProfilePerksWithLevel(branch.Level);
		PopulateBranchPerks(branch);
		profilePerks.Sort((ProfilePerk left, ProfilePerk right) => left.GetLevel().CompareTo(right.GetLevel()));
	}

	private void UpdateStatesForLearnedPerk(PerkHistory.Perk learnedPerk)
	{
		List<ProfilePerk> list = GetProfilePerksAtLevel(learnedPerk.Level);
		for (int i = 0; i < list.Count; i++)
		{
			ProfilePerk profilePerk = list[i];
			ProfilePerk.ProfilePerkState newState = ((!(profilePerk.GetPerkName() == learnedPerk.Name)) ? ProfilePerk.ProfilePerkState.PERK_UNAVAILABLE : ProfilePerk.ProfilePerkState.PERK_SELECTED);
			profilePerk.set_State(newState);
		}
		List<ProfilePerk> list2 = GetNextLevelProfilePerks(learnedPerk.Level);
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
		PerkInfoItem perkInfo = null;
		foreach (PerkInfoItem item in availablePerkInfos)
		{
			if (item.Name == name)
			{
				perkInfo = item;
				break;
			}
		}
		if (perkInfo == null)
		{
			perkInfo = GameUtils.PerkItemList.FindBasePerk(name);
		}
		return perkInfo;
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

	private PerkItemType ParseItemType(string typeName)
	{
		PerkItemType result = PerkItemType.TYPE_NONE;
		if (typeName == "Perk")
		{
			result = PerkItemType.TYPE_PERK;
		}
		else if (typeName == "Upgrade")
		{
			result = PerkItemType.TYPE_UPGRADE;
		}
		return result;
	}

	private ProfilePerk.ProfilePerkType ToProfilePerkType(PerkItemType itemType)
	{
		switch (itemType)
		{
		case PerkItemType.TYPE_PERK:
			return ProfilePerk.ProfilePerkType.TYPE_PERK;
		case PerkItemType.TYPE_UPGRADE:
			return ProfilePerk.ProfilePerkType.TYPE_UPGRADE;
		default:
			return ProfilePerk.ProfilePerkType.TYPE_NONE;
		}
	}

	private List<ProfilePerk> GetNextLevelProfilePerks(int level)
	{
		List<ProfilePerk> list = new List<ProfilePerk>();
		int num = level;
		for (int i = 0; i < profilePerks.Count; i++)
		{
			ProfilePerk profilePerk = profilePerks[i];
			int num2 = profilePerk.GetLevel();
			if (num2 > num && num > level)
			{
				break;
			}
			if (num2 > level)
			{
				num = num2;
				list.Add(profilePerk);
			}
		}
		return list;
	}
}
