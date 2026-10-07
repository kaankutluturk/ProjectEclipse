using System;
using System.Collections.Generic;
using System.Xml;

public class PerkHistory
{
	public class Perk : IComparable<Perk>
	{
		public string Name;

		public int Level;

		public Perk(string name, int GNLOCMLBNHF)
		{
			Name = name;
			Level = GNLOCMLBNHF;
		}

		public int CompareTo(Perk NOLFMPDGCOC)
		{
			return Level.CompareTo(NOLFMPDGCOC.Level);
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
			string gOHIIMFFFJI = item.Attributes["Perk"].GetStringOrDefault();
			int gNLOCMLBNHF = item.Attributes["Value"].ParseInt();
			Perks.Add(new Perk(gOHIIMFFFJI, gNLOCMLBNHF));
		}
		Perks.Sort();
	}

	public Perk FindPerkByLevel(int GNLOCMLBNHF)
	{
		foreach (Perk item in Perks)
		{
			if (item.Level == GNLOCMLBNHF)
			{
				return item;
			}
		}
		return null;
	}

	private bool IsExistPerkWithLevel(int GNLOCMLBNHF)
	{
		return FindPerkByLevel(GNLOCMLBNHF) != null;
	}

	public Perk AddPerk(string name, int GNLOCMLBNHF)
	{
		if (IsExistPerkWithLevel(GNLOCMLBNHF) || name == string.Empty)
		{
			return null;
		}
		Perk hNHILOOIIMO = new Perk(name, GNLOCMLBNHF);
		Perks.Add(hNHILOOIIMO);
		ListSF.GetRoster().GetPerks().SavePerkHistoryEntry(hNHILOOIIMO);
		ListSF.GetInstance().RequestSave();
		return hNHILOOIIMO;
	}
}
