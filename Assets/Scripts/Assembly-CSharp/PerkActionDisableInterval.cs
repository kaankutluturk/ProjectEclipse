using System.Diagnostics;
using System.Xml;

public class PerkActionDisableInterval : PerkAction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _intervalName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _intervalType;

	public string IntervalName
	{
		get
		{
			return GetIntervalName();
		}
		protected set
		{
			SetIntervalName(value);
		}
	}

	public string IntervalType
	{
		get
		{
			return GetIntervalType();
		}
		protected set
		{
			SetIntervalType(value);
		}
	}

	public PerkActionDisableInterval()
	{
	}

	public PerkActionDisableInterval(PerkActionDisableInterval NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		SetIntervalName(NOLFMPDGCOC.GetIntervalName());
		SetIntervalType(NOLFMPDGCOC.GetIntervalType());
	}

	public string GetIntervalName()
	{
		return _intervalName;
	}

	protected void SetIntervalName(string value)
	{
		_intervalName = value;
	}

	public string GetIntervalType()
	{
		return _intervalType;
	}

	protected void SetIntervalType(string value)
	{
		_intervalType = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_DISABLE_INTERVAL);
		SetIntervalName(node.Attributes["IntervalName"].GetStringOrDefault(string.Empty));
		SetIntervalType(node.Attributes["IntervalType"].GetStringOrDefault(string.Empty));
	}
}
