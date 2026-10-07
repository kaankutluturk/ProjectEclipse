using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Tokens;

public class Emitter : IEmitter
{
	private class AnchorData
	{
		public string anchor;

		public bool isAlias;
	}

	private class TagData
	{
		public string handle;

		public string suffix;
	}

	private class ScalarData
	{
		public string value;

		public bool isMultiline;

		public bool isFlowPlainAllowed;

		public bool isBlockPlainAllowed;

		public bool isSingleQuotedAllowed;

		public bool isBlockAllowed;

		public ScalarStyle style;
	}

	private const int MinBestIndent = 4;

	private const int MaxBestIndent = 9;

	private const int MaxAliasLength = 128;

	private static readonly Regex uriReplacer = new Regex("[^0-9A-Za-z_\\-;?@=$~\\\\\\)\\]/:&+,\\.\\*\\(\\[!]", RegexOptions.Singleline);

	private readonly TextWriter output;

	private readonly bool isCanonical;

	private readonly int bestIndent;

	private readonly int bestWidth;

	private EmitterStateKind state;

	private readonly Stack<EmitterStateKind> states = new Stack<EmitterStateKind>();

	private readonly Queue<ParsingEvent> events = new Queue<ParsingEvent>();

	private readonly Stack<int> indents = new Stack<int>();

	private readonly TagDirectiveCollection tagDirectives = new TagDirectiveCollection();

	private int indent;

	private int flowLevel;

	private bool isMappingContext;

	private bool isSimpleKeyContext;

	private bool isRootContext;

	private int column;

	private bool isWhitespace;

	private bool isIndentation;

	private bool isOpenEnded;

	private bool isDocumentEndWritten;

	private readonly AnchorData anchorData = new AnchorData();

	private readonly TagData tagData = new TagData();

	private readonly ScalarData scalarData = new ScalarData();

	public Emitter(TextWriter output)
		: this(output, 4)
	{
	}

	public Emitter(TextWriter output, int bestIndent)
		: this(output, bestIndent, int.MaxValue)
	{
	}

	public Emitter(TextWriter output, int bestIndent, int bestWidth)
		: this(output, bestIndent, bestWidth, false)
	{
	}

	public Emitter(TextWriter output, int bestIndent, int bestWidth, bool isCanonical)
	{
		if (bestIndent < 4 || bestIndent > 9)
		{
			throw new ArgumentOutOfRangeException("bestIndent", string.Format(CultureInfo.InvariantCulture, "The bestIndent parameter must be between {0} and {1}.", 4, 9));
		}
		this.bestIndent = bestIndent;
		if (bestWidth <= bestIndent * 2)
		{
			throw new ArgumentOutOfRangeException("bestWidth", "The bestWidth parameter must be greater than bestIndent * 2.");
		}
		this.bestWidth = bestWidth;
		this.isCanonical = isCanonical;
		this.output = output;
	}

	public void Emit(ParsingEvent evt)
	{
		events.Enqueue(evt);
		while (!NeedMoreEvents())
		{
			ParsingEvent nextEvent = events.Peek();
			try
			{
				AnalyzeEvent(nextEvent);
				StateMachine(nextEvent);
			}
			finally
			{
				events.Dequeue();
			}
		}
	}

	private bool NeedMoreEvents()
	{
		if (events.Count == 0)
		{
			return true;
		}
		int num;
		switch (events.Peek().get_Type())
		{
		case ParsingEventType.DocumentStart:
			num = 1;
			break;
		case ParsingEventType.SequenceStart:
			num = 2;
			break;
		case ParsingEventType.MappingStart:
			num = 3;
			break;
		default:
			return false;
		}
		if (events.Count > num)
		{
			return false;
		}
		int num2 = 0;
		foreach (ParsingEvent item in events)
		{
			switch (item.get_Type())
			{
			case ParsingEventType.DocumentStart:
			case ParsingEventType.SequenceStart:
			case ParsingEventType.MappingStart:
				num2++;
				break;
			case ParsingEventType.DocumentEnd:
			case ParsingEventType.SequenceEnd:
			case ParsingEventType.MappingEnd:
				num2--;
				break;
			}
			if (num2 == 0)
			{
				return false;
			}
		}
		return true;
	}

	private void AnalyzeEvent(ParsingEvent evt)
	{
		anchorData.anchor = null;
		tagData.handle = null;
		tagData.suffix = null;
		AnchorAlias alias = evt as AnchorAlias;
		if (alias != null)
		{
			AnalyzeAnchor(alias.GetValue(), true);
			return;
		}
		NodeEvent nodeEvent = evt as NodeEvent;
		if (nodeEvent != null)
		{
			Scalar scalar = evt as Scalar;
			if (scalar != null)
			{
				AnalyzeScalar(scalar.GetValue());
			}
			AnalyzeAnchor(nodeEvent.GetAnchor(), false);
			if (!string.IsNullOrEmpty(nodeEvent.GetTag()) && (isCanonical || nodeEvent.GetIsCanonical()))
			{
				AnalyzeTag(nodeEvent.GetTag());
			}
		}
	}

