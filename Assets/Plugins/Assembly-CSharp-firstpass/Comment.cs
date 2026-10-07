using System.Diagnostics;
using YamlDotNet.Core;

public class Comment : ParsingEvent
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string commentValue;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private bool isInline;

	public bool IsInlineComment
	{
		get
		{
			return GetIsInline();
		}
		private set
		{
			set_IsInline(value);
		}
	}

	public Comment(string value, bool isInline)
		: this(value, isInline, Mark.Empty, Mark.Empty)
	{
	}

	public Comment(string value, bool isInline, Mark start, Mark end)
		: base(start, end)
	{
		set_Value(value);
		set_IsInline(isInline);
	}

	public string GetValue()
	{
		return commentValue;
	}

	private void set_Value(string value)
	{
		commentValue = value;
	}

	public bool GetIsInline()
	{
		return isInline;
	}

	private void set_IsInline(bool value)
	{
		isInline = value;
	}

	internal override ParsingEventType get_Type()
	{
		return ParsingEventType.Comment;
	}

	public override void Accept(IParsingEventVisitor visitor)
	{
		visitor.Visit(this);
	}
}
