using System.Diagnostics;
using System.Xml;

public class PerkActionChangeHitEffectScale : PerkAction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float _hitEffectScale;

	public float EffectScale
	{
		get
		{
			return GetHitEffectScale();
		}
		protected set
		{
			set_HitEffectScale(value);
		}
	}

	public PerkActionChangeHitEffectScale()
	{
		set_HitEffectScale(1f);
	}

	public PerkActionChangeHitEffectScale(PerkActionChangeHitEffectScale source)
		: base(source)
	{
		set_HitEffectScale(source.GetHitEffectScale());
	}

	public float GetHitEffectScale()
	{
		return _hitEffectScale;
	}

	protected void set_HitEffectScale(float value)
	{
		_hitEffectScale = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_CHANGE_HIT_EFFECT_SCALE);
		set_HitEffectScale(node.Attributes["Scale"].ParseFloat());
	}
}