	private void AnalyzeAnchor(string anchor, bool isAlias)
	{
		anchorData.anchor = anchor;
		anchorData.isAlias = isAlias;
	}

	private void AnalyzeScalar(string value)
	{
		scalarData.value = value;
		if (value.Length == 0)
		{
			scalarData.isMultiline = false;
			scalarData.isFlowPlainAllowed = false;
			scalarData.isBlockPlainAllowed = true;
			scalarData.isSingleQuotedAllowed = true;
			scalarData.isBlockAllowed = false;
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (value.StartsWith("---", StringComparison.Ordinal) || value.StartsWith("...", StringComparison.Ordinal))
		{
			flag = true;
			flag2 = true;
		}
		CharacterAnalyzer<StringLookAheadBuffer> characterAnalyzer = new CharacterAnalyzer<StringLookAheadBuffer>(new StringLookAheadBuffer(value));
		bool flag3 = true;
		bool flag4 = characterAnalyzer.IsWhiteBreakOrZero(1);
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		bool flag9 = false;
		bool flag10 = false;
		bool flag11 = false;
		bool flag12 = false;
		bool flag13 = false;
		bool flag14 = false;
		bool flag15 = true;
		while (!characterAnalyzer.EndOfInput)
		{
			if (flag15)
			{
				if (characterAnalyzer.Check("#,[]{}&*!|>\\\"%@`"))
				{
					flag = true;
					flag2 = true;
				}
				if (characterAnalyzer.Check("?:"))
				{
					flag = true;
					if (flag4)
					{
						flag2 = true;
					}
				}
				if (characterAnalyzer.Check('-') && flag4)
				{
					flag = true;
					flag2 = true;
				}
			}
			else
			{
				if (characterAnalyzer.Check(",?[]{}"))
				{
					flag = true;
				}
				if (characterAnalyzer.Check(':'))
				{
					flag = true;
					if (flag4)
					{
						flag2 = true;
					}
				}
				if (characterAnalyzer.Check('#') && flag3)
				{
					flag = true;
					flag2 = true;
				}
			}
			if (!characterAnalyzer.IsPrintable() || (!characterAnalyzer.IsAscii() && !IsUnicode(output.Encoding)))
			{
				flag14 = true;
			}
			if (characterAnalyzer.IsBreak())
			{
				flag13 = true;
			}
			if (characterAnalyzer.IsSpace())
			{
				if (flag15)
				{
					flag5 = true;
				}
				if (characterAnalyzer.Buffer.Position >= characterAnalyzer.Buffer.Length - 1)
				{
					flag7 = true;
				}
				if (flag12)
				{
					flag9 = true;
				}
				flag11 = true;
				flag12 = false;
			}
			else if (characterAnalyzer.IsBreak())
			{
				if (flag15)
				{
					flag6 = true;
				}
				if (characterAnalyzer.Buffer.Position >= characterAnalyzer.Buffer.Length - 1)
				{
					flag8 = true;
				}
				if (flag11)
				{
					flag10 = true;
				}
				flag11 = false;
				flag12 = true;
			}
			else
			{
				flag11 = false;
				flag12 = false;
			}
			flag3 = characterAnalyzer.IsWhiteBreakOrZero();
			characterAnalyzer.Skip(1);
			if (!characterAnalyzer.EndOfInput)
			{
				flag4 = characterAnalyzer.IsWhiteBreakOrZero(1);
			}
			flag15 = false;
		}
		scalarData.isFlowPlainAllowed = true;
		scalarData.isBlockPlainAllowed = true;
		scalarData.isSingleQuotedAllowed = true;
		scalarData.isBlockAllowed = true;
		if (flag5 || flag6 || flag7 || flag8)
		{
			scalarData.isFlowPlainAllowed = false;
			scalarData.isBlockPlainAllowed = false;
		}
		if (flag7)
		{
			scalarData.isBlockAllowed = false;
		}
		if (flag9)
		{
			scalarData.isFlowPlainAllowed = false;
			scalarData.isBlockPlainAllowed = false;
			scalarData.isSingleQuotedAllowed = false;
		}
		if (flag10 || flag14)
		{
			scalarData.isFlowPlainAllowed = false;
			scalarData.isBlockPlainAllowed = false;
			scalarData.isSingleQuotedAllowed = false;
			scalarData.isBlockAllowed = false;
		}
		scalarData.isMultiline = flag13;
		if (flag13)
		{
			scalarData.isFlowPlainAllowed = false;
			scalarData.isBlockPlainAllowed = false;
		}
		if (flag)
		{
			scalarData.isFlowPlainAllowed = false;
		}
		if (flag2)
		{
			scalarData.isBlockPlainAllowed = false;
		}
	}

	private bool IsUnicode(Encoding encoding)
	{
		return encoding.Equals(Encoding.UTF8) || encoding.Equals(Encoding.Unicode) || encoding.Equals(Encoding.BigEndianUnicode) || encoding.Equals(Encoding.UTF7) || encoding.Equals(Encoding.UTF32);
	}

	private void AnalyzeTag(string tag)
	{
		tagData.handle = tag;
		foreach (TagDirective item in tagDirectives)
		{
			if (tag.StartsWith(item.Prefix, StringComparison.Ordinal))
			{
				tagData.handle = item.Handle;
				tagData.suffix = tag.Substring(item.Prefix.Length);
				break;
			}
		}
	}

	private void StateMachine(ParsingEvent evt)
	{
		Comment comment = evt as Comment;
		if (comment != null)
		{
			EmitComment(comment);
			return;
		}
		switch (state)
		{
		case EmitterStateKind.StreamStart:
			EmitStreamStart(evt);
			break;
		case EmitterStateKind.FirstDocumentStart:
			EmitDocumentStart(evt, true);
			break;
		case EmitterStateKind.DocumentStart:
			EmitDocumentStart(evt, false);
			break;
		case EmitterStateKind.DocumentContent:
			EmitDocumentContent(evt);
			break;
		case EmitterStateKind.DocumentEnd:
			EmitDocumentEnd(evt);
			break;
		case EmitterStateKind.FlowSequenceFirstItem:
			EmitFlowSequenceItem(evt, true);
			break;
		case EmitterStateKind.FlowSequenceItem:
			EmitFlowSequenceItem(evt, false);
			break;
		case EmitterStateKind.FlowMappingFirstKey:
			EmitFlowMappingKey(evt, true);
			break;
		case EmitterStateKind.FlowMappingKey:
			EmitFlowMappingKey(evt, false);
			break;
		case EmitterStateKind.FlowMappingSimpleValue:
			EmitFlowMappingValue(evt, true);
			break;
		case EmitterStateKind.FlowMappingValue:
			EmitFlowMappingValue(evt, false);
			break;
		case EmitterStateKind.BlockSequenceFirstItem:
			EmitBlockSequenceItem(evt, true);
			break;
		case EmitterStateKind.BlockSequenceItem:
			EmitBlockSequenceItem(evt, false);
			break;
		case EmitterStateKind.BlockMappingFirstKey:
			EmitBlockMappingKey(evt, true);
			break;
		case EmitterStateKind.BlockMappingKey:
			EmitBlockMappingKey(evt, false);
			break;
		case EmitterStateKind.BlockMappingSimpleValue:
			EmitBlockMappingValue(evt, true);
			break;
		case EmitterStateKind.BlockMappingValue:
			EmitBlockMappingValue(evt, false);
			break;
		case EmitterStateKind.StreamEnd:
			throw new YamlException("Expected nothing after STREAM-END");
		default:
			throw new InvalidOperationException();
		}
	}

	private void EmitComment(Comment comment)
	{
		if (comment.GetIsInline())
		{
			Write(' ');
		}
		else
		{
			WriteBreak();
		}
		Write("# ");
		Write(comment.GetValue());
		isIndentation = true;
	}

	private void EmitStreamStart(ParsingEvent evt)
	{
		if (!(evt is StreamStart))
		{
			throw new ArgumentException("Expected STREAM-START.", "evt");
		}
		indent = -1;
		column = 0;
		isWhitespace = true;
		isIndentation = true;
		state = EmitterStateKind.FirstDocumentStart;
	}

	private void EmitDocumentStart(ParsingEvent evt, bool isFirst)
	{
		DocumentStart documentStart = evt as DocumentStart;
		if (documentStart != null)
		{
			bool flag = documentStart.GetIsImplicit() && isFirst && !isCanonical;
			TagDirectiveCollection nonDefaultTags = NonDefaultTagsAmong(documentStart.GetTags());
			if (!isFirst && !isDocumentEndWritten && (documentStart.GetVersion() != null || nonDefaultTags.Count > 0))
			{
				isDocumentEndWritten = false;
				WriteIndicator("...", true, false, false);
				WriteIndent();
			}
			if (documentStart.GetVersion() != null)
			{
				AnalyzeVersionDirective(documentStart.GetVersion());
				flag = false;
				WriteIndicator("%YAML", true, false, false);
				WriteIndicator(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", 1, 1), true, false, false);
				WriteIndent();
			}
			foreach (TagDirective item in nonDefaultTags)
			{
				AppendTagDirectiveTo(item, false, tagDirectives);
			}
			TagDirective[] defaultDirectives = YamlConstants.DefaultTagDirectives;
			foreach (TagDirective defaultDirective in defaultDirectives)
			{
				AppendTagDirectiveTo(defaultDirective, true, tagDirectives);
			}
			if (nonDefaultTags.Count > 0)
			{
				flag = false;
				TagDirective[] tagDirectives = YamlConstants.DefaultTagDirectives;
				foreach (TagDirective tagDirective in tagDirectives)
				{
					AppendTagDirectiveTo(tagDirective, true, nonDefaultTags);
				}
				foreach (TagDirective item2 in nonDefaultTags)
				{
					WriteIndicator("%TAG", true, false, false);
					WriteTagHandle(item2.Handle);
					WriteTagContent(item2.Prefix, true);
					WriteIndent();
				}
			}
			if (CheckEmptyDocument())
			{
				flag = false;
			}
			if (!flag)
			{
				WriteIndent();
				WriteIndicator("---", true, false, false);
				if (isCanonical)
				{
					WriteIndent();
				}
			}
			state = EmitterStateKind.DocumentContent;
		}
		else
		{
			if (!(evt is StreamEndEvent))
			{
				throw new YamlException("Expected DOCUMENT-START or STREAM-END");
			}
			if (isOpenEnded)
			{
				WriteIndicator("...", true, false, false);
				WriteIndent();
			}
			state = EmitterStateKind.StreamEnd;
		}
	}

	private TagDirectiveCollection NonDefaultTagsAmong(IEnumerable<TagDirective> directives)
	{
		TagDirectiveCollection result = new TagDirectiveCollection();
		if (directives == null)
		{
			return result;
		}
		foreach (TagDirective item2 in directives)
		{
			AppendTagDirectiveTo(item2, false, result);
		}
		TagDirective[] defaultDirectives = YamlConstants.DefaultTagDirectives;
		foreach (TagDirective item in defaultDirectives)
		{
			result.Remove(item);
		}
		return result;
	}

	private void AnalyzeVersionDirective(VersionDirective versionDirective)
	{
		if (versionDirective.Version.Major != 1 || versionDirective.Version.Minor != 1)
		{
			throw new YamlException("Incompatible %YAML directive");
		}
	}

	private void AppendTagDirectiveTo(TagDirective value, bool allowDuplicates, TagDirectiveCollection target)
	{
		if (target.Contains(value))
		{
			if (!allowDuplicates)
			{
				throw new YamlException("Duplicate %TAG directive.");
			}
		}
		else
		{
			target.Add(value);
		}
	}

	private void EmitDocumentContent(ParsingEvent evt)
	{
		states.Push(EmitterStateKind.DocumentEnd);
		EmitNode(evt, true, false, false);
	}

	private void EmitNode(ParsingEvent evt, bool isRoot, bool isMapping, bool isSimpleKey)
	{
		isRootContext = isRoot;
		isMappingContext = isMapping;
		isSimpleKeyContext = isSimpleKey;
		switch (evt.get_Type())
		{
		case ParsingEventType.Alias:
			EmitAlias();
			break;
		case ParsingEventType.Scalar:
			EmitScalar(evt);
			break;
		case ParsingEventType.SequenceStart:
			EmitSequenceStart(evt);
			break;
		case ParsingEventType.MappingStart:
			EmitMappingStart(evt);
			break;
		default:
			throw new YamlException(string.Format("Expected SCALAR, SEQUENCE-START, MAPPING-START, or ALIAS, got {0}", evt.get_Type()));
		}
	}

	private void EmitAlias()
	{
		ProcessAnchor();
		state = states.Pop();
	}

	private void EmitScalar(ParsingEvent evt)
	{
		SelectScalarStyle(evt);
		ProcessAnchor();
		ProcessTag();
		IncreaseIndent(true, false);
		ProcessScalar();
		indent = indents.Pop();
		state = states.Pop();
	}

	private void SelectScalarStyle(ParsingEvent evt)
	{
		Scalar scalar = (Scalar)evt;
		ScalarStyle style = scalar.GetStyle();
		bool flag = tagData.handle == null && tagData.suffix == null;
		if (flag && !scalar.GetIsPlainImplicit() && !scalar.GetIsQuotedImplicit())
		{
			throw new YamlException("Neither tag nor isImplicit flags are specified.");
		}
		if (style == ScalarStyle.Any)
		{
			style = ((!scalarData.isMultiline) ? ScalarStyle.Plain : ScalarStyle.Folded);
		}
		if (isCanonical)
		{
			style = ScalarStyle.DoubleQuoted;
		}
		if (isSimpleKeyContext && scalarData.isMultiline)
		{
			style = ScalarStyle.DoubleQuoted;
		}
		if (style == ScalarStyle.Plain)
		{
			if ((flowLevel != 0 && !scalarData.isFlowPlainAllowed) || (flowLevel == 0 && !scalarData.isBlockPlainAllowed))
			{
				style = ScalarStyle.SingleQuoted;
			}
			if (string.IsNullOrEmpty(scalarData.value) && (flowLevel != 0 || isSimpleKeyContext))
			{
				style = ScalarStyle.SingleQuoted;
			}
			if (flag && !scalar.GetIsPlainImplicit())
			{
				style = ScalarStyle.SingleQuoted;
			}
		}
		if (style == ScalarStyle.SingleQuoted && !scalarData.isSingleQuotedAllowed)
		{
			style = ScalarStyle.DoubleQuoted;
		}
		if ((style == ScalarStyle.Literal || style == ScalarStyle.Folded) && (!scalarData.isBlockAllowed || flowLevel != 0 || isSimpleKeyContext))
		{
			style = ScalarStyle.DoubleQuoted;
		}
		scalarData.style = style;
	}

	private void ProcessScalar()
	{
		switch (scalarData.style)
		{
		case ScalarStyle.Plain:
			WritePlainScalar(scalarData.value, !isSimpleKeyContext);
			break;
		case ScalarStyle.SingleQuoted:
			WriteSingleQuotedScalar(scalarData.value, !isSimpleKeyContext);
			break;
		case ScalarStyle.DoubleQuoted:
			WriteDoubleQuotedScalar(scalarData.value, !isSimpleKeyContext);
			break;
		case ScalarStyle.Literal:
			WriteLiteralScalar(scalarData.value);
			break;
		case ScalarStyle.Folded:
			WriteFoldedScalar(scalarData.value);
			break;
		default:
			throw new InvalidOperationException();
		}
	}

	private void WritePlainScalar(string value, bool allowBreaks)
	{
		if (!isWhitespace)
		{
			Write(' ');
		}
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (IsSpace(c))
			{
				if (allowBreaks && !flag && column > bestWidth && i + 1 < value.Length && value[i + 1] != ' ')
				{
					WriteIndent();
				}
				else
				{
					Write(c);
				}
				flag = true;
				continue;
			}
			if (IsBreak(c))
			{
				if (!flag2 && c == '\n')
				{
					WriteBreak();
				}
				WriteBreak();
				isIndentation = true;
				flag2 = true;
				continue;
			}
			if (flag2)
			{
				WriteIndent();
			}
			Write(c);
			isIndentation = false;
			flag = false;
			flag2 = false;
		}
		isWhitespace = false;
		isIndentation = false;
		if (isRootContext)
		{
			isOpenEnded = true;
		}
	}

	private void WriteSingleQuotedScalar(string value, bool allowBreaks)
	{
		WriteIndicator("'", true, false, false);
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (c == ' ')
			{
				if (allowBreaks && !flag && column > bestWidth && i != 0 && i + 1 < value.Length && value[i + 1] != ' ')
				{
					WriteIndent();
				}
				else
				{
					Write(c);
				}
				flag = true;
				continue;
			}
			if (IsBreak(c))
			{
				if (!flag2 && c == '\n')
				{
					WriteBreak();
				}
				WriteBreak();
				isIndentation = true;
				flag2 = true;
				continue;
			}
			if (flag2)
			{
				WriteIndent();
			}
			if (c == '\'')
			{
				Write(c);
			}
			Write(c);
			isIndentation = false;
			flag = false;
			flag2 = false;
		}
		WriteIndicator("'", false, false, false);
		isWhitespace = false;
		isIndentation = false;
	}

