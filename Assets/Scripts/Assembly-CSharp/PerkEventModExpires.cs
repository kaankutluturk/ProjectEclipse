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

	public PerkEventModExpires(PerkEventModExpires NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		set_ModName(NOLFMPDGCOC.GetModName());
		set_Namespace(NOLFMPDGCOC.GetNamespace());
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

	public override bool IsEqual(EventStruct EJMEALJNNIL)
	{
		if (GetNamespace() == null || GetNamespace() == string.Empty)
		{
			if (!base.IsEqual(EJMEALJNNIL) || EJMEALJNNIL == null || EJMEALJNNIL.Info == null)
			{
				return false;
			}
		}
		else if (GetNamespace().Equals(EJMEALJNNIL.Namespace))
		{
			return false;
		}
		Dictionary<string, object> dictionary = (Dictionary<string, object>)EJMEALJNNIL.Info;
		if (dictionary != null)
		{
			string value = ((!dictionary.ContainsKey("ModExpires")) ? null : ((string)dictionary["ModExpires"]));
			PerkInfoItem aCONCDFDNJH = ((!dictionary.ContainsKey("ParentPerk")) ? null : ((PerkInfoItem)dictionary["ParentPerk"]));
			if (aCONCDFDNJH == GetPerk() && (GetModName() == null || GetModName() == string.Empty || GetModName().Equals(value)))
			{
				return true;
			}
		}
		return false;
	}
}
