using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core
{
	[Serializable]
	public class Scanner : IScanner
	{
		private const int MaxVersionNumberLength = 9;

		private const int MaxBufferLength = 8;

		private static readonly IDictionary<char, char> simpleEscapeCodes = new SortedDictionary<char, char>
		{
			{ '0', '\0' },
			{ 'a', '\a' },
			{ 'b', '\b' },
			{ 't', '\t' },
			{ '\t', '\t' },
			{ 'n', '\n' },
			{ 'v', '\v' },
			{ 'f', '\f' },
			{ 'r', '\r' },
			{ 'e', '\u001b' },
			{ ' ', ' ' },
			{ '"', '"' },
			{ '\'', '\'' },
			{ '\\', '\\' },
			{ 'N', '\u0085' },
			{ '_', '\u00a0' },
			{ 'L', '\u2028' },
			{ 'P', '\u2029' }
		};

		private readonly Stack<int> indents = new Stack<int>();

		private readonly InsertionQueue<Token> tokens = new InsertionQueue<Token>();

		private readonly Stack<SimpleKey> simpleKeys = new Stack<SimpleKey>();

		private readonly CharacterAnalyzer<LookAheadBuffer> analyzer;

		private Cursor cursor;

		private bool streamStartProduced;

		private bool streamEndProduced;

		private int indent = -1;

		private bool simpleKeyAllowed;

		private int flowLevel;

		private int tokensParsed;

		private bool tokenAvailable;

		private Token previous;

		public bool SkipComments { get; private set; }

		public Token Current { get; private set; }

		public Mark CurrentPosition
		{
			get
			{
				return cursor.Mark();
			}
		}

		public Scanner(TextReader input, bool skipComments = true)
		{
			analyzer = new CharacterAnalyzer<LookAheadBuffer>(new LookAheadBuffer(input, 8));
			cursor = new Cursor();
			SkipComments = skipComments;
		}

		public bool MoveNext()
		{
			if (Current != null)
			{
				ConsumeCurrentToken();
			}
			return FetchNextToken();
		}

		internal bool FetchNextToken()
		{
			if (!tokenAvailable && !streamEndProduced)
			{
				EnsureTokenAvailable();
			}
			if (tokens.Count > 0)
			{
				Current = tokens.Dequeue();
				tokenAvailable = false;
				return true;
			}
			Current = null;
			return false;
		}

		internal void ConsumeCurrentToken()
		{
			tokensParsed++;
			tokenAvailable = false;
			previous = Current;
			Current = null;
		}

		private char ReadChar()
		{
			char result = analyzer.Peek(0);
			Skip();
			return result;
		}

		private char ReadLineBreakNormalized()
		{
			if (analyzer.Check("\r\n\u0085"))
			{
				SkipLineBreak();
				return '\n';
			}
			char result = analyzer.Peek(0);
			SkipLineBreak();
			return result;
		}

		private void EnsureTokenAvailable()
		{
			while (true)
			{
				bool flag = false;
				if (tokens.Count == 0)
				{
					flag = true;
				}
				else
				{
					StaleSimpleKeys();
					foreach (SimpleKey simpleKey in simpleKeys)
					{
						if (simpleKey.IsPossible && simpleKey.TokenNumber == tokensParsed)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					break;
				}
				FetchMoreTokens();
			}
			tokenAvailable = true;
		}

		private static bool StartsWithChar(StringBuilder builder, char character)
		{
			return builder.Length > 0 && builder[0] == character;
		}

		private void StaleSimpleKeys()
		{
			foreach (SimpleKey simpleKey in simpleKeys)
			{
				if (simpleKey.IsPossible && (simpleKey.Line < cursor.Line || simpleKey.Index + 1024 < cursor.Index))
				{
					if (simpleKey.IsRequired)
					{
						Mark mark = cursor.Mark();
						throw new SyntaxErrorException(mark, mark, "While scanning a simple key, could not find expected ':'.");
					}
					simpleKey.IsPossible = false;
				}
			}
		}

		private void FetchMoreTokens()
		{
			if (!streamStartProduced)
			{
				FetchStreamStart();
				return;
			}
			ScanToNextToken();
			StaleSimpleKeys();
			UnrollIndent(cursor.LineOffset);
			analyzer.Buffer.Cache(4);
			if (analyzer.Buffer.EndOfInput)
			{
				FetchStreamEnd();
				return;
			}
			if (cursor.LineOffset == 0 && analyzer.Check('%'))
			{
				FetchDirective();
				return;
			}
			if (cursor.LineOffset == 0 && analyzer.Check('-') && analyzer.Check('-', 1) && analyzer.Check('-', 2) && analyzer.IsWhiteBreakOrZero(3))
			{
				FetchDocumentIndicator(true);
				return;
			}
			if (cursor.LineOffset == 0 && analyzer.Check('.') && analyzer.Check('.', 1) && analyzer.Check('.', 2) && analyzer.IsWhiteBreakOrZero(3))
			{
				FetchDocumentIndicator(false);
				return;
			}
			if (analyzer.Check('['))
			{
				FetchFlowCollectionStart(true);
				return;
			}
			if (analyzer.Check('{'))
			{
				FetchFlowCollectionStart(false);
				return;
			}
			if (analyzer.Check(']'))
			{
				FetchFlowCollectionEnd(true);
				return;
			}
			if (analyzer.Check('}'))
			{
				FetchFlowCollectionEnd(false);
				return;
			}
			if (analyzer.Check(','))
			{
				FetchFlowEntry();
				return;
			}
			if (analyzer.Check('-') && analyzer.IsWhiteBreakOrZero(1))
			{
				FetchBlockEntry();
				return;
			}
			if (analyzer.Check('?') && (flowLevel > 0 || analyzer.IsWhiteBreakOrZero(1)))
			{
				FetchKey();
				return;
			}
			if (analyzer.Check(':') && (flowLevel > 0 || analyzer.IsWhiteBreakOrZero(1)))
			{
				FetchValue();
				return;
			}
			if (analyzer.Check('*'))
			{
				FetchAnchor(true);
				return;
			}
			if (analyzer.Check('&'))
			{
				FetchAnchor(false);
				return;
			}
			if (analyzer.Check('!'))
			{
				FetchTag();
				return;
			}
			if (analyzer.Check('|') && flowLevel == 0)
			{
				FetchBlockScalar(true);
				return;
			}
			if (analyzer.Check('>') && flowLevel == 0)
			{
				FetchBlockScalar(false);
				return;
			}
			if (analyzer.Check('\''))
			{
				FetchFlowScalar(true);
				return;
			}
			if (analyzer.Check('"'))
			{
				FetchFlowScalar(false);
				return;
			}
			if ((!analyzer.IsWhiteBreakOrZero() && !analyzer.Check("-?:,[]{}#&*!|>'\"%@`")) || (analyzer.Check('-') && !analyzer.IsWhite(1)) || (flowLevel == 0 && analyzer.Check("?:") && !analyzer.IsWhiteBreakOrZero(1)))
			{
				FetchPlainScalar();
				return;
			}
			Mark startMark = cursor.Mark();
			Skip();
			Mark endMark = cursor.Mark();
			throw new SyntaxErrorException(startMark, endMark, "While scanning for the next token, find character that cannot start any token.");
		}

		private bool IsWhitespaceToSkip()
		{
			return analyzer.Check(' ') || ((flowLevel > 0 || !simpleKeyAllowed) && analyzer.Check('\t'));
		}

		private bool IsDocumentIndicator()
		{
			if (cursor.LineOffset == 0 && analyzer.IsWhiteBreakOrZero(3))
			{
				bool flag = analyzer.Check('-') && analyzer.Check('-', 1) && analyzer.Check('-', 2);
				bool flag2 = analyzer.Check('.') && analyzer.Check('.', 1) && analyzer.Check('.', 2);
				return flag || flag2;
			}
			return false;
		}

		private void Skip()
		{
			cursor.Skip();
			analyzer.Buffer.Skip(1);
		}

		private void SkipLineBreak()
		{
			if (analyzer.IsCrLf())
			{
				cursor.SkipLineByOffset(2);
				analyzer.Buffer.Skip(2);
			}
			else if (analyzer.IsBreak())
			{
				cursor.SkipLineByOffset(1);
				analyzer.Buffer.Skip(1);
			}
			else if (!analyzer.IsZero())
			{
				throw new InvalidOperationException("Not at a break.");
			}
		}

		private void ScanToNextToken()
		{
			while (true)
			{
				if (IsWhitespaceToSkip())
				{
					Skip();
					continue;
				}
				SkipComment();
				if (analyzer.IsBreak())
				{
					SkipLineBreak();
					if (flowLevel == 0)
					{
						simpleKeyAllowed = true;
					}
					continue;
				}
				break;
			}
		}

		private void SkipComment()
		{
			if (analyzer.Check('#'))
			{
				Mark mark = cursor.Mark();
				Skip();
				while (analyzer.IsSpace())
				{
					Skip();
				}
				StringBuilder stringBuilder = new StringBuilder();
				while (!analyzer.IsBreakOrZero())
				{
					stringBuilder.Append(ReadChar());
				}
				if (!SkipComments)
				{
					bool isInline = previous != null && previous.End.Line == mark.Line && !(previous is StreamStart);
					tokens.Enqueue(new Tokens.Comment(stringBuilder.ToString(), isInline, mark, cursor.Mark()));
				}
			}
		}

		private void FetchStreamStart()
		{
			simpleKeys.Push(new SimpleKey());
			simpleKeyAllowed = true;
			streamStartProduced = true;
			Mark mark = cursor.Mark();
			tokens.Enqueue(new Tokens.StreamStart(mark, mark));
		}

		private void UnrollIndent(int column)
		{
			if (flowLevel == 0)
			{
				while (indent > column)
				{
					Mark mark = cursor.Mark();
					tokens.Enqueue(new BlockEnd(mark, mark));
					indent = indents.Pop();
				}
			}
		}

		private void FetchStreamEnd()
		{
			cursor.ForceSkipLineAfterNonBreak();
			UnrollIndent(-1);
			RemoveSimpleKey();
			simpleKeyAllowed = false;
			streamEndProduced = true;
			Mark mark = cursor.Mark();
			tokens.Enqueue(new StreamEnd(mark, mark));
		}

		private void FetchDirective()
		{
			UnrollIndent(-1);
			RemoveSimpleKey();
			simpleKeyAllowed = false;
			Token token = ScanDirective();
			tokens.Enqueue(token);
		}

		private Token ScanDirective()
		{
			Mark startMark = cursor.Mark();
			Skip();
			Token result;
			switch (ScanDirectiveName(startMark))
			{
			case "YAML":
				result = ScanVersionDirective(startMark);
				break;
			case "TAG":
				result = ScanTagDirective(startMark);
				break;
			default:
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a directive, find uknown directive name.");
			}
			while (analyzer.IsWhite())
			{
				Skip();
			}
			SkipComment();
			if (!analyzer.IsBreakOrZero())
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a directive, did not find expected comment or line break.");
			}
			if (analyzer.IsBreak())
			{
				SkipLineBreak();
			}
			return result;
		}

		private void FetchDocumentIndicator(bool isStart)
		{
			UnrollIndent(-1);
			RemoveSimpleKey();
			simpleKeyAllowed = false;
			Mark mark = cursor.Mark();
			Skip();
			Skip();
			Skip();
			Token token = ((!isStart) ? ((Token)new Tokens.DocumentEnd(mark, mark)) : ((Token)new Tokens.DocumentStart(mark, cursor.Mark())));
			tokens.Enqueue(token);
		}

		private void FetchFlowCollectionStart(bool isSequence)
		{
			SaveSimpleKey();
			IncreaseFlowLevel();
			simpleKeyAllowed = true;
			Mark mark = cursor.Mark();
			Skip();
			Token token = ((!isSequence) ? ((Token)new FlowMappingStart(mark, mark)) : ((Token)new FlowSequenceStart(mark, mark)));
			tokens.Enqueue(token);
		}

		private void IncreaseFlowLevel()
		{
			simpleKeys.Push(new SimpleKey());
			flowLevel++;
		}

		private void FetchFlowCollectionEnd(bool isSequence)
		{
			RemoveSimpleKey();
			DecreaseFlowLevel();
			simpleKeyAllowed = false;
			Mark mark = cursor.Mark();
			Skip();
			Token token = ((!isSequence) ? ((Token)new FlowMappingEnd(mark, mark)) : ((Token)new FlowSequenceEnd(mark, mark)));
			tokens.Enqueue(token);
		}

		private void DecreaseFlowLevel()
		{
			if (flowLevel > 0)
			{
				flowLevel--;
				simpleKeys.Pop();
			}
		}

		private void FetchFlowEntry()
		{
			RemoveSimpleKey();
			simpleKeyAllowed = true;
			Mark startMark = cursor.Mark();
			Skip();
			tokens.Enqueue(new FlowEntry(startMark, cursor.Mark()));
		}

		private void FetchBlockEntry()
		{
			if (flowLevel == 0)
			{
				if (!simpleKeyAllowed)
				{
					Mark mark = cursor.Mark();
					throw new SyntaxErrorException(mark, mark, "Block sequence entries are not allowed in this context.");
				}
				RollIndent(cursor.LineOffset, -1, true, cursor.Mark());
			}
			RemoveSimpleKey();
			simpleKeyAllowed = true;
			Mark startMark = cursor.Mark();
			Skip();
			tokens.Enqueue(new BlockEntry(startMark, cursor.Mark()));
		}

		private void FetchKey()
		{
			if (flowLevel == 0)
			{
				if (!simpleKeyAllowed)
				{
					Mark mark = cursor.Mark();
					throw new SyntaxErrorException(mark, mark, "Mapping keys are not allowed in this context.");
				}
				RollIndent(cursor.LineOffset, -1, false, cursor.Mark());
			}
			RemoveSimpleKey();
			simpleKeyAllowed = flowLevel == 0;
			Mark startMark = cursor.Mark();
			Skip();
			tokens.Enqueue(new Key(startMark, cursor.Mark()));
		}

		private void FetchValue()
		{
			SimpleKey simpleKey = simpleKeys.Peek();
			if (simpleKey.IsPossible)
			{
				tokens.Insert(simpleKey.TokenNumber - tokensParsed, new Key(simpleKey.Mark, simpleKey.Mark));
				RollIndent(simpleKey.LineOffset, simpleKey.TokenNumber, false, simpleKey.Mark);
				simpleKey.IsPossible = false;
				simpleKeyAllowed = false;
			}
			else
			{
				if (flowLevel == 0)
				{
					if (!simpleKeyAllowed)
					{
						Mark mark = cursor.Mark();
						throw new SyntaxErrorException(mark, mark, "Mapping values are not allowed in this context.");
					}
					RollIndent(cursor.LineOffset, -1, false, cursor.Mark());
				}
				simpleKeyAllowed = flowLevel == 0;
			}
			Mark startMark = cursor.Mark();
			Skip();
			tokens.Enqueue(new Value(startMark, cursor.Mark()));
		}

		private void RollIndent(int column, int number, bool isSequence, Mark mark)
		{
			if (flowLevel <= 0 && indent < column)
			{
				indents.Push(indent);
				indent = column;
				Token token = ((!isSequence) ? ((Token)new BlockMappingStart(mark, mark)) : ((Token)new BlockSequenceStart(mark, mark)));
				if (number == -1)
				{
					tokens.Enqueue(token);
				}
				else
				{
					tokens.Insert(number - tokensParsed, token);
				}
			}
		}

		private void FetchAnchor(bool isAlias)
		{
			SaveSimpleKey();
			simpleKeyAllowed = false;
			tokens.Enqueue(ScanAnchor(isAlias));
		}

		private Token ScanAnchor(bool isAlias)
		{
			Mark startMark = cursor.Mark();
			Skip();
			StringBuilder stringBuilder = new StringBuilder();
			while (analyzer.IsAlphaNumericDashOrUnderscore())
			{
				stringBuilder.Append(ReadChar());
			}
			if (stringBuilder.Length == 0 || (!analyzer.IsWhiteBreakOrZero() && !analyzer.Check("?:,]}%@`")))
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning an anchor or alias, did not find expected alphabetic or numeric character.");
			}
			if (isAlias)
			{
				return new Tokens.AnchorAlias(stringBuilder.ToString(), startMark, cursor.Mark());
			}
			return new Anchor(stringBuilder.ToString(), startMark, cursor.Mark());
		}

		private void FetchTag()
		{
			SaveSimpleKey();
			simpleKeyAllowed = false;
			tokens.Enqueue(ScanTag());
		}

		private Token ScanTag()
		{
			Mark startMark = cursor.Mark();
			string text;
			string text2;
			if (analyzer.Check('<', 1))
			{
				text = string.Empty;
				Skip();
				Skip();
				text2 = ScanTagUri(null, startMark);
				if (!analyzer.Check('>'))
				{
					throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a tag, did not find the expected '>'.");
				}
				Skip();
			}
			else
			{
				string text3 = ScanTagHandle(false, startMark);
				if (text3.Length > 1 && text3[0] == '!' && text3[text3.Length - 1] == '!')
				{
					text = text3;
					text2 = ScanTagUri(null, startMark);
				}
				else
				{
					text2 = ScanTagUri(text3, startMark);
					text = "!";
					if (text2.Length == 0)
					{
						text2 = text;
						text = string.Empty;
					}
				}
			}
			if (!analyzer.IsWhiteBreakOrZero())
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a tag, did not find expected whitespace or line break.");
			}
			return new Tag(text, text2, startMark, cursor.Mark());
		}

		private void FetchBlockScalar(bool isLiteral)
		{
			RemoveSimpleKey();
			simpleKeyAllowed = true;
			tokens.Enqueue(ScanBlockScalar(isLiteral));
		}

		private Token ScanBlockScalar(bool isLiteral)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			int num = 0;
			int num2 = 0;
			int blockIndent = 0;
			bool flag = false;
			Mark startMark = cursor.Mark();
			Skip();
			if (analyzer.Check("+-"))
			{
				num = (analyzer.Check('+') ? 1 : (-1));
				Skip();
				if (analyzer.IsDigit())
				{
					if (analyzer.Check('0'))
					{
						throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a block scalar, find an intendation indicator equal to 0.");
					}
					num2 = analyzer.AsDigit();
					Skip();
				}
			}
			else if (analyzer.IsDigit())
			{
				if (analyzer.Check('0'))
				{
					throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a block scalar, find an intendation indicator equal to 0.");
				}
				num2 = analyzer.AsDigit();
				Skip();
				if (analyzer.Check("+-"))
				{
					num = (analyzer.Check('+') ? 1 : (-1));
					Skip();
				}
			}
			while (analyzer.IsWhite())
			{
				Skip();
			}
			SkipComment();
			if (!analyzer.IsBreakOrZero())
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a block scalar, did not find expected comment or line break.");
			}
			if (analyzer.IsBreak())
			{
				SkipLineBreak();
			}
			Mark endMark = cursor.Mark();
			if (num2 != 0)
			{
				blockIndent = ((indent < 0) ? num2 : (indent + num2));
			}
			blockIndent = ScanBlockScalarBreaks(blockIndent, stringBuilder3, startMark, ref endMark);
			while (cursor.LineOffset == blockIndent && !analyzer.IsZero())
			{
				bool flag2 = analyzer.IsWhite();
				if (!isLiteral && StartsWithChar(stringBuilder2, '\n') && !flag && !flag2)
				{
					if (stringBuilder3.Length == 0)
					{
						stringBuilder.Append(' ');
					}
					stringBuilder2.Length = 0;
				}
				else
				{
					stringBuilder.Append(stringBuilder2.ToString());
					stringBuilder2.Length = 0;
				}
				stringBuilder.Append(stringBuilder3.ToString());
				stringBuilder3.Length = 0;
				flag = analyzer.IsWhite();
				while (!analyzer.IsBreakOrZero())
				{
					stringBuilder.Append(ReadChar());
				}
				stringBuilder2.Append(ReadLineBreakNormalized());
				blockIndent = ScanBlockScalarBreaks(blockIndent, stringBuilder3, startMark, ref endMark);
			}
			if (num != -1)
			{
				stringBuilder.Append(stringBuilder2);
			}
			if (num == 1)
			{
				stringBuilder.Append(stringBuilder3);
			}
			ScalarStyle style = ((!isLiteral) ? ScalarStyle.Folded : ScalarStyle.Literal);
			return new Tokens.Scalar(stringBuilder.ToString(), style, startMark, endMark);
		}

		private int ScanBlockScalarBreaks(int blockIndent, StringBuilder breaks, Mark startMark, ref Mark endMark)
		{
			int num = 0;
			endMark = cursor.Mark();
			while (true)
			{
				if ((blockIndent == 0 || cursor.LineOffset < blockIndent) && analyzer.IsSpace())
				{
					Skip();
					continue;
				}
				if (cursor.LineOffset > num)
				{
					num = cursor.LineOffset;
				}
				if ((blockIndent == 0 || cursor.LineOffset < blockIndent) && analyzer.IsTab())
				{
					throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a block scalar, find a tab character where an intendation space is expected.");
				}
				if (!analyzer.IsBreak())
				{
					break;
				}
				breaks.Append(ReadLineBreakNormalized());
				endMark = cursor.Mark();
			}
			if (blockIndent == 0)
			{
				blockIndent = Math.Max(num, Math.Max(indent + 1, 1));
			}
			return blockIndent;
		}

		private void FetchFlowScalar(bool isSingleQuoted)
		{
			SaveSimpleKey();
			simpleKeyAllowed = false;
			tokens.Enqueue(ScanFlowScalar(isSingleQuoted));
		}

		private Token ScanFlowScalar(bool isSingleQuoted)
		{
			Mark startMark = cursor.Mark();
			Skip();
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			StringBuilder stringBuilder4 = new StringBuilder();
			while (true)
			{
				if (IsDocumentIndicator())
				{
					throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a quoted scalar, find unexpected document indicator.");
				}
				if (analyzer.IsZero())
				{
					throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a quoted scalar, find unexpected end of stream.");
				}
				bool flag = false;
				while (!analyzer.IsWhiteBreakOrZero())
				{
					if (isSingleQuoted && analyzer.Check('\'') && analyzer.Check('\'', 1))
					{
						stringBuilder.Append('\'');
						Skip();
						Skip();
						continue;
					}
					if (analyzer.Check((!isSingleQuoted) ? '"' : '\''))
					{
						break;
					}
					if (!isSingleQuoted && analyzer.Check('\\') && analyzer.IsBreak(1))
					{
						Skip();
						SkipLineBreak();
						flag = true;
						break;
					}
					if (!isSingleQuoted && analyzer.Check('\\'))
					{
						int num = 0;
						char c = analyzer.Peek(1);
						switch (c)
						{
						case 'x':
							num = 2;
							break;
						case 'u':
							num = 4;
							break;
						case 'U':
							num = 8;
							break;
						default:
						{
							char value;
							if (simpleEscapeCodes.TryGetValue(c, out value))
							{
								stringBuilder.Append(value);
								break;
							}
							throw new SyntaxErrorException(startMark, cursor.Mark(), "While parsing a quoted scalar, find unknown escape character.");
						}
						}
						Skip();
						Skip();
						if (num <= 0)
						{
							continue;
						}
						uint num2 = 0u;
						for (int i = 0; i < num; i++)
						{
							if (!analyzer.IsHex(i))
							{
								throw new SyntaxErrorException(startMark, cursor.Mark(), "While parsing a quoted scalar, did not find expected hexdecimal number.");
							}
							num2 = (uint)((num2 << 4) + analyzer.AsHex(i));
						}
						if ((num2 >= 55296 && num2 <= 57343) || num2 > 1114111)
						{
							throw new SyntaxErrorException(startMark, cursor.Mark(), "While parsing a quoted scalar, find invalid Unicode character escape code.");
						}
						stringBuilder.Append((char)num2);
						for (int j = 0; j < num; j++)
						{
							Skip();
						}
					}
					else
					{
						stringBuilder.Append(ReadChar());
					}
				}
				if (analyzer.Check((!isSingleQuoted) ? '"' : '\''))
				{
					break;
				}
				while (analyzer.IsWhite() || analyzer.IsBreak())
				{
					if (analyzer.IsWhite())
					{
						if (!flag)
						{
							stringBuilder2.Append(ReadChar());
						}
						else
						{
							Skip();
						}
					}
					else if (!flag)
					{
						stringBuilder2.Length = 0;
						stringBuilder3.Append(ReadLineBreakNormalized());
						flag = true;
					}
					else
					{
						stringBuilder4.Append(ReadLineBreakNormalized());
					}
				}
				if (flag)
				{
					if (StartsWithChar(stringBuilder3, '\n'))
					{
						if (stringBuilder4.Length == 0)
						{
							stringBuilder.Append(' ');
						}
						else
						{
							stringBuilder.Append(stringBuilder4.ToString());
						}
					}
					else
					{
						stringBuilder.Append(stringBuilder3.ToString());
						stringBuilder.Append(stringBuilder4.ToString());
					}
					stringBuilder3.Length = 0;
					stringBuilder4.Length = 0;
				}
				else
				{
					stringBuilder.Append(stringBuilder2.ToString());
					stringBuilder2.Length = 0;
				}
			}
			Skip();
			return new Tokens.Scalar(stringBuilder.ToString(), (!isSingleQuoted) ? ScalarStyle.DoubleQuoted : ScalarStyle.SingleQuoted);
		}

		private void FetchPlainScalar()
		{
			SaveSimpleKey();
			simpleKeyAllowed = false;
			tokens.Enqueue(ScanPlainScalar());
		}

		private Token ScanPlainScalar()
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			StringBuilder stringBuilder4 = new StringBuilder();
			bool flag = false;
			int num = indent + 1;
			Mark mark = cursor.Mark();
			Mark endMark = mark;
			while (!IsDocumentIndicator() && !analyzer.Check('#'))
			{
				while (!analyzer.IsWhiteBreakOrZero())
				{
					if (flowLevel > 0 && analyzer.Check(':') && !analyzer.IsWhiteBreakOrZero(1))
					{
						throw new SyntaxErrorException(mark, cursor.Mark(), "While scanning a plain scalar, find unexpected ':'.");
					}
					if ((analyzer.Check(':') && analyzer.IsWhiteBreakOrZero(1)) || (flowLevel > 0 && analyzer.Check(",:?[]{}")))
					{
						break;
					}
					if (flag || stringBuilder2.Length > 0)
					{
						if (flag)
						{
							if (StartsWithChar(stringBuilder3, '\n'))
							{
								if (stringBuilder4.Length == 0)
								{
									stringBuilder.Append(' ');
								}
								else
								{
									stringBuilder.Append(stringBuilder4);
								}
							}
							else
							{
								stringBuilder.Append(stringBuilder3);
								stringBuilder.Append(stringBuilder4);
							}
							stringBuilder3.Length = 0;
							stringBuilder4.Length = 0;
							flag = false;
						}
						else
						{
							stringBuilder.Append(stringBuilder2);
							stringBuilder2.Length = 0;
						}
					}
					stringBuilder.Append(ReadChar());
					endMark = cursor.Mark();
				}
				if (!analyzer.IsWhite() && !analyzer.IsBreak())
				{
					break;
				}
				while (analyzer.IsWhite() || analyzer.IsBreak())
				{
					if (analyzer.IsWhite())
					{
						if (flag && cursor.LineOffset < num && analyzer.IsTab())
						{
							throw new SyntaxErrorException(mark, cursor.Mark(), "While scanning a plain scalar, find a tab character that violate intendation.");
						}
						if (!flag)
						{
							stringBuilder2.Append(ReadChar());
						}
						else
						{
							Skip();
						}
					}
					else if (!flag)
					{
						stringBuilder2.Length = 0;
						stringBuilder3.Append(ReadLineBreakNormalized());
						flag = true;
					}
					else
					{
						stringBuilder4.Append(ReadLineBreakNormalized());
					}
				}
				if (flowLevel == 0 && cursor.LineOffset < num)
				{
					break;
				}
			}
			if (flag)
			{
				simpleKeyAllowed = true;
			}
			return new Tokens.Scalar(stringBuilder.ToString(), ScalarStyle.Plain, mark, endMark);
		}

		private void RemoveSimpleKey()
		{
			SimpleKey simpleKey = simpleKeys.Peek();
			if (simpleKey.IsPossible && simpleKey.IsRequired)
			{
				throw new SyntaxErrorException(simpleKey.Mark, simpleKey.Mark, "While scanning a simple key, could not find expected ':'.");
			}
			simpleKey.IsPossible = false;
		}

		private string ScanDirectiveName(Mark startMark)
		{
			StringBuilder stringBuilder = new StringBuilder();
			while (analyzer.IsAlphaNumericDashOrUnderscore())
			{
				stringBuilder.Append(ReadChar());
			}
			if (stringBuilder.Length == 0)
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a directive, could not find expected directive name.");
			}
			if (!analyzer.IsWhiteBreakOrZero())
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a directive, find unexpected non-alphabetical character.");
			}
			return stringBuilder.ToString();
		}

		private void SkipWhitespace()
		{
			while (analyzer.IsWhite())
			{
				Skip();
			}
		}

		private Token ScanVersionDirective(Mark startMark)
		{
			SkipWhitespace();
			int major = ScanVersionDirectiveNumber(startMark);
			if (!analyzer.Check('.'))
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a %YAML directive, did not find expected digit or '.' character.");
			}
			Skip();
			int minor = ScanVersionDirectiveNumber(startMark);
			return new VersionDirective(new Version(major, minor), startMark, startMark);
		}

		private Token ScanTagDirective(Mark startMark)
		{
			SkipWhitespace();
			string handle = ScanTagHandle(true, startMark);
			if (!analyzer.IsWhite())
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a %TAG directive, did not find expected whitespace.");
			}
			SkipWhitespace();
			string prefix = ScanTagUri(null, startMark);
			if (!analyzer.IsWhiteBreakOrZero())
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a %TAG directive, did not find expected whitespace or line break.");
			}
			return new TagDirective(handle, prefix, startMark, startMark);
		}

		private string ScanTagUri(string head, Mark startMark)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (head != null && head.Length > 1)
			{
				stringBuilder.Append(head.Substring(1));
			}
			while (analyzer.IsAlphaNumericDashOrUnderscore() || analyzer.Check(";/?:@&=+$,.!~*'()[]%"))
			{
				if (analyzer.Check('%'))
				{
					stringBuilder.Append(ScanUriEscapes(startMark));
				}
				else
				{
					stringBuilder.Append(ReadChar());
				}
			}
			if (stringBuilder.Length == 0)
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While parsing a tag, did not find expected tag URI.");
			}
			return stringBuilder.ToString();
		}

		private char ScanUriEscapes(Mark startMark)
		{
			List<byte> list = new List<byte>();
			int num = 0;
			do
			{
				if (!analyzer.Check('%') || !analyzer.IsHex(1) || !analyzer.IsHex(2))
				{
					throw new SyntaxErrorException(startMark, cursor.Mark(), "While parsing a tag, did not find URI escaped octet.");
				}
				int num2 = (analyzer.AsHex(1) << 4) + analyzer.AsHex(2);
				if (num == 0)
				{
					num = (((num2 & 0x80) == 0) ? 1 : (((num2 & 0xE0) == 192) ? 2 : (((num2 & 0xF0) == 224) ? 3 : (((num2 & 0xF8) == 240) ? 4 : 0))));
					if (num == 0)
					{
						throw new SyntaxErrorException(startMark, cursor.Mark(), "While parsing a tag, find an incorrect leading UTF-8 octet.");
					}
				}
				else if ((num2 & 0xC0) != 128)
				{
					throw new SyntaxErrorException(startMark, cursor.Mark(), "While parsing a tag, find an incorrect trailing UTF-8 octet.");
				}
				list.Add((byte)num2);
				Skip();
				Skip();
				Skip();
			}
			while (--num > 0);
			char[] chars = Encoding.UTF8.GetChars(list.ToArray());
			if (chars.Length != 1)
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While parsing a tag, find an incorrect UTF-8 sequence.");
			}
			return chars[0];
		}

		private string ScanTagHandle(bool isDirective, Mark startMark)
		{
			if (!analyzer.Check('!'))
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a tag, did not find expected '!'.");
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(ReadChar());
			while (analyzer.IsAlphaNumericDashOrUnderscore())
			{
				stringBuilder.Append(ReadChar());
			}
			if (analyzer.Check('!'))
			{
				stringBuilder.Append(ReadChar());
			}
			else if (isDirective && (stringBuilder.Length != 1 || stringBuilder[0] != '!'))
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While parsing a tag directive, did not find expected '!'.");
			}
			return stringBuilder.ToString();
		}

		private int ScanVersionDirectiveNumber(Mark startMark)
		{
			int num = 0;
			int num2 = 0;
			while (analyzer.IsDigit())
			{
				if (++num2 > 9)
				{
					throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a %YAML directive, find extremely long version number.");
				}
				num = num * 10 + analyzer.AsDigit();
				Skip();
			}
			if (num2 == 0)
			{
				throw new SyntaxErrorException(startMark, cursor.Mark(), "While scanning a %YAML directive, did not find expected version number.");
			}
			return num;
		}

		private void SaveSimpleKey()
		{
			bool isRequired = flowLevel == 0 && indent == cursor.LineOffset;
			if (simpleKeyAllowed)
			{
				SimpleKey t = new SimpleKey(true, isRequired, tokensParsed + tokens.Count, cursor);
				RemoveSimpleKey();
				simpleKeys.Pop();
				simpleKeys.Push(t);
			}
		}
	}
}
