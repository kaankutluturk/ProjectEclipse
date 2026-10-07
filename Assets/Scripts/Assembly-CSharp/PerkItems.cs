using System;
using System.Collections.Generic;
using System.Xml;

public class PerkItems
{
	private List<PerkInfoItem> basePerks = new List<PerkInfoItem>();

	private List<PerkInfoItem> progressionPerks = new List<PerkInfoItem>();

	private List<PerkInfoItem> userPerks = new List<PerkInfoItem>();

	private HashSet<string> externalBasePerkNames = new HashSet<string>(StringComparer.Ordinal);
	private readonly HashSet<PerkInfoItem> externalUpgradeVariants = new HashSet<PerkInfoItem>();

    public void AddExternalPerkUpgrades(string name, XmlNode upgrades, int initialUpgradeLevel = 0)
	{
		if (!externalBasePerkNames.Contains(name)) throw new InvalidOperationException("Upgrade owner is not an external perk: " + name);
		if (GetProgressionVariants(name).Count != 0) throw new InvalidOperationException("Progression variants already exist: " + name);
		PerkInfoItem original = FindBasePerk(name);
		var variants = new List<PerkInfoItem>();
        if (upgrades == null || initialUpgradeLevel < 0)
            throw new ArgumentException("Perk upgrades require a nonnegative initial level and a table.");
        if (initialUpgradeLevel == 0)
        {
            var baseline = original.Clone(null, null);
            baseline.UpgradeLevel = 0;
            variants.Add(baseline);
        }
        foreach (XmlNode node in upgrades.ChildNodes)
            if (node.Name == "UpgradeLevel")
            {
                var variant = CreateUpgradeVariant(original, node);
                if (variant.UpgradeLevel >= initialUpgradeLevel) variants.Add(variant);
            }
        if (variants.Count == 0 || variants[0].UpgradeLevel != initialUpgradeLevel)
            throw new InvalidOperationException("Initial perk upgrade is unavailable: " + name + "/" + initialUpgradeLevel);
		foreach (var variant in variants)
		{
			variant.IsHidden = false;
			progressionPerks.Add(variant);
			externalUpgradeVariants.Add(variant);
		}
	}

	public List<PerkInfoItem> BasePerks
	{
		get
		{
			return GetBasePerks();
		}
	}

	public List<PerkInfoItem> ProgressionPerks
	{
		get
		{
			return GetProgressionPerks();
		}
	}

	public List<PerkInfoItem> UserPerks
	{
		get
		{
			return GetUserPerks();
		}
	}

	public List<PerkInfoItem> GetBasePerks()
	{
		return basePerks;
	}

	public List<PerkInfoItem> GetProgressionPerks()
	{
		return progressionPerks;
	}

	public List<PerkInfoItem> GetUserPerks()
	{
		return userPerks;
	}

