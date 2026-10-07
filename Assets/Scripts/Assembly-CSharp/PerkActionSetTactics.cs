using System.Diagnostics;
using System.Xml;

public class PerkActionSetTactics : PerkAction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _tactics;

	public string TacticsName
	{
		get
		{
			return GetTactics();
		}
		protected set
		{
			set_Tactics(value);
		}
	}

	public PerkActionSetTactics()
	{
	}

	public PerkActionSetTactics(PerkActionSetTactics source)
		: base(source)
	{
		set_Tactics(source.GetTactics());
	}

	public string GetTactics()
	{
		return _tactics;
	}

	protected void set_Tactics(string value)
	{
		_tactics = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_SET_TACTICS);
		set_Tactics(node.Attributes["Name"].GetStringOrDefault(string.Empty));
	}
}
