using System.Diagnostics;
using System.Xml;

public class PerkActionClearAction : PerkAction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _actionName;

	public string ClearedActionName
	{
		get
		{
			return GetNameAction();
		}
		protected set
		{
			set_NameAction(value);
		}
	}

	public PerkActionClearAction()
	{
	}

	public PerkActionClearAction(PerkActionClearAction NOLFMPDGCOC)
		: base(NOLFMPDGCOC)
	{
		set_NameAction(NOLFMPDGCOC.GetNameAction());
	}

	public string GetNameAction()
	{
		return _actionName;
	}

	protected void set_NameAction(string value)
	{
		_actionName = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_CLEAR_ACTION);
		set_NameAction(node.Attributes["Name"].GetStringOrDefault(string.Empty));
	}
}
