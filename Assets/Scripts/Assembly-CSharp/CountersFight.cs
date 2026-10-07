using System.Collections.Generic;
using Nekki.SF2.GUI.Map;

public class CountersFight : global::EventDispatcher<object>
{
	public enum CounterEvent
	{
		OnCounterIncrement = 0
	}

	public class CurrentCounter
	{
		public Counter Definition;

		public int Value;

		public bool IsNot;

		public void Increment()
		{
			Value++;
		}
	}

	public class CounterStorage
	{
		public Dictionary<string, CurrentCounter> CountersByName = new Dictionary<string, CurrentCounter>();
	}

	private bool isFirstBlock;

	private ModelParameters modelParameters;

	private Dictionary<string, Counter> counterDefinitions = new Dictionary<string, Counter>();

	private CounterStorage currentCounters = new CounterStorage();

	private CounterConditions conditions = new CounterConditions();

	public void Init(Dictionary<string, Counter> definitions, ModelParameters parameters, BattleType battleType, float ratio)
	{
		conditions.BattleType = battleType;
		conditions.Ratio = ratio;
		counterDefinitions = definitions;
		modelParameters = parameters;
		isFirstBlock = false;
		foreach (KeyValuePair<string, Counter> item in counterDefinitions)
		{
			CurrentCounter currentCounter = new CurrentCounter();
			currentCounter.Definition = item.Value;
			currentCounter.Value = 0;
			currentCounters.CountersByName[item.Key] = currentCounter;
		}
	}

	public void LogCounters()
	{
		GameLog.Info("Counters ------------------------------------- ");
		foreach (KeyValuePair<string, CurrentCounter> item in currentCounters.CountersByName)
		{
			GameLog.Info("Counter: {0} -- Value: {1})", item.Key, item.Value.Value);
		}
	}

	public void OnNoLose()
	{
		CallCountersByType("NoLose");
	}

	public void OnBossNoLose()
	{
		CallCountersByType("BossNoLose");
	}

	public void OnEnchantments()
	{
		CallCountersByType("Enchantments");
	}

	public void OnPerfectRound()
	{
		CallCountersByType("PerfectRound");
	}

	public void OnLoss()
	{
		CallCountersByType("Losses");
	}

	public void OnShockWin()
	{
		CallCountersByType("ShockWin");
	}

	public void OnBodyguardsWin()
	{
		CallCountersByType("BodyguardsWin");
	}

	public void OnBossWin()
	{
		CallCountersByType("BossWin");
	}

	public void OnTournamentBeaten()
	{
		CallCountersByType("TournamentsBeaten");
	}

	public void OnDailyBeaten()
	{
		CallCountersByType("DailyBeaten");
	}

	public void OnChallengeBeaten()
	{
		CallCountersByType("ChallangesBeaten");
	}

	public void OnChallenge2Beaten()
	{
		CallCountersByType("Challanges2Beaten");
	}

	public void OnMaximumLevel()
	{
		CallCountersByType("MaximumLevel");
	}

	public void OnDifficultyWin()
	{
		CallCountersByType("DifficultyWin");
	}

	public void OnWinBattle(FightIDS fightId)
	{
		List<CurrentCounter> list = GetCountersByType("WinBattle");
		foreach (CurrentCounter item in list)
		{
			if (item.Definition.IsFightComplete(fightId))
			{
				IncrementCounter(item);
			}
		}
	}

	public void SetComboCount(int comboCount)
	{
		List<CurrentCounter> list = GetCountersByType("ComboCount");
		foreach (CurrentCounter item in list)
		{
			if ((float)comboCount >= item.Definition.Value)
			{
				IncrementCounter(item);
			}
		}
	}

	public void SetStyle(int style)
	{
		List<CurrentCounter> list = GetCountersByType("Style");
		foreach (CurrentCounter item in list)
		{
			if ((float)style >= item.Definition.Value)
			{
				IncrementCounter(item);
			}
		}
	}

