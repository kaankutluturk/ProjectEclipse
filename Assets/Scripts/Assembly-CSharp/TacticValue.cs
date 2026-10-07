using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class TacticValue
{
	private enum FactorType
	{
		Exponential = 0,
		Linear = 1
	}

	private float _base;

	private float _counterFactor;

	private float _damageFactor;

	private float _healthFactor;

	private float _enemyHealthFactor;

	private float _animationFramesFactor;

	private float _magicBulletFactor;

	private float _missileBulletFactor;

	private float _hitFactor;

	private float _childFramesFactor;

	private float _distanceFactor;

	private float _limit;

	private float _antiLimit;

	private float _shift;

	private FactorType _factorType = FactorType.Linear;

	private List<global::Pair<string, TacticValue>> _animationFactors = new List<global::Pair<string, TacticValue>>();

	private List<global::Pair<InfoAnimation, float>> _myAnimationFactors = new List<global::Pair<InfoAnimation, float>>();

	private List<global::Pair<InfoAnimation, float>> _enemyAnimationFactors = new List<global::Pair<InfoAnimation, float>>();

	public TacticValue()
	{
	}

	public TacticValue(XmlNode node)
	{
		Parse(node);
	}

	public TacticValue(TacticValue value)
	{
		CopyFrom(value);
	}

	public void Parse(XmlNode node)
	{
		if (node != null)
		{
			_base = node.Attributes["Base"].ParseFloat();
			_counterFactor = node.Attributes["CounterFactor"].ParseFloat();
			_damageFactor = node.Attributes["DamageFactor"].ParseFloat();
			_healthFactor = node.Attributes["HealthFactor"].ParseFloat();
			_enemyHealthFactor = node.Attributes["EnemyHealthFactor"].ParseFloat();
			_animationFramesFactor = node.Attributes["AnimationFramesFactor"].ParseFloat();
			_childFramesFactor = node.Attributes["ChildFramesFactor"].ParseFloat();
			_magicBulletFactor = node.Attributes["MagicBulletFactor"].ParseFloat();
			_missileBulletFactor = node.Attributes["MissileBulletFactor"].ParseFloat();
			_hitFactor = node.Attributes["HitFactor"].ParseFloat();
			_distanceFactor = node.Attributes["DistanceFactor"].ParseFloat();
			_shift = node.Attributes["Shift"].ParseFloat();
			_limit = node.Attributes["Limit"].ParseFloat();
			_antiLimit = node.Attributes["AntiLimit"].ParseFloat();
			SetFactorType(node.Attributes["FactorType"].GetStringOrDefault(string.Empty));
			ParseAnimations(node);
		}
	}

	public void ParseAnimations(XmlNode node)
	{
		_animationFactors.Clear();
		int num = 0;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "AnimationFactors")
			{
				_animationFactors.Add(new global::Pair<string, TacticValue>(string.Empty, new TacticValue()));
				_animationFactors[num].First = childNode.Attributes["Animation"].GetStringOrDefault(string.Empty);
				_animationFactors[num].Second.Parse(childNode);
				num++;
			}
			else if (childNode.Name == "CurrentAnimation")
			{
				global::Pair<InfoAnimation, float> animationFactor = new global::Pair<InfoAnimation, float>(null, 0f);
				animationFactor.First = AnimationData.GetAnimationByName(childNode.Attributes["Animation"].GetStringOrDefault(string.Empty));
				animationFactor.Second = childNode.Attributes["Factor"].ParseFloat();
				string text = childNode.Attributes["Player"].GetStringOrDefault("Me");
				if (text == "Enemy")
				{
					_enemyAnimationFactors.Add(animationFactor);
				}
				else if (text == "Me")
				{
					_myAnimationFactors.Add(animationFactor);
				}
			}
		}
	}

	public float GetValue(TacticFactors factors)
	{
		float num = factors.FactorsCount * _counterFactor;
		float num2 = factors.Damage * _damageFactor;
		float num3 = (1f - factors.Health) * _healthFactor;
		float num4 = (1f - factors.EnemyHealth) * _enemyHealthFactor;
		float num5 = (float)factors.AnimationFrames * _animationFramesFactor;
		float num6 = (float)factors.MagicBullets * _magicBulletFactor;
		float num7 = (float)factors.MissileBullets * _missileBulletFactor;
		float num8 = factors.Hits * _hitFactor;
		float num9 = (float)factors.ChildFrames * _childFramesFactor;
		float num10 = factors.Distance * _distanceFactor;
		float num11 = num + num2 + num3 + num4 + num5 + num6 + num7 + num8 + num9 + num10 + _shift;
		foreach (global::Pair<string, TacticValue> item in _animationFactors)
		{
			float count = 0f;
			float damage = 0f;
			float hits = 0f;
			factors.Statistics.GetCountAndDamage(true, item.First, ref count, ref damage, ref hits);
			float num12 = count * item.Second._counterFactor;
			float num13 = damage * item.Second._damageFactor;
			float num14 = hits * item.Second._hitFactor;
			num11 += num12 + num13 + num14;
		}
		num11 += GetAnimationSummands(factors.CurrentAnimation, _myAnimationFactors);
		num11 += GetAnimationSummands(factors.EnemyCurrentAnimation, _enemyAnimationFactors);
		if (_factorType == FactorType.Exponential)
		{
			return CalculateExponentialChance(num11);
		}
		if (_factorType == FactorType.Linear)
		{
			return CalculateLinearChance(num11);
		}
		return 0f;
	}

	private float CalculateExponentialChance(float factor)
	{
		float num = 0f;
		if (0f <= factor)
		{
			return _limit + (_base - _limit) * Mathf.Pow(2f, 0f - factor);
		}
		return _antiLimit + (_base - _antiLimit) * Mathf.Pow(2f, factor);
	}

	private float CalculateLinearChance(float factor)
	{
		float num = 0f;
		if (0f <= factor)
		{
			return _base + (_limit - _base) * Mathf.Min(1f, factor);
		}
		return _base + (_antiLimit - _base) * Mathf.Min(1f, 0f - factor);
	}

	private void SetFactorType(string factorType)
	{
		if (factorType == "Linear")
		{
			_factorType = FactorType.Linear;
		}
		else if (factorType == "Exponential")
		{
			_factorType = FactorType.Exponential;
		}
		else
		{
			_factorType = FactorType.Linear;
		}
	}

	private float GetAnimationSummands(InfoAnimation animation, List<global::Pair<InfoAnimation, float>> animationFactors)
	{
		foreach (global::Pair<InfoAnimation, float> item in animationFactors)
		{
			if (animation == item.First)
			{
				return item.Second;
			}
		}
		return 0f;
	}

	private void CopyFrom(TacticValue source)
	{
		_base = source._base;
		_counterFactor = source._counterFactor;
		_damageFactor = source._damageFactor;
		_healthFactor = source._healthFactor;
		_enemyHealthFactor = source._enemyHealthFactor;
		_animationFramesFactor = source._animationFramesFactor;
		_childFramesFactor = source._childFramesFactor;
		_magicBulletFactor = source._magicBulletFactor;
		_missileBulletFactor = source._missileBulletFactor;
		_hitFactor = source._hitFactor;
		_limit = source._limit;
		_antiLimit = source._antiLimit;
		_shift = source._shift;
		_animationFactors = source._animationFactors;
		_factorType = source._factorType;
		_distanceFactor = source._distanceFactor;
		_myAnimationFactors = source._myAnimationFactors;
		_enemyAnimationFactors = source._enemyAnimationFactors;
	}
}
