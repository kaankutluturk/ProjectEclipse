using System.Xml;

public class DarknessRule : InFightRule
{
	public enum DarknessStage
	{
		STAGE_BLACKOUT = 0,
		STAGE_LASTING = 1,
		STAGE_LIGHT = 2,
		STAGE_PAUSE = 3
	}

	private LocationSelectorDarknessData darknessData = new LocationSelectorDarknessData();

	private bool _active;

	private int _currentFrame;

	private float currentAlpha;

	private float fadeInStep;

	private float fadeOutStep;

	protected DarknessStage stage;

	public DarknessRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleDarkness, EJPOJJKKICO, node)
	{
		darknessData = new LocationSelectorDarknessData();
		stage = DarknessStage.STAGE_PAUSE;
		_currentFrame = 0;
		currentAlpha = 0f;
		_active = false;
		SubscribeEvent(FightEvent.RenderEvent);
		Parse(node);
	}

	public override void InitRule(object data)
	{
		_currentFrame = 0;
		currentAlpha = 0f;
	}

	public float GetAlpha()
	{
		return currentAlpha;
	}

	public LocationSelectorDarknessData GetDarknessData()
	{
		return darknessData;
	}

	public override void SetActive(bool value)
	{
		base.SetActive(value);
		_active = value;
	}

	protected override bool CompareSingle(object data)
	{
		_currentFrame++;
		if (_currentFrame <= darknessData.lightEndFrame)
		{
			if (!_active)
			{
				_currentFrame = 0;
			}
			currentAlpha = 0f;
		}
		else if (_currentFrame <= darknessData.darkeningEndFrame)
		{
			currentAlpha = (float)(_currentFrame - darknessData.lightEndFrame) * fadeInStep;
		}
		else if (_currentFrame <= darknessData.darkEndFrame)
		{
			currentAlpha = 255f;
		}
		else if (_currentFrame <= darknessData.lightingEndFrame)
		{
			currentAlpha = 255f - (float)(_currentFrame - darknessData.darkEndFrame) * fadeOutStep;
		}
		else
		{
			_currentFrame = 0;
			currentAlpha = 0f;
		}
		return true;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		darknessData.lightEndFrame = node.Attributes["LightLasting"].ParseInt();
		darknessData.darkeningEndFrame = darknessData.lightEndFrame + node.Attributes["DarkOn"].ParseInt();
		darknessData.darkEndFrame = darknessData.darkeningEndFrame + node.Attributes["DarkLasting"].ParseInt();
		darknessData.lightingEndFrame = darknessData.darkEndFrame + node.Attributes["LightOn"].ParseInt();
		fadeInStep = 255f / (float)(darknessData.darkeningEndFrame - darknessData.lightEndFrame);
		fadeOutStep = 255f / (float)(darknessData.lightingEndFrame - darknessData.darkEndFrame);
	}

	public override void Stop()
	{
		SetActive(false);
	}

	public override void Reset()
	{
		SetActive(true);
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		RuleAppliance eJPOJJKKICO = GetAppliance();
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new DarknessRule(hKPPBKPJOEO, eJPOJJKKICO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
