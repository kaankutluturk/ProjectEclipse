using System.Diagnostics;
using System.Xml;

public class PerkActionAddBullets : PerkAction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _bulletType;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private FunctionExtension _value;

	public PerkActionAddBullets()
	{
	}

	public PerkActionAddBullets(PerkActionAddBullets source)
		: base(source)
	{
		set_BulletType(source.GetBulletType());
		set_Value(source.GetValue());
	}

	public string GetBulletType()
	{
		return _bulletType;
	}

	protected void set_BulletType(string value)
	{
		_bulletType = value;
	}

	public FunctionExtension GetValue()
	{
		return _value;
	}

	protected void set_Value(FunctionExtension value)
	{
		_value = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_ADD_BULLETS);
		set_BulletType(node.Attributes["BulletType"].GetStringOrDefault(string.Empty));
		string text = node.Attributes["Value"].GetStringOrDefault(string.Empty);
		if (text != null && text != string.Empty)
		{
			set_Value(new FunctionExtension());
			GetValue().Parse(text);
			GetValue().SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
			GetValue().SetVariableCallback(GetPerk().OnFunctionPreCallback);
			GetValue().set_Target(this);
		}
	}
}
