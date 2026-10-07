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

	private void OnLedgerReceived(bool DCJLKCFKCOM, string data, object IEHMCKBJCAK)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (nKGLHEGIKKP == null)
		{
			Debug.LogError("Roster not created error");
		}
		else
		{
			if (!DCJLKCFKCOM)
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
					string lFLGCDNKNJI = jSONNode2["cur"].GetString();
					string fDGOFODPGPH = jSONNode2["ini"].GetString();
					GiveReward(lFLGCDNKNJI, jSONNode2["cnt"], fDGOFODPGPH);
				}
			}
			OnRewardsGiven();
			SystemProperties.set_UnconfirmedLedgerIDs(unconfirmedIds.ToArray());
			ConfirmRewards();
		}
	}

	private void GiveReward(string LFLGCDNKNJI, JSONNode NICNMHCJIBJ, string FDGOFODPGPH)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		if (FDGOFODPGPH == "admin")
		{
			switch (LFLGCDNKNJI)
			{
			case "GEMS":
				AddGems(NICNMHCJIBJ.ParseInt());
				DialogsOpener.OpenSimpleDialog("dlgAlertTitle", string.Concat("dlgGotGift{img::MiscSprites.ruby}{", NICNMHCJIBJ, "}"), "dlgStoryBtnTake", string.Empty, null, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
				break;
			case "COINS":
				AddCoins(NICNMHCJIBJ.ParseInt());
				DialogsOpener.OpenSimpleDialog("dlgAlertTitle", string.Concat("dlgGotGift{img::MiscSprites.gold}{", NICNMHCJIBJ, "}"), "dlgStoryBtnTake", string.Empty, null, LabelButton.ButtonColor.BUTTON_WHITE, LabelButton.ButtonColor.BUTTON_DARK, false, false, string.Empty);
				break;
			default:
				Debug.LogError("LedgerManagerSF::giveReward Unknown currency type");
				break;
			}
		}
		else if (!(LFLGCDNKNJI == "GEMS"))
		{
			if (LFLGCDNKNJI == "AscensionTicket")
			{
				nKGLHEGIKKP.AddCurrencyCount("AscensionTicket", NICNMHCJIBJ.ParseInt());
			}
			else
			{
				Debug.LogError("LedgerManager.GiveReward() Unknown currency type");
			}
		}
		MenuController.RefreshMoney();
	}

	private static void AddCoins(int NICNMHCJIBJ)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		nKGLHEGIKKP.SetMoney(Math.Max(0L, nKGLHEGIKKP.GetMoney() + NICNMHCJIBJ));
	}

	private static void AddGems(int NICNMHCJIBJ)
	{
		Roster nKGLHEGIKKP = ListSF.GetRoster();
		nKGLHEGIKKP.SetBonus(Math.Max(0L, nKGLHEGIKKP.GetBonus() + NICNMHCJIBJ), Roster.BalanceChangeType.CHANGE_LEDGER);
	}

	private void OnRewardsGiven()
	{
	}

	private void ConfirmRewards()
	{
		// No remote reward confirmation in offline builds.
	}

	private void OnConfirmResponse(bool DCJLKCFKCOM, string data, object IEHMCKBJCAK)
	{
		if (DCJLKCFKCOM)
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
