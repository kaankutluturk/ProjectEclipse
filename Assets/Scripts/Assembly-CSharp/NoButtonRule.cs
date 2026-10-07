using System.Xml;

public class NoButtonRule : Rule
{
	public enum NoButtonType
	{
		ButtonTypePunch = 0,
		ButtonTypeKick = 1,
		ButtonTypeRanged = 2,
		ButtonTypeMagic = 3,
		ButtonTypeRaidCharge = 4,
		ButtonTypeDefault = 5
	}

	private NoButtonType _buttonType;

	public NoButtonRule(XmlNode node)
		: base(RuleType.RuleNoButton, node)
	{
		_buttonType = ParseButtonType(node);
	}

	public NoButtonType GetButtonType()
	{
		return _buttonType;
	}

	private NoButtonType ParseButtonType(XmlNode node)
	{
		string text = node.Attributes["Name"].GetStringOrDefault(string.Empty);
		if (text != string.Empty)
		{
			switch (text)
			{
			case "Punch":
				return NoButtonType.ButtonTypePunch;
			case "Kick":
				return NoButtonType.ButtonTypeKick;
			case "Ranged":
				return NoButtonType.ButtonTypeRanged;
			case "Magic":
				return NoButtonType.ButtonTypeMagic;
			case "RaidCharge":
				return NoButtonType.ButtonTypeRaidCharge;
			}
		}
		GameLog.Error("Error - parseButtonType - unknown type");
		return NoButtonType.ButtonTypeDefault;
	}
}
