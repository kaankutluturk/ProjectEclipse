using System.Collections.Generic;
using System.Xml;

public class Tactic
{
	public enum TacticType
	{
		TacticNone = 0,
		TacticRandom = 1,
		TacticTabular = 2
	}

	public class Memory
	{
		public float Strikes = 10f;

		public float RoundFactor = 10f;

		public Memory()
		{
		}

		public Memory(Memory MOLELAFGIPG)
		{
			Strikes = MOLELAFGIPG.Strikes;
			RoundFactor = MOLELAFGIPG.RoundFactor;
		}
	}

	private TacticType _type;

	public Memory MemoryConfig = new Memory();

	private string _name = string.Empty;

	private TacticValue _counterAttackChance = new TacticValue();

	private TacticValue _dodgeChance = new TacticValue();

	private TacticValue _blockChance = new TacticValue();

	private TacticValue _useSafeAttackChance = new TacticValue();

	private TacticValue _tableAttackChance = new TacticValue();

	private List<global::Pair<string, TacticValue>> _quickAttacks = new List<global::Pair<string, TacticValue>>();

	private List<global::Pair<string, TacticValue>> _evades = new List<global::Pair<string, TacticValue>>();

	private TacticValue _cautiousMovementsChance = new TacticValue();

	private TacticValue _dodgeMissilesChance = new TacticValue();

	private TacticValue _dodgeMagicChance = new TacticValue();

	private global::Pair<TacticValue, TacticValue> _distanceError = new global::Pair<TacticValue, TacticValue>(new TacticValue(), new TacticValue());

	private global::Pair<TacticValue, TacticValue> _frameError = new global::Pair<TacticValue, TacticValue>(new TacticValue(), new TacticValue());

	private global::Pair<TacticValue, TacticValue> _responseDelay = new global::Pair<TacticValue, TacticValue>(new TacticValue(), new TacticValue());

	private global::Pair<TacticValue, TacticValue> _enemyResponseDelay = new global::Pair<TacticValue, TacticValue>(new TacticValue(), new TacticValue());

	private List<global::Pair<string, TacticValue>> _animationWeights = new List<global::Pair<string, TacticValue>>();

	private List<global::Pair<string, TacticValue>> _expectedWaits = new List<global::Pair<string, TacticValue>>();

	public List<global::Pair<string, TacticValue>> QuickAttackList
	{
		get
		{
			return get_QuickAttacks();
		}
	}

	public List<global::Pair<string, TacticValue>> EvadeList
	{
		get
		{
			return get_Evades();
		}
	}

	public Tactic()
	{
	}

