using System.Collections.Generic;
using UnityEngine;

public class ModelStatistics
{
	private class AttackStatistics
	{
		private InfoAnimation animation;

		private float pendingDamage;

		private float totalDamage;

		private float _count;

		private float pendingHitCount;

		private float totalHitCount;

		private int _strikeIndex;

		public InfoAnimation Animation
		{
			get
			{
				return GetAnimation();
			}
		}

		public float PendingDamage
		{
			get
			{
				return GetPendingDamage();
			}
			set
			{
				SetPendingDamage(value);
			}
		}

		public float TotalDamage
		{
			get
			{
				return GetTotalDamage();
			}
			set
			{
				SetTotalDamage(value);
			}
		}

		public float Count
		{
			get
			{
				return GetCount();
			}
			set
			{
				SetCount(value);
			}
		}

		public float TotalHitCount
		{
			get
			{
				return GetTotalHitCount();
			}
			set
			{
				SetTotalHitCount(value);
			}
		}

		public AttackStatistics(InfoAnimation DBOLBEOCEME)
		{
			animation = DBOLBEOCEME;
			totalDamage = 0f;
			pendingDamage = 0f;
			_count = 0f;
			totalHitCount = 0f;
			pendingHitCount = 0f;
			_strikeIndex = 0;
		}

		public AttackStatistics(AttackStatistics NBMGOEMJJAF)
		{
			animation = NBMGOEMJJAF.animation;
			totalDamage = NBMGOEMJJAF.totalDamage;
			pendingDamage = NBMGOEMJJAF.pendingDamage;
			_count = NBMGOEMJJAF._count;
			totalHitCount = NBMGOEMJJAF.totalHitCount;
			pendingHitCount = NBMGOEMJJAF.pendingHitCount;
			_strikeIndex = 0;
		}

		public InfoAnimation GetAnimation()
		{
			return animation;
		}

		public float GetPendingDamage()
		{
			return pendingDamage;
		}

		public void SetPendingDamage(float value)
		{
			pendingDamage = value;
		}

		public float GetTotalDamage()
		{
			return totalDamage;
		}

		public void SetTotalDamage(float value)
		{
			totalDamage = value;
		}

		public float GetCount()
		{
			return _count;
		}

		public void SetCount(float value)
		{
			_count = value;
		}

		public float GetTotalHitCount()
		{
			return totalHitCount;
		}

		public void SetTotalHitCount(float value)
		{
			totalHitCount = value;
		}

		private void ApplyDecay(int BCAOGKPNMFG, float DAIGFEOMFIE)
		{
			int num = BCAOGKPNMFG - _strikeIndex;
			if (0 < num)
			{
				float num2 = Mathf.Pow(2f, (0f - (float)num) / DAIGFEOMFIE);
				totalDamage *= num2;
				pendingDamage *= num2;
				_count *= num2;
				totalHitCount *= num2;
				pendingHitCount *= num2;
			}
			_strikeIndex = BCAOGKPNMFG;
		}

		public void AddDamage(float CKKFKEIELCP, int BCAOGKPNMFG, float DAIGFEOMFIE)
		{
			ApplyDecay(BCAOGKPNMFG, DAIGFEOMFIE);
			pendingDamage += CKKFKEIELCP;
			pendingHitCount++;
		}

		public void CommitPending()
		{
			totalDamage += pendingDamage;
			pendingDamage = 0f;
			totalHitCount += pendingHitCount;
			pendingHitCount = 0f;
		}

		public void AddUse(int BCAOGKPNMFG, float DAIGFEOMFIE)
		{
			ApplyDecay(BCAOGKPNMFG, DAIGFEOMFIE);
			_count++;
		}

		public float GetDecayedTotalDamage(int BCAOGKPNMFG, float DAIGFEOMFIE)
		{
			ApplyDecay(BCAOGKPNMFG, DAIGFEOMFIE);
			return totalDamage;
		}

		public float GetDecayedCount(int BCAOGKPNMFG, float DAIGFEOMFIE)
		{
			ApplyDecay(BCAOGKPNMFG, DAIGFEOMFIE);
			return _count;
		}

