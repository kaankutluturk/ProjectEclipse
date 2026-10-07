using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Tokens;

public class DocumentStart : ParsingEvent
{
	private readonly TagDirectiveCollection tags;

	private readonly VersionDirective version;

	private readonly bool isImplicit;

	public override int NestingIncrease
	{
		get
		{
			return GetNestingIncrease();
		}
	}

	public TagDirectiveCollection Tags
	{
		get
		{
			return GetTags();
		}
	}

	public VersionDirective Version
	{
		get
		{
			return GetVersion();
		}
	}

	public bool IsImplicit
	{
		get
		{
			return GetIsImplicit();
		}
	}

	public DocumentStart(VersionDirective version, TagDirectiveCollection CPAIGLNDIOK, bool isImplicit, Mark ILENLCMAMBH, Mark PCLFFOBJJFO)
		: base(ILENLCMAMBH, PCLFFOBJJFO)
	{
		this.version = version;
		this.tags = CPAIGLNDIOK;
		this.isImplicit = isImplicit;
	}

	public DocumentStart(VersionDirective version, TagDirectiveCollection CPAIGLNDIOK, bool isImplicit)
		: this(version, CPAIGLNDIOK, isImplicit, Mark.Empty, Mark.Empty)
	{
	}

	public DocumentStart(Mark ILENLCMAMBH, Mark PCLFFOBJJFO)
		: this(null, null, true, ILENLCMAMBH, PCLFFOBJJFO)
	{
	}

	public DocumentStart()
		: this(null, null, true, Mark.Empty, Mark.Empty)
	{
	}

	public override int GetNestingIncrease()
	{
		return 1;
	}

	internal override ParsingEventType get_Type()
	{
		return ParsingEventType.DocumentStart;
	}

	public TagDirectiveCollection GetTags()
	{
		return tags;
	}

	public VersionDirective GetVersion()
	{
		return version;
	}

	public bool GetIsImplicit()
	{
		return isImplicit;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "Document start [isImplicit = {0}]", isImplicit);
	}

	public override void Accept(IParsingEventVisitor NKECMANOOEM)
	{
		NKECMANOOEM.Visit(this);
	}
}
