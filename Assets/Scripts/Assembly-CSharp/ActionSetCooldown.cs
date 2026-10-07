using System.Xml;

public class ActionSetCooldown : ActionAnimation
{
	private int _Duration;

	private string _ButtonName;

	public int Duration
	{
		get
		{
			return GetDuration();
		}
	}

	public string CooldownButtonName
	{
		get
		{
			return GetButtonName();
		}
	}

	public ActionSetCooldown(XmlNode node)
		: base(ActionType.SET_COOLDOWN)
	{
		Parse(node);
	}

	public int GetDuration()
	{
		return _Duration;
	}

	public string GetButtonName()
	{
		return _ButtonName;
	}

	public override void Visit(Model model)
	{
		model.StartAction(this);
	}

	protected override void Parse(XmlNode node)
	{
		base.Parse(node);
		_Duration = node.Attributes["Duration"].ParseInt();
		_ButtonName = node.Attributes["Button"].GetStringOrDefault(string.Empty);
	}
}