	public void OnFightBeaten(FightIDS fightId)
	{
		List<CurrentCounter> list = GetCountersByType("FightBeaten");
		foreach (CurrentCounter item in list)
		{
			if (fightId.Equals(item.Definition.FightName) || fightId.Equals(item.Definition.SecondFightName))
			{
				IncrementCounter(item);
			}
		}
	}

	public void SetSurvivalRounds(int value)
	{
		List<CurrentCounter> list = GetCountersByType("SurvivalRounds");
		foreach (CurrentCounter item in list)
		{
			if ((float)value >= item.Definition.Value)
			{
				IncrementCounter(item);
			}
		}
	}

	public void SetLife(float value)
	{
		List<CurrentCounter> list = GetCountersByType("HealthRemained");
		foreach (CurrentCounter item in list)
		{
			if (value <= item.Definition.Value)
			{
				IncrementCounter(item);
			}
		}
	}

	public void SetTime(int value)
	{
		List<CurrentCounter> list = GetCountersByType("RoundQuicker");
		foreach (CurrentCounter item in list)
		{
			if ((float)value <= item.Definition.Value)
			{
				IncrementCounter(item);
			}
		}
		List<CurrentCounter> list2 = GetCountersByType("RoundLonger");
		foreach (CurrentCounter item2 in list2)
		{
			if ((float)value >= item2.Definition.Value)
			{
				IncrementCounter(item2);
			}
		}
	}

	public void OnBlock(bool isBlocked)
	{
		isFirstBlock = true;
		CurrentCounter blockedCounter = GetCounterByName("BlockedRound");
		if (!isBlocked && blockedCounter != null && !blockedCounter.IsNot)
		{
			blockedCounter.IsNot = true;
		}
	}

	public void OnAnimationHit(InfoAnimation animationInfo, bool isHeadHit, bool isFirstStrike, bool isDisarm, bool isTargetDead, bool isBlocked, bool isShock)
	{
		if (!isBlocked && isFirstStrike)
		{
			List<CurrentCounter> list = GetCountersByType("FirstHits");
			foreach (CurrentCounter item in list)
			{
				IncrementCounter(item);
			}
		}
		if (isDisarm)
		{
			List<CurrentCounter> list2 = GetCountersByType("Disarm");
			foreach (CurrentCounter item2 in list2)
			{
				IncrementCounter(item2);
			}
		}
		List<CurrentCounter> list3 = GetCountersByType("RestrictedAnimation");
		foreach (CurrentCounter item3 in list3)
		{
			if (!item3.IsNot && !animationInfo.HasTemplateName(item3.Definition.AnimationName))
			{
				item3.IsNot = true;
			}
			string weaponName = item3.Definition.WeaponName;
			if (modelParameters.Weapon != null && weaponName != string.Empty && modelParameters.Weapon.Name != weaponName)
			{
				item3.IsNot = true;
			}
			if (isTargetDead && !item3.IsNot)
			{
				IncrementCounter(item3);
			}
		}
		if (isTargetDead)
		{
			if (isHeadHit)
			{
				List<CurrentCounter> list4 = GetCountersByType("HeadHitRound");
				foreach (CurrentCounter item4 in list4)
				{
					IncrementCounter(item4);
				}
			}
			CurrentCounter blockedCounter = GetCounterByName("BlockedRound");
			if (blockedCounter != null && !blockedCounter.IsNot && isFirstBlock)
			{
				IncrementCounter(blockedCounter);
			}
		}
		if (isBlocked || !isHeadHit)
		{
			return;
		}
		List<CurrentCounter> list5 = GetCountersByType("HeadKick");
		foreach (CurrentCounter item5 in list5)
		{
			if (item5 != null && animationInfo.HasTemplateName(item5.Definition.AnimationName))
			{
				IncrementCounter(item5);
			}
		}
	}

	public void CallCountersByType(string counterType)
	{
		List<CurrentCounter> list = GetCountersByType(counterType);
		foreach (CurrentCounter item in list)
		{
			IncrementCounter(item);
		}
	}

