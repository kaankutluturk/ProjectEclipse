using System.Diagnostics;
using System.Xml;

internal class PerkActionSetModEffect : PerkAction
{
	public enum ModEffectType
	{
		EFFECT_NONE = 0,
		EFFECT_PULSE = 1
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _modName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ModEffectType _effectType;

	public string EffectModName
	{
		get
		{
			return GetModName();
		}
		protected set
		{
			set_ModName(value);
		}
	}

	public ModEffectType EffectType
	{
		get
		{
			return GetEffectType();
		}
		protected set
		{
			SetEffectType(value);
		}
	}

	public PerkActionSetModEffect()
	{
	}

	public PerkActionSetModEffect(PerkActionSetModEffect NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		set_ModName(NOLFMPDGCOC.GetModName());
		SetEffectType(NOLFMPDGCOC.GetEffectType());
	}

	public string GetModName()
	{
		return _modName;
	}

	protected void set_ModName(string value)
	{
		_modName = value;
	}

	public ModEffectType GetEffectType()
	{
		return _effectType;
	}

	protected void SetEffectType(ModEffectType value)
	{
		_effectType = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_MOD_EFFECT);
		set_ModName(node.Attributes["Name"].GetStringOrDefault(string.Empty));
		SetEffectType(ParseEffectType(node.Attributes["Type"].GetStringOrDefault(string.Empty)));
	}

	private ModEffectType ParseEffectType(string CNKBLODAFDO)
	{
		if (CNKBLODAFDO.Equals("Pulse"))
		{
			return ModEffectType.EFFECT_PULSE;
		}
		return ModEffectType.EFFECT_NONE;
	}
}
