using System.Xml;

public class RandomAreaRule : InFightRule
{
	public const string FILE_PATH_PERK_ACTIVATION_AREA = "Textures/fight/rules/randomarea/";

	private NekkiRandom random = new NekkiRandom();

	private float areaWidth;

	private float areaPositionX;

	private float minPositionX;

	private float maxPositionX;

	private string imagePath;

	private string iconPath;

	public bool IsMarkerHidden;

	private LocationSelectorDarknessData phaseFrames = new LocationSelectorDarknessData();

	private bool _active;

	private bool isAreaVisible;

	private int _currentFrame;

	private float alpha;

	private float fadeInStep;

	private float fadeOutStep;

	public RandomAreaRule(XmlNode node)
		: base(RuleType.RuleRandomArea, RuleAppliance.AppliancePlayer, node)
	{
		areaWidth = 0f;
		imagePath = string.Empty;
		iconPath = string.Empty;
		areaPositionX = 0f;
		_currentFrame = 0;
		minPositionX = -50f;
		maxPositionX = 50f;
		IsMarkerHidden = false;
		_active = true;
		isAreaVisible = false;
		alpha = 0f;
		fadeInStep = 0f;
		fadeOutStep = 0f;
		phaseFrames = new LocationSelectorDarknessData();
		SubscribeEvent(FightEvent.RenderEvent);
		Parse(node);
	}

	public override void InitRule(object data)
	{
		RuleInitData oIFPCFEGFOB = (RuleInitData)data;
		random.setSeed((uint)ListSF.GetCurrentTime());
		minPositionX = (0f - oIFPCFEGFOB.FightLocation.width) / 2f + oIFPCFEGFOB.FightLocation.wallWidth + areaWidth / 2f;
		maxPositionX = oIFPCFEGFOB.FightLocation.width / 2f - oIFPCFEGFOB.FightLocation.wallWidth - areaWidth / 2f;
		_currentFrame = 0;
		alpha = 0f;
		isAreaVisible = false;
	}

	public float GetPositionX()
	{
		return areaPositionX;
	}

	public float GetWidth()
	{
		return areaWidth;
	}

	public string GetImagePath()
	{
		return imagePath;
	}

	public string GetIconPath()
	{
		return iconPath;
	}

	public bool IsAreaVisible()
	{
		return isAreaVisible;
	}

	public float GetAlpha()
	{
		return alpha;
	}

	public override void Stop()
	{
		_active = false;
	}

	public override void Reset()
	{
		_active = true;
	}

	public override void SetActive(bool value)
	{
		base.SetActive(value);
		_active = value;
	}

	protected override bool CompareSingle(object data)
	{
		_currentFrame++;
		if (_currentFrame <= phaseFrames.lightEndFrame)
		{
			if (!_active)
			{
				_currentFrame = 0;
			}
			alpha = 0f;
			isAreaVisible = false;
		}
		else if (_currentFrame <= phaseFrames.darkeningEndFrame)
		{
			alpha = (float)(_currentFrame - phaseFrames.lightEndFrame) * fadeInStep;
			isAreaVisible = true;
		}
		else if (_currentFrame <= phaseFrames.darkEndFrame)
		{
			alpha = 255f;
			isAreaVisible = true;
		}
		else if (_currentFrame <= phaseFrames.lightingEndFrame)
		{
			alpha = 255f - (float)(_currentFrame - phaseFrames.darkEndFrame) * fadeOutStep;
			isAreaVisible = true;
		}
		else
		{
			_currentFrame = 0;
			alpha = 0f;
			isAreaVisible = false;
			RandomizePosition();
		}
		return true;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		areaWidth = node.Attributes["Width"].ParseFloat();
		imagePath += "Textures/fight/rules/randomarea/";
		imagePath += node.Attributes["Image"].GetStringOrDefault(string.Empty);
		if (!node.Attributes["Icon"].Empty())
		{
			iconPath += "Textures/fight/rules/randomarea/";
			iconPath += node.Attributes["Icon"].GetStringOrDefault(string.Empty);
		}
		phaseFrames.lightEndFrame = node.Attributes["FadeIn"].ParseInt();
		phaseFrames.darkeningEndFrame = phaseFrames.lightEndFrame + node.Attributes["FramesOn"].ParseInt();
		phaseFrames.darkEndFrame = phaseFrames.darkeningEndFrame + node.Attributes["FadeOut"].ParseInt();
		phaseFrames.lightingEndFrame = phaseFrames.darkEndFrame + node.Attributes["FramesOff"].ParseInt();
		fadeInStep = 255f / (float)(phaseFrames.darkeningEndFrame - phaseFrames.lightEndFrame);
		fadeOutStep = 255f / (float)(phaseFrames.lightingEndFrame - phaseFrames.darkEndFrame);
	}

	protected void RandomizePosition()
	{
		areaPositionX = random.randomFloat(minPositionX, maxPositionX);
	}

	public override InFightRule Copy()
	{
		InFightRule aAJIFBJLJOA = null;
		XmlNode hKPPBKPJOEO = GetXmlSource().GetNode();
		aAJIFBJLJOA = new RandomAreaRule(hKPPBKPJOEO);
		aAJIFBJLJOA.IsRandom = IsRandom;
		return aAJIFBJLJOA;
	}
}