	public void Complete(int roundTotal, bool skipFightEndCounters = false)
	{
		if (skipFightEndCounters)
		{
			return;
		}
		foreach (KeyValuePair<string, CurrentCounter> item in currentCounters.CountersByName)
		{
			CurrentCounter value = item.Value;
			if (value.Definition.AreConditionsMet(conditions) && value.Definition.Span == Counter.CounterSpan.SPAN_FIGHT)
			{
				value.Value = ((value.Value == roundTotal) ? 1 : 0);
				CallEvent(0, value);
			}
		}
	}

	public void SaveCompleteValues(bool skipFightEndCounters)
	{
		foreach (KeyValuePair<string, Counter> item in counterDefinitions)
		{
			if (!skipFightEndCounters || !item.Value.IsFightEnd)
			{
				item.Value.CompleteValue = currentCounters.CountersByName[item.Key].Value;
			}
		}
		LogCounters();
	}

	public void ResetRound()
	{
		isFirstBlock = false;
		foreach (KeyValuePair<string, CurrentCounter> item in currentCounters.CountersByName)
		{
			CurrentCounter value = item.Value;
			if (value.Definition.Span == Counter.CounterSpan.SPAN_ROUND)
			{
				value.IsNot = false;
			}
		}
	}

	private void IncrementCounter(CurrentCounter counter)
	{
		bool flag = CheckFightType(counter.Definition.FightType);
		bool flag2 = CheckDifficult(counter.Definition.MinDifficulty, counter.Definition.MaxDifficulty);
		if (counter.Definition.AreConditionsMet(conditions) && flag && flag2)
		{
			counter.Increment();
			if (counter.Definition.Span != Counter.CounterSpan.SPAN_FIGHT)
			{
				CallEvent(0, counter);
			}
		}
	}

	private List<CurrentCounter> GetCountersByType(string counterType)
	{
		List<CurrentCounter> list = new List<CurrentCounter>();
		foreach (KeyValuePair<string, CurrentCounter> item in currentCounters.CountersByName)
		{
			if (item.Value.Definition.Type == counterType)
			{
				list.Add(item.Value);
			}
		}
		return list;
	}

	private CurrentCounter GetCounterByName(string name)
	{
		if (currentCounters.CountersByName.ContainsKey(name))
		{
			return currentCounters.CountersByName[name];
		}
		return null;
	}

	private bool CheckFightType(string fightTypes)
	{
		if (fightTypes == null || fightTypes == string.Empty)
		{
			return true;
		}
		string[] array = fightTypes.Split('|');
		for (int i = 0; i < array.Length; i++)
		{
			BattleType battleType = ListSF.GetInstance().GetBattleTypeByName(array[i]);
			if (battleType == conditions.BattleType)
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckDifficult(string minDifficulty, string maxDifficulty)
	{
		if (minDifficulty == string.Empty && maxDifficulty == string.Empty)
		{
			return true;
		}
		bool flag = minDifficulty != string.Empty;
		bool flag2 = maxDifficulty != string.Empty;
		float num = GetRationForDifficult(minDifficulty);
		float num2 = GetRationForDifficult(maxDifficulty);
		if (flag && flag2)
		{
			return conditions.Ratio > num && conditions.Ratio < num2;
		}
		if (flag)
		{
			return conditions.Ratio > num;
		}
		if (flag2)
		{
			return conditions.Ratio < num2;
		}
		return false;
	}

	private float GetRationForDifficult(string difficultyName)
	{
		List<global::Pair<string, float>> difficultyEvaluation = DifficultyPanel.get_DifficultyEvaluation();
		for (int i = 0; i < difficultyEvaluation.Count; i++)
		{
			global::Pair<string, float> evaluation = difficultyEvaluation[i];
			if (evaluation.First == difficultyName)
			{
				return evaluation.Second;
			}
		}
		return difficultyName.ToFloat();
	}
}
