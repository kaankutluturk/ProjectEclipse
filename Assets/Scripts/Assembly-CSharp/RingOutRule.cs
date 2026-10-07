using System.Xml;

public class RingOutRule : InFightRule
{
	public const float DefaultMax = 100000f;

	public const float DefaultMin = -100000f;

	public const float DefaultSequenceSpeed = 3f;

	public const string DEFAULT_SEQUENTION_NAME = "ringout";

	private float originOffsetX;

	private float originOffsetY;

	private string _nodeName;

	private string sequenceName;

	private ModelNode _node;

    internal override System.Action PrepareModelRebind(Model expected, Model replacement)
    {
        if (_node == null || expected == null || expected.GetBodyObject().GetNodeByName(_nodeName) != _node) return null;
        ModelNode target = replacement?.GetBodyObject()?.GetNodeByName(_nodeName);
        if (target == null) throw new System.InvalidOperationException("Form is missing RingOutRule node: " + _nodeName);
        return () => _node = target;
    }

	private float minX;

	private float maxX;

	private float maxY;

	private float minY;

	private float sequenceSpeed;

	public RingOutRule(XmlNode node, RuleAppliance appliance)
		: base(RuleType.RuleRingout, appliance, node)
	{
		_nodeName = string.Empty;
		maxY = 100000f;
		minY = -100000f;
		minX = -100000f;
		maxX = 100000f;
		_node = null;
		originOffsetX = 0f;
		sequenceSpeed = 3f;
		SubscribeEvent(FightEvent.RenderEvent);
		Parse(node);
	}

	public float GetMinX()
	{
		return minX;
	}

	public float GetMaxX()
	{
		return maxX;
	}

	public float GetSequenceSpeed()
	{
		return sequenceSpeed;
	}

	public override void InitRule(object data)
	{
		RuleInitData initData = (RuleInitData)data;
		if (initData.FightLocation != null)
		{
			originOffsetX = (0f - initData.FightLocation.width) / 2f;
			originOffsetY = 0f - initData.FightLocation.floorHeight;
		}
		switch (appliance)
		{
		case RuleAppliance.AppliancePlayer:
			if (initData.PlayerModel != null)
			{
				_node = initData.PlayerModel.GetBodyObject().GetNodeByName(_nodeName);
			}
			break;
		case RuleAppliance.ApplianceOpponent:
			if (initData.OpponentModel != null)
			{
				_node = initData.OpponentModel.GetBodyObject().GetNodeByName(_nodeName);
			}
			break;
		}
		if (_node == null)
		{
			GameLog.Error("RingoutRule::initRule error - no ModelNode found with name %s", _nodeName);
		}
	}

	public string GetSequenceName()
	{
		return sequenceName;
	}

	protected override bool CompareSingle(object data)
	{
		if (_node == null)
		{
			return false;
		}
		Vector3f edgePosition = _node.GetStart();
		edgePosition = new Vector3f(edgePosition.GetX() + originOffsetX, 0f - edgePosition.GetY() + originOffsetY, edgePosition.GetZ());
		bool flag = edgePosition.GetX() > maxX || edgePosition.GetX() < minX || edgePosition.GetY() > maxY || edgePosition.GetY() < minY;
		if (flag)
		{
			SetActive(false);
		}
		return flag;
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		_nodeName = node.Attributes["Node"].GetStringOrDefault(string.Empty);
		string text = node.Attributes["Axis"].GetStringOrDefault(string.Empty);
		if (text == "X")
		{
			maxX = node.Attributes["Max"].ParseFloat(100000f);
			minX = node.Attributes["Min"].ParseFloat(-100000f);
		}
		if (text == "Y")
		{
			maxY = node.Attributes["Max"].ParseFloat(100000f);
			minY = node.Attributes["Min"].ParseFloat(-100000f);
		}
		sequenceSpeed = node.Attributes["SequentionSpeed"].ParseFloat(3f);
		sequenceName = node.Attributes["Sequence"].GetStringOrDefault("ringout");
	}

	public override InFightRule Copy()
	{
		InFightRule copy = null;
		RuleAppliance appliance = GetAppliance();
		XmlNode node = GetXmlSource().GetNode();
		copy = new RingOutRule(node, appliance);
		copy.IsRandom = IsRandom;
		return copy;
	}
}