	public Tactic(XmlNode AFHNINCKJEE)
	{
		_name = AFHNINCKJEE.Attributes["Name"].GetStringOrDefault(string.Empty);
		_type = GetType(AFHNINCKJEE.Attributes["Type"].GetStringOrDefault(string.Empty));
		XmlNode xmlNode = AFHNINCKJEE["Memory"];
		if (xmlNode != null)
		{
			MemoryConfig.Strikes = xmlNode.Attributes["Strikes"].ParseFloat();
			MemoryConfig.RoundFactor = xmlNode.Attributes["RoundFactor"].ParseFloat();
		}
		XmlNode xmlNode2 = AFHNINCKJEE["UseDefense"];
		if (xmlNode2 != null)
		{
			_counterAttackChance.Parse(xmlNode2["CounterAttackChance"]);
			_dodgeChance.Parse(xmlNode2["DodgeChance"]);
			_blockChance.Parse(xmlNode2["BlockChance"]);
		}
		_useSafeAttackChance.Parse(AFHNINCKJEE["UseSafeAttackChance"]);
		_tableAttackChance.Parse(AFHNINCKJEE["TableAttackChance"]);
		ParseQuickAttacks(AFHNINCKJEE["QuickAttacks"]);
		ParseEvades(AFHNINCKJEE["Evades"]);
		_cautiousMovementsChance.Parse(AFHNINCKJEE["CautiousMovementsChance"]);
		_dodgeMissilesChance.Parse(AFHNINCKJEE["DodgeMissilesChance"]);
		_dodgeMagicChance.Parse(AFHNINCKJEE["DodgeMagicChance"]);
		ParseInterval(AFHNINCKJEE["DistanceError"], _distanceError);
		ParseInterval(AFHNINCKJEE["FrameError"], _frameError);
		ParseInterval(AFHNINCKJEE["ResponseDelay"], _responseDelay);
		ParseInterval(AFHNINCKJEE["EnemyResponseDelay"], _enemyResponseDelay);
		XmlNode xmlNode3 = AFHNINCKJEE["AnimationWeights"];
		if (xmlNode3 != null)
		{
			foreach (XmlNode childNode in xmlNode3.ChildNodes)
			{
				if (childNode.Name == "Animation")
				{
					string gBCLEDJAOBM = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
					TacticValue pOFHDGJAFMP = new TacticValue(childNode);
					_animationWeights.Add(new global::Pair<string, TacticValue>(gBCLEDJAOBM, pOFHDGJAFMP));
				}
			}
		}
		XmlNode xmlNode5 = AFHNINCKJEE["ExpectedWait"];
		if (xmlNode5 == null)
		{
			return;
		}
		foreach (XmlNode childNode2 in xmlNode5.ChildNodes)
		{
			if (childNode2.Name == "Animation")
			{
				string gBCLEDJAOBM2 = childNode2.Attributes["Name"].GetStringOrDefault(string.Empty);
				TacticValue pOFHDGJAFMP2 = new TacticValue(childNode2);
				_expectedWaits.Add(new global::Pair<string, TacticValue>(gBCLEDJAOBM2, pOFHDGJAFMP2));
			}
		}
	}

	public Tactic(Tactic BJBIGPGJKIE)
	{
		_name = BJBIGPGJKIE._name;
		_type = BJBIGPGJKIE._type;
		_counterAttackChance = BJBIGPGJKIE._counterAttackChance;
		_dodgeChance = BJBIGPGJKIE._dodgeChance;
		_blockChance = BJBIGPGJKIE._blockChance;
		_useSafeAttackChance = BJBIGPGJKIE._useSafeAttackChance;
		_tableAttackChance = BJBIGPGJKIE._tableAttackChance;
		_cautiousMovementsChance = BJBIGPGJKIE._cautiousMovementsChance;
		_dodgeMissilesChance = BJBIGPGJKIE._dodgeMissilesChance;
		_dodgeMagicChance = BJBIGPGJKIE._dodgeMagicChance;
		_distanceError = BJBIGPGJKIE._distanceError;
		_frameError = BJBIGPGJKIE._frameError;
		_responseDelay = BJBIGPGJKIE._responseDelay;
		_enemyResponseDelay = BJBIGPGJKIE._enemyResponseDelay;
		_animationWeights = BJBIGPGJKIE._animationWeights;
		_expectedWaits = BJBIGPGJKIE._expectedWaits;
		_quickAttacks = BJBIGPGJKIE._quickAttacks;
		_evades = BJBIGPGJKIE._evades;
		MemoryConfig = BJBIGPGJKIE.MemoryConfig;
	}

	public TacticType get_Type()
	{
		return _type;
	}

	public string get_Name()
	{
		return _name;
	}

	public List<global::Pair<string, TacticValue>> get_QuickAttacks()
	{
		return _quickAttacks;
	}

	public List<global::Pair<string, TacticValue>> get_Evades()
	{
		return _evades;
	}

	public float GetCounterAttackChance(TacticFactors FJCBLOKOBBD)
	{
		return _counterAttackChance.GetValue(FJCBLOKOBBD);
	}

	public float GetDodgeChance(TacticFactors FJCBLOKOBBD)
	{
		return _dodgeChance.GetValue(FJCBLOKOBBD);
	}

