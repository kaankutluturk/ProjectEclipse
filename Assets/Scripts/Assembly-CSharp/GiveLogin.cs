using System;
using System.Collections.Generic;
using Nekki.SF2.Core.Network;
using SimpleJSON;

public class GiveLogin
{
	public bool HasPendingGive;

	public bool WasGiveApplied;

	public long BonusAmount;

	public long MoneyAmount;

	public List<GiveItemLogin> Items = new List<GiveItemLogin>();

	public void Parse(JSONNode value)
	{
		JSONNode jSONNode = null;
		JSONNode jSONNode2 = value["data"];
		if (jSONNode2 != null && jSONNode2.Value.Equals("user"))
		{
			JSONNode jSONNode3 = value["value"];
			if (jSONNode3 != null)
			{
				jSONNode = jSONNode3["gives"];
			}
		}
		if (!(jSONNode != null) || jSONNode.Count <= 0)
		{
			return;
		}
		JSONNode bonusNode = jSONNode["Bonus"];
		JSONNode moneyNode = jSONNode["Money"];
		JSONNode jSONNode4 = jSONNode["Items"];
		long bonusAmount = bonusNode.ParseLong(0L);
		long moneyAmount = moneyNode.ParseLong(0L);
		int num = ((jSONNode4 != null) ? jSONNode4.Count : 0);
		int i = 0;
		for (int num2 = num; i < num2; i++)
		{
			JSONNode jSONNode5 = jSONNode4[i];
			JSONNode jSONNode6 = jSONNode5["Item"];
			JSONNode upgradeLevelNode = jSONNode5["UpgradeLevel"];
			JSONNode countNode = jSONNode5["Count"];
			JSONNode equipNode = jSONNode5["Equip"];
			if (jSONNode6 != null && jSONNode6.Value != null)
			{
				string valueText = jSONNode6.Value;
				int upgradeLevel = upgradeLevelNode.ParseInt();
				int num3 = countNode.ParseInt(1);
				int num4 = equipNode.ParseInt();
				if (num3 > 0)
				{
					GiveItemLogin item = new GiveItemLogin
					{
						Name = valueText,
						UpgradeLevel = upgradeLevel,
						Count = num3,
						Equip = (num4 > 0)
					};
					Items.Add(item);
				}
			}
		}
		BonusAmount = bonusAmount;
		MoneyAmount = moneyAmount;
		HasPendingGive = true;
	}

	public void SendGiveLogin()
	{
		WasGiveApplied = false;
		if (HasPendingGive)
		{
			ServerProvider.get_Instance().SendGiveLogin(OnGiveLoginResponse);
		}
	}

	private void OnGiveLoginResponse(bool success, string data, object state)
	{
		if (success)
		{
			JSONNode jSONNode = JSON.Parse(data)["data"];
			if (jSONNode != null && jSONNode.Value.Equals("success"))
			{
				ApplyGives();
			}
		}
	}

	private void ApplyGives()
	{
		Roster roster = ListSF.GetRoster();
		if (MoneyAmount != 0)
		{
			roster.SetMoney(Math.Max(0L, roster.GetMoney() + MoneyAmount));
		}
		if (BonusAmount != 0)
		{
			roster.SetBonus(Math.Max(0L, roster.GetBonus() + BonusAmount), Roster.BalanceChangeType.CHANGE_SERVER_GIVE);
		}
		if (MoneyAmount != 0 || BonusAmount != 0)
		{
			MenuController.RefreshMoney();
		}
		foreach (GiveItemLogin item in Items)
		{
			ListSF.GetItems().GetItemsByMarketId(item.Name).ForEach((ItemInfo itemInfo) =>
			{
				if (item.Equip)
				{
					ListSF.UnequipOtherItemsOfType(itemInfo);
				}
				ListSF.AddItem(itemInfo, item.Count, 0L, item.Equip);
				if (itemInfo.ItemLevel <= roster.GetLevel())
				{
					itemInfo.SetIsNew(true);
				}
				if (item.UpgradeLevel > 0)
				{
					itemInfo.UpgradeLevel = item.UpgradeLevel;
					ItemInfo currentItem = null;
					ItemInfo nextItem = null;
					itemInfo.FindNextUpgradeItems(roster.GetLevel(), item.UpgradeLevel, ref currentItem, ref nextItem);
				}
			});
		}
		HasPendingGive = false;
		WasGiveApplied = BonusAmount != 0 || MoneyAmount != 0 || Items.Count > 0;
		if (ListSF.GetInstance().RaiseQuestEvent(QuestEvent.QuestEventType.QUEST_EVENT_SERVER_CURRENCY))
		{
			ListSF.GetInstance().RunQuestActions();
		}
	}
}
