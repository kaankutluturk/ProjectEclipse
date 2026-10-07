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

	public void Init(Dictionary<string, Counter> GGOFNBMGJAF, ModelParameters KKNOCIPBIIK, BattleType JBJHPJMJNNF, float ratio)
	{
		conditions.BattleType = JBJHPJMJNNF;
		conditions.Ratio = ratio;
		counterDefinitions = GGOFNBMGJAF;
		modelParameters = KKNOCIPBIIK;
		isFirstBlock = false;
		foreach (KeyValuePair<string, Counter> item in counterDefinitions)
		{
			CurrentCounter pEMLBKDIDHA = new CurrentCounter();
			pEMLBKDIDHA.Definition = item.Value;
			pEMLBKDIDHA.Value = 0;
			currentCounters.CountersByName[item.Key] = pEMLBKDIDHA;
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

	public void OnWinBattle(FightIDS DIAIIPCBMFL)
	{
		List<CurrentCounter> list = GetCountersByType("WinBattle");
		foreach (CurrentCounter item in list)
		{
			if (item.Definition.IsFightComplete(DIAIIPCBMFL))
			{
				IncrementCounter(item);
			}
		}
	}

	public void SetComboCount(int PKHDLOGJKAD)
	{
		List<CurrentCounter> list = GetCountersByType("ComboCount");
		foreach (CurrentCounter item in list)
		{
			if ((float)PKHDLOGJKAD >= item.Definition.Value)
			{
				IncrementCounter(item);
			}
		}
	}

	public void SetStyle(int PKHDLOGJKAD)
	{
		List<CurrentCounter> list = GetCountersByType("Style");
		foreach (CurrentCounter item in list)
		{
			if ((float)PKHDLOGJKAD >= item.Definition.Value)
			{
				IncrementCounter(item);
			}
		}
	}

	public void OnFightBeaten(FightIDS DIAIIPCBMFL)
	{
		List<CurrentCounter> list = GetCountersByType("FightBeaten");
		foreach (CurrentCounter item in list)
		{
			if (DIAIIPCBMFL.Equals(item.Definition.FightName) || DIAIIPCBMFL.Equals(item.Definition.SecondFightName))
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

	public void OnBlock(bool OOCLHFGEPML)
	{
		isFirstBlock = true;
		CurrentCounter pEMLBKDIDHA = GetCounterByName("BlockedRound");
		if (!OOCLHFGEPML && pEMLBKDIDHA != null && !pEMLBKDIDHA.IsNot)
		{
			pEMLBKDIDHA.IsNot = true;
		}
	}

	public void OnAnimationHit(InfoAnimation DBOLBEOCEME, bool APLJLFHDJIM, bool isFirstStrike, bool INDFLCGLJPP, bool LGNDOAHHHNP, bool OOCLHFGEPML, bool EPKEEMFHHFM)
	{
		if (!OOCLHFGEPML && isFirstStrike)
		{
			List<CurrentCounter> list = GetCountersByType("FirstHits");
			foreach (CurrentCounter item in list)
			{
				IncrementCounter(item);
			}
		}
		if (INDFLCGLJPP)
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
			if (!item3.IsNot && !DBOLBEOCEME.HasTemplateName(item3.Definition.AnimationName))
			{
				item3.IsNot = true;
			}
			string jIIFFJAJNNN = item3.Definition.WeaponName;
			if (modelParameters.Weapon != null && jIIFFJAJNNN != string.Empty && modelParameters.Weapon.Name != jIIFFJAJNNN)
			{
				item3.IsNot = true;
			}
			if (LGNDOAHHHNP && !item3.IsNot)
			{
				IncrementCounter(item3);
			}
		}
		if (LGNDOAHHHNP)
		{
			if (APLJLFHDJIM)
			{
				List<CurrentCounter> list4 = GetCountersByType("HeadHitRound");
				foreach (CurrentCounter item4 in list4)
				{
					IncrementCounter(item4);
				}
			}
			CurrentCounter pEMLBKDIDHA = GetCounterByName("BlockedRound");
			if (pEMLBKDIDHA != null && !pEMLBKDIDHA.IsNot && isFirstBlock)
			{
				IncrementCounter(pEMLBKDIDHA);
			}
		}
		if (OOCLHFGEPML || !APLJLFHDJIM)
		{
			return;
		}
		List<CurrentCounter> list5 = GetCountersByType("HeadKick");
		foreach (CurrentCounter item5 in list5)
		{
			if (item5 != null && DBOLBEOCEME.HasTemplateName(item5.Definition.AnimationName))
			{
				IncrementCounter(item5);
			}
		}
	}

	public void CallCountersByType(string KFLJDKNOPCE)
	{
		List<CurrentCounter> list = GetCountersByType(KFLJDKNOPCE);
		foreach (CurrentCounter item in list)
		{
			IncrementCounter(item);
		}
	}

	public void Complete(int roundTotal, bool CDCEOCEPMPK = false)
	{
		if (CDCEOCEPMPK)
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

	public void SaveCompleteValues(bool CDCEOCEPMPK)
	{
		foreach (KeyValuePair<string, Counter> item in counterDefinitions)
		{
			if (!CDCEOCEPMPK || !item.Value.IsFightEnd)
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

	private void IncrementCounter(CurrentCounter EPJGLECOIBG)
	{
		bool flag = CheckFightType(EPJGLECOIBG.Definition.FightType);
		bool flag2 = CheckDifficult(EPJGLECOIBG.Definition.MinDifficulty, EPJGLECOIBG.Definition.MaxDifficulty);
		if (EPJGLECOIBG.Definition.AreConditionsMet(conditions) && flag && flag2)
		{
			EPJGLECOIBG.Increment();
			if (EPJGLECOIBG.Definition.Span != Counter.CounterSpan.SPAN_FIGHT)
			{
				CallEvent(0, EPJGLECOIBG);
			}
		}
	}

	private List<CurrentCounter> GetCountersByType(string LFLGCDNKNJI)
	{
		List<CurrentCounter> list = new List<CurrentCounter>();
		foreach (KeyValuePair<string, CurrentCounter> item in currentCounters.CountersByName)
		{
			if (item.Value.Definition.Type == LFLGCDNKNJI)
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

	private bool CheckFightType(string MPBIEONNLIJ)
	{
		if (MPBIEONNLIJ == null || MPBIEONNLIJ == string.Empty)
		{
			return true;
		}
		string[] array = MPBIEONNLIJ.Split('|');
		for (int i = 0; i < array.Length; i++)
		{
			BattleType pJMEMGHKKBM = ListSF.GetInstance().GetBattleTypeByName(array[i]);
			if (pJMEMGHKKBM == conditions.BattleType)
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckDifficult(string HBACJNBIFCH, string EBGLLCMNIED)
	{
		if (HBACJNBIFCH == string.Empty && EBGLLCMNIED == string.Empty)
		{
			return true;
		}
		bool flag = HBACJNBIFCH != string.Empty;
		bool flag2 = EBGLLCMNIED != string.Empty;
		float num = GetRationForDifficult(HBACJNBIFCH);
		float num2 = GetRationForDifficult(EBGLLCMNIED);
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

	private float GetRationForDifficult(string DOACAKFFFPB)
	{
		List<global::Pair<string, float>> difficultyEvaluation = DifficultyPanel.get_DifficultyEvaluation();
		for (int i = 0; i < difficultyEvaluation.Count; i++)
		{
			global::Pair<string, float> cCKLNOPEKHO = difficultyEvaluation[i];
			if (cCKLNOPEKHO.First == DOACAKFFFPB)
			{
				return cCKLNOPEKHO.Second;
			}
		}
		return DOACAKFFFPB.ToFloat();
	}
}
