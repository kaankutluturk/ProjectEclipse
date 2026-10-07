using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class PerkActionSetAttributes : PerkActionModificator
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Dictionary<string, FunctionExtension> _attributes;

	public Dictionary<string, FunctionExtension> AttributeValues
	{
		get
		{
			return GetAttributes();
		}
		protected set
		{
			SetAttributes(value);
		}
	}

	public PerkActionSetAttributes()
	{
	}

	public PerkActionSetAttributes(PerkActionSetAttributes source)
		: base(source)
	{
		SetAttributes(source.GetAttributes());
	}

	public Dictionary<string, FunctionExtension> GetAttributes()
	{
		return _attributes;
	}

	protected void SetAttributes(Dictionary<string, FunctionExtension> value)
	{
		_attributes = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_SET_ATTRIBUTES);
		SetAttributes(new Dictionary<string, FunctionExtension>());
		foreach (WarriorAttribute item in GameUtils.WarriorAttributeList.AttributeList)
		{
			XmlAttribute xmlAttribute = node.Attributes[item.get_Name()];
			if (xmlAttribute != null)
			{
				string key = item.get_Name();
				string expression = xmlAttribute.GetStringOrDefault(string.Empty);
				FunctionExtension functionExtension = new FunctionExtension();
				functionExtension.Parse(expression);
				functionExtension.SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
				functionExtension.SetVariableCallback(GetPerk().OnFunctionPreCallback);
				functionExtension.set_Target(this);
				GetAttributes()[key] = functionExtension;
			}
		}
	}
}
