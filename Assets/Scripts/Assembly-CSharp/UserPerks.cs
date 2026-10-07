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

	public UserPerks(ModelParameters JCICKLIMBEF)
	{
		modelParameters = JCICKLIMBEF;
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

	public RosterPerk AddOrUpgradePerk(RosterPerkInfo AEFFHJGMNFI)
	{
		foreach (RosterPerk item in perks)
		{
			bool flag = item.get_Name().Equals(AEFFHJGMNFI.Name);
			if (flag)
			{
				int aKKLOMFOLNO = AEFFHJGMNFI.UpgradeLevel;
				if (aKKLOMFOLNO > 0)
				{
					item.SetUpgradeLevel(aKKLOMFOLNO);
				}
					item.AppendNodeChild(AEFFHJGMNFI.Pairs);
					InitializeEclipsePerkParameters(item);
					item.SetPerkInfo(null);
				AddPerk(item, AEFFHJGMNFI);
				return item;
			}
			bool flag2 = string.IsNullOrEmpty(AEFFHJGMNFI.Name);
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
		RosterPerk hOGDBKBFFDJ = new RosterPerk(newChild);
		hOGDBKBFFDJ.SetLevel(AEFFHJGMNFI.Level);
		hOGDBKBFFDJ.set_Name(AEFFHJGMNFI.Name);
		int aKKLOMFOLNO2 = AEFFHJGMNFI.UpgradeLevel;
		if (aKKLOMFOLNO2 > 0)
		{
			hOGDBKBFFDJ.SetUpgradeLevel(aKKLOMFOLNO2);
		}
			hOGDBKBFFDJ.AppendNodeChild(AEFFHJGMNFI.Pairs);
			InitializeEclipsePerkParameters(hOGDBKBFFDJ);
			AddPerk(hOGDBKBFFDJ, AEFFHJGMNFI);
		return hOGDBKBFFDJ;
	}

	public RosterPerk AddOrUpgradePerk(PerkInfoItem AEFFHJGMNFI)
	{
		XmlNode hKPPBKPJOEO = (_node["Perks"] ?? _node.AppendElement("Perks")).AppendElement("Perk");
		RosterPerk hOGDBKBFFDJ = new RosterPerk(hKPPBKPJOEO);
		hOGDBKBFFDJ.set_Name(AEFFHJGMNFI.Name);
		hOGDBKBFFDJ.SetLevel(AEFFHJGMNFI.Level);
			hOGDBKBFFDJ.SetUpgradeLevel(AEFFHJGMNFI.UpgradeLevel);
			InitializeEclipsePerkParameters(hOGDBKBFFDJ);
			return hOGDBKBFFDJ;
	}

	public RosterPerk AddOrUpgradePerk(ProfilePerk AEFFHJGMNFI)
	{
		foreach (RosterPerk item in perks)
		{
			bool flag = item.get_Name() == AEFFHJGMNFI.GetPerkName();
			bool flag2 = AEFFHJGMNFI.get_Type() == ProfilePerk.ProfilePerkType.TYPE_UPGRADE;
			bool flag3 = AEFFHJGMNFI.GetPerkName() == string.Empty;
			if (flag && flag2)
			{
				int num = AEFFHJGMNFI.GetUpgradeLevel();
				if (num > 0)
				{
					item.SetUpgradeLevel(num);
				}
					item.SetPerkInfo(null);
					InitializeEclipsePerkParameters(item);
					AddPerk(item, AEFFHJGMNFI);
				return item;
			}
			if (flag || flag3)
			{
				return item;
			}
		}
		string text = "Perks";
		string jLEKBBJBLOE = "Perk";
		XmlNode mEEAKLDGLDF = ((_node[text] == null) ? _node.AppendElement(text) : _node[text]);
		XmlNode hKPPBKPJOEO = mEEAKLDGLDF.AppendElement(jLEKBBJBLOE);
		RosterPerk hOGDBKBFFDJ = new RosterPerk(hKPPBKPJOEO);
		hOGDBKBFFDJ.SetLevel(AEFFHJGMNFI.GetLevel());
		hOGDBKBFFDJ.set_Name(AEFFHJGMNFI.GetPerkName());
		int num2 = AEFFHJGMNFI.GetUpgradeLevel();
			if (num2 > 0)
			{
				hOGDBKBFFDJ.SetUpgradeLevel(num2);
			}
			InitializeEclipsePerkParameters(hOGDBKBFFDJ);
			AddPerk(hOGDBKBFFDJ, AEFFHJGMNFI);
		return hOGDBKBFFDJ;
	}

	public void SavePerkHistoryEntry(PerkHistory.Perk AEFFHJGMNFI)
	{
		if (AEFFHJGMNFI != null)
		{
			GameLog.Write("Save " + _node.Name);
			XmlNode mEEAKLDGLDF = _node["PerkHistory"] ?? _node.AppendElement("PerkHistory");
			XmlNode mEEAKLDGLDF2 = mEEAKLDGLDF.AppendElement("Level");
			mEEAKLDGLDF2.AppendAttribute("Value").Value = AEFFHJGMNFI.Level.ToString();
			mEEAKLDGLDF2.AppendAttribute("Perk").Value = AEFFHJGMNFI.Name;
			ListSF.GetRoster().RequestSave();
		}
	}

	public void AddPerk(RosterPerk AEFFHJGMNFI)
	{
		if (AEFFHJGMNFI != null)
		{
			RegisterPerk(AEFFHJGMNFI);
		}
	}

	public void AddPerk(RosterPerk PPPNCJLGJPE, ProfilePerk AEFFHJGMNFI)
	{
		RemoveLearnedPerk(AEFFHJGMNFI.GetPerkInfo());
		AddPerk(PPPNCJLGJPE);
		ListSF.GetRoster().RequestSave();
		Sound.PlaySound("snd_learn");
	}

	public void AddPerk(RosterPerk PPPNCJLGJPE, RosterPerkInfo BPANICNCIAO)
	{
		RemoveLearnedPerk(BPANICNCIAO.PerkInfo);
		if (PPPNCJLGJPE != null)
		{
			RegisterPerk(PPPNCJLGJPE, true);
		}
		ListSF.GetRoster().RequestSave();
	}

	public RosterPerk FindPerk(string name)
	{
		for (int i = 0; i < perks.Count; i++)
		{
			RosterPerk hOGDBKBFFDJ = perks[i];
			if (hOGDBKBFFDJ.get_Name() == name)
			{
				return hOGDBKBFFDJ;
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
		UserItem dKCHDHMLKHN = ListSF.GetRoster().GetInventory().FindItem("Perk_Reset");
		if (dKCHDHMLKHN != null)
		{
			return dKCHDHMLKHN.GetCount();
		}
		return 0;
	}

	public void RegisterPerk(RosterPerk value, bool PGDIBFDIEIB = false)
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
			RosterPerk hOGDBKBFFDJ = perks.Find((RosterPerk DHDMNHCIPEH) => DHDMNHCIPEH.Equals(value.get_Name()));
			if (hOGDBKBFFDJ != null)
			{
				perks.Remove(hOGDBKBFFDJ);
			}
			if (PGDIBFDIEIB)
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
			PerkInfoItem aCONCDFDNJH = modelParameters.LearnedPerks.Find((PerkInfoItem DHDMNHCIPEH) => DHDMNHCIPEH.Name.Equals(value.Name));
			if (aCONCDFDNJH != null)
			{
				modelParameters.LearnedPerks.Remove(aCONCDFDNJH);
			}
		}
	}
}
