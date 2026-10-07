using System.Collections.Generic;
using System.Xml;

public class HotGroundRule : AnimationListRule
{
	public class LimitedNode
	{
		public ModelNode node;

		public string name;

		public float MaxX;

		public float MaxY;

		public float MinX;

		public float MinY;

		public LimitedNode(XmlNode EABJIAHGLEO)
		{
			MaxX = float.MaxValue;
			MinX = float.MinValue;
			MaxY = float.MaxValue;
			MinY = float.MinValue;
			name = string.Empty;
			node = null;
			name = EABJIAHGLEO.Attributes["Name"].GetStringOrDefault(string.Empty);
			string text = EABJIAHGLEO.Attributes["Axis"].GetStringOrDefault(string.Empty);
			if (text == "X")
			{
				MaxX = EABJIAHGLEO.Attributes["Max"].ParseFloat(float.MaxValue);
				MinX = EABJIAHGLEO.Attributes["Min"].ParseFloat(float.MinValue);
			}
			if (text == "Y")
			{
				MaxY = EABJIAHGLEO.Attributes["Max"].ParseFloat(float.MaxValue);
				MinY = EABJIAHGLEO.Attributes["Min"].ParseFloat(float.MinValue);
			}
		}
	}

	public const int FRAMES_IN_SECOND = 60;

	private List<LimitedNode> limitedNodes = new List<LimitedNode>();

    internal override System.Action PrepareModelRebind(Model expected, Model replacement)
    {
        var assignments = new List<System.Action>();
        foreach (LimitedNode point in limitedNodes)
        {
            if (point.node == null || expected == null || expected.GetBodyObject().GetNodeByName(point.name) != point.node) continue;
            ModelNode target = replacement?.GetBodyObject()?.GetNodeByName(point.name);
            if (target == null) throw new System.InvalidOperationException("Form is missing HotGroundRule node: " + point.name);
            assignments.Add(() => point.node = target);
        }
        return assignments.Count == 0 ? (System.Action)null : () => { foreach (var assign in assignments) assign(); };
    }

	private string _sequenceName;

	private bool hasSequence;

	private float sequenceWidth;

	public bool timerChanged;

	private bool isAnimationMatched;

	private bool isTimerReset;

	private int remainingSeconds;

	private int maxSeconds;

	private float _frames;

	private float totalFrames;

	private float offsetX;

	private float offsetY;

	private int slowModeDivisor;

	public HotGroundRule(XmlNode node, RuleAppliance EJPOJJKKICO)
		: base(RuleType.RuleHotGround, EJPOJJKKICO, node)
	{
		timerChanged = true;
		offsetX = 0f;
		offsetY = 0f;
		isAnimationMatched = false;
		isTimerReset = false;
		slowModeDivisor = 1;
		sequenceWidth = 0f;
		hasSequence = false;
		SubscribeEvent(FightEvent.AnimationStartEvent);
		SubscribeEvent(FightEvent.RenderEvent);
		Parse(node);
		Reset();
	}

	public int GetRemainingSeconds()
	{
		return remainingSeconds;
	}

	public int GetMaxSeconds()
	{
		return maxSeconds;
	}

	public override void Reset()
	{
		remainingSeconds = maxSeconds;
		_frames = 0f;
		timerChanged = true;
	}

	public override void InitRule(object data)
	{
		RuleInitData oIFPCFEGFOB = (RuleInitData)data;
		if (oIFPCFEGFOB.FightLocation != null)
		{
			offsetX = (0f - oIFPCFEGFOB.FightLocation.width) / 2f;
			offsetY = 0f - oIFPCFEGFOB.FightLocation.floorHeight;
		}
		Model fGCODGKLHED = null;
		switch (appliance)
		{
		case RuleAppliance.AppliancePlayer:
			fGCODGKLHED = oIFPCFEGFOB.PlayerModel;
			break;
		case RuleAppliance.ApplianceOpponent:
			fGCODGKLHED = oIFPCFEGFOB.OpponentModel;
			break;
		}
		if (fGCODGKLHED != null)
		{
			foreach (LimitedNode item in limitedNodes)
			{
				item.node = fGCODGKLHED.GetBodyObject().GetNodeByName(item.name);
				if (item.node == null)
				{
					GameLog.Error("RingoutRule::initRule error - no ModelNode found with name " + item.name);
				}
			}
		}
		Reset();
	}

	public bool HasSequence()
	{
		return hasSequence;
	}

	public string GetSequenceName()
	{
		return _sequenceName;
	}

	public float GetSequenceWidth()
	{
		return sequenceWidth;
	}

	protected override bool CompareSingle(object data)
	{
		FightData hCPJJKMNMCE = (FightData)data;
		switch (hCPJJKMNMCE.FightEventType)
		{
		case FightEvent.RenderEvent:
			if (isAnimationMatched && AreAllNodesOutsideLimits())
			{
				if (!isTimerReset)
				{
					remainingSeconds = maxSeconds;
					_frames = 0f;
					timerChanged = true;
					isTimerReset = true;
				}
				break;
			}
			_frames += 1f / (float)slowModeDivisor;
			if (_frames >= 60f)
			{
				_frames = 0f;
				if (remainingSeconds > 0)
				{
					remainingSeconds--;
					timerChanged = true;
				}
			}
			break;
		case FightEvent.AnimationStartEvent:
			isTimerReset = false;
			isAnimationMatched = CheckAnimation(hCPJJKMNMCE.CurrentAnimation);
			break;
		}
		return remainingSeconds <= 0;
	}

	protected override void PrepareCompare(object data)
	{
		PlayersFightData jNGGHELCPFM = (PlayersFightData)data;
		slowModeDivisor = jNGGHELCPFM.SlowMode;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		ParseLimitedNodes(node);
		totalFrames = node.Attributes["Frames"].ParseFloat();
		maxSeconds = (int)(totalFrames / 60f);
		hasSequence = !node.Attributes["Sequence"].Empty();
		if (hasSequence)
		{
			_sequenceName = node.Attributes["Sequence"].GetStringOrDefault(string.Empty);
			sequenceWidth = node.Attributes["SequenceWidth"].ParseFloat();
		}
	}

	protected void ParseLimitedNodes(XmlNode node)
	{
		foreach (XmlNode item in node.SelectNodes("Node"))
		{
			limitedNodes.Add(new LimitedNode(item));
		}
	}

	protected bool AreAllNodesOutsideLimits()
	{
		foreach (LimitedNode item in limitedNodes)
		{
			Vector3f eMAFACPEPDK = item.node.GetStart();
			eMAFACPEPDK = new Vector3f(eMAFACPEPDK.GetX() + offsetX, 0f - eMAFACPEPDK.GetY(), eMAFACPEPDK.GetZ());
			if (!(eMAFACPEPDK.GetX() >= item.MaxX) && !(eMAFACPEPDK.GetX() <= item.MinX) && !(eMAFACPEPDK.GetY() >= item.MaxY) && !(eMAFACPEPDK.GetY() <= item.MinY))
			{
				return false;
			}
		}
		return true;
	}
}