	public float GetBlockChance(TacticFactors FJCBLOKOBBD)
	{
		return _blockChance.GetValue(FJCBLOKOBBD);
	}

	public float GetUseSafeAttackChance(TacticFactors FJCBLOKOBBD)
	{
		return _useSafeAttackChance.GetValue(FJCBLOKOBBD);
	}

	public float GetTableAttackChance(TacticFactors FJCBLOKOBBD)
	{
		return _tableAttackChance.GetValue(FJCBLOKOBBD);
	}

	public float GetCautiousMovementsChance(TacticFactors FJCBLOKOBBD)
	{
		return _cautiousMovementsChance.GetValue(FJCBLOKOBBD);
	}

	public float GetDodgeMissileChance(TacticFactors FJCBLOKOBBD)
	{
		return _dodgeMissilesChance.GetValue(FJCBLOKOBBD);
	}

	public float GetDodgeMagicChance(TacticFactors FJCBLOKOBBD)
	{
		return _dodgeMagicChance.GetValue(FJCBLOKOBBD);
	}

	public float GetExpectedWait(InfoAnimation DBOLBEOCEME, TacticFactors FJCBLOKOBBD)
	{
		if (DBOLBEOCEME != null)
		{
			foreach (global::Pair<string, TacticValue> item in _expectedWaits)
			{
				if (string.IsNullOrEmpty(item.First) || DBOLBEOCEME.HasName(item.First))
				{
					return item.Second.GetValue(FJCBLOKOBBD);
				}
			}
		}
		else
		{
			foreach (global::Pair<string, TacticValue> item2 in _expectedWaits)
			{
				if (string.IsNullOrEmpty(item2.First))
				{
					return item2.Second.GetValue(FJCBLOKOBBD);
				}
			}
		}
		GameLog.Error("Expected Wait ERROR");
		return 1f;
	}

	public int SelectAnimationWithWeights(List<InfoAnimation> MAHEJFLCCHP, InfoAnimation HNCCGJECKLL, TacticFactors FJCBLOKOBBD)
	{
		int count = MAHEJFLCCHP.Count;
		if (0 < count)
		{
			float num = 0f;
			for (int i = 0; i < MAHEJFLCCHP.Count; i++)
			{
				InfoAnimation pJAHIOELGGD = MAHEJFLCCHP[i];
				if (pJAHIOELGGD == null && HNCCGJECKLL != null)
				{
					pJAHIOELGGD = HNCCGJECKLL;
				}
				if (pJAHIOELGGD != null)
				{
					float num2 = GetWeight(pJAHIOELGGD, FJCBLOKOBBD);
					num += num2;
				}
			}
			if (0f < num)
			{
				float num3 = NekkiMath.randomFloat(num);
				int num4 = 0;
				for (int j = 0; j < MAHEJFLCCHP.Count; j++)
				{
					InfoAnimation pJAHIOELGGD2 = MAHEJFLCCHP[j];
					if (pJAHIOELGGD2 == null && HNCCGJECKLL != null)
					{
						pJAHIOELGGD2 = HNCCGJECKLL;
					}
					if (pJAHIOELGGD2 != null)
					{
						float num5 = GetWeight(pJAHIOELGGD2, FJCBLOKOBBD);
						float num6 = num3 - num5;
						if (num6 < 0f)
						{
							return num4;
						}
						num3 = num6;
						num4++;
					}
				}
			}
		}
		return -1;
	}

	public float GetWeight(InfoAnimation DBOLBEOCEME, TacticFactors JCICKLIMBEF)
	{
		foreach (global::Pair<string, TacticValue> item in _animationWeights)
		{
			string lLHEDBIEHAA = item.First;
			if (lLHEDBIEHAA == string.Empty || DBOLBEOCEME.HasName(lLHEDBIEHAA))
			{
				return item.Second.GetValue(JCICKLIMBEF);
			}
		}
		return 0f;
	}

