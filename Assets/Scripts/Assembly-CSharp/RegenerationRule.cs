using System.Xml;

public class RegenerationRule : InFightRule
{
	private float _frames;

	private float framesAfterHit;

	private float rate;

	private bool _isWeaponStrike;

	public RegenerationRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleRegeneration, EJPOJJKKICO, node)
	{
		_frames = 0f;
		framesAfterHit = 0f;
		rate = 0f;
		_isWeaponStrike = false;
		SubscribeEvent(FightEvent.HitEvent);
		SubscribeEvent(FightEvent.RenderEvent);
		Parse(node);
		Reset();
	}

	public override void InitRule(object data)
	{
		_frames = 0f;
	}

	public float GetRate()
	{
		return rate;
	}

	protected override bool CompareSingle(object data)
	{
		FightData hCPJJKMNMCE = (FightData)data;
		switch (hCPJJKMNMCE.FightEventType)
		{
		case FightEvent.RenderEvent:
			_frames++;
			if (_isWeaponStrike && hCPJJKMNMCE.IsUsingItem)
			{
				return false;
			}
			if (_frames >= framesAfterHit)
			{
				return true;
			}
			break;
		case FightEvent.HitEvent:
			_frames = 0f;
			break;
		}
		return false;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		framesAfterHit = node.Attributes["FramesAfterHit"].ParseFloat();
		rate = node.Attributes["Rate"].ParseFloat();
		_isWeaponStrike = node.Attributes["WeaponStrike"].ParseBool();
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new RegenerationRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
