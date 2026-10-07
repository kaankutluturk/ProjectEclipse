using System.Diagnostics;
using System.Xml;

public class PerkActionChangeAdditionalDamageValue : PerkActionModificator
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private float _additionalDamageValue;

	public float AdditionalDamageAmount
	{
		get
		{
			return GetAdditionalDamageValue();
		}
		protected set
		{
			set_AdditionalDamageValue(value);
		}
	}

	public PerkActionChangeAdditionalDamageValue()
	{
		set_AdditionalDamageValue(0f);
	}

	public PerkActionChangeAdditionalDamageValue(PerkActionChangeAdditionalDamageValue NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		set_AdditionalDamageValue(NOLFMPDGCOC.GetAdditionalDamageValue());
	}

	public float GetAdditionalDamageValue()
	{
		return _additionalDamageValue;
	}

	protected void set_AdditionalDamageValue(float value)
	{
		_additionalDamageValue = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_CHANGE_ADD_DAMAGE_VALUE);
		set_AdditionalDamageValue(node.Attributes["Value"].ParseFloat());
	}
}
