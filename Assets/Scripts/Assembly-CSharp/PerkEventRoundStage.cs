using System.Diagnostics;
using System.Xml;

public class PerkEventRoundStage : PerkEvent
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _roundStage;

	public int RoundStageIndex
	{
		get
		{
			return GetRoundStageIndex();
		}
		protected set
		{
			set_RoundStage(value);
		}
	}

	public PerkEventRoundStage()
	{
	}

	public PerkEventRoundStage(PerkEventRoundStage source)
		: base(source)
	{
		set_RoundStage(source.GetRoundStageIndex());
	}

	public int GetRoundStageIndex()
	{
		return _roundStage;
	}

	protected void set_RoundStage(int value)
	{
		_roundStage = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_RoundStage(GetRoundStage(node.Attributes["Name"].GetStringOrDefault(string.Empty)));
	}

	public override bool IsEqual(EventStruct eventData)
	{
		if (!base.IsEqual(eventData))
		{
			return false;
		}
		int currentRoundStage = eventData.EventModel.RoundStage;
		if (GetRoundStageIndex() != 0 && GetRoundStageIndex() != currentRoundStage)
		{
			return false;
		}
		return true;
	}
}