	public void Parse(XmlNode node)
	{
		basePerks.Clear();
		externalBasePerkNames.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			PerkInfoItem basePerk = new PerkInfoItem();
			basePerk.Parse(childNode);
			basePerk.IsClone = false;
			basePerks.Add(basePerk);
		}
	}

	public PerkInfoItem AddExternalBasePerk(XmlNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (!node.Name.Equals("Perk", StringComparison.Ordinal))
		{
			throw new ArgumentException("External base perk node must be a complete Perk element.", "node");
		}
		string name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentException("External base perk requires a non-empty Name attribute.", "node");
		}
		if (FindBasePerk(name) != null)
		{
			throw new InvalidOperationException("Perk already exists: " + name);
		}

		PerkInfoItem perk = new PerkInfoItem();
		perk.Parse(node);
		perk.IsClone = false;
		basePerks.Add(perk);
		externalBasePerkNames.Add(name);
		return perk;
	}

	public bool RemoveExternalBasePerk(string name)
	{
		if (string.IsNullOrEmpty(name) || !externalBasePerkNames.Contains(name))
		{
			return false;
		}
		for (int i = 0; i < basePerks.Count; i++)
		{
			if (basePerks[i].Name.Equals(name, StringComparison.Ordinal))
			{
				basePerks.RemoveAt(i);
				progressionPerks.RemoveAll(perk => perk.Name == name && externalUpgradeVariants.Remove(perk));
				externalBasePerkNames.Remove(name);
				return true;
			}
		}
		return false;
	}

	public void ParseProgression(XmlNode node)
	{
		progressionPerks.Clear();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string text = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			int level = childNode.Attributes["Level"].ParseInt();
			string descriptionKey = childNode.Attributes["Description"].GetStringOrDefault(string.Empty);
			string moveName = childNode.Attributes["Move"].GetStringOrDefault(string.Empty);
			PerkInfoItem basePerk = FindBasePerk(text);
			if (basePerk == null)
			{
				continue;
			}
			if (childNode.ChildNodes.Count > 0)
			{
				PerkInfoItem aCONCDFDNJH2 = CloneIfOverridden(basePerk, childNode);
				if (aCONCDFDNJH2 != basePerk)
				{
					AddProgressionVariant(aCONCDFDNJH2, level, descriptionKey, moveName);
				}
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name.Equals("UpgradeLevel"))
					{
						PerkInfoItem variantPerk = CreateUpgradeVariant(basePerk, childNode2);
						AddProgressionVariant(variantPerk, level, descriptionKey, moveName);
					}
				}
			}
			else
			{
				AddProgressionVariant(basePerk, level, descriptionKey, moveName);
			}
		}
	}

	public void ParseUserPerks(XmlNode node, bool clearExisting = true, bool replaceExisting = false)
	{
		if (clearExisting)
		{
			userPerks.Clear();
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			XmlNode xmlNode2 = childNode["Set"];
			XmlNode xmlNode3 = childNode["RatingEvaluation"];
			if (xmlNode2 == null && xmlNode3 == null)
			{
				continue;
			}
			string perkName = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
			List<PerkInfoItem> list = GetProgressionVariants(perkName);
			foreach (PerkInfoItem item in list)
			{
				PerkInfoItem userPerk = item.Clone(xmlNode2, xmlNode3);
				if (replaceExisting)
				{
					RemoveUsersPerkByName(userPerk.Name);
				}
				userPerks.Add(userPerk);
			}
		}
	}

	private void RemoveUsersPerkByName(string name)
	{
		PerkInfoItem userPerk = userPerks.Find((PerkInfoItem entry) => entry.Name.Equals(name));
		if (userPerk != null)
		{
			userPerks.Remove(userPerk);
		}
	}

	private void AddProgressionVariant(PerkInfoItem perk, int level, string descriptionKey, string moveName)
	{
		if (!string.IsNullOrEmpty(descriptionKey))
		{
			perk.DescriptionKey = descriptionKey;
		}
		perk.IsHidden = false;
		perk.Level = level;
		perk.MoveName = moveName;
		progressionPerks.Add(perk);
	}

	private PerkInfoItem CloneIfOverridden(PerkInfoItem perk, XmlNode node)
	{
		PerkInfoItem result = perk;
		XmlNode xmlNode = node["Set"];
		XmlNode xmlNode2 = node["RatingEvaluation"];
		if (xmlNode != null || xmlNode2 != null)
		{
			result = perk.Clone(xmlNode, xmlNode2);
		}
		return result;
	}

	private PerkInfoItem CreateUpgradeVariant(PerkInfoItem basePerk, XmlNode node)
	{
		PerkInfoItem upgradeVariant = basePerk.Clone(node["Set"], node["RatingEvaluation"]);
		string text = node.Attributes["Description"].GetStringOrDefault(string.Empty);
		int upgradeLevel = node.Attributes["Value"].ParseInt();
		if (!string.IsNullOrEmpty(text))
		{
			upgradeVariant.DescriptionKey = text;
		}
		upgradeVariant.UpgradeLevel = upgradeLevel;
		return upgradeVariant;
	}

	public PerkInfoItem FindBasePerk(string name)
	{
		foreach (PerkInfoItem item in basePerks)
		{
			if (item.Name.Equals(name))
			{
				return item;
			}
		}
		return null;
	}

	public PerkInfoItem FindUserPerk(string name)
	{
		return userPerks.Find((PerkInfoItem entry) => entry.Name.Equals(name));
	}

	public PerkInfoItem FindProgressionPerk(string name, int upgradeLevel = -1)
	{
		return progressionPerks.Find((PerkInfoItem entry) =>
		{
			bool flag = entry.Name.Equals(name);
			bool flag2 = upgradeLevel < 0 || entry.UpgradeLevel == upgradeLevel;
			return flag && flag2;
		});
	}

	public List<PerkInfoItem> GetProgressionVariants(string name)
	{
		return progressionPerks.FindAll((PerkInfoItem entry) => entry.Name.Equals(name));
	}
}
