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

	public TacticValue(XmlNode AFHNINCKJEE)
	{
		Parse(AFHNINCKJEE);
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
				global::Pair<InfoAnimation, float> cCKLNOPEKHO = new global::Pair<InfoAnimation, float>(null, 0f);
				cCKLNOPEKHO.First = AnimationData.GetAnimationByName(childNode.Attributes["Animation"].GetStringOrDefault(string.Empty));
				cCKLNOPEKHO.Second = childNode.Attributes["Factor"].ParseFloat();
				string text = childNode.Attributes["Player"].GetStringOrDefault("Me");
				if (text == "Enemy")
				{
					_enemyAnimationFactors.Add(cCKLNOPEKHO);
				}
				else if (text == "Me")
				{
					_myAnimationFactors.Add(cCKLNOPEKHO);
				}
			}
		}
	}

	public float GetValue(TacticFactors JCICKLIMBEF)
	{
		float num = JCICKLIMBEF.FactorsCount * _counterFactor;
		float num2 = JCICKLIMBEF.Damage * _damageFactor;
		float num3 = (1f - JCICKLIMBEF.Health) * _healthFactor;
		float num4 = (1f - JCICKLIMBEF.EnemyHealth) * _enemyHealthFactor;
		float num5 = (float)JCICKLIMBEF.AnimationFrames * _animationFramesFactor;
		float num6 = (float)JCICKLIMBEF.MagicBullets * _magicBulletFactor;
		float num7 = (float)JCICKLIMBEF.MissileBullets * _missileBulletFactor;
		float num8 = JCICKLIMBEF.Hits * _hitFactor;
		float num9 = (float)JCICKLIMBEF.ChildFrames * _childFramesFactor;
		float num10 = JCICKLIMBEF.Distance * _distanceFactor;
		float num11 = num + num2 + num3 + num4 + num5 + num6 + num7 + num8 + num9 + num10 + _shift;
		foreach (global::Pair<string, TacticValue> item in _animationFactors)
		{
			float count = 0f;
			float CKKFKEIELCP = 0f;
			float JOOJIMPEPOJ = 0f;
			JCICKLIMBEF.Statistics.GetCountAndDamage(true, item.First, ref count, ref CKKFKEIELCP, ref JOOJIMPEPOJ);
			float num12 = count * item.Second._counterFactor;
			float num13 = CKKFKEIELCP * item.Second._damageFactor;
			float num14 = JOOJIMPEPOJ * item.Second._hitFactor;
			num11 += num12 + num13 + num14;
		}
		num11 += GetAnimationSummands(JCICKLIMBEF.CurrentAnimation, _myAnimationFactors);
		num11 += GetAnimationSummands(JCICKLIMBEF.EnemyCurrentAnimation, _enemyAnimationFactors);
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

	private float CalculateExponentialChance(float IGAPINAEDPP)
	{
		float num = 0f;
		if (0f <= IGAPINAEDPP)
		{
			return _limit + (_base - _limit) * Mathf.Pow(2f, 0f - IGAPINAEDPP);
		}
		return _antiLimit + (_base - _antiLimit) * Mathf.Pow(2f, IGAPINAEDPP);
	}

	private float CalculateLinearChance(float IGAPINAEDPP)
	{
		float num = 0f;
		if (0f <= IGAPINAEDPP)
		{
			return _base + (_limit - _base) * Mathf.Min(1f, IGAPINAEDPP);
		}
		return _base + (_antiLimit - _base) * Mathf.Min(1f, 0f - IGAPINAEDPP);
	}

	private void SetFactorType(string JNPHBPCMFEH)
	{
		if (JNPHBPCMFEH == "Linear")
		{
			_factorType = FactorType.Linear;
		}
		else if (JNPHBPCMFEH == "Exponential")
		{
			_factorType = FactorType.Exponential;
		}
		else
		{
			_factorType = FactorType.Linear;
		}
	}

	private float GetAnimationSummands(InfoAnimation DBOLBEOCEME, List<global::Pair<InfoAnimation, float>> JCJDOODBPBB)
	{
		foreach (global::Pair<InfoAnimation, float> item in JCJDOODBPBB)
		{
			if (DBOLBEOCEME == item.First)
			{
				return item.Second;
			}
		}
		return 0f;
	}

	private void CopyFrom(TacticValue JFMALLHPPMH)
	{
		_base = JFMALLHPPMH._base;
		_counterFactor = JFMALLHPPMH._counterFactor;
		_damageFactor = JFMALLHPPMH._damageFactor;
		_healthFactor = JFMALLHPPMH._healthFactor;
		_enemyHealthFactor = JFMALLHPPMH._enemyHealthFactor;
		_animationFramesFactor = JFMALLHPPMH._animationFramesFactor;
		_childFramesFactor = JFMALLHPPMH._childFramesFactor;
		_magicBulletFactor = JFMALLHPPMH._magicBulletFactor;
		_missileBulletFactor = JFMALLHPPMH._missileBulletFactor;
		_hitFactor = JFMALLHPPMH._hitFactor;
		_limit = JFMALLHPPMH._limit;
		_antiLimit = JFMALLHPPMH._antiLimit;
		_shift = JFMALLHPPMH._shift;
		_animationFactors = JFMALLHPPMH._animationFactors;
		_factorType = JFMALLHPPMH._factorType;
		_distanceFactor = JFMALLHPPMH._distanceFactor;
		_myAnimationFactors = JFMALLHPPMH._myAnimationFactors;
		_enemyAnimationFactors = JFMALLHPPMH._enemyAnimationFactors;
	}
}
