using UnityEngine;
using UnityEngine.UI;

namespace Nekki.SF2.GUI.Map
{
	public class ContentDuel : ContentBase
	{
		[SerializeField]
		private LabelAlias _lblDescription;

		[SerializeField]
		private DifficultyPanel _difficultyPanel;

		[SerializeField]
		private PrizePanel _prizePanel;

		[SerializeField]
		protected Button _btnPeriodicReset;

		private Battle battle;

		public void Init(Battle DPOOIONCEOA, FightList KOMGFJOCEDN)
		{
			battle = DPOOIONCEOA;
			_lblDescription.SetAlias(KOMGFJOCEDN.GetDescription());
			bool mMDLKOPCFLK = (DPOOIONCEOA.get_Type() != BattleType.FightBosses && DPOOIONCEOA.get_Type() != BattleType.FightBossesReplayable && DPOOIONCEOA.get_Type() != BattleType.FightFinalTitan) || !HasRewardItem(KOMGFJOCEDN);
			_prizePanel.Init(-1, mMDLKOPCFLK, KOMGFJOCEDN);
			_difficultyPanel.gameObject.SetActive(DPOOIONCEOA.GetFightCount() != 0);
			_difficultyPanel.Init(GameUtils.CalculateFightDifficulty(KOMGFJOCEDN));
			_btnPeriodicReset.gameObject.SetActive(false);
			if (DPOOIONCEOA.get_Type() == BattleType.FightPeriodic && SystemProperties.IsDebug())
			{
				_btnPeriodicReset.gameObject.SetActive(true);
				_btnPeriodicReset.onClick.AddListener(OnPeriodicResetClicked);
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

		private void OnPeriodicResetClicked()
		{
			if (battle != null)
			{
				if (battle.get_Type() == BattleType.FightPeriodic)
				{
					BattlePeriodic.Reset(false);
				}
				CallEvent(0, battle);
			}
		}
	}
}