		public float GetDecayedTotalHitCount(int BCAOGKPNMFG, float DAIGFEOMFIE)
		{
			ApplyDecay(BCAOGKPNMFG, DAIGFEOMFIE);
			return totalHitCount;
		}

		public void ScaleAll(float ratio)
		{
			totalDamage *= ratio;
			pendingDamage *= ratio;
			_count *= ratio;
			totalHitCount *= ratio;
			pendingHitCount *= ratio;
		}
	}

	private Model _model;

	private Dictionary<InfoAnimation, AttackStatistics> dealtStats = new Dictionary<InfoAnimation, AttackStatistics>();

	private Dictionary<InfoAnimation, AttackStatistics> receivedStats = new Dictionary<InfoAnimation, AttackStatistics>();

	private List<InfoAnimation> animationBuffer = new List<InfoAnimation>();

	private static AttackStatistics unusedStatistics;

	private int raidHitCount;

	private int raidCritCount;

	public bool IsCritAvailable
	{
		get
		{
			return CheckCritAvailable();
		}
	}

	public ModelStatistics(Model ACENLMONNPA)
	{
		_model = ACENLMONNPA;
	}

    internal void RebindFormOwner(Model model)
    {
        if (model == null) throw new System.ArgumentNullException(nameof(model));
        _model = model;
    }

	private AttackStatistics GetOrCreateStatistics(bool MNJPFPLKNFA, InfoAnimation DBOLBEOCEME)
	{
		Dictionary<InfoAnimation, AttackStatistics> dictionary = ((!MNJPFPLKNFA) ? receivedStats : dealtStats);
		if (dictionary.ContainsKey(DBOLBEOCEME))
		{
			return dictionary[DBOLBEOCEME];
		}
		AttackStatistics iINOIHKEDDJ = new AttackStatistics(DBOLBEOCEME);
		dictionary.Add(DBOLBEOCEME, iINOIHKEDDJ);
		return iINOIHKEDDJ;
	}

	public void RecordDamage(bool MNJPFPLKNFA, InfoAnimation DBOLBEOCEME, float CKKFKEIELCP)
	{
		int bCAOGKPNMFG = _model.GetStrikesTaken();
		float dAIGFEOMFIE = GetStrikeHalfLife();
		AttackStatistics iINOIHKEDDJ = GetOrCreateStatistics(MNJPFPLKNFA, DBOLBEOCEME);
		iINOIHKEDDJ.AddDamage(CKKFKEIELCP, bCAOGKPNMFG, dAIGFEOMFIE);
	}

	public void CommitPendingStatistics(bool MNJPFPLKNFA, InfoAnimation DBOLBEOCEME)
	{
		AttackStatistics iINOIHKEDDJ = GetOrCreateStatistics(MNJPFPLKNFA, DBOLBEOCEME);
		iINOIHKEDDJ.CommitPending();
	}

	public void RecordUse(bool MNJPFPLKNFA, InfoAnimation DBOLBEOCEME)
	{
		int bCAOGKPNMFG = _model.GetStrikesTaken();
		float dAIGFEOMFIE = GetStrikeHalfLife();
		AttackStatistics iINOIHKEDDJ = GetOrCreateStatistics(MNJPFPLKNFA, DBOLBEOCEME);
		iINOIHKEDDJ.AddUse(bCAOGKPNMFG, dAIGFEOMFIE);
	}

	public void Reset()
	{
		dealtStats.Clear();
		receivedStats.Clear();
	}

	public void ApplyRoundFactor()
	{
		foreach (AttackStatistics value in dealtStats.Values)
		{
			value.SetCount(value.GetCount() * GetRoundFactor());
			value.SetTotalDamage(value.GetTotalDamage() * GetRoundFactor());
			value.SetPendingDamage(value.GetPendingDamage() * GetRoundFactor());
			value.SetTotalHitCount(value.GetTotalHitCount() * GetRoundFactor());
			value.ScaleAll(GetRoundFactor());
		}
		foreach (AttackStatistics value2 in receivedStats.Values)
		{
			value2.SetCount(value2.GetCount() * GetRoundFactor());
			value2.SetTotalDamage(value2.GetTotalDamage() * GetRoundFactor());
			value2.SetPendingDamage(value2.GetPendingDamage() * GetRoundFactor());
			value2.SetTotalHitCount(value2.GetTotalHitCount() * GetRoundFactor());
			value2.ScaleAll(GetRoundFactor());
		}
	}

