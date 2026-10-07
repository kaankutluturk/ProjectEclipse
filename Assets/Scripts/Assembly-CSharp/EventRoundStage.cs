using System.Xml;

public class EventRoundStage : EventAnimation
{
	private StageType.Stage stage;

	public StageType.Stage Stage
	{
		get
		{
			return GetStage();
		}
	}

	public EventRoundStage()
		: base(EventAnimationType.EVENT_ROUND_STAGE)
	{
		stage = StageType.Stage.STAGE_NONE;
	}

	public StageType.Stage GetStage()
	{
		return stage;
	}

	protected override bool Compare(EventAnimation other)
	{
		EventRoundStage otherEvent = other as EventRoundStage;
		bool flag = otherEvent.stage == stage;
		return (!IsNot) ? flag : (!flag);
	}

	protected override void Parse(XmlNode node)
	{
		stage = StageType.GetStageByName(AnimationName);
	}
}
