using System.Diagnostics;
using System.Xml;

public class PerkActionVariable : PerkActionModificator
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private FunctionExtension _value;

	public PerkActionVariable()
	{
	}

	public PerkActionVariable(PerkActionVariable NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		set_Value(NOLFMPDGCOC.GetValue());
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
		set_Type(ActionType.ACTION_VARIABLE);
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
