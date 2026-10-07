using System;
using System.Collections.Generic;
using System.Xml;

public class PerkHistory
{
	public class Perk : IComparable<Perk>
	{
		public string Name;

		public int Level;

		public Perk(string name, int level)
		{
			Name = name;
			Level = level;
		}

		public int CompareTo(Perk other)
		{
			return Level.CompareTo(other.Level);
		}
	}

	public List<Perk> Perks = new List<Perk>();

	public void Parse(XmlNode node)
	{
		Perks.Clear();
		if (node == null)
		{
			return;
		}
		foreach (XmlNode item in node)
		{
			string perkName = item.Attributes["Perk"].GetStringOrDefault();
			int level = item.Attributes["Value"].ParseInt();
			Perks.Add(new Perk(perkName, level));
		}
		Perks.Sort();
	}

	public Perk FindPerkByLevel(int level)
	{
		foreach (Perk item in Perks)
		{
			if (item.Level == level)
			{
				return item;
			}
		}
		return null;
	}

	private bool IsExistPerkWithLevel(int level)
	{
		return FindPerkByLevel(level) != null;
	}

	public Perk AddPerk(string name, int level)
	{
		if (IsExistPerkWithLevel(level) || name == string.Empty)
		{
			return null;
		}
		Perk perkEntry = new Perk(name, level);
		Perks.Add(perkEntry);
		ListSF.GetRoster().GetPerks().SavePerkHistoryEntry(perkEntry);
		ListSF.GetInstance().RequestSave();
		return perkEntry;
	}
}
