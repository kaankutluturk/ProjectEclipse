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

		public void Init(int count, bool MMDLKOPCFLK, FightList KOMGFJOCEDN)
		{
			if (KOMGFJOCEDN != null)
			{
				long bAINMLLIKOL = 0L;
				long num = 0L;
				if (MMDLKOPCFLK)
				{
					bAINMLLIKOL = (ObscuredLong)(KOMGFJOCEDN.PrizeMoney);
					num = (ObscuredLong)(KOMGFJOCEDN.PrizeBonus);
				}
				bAINMLLIKOL = GameUtils.GetDenominatedValue(bAINMLLIKOL);
				if (count > 0)
				{
					bAINMLLIKOL *= count;
					num *= count;
				}
				RewardStruct fDFKLPHBAHJ = null;
				if (KOMGFJOCEDN.GetRewards().Count > 0)
				{
					fDFKLPHBAHJ = KOMGFJOCEDN.GetRewards()[KOMGFJOCEDN.GetRewards().Count - 1];
				}
				float num2 = 0f;
				BattleType pJMEMGHKKBM = KOMGFJOCEDN.get_Type();
				num2 = ((pJMEMGHKKBM != BattleType.FightBosses && pJMEMGHKKBM != BattleType.FightFinalTitan) ? 200f : 300f);
				int gNLOCMLBNHF = ListSF.GetRoster().GetLevel();
				RewardPrize dPIIJICBGGA = fDFKLPHBAHJ == null ? new RewardPrize() : fDFKLPHBAHJ.GetPrizeForLevel(gNLOCMLBNHF);
				int cFMPJLLNCFF = 68;
				_prize.Init(bAINMLLIKOL, num, dPIIJICBGGA, 0f, num2, cFMPJLLNCFF);
				UpdatePrizeLayout(0f, num2);
			}
		}

		private void UpdatePrizeLayout(float JMLAKAKDBBL, float FEIHFIPFNKF)
		{
		}
	}
}
