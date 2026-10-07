using System;

namespace YamlDotNet.Core
{
	[Serializable]
	internal class CharacterAnalyzer<TBuffer> where TBuffer : ILookAheadBuffer
	{
		private readonly TBuffer buffer;

		public TBuffer Buffer
		{
			get
			{
				return buffer;
			}
		}

		public bool EndOfInput
		{
			get
			{
				return buffer.EndOfInput;
			}
		}

		public CharacterAnalyzer(TBuffer buffer)
		{
			buffer = buffer;
		}

		public char Peek(int IPCOBJBKNAO)
		{
			return buffer.Peek(IPCOBJBKNAO);
		}

		public void Skip(int BDBOAEGELMC)
		{
			buffer.Skip(BDBOAEGELMC);
		}

		public bool IsAlphaNumericDashOrUnderscore(int IPCOBJBKNAO = 0)
		{
			char c = buffer.Peek(IPCOBJBKNAO);
			return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_' || c == '-';
		}

		public bool IsAscii(int IPCOBJBKNAO = 0)
		{
			return buffer.Peek(IPCOBJBKNAO) <= '\u007f';
		}

		public bool IsPrintable(int IPCOBJBKNAO = 0)
		{
			char c = buffer.Peek(IPCOBJBKNAO);
			return c == '\t' || c == '\n' || c == '\r' || (c >= ' ' && c <= '~') || c == '\u0085' || (c >= '\u00a0' && c <= '\ud7ff') || (c >= '\ue000' && c <= '\ufffd');
		}

		public bool IsDigit(int IPCOBJBKNAO = 0)
		{
			char c = buffer.Peek(IPCOBJBKNAO);
			return c >= '0' && c <= '9';
		}

		public int AsDigit(int IPCOBJBKNAO = 0)
		{
			return buffer.Peek(IPCOBJBKNAO) - 48;
		}

		public bool IsHex(int IPCOBJBKNAO)
		{
			char c = buffer.Peek(IPCOBJBKNAO);
			return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
		}

		public int AsHex(int IPCOBJBKNAO)
		{
			char c = buffer.Peek(IPCOBJBKNAO);
			if (c <= '9')
			{
				return c - 48;
			}
			if (c <= 'F')
			{
				return c - 65 + 10;
			}
			return c - 97 + 10;
		}

		public bool IsSpace(int IPCOBJBKNAO = 0)
		{
			return Check(' ', IPCOBJBKNAO);
		}

		public bool IsZero(int IPCOBJBKNAO = 0)
		{
			return Check('\0', IPCOBJBKNAO);
		}

		public bool IsTab(int IPCOBJBKNAO = 0)
		{
			return Check('\t', IPCOBJBKNAO);
		}

		public bool IsWhite(int IPCOBJBKNAO = 0)
		{
			return IsSpace(IPCOBJBKNAO) || IsTab(IPCOBJBKNAO);
		}

		public bool IsBreak(int IPCOBJBKNAO = 0)
		{
			return Check("\r\n\u0085\u2028\u2029", IPCOBJBKNAO);
		}

		public bool IsCrLf(int IPCOBJBKNAO = 0)
		{
			return Check('\r', IPCOBJBKNAO) && Check('\n', IPCOBJBKNAO + 1);
		}

		public bool IsBreakOrZero(int IPCOBJBKNAO = 0)
		{
			return IsBreak(IPCOBJBKNAO) || IsZero(IPCOBJBKNAO);
		}

		public bool IsWhiteBreakOrZero(int IPCOBJBKNAO = 0)
		{
			return IsWhite(IPCOBJBKNAO) || IsBreakOrZero(IPCOBJBKNAO);
		}

		public bool Check(char EFCPGPEFNJI, int IPCOBJBKNAO = 0)
		{
			return buffer.Peek(IPCOBJBKNAO) == EFCPGPEFNJI;
		}

		public bool Check(string PAJEGDIJODA, int IPCOBJBKNAO = 0)
		{
			char value = buffer.Peek(IPCOBJBKNAO);
			return PAJEGDIJODA.IndexOf(value) != -1;
		}
	}
}
