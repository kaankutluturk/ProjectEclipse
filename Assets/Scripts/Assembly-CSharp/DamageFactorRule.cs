using System.Xml;

public class DamageFactorRule : AnimationListRule
{
	public const float DefaultFactor = 1f;

	public const float DefaultRepeatFactor = 1f;

	private float factor;

	private float repeatFactor;

	public DamageFactorRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleDamageFactor, EJPOJJKKICO, node)
	{
		Parse(node);
	}

	protected override bool CompareSingle(object data)
	{
		FightData hCPJJKMNMCE = (FightData)data;
		return CheckAnimation(hCPJJKMNMCE.CurrentAnimation);
	}

	public override void InitRule(object data)
	{
		foreach (InfoAnimation item in animations)
		{
			foreach (IntervalAnimation item2 in item.MoveData.Intervals)
			{
				if (item2.Type == IntervalAnimation.IntervalType.INTERVAL_ATTACK)
				{
					IntervalAttack hFIIPNLCIEE = (IntervalAttack)item2;
					IntervalAttack.Factors bPLPKPIBEIF = hFIIPNLCIEE.GetFactors(appliance);
					if (bPLPKPIBEIF.IsFactorSet || bPLPKPIBEIF.IsMultiplierSet)
					{
						break;
					}
					bPLPKPIBEIF.IsFactorSet = true;
					bPLPKPIBEIF.Factor = factor;
					bPLPKPIBEIF.IsMultiplierSet = true;
					bPLPKPIBEIF.FactorMultiplier = repeatFactor;
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
					IntervalAttack hFIIPNLCIEE = (IntervalAttack)item2;
					IntervalAttack.Factors bPLPKPIBEIF = hFIIPNLCIEE.GetFactors(appliance);
					if (bPLPKPIBEIF.IsFactorSet || bPLPKPIBEIF.IsMultiplierSet)
					{
						bPLPKPIBEIF.IsFactorSet = false;
						bPLPKPIBEIF.Factor = 1f;
						bPLPKPIBEIF.IsMultiplierSet = false;
						bPLPKPIBEIF.FactorMultiplier = 1f;
					}
				}
			}
		}
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		string gOHIIMFFFJI = node.Attributes["Animation"].GetStringOrDefault(string.Empty);
		AnimationData.AddTemplateAnimations(gOHIIMFFFJI, animations);
		factor = node.Attributes["Factor"].ParseFloat(1f);
		repeatFactor = node.Attributes["RepeatFactor"].ParseFloat(1f);
	}
}
