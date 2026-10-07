using UnityEngine;

namespace Nekki.SF2.GUI.Map
{
	public class ContentBossesFinal : ContentBase
	{
		[SerializeField]
		private LabelAlias _replaysLabel;

		[SerializeField]
		private LabelAlias _lblDescription;

		[SerializeField]
		private PrizePanel _prizePanel;

		public void Init(Battle DPOOIONCEOA, FightList KOMGFJOCEDN)
		{
			string empty = string.Empty;
			empty = ((DPOOIONCEOA.get_Type() == BattleType.FightBosses || DPOOIONCEOA.get_Type() == BattleType.FightBossesReplayable || DPOOIONCEOA.get_Type() == BattleType.FightFinalTitan) ? (LocalizationManager.GetString(DPOOIONCEOA.GetTitle()) + " " + LocalizationManager.GetString("challengeBoss")) : LocalizationManager.GetString(DPOOIONCEOA.GetDescription()));
            if (DPOOIONCEOA.get_Type() == BattleType.FightRaid)
            {
                empty = LocalizationManager.GetString(DPOOIONCEOA.GetTitle()) + " " +
                    LocalizationManager.GetString("challengeBoss") + ".";
            }
			_lblDescription.set_text(empty);
			UpdateReplaysLabel(DPOOIONCEOA);
			bool mMDLKOPCFLK = (DPOOIONCEOA.get_Type() != BattleType.FightBosses && DPOOIONCEOA.get_Type() != BattleType.FightBossesReplayable && DPOOIONCEOA.get_Type() != BattleType.FightFinalTitan) || !HasRewardItem(KOMGFJOCEDN);
			_prizePanel.Init(-1, mMDLKOPCFLK, KOMGFJOCEDN);
		}

		private void UpdateReplaysLabel(Battle DPOOIONCEOA)
		{
			if (DPOOIONCEOA.get_Type() == BattleType.FightFinalReplayable)
			{
				_replaysLabel.gameObject.SetActive(true);
				BattleReplayable bKKPCBGAEHC = (BattleReplayable)DPOOIONCEOA;
				_replaysLabel.set_text(Eclipse.UI.BattleInfoText.Replays(bKKPCBGAEHC));
			}
			else
			{
				_replaysLabel.gameObject.SetActive(false);
			}
		}

		private bool HasRewardItem(FightList KOMGFJOCEDN)
		{
			if (KOMGFJOCEDN == null)
			{
				return false;
			}
			RewardStruct fDFKLPHBAHJ = KOMGFJOCEDN.GetRewards()[KOMGFJOCEDN.GetRewards().Count - 1];
			int gNLOCMLBNHF = ListSF.GetRoster().GetLevel();
			RewardPrize cMHHEHILIIH = fDFKLPHBAHJ.GetPrizeForLevel(gNLOCMLBNHF);
			if (cMHHEHILIIH.items.Count == 0)
			{
				return false;
			}
			RewardItem cACJANFAJEC = cMHHEHILIIH.items[0];
			if (!cACJANFAJEC.ShowReward)
			{
				return false;
			}
			ItemInfo dJKEECEOCJB = ListSF.GetItems().GetItemByName(cACJANFAJEC.Name);
			return dJKEECEOCJB != null;
		}
	}
}
