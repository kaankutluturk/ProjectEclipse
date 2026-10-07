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

	public Emitter(TextWriter output, int EMOCJNOCJKM)
		: this(output, EMOCJNOCJKM, int.MaxValue)
	{
	}

	public Emitter(TextWriter output, int EMOCJNOCJKM, int CEIKEMJHLKL)
		: this(output, EMOCJNOCJKM, CEIKEMJHLKL, false)
	{
	}

	public Emitter(TextWriter output, int EMOCJNOCJKM, int CEIKEMJHLKL, bool LGDHGOGFFCJ)
	{
		if (EMOCJNOCJKM < 4 || EMOCJNOCJKM > 9)
		{
			throw new ArgumentOutOfRangeException("bestIndent", string.Format(CultureInfo.InvariantCulture, "The bestIndent parameter must be between {0} and {1}.", 4, 9));
		}
		this.bestIndent = EMOCJNOCJKM;
		if (CEIKEMJHLKL <= EMOCJNOCJKM * 2)
		{
			throw new ArgumentOutOfRangeException("bestWidth", "The bestWidth parameter must be greater than bestIndent * 2.");
		}
		this.bestWidth = CEIKEMJHLKL;
		this.isCanonical = LGDHGOGFFCJ;
		this.output = output;
	}

	public void Emit(ParsingEvent KEAJCHAAIEP)
	{
		events.Enqueue(KEAJCHAAIEP);
		while (!NeedMoreEvents())
		{
			ParsingEvent iILOLJJLLGH = events.Peek();
			try
			{
				AnalyzeEvent(iILOLJJLLGH);
				StateMachine(iILOLJJLLGH);
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

	private void AnalyzeEvent(ParsingEvent IILOLJJLLGH)
	{
		anchorData.anchor = null;
		tagData.handle = null;
		tagData.suffix = null;
		AnchorAlias mBEGNNDMDKH = IILOLJJLLGH as AnchorAlias;
		if (mBEGNNDMDKH != null)
		{
			AnalyzeAnchor(mBEGNNDMDKH.GetValue(), true);
			return;
		}
		NodeEvent dGMPGIHHKCN = IILOLJJLLGH as NodeEvent;
		if (dGMPGIHHKCN != null)
		{
			Scalar lEACOCDHICF = IILOLJJLLGH as Scalar;
			if (lEACOCDHICF != null)
			{
				AnalyzeScalar(lEACOCDHICF.GetValue());
			}
			AnalyzeAnchor(dGMPGIHHKCN.GetAnchor(), false);
			if (!string.IsNullOrEmpty(dGMPGIHHKCN.GetTag()) && (isCanonical || dGMPGIHHKCN.GetIsCanonical()))
			{
				AnalyzeTag(dGMPGIHHKCN.GetTag());
			}
		}
	}

	private void AnalyzeAnchor(string KOLNNNLOCFE, bool LCPNKFDMFIA)
	{
		anchorData.anchor = KOLNNNLOCFE;
		anchorData.isAlias = LCPNKFDMFIA;
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

	private bool IsUnicode(Encoding JIBCJOMMFCO)
	{
		return JIBCJOMMFCO.Equals(Encoding.UTF8) || JIBCJOMMFCO.Equals(Encoding.Unicode) || JIBCJOMMFCO.Equals(Encoding.BigEndianUnicode) || JIBCJOMMFCO.Equals(Encoding.UTF7) || JIBCJOMMFCO.Equals(Encoding.UTF32);
	}

	private void AnalyzeTag(string EDLADAAKMDF)
	{
		tagData.handle = EDLADAAKMDF;
		foreach (TagDirective item in tagDirectives)
		{
			if (EDLADAAKMDF.StartsWith(item.Prefix, StringComparison.Ordinal))
			{
				tagData.handle = item.Handle;
				tagData.suffix = EDLADAAKMDF.Substring(item.Prefix.Length);
				break;
			}
		}
	}

	private void StateMachine(ParsingEvent IILOLJJLLGH)
	{
		Comment mGMGDDOIHAJ = IILOLJJLLGH as Comment;
		if (mGMGDDOIHAJ != null)
		{
			EmitComment(mGMGDDOIHAJ);
			return;
		}
		switch (state)
		{
		case EmitterStateKind.StreamStart:
			EmitStreamStart(IILOLJJLLGH);
			break;
		case EmitterStateKind.FirstDocumentStart:
			EmitDocumentStart(IILOLJJLLGH, true);
			break;
		case EmitterStateKind.DocumentStart:
			EmitDocumentStart(IILOLJJLLGH, false);
			break;
		case EmitterStateKind.DocumentContent:
			EmitDocumentContent(IILOLJJLLGH);
			break;
		case EmitterStateKind.DocumentEnd:
			EmitDocumentEnd(IILOLJJLLGH);
			break;
		case EmitterStateKind.FlowSequenceFirstItem:
			EmitFlowSequenceItem(IILOLJJLLGH, true);
			break;
		case EmitterStateKind.FlowSequenceItem:
			EmitFlowSequenceItem(IILOLJJLLGH, false);
			break;
		case EmitterStateKind.FlowMappingFirstKey:
			EmitFlowMappingKey(IILOLJJLLGH, true);
			break;
		case EmitterStateKind.FlowMappingKey:
			EmitFlowMappingKey(IILOLJJLLGH, false);
			break;
		case EmitterStateKind.FlowMappingSimpleValue:
			EmitFlowMappingValue(IILOLJJLLGH, true);
			break;
		case EmitterStateKind.FlowMappingValue:
			EmitFlowMappingValue(IILOLJJLLGH, false);
			break;
		case EmitterStateKind.BlockSequenceFirstItem:
			EmitBlockSequenceItem(IILOLJJLLGH, true);
			break;
		case EmitterStateKind.BlockSequenceItem:
			EmitBlockSequenceItem(IILOLJJLLGH, false);
			break;
		case EmitterStateKind.BlockMappingFirstKey:
			EmitBlockMappingKey(IILOLJJLLGH, true);
			break;
		case EmitterStateKind.BlockMappingKey:
			EmitBlockMappingKey(IILOLJJLLGH, false);
			break;
		case EmitterStateKind.BlockMappingSimpleValue:
			EmitBlockMappingValue(IILOLJJLLGH, true);
			break;
		case EmitterStateKind.BlockMappingValue:
			EmitBlockMappingValue(IILOLJJLLGH, false);
			break;
		case EmitterStateKind.StreamEnd:
			throw new YamlException("Expected nothing after STREAM-END");
		default:
			throw new InvalidOperationException();
		}
	}

	private void EmitComment(Comment MPMFGPGDGDN)
	{
		if (MPMFGPGDGDN.GetIsInline())
		{
			Write(' ');
		}
		else
		{
			WriteBreak();
		}
		Write("# ");
		Write(MPMFGPGDGDN.GetValue());
		isIndentation = true;
	}

	private void EmitStreamStart(ParsingEvent IILOLJJLLGH)
	{
		if (!(IILOLJJLLGH is StreamStart))
		{
			throw new ArgumentException("Expected STREAM-START.", "evt");
		}
		indent = -1;
		column = 0;
		isWhitespace = true;
		isIndentation = true;
		state = EmitterStateKind.FirstDocumentStart;
	}

	private void EmitDocumentStart(ParsingEvent IILOLJJLLGH, bool IKNHLPGLLKB)
	{
		DocumentStart aOGNBDOIKPE = IILOLJJLLGH as DocumentStart;
		if (aOGNBDOIKPE != null)
		{
			bool flag = aOGNBDOIKPE.GetIsImplicit() && IKNHLPGLLKB && !isCanonical;
			TagDirectiveCollection iDHIKALFADG = NonDefaultTagsAmong(aOGNBDOIKPE.GetTags());
			if (!IKNHLPGLLKB && !isDocumentEndWritten && (aOGNBDOIKPE.GetVersion() != null || iDHIKALFADG.Count > 0))
			{
				isDocumentEndWritten = false;
				WriteIndicator("...", true, false, false);
				WriteIndent();
			}
			if (aOGNBDOIKPE.GetVersion() != null)
			{
				AnalyzeVersionDirective(aOGNBDOIKPE.GetVersion());
				flag = false;
				WriteIndicator("%YAML", true, false, false);
				WriteIndicator(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", 1, 1), true, false, false);
				WriteIndent();
			}
			foreach (TagDirective item in iDHIKALFADG)
			{
				AppendTagDirectiveTo(item, false, tagDirectives);
			}
			TagDirective[] gNPKLFKPLCM = YamlConstants.DefaultTagDirectives;
			foreach (TagDirective bAINMLLIKOL in gNPKLFKPLCM)
			{
				AppendTagDirectiveTo(bAINMLLIKOL, true, tagDirectives);
			}
			if (iDHIKALFADG.Count > 0)
			{
				flag = false;
				TagDirective[] gNPKLFKPLCM2 = YamlConstants.DefaultTagDirectives;
				foreach (TagDirective bAINMLLIKOL2 in gNPKLFKPLCM2)
				{
					AppendTagDirectiveTo(bAINMLLIKOL2, true, iDHIKALFADG);
				}
				foreach (TagDirective item2 in iDHIKALFADG)
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
			if (!(IILOLJJLLGH is StreamEndEvent))
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

	private TagDirectiveCollection NonDefaultTagsAmong(IEnumerable<TagDirective> FIMJCFLNJIK)
	{
		TagDirectiveCollection iDHIKALFADG = new TagDirectiveCollection();
		if (FIMJCFLNJIK == null)
		{
			return iDHIKALFADG;
		}
		foreach (TagDirective item2 in FIMJCFLNJIK)
		{
			AppendTagDirectiveTo(item2, false, iDHIKALFADG);
		}
		TagDirective[] gNPKLFKPLCM = YamlConstants.DefaultTagDirectives;
		foreach (TagDirective item in gNPKLFKPLCM)
		{
			iDHIKALFADG.Remove(item);
		}
		return iDHIKALFADG;
	}

	private void AnalyzeVersionDirective(VersionDirective JMCEGKIENKI)
	{
		if (JMCEGKIENKI.Version.Major != 1 || JMCEGKIENKI.Version.Minor != 1)
		{
			throw new YamlException("Incompatible %YAML directive");
		}
	}

	private void AppendTagDirectiveTo(TagDirective value, bool KBLBEMDBNGB, TagDirectiveCollection FMCEHNBELJF)
	{
		if (FMCEHNBELJF.Contains(value))
		{
			if (!KBLBEMDBNGB)
			{
				throw new YamlException("Duplicate %TAG directive.");
			}
		}
		else
		{
			FMCEHNBELJF.Add(value);
		}
	}

	private void EmitDocumentContent(ParsingEvent IILOLJJLLGH)
	{
		states.Push(EmitterStateKind.DocumentEnd);
		EmitNode(IILOLJJLLGH, true, false, false);
	}

	private void EmitNode(ParsingEvent IILOLJJLLGH, bool OHJNFDICPDH, bool CLHNCJFJJKN, bool MJHMCMNBBAA)
	{
		isRootContext = OHJNFDICPDH;
		isMappingContext = CLHNCJFJJKN;
		isSimpleKeyContext = MJHMCMNBBAA;
		switch (IILOLJJLLGH.get_Type())
		{
		case ParsingEventType.Alias:
			EmitAlias();
			break;
		case ParsingEventType.Scalar:
			EmitScalar(IILOLJJLLGH);
			break;
		case ParsingEventType.SequenceStart:
			EmitSequenceStart(IILOLJJLLGH);
			break;
		case ParsingEventType.MappingStart:
			EmitMappingStart(IILOLJJLLGH);
			break;
		default:
			throw new YamlException(string.Format("Expected SCALAR, SEQUENCE-START, MAPPING-START, or ALIAS, got {0}", IILOLJJLLGH.get_Type()));
		}
	}

	private void EmitAlias()
	{
		ProcessAnchor();
		state = states.Pop();
	}

	private void EmitScalar(ParsingEvent IILOLJJLLGH)
	{
		SelectScalarStyle(IILOLJJLLGH);
		ProcessAnchor();
		ProcessTag();
		IncreaseIndent(true, false);
		ProcessScalar();
		indent = indents.Pop();
		state = states.Pop();
	}

	private void SelectScalarStyle(ParsingEvent IILOLJJLLGH)
	{
		Scalar lEACOCDHICF = (Scalar)IILOLJJLLGH;
		ScalarStyle iBEOFCPMMJJ = lEACOCDHICF.GetStyle();
		bool flag = tagData.handle == null && tagData.suffix == null;
		if (flag && !lEACOCDHICF.GetIsPlainImplicit() && !lEACOCDHICF.GetIsQuotedImplicit())
		{
			throw new YamlException("Neither tag nor isImplicit flags are specified.");
		}
		if (iBEOFCPMMJJ == ScalarStyle.Any)
		{
			iBEOFCPMMJJ = ((!scalarData.isMultiline) ? ScalarStyle.Plain : ScalarStyle.Folded);
		}
		if (isCanonical)
		{
			iBEOFCPMMJJ = ScalarStyle.DoubleQuoted;
		}
		if (isSimpleKeyContext && scalarData.isMultiline)
		{
			iBEOFCPMMJJ = ScalarStyle.DoubleQuoted;
		}
		if (iBEOFCPMMJJ == ScalarStyle.Plain)
		{
			if ((flowLevel != 0 && !scalarData.isFlowPlainAllowed) || (flowLevel == 0 && !scalarData.isBlockPlainAllowed))
			{
				iBEOFCPMMJJ = ScalarStyle.SingleQuoted;
			}
			if (string.IsNullOrEmpty(scalarData.value) && (flowLevel != 0 || isSimpleKeyContext))
			{
				iBEOFCPMMJJ = ScalarStyle.SingleQuoted;
			}
			if (flag && !lEACOCDHICF.GetIsPlainImplicit())
			{
				iBEOFCPMMJJ = ScalarStyle.SingleQuoted;
			}
		}
		if (iBEOFCPMMJJ == ScalarStyle.SingleQuoted && !scalarData.isSingleQuotedAllowed)
		{
			iBEOFCPMMJJ = ScalarStyle.DoubleQuoted;
		}
		if ((iBEOFCPMMJJ == ScalarStyle.Literal || iBEOFCPMMJJ == ScalarStyle.Folded) && (!scalarData.isBlockAllowed || flowLevel != 0 || isSimpleKeyContext))
		{
			iBEOFCPMMJJ = ScalarStyle.DoubleQuoted;
		}
		scalarData.style = iBEOFCPMMJJ;
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

	private void WritePlainScalar(string value, bool AEMLFBEACGF)
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
				if (AEMLFBEACGF && !flag && column > bestWidth && i + 1 < value.Length && value[i + 1] != ' ')
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

	private void WriteSingleQuotedScalar(string value, bool AEMLFBEACGF)
	{
		WriteIndicator("'", true, false, false);
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (c == ' ')
			{
				if (AEMLFBEACGF && !flag && column > bestWidth && i != 0 && i + 1 < value.Length && value[i + 1] != ' ')
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

	private void WriteDoubleQuotedScalar(string value, bool AEMLFBEACGF)
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
					if (AEMLFBEACGF && !flag && column > bestWidth && i > 0 && i + 1 < value.Length)
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

	private static bool IsSpace(char KGDPNIINCJH)
	{
		return KGDPNIINCJH == ' ';
	}

	private static bool IsBreak(char KGDPNIINCJH)
	{
		return KGDPNIINCJH == '\r' || KGDPNIINCJH == '\n' || KGDPNIINCJH == '\u0085' || KGDPNIINCJH == '\u2028' || KGDPNIINCJH == '\u2029';
	}

	private static bool IsBlank(char KGDPNIINCJH)
	{
		return KGDPNIINCJH == ' ' || KGDPNIINCJH == '\t';
	}

	private static bool IsPrintable(char KGDPNIINCJH)
	{
		return KGDPNIINCJH == '\t' || KGDPNIINCJH == '\n' || KGDPNIINCJH == '\r' || (KGDPNIINCJH >= ' ' && KGDPNIINCJH <= '~') || KGDPNIINCJH == '\u0085' || (KGDPNIINCJH >= '\u00a0' && KGDPNIINCJH <= '\ud7ff') || (KGDPNIINCJH >= '\ue000' && KGDPNIINCJH <= '\ufffd');
	}

	private void EmitSequenceStart(ParsingEvent IILOLJJLLGH)
	{
		ProcessAnchor();
		ProcessTag();
		SequenceStart jODGINIKFJF = (SequenceStart)IILOLJJLLGH;
		if (flowLevel != 0 || isCanonical || jODGINIKFJF.GetStyle() == SequenceStyle.Flow || CheckEmptySequence())
		{
			state = EmitterStateKind.FlowSequenceFirstItem;
		}
		else
		{
			state = EmitterStateKind.BlockSequenceFirstItem;
		}
	}

	private void EmitMappingStart(ParsingEvent IILOLJJLLGH)
	{
		ProcessAnchor();
		ProcessTag();
		MappingStart oGMPNFCPPDH = (MappingStart)IILOLJJLLGH;
		if (flowLevel != 0 || isCanonical || oGMPNFCPPDH.GetStyle() == MappingStyle.Flow || CheckEmptyMapping())
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

	private void EmitDocumentEnd(ParsingEvent IILOLJJLLGH)
	{
		DocumentEnd nKCBFAMCLMO = IILOLJJLLGH as DocumentEnd;
		if (nKCBFAMCLMO != null)
		{
			WriteIndent();
			if (!nKCBFAMCLMO.GetIsImplicit())
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

	private void EmitFlowSequenceItem(ParsingEvent IILOLJJLLGH, bool IKNHLPGLLKB)
	{
		if (IKNHLPGLLKB)
		{
			WriteIndicator("[", true, true, false);
			IncreaseIndent(true, false);
			flowLevel++;
		}
		if (IILOLJJLLGH is SequenceEnd)
		{
			flowLevel--;
			indent = indents.Pop();
			if (isCanonical && !IKNHLPGLLKB)
			{
				WriteIndicator(",", false, false, false);
				WriteIndent();
			}
			WriteIndicator("]", false, false, false);
			state = states.Pop();
		}
		else
		{
			if (!IKNHLPGLLKB)
			{
				WriteIndicator(",", false, false, false);
			}
			if (isCanonical || column > bestWidth)
			{
				WriteIndent();
			}
			states.Push(EmitterStateKind.FlowSequenceItem);
			EmitNode(IILOLJJLLGH, false, false, false);
		}
	}

	private void EmitFlowMappingKey(ParsingEvent IILOLJJLLGH, bool IKNHLPGLLKB)
	{
		if (IKNHLPGLLKB)
		{
			WriteIndicator("{", true, true, false);
			IncreaseIndent(true, false);
			flowLevel++;
		}
		if (IILOLJJLLGH is MappingEnd)
		{
			flowLevel--;
			indent = indents.Pop();
			if (isCanonical && !IKNHLPGLLKB)
			{
				WriteIndicator(",", false, false, false);
				WriteIndent();
			}
			WriteIndicator("}", false, false, false);
			state = states.Pop();
			return;
		}
		if (!IKNHLPGLLKB)
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
			EmitNode(IILOLJJLLGH, false, true, true);
		}
		else
		{
			WriteIndicator("?", true, false, false);
			states.Push(EmitterStateKind.FlowMappingValue);
			EmitNode(IILOLJJLLGH, false, true, false);
		}
	}

	private void EmitFlowMappingValue(ParsingEvent IILOLJJLLGH, bool FBFEFFJCLBE)
	{
		if (FBFEFFJCLBE)
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
		EmitNode(IILOLJJLLGH, false, true, false);
	}

	private void EmitBlockSequenceItem(ParsingEvent IILOLJJLLGH, bool IKNHLPGLLKB)
	{
		if (IKNHLPGLLKB)
		{
			IncreaseIndent(false, isMappingContext && !isIndentation);
		}
		if (IILOLJJLLGH is SequenceEnd)
		{
			indent = indents.Pop();
			state = states.Pop();
			return;
		}
		WriteIndent();
		WriteIndicator("  -", true, false, true);
		states.Push(EmitterStateKind.BlockSequenceItem);
		EmitNode(IILOLJJLLGH, false, false, false);
	}

	private void EmitBlockMappingKey(ParsingEvent IILOLJJLLGH, bool IKNHLPGLLKB)
	{
		if (IKNHLPGLLKB)
		{
			IncreaseIndent(false, false);
		}
		if (IILOLJJLLGH is MappingEnd)
		{
			indent = indents.Pop();
			state = states.Pop();
			return;
		}
		WriteIndent();
		if (CheckSimpleKey())
		{
			states.Push(EmitterStateKind.BlockMappingSimpleValue);
			EmitNode(IILOLJJLLGH, false, true, true);
		}
		else
		{
			WriteIndicator("?", true, false, true);
			states.Push(EmitterStateKind.BlockMappingValue);
			EmitNode(IILOLJJLLGH, false, true, false);
		}
	}

	private void EmitBlockMappingValue(ParsingEvent IILOLJJLLGH, bool FBFEFFJCLBE)
	{
		if (FBFEFFJCLBE)
		{
			WriteIndicator(":", false, false, false);
		}
		else
		{
			WriteIndent();
			WriteIndicator(":", true, false, true);
		}
		states.Push(EmitterStateKind.BlockMappingKey);
		EmitNode(IILOLJJLLGH, false, true, false);
	}

	private void IncreaseIndent(bool LMEEIAPIDIJ, bool BHHEHBPGKIO)
	{
		indents.Push(indent);
		if (indent < 0)
		{
			indent = (LMEEIAPIDIJ ? bestIndent : 0);
		}
		else if (!BHHEHBPGKIO)
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
				Scalar lEACOCDHICF = item as Scalar;
				if (lEACOCDHICF != null)
				{
					return string.IsNullOrEmpty(lEACOCDHICF.GetValue());
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
		global::FakeList<ParsingEvent> aGIJCJFMLNN = new global::FakeList<ParsingEvent>(events);
		return aGIJCJFMLNN.get_Item(0) is SequenceStart && aGIJCJFMLNN.get_Item(1) is SequenceEnd;
	}

	private bool CheckEmptyMapping()
	{
		if (events.Count < 2)
		{
			return false;
		}
		global::FakeList<ParsingEvent> aGIJCJFMLNN = new global::FakeList<ParsingEvent>(events);
		return aGIJCJFMLNN.get_Item(0) is MappingStart && aGIJCJFMLNN.get_Item(1) is MappingEnd;
	}

	private void WriteBlockScalarHints(string value)
	{
		CharacterAnalyzer<StringLookAheadBuffer> characterAnalyzer = new CharacterAnalyzer<StringLookAheadBuffer>(new StringLookAheadBuffer(value));
		if (characterAnalyzer.IsSpace() || characterAnalyzer.IsBreak())
		{
			string gPKBINAOGDC = string.Format(CultureInfo.InvariantCulture, "{0}\0", bestIndent);
			WriteIndicator(gPKBINAOGDC, false, false, false);
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

	private void WriteIndicator(string GPKBINAOGDC, bool EMBMHCGJHDL, bool KCCMOOJPCBM, bool FCOACAMEHOE)
	{
		if (EMBMHCGJHDL && !isWhitespace)
		{
			Write(' ');
		}
		Write(GPKBINAOGDC);
		isWhitespace = KCCMOOJPCBM;
		isIndentation &= FCOACAMEHOE;
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

	private void WriteTagContent(string value, bool BFPMMILLOHL)
	{
		if (BFPMMILLOHL && !isWhitespace)
		{
			Write(' ');
		}
		Write(UrlEncode(value));
		isWhitespace = false;
		isIndentation = false;
	}

	private string UrlEncode(string HCPNFPMHFCM)
	{
		return uriReplacer.Replace(HCPNFPMHFCM, (System.Text.RegularExpressions.Match MLPEJKLNAKF) =>
		{
			StringBuilder stringBuilder = new StringBuilder();
			byte[] bytes = Encoding.UTF8.GetBytes(MLPEJKLNAKF.Value);
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
