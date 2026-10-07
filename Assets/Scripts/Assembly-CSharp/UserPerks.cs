using System.Collections.Generic;
using System.Xml;

public class UserPerks
{
	private XmlNode _node;

	private XmlNode perksNode;

	private XmlNode perkHistoryNode;

	private List<RosterPerk> perks = new List<RosterPerk>();

	public PerkHistory History = new PerkHistory();

	protected ModelParameters modelParameters;

	public List<RosterPerk> Perks
	{
		get
		{
			return GetPerks();
		}
	}

	public int FreePerkPoints
	{
		get
		{
			return GetFreePerkPoints();
		}
	}

	public int TotalPerkLevels
	{
		get
		{
			return GetTotalPerkLevels();
		}
	}

	public int PerkResetCount
	{
		get
		{
			return GetPerkResetCount();
		}
	}

	public UserPerks(ModelParameters parameters)
	{
		modelParameters = parameters;
	}

	public List<RosterPerk> GetPerks()
	{
		return perks;
	}

	public int GetFreePerkPoints()
	{
		int num = ListSF.GetRoster().GetLevel() - History.Perks.Count - 1;
		int num2 = PerkTree.GetInstance().GetUnlockedBranchCount();
		int count = History.Perks.Count;
		return num2 - count;
	}

	public void Parse(XmlNode node)
	{
		if (node == null)
		{
			return;
		}
		_node = node;
		perkHistoryNode = node["PerkHistory"];
		History.Parse(perkHistoryNode);
		perksNode = node["Perks"];
		if (perksNode == null)
		{
			return;
		}
		GameUtils.PerkItemList.ParseUserPerks(perksNode);
		foreach (XmlNode childNode in perksNode.ChildNodes)
		{
			AddPerk(new RosterPerk(childNode));
		}
	}

	public RosterPerk AddOrUpgradePerk(RosterPerkInfo rosterPerkInfo)
	{
		foreach (RosterPerk item in perks)
		{
			bool flag = item.get_Name().Equals(rosterPerkInfo.Name);
			if (flag)
			{
				int upgradeLevel = rosterPerkInfo.UpgradeLevel;
				if (upgradeLevel > 0)
				{
					item.SetUpgradeLevel(upgradeLevel);
				}
					item.AppendNodeChild(rosterPerkInfo.Pairs);
					InitializeEclipsePerkParameters(item);
					item.SetPerkInfo(null);
				AddPerk(item, rosterPerkInfo);
				return item;
			}
			bool flag2 = string.IsNullOrEmpty(rosterPerkInfo.Name);
			if (flag || flag2)
			{
				return item;
			}
		}
		string text = "Perks";
		string name = "Perk";
		XmlNode xmlNode = _node[text];
		if (xmlNode == null)
		{
			xmlNode = _node.AppendElement(text);
		}
		XmlNode newChild = _node.OwnerDocument.CreateNode(XmlNodeType.Element, name, null);
		newChild = xmlNode.PrependChild(newChild);
		RosterPerk newPerk = new RosterPerk(newChild);
		newPerk.SetLevel(rosterPerkInfo.Level);
		newPerk.set_Name(rosterPerkInfo.Name);
		int aKKLOMFOLNO2 = rosterPerkInfo.UpgradeLevel;
		if (aKKLOMFOLNO2 > 0)
		{
			newPerk.SetUpgradeLevel(aKKLOMFOLNO2);
		}
			newPerk.AppendNodeChild(rosterPerkInfo.Pairs);
			InitializeEclipsePerkParameters(newPerk);
			AddPerk(newPerk, rosterPerkInfo);
		return newPerk;
	}

	public RosterPerk AddOrUpgradePerk(PerkInfoItem perkInfo)
	{
		XmlNode perkNode = (_node["Perks"] ?? _node.AppendElement("Perks")).AppendElement("Perk");
		RosterPerk newPerk = new RosterPerk(perkNode);
		newPerk.set_Name(perkInfo.Name);
		newPerk.SetLevel(perkInfo.Level);
			newPerk.SetUpgradeLevel(perkInfo.UpgradeLevel);
			InitializeEclipsePerkParameters(newPerk);
			return newPerk;
	}

	public RosterPerk AddOrUpgradePerk(ProfilePerk profilePerk)
	{
		foreach (RosterPerk item in perks)
		{
			bool flag = item.get_Name() == profilePerk.GetPerkName();
			bool flag2 = profilePerk.get_Type() == ProfilePerk.ProfilePerkType.TYPE_UPGRADE;
			bool flag3 = profilePerk.GetPerkName() == string.Empty;
			if (flag && flag2)
			{
				int num = profilePerk.GetUpgradeLevel();
				if (num > 0)
				{
					item.SetUpgradeLevel(num);
				}
					item.SetPerkInfo(null);
					InitializeEclipsePerkParameters(item);
					AddPerk(item, profilePerk);
				return item;
			}
			if (flag || flag3)
			{
				return item;
			}
		}
		string text = "Perks";
		string elementName = "Perk";
		XmlNode perksElement = ((_node[text] == null) ? _node.AppendElement(text) : _node[text]);
		XmlNode perkNode = perksElement.AppendElement(elementName);
		RosterPerk newPerk = new RosterPerk(perkNode);
		newPerk.SetLevel(profilePerk.GetLevel());
		newPerk.set_Name(profilePerk.GetPerkName());
		int num2 = profilePerk.GetUpgradeLevel();
			if (num2 > 0)
			{
				newPerk.SetUpgradeLevel(num2);
			}
			InitializeEclipsePerkParameters(newPerk);
			AddPerk(newPerk, profilePerk);
		return newPerk;
	}

