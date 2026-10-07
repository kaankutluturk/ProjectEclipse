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

	protected override bool Compare(EventAnimation FOPOKALJIIJ)
	{
		EventRoundStage gBIJAGPBADA = FOPOKALJIIJ as EventRoundStage;
		bool flag = gBIJAGPBADA.stage == stage;
		return (!IsNot) ? flag : (!flag);
	}

	protected override void Parse(XmlNode MEEAKLDGLDF)
	{
		stage = StageType.GetStageByName(AnimationName);
	}
}
