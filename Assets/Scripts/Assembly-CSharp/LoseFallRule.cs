using System.Xml;

public class LoseFallRule : AnimationListRule
{
	public const float DefaultMax = -100000f;

	public const float DefaultMin = 100000f;

	private float offsetX;

	private float offsetY;

	private string _nodeName;

	private ModelNode _node;

    internal override System.Action PrepareModelRebind(Model expected, Model replacement)
    {
        if (_node == null || expected == null || expected.GetBodyObject().GetNodeByName(_nodeName) != _node) return null;
        ModelNode target = replacement?.GetBodyObject()?.GetNodeByName(_nodeName);
        if (target == null) throw new System.InvalidOperationException("Form is missing LoseFallRule node: " + _nodeName);
        return () => _node = target;
    }

	private float minX;

	private float maxX;

	private float maxY;

	private float minY;

	private bool _isCheckRender;

	public LoseFallRule(XmlNode node, RuleAppliance appliance)
		: base(RuleType.RuleLoseFall, appliance, node)
	{
		_isCheckRender = false;
		maxX = -100000f;
		minX = 100000f;
		maxY = -100000f;
		minY = 100000f;
		offsetX = 0f;
		offsetY = 0f;
		_node = null;
		Parse(node);
		SubscribeEvent(FightEvent.AnimationStartEvent);
		SubscribeEvent(FightEvent.RenderEvent);
		if (CheckAnimation("Physical"))
		{
			SubscribeEvent(FightEvent.PhysicsStartEvent);
		}
	}

	public float GetMinX()
	{
		return minX;
	}

	public float GetMaxX()
	{
		return maxX;
	}

	public bool CheckOutOfBounds()
	{
		if (_node == null || !_isCheckRender)
		{
			return false;
		}
		Vector3f fallPosition = _node.GetStart();
		fallPosition = new Vector3f(fallPosition.GetX() + offsetX, 0f - fallPosition.GetY() + offsetY, fallPosition.GetZ());
		bool flag = fallPosition.GetX() > maxX || fallPosition.GetX() < minX || fallPosition.GetY() > maxY || fallPosition.GetY() < minY;
		if (flag)
		{
			SetActive(false);
		}
		return flag;
	}

	public override void InitRule(object data)
	{
		Reset();
		RuleInitData initData = (RuleInitData)data;
		if (initData.FightLocation != null)
		{
			offsetX = (0f - initData.FightLocation.width) / 2f;
			offsetY = 0f - initData.FightLocation.positionY;
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
			GameLog.Error("LoseFallRule::initRule error - no ModelNode found with name %s", _nodeName);
		}
	}

	public override void Reset()
	{
		_isCheckRender = false;
	}

	public override void Clear()
	{
		_isCheckRender = false;
	}

	protected override bool CompareSingle(object data)
	{
		FightData fightData = (FightData)data;
		switch (fightData.FightEventType)
		{
		case FightEvent.PhysicsStartEvent:
			_isCheckRender = true;
			return CheckOutOfBounds();
		case FightEvent.AnimationStartEvent:
			_isCheckRender = CheckAnimation(fightData.CurrentAnimation);
			return CheckOutOfBounds();
		case FightEvent.RenderEvent:
			return CheckOutOfBounds();
		default:
			return false;
		}
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		_nodeName = node.Attributes["Node"].GetStringOrDefault(string.Empty);
		string text = node.Attributes["Axis"].GetStringOrDefault(string.Empty);
		if (text == "X")
		{
			maxX = node.Attributes["Max"].ParseFloat(-100000f);
			minX = node.Attributes["Min"].ParseFloat(100000f);
		}
		if (text == "Y")
		{
			maxY = node.Attributes["Max"].ParseFloat(-100000f);
			minY = node.Attributes["Min"].ParseFloat(100000f);
		}
	}
}
