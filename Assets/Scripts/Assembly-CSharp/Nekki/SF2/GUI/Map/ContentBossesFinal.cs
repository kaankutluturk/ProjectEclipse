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

		public void Init(Battle battle, FightList fight)
		{
			string empty = string.Empty;
			empty = ((battle.get_Type() == BattleType.FightBosses || battle.get_Type() == BattleType.FightBossesReplayable || battle.get_Type() == BattleType.FightFinalTitan) ? (LocalizationManager.GetString(battle.GetTitle()) + " " + LocalizationManager.GetString("challengeBoss")) : LocalizationManager.GetString(battle.GetDescription()));
            if (battle.get_Type() == BattleType.FightRaid)
            {
                empty = LocalizationManager.GetString(battle.GetTitle()) + " " +
                    LocalizationManager.GetString("challengeBoss") + ".";
            }
			_lblDescription.set_text(empty);
			UpdateReplaysLabel(battle);
			bool hideItems = (battle.get_Type() != BattleType.FightBosses && battle.get_Type() != BattleType.FightBossesReplayable && battle.get_Type() != BattleType.FightFinalTitan) || !HasRewardItem(fight);
			_prizePanel.Init(-1, hideItems, fight);
		}

		private void UpdateReplaysLabel(Battle battle)
		{
			if (battle.get_Type() == BattleType.FightFinalReplayable)
			{
				_replaysLabel.gameObject.SetActive(true);
				BattleReplayable replayable = (BattleReplayable)battle;
				_replaysLabel.set_text(Eclipse.UI.BattleInfoText.Replays(replayable));
			}
			else
			{
				_replaysLabel.gameObject.SetActive(false);
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
	}
}
