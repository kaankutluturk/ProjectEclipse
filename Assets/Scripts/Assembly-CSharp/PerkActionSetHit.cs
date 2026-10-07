using System.Diagnostics;
using System.Xml;

public class PerkActionSetHit : PerkAction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _critical;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _block;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _shock;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _disarm;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private FunctionExtension _damage;

	public int Critical
	{
		get
		{
			return GetCritical();
		}
		protected set
		{
			SetCritical(value);
		}
	}

	public int Block
	{
		get
		{
			return GetBlock();
		}
		protected set
		{
			SetBlock(value);
		}
	}

	public int Shock
	{
		get
		{
			return GetShock();
		}
		protected set
		{
			SetShock(value);
		}
	}

	public int Disarm
	{
		get
		{
			return GetDisarm();
		}
		protected set
		{
			SetDisarm(value);
		}
	}

	public FunctionExtension DamageFunction
	{
		get
		{
			return GetDamage();
		}
		protected set
		{
			SetDamage(value);
		}
	}

	public PerkActionSetHit()
	{
	}

	public PerkActionSetHit(PerkActionSetHit source)
		: base(source)
	{
		SetCritical(source.GetCritical());
		SetBlock(source.GetBlock());
		SetShock(source.GetShock());
		SetDisarm(source.GetDisarm());
		SetDamage(source.GetDamage());
	}

	public int GetCritical()
	{
		return _critical;
	}

	protected void SetCritical(int value)
	{
		_critical = value;
	}

	public int GetBlock()
	{
		return _block;
	}

	protected void SetBlock(int value)
	{
		_block = value;
	}

	public int GetShock()
	{
		return _shock;
	}

	protected void SetShock(int value)
	{
		_shock = value;
	}

	public int GetDisarm()
	{
		return _disarm;
	}

	protected void SetDisarm(int value)
	{
		_disarm = value;
	}

	public FunctionExtension GetDamage()
	{
		return _damage;
	}

	protected void SetDamage(FunctionExtension value)
	{
		_damage = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_SET_HIT);
		SetCritical(node.Attributes["Critical"].ParseInt(-1));
		SetBlock(node.Attributes["Block"].ParseInt(-1));
		SetShock(node.Attributes["Shock"].ParseInt(-1));
		SetDisarm(node.Attributes["Disarm"].ParseInt(-1));
		SetDamage(null);
		string text = node.Attributes["Damage"].GetStringOrDefault(string.Empty);
		if (text != null && text != string.Empty)
		{
			SetDamage(new FunctionExtension());
			GetDamage().Parse(text);
			GetDamage().SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
			GetDamage().SetVariableCallback(GetPerk().OnFunctionPreCallback);
			GetDamage().set_Target(this);
		}
	}
}
