using System.Globalization;
using YamlDotNet.Core;

public class AnchorAlias : ParsingEvent
{
	private readonly string value;

	public string Value
	{
		get
		{
			return GetValue();
		}
	}

	public AnchorAlias(string value, Mark start, Mark end)
		: base(start, end)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new YamlException(start, end, "Anchor value must not be empty.");
		}
		if (!NodeEvent.anchorValidator.IsMatch(value))
		{
			throw new YamlException(start, end, "Anchor value must contain alphanumerical characters only.");
		}
		this.value = value;
	}

	public AnchorAlias(string value)
		: this(value, Mark.Empty, Mark.Empty)
	{
	}

	internal override ParsingEventType get_Type()
	{
		return ParsingEventType.Alias;
	}

	public string GetValue()
	{
		return value;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "Alias [value = {0}]", value);
	}

	public override void Accept(IParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
