using System.Diagnostics;
using System.Xml;

public class PerkActionSetCooldown : PerkAction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _cooldownFrames;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _buttonName;

	public new int CooldownFrames
	{
		get
		{
			return GetCooldownFrames();
		}
		protected set
		{
			set_Frames(value);
		}
	}

	public string CooldownButtonName
	{
		get
		{
			return GetButtonName();
		}
		protected set
		{
			set_ButtonName(value);
		}
	}

	public PerkActionSetCooldown()
	{
	}

	public PerkActionSetCooldown(PerkActionSetCooldown NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		set_Frames(NOLFMPDGCOC.GetCooldownFrames());
		set_ButtonName(NOLFMPDGCOC.GetButtonName());
	}

	public new int GetCooldownFrames()
	{
		return _cooldownFrames;
	}

	protected void set_Frames(int value)
	{
		_cooldownFrames = value;
	}

	public string GetButtonName()
	{
		return _buttonName;
	}

	protected void set_ButtonName(string value)
	{
		_buttonName = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_SET_COOLDOWN);
		set_Frames(node.Attributes["Frames"].ParseInt());
		set_ButtonName(node.Attributes["Button"].GetStringOrDefault(string.Empty));
	}
}
