using System;
using System.Collections.Generic;
using System.Linq;
using Nekki.SF2.Core.Network;
using SimpleJSON;
using UnityEngine;

public class LedgerManager
{
	private List<int> unconfirmedIds;

	public void Check()
	{
		// The dojo calls this during Fight construction. Offline startup does not
		// load remote ledger settings, and local fights must not depend on them.
	}

	private void OnLedgerReceived(bool success, string data, object state)
	{
		Roster roster = ListSF.GetRoster();
		if (roster == null)
		{
			Debug.LogError("Roster not created error");
		}
		else
		{
			if (!success)
			{
				return;
			}
			JSONNode jSONNode = JSON.Parse(data);
			unconfirmedIds = SystemProperties.GetUnconfirmedLedgerIDs().ToList();
			for (int i = 0; i < jSONNode.Count; i++)
			{
				JSONNode jSONNode2 = jSONNode[i];
				int asInt = jSONNode2["rid"].AsInt;
				if (!unconfirmedIds.Contains(asInt))
				{
					unconfirmedIds.Add(asInt);
					string currency = jSONNode2["cur"].GetString();
					string source = jSONNode2["ini"].GetString();
					GiveReward(currency, jSONNode2["cnt"], source);
				}
			}
			OnRewardsGiven();
			SystemProperties.set_UnconfirmedLedgerIDs(unconfirmedIds.ToArray());
			ConfirmRewards();
		}
	}

	private void GiveReward(string currency, JSONNode amount, string source)
	{
		Roster roster = ListSF.GetRoster();
		if (source == "admin")
		{
			switch (currency)
			{
			case "GEMS":
				AddGems(amount.ParseInt());
				DialogsOpener.OpenSimpleDialog("dlgAlertTitle", string.Concat("dlgGotGift{img::MiscSprites.ruby}{", amount, "}"), "dlgStoryBtnTake", string.Empty, null, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
				break;
			case "COINS":
				AddCoins(amount.ParseInt());
				DialogsOpener.OpenSimpleDialog("dlgAlertTitle", string.Concat("dlgGotGift{img::MiscSprites.gold}{", amount, "}"), "dlgStoryBtnTake", string.Empty, null, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
				break;
			default:
				Debug.LogError("LedgerManagerSF::giveReward Unknown currency type");
				break;
			}
		}
		else if (!(currency == "GEMS"))
		{
			if (currency == "AscensionTicket")
			{
				roster.AddCurrencyCount("AscensionTicket", amount.ParseInt());
			}
			else
			{
				Debug.LogError("LedgerManager.GiveReward() Unknown currency type");
			}
		}
		MenuController.RefreshMoney();
	}

	private static void AddCoins(int amount)
	{
		Roster roster = ListSF.GetRoster();
		roster.SetMoney(Math.Max(0L, roster.GetMoney() + amount));
	}

	private static void AddGems(int amount)
	{
		Roster roster = ListSF.GetRoster();
		roster.SetBonus(Math.Max(0L, roster.GetBonus() + amount), Roster.BalanceChangeType.CHANGE_LEDGER);
	}

	private void OnRewardsGiven()
	{
	}

	private void ConfirmRewards()
	{
		// No remote reward confirmation in offline builds.
	}

	private void OnConfirmResponse(bool success, string data, object state)
	{
		if (success)
		{
			JSONNode jSONNode = JSON.Parse(data);
			for (int i = 0; i < jSONNode.Count; i++)
			{
				unconfirmedIds.Remove(jSONNode[i].AsInt);
			}
			SystemProperties.set_UnconfirmedLedgerIDs(unconfirmedIds.ToArray());
		}
	}
}
