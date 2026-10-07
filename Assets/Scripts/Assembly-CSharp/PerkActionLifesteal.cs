using System.Diagnostics;
using System.Xml;

public class PerkActionLifesteal : PerkAction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float _damagePart;

	public float LifestealFraction
	{
		get
		{
			return GetDamagePart();
		}
		protected set
		{
			set_DamagePart(value);
		}
	}

	public PerkActionLifesteal()
	{
	}

	public PerkActionLifesteal(PerkActionLifesteal NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		set_DamagePart(NOLFMPDGCOC.GetDamagePart());
	}

	public float GetDamagePart()
	{
		return _damagePart;
	}

	protected void set_DamagePart(float value)
	{
		_damagePart = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_LIFE_STEAL);
		set_DamagePart(node.Attributes["DamagePart"].ParseFloat());
	}
}
