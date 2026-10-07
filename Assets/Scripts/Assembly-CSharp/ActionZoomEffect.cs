using System.Xml;

public class ActionZoomEffect : ActionAnimation
{
	private GameUtils.ZoomEffect _Effect = new GameUtils.ZoomEffect();

	public GameUtils.ZoomEffect Effect
	{
		get
		{
			return GetEffect();
		}
	}

	public ActionZoomEffect(XmlNode node)
		: base(ActionType.ZOOM_EFFECT)
	{
		Parse(node);
	}

	public GameUtils.ZoomEffect GetEffect()
	{
		return _Effect;
	}

	public override void Visit(Model ACENLMONNPA)
	{
		ACENLMONNPA.StartAction(this);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		_Effect.EffectTime = node.Attributes["EffectTime"].ParseInt();
		_Effect.TargetScale = node.Attributes["ZoomScale"].ParseFloat();
	}
}
