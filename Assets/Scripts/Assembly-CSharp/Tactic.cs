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

		public Memory(Memory source)
		{
			Strikes = source.Strikes;
			RoundFactor = source.RoundFactor;
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

	public Tactic(XmlNode node)
	{
		_name = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		_type = GetType(node.Attributes["Type"].GetStringOrDefault(string.Empty));
		XmlNode xmlNode = node["Memory"];
		if (xmlNode != null)
		{
			MemoryConfig.Strikes = xmlNode.Attributes["Strikes"].ParseFloat();
			MemoryConfig.RoundFactor = xmlNode.Attributes["RoundFactor"].ParseFloat();
		}
		XmlNode xmlNode2 = node["UseDefense"];
		if (xmlNode2 != null)
		{
			_counterAttackChance.Parse(xmlNode2["CounterAttackChance"]);
			_dodgeChance.Parse(xmlNode2["DodgeChance"]);
			_blockChance.Parse(xmlNode2["BlockChance"]);
		}
		_useSafeAttackChance.Parse(node["UseSafeAttackChance"]);
		_tableAttackChance.Parse(node["TableAttackChance"]);
		ParseQuickAttacks(node["QuickAttacks"]);
		ParseEvades(node["Evades"]);
		_cautiousMovementsChance.Parse(node["CautiousMovementsChance"]);
		_dodgeMissilesChance.Parse(node["DodgeMissilesChance"]);
		_dodgeMagicChance.Parse(node["DodgeMagicChance"]);
		ParseInterval(node["DistanceError"], _distanceError);
		ParseInterval(node["FrameError"], _frameError);
		ParseInterval(node["ResponseDelay"], _responseDelay);
		ParseInterval(node["EnemyResponseDelay"], _enemyResponseDelay);
		XmlNode xmlNode3 = node["AnimationWeights"];
		if (xmlNode3 != null)
		{
			foreach (XmlNode childNode in xmlNode3.ChildNodes)
			{
				if (childNode.Name == "Animation")
				{
					string animationName = childNode.Attributes["Name"].GetStringOrDefault(string.Empty);
					TacticValue tacticValue = new TacticValue(childNode);
					_animationWeights.Add(new global::Pair<string, TacticValue>(animationName, tacticValue));
				}
			}
		}
		XmlNode xmlNode5 = node["ExpectedWait"];
		if (xmlNode5 == null)
		{
			return;
		}
		foreach (XmlNode childNode2 in xmlNode5.ChildNodes)
		{
			if (childNode2.Name == "Animation")
			{
				string animationName = childNode2.Attributes["Name"].GetStringOrDefault(string.Empty);
				TacticValue tacticValue = new TacticValue(childNode2);
				_expectedWaits.Add(new global::Pair<string, TacticValue>(animationName, tacticValue));
			}
		}
	}

	public Tactic(Tactic source)
	{
		_name = source._name;
		_type = source._type;
		_counterAttackChance = source._counterAttackChance;
		_dodgeChance = source._dodgeChance;
		_blockChance = source._blockChance;
		_useSafeAttackChance = source._useSafeAttackChance;
		_tableAttackChance = source._tableAttackChance;
		_cautiousMovementsChance = source._cautiousMovementsChance;
		_dodgeMissilesChance = source._dodgeMissilesChance;
		_dodgeMagicChance = source._dodgeMagicChance;
		_distanceError = source._distanceError;
		_frameError = source._frameError;
		_responseDelay = source._responseDelay;
		_enemyResponseDelay = source._enemyResponseDelay;
		_animationWeights = source._animationWeights;
		_expectedWaits = source._expectedWaits;
		_quickAttacks = source._quickAttacks;
		_evades = source._evades;
		MemoryConfig = source.MemoryConfig;
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

	public float GetCounterAttackChance(TacticFactors factors)
	{
		return _counterAttackChance.GetValue(factors);
	}

	public float GetDodgeChance(TacticFactors factors)
	{
		return _dodgeChance.GetValue(factors);
	}

	public float GetBlockChance(TacticFactors factors)
	{
		return _blockChance.GetValue(factors);
	}

	public float GetUseSafeAttackChance(TacticFactors factors)
	{
		return _useSafeAttackChance.GetValue(factors);
	}

	public float GetTableAttackChance(TacticFactors factors)
	{
		return _tableAttackChance.GetValue(factors);
	}

	public float GetCautiousMovementsChance(TacticFactors factors)
	{
		return _cautiousMovementsChance.GetValue(factors);
	}

	public float GetDodgeMissileChance(TacticFactors factors)
	{
		return _dodgeMissilesChance.GetValue(factors);
	}

	public float GetDodgeMagicChance(TacticFactors factors)
	{
		return _dodgeMagicChance.GetValue(factors);
	}

	public float GetExpectedWait(InfoAnimation animation, TacticFactors factors)
	{
		if (animation != null)
		{
			foreach (global::Pair<string, TacticValue> item in _expectedWaits)
			{
				if (string.IsNullOrEmpty(item.First) || animation.HasName(item.First))
				{
					return item.Second.GetValue(factors);
				}
			}
		}
		else
		{
			foreach (global::Pair<string, TacticValue> item2 in _expectedWaits)
			{
				if (string.IsNullOrEmpty(item2.First))
				{
					return item2.Second.GetValue(factors);
				}
			}
		}
		GameLog.Error("Expected Wait ERROR");
		return 1f;
	}

	public int SelectAnimationWithWeights(List<InfoAnimation> candidateAnimations, InfoAnimation fallbackAnimation, TacticFactors factors)
	{
		int count = candidateAnimations.Count;
		if (0 < count)
		{
			float num = 0f;
			for (int i = 0; i < candidateAnimations.Count; i++)
			{
				InfoAnimation candidateAnimation = candidateAnimations[i];
				if (candidateAnimation == null && fallbackAnimation != null)
				{
					candidateAnimation = fallbackAnimation;
				}
				if (candidateAnimation != null)
				{
					float num2 = GetWeight(candidateAnimation, factors);
					num += num2;
				}
			}
			if (0f < num)
			{
				float num3 = NekkiMath.randomFloat(num);
				int num4 = 0;
				for (int j = 0; j < candidateAnimations.Count; j++)
				{
					InfoAnimation selectedAnimation = candidateAnimations[j];
					if (selectedAnimation == null && fallbackAnimation != null)
					{
						selectedAnimation = fallbackAnimation;
					}
					if (selectedAnimation != null)
					{
						float num5 = GetWeight(selectedAnimation, factors);
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

	public float GetWeight(InfoAnimation animation, TacticFactors factors)
	{
		foreach (global::Pair<string, TacticValue> item in _animationWeights)
		{
			string animationName = item.First;
			if (animationName == string.Empty || animation.HasName(animationName))
			{
				return item.Second.GetValue(factors);
			}
		}
		return 0f;
	}

	public float GetDistanceError(TacticFactors factors)
	{
		float minError = _distanceError.First.GetValue(factors);
		float maxError = _distanceError.Second.GetValue(factors);
		return GetValueFromInterval(minError, maxError);
	}

	public int GetFrameError(TacticFactors factors)
	{
		float minError = _frameError.First.GetValue(factors);
		float maxError = _frameError.Second.GetValue(factors);
		return (int)GetValueFromInterval(minError, maxError);
	}

	public int GetResponseDelay(TacticFactors factors)
	{
		float minDelay = _responseDelay.First.GetValue(factors);
		float maxDelay = _responseDelay.Second.GetValue(factors);
		return (int)GetValueFromInterval(minDelay, maxDelay);
	}

	public int GetEnemyResponseDelay(TacticFactors factors)
	{
		float minDelay = _enemyResponseDelay.First.GetValue(factors);
		float maxDelay = _enemyResponseDelay.Second.GetValue(factors);
		return (int)GetValueFromInterval(minDelay, maxDelay);
	}

	private void ParseQuickAttacks(XmlNode node)
	{
		int num = _quickAttacks.Count;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "QuickAttackChance")
			{
				string animationName = childNode.Attributes["Animation"].GetStringOrDefault(string.Empty);
				_quickAttacks.Add(new global::Pair<string, TacticValue>(string.Empty, new TacticValue()));
				_quickAttacks[num].First = animationName;
				_quickAttacks[num].Second.Parse(childNode);
				num++;
			}
		}
	}

	private void ParseEvades(XmlNode node)
	{
		int num = _evades.Count;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "EvadeChance")
			{
				string animationName = childNode.Attributes["Animation"].GetStringOrDefault(string.Empty);
				_evades.Add(new global::Pair<string, TacticValue>(string.Empty, new TacticValue()));
				_evades[num].First = animationName;
				_evades[num].Second.Parse(childNode);
				num++;
			}
		}
	}

	private static void ParseInterval(XmlNode node, global::Pair<TacticValue, TacticValue> interval)
	{
		if (node != null)
		{
			interval.First.Parse(node["Min"]);
			interval.Second.Parse(node["Max"]);
		}
	}

	private static TacticType GetType(string typeName)
	{
		if (typeName == "Random")
		{
			return TacticType.TacticRandom;
		}
		if (typeName == "Tabular")
		{
			return TacticType.TacticTabular;
		}
		GameLog.Error("Strange tactic type: %s", typeName);
		return TacticType.TacticNone;
	}

	private static float GetValueFromInterval(float minValue, float maxValue)
	{
		float num = NekkiMath.randomFloat() * (maxValue - minValue);
		return minValue + num;
	}
}