	public void SavePerkHistoryEntry(PerkHistory.Perk historyEntry)
	{
		if (historyEntry != null)
		{
			GameLog.Write("Save " + _node.Name);
			XmlNode historyNode = _node["PerkHistory"] ?? _node.AppendElement("PerkHistory");
			XmlNode mEEAKLDGLDF2 = historyNode.AppendElement("Level");
			mEEAKLDGLDF2.AppendAttribute("Value").Value = historyEntry.Level.ToString();
			mEEAKLDGLDF2.AppendAttribute("Perk").Value = historyEntry.Name;
			ListSF.GetRoster().RequestSave();
		}
	}

	public void AddPerk(RosterPerk perk)
	{
		if (perk != null)
		{
			RegisterPerk(perk);
		}
	}

	public void AddPerk(RosterPerk perk, ProfilePerk profilePerk)
	{
		RemoveLearnedPerk(profilePerk.GetPerkInfo());
		AddPerk(perk);
		ListSF.GetRoster().RequestSave();
		Sound.PlaySound("snd_learn");
	}

	public void AddPerk(RosterPerk perk, RosterPerkInfo rosterPerkInfo)
	{
		RemoveLearnedPerk(rosterPerkInfo.PerkInfo);
		if (perk != null)
		{
			RegisterPerk(perk, true);
		}
		ListSF.GetRoster().RequestSave();
	}

	public RosterPerk FindPerk(string name)
	{
		for (int i = 0; i < perks.Count; i++)
		{
			RosterPerk perk = perks[i];
			if (perk.get_Name() == name)
			{
				return perk;
			}
		}
			return null;
		}

	private static void InitializeEclipsePerkParameters(RosterPerk perk)
	{
		if (perk == null || perk.Node == null) return;
		string error;
		if (!Eclipse.Modding.ModRuntime.TryInitializeSavedPerkParameters(perk.Node, out error))
			UnityEngine.Debug.LogWarning("[ModSave] Failed to initialize scripted perk state for '" +
				perk.get_Name() + "': " + error);
	}

	public int GetTotalPerkLevels()
	{
		int num = 0;
		foreach (RosterPerk item in perks)
		{
			num += item.GetUpgradeLevel();
		}
		return num;
	}

	public void ResetPerks()
	{
		XmlNode xmlNode = _node["Perks"];
		if (xmlNode != null)
		{
			_node.RemoveChild(xmlNode);
		}
		XmlNode xmlNode2 = _node["PerkHistory"];
		if (xmlNode2 != null)
		{
			_node.RemoveChild(xmlNode2);
		}
		XmlNode xmlNode3 = _node["OpenTricks"];
		if (xmlNode3 != null)
		{
			_node.RemoveChild(xmlNode3);
		}
		ListSF.GetRoster().RequestSave();
		perks.Clear();
		History.Perks.Clear();
		PerkTree.GetInstance().RebuildProfile();
		modelParameters.LearnedPerks.Clear();
		GameUtils.PerkItemList.GetUserPerks().Clear();
	}

	public int GetPerkResetCount()
	{
		UserItem resetItem = ListSF.GetRoster().GetInventory().FindItem("Perk_Reset");
		if (resetItem != null)
		{
			return resetItem.GetCount();
		}
		return 0;
	}

	public void RegisterPerk(RosterPerk value, bool insertAtFront = false)
	{
		List<PerkInfoItem> list = GameUtils.PerkItemList.GetUserPerks();
		foreach (PerkInfoItem item in list)
		{
			bool flag = item.Name.Equals(value.get_Name());
			bool flag2 = item.UpgradeLevel == value.GetUpgradeLevel();
			if (flag && flag2)
			{
				value.SetPerkInfo(item);
			}
		}
		if (value.GetPerkInfo() == null)
		{
			List<PerkInfoItem> list2 = GameUtils.PerkItemList.GetProgressionPerks();
			foreach (PerkInfoItem item2 in list2)
			{
				bool flag3 = item2.Name.Equals(value.get_Name());
				bool flag4 = item2.UpgradeLevel == value.GetUpgradeLevel();
				if (flag3 && flag4)
				{
					value.SetPerkInfo(item2);
				}
			}
		}
		if (value.GetPerkInfo() == null)
		{
			List<PerkInfoItem> list3 = GameUtils.PerkItemList.GetBasePerks();
			foreach (PerkInfoItem item3 in list3)
			{
				if (item3.Name.Equals(value.get_Name()))
				{
					value.SetPerkInfo(item3);
				}
			}
		}
		if (value.GetPerkInfo() != null)
		{
			RosterPerk existingPerk = perks.Find((RosterPerk candidate) => candidate.Equals(value.get_Name()));
			if (existingPerk != null)
			{
				perks.Remove(existingPerk);
			}
			if (insertAtFront)
			{
				perks.Insert(0, value);
			}
			else
			{
				perks.Add(value);
			}
			AddLearnedPerk(value.GetPerkInfo());
		}
	}

	private void AddLearnedPerk(PerkInfoItem value)
	{
		if (modelParameters != null)
		{
			modelParameters.LearnedPerks.Add(value);
		}
	}

	private void RemoveLearnedPerk(PerkInfoItem value)
	{
		if (modelParameters != null && value != null)
		{
			PerkInfoItem learnedPerk = modelParameters.LearnedPerks.Find((PerkInfoItem candidate) => candidate.Name.Equals(value.Name));
			if (learnedPerk != null)
			{
				modelParameters.LearnedPerks.Remove(learnedPerk);
			}
		}
	}
}
