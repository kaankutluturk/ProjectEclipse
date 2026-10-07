using System.Diagnostics;
using System.Xml;

public class PerkActionProvoke : PerkAction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _provokeTrigger;

	public string ProvokeTrigger
	{
		get
		{
			return GetProvokeTrigger();
		}
		protected set
		{
			set_Trigger(value);
		}
	}

	public PerkActionProvoke()
	{
	}

	public PerkActionProvoke(PerkActionProvoke source)
		: base(source)
	{
		set_Trigger(source.GetProvokeTrigger());
	}

	public string GetProvokeTrigger()
	{
		return _provokeTrigger;
	}

	protected void set_Trigger(string value)
	{
		_provokeTrigger = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_PROVOKE);
		set_Trigger(node.Attributes["Trigger"].GetStringOrDefault(string.Empty));
	}
}