	public float GetDistanceError(TacticFactors FJCBLOKOBBD)
	{
		float lHNCHOAEGEA = _distanceError.First.GetValue(FJCBLOKOBBD);
		float kAEPJHHLLPK = _distanceError.Second.GetValue(FJCBLOKOBBD);
		return GetValueFromInterval(lHNCHOAEGEA, kAEPJHHLLPK);
	}

	public int GetFrameError(TacticFactors FJCBLOKOBBD)
	{
		float lHNCHOAEGEA = _frameError.First.GetValue(FJCBLOKOBBD);
		float kAEPJHHLLPK = _frameError.Second.GetValue(FJCBLOKOBBD);
		return (int)GetValueFromInterval(lHNCHOAEGEA, kAEPJHHLLPK);
	}

	public int GetResponseDelay(TacticFactors FJCBLOKOBBD)
	{
		float lHNCHOAEGEA = _responseDelay.First.GetValue(FJCBLOKOBBD);
		float kAEPJHHLLPK = _responseDelay.Second.GetValue(FJCBLOKOBBD);
		return (int)GetValueFromInterval(lHNCHOAEGEA, kAEPJHHLLPK);
	}

	public int GetEnemyResponseDelay(TacticFactors FJCBLOKOBBD)
	{
		float lHNCHOAEGEA = _enemyResponseDelay.First.GetValue(FJCBLOKOBBD);
		float kAEPJHHLLPK = _enemyResponseDelay.Second.GetValue(FJCBLOKOBBD);
		return (int)GetValueFromInterval(lHNCHOAEGEA, kAEPJHHLLPK);
	}

	private void ParseQuickAttacks(XmlNode AFHNINCKJEE)
	{
		int num = _quickAttacks.Count;
		foreach (XmlNode childNode in AFHNINCKJEE.ChildNodes)
		{
			if (childNode.Name == "QuickAttackChance")
			{
				string lLHEDBIEHAA = childNode.Attributes["Animation"].GetStringOrDefault(string.Empty);
				_quickAttacks.Add(new global::Pair<string, TacticValue>(string.Empty, new TacticValue()));
				_quickAttacks[num].First = lLHEDBIEHAA;
				_quickAttacks[num].Second.Parse(childNode);
				num++;
			}
		}
	}

	private void ParseEvades(XmlNode AFHNINCKJEE)
	{
		int num = _evades.Count;
		foreach (XmlNode childNode in AFHNINCKJEE.ChildNodes)
		{
			if (childNode.Name == "EvadeChance")
			{
				string lLHEDBIEHAA = childNode.Attributes["Animation"].GetStringOrDefault(string.Empty);
				_evades.Add(new global::Pair<string, TacticValue>(string.Empty, new TacticValue()));
				_evades[num].First = lLHEDBIEHAA;
				_evades[num].Second.Parse(childNode);
				num++;
			}
		}
	}

	private static void ParseInterval(XmlNode AFHNINCKJEE, global::Pair<TacticValue, TacticValue> CHCGJBLDPML)
	{
		if (AFHNINCKJEE != null)
		{
			CHCGJBLDPML.First.Parse(AFHNINCKJEE["Min"]);
			CHCGJBLDPML.Second.Parse(AFHNINCKJEE["Max"]);
		}
	}

	private static TacticType GetType(string CNKBLODAFDO)
	{
		if (CNKBLODAFDO == "Random")
		{
			return TacticType.TacticRandom;
		}
		if (CNKBLODAFDO == "Tabular")
		{
			return TacticType.TacticTabular;
		}
		GameLog.Error("Strange tactic type: %s", CNKBLODAFDO);
		return TacticType.TacticNone;
	}

	private static float GetValueFromInterval(float LHNCHOAEGEA, float KAEPJHHLLPK)
	{
		float num = NekkiMath.randomFloat() * (KAEPJHHLLPK - LHNCHOAEGEA);
		return LHNCHOAEGEA + num;
	}
}
