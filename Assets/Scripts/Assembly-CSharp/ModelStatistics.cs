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

		public AttackStatistics(InfoAnimation attackAnimation)
		{
			animation = attackAnimation;
			totalDamage = 0f;
			pendingDamage = 0f;
			_count = 0f;
			totalHitCount = 0f;
			pendingHitCount = 0f;
			_strikeIndex = 0;
		}

		public AttackStatistics(AttackStatistics source)
		{
			animation = source.animation;
			totalDamage = source.totalDamage;
			pendingDamage = source.pendingDamage;
			_count = source._count;
			totalHitCount = source.totalHitCount;
			pendingHitCount = source.pendingHitCount;
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

		private void ApplyDecay(int strikeIndex, float halfLife)
		{
			int num = strikeIndex - _strikeIndex;
			if (0 < num)
			{
				float num2 = Mathf.Pow(2f, (0f - (float)num) / halfLife);
				totalDamage *= num2;
				pendingDamage *= num2;
				_count *= num2;
				totalHitCount *= num2;
				pendingHitCount *= num2;
			}
			_strikeIndex = strikeIndex;
		}

		public void AddDamage(float damage, int strikeIndex, float halfLife)
		{
			ApplyDecay(strikeIndex, halfLife);
			pendingDamage += damage;
			pendingHitCount++;
		}

		public void CommitPending()
		{
			totalDamage += pendingDamage;
			pendingDamage = 0f;
			totalHitCount += pendingHitCount;
			pendingHitCount = 0f;
		}

		public void AddUse(int strikeIndex, float halfLife)
		{
			ApplyDecay(strikeIndex, halfLife);
			_count++;
		}

		public float GetDecayedTotalDamage(int strikeIndex, float halfLife)
		{
			ApplyDecay(strikeIndex, halfLife);
			return totalDamage;
		}

		public float GetDecayedCount(int strikeIndex, float halfLife)
		{
			ApplyDecay(strikeIndex, halfLife);
			return _count;
		}

		public float GetDecayedTotalHitCount(int strikeIndex, float halfLife)
		{
			ApplyDecay(strikeIndex, halfLife);
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

	public ModelStatistics(Model model)
	{
		_model = model;
	}

    internal void RebindFormOwner(Model model)
    {
        if (model == null) throw new System.ArgumentNullException(nameof(model));
        _model = model;
    }

	private AttackStatistics GetOrCreateStatistics(bool isDealt, InfoAnimation animation)
	{
		Dictionary<InfoAnimation, AttackStatistics> dictionary = ((!isDealt) ? receivedStats : dealtStats);
		if (dictionary.ContainsKey(animation))
		{
			return dictionary[animation];
		}
		AttackStatistics attackStatistics = new AttackStatistics(animation);
		dictionary.Add(animation, attackStatistics);
		return attackStatistics;
	}

	public void RecordDamage(bool isDealt, InfoAnimation animation, float damage)
	{
		int strikeIndex = _model.GetStrikesTaken();
		float halfLife = GetStrikeHalfLife();
		AttackStatistics attackStatistics = GetOrCreateStatistics(isDealt, animation);
		attackStatistics.AddDamage(damage, strikeIndex, halfLife);
	}

	public void CommitPendingStatistics(bool isDealt, InfoAnimation animation)
	{
		AttackStatistics attackStatistics = GetOrCreateStatistics(isDealt, animation);
		attackStatistics.CommitPending();
	}

	public void RecordUse(bool isDealt, InfoAnimation animation)
	{
		int strikeIndex = _model.GetStrikesTaken();
		float halfLife = GetStrikeHalfLife();
		AttackStatistics attackStatistics = GetOrCreateStatistics(isDealt, animation);
		attackStatistics.AddUse(strikeIndex, halfLife);
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

	public void GetCountAndDamage(bool isDealt, string templateName, ref float count, ref float damage, ref float hitCount)
	{
		count = 0f;
		damage = 0f;
		animationBuffer.Clear();
		AnimationData.AddTemplateAnimations(templateName, animationBuffer);
		float BLJGEOEHIGP2 = 0f;
		float CKKFKEIELCP2 = 0f;
		float JOOJIMPEPOJ2 = 0f;
		for (int i = 0; i < animationBuffer.Count; i++)
		{
			GetCountAndDamage(isDealt, animationBuffer[i], ref BLJGEOEHIGP2, ref CKKFKEIELCP2, ref JOOJIMPEPOJ2);
			count += BLJGEOEHIGP2;
			damage += CKKFKEIELCP2;
			hitCount += JOOJIMPEPOJ2;
		}
	}

	public void GetCountAndDamage(bool isDealt, InfoAnimation animation, ref float count, ref float damage, ref float hitCount)
	{
		AttackStatistics attackStatistics = GetOrCreateStatistics(isDealt, animation);
		int strikeIndex = _model.GetStrikesTaken();
		float halfLife = GetStrikeHalfLife();
		count = attackStatistics.GetDecayedCount(strikeIndex, halfLife);
		damage = attackStatistics.GetDecayedTotalDamage(strikeIndex, halfLife);
		hitCount = attackStatistics.GetDecayedTotalHitCount(strikeIndex, halfLife);
	}

	private float GetStrikeHalfLife()
	{
		float result = 0f;
		ModelAi modelAi = _model.GetAi();
		if (modelAi != null)
		{
			Tactic tactic = modelAi.get_Tactic();
			if (tactic != null)
			{
				result = tactic.MemoryConfig.Strikes;
			}
		}
		return result;
	}

	private float GetRoundFactor()
	{
		float result = 0f;
		ModelAi modelAi = _model.GetAi();
		if (modelAi != null)
		{
			Tactic tactic = modelAi.get_Tactic();
			if (tactic != null)
			{
				result = tactic.MemoryConfig.RoundFactor;
			}
		}
		return result;
	}

	public void AddRaidHitInfo(bool isBlocked, bool isCritical)
	{
		if (!isBlocked)
		{
			raidHitCount++;
		}
		if (isCritical)
		{
			raidCritCount++;
		}
	}

	public bool CheckCritAvailable()
	{
		int critAdditional = QuestUtils.GetNoAnimationMoves().GetCritSettings().CritAdditional;
		float critProbability = QuestUtils.GetNoAnimationMoves().GetCritSettings().CritProbablity;
		int num = critAdditional + (int)((float)raidHitCount * critProbability);
		if (raidCritCount + 1 <= num)
		{
			return true;
		}
		return false;
	}
}