	public void GetCountAndDamage(bool MNJPFPLKNFA, string KCAIJCBMNKP, ref float count, ref float CKKFKEIELCP, ref float JOOJIMPEPOJ)
	{
		count = 0f;
		CKKFKEIELCP = 0f;
		animationBuffer.Clear();
		AnimationData.AddTemplateAnimations(KCAIJCBMNKP, animationBuffer);
		float BLJGEOEHIGP2 = 0f;
		float CKKFKEIELCP2 = 0f;
		float JOOJIMPEPOJ2 = 0f;
		for (int i = 0; i < animationBuffer.Count; i++)
		{
			GetCountAndDamage(MNJPFPLKNFA, animationBuffer[i], ref BLJGEOEHIGP2, ref CKKFKEIELCP2, ref JOOJIMPEPOJ2);
			count += BLJGEOEHIGP2;
			CKKFKEIELCP += CKKFKEIELCP2;
			JOOJIMPEPOJ += JOOJIMPEPOJ2;
		}
	}

	public void GetCountAndDamage(bool MNJPFPLKNFA, InfoAnimation DBOLBEOCEME, ref float count, ref float CKKFKEIELCP, ref float JOOJIMPEPOJ)
	{
		AttackStatistics iINOIHKEDDJ = GetOrCreateStatistics(MNJPFPLKNFA, DBOLBEOCEME);
		int bCAOGKPNMFG = _model.GetStrikesTaken();
		float dAIGFEOMFIE = GetStrikeHalfLife();
		count = iINOIHKEDDJ.GetDecayedCount(bCAOGKPNMFG, dAIGFEOMFIE);
		CKKFKEIELCP = iINOIHKEDDJ.GetDecayedTotalDamage(bCAOGKPNMFG, dAIGFEOMFIE);
		JOOJIMPEPOJ = iINOIHKEDDJ.GetDecayedTotalHitCount(bCAOGKPNMFG, dAIGFEOMFIE);
	}

	private float GetStrikeHalfLife()
	{
		float result = 0f;
		ModelAi pCFGKAFOCDO = _model.GetAi();
		if (pCFGKAFOCDO != null)
		{
			Tactic eEJNOAKLOLG = pCFGKAFOCDO.get_Tactic();
			if (eEJNOAKLOLG != null)
			{
				result = eEJNOAKLOLG.MemoryConfig.Strikes;
			}
		}
		return result;
	}

	private float GetRoundFactor()
	{
		float result = 0f;
		ModelAi pCFGKAFOCDO = _model.GetAi();
		if (pCFGKAFOCDO != null)
		{
			Tactic eEJNOAKLOLG = pCFGKAFOCDO.get_Tactic();
			if (eEJNOAKLOLG != null)
			{
				result = eEJNOAKLOLG.MemoryConfig.RoundFactor;
			}
		}
		return result;
	}

	public void AddRaidHitInfo(bool OOCLHFGEPML, bool OOGIBOBMGJA)
	{
		if (!OOCLHFGEPML)
		{
			raidHitCount++;
		}
		if (OOGIBOBMGJA)
		{
			raidCritCount++;
		}
	}

	public bool CheckCritAvailable()
	{
		int pOJMKEEPBJK = QuestUtils.GetNoAnimationMoves().GetCritSettings().CritAdditional;
		float iMPHONCGFGP = QuestUtils.GetNoAnimationMoves().GetCritSettings().CritProbablity;
		int num = pOJMKEEPBJK + (int)((float)raidHitCount * iMPHONCGFGP);
		if (raidCritCount + 1 <= num)
		{
			return true;
		}
		return false;
	}
}
