using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class BattleRaid : Battle
{
	public BattleRaid(string typeName, Vector2 mapPosition, string name, string iconName, string previewIcon, string description, ushort rewardDigits, ushort prizeBaseDigits, string alias, string title, string location, string music, string rewardImage, string showResistance)
		: base(typeName, mapPosition, name, iconName, previewIcon, description, rewardDigits, prizeBaseDigits, alias, title, location, music, rewardImage, showResistance)
	{
	}

	public void Parse(XmlNode node)
	{
		XmlNode raidNode = node["RaidData"];
		ParseRaidData(raidNode);
	}

	public bool CanAffordFightCost(FightList fightList)
	{
		List<CurrencyCostRule> list = fightList.GetCurrencyCostRules();
		if (list.Count == 0)
		{
			return true;
		}
		foreach (CurrencyCostRule item in list)
		{
			string text = item.GetCurrencyName();
			if (!(text == string.Empty))
			{
				continue;
			}
			int num = 0;
			int num2 = 0;
			foreach (CurrencyCostRule item2 in list)
			{
				if (item2.GetCurrencyName() == text)
				{
					int num3 = item2.GetCurrencyValue();
					num2 += num3;
				}
			}
			if (num < num2)
			{
				return false;
			}
		}
		return true;
	}

	private void ParseRaidData(XmlNode node)
	{
	}
}
