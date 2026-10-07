using System.Diagnostics;
using System.Xml;

public class PerkActionChangeImpulse : PerkAction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float _multiplierX;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float _multiplierY;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float _multiplierZ;

	public float ImpulseMultiplierX
	{
		get
		{
			return GetMultiplierX();
		}
		protected set
		{
			SetMultiplierX(value);
		}
	}

	public float ImpulseMultiplierY
	{
		get
		{
			return GetMultiplierY();
		}
		protected set
		{
			SetMultiplierY(value);
		}
	}

	public float ImpulseMultiplierZ
	{
		get
		{
			return GetMultiplierZ();
		}
		protected set
		{
			SetMultiplierZ(value);
		}
	}

	public PerkActionChangeImpulse()
	{
		SetMultiplierX(1f);
		SetMultiplierY(1f);
		SetMultiplierZ(1f);
	}

	public PerkActionChangeImpulse(PerkActionChangeImpulse NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		SetMultiplierX(NOLFMPDGCOC.GetMultiplierX());
		SetMultiplierY(NOLFMPDGCOC.GetMultiplierY());
		SetMultiplierZ(NOLFMPDGCOC.GetMultiplierZ());
	}

	public float GetMultiplierX()
	{
		return _multiplierX;
	}

	protected void SetMultiplierX(float value)
	{
		_multiplierX = value;
	}

	public float GetMultiplierY()
	{
		return _multiplierY;
	}

	protected void SetMultiplierY(float value)
	{
		_multiplierY = value;
	}

	public float GetMultiplierZ()
	{
		return _multiplierZ;
	}

	protected void SetMultiplierZ(float value)
	{
		_multiplierZ = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_CHANGE_IMPULSE);
		SetMultiplierX(node.Attributes["MultiplierX"].ParseFloat());
		SetMultiplierY(node.Attributes["MultiplierY"].ParseFloat());
		SetMultiplierZ(node.Attributes["MultiplierZ"].ParseFloat());
	}
}
