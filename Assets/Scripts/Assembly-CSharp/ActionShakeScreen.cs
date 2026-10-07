using System.Xml;

public class ActionShakeScreen : ActionAnimation
{
	private GameUtils.HitEffect _Effect = new GameUtils.HitEffect();

	public GameUtils.HitEffect Effect
	{
		get
		{
			return GetEffect();
		}
	}

	public ActionShakeScreen(XmlNode node)
		: base(ActionType.SHAKE_SCREEN)
	{
		Parse(node);
	}

	public GameUtils.HitEffect GetEffect()
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
		_Effect.PauseTime = node.Attributes["PauseTime"].ParseInt();
		_Effect.EffectTime = node.Attributes["EffectTime"].ParseInt();
		_Effect.AmplitudeX = node.Attributes["AmplitudeX"].ParseFloat();
		_Effect.AmplitudeY = node.Attributes["AmplitudeY"].ParseFloat();
		_Effect.FrequencyX = node.Attributes["FrequencyX"].ParseFloat();
		_Effect.FrequencyY = node.Attributes["FrequencyY"].ParseFloat();
	}
}
