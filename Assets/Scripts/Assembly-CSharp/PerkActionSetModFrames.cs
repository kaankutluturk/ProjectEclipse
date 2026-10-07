using System.Diagnostics;
using System.Xml;

public class PerkActionSetModFrames : PerkAction
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string _modName;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private FunctionExtension _modFrames;

	public string FramesModName
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

	public FunctionExtension ModFramesFunction
	{
		get
		{
			return GetModFrames();
		}
		protected set
		{
			SetModFrames(value);
		}
	}

	public PerkActionSetModFrames()
	{
	}

	public PerkActionSetModFrames(PerkActionSetModFrames source)
		: base(source)
	{
		set_ModName(source.GetModName());
		SetModFrames(source.GetModFrames());
	}

	public string GetModName()
	{
		return _modName;
	}

	protected void set_ModName(string value)
	{
		_modName = value;
	}

	public FunctionExtension GetModFrames()
	{
		return _modFrames;
	}

	protected void SetModFrames(FunctionExtension value)
	{
		_modFrames = value;
	}

	public override void Parse(XmlNode node)
	{
		base.Parse(node);
		set_Type(ActionType.ACTION_SET_MOD_FRAMES);
		set_ModName(node.Attributes["Name"].GetStringOrDefault(string.Empty));
		SetModFrames(null);
		string text = node.Attributes["Frames"].GetStringOrDefault(string.Empty);
		if (text != null && text != string.Empty)
		{
			SetModFrames(new FunctionExtension());
			GetModFrames().Parse(text);
			GetModFrames().SetFunctionCallback(GetPerk().EvaluateFunctionCallback);
			GetModFrames().SetVariableCallback(GetPerk().OnFunctionPreCallback);
			GetModFrames().set_Target(this);
		}
	}
}
