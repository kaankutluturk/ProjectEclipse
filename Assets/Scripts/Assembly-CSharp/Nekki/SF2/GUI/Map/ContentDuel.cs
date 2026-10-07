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

		public void Init(Battle newBattle, FightList fight)
		{
			battle = newBattle;
			_lblDescription.SetAlias(fight.GetDescription());
			bool hideItems = (newBattle.get_Type() != BattleType.FightBosses && newBattle.get_Type() != BattleType.FightBossesReplayable && newBattle.get_Type() != BattleType.FightFinalTitan) || !HasRewardItem(fight);
			_prizePanel.Init(-1, hideItems, fight);
			_difficultyPanel.gameObject.SetActive(newBattle.GetFightCount() != 0);
			_difficultyPanel.Init(GameUtils.CalculateFightDifficulty(fight));
			_btnPeriodicReset.gameObject.SetActive(false);
			if (newBattle.get_Type() == BattleType.FightPeriodic && SystemProperties.IsDebug())
			{
				_btnPeriodicReset.gameObject.SetActive(true);
				_btnPeriodicReset.onClick.AddListener(OnPeriodicResetClicked);
			}
		}

		private bool HasRewardItem(FightList fight)
		{
			if (fight == null)
			{
				return false;
			}
			RewardStruct rewardStruct = fight.GetRewards()[fight.GetRewards().Count - 1];
			int level = ListSF.GetRoster().GetLevel();
			RewardPrize prize = rewardStruct.GetPrizeForLevel(level);
			if (prize.items.Count == 0)
			{
				return false;
			}
			RewardItem rewardItem = prize.items[0];
			if (!rewardItem.ShowReward)
			{
				return false;
			}
			ItemInfo itemInfo = ListSF.GetItems().GetItemByName(rewardItem.Name);
			return itemInfo != null;
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
