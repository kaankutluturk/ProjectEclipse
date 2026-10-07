using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

public class PerkEventAnimationStart : PerkEvent
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _name;

	public PerkEventAnimationStart()
	{
	}

	public PerkEventAnimationStart(PerkEventAnimationStart source)
		: base(source)
	{
		set_Name(source.get_Name());
	}

	public string get_Name()
	{
		return _name;
	}

	protected void set_Name(string value)
	{
		_name = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Name(node.Attributes["Name"].GetStringOrDefault(string.Empty));
	}

	public override bool IsEqual(EventStruct eventData)
	{
		if (!base.IsEqual(eventData) || eventData == null || eventData.Info == null)
		{
			return false;
		}
		Dictionary<string, object> dictionary = (Dictionary<string, object>)eventData.Info;
		InfoAnimation animation = ((!dictionary.ContainsKey("Animation")) ? null : ((InfoAnimation)dictionary["Animation"]));
		if (get_Name() != string.Empty && (animation == null || !animation.HasName(get_Name())))
		{
			return false;
		}
		return true;
	}
}
