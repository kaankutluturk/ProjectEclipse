using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Nekki.SF2.GUI.Map
{
	public class ContentSurvival : ContentBase
	{
		[SerializeField]
		private LabelAlias _minLabel;

		[SerializeField]
		private LabelAlias _maxLabel;

		[SerializeField]
		private BattlePrize _survivalPrizeMin;

		[SerializeField]
		private BattlePrize _survivalPrizeMax;

		[SerializeField]
		private LabelAlias _lblDescription;

		public const float PRIZE_CONTAINER_HEIGHT = 100f;

		public const int MONEY_LABEL_SIZE = 68;

		public const int SURVIVAL_LABEL_INTERMISSION_SIZE = 50;

		public void Init(Battle battle)
		{
			List<FightList> list = battle.GetFights();
			_minLabel.gameObject.SetActive(list.Count > 0);
			_maxLabel.gameObject.SetActive(list.Count > 0);
			_survivalPrizeMin.gameObject.SetActive(list.Count > 0);
			_survivalPrizeMax.gameObject.SetActive(list.Count > 0);
			if (list.Count == 0)
			{
				return;
			}
			List<RewardStruct> list2 = list[0].GetRewards();
			long num = 0L;
			long num2 = 0L;
			long num3 = 0L;
			long num4 = 0L;
			int count = list2.Count;
			if (count < 2)
			{
				GameLog.Error("Survival has no rewards");
				return;
			}
			int level = ListSF.GetRoster().GetLevel();
			RewardPrize minPrize = list2[1].GetPrizeForLevel(level);
			RewardPrize prize = list2[list2.Count - 1].GetPrizeForLevel(level);
			num = (ObscuredLong)(minPrize.bonus);
			num2 = (ObscuredLong)(prize.bonus);
			num3 = (ObscuredLong)(minPrize.money);
			num4 = (ObscuredLong)(prize.money);
			num3 = GameUtils.GetDenominatedValue(num3);
			num4 = GameUtils.GetDenominatedValue(num4);
			int fontSize = 68;
			if (battle.get_Type() == BattleType.FightBossesIntermission)
			{
				fontSize = 50;
			}
			_survivalPrizeMin.Init(num3, num, minPrize, 0f, 100f, fontSize);
			_survivalPrizeMax.Init(num4, num2, prize, 0f, 100f, fontSize);
			if (battle.get_Type() == BattleType.FightBossesIntermission)
			{
				_lblDescription.SetAlias(battle.GetDescription());
			}
		}
	}
}
