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

	public PerkEventRoundStage(PerkEventRoundStage NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		set_RoundStage(NOLFMPDGCOC.GetRoundStageIndex());
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

	public override bool IsEqual(EventStruct EJMEALJNNIL)
	{
		if (!base.IsEqual(EJMEALJNNIL))
		{
			return false;
		}
		int jMHJDHLBHLK = EJMEALJNNIL.EventModel.RoundStage;
		if (GetRoundStageIndex() != 0 && GetRoundStageIndex() != jMHJDHLBHLK)
		{
			return false;
		}
		return true;
	}
}