	private void WriteDoubleQuotedScalar(string value, bool allowBreaks)
	{
		WriteIndicator("\"", true, false, false);
		bool flag = false;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (IsPrintable(c) && !IsBreak(c))
			{
				switch (c)
				{
				case '"':
				case '\\':
					break;
				case ' ':
					if (allowBreaks && !flag && column > bestWidth && i > 0 && i + 1 < value.Length)
					{
						WriteIndent();
						if (value[i + 1] == ' ')
						{
							Write('\\');
						}
					}
					else
					{
						Write(c);
					}
					flag = true;
					continue;
				default:
					Write(c);
					flag = false;
					continue;
				}
			}
			Write('\\');
			switch (c)
			{
			case '\0':
				Write('0');
				break;
			case '\a':
				Write('a');
				break;
			case '\b':
				Write('b');
				break;
			case '\t':
				Write('t');
				break;
			case '\n':
				Write('n');
				break;
			case '\v':
				Write('v');
				break;
			case '\f':
				Write('f');
				break;
			case '\r':
				Write('r');
				break;
			case '\u001b':
				Write('e');
				break;
			case '"':
				Write('"');
				break;
			case '\\':
				Write('\\');
				break;
			case '\u0085':
				Write('N');
				break;
			case '\u00a0':
				Write('_');
				break;
			case '\u2028':
				Write('L');
				break;
			case '\u2029':
				Write('P');
				break;
			default:
			{
				short num = (short)c;
				if (num <= 255)
				{
					Write('x');
					Write(num.ToString("X02", CultureInfo.InvariantCulture));
				}
				else
				{
					Write('u');
					Write(num.ToString("X04", CultureInfo.InvariantCulture));
				}
				break;
			}
			}
			flag = false;
		}
		WriteIndicator("\"", false, false, false);
		isWhitespace = false;
		isIndentation = false;
	}

	private void WriteLiteralScalar(string value)
	{
		bool flag = true;
		WriteIndicator("|", true, false, false);
		WriteBlockScalarHints(value);
		WriteBreak();
		isIndentation = true;
		isWhitespace = true;
		foreach (char c in value)
		{
			if (IsBreak(c))
			{
				WriteBreak();
				isIndentation = true;
				flag = true;
				continue;
			}
			if (flag)
			{
				WriteIndent();
			}
			Write(c);
			isIndentation = false;
			flag = false;
		}
	}

	private void WriteFoldedScalar(string value)
	{
		bool flag = true;
		bool flag2 = true;
		WriteIndicator(">", true, false, false);
		WriteBlockScalarHints(value);
		WriteBreak();
		isIndentation = true;
		isWhitespace = true;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (IsBreak(c))
			{
				if (!flag && !flag2 && c == '\n')
				{
					int j;
					for (j = 0; i + j < value.Length && IsBreak(value[i + j]); j++)
					{
					}
					if (i + j < value.Length && !IsBlank(value[i + j]) && !IsBreak(value[i + j]))
					{
						WriteBreak();
					}
				}
				WriteBreak();
				isIndentation = true;
				flag = true;
			}
			else
			{
				if (flag)
				{
					WriteIndent();
					flag2 = IsBlank(c);
				}
				if (!flag && c == ' ' && i + 1 < value.Length && value[i + 1] != ' ' && column > bestWidth)
				{
					WriteIndent();
				}
				else
				{
					Write(c);
				}
				isIndentation = false;
				flag = false;
			}
		}
	}

	private static bool IsSpace(char ch)
	{
		return ch == ' ';
	}

	private static bool IsBreak(char ch)
	{
		return ch == '\r' || ch == '\n' || ch == '\u0085' || ch == '\u2028' || ch == '\u2029';
	}

	private static bool IsBlank(char ch)
	{
		return ch == ' ' || ch == '\t';
	}

	private static bool IsPrintable(char ch)
	{
		return ch == '\t' || ch == '\n' || ch == '\r' || (ch >= ' ' && ch <= '~') || ch == '\u0085' || (ch >= '\u00a0' && ch <= '\ud7ff') || (ch >= '\ue000' && ch <= '\ufffd');
	}

	private void EmitSequenceStart(ParsingEvent evt)
	{
		ProcessAnchor();
		ProcessTag();
		SequenceStart sequenceStart = (SequenceStart)evt;
		if (flowLevel != 0 || isCanonical || sequenceStart.GetStyle() == SequenceStyle.Flow || CheckEmptySequence())
		{
			state = EmitterStateKind.FlowSequenceFirstItem;
		}
		else
		{
			state = EmitterStateKind.BlockSequenceFirstItem;
		}
	}

	private void EmitMappingStart(ParsingEvent evt)
	{
		ProcessAnchor();
		ProcessTag();
		MappingStart mappingStart = (MappingStart)evt;
		if (flowLevel != 0 || isCanonical || mappingStart.GetStyle() == MappingStyle.Flow || CheckEmptyMapping())
		{
			state = EmitterStateKind.FlowMappingFirstKey;
		}
		else
		{
			state = EmitterStateKind.BlockMappingFirstKey;
		}
	}

	private void ProcessAnchor()
	{
		if (anchorData.anchor != null)
		{
			WriteIndicator((!anchorData.isAlias) ? "&" : "*", true, false, false);
			WriteAnchor(anchorData.anchor);
		}
	}

	private void ProcessTag()
	{
		if (tagData.handle == null && tagData.suffix == null)
		{
			return;
		}
		if (tagData.handle != null)
		{
			WriteTagHandle(tagData.handle);
			if (tagData.suffix != null)
			{
				WriteTagContent(tagData.suffix, false);
			}
		}
		else
		{
			WriteIndicator("!<", true, false, false);
			WriteTagContent(tagData.suffix, false);
			WriteIndicator(">", false, false, false);
		}
	}

	private void EmitDocumentEnd(ParsingEvent evt)
	{
		DocumentEnd documentEnd = evt as DocumentEnd;
		if (documentEnd != null)
		{
			WriteIndent();
			if (!documentEnd.GetIsImplicit())
			{
				WriteIndicator("...", true, false, false);
				WriteIndent();
				isDocumentEndWritten = true;
			}
			state = EmitterStateKind.DocumentStart;
			tagDirectives.Clear();
			return;
		}
		throw new YamlException("Expected DOCUMENT-END.");
	}

	private void EmitFlowSequenceItem(ParsingEvent evt, bool isFirst)
	{
		if (isFirst)
		{
			WriteIndicator("[", true, true, false);
			IncreaseIndent(true, false);
			flowLevel++;
		}
		if (evt is SequenceEnd)
		{
			flowLevel--;
			indent = indents.Pop();
			if (isCanonical && !isFirst)
			{
				WriteIndicator(",", false, false, false);
				WriteIndent();
			}
			WriteIndicator("]", false, false, false);
			state = states.Pop();
		}
		else
		{
			if (!isFirst)
			{
				WriteIndicator(",", false, false, false);
			}
			if (isCanonical || column > bestWidth)
			{
				WriteIndent();
			}
			states.Push(EmitterStateKind.FlowSequenceItem);
			EmitNode(evt, false, false, false);
		}
	}

	private void EmitFlowMappingKey(ParsingEvent evt, bool isFirst)
	{
		if (isFirst)
		{
			WriteIndicator("{", true, true, false);
			IncreaseIndent(true, false);
			flowLevel++;
		}
		if (evt is MappingEnd)
		{
			flowLevel--;
			indent = indents.Pop();
			if (isCanonical && !isFirst)
			{
				WriteIndicator(",", false, false, false);
				WriteIndent();
			}
			WriteIndicator("}", false, false, false);
			state = states.Pop();
			return;
		}
		if (!isFirst)
		{
			WriteIndicator(",", false, false, false);
		}
		if (isCanonical || column > bestWidth)
		{
			WriteIndent();
		}
		if (!isCanonical && CheckSimpleKey())
		{
			states.Push(EmitterStateKind.FlowMappingSimpleValue);
			EmitNode(evt, false, true, true);
		}
		else
		{
			WriteIndicator("?", true, false, false);
			states.Push(EmitterStateKind.FlowMappingValue);
			EmitNode(evt, false, true, false);
		}
	}

	private void EmitFlowMappingValue(ParsingEvent evt, bool isSimple)
	{
		if (isSimple)
		{
			WriteIndicator(":", false, false, false);
		}
		else
		{
			if (isCanonical || column > bestWidth)
			{
				WriteIndent();
			}
			WriteIndicator(":", true, false, false);
		}
		states.Push(EmitterStateKind.FlowMappingKey);
		EmitNode(evt, false, true, false);
	}

	private void EmitBlockSequenceItem(ParsingEvent evt, bool isFirst)
	{
		if (isFirst)
		{
			IncreaseIndent(false, isMappingContext && !isIndentation);
		}
		if (evt is SequenceEnd)
		{
			indent = indents.Pop();
			state = states.Pop();
			return;
		}
		WriteIndent();
		WriteIndicator("  -", true, false, true);
		states.Push(EmitterStateKind.BlockSequenceItem);
		EmitNode(evt, false, false, false);
	}

	private void EmitBlockMappingKey(ParsingEvent evt, bool isFirst)
	{
		if (isFirst)
		{
			IncreaseIndent(false, false);
		}
		if (evt is MappingEnd)
		{
			indent = indents.Pop();
			state = states.Pop();
			return;
		}
		WriteIndent();
		if (CheckSimpleKey())
		{
			states.Push(EmitterStateKind.BlockMappingSimpleValue);
			EmitNode(evt, false, true, true);
		}
		else
		{
			WriteIndicator("?", true, false, true);
			states.Push(EmitterStateKind.BlockMappingValue);
			EmitNode(evt, false, true, false);
		}
	}

	private void EmitBlockMappingValue(ParsingEvent evt, bool isSimple)
	{
		if (isSimple)
		{
			WriteIndicator(":", false, false, false);
		}
		else
		{
			WriteIndent();
			WriteIndicator(":", true, false, true);
		}
		states.Push(EmitterStateKind.BlockMappingKey);
		EmitNode(evt, false, true, false);
	}

	private void IncreaseIndent(bool flow, bool indentless)
	{
		indents.Push(indent);
		if (indent < 0)
		{
			indent = (flow ? bestIndent : 0);
		}
		else if (!indentless)
		{
			indent += bestIndent;
		}
	}

	private bool CheckEmptyDocument()
	{
		int num = 0;
		foreach (ParsingEvent item in events)
		{
			num++;
			if (num == 2)
			{
				Scalar scalar = item as Scalar;
				if (scalar != null)
				{
					return string.IsNullOrEmpty(scalar.GetValue());
				}
				break;
			}
		}
		return false;
	}

	private bool CheckSimpleKey()
	{
		if (events.Count < 1)
		{
			return false;
		}
		int num;
		switch (events.Peek().get_Type())
		{
		case ParsingEventType.Alias:
			num = SafeStringLength(anchorData.anchor);
			break;
		case ParsingEventType.Scalar:
			if (scalarData.isMultiline)
			{
				return false;
			}
			num = SafeStringLength(anchorData.anchor) + SafeStringLength(tagData.handle) + SafeStringLength(tagData.suffix) + SafeStringLength(scalarData.value);
			break;
		case ParsingEventType.SequenceStart:
			if (!CheckEmptySequence())
			{
				return false;
			}
			num = SafeStringLength(anchorData.anchor) + SafeStringLength(tagData.handle) + SafeStringLength(tagData.suffix);
			break;
		case ParsingEventType.MappingStart:
			if (!CheckEmptySequence())
			{
				return false;
			}
			num = SafeStringLength(anchorData.anchor) + SafeStringLength(tagData.handle) + SafeStringLength(tagData.suffix);
			break;
		default:
			return false;
		}
		return num <= 128;
	}

	private int SafeStringLength(string value)
	{
		return (value != null) ? value.Length : 0;
	}

	private bool CheckEmptySequence()
	{
		if (events.Count < 2)
		{
			return false;
		}
		global::FakeList<ParsingEvent> eventList = new global::FakeList<ParsingEvent>(events);
		return eventList.get_Item(0) is SequenceStart && eventList.get_Item(1) is SequenceEnd;
	}

	private bool CheckEmptyMapping()
	{
		if (events.Count < 2)
		{
			return false;
		}
		global::FakeList<ParsingEvent> eventList = new global::FakeList<ParsingEvent>(events);
		return eventList.get_Item(0) is MappingStart && eventList.get_Item(1) is MappingEnd;
	}

	private void WriteBlockScalarHints(string value)
	{
		CharacterAnalyzer<StringLookAheadBuffer> characterAnalyzer = new CharacterAnalyzer<StringLookAheadBuffer>(new StringLookAheadBuffer(value));
		if (characterAnalyzer.IsSpace() || characterAnalyzer.IsBreak())
		{
			string indentHint = string.Format(CultureInfo.InvariantCulture, "{0}\0", bestIndent);
			WriteIndicator(indentHint, false, false, false);
		}
		isOpenEnded = false;
		string text = null;
		if (value.Length == 0 || !characterAnalyzer.IsBreak(value.Length - 1))
		{
			text = "-";
		}
		else if (value.Length >= 2 && characterAnalyzer.IsBreak(value.Length - 2))
		{
			text = "+";
			isOpenEnded = true;
		}
		if (text != null)
		{
			WriteIndicator(text, false, false, false);
		}
	}

	private void WriteIndicator(string indicator, bool needsWhitespace, bool whitespace, bool indentation)
	{
		if (needsWhitespace && !isWhitespace)
		{
			Write(' ');
		}
		Write(indicator);
		isWhitespace = whitespace;
		isIndentation &= indentation;
		isOpenEnded = false;
	}

	private void WriteIndent()
	{
		int num = Math.Max(indent, 0);
		if (!isIndentation || column > num || (column == num && !isWhitespace))
		{
			WriteBreak();
		}
		while (column < num)
		{
			Write(' ');
		}
		isWhitespace = true;
		isIndentation = true;
	}

	private void WriteAnchor(string value)
	{
		Write(value);
		isWhitespace = false;
		isIndentation = false;
	}

	private void WriteTagHandle(string value)
	{
		if (!isWhitespace)
		{
			Write(' ');
		}
		Write(value);
		isWhitespace = false;
		isIndentation = false;
	}

	private void WriteTagContent(string value, bool needsWhitespace)
	{
		if (needsWhitespace && !isWhitespace)
		{
			Write(' ');
		}
		Write(UrlEncode(value));
		isWhitespace = false;
		isIndentation = false;
	}

	private string UrlEncode(string text)
	{
		return uriReplacer.Replace(text, (System.Text.RegularExpressions.Match match) =>
		{
			StringBuilder stringBuilder = new StringBuilder();
			byte[] bytes = Encoding.UTF8.GetBytes(match.Value);
			foreach (byte b in bytes)
			{
				stringBuilder.AppendFormat("%{0:X02}", b);
			}
			return stringBuilder.ToString();
		});
	}

	private void Write(char value)
	{
		output.Write(value);
		column++;
	}

	private void Write(string value)
	{
		output.Write(value);
		column += value.Length;
	}

	private void WriteBreak()
	{
		output.WriteLine();
		column = 0;
	}
}
