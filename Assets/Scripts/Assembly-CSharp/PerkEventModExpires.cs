using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class PerkEventModExpires : PerkEvent
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _modName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _namespace;

	public string ExpiredModName
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

	public PerkEventModExpires()
	{
	}

	public PerkEventModExpires(PerkEventModExpires source)
		: base(source)
	{
		set_ModName(source.GetModName());
		set_Namespace(source.GetNamespace());
	}

	public string GetModName()
	{
		return _modName;
	}

	protected void set_ModName(string value)
	{
		_modName = value;
	}

	public string GetNamespace()
	{
		return _namespace;
	}

	protected void set_Namespace(string value)
	{
		_namespace = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_ModName(node.Attributes["Name"].GetStringOrDefault(string.Empty));
		set_Namespace(node.Attributes["Namespace"].GetStringOrDefault(string.Empty));
	}

	public override bool IsEqual(EventStruct eventData)
	{
		if (GetNamespace() == null || GetNamespace() == string.Empty)
		{
			if (!base.IsEqual(eventData) || eventData == null || eventData.Info == null)
			{
				return false;
			}
		}
		else if (GetNamespace().Equals(eventData.Namespace))
		{
			return false;
		}
		Dictionary<string, object> dictionary = (Dictionary<string, object>)eventData.Info;
		if (dictionary != null)
		{
			string value = ((!dictionary.ContainsKey("ModExpires")) ? null : ((string)dictionary["ModExpires"]));
			PerkInfoItem eventPerk = ((!dictionary.ContainsKey("ParentPerk")) ? null : ((PerkInfoItem)dictionary["ParentPerk"]));
			if (eventPerk == GetPerk() && (GetModName() == null || GetModName() == string.Empty || GetModName().Equals(value)))
			{
				return true;
			}
		}
		return false;
	}
}
