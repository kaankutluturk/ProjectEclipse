using System.Diagnostics;
using System.Xml;

public class ModHealthChange : PerkActionModificator
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float perFrameValue;

	public float HealthPerFrame
	{
		get
		{
			return GetPerFrameValue();
		}
		protected set
		{
			set_PerFrameValue(value);
		}
	}

	public ModHealthChange()
	{
	}

	public ModHealthChange(ModHealthChange NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		set_PerFrameValue(NOLFMPDGCOC.GetPerFrameValue());
	}

	public float GetPerFrameValue()
	{
		return perFrameValue;
	}

	protected void set_PerFrameValue(float value)
	{
		perFrameValue = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_MOD_HEALTH_CHANGE);
		set_PerFrameValue(node.Attributes["PerFrameValue"].ParseFloat());
	}
}
