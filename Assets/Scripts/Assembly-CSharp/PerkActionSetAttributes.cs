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

	public PerkActionSetAttributes(PerkActionSetAttributes NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		SetAttributes(NOLFMPDGCOC.GetAttributes());
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
				string bLLCOEAOJGF = xmlAttribute.GetStringOrDefault(string.Empty);
				FunctionExtension oPIFBDJNMKD = new FunctionExtension();
				oPIFBDJNMKD.Parse(bLLCOEAOJGF);
				oPIFBDJNMKD.SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
				oPIFBDJNMKD.SetVariableCallback(GetPerk().OnFunctionPreCallback);
				oPIFBDJNMKD.set_Target(this);
				GetAttributes()[key] = oPIFBDJNMKD;
			}
		}
	}
}
