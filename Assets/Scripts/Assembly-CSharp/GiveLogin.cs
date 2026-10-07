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
		JSONNode mEEAKLDGLDF = jSONNode["Bonus"];
		JSONNode mEEAKLDGLDF2 = jSONNode["Money"];
		JSONNode jSONNode4 = jSONNode["Items"];
		long oHHLCBPGOIM = mEEAKLDGLDF.ParseLong(0L);
		long jDPAGMPKLHB = mEEAKLDGLDF2.ParseLong(0L);
		int num = ((jSONNode4 != null) ? jSONNode4.Count : 0);
		int i = 0;
		for (int num2 = num; i < num2; i++)
		{
			JSONNode jSONNode5 = jSONNode4[i];
			JSONNode jSONNode6 = jSONNode5["Item"];
			JSONNode mEEAKLDGLDF3 = jSONNode5["UpgradeLevel"];
			JSONNode mEEAKLDGLDF4 = jSONNode5["Count"];
			JSONNode mEEAKLDGLDF5 = jSONNode5["Equip"];
			if (jSONNode6 != null && jSONNode6.Value != null)
			{
				string valueText = jSONNode6.Value;
				int aKKLOMFOLNO = mEEAKLDGLDF3.ParseInt();
				int num3 = mEEAKLDGLDF4.ParseInt(1);
				int num4 = mEEAKLDGLDF5.ParseInt();
				if (num3 > 0)
				{
					GiveItemLogin item = new GiveItemLogin
					{
						Name = valueText,
						UpgradeLevel = aKKLOMFOLNO,
						Count = num3,
						Equip = (num4 > 0)
					};
					Items.Add(item);
				}
			}
		}
		BonusAmount = oHHLCBPGOIM;
		MoneyAmount = jDPAGMPKLHB;
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

	private void OnGiveLoginResponse(bool DCJLKCFKCOM, string data, object IEHMCKBJCAK)
	{
		if (DCJLKCFKCOM)
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
		Roster GJJHILBJOGF = ListSF.GetRoster();
		if (MoneyAmount != 0)
		{
			GJJHILBJOGF.SetMoney(Math.Max(0L, GJJHILBJOGF.GetMoney() + MoneyAmount));
		}
		if (BonusAmount != 0)
		{
			GJJHILBJOGF.SetBonus(Math.Max(0L, GJJHILBJOGF.GetBonus() + BonusAmount), Roster.BalanceChangeType.CHANGE_SERVER_GIVE);
		}
		if (MoneyAmount != 0 || BonusAmount != 0)
		{
			MenuController.RefreshMoney();
		}
		foreach (GiveItemLogin item in Items)
		{
			ListSF.GetItems().GetItemsByMarketId(item.Name).ForEach((ItemInfo PJDAGCBPLJE) =>
			{
				if (item.Equip)
				{
					ListSF.UnequipOtherItemsOfType(PJDAGCBPLJE);
				}
				ListSF.AddItem(PJDAGCBPLJE, item.Count, 0L, item.Equip);
				if (PJDAGCBPLJE.ItemLevel <= GJJHILBJOGF.GetLevel())
				{
					PJDAGCBPLJE.SetIsNew(true);
				}
				if (item.UpgradeLevel > 0)
				{
					PJDAGCBPLJE.UpgradeLevel = item.UpgradeLevel;
					ItemInfo HDMHCCKLLGK = null;
					ItemInfo JLNLOCNBGEK = null;
					PJDAGCBPLJE.FindNextUpgradeItems(GJJHILBJOGF.GetLevel(), item.UpgradeLevel, ref HDMHCCKLLGK, ref JLNLOCNBGEK);
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
