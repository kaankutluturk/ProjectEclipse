using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class PerkEventPostHit : PerkEvent
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _defense;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _animation;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _block;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _critical;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _shock;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float _damageMin;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float _damageMax;

	public string DefenseName
	{
		get
		{
			return GetDefense();
		}
		protected set
		{
			SetDefense(value);
		}
	}

	public string AnimationName
	{
		get
		{
			return GetAnimation();
		}
		protected set
		{
			SetAnimation(value);
		}
	}

	public int BlockFilter
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

	public int CriticalFilter
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

	public int ShockFilter
	{
		get
		{
			return GetIsShock();
		}
		protected set
		{
			set_IsShock(value);
		}
	}

	public float MinDamage
	{
		get
		{
			return GetDamageMin();
		}
		protected set
		{
			SetDamageMin(value);
		}
	}

	public float MaxDamage
	{
		get
		{
			return GetDamageMax();
		}
		protected set
		{
			SetDamageMax(value);
		}
	}

	public PerkEventPostHit()
	{
	}

	public PerkEventPostHit(PerkEventPostHit NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		SetDefense(NOLFMPDGCOC.GetDefense());
		SetAnimation(NOLFMPDGCOC.GetAnimation());
		SetBlock(NOLFMPDGCOC.GetBlock());
		SetCritical(NOLFMPDGCOC.GetCritical());
		set_IsShock(NOLFMPDGCOC.GetIsShock());
		SetDamageMin(NOLFMPDGCOC.GetDamageMin());
		SetDamageMax(NOLFMPDGCOC.GetDamageMax());
	}

	public string GetDefense()
	{
		return _defense;
	}

	protected void SetDefense(string value)
	{
		_defense = value;
	}

	public string GetAnimation()
	{
		return _animation;
	}

	protected void SetAnimation(string value)
	{
		_animation = value;
	}

	public int GetBlock()
	{
		return _block;
	}

	protected void SetBlock(int value)
	{
		_block = value;
	}

	public int GetCritical()
	{
		return _critical;
	}

	protected void SetCritical(int value)
	{
		_critical = value;
	}

	public int GetIsShock()
	{
		return _shock;
	}

	protected void set_IsShock(int value)
	{
		_shock = value;
	}

	public float GetDamageMin()
	{
		return _damageMin;
	}

	protected void SetDamageMin(float value)
	{
		_damageMin = value;
	}

	public float GetDamageMax()
	{
		return _damageMax;
	}

	protected void SetDamageMax(float value)
	{
		_damageMax = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		SetDefense(node.Attributes["Defense"].GetStringOrDefault(string.Empty));
		SetBlock(node.Attributes["Block"].ParseInt(-1));
		SetCritical(node.Attributes["Critical"].ParseInt(-1));
		set_IsShock(node.Attributes["Shock"].ParseInt(-1));
		SetAnimation(node.Attributes["Animation"].GetStringOrDefault(string.Empty));
		SetDamageMin(node.Attributes["DamageMin"].ParseFloat(-1f));
		SetDamageMax(node.Attributes["DamageMax"].ParseFloat(-1f));
	}

	public override bool IsEqual(EventStruct EJMEALJNNIL)
	{
		if (!base.IsEqual(EJMEALJNNIL) || EJMEALJNNIL == null || EJMEALJNNIL.Info == null)
		{
			return false;
		}
		Dictionary<string, object> dictionary = (Dictionary<string, object>)EJMEALJNNIL.Info;
		InfoAnimation pJAHIOELGGD = ((!dictionary.ContainsKey("Animation")) ? null : ((InfoAnimation)dictionary["Animation"]));
		string text = ((!dictionary.ContainsKey("Defense")) ? null : ((string)dictionary["Defense"]));
		bool flag = dictionary.ContainsKey("Critical") && (bool)dictionary["Critical"];
		bool flag2 = dictionary.ContainsKey("Shock") && (bool)dictionary["Shock"];
		bool flag3 = dictionary.ContainsKey("Block") && (bool)dictionary["Block"];
		float num = ((!dictionary.ContainsKey("Damage")) ? 0f : ((float)dictionary["Damage"]));
		if (GetDefense() != string.Empty && GetDefense() != text)
		{
			return false;
		}
		if (GetAnimation() != null && !GetAnimation().Equals(string.Empty) && (pJAHIOELGGD == null || !pJAHIOELGGD.HasName(GetAnimation())))
		{
			return false;
		}
		if (GetCritical() > -1 && GetCritical() != (flag ? 1 : 0))
		{
			return false;
		}
		if (GetIsShock() > -1 && GetIsShock() != (flag2 ? 1 : 0))
		{
			return false;
		}
		if (GetBlock() > -1 && GetBlock() != (flag3 ? 1 : 0))
		{
			return false;
		}
		if (GetDamageMin() > -1f && num < GetDamageMin())
		{
			return false;
		}
		if (GetDamageMax() > -1f && num > GetDamageMax())
		{
			return false;
		}
		return true;
	}
}
