using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Nekki.SF2.GUI.Map
{
	public class PrizePanel : SFMonoBehaviour<object>
	{
		[SerializeField]
		private BattlePrize _prize;

		public const int MONEY_LABEL_SIZE = 68;

		public const float PRIZE_CONTAINER_HEIGHT = 200f;

		public const float PRIZE_CONTAINER_HEIGHT_BOSSES = 300f;

		public void Init(int count, bool includeRewards, FightList fightList)
		{
			if (fightList != null)
			{
				long money = 0L;
				long num = 0L;
				if (includeRewards)
				{
					money = (ObscuredLong)(fightList.PrizeMoney);
					num = (ObscuredLong)(fightList.PrizeBonus);
				}
				money = GameUtils.GetDenominatedValue(money);
				if (count > 0)
				{
					money *= count;
					num *= count;
				}
				RewardStruct lastReward = null;
				if (fightList.GetRewards().Count > 0)
				{
					lastReward = fightList.GetRewards()[fightList.GetRewards().Count - 1];
				}
				float num2 = 0f;
				BattleType fightType = fightList.get_Type();
				num2 = ((fightType != BattleType.FightBosses && fightType != BattleType.FightFinalTitan) ? 200f : 300f);
				int playerLevel = ListSF.GetRoster().GetLevel();
				RewardPrize rewardPrize = lastReward == null ? new RewardPrize() : lastReward.GetPrizeForLevel(playerLevel);
				int prizeIconSize = 68;
				_prize.Init(money, num, rewardPrize, 0f, num2, prizeIconSize);
				UpdatePrizeLayout(0f, num2);
			}
		}

		private void UpdatePrizeLayout(float offset, float containerHeight)
		{
		}
	}
}
