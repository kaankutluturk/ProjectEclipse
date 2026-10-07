using System.Xml;

public class DamageFactorRule : AnimationListRule
{
	public const float DefaultFactor = 1f;

	public const float DefaultRepeatFactor = 1f;

	private float factor;

	private float repeatFactor;

	public DamageFactorRule(XmlNode node, RuleAppliance ruleAppliance)
		: base(RuleType.RuleDamageFactor, ruleAppliance, node)
	{
		Parse(node);
	}

	protected override bool CompareSingle(object data)
	{
		FightData fightData = (FightData)data;
		return CheckAnimation(fightData.CurrentAnimation);
	}

	public override void InitRule(object data)
	{
		foreach (InfoAnimation item in animations)
		{
			foreach (IntervalAnimation item2 in item.MoveData.Intervals)
			{
				if (item2.Type == IntervalAnimation.IntervalType.INTERVAL_ATTACK)
				{
					IntervalAttack intervalAttack = (IntervalAttack)item2;
					IntervalAttack.Factors attackFactors = intervalAttack.GetFactors(appliance);
					if (attackFactors.IsFactorSet || attackFactors.IsMultiplierSet)
					{
						break;
					}
					attackFactors.IsFactorSet = true;
					attackFactors.Factor = factor;
					attackFactors.IsMultiplierSet = true;
					attackFactors.FactorMultiplier = repeatFactor;
				}
			}
		}
	}

	public override void Reset()
	{
		Clear();
	}

	public override void Clear()
	{
		foreach (InfoAnimation item in animations)
		{
			foreach (IntervalAnimation item2 in item.MoveData.Intervals)
			{
				if (item2.Type == IntervalAnimation.IntervalType.INTERVAL_ATTACK)
				{
					IntervalAttack intervalAttack = (IntervalAttack)item2;
					IntervalAttack.Factors attackFactors = intervalAttack.GetFactors(appliance);
					if (attackFactors.IsFactorSet || attackFactors.IsMultiplierSet)
					{
						attackFactors.IsFactorSet = false;
						attackFactors.Factor = 1f;
						attackFactors.IsMultiplierSet = false;
						attackFactors.FactorMultiplier = 1f;
					}
				}
			}
		}
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		string animationName = node.Attributes["Animation"].GetStringOrDefault(string.Empty);
		AnimationData.AddTemplateAnimations(animationName, animations);
		factor = node.Attributes["Factor"].ParseFloat(1f);
		repeatFactor = node.Attributes["RepeatFactor"].ParseFloat(1f);
	}
}
